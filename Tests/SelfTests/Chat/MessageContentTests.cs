namespace OpenTgResearcherDesktop.ChatSelfTests;

internal static partial class Program
{
    // Проверяем текст, подписи и действие обложки на настоящих моделях и обработчиках
    private static IEnumerable<(string Name, Func<Task> Run)> GetMessageContentTests() =>
    [
        ("MessageLiteralPipePreserved", MessageLiteralPipePreservedAsync),
        ("LegacyDocumentHasNoInventedCaption", LegacyDocumentHasNoInventedCaptionAsync),
        ("VideoThumbnailOpensOriginalFile", VideoThumbnailOpensOriginalFileAsync),
        ("DownloadedVideoSavesCaptionAndEntities", DownloadedVideoSavesCaptionAndEntitiesAsync),
        ("ExistingVideoSavesCaptionAndEntities", ExistingVideoSavesCaptionAndEntitiesAsync),
        ("TextWithWebPreviewStillSaved", TextWithWebPreviewStillSavedAsync),
        ("FormattedMessagePreservesNestedUtf16Ranges", FormattedMessagePreservesNestedUtf16RangesAsync),
        ("FormattedTextSavedAndStatusReadable", FormattedTextSavedAndStatusReadableAsync),
        ("MessageFormatCompatibility", MessageFormatCompatibilityAsync),
        ("PhotoSavesFormattedCaptionAndRelativePath", PhotoSavesFormattedCaptionAndRelativePathAsync),
    ];

    // Подпись фотографии хранится отдельно от относительного имени и сохраняет символ разделителя
    private static async Task PhotoSavesFormattedCaptionAndRelativePathAsync()
    {
        using var storage = new TgStorageService(); using var files = new MediaFixture();
        typeof(TgConnectClientBase).GetProperty("StorageManager", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(files.Connection, storage);
        files.Settings.SourceVm.Dto.Id = 100; files.Settings.SourceVm.Dto.Directory = files.Directory;
        files.Settings.SourceVm.Dto.IsSaveMessages = true; files.Settings.SourceVm.Dto.IsCreatingSubdirectories = false;
        await storage.SourceRepository.SaveAsync(files.Settings.SourceVm.Dto);
        var message = new TL.Message { id = 45, date = DateTime.UtcNow, message = "photo | caption",
            flags = TL.Message.Flags.has_media | TL.Message.Flags.has_entities,
            entities = [new TL.MessageEntityItalic { offset = 0, length = 5 }],
            media = new TL.MessageMediaPhoto { photo = new TL.Photo { id = 88, date = DateTime.UtcNow,
                sizes = [new TL.PhotoSize { type = "m", size = 4 }] } } };
        await files.CallAsync("HandleMessageContentAsync", files.Settings, new TgParseHelper(), message, new TgChatCache(),
            new TgForumTopicSettings(), message, new TgMessageSettings { CurrentChatId = 100, CurrentMessageId = 45 }, CancellationToken.None);
        await files.CallAsync("FlushMessageBufferAsync", new TgParseHelper(), true, false, true, CancellationToken.None);
        using var messages = new TgEfMessageRepository(); var dto = await messages.GetDtoAsync(x => x.SourceId == 100 && x.Id == 45);
        dto.Directory = files.Directory;
        Assert(dto.Type == TgEnumMessageType.Photo && dto.MessageText == message.message && dto.TextRuns[0].Italic,
            "Фотография потеряла подпись или разметку");
        Assert(!Path.IsPathRooted(dto.FileName) && dto.IsImageExists && files.Connection.Downloads == 1 && files.NetworkCalls == 0,
            "Фотография потеряла относительный путь или не скачалась");
    }

    // Диапазоны Telegram считаются в UTF-16 и могут накладываться друг на друга
    private static Task FormattedMessagePreservesNestedUtf16RangesAsync()
    {
        var dto = new TgEfMessageDto { Message = "OTR:1:" + System.Text.Json.JsonSerializer.Serialize(new
        {
            Text = "😀 bold link <>&", FileName = "", Entities = new object[]
            {
                new { Type = "Bold", Offset = 3, Length = 4 }, new { Type = "Italic", Offset = 5, Length = 2 },
                new { Type = "TextUrl", Offset = 8, Length = 4, Value = "https://t.me/durov" },
                new { Type = "Bold", Offset = int.MaxValue, Length = 10 },
            }
        }) };
        Assert(dto.MessageText == "😀 bold link <>&", "Форматированный текст не прочитан без потерь");
        var runs = dto.TextRuns;
        Assert(runs.Count == 6, "Не выделены границы вложенных диапазонов");
        Assert(runs[2] is { Text: "ld", Bold: true, Italic: true }, "Потеряны вложенные стили после emoji");
        Assert(runs[4].Link == "https://t.me/durov", "Потеряна ссылка Telegram");
        return Task.CompletedTask;
    }

    // Старые фото читаются без миграции, новый формат не интерпретирует пользовательский HTML и Markdown
    private static Task MessageFormatCompatibilityAsync()
    {
        var photo = new TgEfMessageDto { Type = TgEnumMessageType.Photo, Message = "old | caption | \\photo.jpg" };
        Assert(photo.MessageText == "old | caption" && photo.FileName == "\\photo.jpg", "Потеряна старая подпись фотографии");
        var literal = new TgMessageContent { Text = "OTR:1:<b>**plain**</b>\n  value | end", Entities = [] };
        Assert(TgMessageContent.Read(literal.Serialize(), TgEnumMessageType.Message).Text == literal.Text, "Префикс или разметка изменили исходный текст");
        var content = new TgMessageContent { Text = "code link", Entities = [new("Code", 0, 4), new("Underline", 0, 4),
            new("Strike", 0, 4), new("TextUrl", 5, 4, "javascript:alert(1)"), new("CustomEmoji", 5, 4, "123")] };
        var runs = content.GetRuns();
        Assert(runs[0] is { Monospace: true, Underline: true, Strike: true } && runs[^1].Link == ""
            && string.Concat(runs.Select(x => x.Text)) == content.Text, "Стиль или неизвестная сущность потеряли текст либо открыли неподдерживаемую ссылку");
        var markup = ReadMarkup("TgChatContentPage.xaml");
        Assert(markup.Descendants().Any(x => x.Name.LocalName == "RichTextBlock" && x.Attributes().Any(a => a.Name.LocalName == "TgMessageTextHelper.Content")),
            "Лента не использует нативное форматирование");
        return Task.CompletedTask;
    }

    // Сериализация не должна показывать JSON пользователю в статусе парсинга
    private static async Task FormattedTextSavedAndStatusReadableAsync()
    {
        using var storage = new TgStorageService(); using var files = new MediaFixture();
        typeof(TgConnectClientBase).GetProperty("StorageManager", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(files.Connection, storage);
        files.Settings.SourceVm.Dto.IsSaveMessages = true;
        var status = string.Empty;
        files.Connection.SetupUpdateStateMessageThread((_, _, text, _, _) => { status = text; return Task.CompletedTask; });
        var message = new TL.Message { id = 44, date = DateTime.UtcNow, message = "bold\ntext | value",
            flags = TL.Message.Flags.has_entities, entities = [new TL.MessageEntityBold { offset = 0, length = 4 }] };
        await files.CallAsync("HandleMessageContentAsync", files.Settings, new TgParseHelper(), message, new TgChatCache(),
            new TgForumTopicSettings(), message, new TgMessageSettings { CurrentChatId = 100, CurrentMessageId = 44 }, CancellationToken.None);
        await files.CallAsync("FlushMessageBufferAsync", new TgParseHelper(), true, false, true, CancellationToken.None);
        using var messages = new TgEfMessageRepository(); var dto = await messages.GetDtoAsync(x => x.SourceId == 100 && x.Id == 44);
        Assert(dto.MessageText == message.message && dto.TextRuns[0] is { Text: "bold", Bold: true }, "Обычный текст потерял форматирование в БД");
        Assert(status == message.message, "В статус парсинга попал JSON вместо исходного текста");
    }

    // Разделитель старого формата фотографий не должен обрезать обычный текст
    private static Task MessageLiteralPipePreservedAsync()
    {
        var dto = new TgEfMessageDto { Message = "  A | B\n<C> & **literal**", Type = TgEnumMessageType.Message };
        Assert(dto.MessageText == dto.Message && dto.FileName == "", "Обычный текст обрезан или принят за имя файла");
        return Task.CompletedTask;
    }

    // Старое имя файла не является утраченной подписью к документу
    private static Task LegacyDocumentHasNoInventedCaptionAsync()
    {
        var dto = new TgEfMessageDto { Message = "IMG_6025.MP4", Type = TgEnumMessageType.Document };
        Assert(dto.MessageText == "" && dto.FileName == "IMG_6025.MP4", "Имя старого документа показано как подпись");
        return Task.CompletedTask;
    }

    // Обложка должна открывать реальный файл, включая старое имя без ID в БД
    private static Task VideoThumbnailOpensOriginalFileAsync()
    {
        using var files = new MediaFixture();
        File.WriteAllBytes(Path.Combine(files.Directory, "552 IMG_6025.MP4"), [1, 2, 3]);
        var dto = new TgEfMessageDto { Id = 552, Directory = files.Directory, Type = TgEnumMessageType.Document, Message = "IMG_6025.MP4" };
        var path = dto.MediaFullPath;
        Assert(path == Path.Combine(files.Directory, "552 IMG_6025.MP4"), "Не найден оригинал видео для воспроизведения");
        var thumbnail = ReadMarkup("TgChatContentPage.xaml").Descendants().Single(x =>
            (string?)x.Attribute("AutomationProperties.AutomationId") == "MessageThumbnail");
        Assert((string?)thumbnail.Attribute("Tapped") == "OnMediaPreviewClick"
            && ((string?)thumbnail.Attribute("Tag"))?.Contains("MediaFullPath") == true, "Обложка открывает картинку вместо видео");
        return Task.CompletedTask;
    }

    // Первый путь записи после скачивания не должен потерять подпись и UTF-16 диапазоны Telegram
    private static async Task DownloadedVideoSavesCaptionAndEntitiesAsync() => await SaveVideoCaptionAsync(existing: false);

    // Наличие полного файла не отменяет обновление подписи в БД и не требует скачивать его заново
    private static async Task ExistingVideoSavesCaptionAndEntitiesAsync() => await SaveVideoCaptionAsync(existing: true);

    // Проверяем сохранение в SQLite и повторное чтение DTO без подмены обработчиков контента
    private static async Task SaveVideoCaptionAsync(bool existing)
    {
        using var storage = new TgStorageService();
        using var files = new MediaFixture();
        typeof(TgConnectClientBase).GetProperty("StorageManager", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(files.Connection, storage);
        files.Settings.SourceVm.Dto.Id = 100;
        files.Settings.SourceVm.Dto.Directory = files.Directory;
        files.Settings.SourceVm.Dto.IsSaveMessages = true;
        files.Settings.SourceVm.Dto.IsSaveFiles = true;
        files.Settings.SourceVm.Dto.IsCreatingSubdirectories = false;
        files.Settings.SourceVm.Dto.IsFileNamingByMessage = false;
        files.Settings.SourceVm.Dto.IsDownloadThumbnail = false;
        files.Connection.DicChats[100] = new TL.Channel { id = 100, access_hash = 123 };
        await storage.SourceRepository.SaveAsync(files.Settings.SourceVm.Dto);
        if (existing) File.WriteAllBytes(Path.Combine(files.Directory, "042 clip.mp4"), [1, 2, 3, 4]);
        var message = new TL.Message { id = 42, peer_id = new TL.PeerChannel { channel_id = 100 }, date = DateTime.UtcNow,
            flags = TL.Message.Flags.has_media | TL.Message.Flags.has_entities, message = "😀 bold | caption",
            entities = [new TL.MessageEntityBold { offset = 3, length = 4 }],
            media = new TL.MessageMediaDocument { flags = TL.MessageMediaDocument.Flags.has_document,
                document = new TL.Document { id = 99, size = 4, date = DateTime.UtcNow,
                mime_type = "video/mp4", attributes = [new TL.DocumentAttributeFilename { file_name = "clip.mp4" }] } } };
        await files.CallAsync("HandleMessageContentAsync", files.Settings, new TgParseHelper(), message, new TgChatCache(),
            new TgForumTopicSettings(), message, new TgMessageSettings { CurrentChatId = 100, CurrentMessageId = 42 }, CancellationToken.None);
        await files.CallAsync("FlushMessageBufferAsync", new TgParseHelper(), true, false, true, CancellationToken.None);
        using var messages = new TgEfMessageRepository();
        var saved = await messages.GetDtoAsync(x => x.SourceId == 100 && x.Id == 42);
        Assert(saved.MessageText == message.message, "При сохранении документа потеряна подпись Telegram: " + saved.Message);
        Assert(saved.Message.Contains("Bold") && saved.Message.Contains("Offset"), "Потеряны диапазоны форматирования Telegram");
        saved.Directory = files.Directory;
        var path = saved.MediaFullPath;
        Assert(File.Exists(path), "Подпись потеряла связь с файлом");
        Assert(files.NetworkCalls == 0 && files.Connection.Downloads == (existing ? 0 : 1), "Полный файл скачивается повторно или вызвана настоящая сеть");
        message.message = "😀 bold | updated";
        await files.CallAsync("HandleMessageContentAsync", files.Settings, new TgParseHelper(), message, new TgChatCache(),
            new TgForumTopicSettings(), message, new TgMessageSettings { CurrentChatId = 100, CurrentMessageId = 42 }, CancellationToken.None);
        await files.CallAsync("FlushMessageBufferAsync", new TgParseHelper(), true, false, true, CancellationToken.None);
        var repeated = await messages.GetDtoAsync(x => x.SourceId == 100 && x.Id == 42);
        Assert(repeated.MessageText == message.message && await messages.GetCountAsync(x => x.SourceId == 100 && x.Id == 42) == 1,
            "Повторное чтение не обновило подпись или создало дубликат");
        Assert(files.Connection.Downloads == (existing ? 0 : 1), "Обновление подписи скачало готовый файл заново");
    }

    // Telegram помечает предпросмотр ссылки как медиа, но текст всё равно нужно сохранить
    private static async Task TextWithWebPreviewStillSavedAsync()
    {
        using var storage = new TgStorageService();
        using var files = new MediaFixture(); files.Settings.SourceVm.Dto.IsSaveMessages = true;
        typeof(TgConnectClientBase).GetProperty("StorageManager", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(files.Connection, storage);
        files.Connection.DicChats[100] = new TL.Channel { id = 100, access_hash = 123 };
        var message = new TL.Message { id = 43, peer_id = new TL.PeerChannel { channel_id = 100 }, date = DateTime.UtcNow,
            flags = TL.Message.Flags.has_media, message = "https://t.me/durov", media = new TL.MessageMediaWebPage() };
        await files.CallAsync("HandleMessageContentAsync", files.Settings, new TgParseHelper(), message, new TgChatCache(),
            new TgForumTopicSettings(), message, new TgMessageSettings { CurrentChatId = 100, CurrentMessageId = 43 }, CancellationToken.None);
        await files.CallAsync("FlushMessageBufferAsync", new TgParseHelper(), true, false, true, CancellationToken.None);
        using var messages = new TgEfMessageRepository();
        var saved = await messages.GetDtoAsync(x => x.SourceId == 100 && x.Id == 43);
        Assert(saved.Id == 43 && saved.MessageText == message.message, "Предпросмотр ссылки привёл к потере текста");
    }
}
