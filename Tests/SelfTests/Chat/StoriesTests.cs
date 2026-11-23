namespace OpenTgResearcherDesktop.ChatSelfTests;

internal static partial class Program
{
    private static IEnumerable<(string Name, Func<Task> Run)> GetStoriesTests() =>
    [
        ("StoryJournalSearchAndSelection", StoryJournalSearchAndSelectionAsync),
        ("StoryImportKeepsAllStoriesAndSources", StoryImportKeepsAllStoriesAndSourcesAsync),
        ("StoryArchiveDownloadsAndKeepsDeletedMedia", StoryArchiveDownloadsAndKeepsDeletedMediaAsync),
        ("StoryArchiveRejectsPartialAndEscapingFiles", StoryArchiveRejectsPartialAndEscapingFilesAsync),
        ("StoryArchiveClearDeletesOnlyRequestedData", StoryArchiveClearDeletesOnlyRequestedDataAsync),
    ];

    // Проверяем оба режима очистки и независимость операции от фильтра журнала
    private static async Task StoryArchiveClearDeletesOnlyRequestedDataAsync()
    {
        using var fixture = new MediaFixture();
        var settings = new ITgSettingsService { AppStorage = Path.Combine(fixture.Directory, "stories.db") };
        using var vm = new TgStoriesViewModel(_state, settings, _navigation, null!);
        using var repository = new TgEfStoryRepository();
        using var users = new TgEfUserRepository();
        var userCount = await users.GetCountAsync();
        await vm.OnNavigatedToAsync(null);
        var mediaPath = Path.Combine("user-100", "7-300.mp4");
        var media = Path.Combine(vm.ArchiveDirectory, mediaPath);
        var marker = media + ".deleted";
        var keep = Path.Combine(vm.ArchiveDirectory, "keep.mp4");
        var outside = Path.Combine(fixture.Directory, "outside.mp4");
        Directory.CreateDirectory(Path.GetDirectoryName(media)!);
        foreach (var file in new[] { media, marker, keep, outside })
            await File.WriteAllTextAsync(file, "fixture");
        var stories = new TgEfStoryDto[]
        {
            new() { Id = 7, FromId = 100, Caption = "Видео", MediaPath = mediaPath, IsDeleted = true },
            new() { Id = 8, FromId = 101 },
        };
        await repository.SaveListAsync(stories);
        await vm.ReloadUiAsync();
        vm.SearchText = "Видео";
        Assert(vm.Rows.Count == 1, "Для проверки нужен отфильтрованный журнал");
        await vm.ClearArchiveAsync(includeFiles: false);
        Assert(await repository.GetCountAsync() == 0 && vm.SelectedStory is null && vm.IsJournalEmpty, "Очистка оставила записи или карточку");
        Assert(new[] { media, marker, keep }.All(File.Exists), "Очистка без флажка изменила файлы");

        await repository.SaveListAsync(stories);
        await repository.SaveAsync(new() { Id = 9, FromId = 101, MediaPath = Path.Combine("..", "outside.mp4") });
        await vm.ClearArchiveAsync(includeFiles: true);
        Assert(vm.ArchiveMessage == "StoriesClearWithFilesError" && await repository.GetCountAsync() == 3
            && new[] { media, marker, keep, outside }.All(File.Exists), "Недопустимый путь должен остановить очистку до удаления");
        var badPath = await repository.GetByDtoAsync(new() { Id = 9, FromId = 101 });
        await repository.DeleteAsync(badPath.Item!);

        vm.ArchiveDirectoryInput = fixture.Directory;
        await vm.ClearArchiveAsync(includeFiles: true);
        Assert(await repository.GetCountAsync() == 0 && await users.GetCountAsync() == userCount, "Очистка затронула не только таблицу сторис");
        Assert(!File.Exists(media) && !File.Exists(marker) && File.Exists(keep) && File.Exists(outside),
            "Очистка должна удалить только медиа сторис и их отметки из сохранённого каталога");
    }

    private static async Task StoryJournalSearchAndSelectionAsync()
    {
        var now = DateTime.UtcNow;
        using var repository = new TgEfStoryRepository();
        await repository.SaveListAsync([
            new() { Id = 10, FromId = 100, FromName = "Мастерская", Caption = "Подробная подпись", Date = now.AddDays(-2), ExpireDate = now.AddDays(-1) },
            new() { Id = 11, FromId = 100, FromName = "Фотоархив", Caption = "Утро на набережной", Date = now, ExpireDate = now.AddDays(1) },
            new() { Id = 12, FromId = 101, Date = now.AddDays(-3) },
        ]);
        using var vm = new TgStoriesViewModel(_state, _settings, _navigation, null!);
        await vm.OnNavigatedToAsync(null);
        Assert(vm.Errors.Count == 0 && vm.Rows.Select(row => row.Id).SequenceEqual(["11", "10", "12"])
            && vm.SelectedStory?.Id == "11", "Сторис должны сортироваться по дате публикации, с выбором самой новой");
        Assert(vm.Groups.Count == 2 && vm.Groups[0].Count == 2, "Публикации одного источника должны попадать в общую группу");
        vm.SortIndex = 2;
        Assert(vm.Rows.Select(row => row.Id).SequenceEqual(["12", "11", "10"]), "Сортировка по ID должна быть числовой");
        vm.SortIndex = 1;
        Assert(vm.Groups.Select(group => group.Header).SequenceEqual(new[] { "Фотоархив", "ID 101" }.Order(StringComparer.CurrentCultureIgnoreCase)),
            "Порядок групп должен соответствовать сортировке по имени");
        vm.SortIndex = 0;
        vm.SelectedStory = vm.Rows[1];
        await vm.ReloadUiAsync();
        Assert(vm.SelectedStory?.Id == "10" && vm.SelectedStory.Caption == "Подробная подпись", "Повторная загрузка потеряла выбранную запись");
        vm.SearchText = "ФОТОАРХИВ";
        Assert(vm.Rows.Count == 1 && vm.SelectedStory?.Id == "11", "Поиск по автору не обновил журнал и выбор");
        vm.SearchText = "на набережной";
        Assert(vm.Rows.Count == 1 && vm.SelectedStory?.IsActive == true, "Поиск по подписи не нашёл активную сторис");
        vm.FilterIndex = 2;
        Assert(vm.Rows.Count == 0 && vm.SelectedStory is null, "Фильтр должен применяться вместе с поиском и очищать карточку при пустой выдаче");
        vm.SearchText = string.Empty;
        Assert(vm.Rows.Single().Id == "10", "Запись с неизвестным сроком не должна считаться истёкшей");
        vm.FilterIndex = 1;
        Assert(vm.Rows.Single().Id == "11", "В фильтр активных попала лишняя запись");
        vm.FilterIndex = 0;
        Assert(vm.Rows.Single(row => row.Id == "12").Author == "ID 101", "Для старой записи без имени отсутствует подпись источника");
        await vm.ClearViewCommand.ExecuteAsync(null);
        Assert(vm.IsJournalEmpty && vm.SelectedStory is null && await repository.GetCountAsync() == 3, "Очистка затронула архив");
        await vm.LoadDataStorageCommand.ExecuteAsync(null);
        Assert(vm.Rows.Count == 3 && vm.SelectedStory?.Id == "11", "Чтение архива после очистки не вернуло записи");
    }

    private static async Task StoryImportKeepsAllStoriesAndSourcesAsync()
    {
        using var storage = new TgStorageService();
        using var fixture = new MediaFixture((cache, state) => new StoryTestConnection(cache, state, storage));
        var connection = (StoryTestConnection)fixture.Connection;
        fixture.Settings.SourceVm.Dto.IsSaveMessages = true;

        // Запись с тем же ID и другим источником должна остаться отдельной
        await storage.StoryRepository.SaveAsync(new() { FromId = 300, Id = 1, Caption = "Existing story" });
        await connection.SearchSourcesTgAsync(fixture.Settings, TgEnumSourceType.Story);
        var stories = await storage.StoryRepository.GetListDtosAsync();
        Assert(connection.Cursors.SequenceEqual([null, "next"]), "Импорт не обработал вторую страницу ответа Telegram");
        Assert(fixture.NetworkCalls == 0 && stories.Count == 4 && stories.All(story => story.Uid != Guid.Empty), "Потеряны импортированные записи");
        Assert(stories.Where(story => story.FromId == 100).Select(story => story.Id).Order().SequenceEqual([1L, 2L]),
            "Две сторис одного источника должны сохраняться отдельно");
        Assert(stories.Single(story => story.FromId == 100 && story.Id == 1).FromName == "Фотоархив"
            && stories.Single(story => story.FromId == 200 && story.Id == 1).FromName == "Мария Пример"
            && stories.Single(story => story.FromId == 300).Caption == "Existing story", "Импорт потерял имя или изменил сторис другого источника");
        var uid = stories.Single(story => story.FromId == 200).Uid;
        connection.Caption = "Updated caption";
        await connection.SearchSourcesTgAsync(fixture.Settings, TgEnumSourceType.Story);
        stories = await storage.StoryRepository.GetListDtosAsync();
        Assert(stories.Count == 4 && stories.Single(story => story.Uid == uid).Caption == "Updated caption",
            "Повторный импорт создал дубликат или не обновил подпись");
        var byKey = await storage.StoryRepository.GetByDtoAsync(new() { Id = 1, FromId = 200 });
        Assert(byKey.Item?.Uid == uid, "Репозиторий ищет сторис с повторяющимся ID без учёта источника");
    }

    private static async Task StoryArchiveDownloadsAndKeepsDeletedMediaAsync()
    {
        using var storage = new TgStorageService();
        using var fixture = new MediaFixture((cache, state) => new StoryTestConnection(cache, state, storage));
        var connection = (StoryTestConnection)fixture.Connection;
        var archive = Path.Combine(fixture.Directory, "Stories");
        var selected = new TgEfStoryDto { FromId = 400, FromName = "Мастерская", PeerAccessHash = 201 };
        var initial = await connection.SyncStoryArchiveAsync(archive, selected);
        var previous = await storage.StoryRepository.GetListDtosAsync();
        Assert(initial is { Stories: 4, Downloaded: 3, Failed: 0 } && connection.Offsets.SequenceEqual([0, 5, 3]),
            "Импорт автора должен пройти по активным и двум страницам закреплённых сторис");
        Assert(previous.Single(row => row.Id == 4).MediaPath.Length == 0, "Telegram ограничил сохранение этой сторис");
        var photoRow = previous.Single(row => row.Id == 8);
        var photoPath = TgStoryMediaArchive.ResolvePath(archive, photoRow.MediaPath);
        Assert(photoPath.EndsWith(".jpg") && File.ReadAllBytes(photoPath).SequenceEqual(new byte[] { 1, 2, 3, 4 }), "Фото не попало в каталог автора");
        Assert(previous.Any(row => row.MediaPath.EndsWith(".mp4")), "Видео не сохранилось в архиве");

        connection.ReportDeleted = true;
        var repeated = await connection.SyncStoryArchiveAsync(archive, selected);
        var after = await storage.StoryRepository.GetListDtosAsync();
        var deleted = after.Single(row => row.Id == 8);
        Assert(repeated.Downloaded == 0 && deleted.IsDeleted && deleted.MediaPath == photoRow.MediaPath
            && File.Exists(photoPath) && File.Exists(photoPath + ".deleted"), "Удаление затронуло файл или потеряло его отметку");
        Assert(after.Count == previous.Count && fixture.NetworkCalls == 0, "Повторное чтение создало дубликаты");

        using var vm = new TgStoriesViewModel(_state, _settings, _navigation, null!);
        await vm.OnNavigatedToAsync(null);
        vm.FilterIndex = 3;
        Assert(vm.Rows.Single().Id == "8" && vm.SelectedStory?.Status == "StoriesDeleted", "Удалённая публикация не видна в журнале");
    }

    private static async Task StoryArchiveRejectsPartialAndEscapingFilesAsync()
    {
        using var fixture = new MediaFixture();
        var root = fixture.Directory;
        Assert(TgStoryMediaArchive.ResolvePath(root, Path.Combine("..", "other.jpg")) == string.Empty,
            "Относительный путь не должен покидать каталог сторис");
        var path = Path.Combine(root, "story.mp4");
        await File.WriteAllBytesAsync(path, [9]);
        try
        {
            await TgStoryMediaArchive.SaveAsync(path, 4, async stream => await stream.WriteAsync(new byte[] { 1, 2 }), default);
            throw new InvalidOperationException("Неполная загрузка была принята");
        }
        catch (IOException)
        {
            Assert(File.ReadAllBytes(path).SequenceEqual(new byte[] { 9 }) && !Directory.EnumerateFiles(root, "*.part").Any(),
                "Неполная загрузка перезаписала файл или оставила временные данные");
        }
    }
}

internal sealed class StoryTestConnection(FusionCache cache, LoadStateService state, TgStorageService storage) : MediaTestConnection(cache, state)
{
    public List<string?> Cursors { get; } = [];
    public string Caption { get; set; } = "Original caption";
    public bool ReportDeleted { get; set; }
    public List<int> Offsets { get; } = [];

    public override Task LoginUserAsync(bool isProxyUpdate)
    {
        StorageManager = storage;
        return Task.CompletedTask;
    }

    protected override Task<TL.Stories_AllStories?> ReadStoriesPageAsync(string? cursor, CancellationToken ct)
    {
        Cursors.Add(cursor);
        var now = DateTime.UtcNow;
        TL.Stories_AllStories page = cursor is null
            ? new()
            {
                flags = TL.Stories_AllStories.Flags.has_more, state = "next",
                chats = new() { [100] = new TL.Channel { id = 100, title = "Фотоархив" } },
                users = [],
                peer_stories =
                [
                    new() { peer = new TL.PeerChannel { channel_id = 100 }, stories =
                        [
                            new TL.StoryItem { id = 1, date = now, expire_date = now.AddDays(1), caption = "First story", entities = [] },
                            new TL.StoryItem { id = 2, date = now, expire_date = now.AddDays(1), caption = "Second story" },
                        ] },
                ],
            }
            : new()
            {
                chats = [],
                users = new() { [200] = new TL.User { id = 200, first_name = "Мария", last_name = "Пример" } },
                peer_stories =
                [
                    new() { peer = new TL.PeerUser { user_id = 200 }, stories =
                        [new TL.StoryItem { id = 1, date = now, expire_date = now.AddDays(1), caption = Caption }] },
                ],
            };
        return Task.FromResult<TL.Stories_AllStories?>(page);
    }

    protected override Task<TL.StoryItemBase[]> ReadPeerStoryItemsAsync(TL.InputPeer peer, CancellationToken ct)
    {
        TL.StoryItem protectedStory = new() { id = 4, flags = TL.StoryItem.Flags.noforwards, media = GetPhoto() };
        return Task.FromResult<TL.StoryItemBase[]>(ReportDeleted
            ? [protectedStory] : [new TL.StoryItem { id = 8, caption = "Фото", media = GetPhoto() }, protectedStory]);
    }

    protected override Task<TL.StoryItemBase[]> ReadStoryProfilePageAsync(TL.InputPeer peer, int offset, int limit, bool archive, CancellationToken ct)
    {
        Offsets.Add(offset);
        return Task.FromResult<TL.StoryItemBase[]>(offset switch
        {
            0 => [new TL.StoryItem { id = 5, caption = "Видео", media = new TL.MessageMediaDocument
                { document = new TL.Document { id = 88, size = 4, mime_type = "video/mp4" } } }],
            5 => [new TL.StoryItemSkipped { id = 3 }],
            _ => [],
        });
    }

    protected override Task<TL.StoryItemBase[]> ReadStoryItemsByIdAsync(TL.InputPeer peer, int[] ids, CancellationToken ct) =>
        Task.FromResult<TL.StoryItemBase[]>([.. ids.Select(id => id == 8 ? (TL.StoryItemBase)new TL.StoryItemDeleted { id = 8 }
            : new TL.StoryItem { id = id, media = GetPhoto() })]);

    private static TL.MessageMediaPhoto GetPhoto() => new()
    {
        photo = new TL.Photo { id = 55, sizes = [new TL.PhotoSize { size = 4, w = 800, h = 800, type = "x" }] },
    };
}
