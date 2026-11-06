namespace OpenTgResearcherDesktop.ChatSelfTests;

internal static class MediaTests
{
    // Читаем выборку сообщений и файлы пользователя без изменения БД, файлов и авторизации
    internal static async Task<int> InspectAsync(string database, long chatId, string directory)
    {
        try
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
                { DataSource = database, Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly };
            await using var context = new TestContext(new DbContextOptionsBuilder().UseSqlite(connection.ToString()).Options);
            var messages = await context.Messages.AsNoTracking().Where(x => x.SourceId == chatId).OrderByDescending(x => x.Id).Take(1000)
                .Select(x => new TgEfMessageDto { Id = x.Id, Type = x.Type, Message = x.Message, Size = x.Size }).ToListAsync();
            foreach (var message in messages) message.Directory = directory;
            var files = new DirectoryInfo(directory).EnumerateFiles().ToList();
            Console.WriteLine($"Только чтение: файлов {files.Count}; нулевых файлов {files.Count(x => x.Length == 0)}; сообщений {messages.Count}");
            Console.WriteLine($"Найдены локальные изображения: {messages.Count(x => x.IsImageExists)}; обложки: {messages.Count(x => x.IsThumbExists)}");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    // Проверяем настоящие DTO, обработчики загрузки и файлы в отдельном каталоге
    internal static IEnumerable<(string Name, Func<Task> Run)> GetTests() =>
    [
        ("DocumentThumbnailSidecar", DocumentThumbnailSidecarAsync),
        ("NumberedDocumentThumbnail", NumberedDocumentThumbnailAsync),
        ("NumberedMessageFindsLegacyThumbnail", NumberedMessageFindsLegacyThumbnailAsync),
        ("PhotoRelativePath", PhotoRelativePathAsync),
        ("EmptyImageNotDisplayed", EmptyImageNotDisplayedAsync),
        ("EmptyThumbnailDownloaded", EmptyThumbnailDownloadedAsync),
        ("EmptyPhotoDownloaded", EmptyPhotoDownloadedAsync),
        ("EmptyDocumentDownloaded", EmptyDocumentDownloadedAsync),
        ("FailedDownloadRetried", FailedDownloadRetriedAsync),
        ("ExistingFileWithoutIdReused", ExistingFileWithoutIdReusedAsync),
        ("ExistingFileAndThumbnailMovedTogether", ExistingFileAndThumbnailMovedTogetherAsync),
        ("ExistingFileSizeMismatchPreserved", ExistingFileSizeMismatchPreservedAsync),
        ("CompleteNumberedFilePreserved", CompleteNumberedFilePreservedAsync),
        ("CompleteFileAdoptsLegacyThumbnail", CompleteFileAdoptsLegacyThumbnailAsync),
        ("PartialFileRewriteSetting", PartialFileRewriteSettingAsync),
        ("DisconnectedParseReleasesState", DisconnectedParseReleasesStateAsync),
        ("NetworkFileProgress", NetworkFileProgressAsync),
        ("MediaHandlersPassChat", MediaHandlersPassChatAsync),
        ("NetworkFileStop", NetworkFileStopAsync), ("ParsingStopUsesOwnToken", ParsingStopUsesOwnTokenAsync),
        ("NetworkProgressCallbackFailure", NetworkProgressCallbackFailureAsync),
        ("NetworkAsyncProgressFailure", NetworkAsyncProgressFailureAsync), ("PausedMediaWaitsForResume", PausedMediaWaitsForResumeAsync),
        ("CancelledChunkKeepsPendingTasks", CancelledChunkKeepsPendingTasksAsync),
        ("ParsingRequestIdsBeforeResponse", ParsingRequestIdsBeforeResponseAsync),
        ("CancelledParseKeepsPosition", CancelledParseKeepsPositionAsync),
        ("RepeatedPassDownloadsOnlyMissing", RepeatedPassDownloadsOnlyMissingAsync),
        ("FloodWaitHonorsStop", FloodWaitHonorsStopAsync),
    ];

    // Документ, обложка и фотография получают чат из настроек своей загрузки
    private static async Task MediaHandlersPassChatAsync()
    {
        using var files = new MediaFixture();
        files.Settings.SourceVm.Dto.Id = 101; files.Settings.SourceVm.Dto.Title = "Первый чат";
        var document = new TL.Document { id = 1, size = 4, thumbs = [new TL.PhotoCachedSize { type = "m", bytes = [1, 2, 3], w = 1, h = 1 }] };
        await files.CallAsync("DownloadMediaThumbnailFromMessageAsync", files.Settings, new TL.MessageMediaDocument { document = document },
            files.Media, CancellationToken.None);
        await files.CallAsync("DownloadMediaFileFromMessageAsync", files.Settings, new TgParseHelper(), new TL.Message(),
            new TL.MessageMediaDocument { document = document }, new TgMessageSettings(), files.Media, CancellationToken.None);
        File.Delete(files.Media.LocalPathWithNumber);
        files.Settings.SourceVm.Dto.Id = 202; files.Settings.SourceVm.Dto.Title = "Второй чат";
        var photo = new TL.Photo { id = 2, sizes = [new TL.PhotoCachedSize { type = "m", bytes = [1, 2, 3, 4], w = 1, h = 1 }] };
        await files.CallAsync("DownloadMediaFileFromMessageAsync", files.Settings, new TgParseHelper(), new TL.Message(),
            new TL.MessageMediaPhoto { photo = photo }, new TgMessageSettings(), files.Media, CancellationToken.None);
        Assert(files.Connection.DownloadChats.SequenceEqual(new (long, string)[] { (101, "Первый чат"), (101, "Первый чат"), (202, "Второй чат") }),
            "Обложка, документ или фото потеряли принадлежность своему чату");
        Assert(files.NetworkCalls == 0, "Проверка принадлежности файлов обратилась к Telegram");
    }

    // Настоящий обработчик прогресса сохраняет байты и продолжает обновлять прежнюю страницу скачивания
    private static Task NetworkFileProgressAsync()
    {
        using var files = new MediaFixture(); var uid = Guid.NewGuid(); long forwarded = 0;
        files.State.NetworkActivity.Start(uid, TgBusinessLogic.Models.TgNetworkOperationKind.Download);
        var factory = typeof(TgConnectClientBase).GetMethod("NetworkProgressForFile", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Нет сетевого обработчика прогресса файла");
        WTelegram.Client.ProgressCallback existing = (received, total) => forwarded = received;
        var progress = (WTelegram.Client.ProgressCallback)factory.Invoke(files.Connection, [uid, "clip.mp4", existing, CancellationToken.None])!;
        progress(25, 100);
        var operation = files.State.NetworkActivity.Operations.Single();
        Assert(operation.Detail == "clip.mp4" && operation.Progress == 25 && operation.TransferredBytes == 25 && forwarded == 25,
            "Настоящий обработчик потерял сетевые сведения или прежний прогресс");
        return Task.CompletedTask;
    }

    // Ошибка отображения не должна прерывать передачу файла
    private static Task NetworkProgressCallbackFailureAsync()
    {
        using var files = new MediaFixture(); var uid = Guid.NewGuid();
        files.State.NetworkActivity.Start(uid, TgBusinessLogic.Models.TgNetworkOperationKind.Download);
        WTelegram.Client.ProgressCallback failed = (_, _) => throw new InvalidOperationException("Тестовая ошибка отображения прогресса");
        var factory = typeof(TgConnectClientBase).GetMethod("NetworkProgressForFile", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var progress = (WTelegram.Client.ProgressCallback)factory.Invoke(files.Connection, [uid, "clip.mp4", failed, CancellationToken.None])!;
        progress(25, 100);
        Assert(files.State.NetworkActivity.Operations.Single().Progress == 25, "Ошибка отображения потеряла фактический прогресс");
        return Task.CompletedTask;
    }

    // Асинхронный обработчик страницы также обязан перехватить и записать свою ошибку
    private static async Task NetworkAsyncProgressFailureAsync()
    {
        using var files = new MediaFixture();
        var marker = "Тестовая асинхронная ошибка прогресса " + Guid.NewGuid().ToString("N"); var calls = 0;
        files.Connection.SetupUpdateStateFile(async (_, _, _, _, _, _, _, _) =>
        {
            calls++; await Task.Yield(); throw new InvalidOperationException(marker);
        });
        var factory = typeof(TgConnectClientBase).GetMethod("ClientProgressForFile", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var progress = (WTelegram.Client.ProgressCallback)factory.Invoke(files.Connection, [new TgMessageSettings(), "clip.mp4"])!;
        progress(25, 100);
        var logPath = Path.Combine(AppContext.BaseDirectory, "Chat.SelfTest.log");
        for (var attempt = 0; attempt < 30; attempt++)
        {
            if (ReadProgressLog(logPath).Contains(marker)) break;
            await Task.Delay(25);
        }
        Assert(calls == 1 && ReadProgressLog(logPath).Contains(marker), "Асинхронная ошибка не записана в журнал");
    }

    // Согласуем чтение журнала с настоящим логгером, который пишет из фонового обработчика
    private static string ReadProgressLog(string path)
    {
        var gate = typeof(TgLogUtils).GetField("_logLock", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        lock (gate) return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    // Настоящий обработчик не создаёт файл на паузе, а после продолжения сохраняет его байты
    private static async Task PausedMediaWaitsForResumeAsync()
    {
        using var files = new MediaFixture(); var uid = Guid.NewGuid(); using var cancel = new CancellationTokenSource();
        files.State.NetworkActivity.Start(uid, TgBusinessLogic.Models.TgNetworkOperationKind.Parsing);
        files.State.NetworkActivity.RegisterCancellation(uid, cancel.Cancel);
        files.State.NetworkActivity.ToggleParsingPause();
        var helper = new TgParseHelper { NetworkOperationId = uid };
        var photo = new TL.MessageMediaPhoto { photo = new TL.Photo { id = 1, sizes = [new TL.PhotoSize { type = "m", size = 4 }] } };
        var pending = files.CallAsync("DownloadMediaFileFromMessageAsync", files.Settings, helper, new TL.Message(), photo,
            new TgMessageSettings(), files.Media, cancel.Token);
        Assert(!pending.IsCompleted && !File.Exists(files.Media.LocalPathWithNumber) && files.Connection.Downloads == 0,
            "Пауза не остановила запуск файла");
        files.State.NetworkActivity.ToggleParsingPause(); await pending;
        Assert(files.Connection.Downloads == 1 && File.ReadAllBytes(files.Media.LocalPathWithNumber).Length == 4, "Продолжение не скачало файл");
    }

    // Запрос отмены парсинга должен прерывать передачу на следующем обратном вызове WTelegram
    private static Task NetworkFileStopAsync()
    {
        using var files = new MediaFixture(); using var stop = new CancellationTokenSource(); var uid = Guid.NewGuid();
        files.State.NetworkActivity.Start(uid, TgBusinessLogic.Models.TgNetworkOperationKind.Download);
        var factory = typeof(TgConnectClientBase).GetMethod("NetworkProgressForFile", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var parameters = factory.GetParameters().Length == 4 ? new object?[] { uid, "clip.mp4", null, stop.Token } : [uid, "clip.mp4", null];
        var progress = (WTelegram.Client.ProgressCallback)factory.Invoke(files.Connection, parameters)!;
        stop.Cancel();
        try { progress(25, 100); throw new InvalidOperationException("Передача файла проигнорировала отмену парсинга"); }
        catch (OperationCanceledException) { }
        return Task.CompletedTask;
    }

    // Настоящий ParseChatAsync регистрирует собственную отмену и дожидается завершающих действий
    private static async Task ParsingStopUsesOwnTokenAsync()
    {
        using var files = new MediaFixture(); files.Connection.Client = null;
        var entered = new TaskCompletionSource(); var release = new TaskCompletionSource();
        files.Connection.SetupUpdateTasksViewModel(_ => { entered.TrySetResult(); return release.Task; });
        var parsing = files.Connection.ParseChatAsync(files.Settings);
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
            files.State.NetworkActivity.CancelParsing();
            Assert(!files.State.DownloadToken.IsCancellationRequested, "Отмена парсинга изменила общий токен загрузок");
            Assert(files.State.NetworkActivity.Operations.Single().IsCancellationRequested, "Настоящий парсинг не получил запрос остановки");
        }
        finally { release.TrySetResult(); await parsing; }
        Assert(!files.State.IsDownloadProcessing && files.State.NetworkActivity.Operations.Single().State == TgBusinessLogic.Models.TgNetworkOperationState.Canceled,
            "Остановленный парсинг не освободил состояние или потерял отмену");
    }

    // Отказ до обращения к Telegram не должен оставлять интерфейс в состоянии загрузки
    private static async Task DisconnectedParseReleasesStateAsync()
    {
        using var files = new MediaFixture();
        files.Settings.SourceVm.Dto.Id = 101; files.Settings.SourceVm.Dto.Title = "Первый чат";
        files.Connection.Client = null;
        await files.Connection.ParseChatAsync(files.Settings);
        Assert(!files.State.IsDownloadProcessing, "После раннего выхода парсинг остался активным");
        var field = typeof(TgConnectClientBase).GetField("_downloadSettings", BindingFlags.Instance | BindingFlags.NonPublic)!;
        Assert(field.GetValue(files.Connection) is null, "После раннего выхода остались настройки завершённого парсинга");
        Assert(files.NetworkCalls == 0, "Отключённый клиент попытался обратиться к Telegram");
        var parsing = files.State.NetworkActivity.Operations.Single(x => x.Kind == TgBusinessLogic.Models.TgNetworkOperationKind.Parsing);
        Assert(parsing.State == TgBusinessLogic.Models.TgNetworkOperationState.Failed, "Отказ парсинга без подключения отображается как завершение");
        Assert(parsing.ChatId == 101 && parsing.ChatTitle == "Первый чат", "Настоящий парсинг не передал свой чат сетевой операции");
    }

    // Настоящий парсер публикует начало порции до чтения ответа из кеша и сохраняет ID при паузе
    private static async Task ParsingRequestIdsBeforeResponseAsync()
    {
        using var files = new MediaFixture();
        var uid = Guid.NewGuid();
        await files.State.PrepareDownloadTokenAsync(uid);
        files.State.NetworkActivity.SetKind(uid, TgNetworkOperationKind.Parsing);
        files.State.NetworkActivity.RegisterCancellation(uid, () => { });
        var helper = new TgParseHelper { NetworkOperationId = uid, SourceFirstId = 101, SourceLastId = 1000,
            Channel = new TL.Channel { id = 321 }, AppDto = new() { UseClient = true } };
        var key = TgBusinessLogic.Helpers.TgCacheUtils.GetCacheKeyMessages(321, 101, 150);
        await files.Cache.SetAsync<TL.Messages_MessagesBase>(key, new TL.Messages_Messages { messages = [new TL.MessageEmpty { id = 101 }] });
        TgNetworkOperation? beforeResponse = null;
        files.Cache.Events.Hit += (_, args) =>
        {
            if (args.Key == key) beforeResponse = files.State.NetworkActivity.Operations.Single(x => x.Id == uid);
        };
        files.State.NetworkActivity.ToggleParsingPause();
        var pending = files.CallAsync("DownloadMessagesWithClientAsync", files.Settings, helper, 50, files.State.DownloadToken);
        Assert(!pending.IsCompleted && beforeResponse is null && helper.SourceFirstId == 101, "Пауза не остановила запрос порции");
        files.State.NetworkActivity.ToggleParsingPause();
        await pending;
        Assert(beforeResponse is not null, "Парсер не запросил подготовленный ответ с сообщениями");
        Assert((long?)beforeResponse!.GetType().GetProperty("CurrentMessageId")?.GetValue(beforeResponse) == 101
            && (long?)beforeResponse.GetType().GetProperty("LastMessageId")?.GetValue(beforeResponse) == 1000,
            "До получения ответа парсер не сообщил запрашиваемый ID 101 из 1000");
        var afterResponse = files.State.NetworkActivity.Operations.Single(x => x.Id == uid);
        Assert(afterResponse.Progress == 15 && helper.SourceFirstId == 151, "Запрос потерял процент или продвижение порции");
        Assert(files.NetworkCalls == 0, "Изолированная проверка попыталась обратиться к Telegram");
    }

    // При отмене очередь должна остаться доступной завершающему ожиданию задач
    private static async Task CancelledChunkKeepsPendingTasksAsync()
    {
        using var files = new MediaFixture();
        await files.State.PrepareDownloadTokenAsync(Guid.NewGuid());
        var ct = files.State.DownloadToken;
        files.State.StopHardDownloadProcessing();
        Assert(ct.IsCancellationRequested && !files.State.DownloadToken.IsCancellationRequested, "Стенд не воспроизвёл замену токена после остановки");
        var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var helper = new TgParseHelper { SourceFirstId = 1, SourceLastId = 5 };
        helper.AddTask(pending.Task, 1);
        await files.CallAsync("DownloadMessagesWithClientAsync", files.Settings, helper, 5, ct);
        Assert(helper.TasksCount == 1 && helper.Tasks.Single().Task == pending.Task, "Отмена потеряла незавершённую задачу");
        Assert(helper.SourceFirstId == 1 && files.NetworkCalls == 0, "Отмена запустила следующую порцию сообщений");
        pending.SetResult();
        await helper.Tasks.Single().Task;
    }

    // Отменённый проход нельзя отмечать обработанным до последнего сообщения
    private static async Task CancelledParseKeepsPositionAsync()
    {
        using var files = new MediaFixture();
        files.Settings.SourceVm.Dto.FirstId = 10;
        files.Settings.SourceVm.Dto.Count = 100;
        await files.State.PrepareDownloadTokenAsync(Guid.NewGuid());
        var ct = files.State.DownloadToken;
        files.State.StopHardDownloadProcessing();
        await files.CallAsync("FinalizeDownloadSessionAsync", files.Settings, new TgParseHelper(), Guid.NewGuid(), ct);
        Assert(files.Settings.SourceVm.Dto.FirstId == 10 && !files.State.IsDownloadProcessing, "Остановка продвинула позицию до конца чата");
    }

    // Повторный проход проверяет диск заново, пропуская готовое и восстанавливая удалённое
    private static async Task RepeatedPassDownloadsOnlyMissingAsync()
    {
        using var files = new MediaFixture();
        var media = new TL.MessageMediaPhoto { photo = new TL.Photo { id = 1, sizes = [new TL.PhotoSize { type = "m", size = 4 }] } };
        var helper = new TgParseHelper();
        await files.CallAsync("DownloadMediaFileFromMessageAsync", files.Settings, helper, new TL.Message(), media,
            new TgMessageSettings(), files.Media, CancellationToken.None);
        var timestamp = File.GetLastWriteTimeUtc(files.Media.LocalPathWithNumber);
        await files.CallAsync("DownloadMediaFileFromMessageAsync", files.Settings, helper, new TL.Message(), media,
            new TgMessageSettings(), files.Media, CancellationToken.None);
        Assert(files.Connection.Downloads == 1 && File.GetLastWriteTimeUtc(files.Media.LocalPathWithNumber) == timestamp,
            "Повторный проход перекачал готовый файл");
        File.Delete(files.Media.LocalPathWithNumber);
        await files.CallAsync("DownloadMediaFileFromMessageAsync", files.Settings, helper, new TL.Message(), media,
            new TgMessageSettings(), files.Media, CancellationToken.None);
        Assert(files.Connection.Downloads == 2 && File.ReadAllBytes(files.Media.LocalPathWithNumber).Length == 4,
            "Повторный проход не восстановил удалённый файл");
    }

    // Остановка должна прерывать ожидание FLOOD_WAIT, освобождая общий шлюз Telegram
    private static async Task FloodWaitHonorsStopAsync()
    {
        var service = new TgFloodControlService();
        GC.SuppressFinalize(service);
        using var stop = new CancellationTokenSource();
        var calls = 0;
        var operation = service.ExecuteWithFloodControlAsync<int>(_ =>
        {
            calls++;
            return Task.FromException<int>(new InvalidOperationException("FLOOD_WAIT_1"));
        }, isThrow: true, stop.Token);
        await Task.Delay(50);
        stop.Cancel();
        var timer = System.Diagnostics.Stopwatch.StartNew();
        try { await operation.WaitAsync(TimeSpan.FromMilliseconds(500)); }
        catch (OperationCanceledException) { }
        catch (TimeoutException)
        {
            try { await operation; } catch (OperationCanceledException) { }
            throw new InvalidOperationException("Остановка не прервала ожидание защиты от флуда за 500 мс");
        }
        Assert(operation.IsCanceled && calls == 1, "После остановки запрос Telegram повторился или отмена потеряна");
        Assert(await service.ExecuteWithFloodControlAsync(_ => Task.FromResult(42), isThrow: true, CancellationToken.None) == 42,
            "После остановки общий шлюз Telegram остался заблокирован");
        Console.WriteLine($"Отмена FLOOD_WAIT: {timer.ElapsedMilliseconds} мс");
    }

    // Обложка документа хранится рядом с медиафайлом с расширением thumb.jpg
    private static Task DocumentThumbnailSidecarAsync()
    {
        using var files = new MediaFixture();
        File.WriteAllBytes(Path.Combine(files.Directory, "clip.thumb.jpg"), [1, 2, 3]);
        var message = new TgEfMessageDto { Id = 42, Type = TgEnumMessageType.Document, Directory = files.Directory, Message = "clip.mp4" };
        Assert(message.ThumbFullPath == Path.Combine(files.Directory, "clip.thumb.jpg") && message.IsThumbExists,
            "Не найдена отдельная обложка документа");
        return Task.CompletedTask;
    }

    // Старые сообщения без локального пути должны находить имя с дополненным нулями ID
    private static Task NumberedDocumentThumbnailAsync()
    {
        using var files = new MediaFixture();
        File.WriteAllBytes(Path.Combine(files.Directory, "00042 clip.mp4"), [1, 2, 3, 4]);
        File.WriteAllBytes(Path.Combine(files.Directory, "00042 clip.thumb.jpg"), [1, 2, 3]);
        var message = new TgEfMessageDto { Id = 42, Type = TgEnumMessageType.Document, Directory = files.Directory, Message = "clip.mp4", Size = 4 };
        Assert(message.ThumbFullPath == Path.Combine(files.Directory, "00042 clip.thumb.jpg"), "Не найдена обложка файла с ID");
        return Task.CompletedTask;
    }

    // Запись с ID в имени должна находить прежнюю обложку без ID
    private static Task NumberedMessageFindsLegacyThumbnailAsync()
    {
        using var files = new MediaFixture();
        var thumbnail = Path.Combine(files.Directory, "clip.thumb.jpg");
        File.WriteAllBytes(thumbnail, [1, 2, 3]);
        var message = new TgEfMessageDto { Id = 42, Type = TgEnumMessageType.Document, Directory = files.Directory, Message = "00042 clip.mp4" };
        Assert(message.ThumbFullPath == thumbnail, "Имя с ID не сопоставлено с обложкой без ID");
        return Task.CompletedTask;
    }

    // Исторически сохранённый относительный путь начинается с разделителя каталога
    private static Task PhotoRelativePathAsync()
    {
        using var files = new MediaFixture();
        File.WriteAllBytes(Path.Combine(files.Directory, "photo.jpg"), [1, 2, 3]);
        var message = new TgEfMessageDto { Directory = files.Directory, Message = "Подпись | \\photo.jpg", Type = TgEnumMessageType.Photo };
        Assert(message.ImageFullPath == Path.Combine(files.Directory, "photo.jpg"), "Относительный путь превратился в путь от корня диска");
        return Task.CompletedTask;
    }

    // Пустая картинка не должна создавать видимость доступного предпросмотра
    private static Task EmptyImageNotDisplayedAsync()
    {
        using var files = new MediaFixture();
        File.WriteAllBytes(Path.Combine(files.Directory, "photo.jpg"), []);
        var message = new TgEfMessageDto { Directory = files.Directory, Message = "Фото | photo.jpg", Type = TgEnumMessageType.Photo };
        Assert(!message.IsImageExists && message.ImageFullPath == string.Empty, "Нулевой файл отображается как картинка");
        return Task.CompletedTask;
    }

    // Настоящий обработчик восстановления записывает обложку через подменённый транспорт
    private static async Task EmptyThumbnailDownloadedAsync()
    {
        using var files = new MediaFixture();
        File.WriteAllBytes(files.Media.ThumbnailPathWithNumber, []);
        var doc = new TL.Document { id = 1, size = 4, thumbs = [new TL.PhotoCachedSize { type = "m", bytes = [1, 2, 3], w = 1, h = 1 }] };
        await files.CallAsync("DownloadMediaThumbnailFromMessageAsync", files.Settings, new TL.MessageMediaDocument { document = doc },
            files.Media, CancellationToken.None);
        Assert(File.ReadAllBytes(files.Media.ThumbnailPathWithNumber).SequenceEqual(new byte[] { 1, 2, 3 }), "Пустая обложка не загружена повторно");
        Assert(files.Connection.Downloads == 1, "Обработчик не вызвал загрузку обложки");
        Assert(files.NetworkCalls == 0, "Селфтест попытался обратиться к сети");
    }

    // Нулевой основной файл удаляется независимо от настройки перезаписи и загружается заново
    private static async Task EmptyPhotoDownloadedAsync()
    {
        using var files = new MediaFixture();
        File.WriteAllBytes(files.Media.LocalPathWithNumber, []);
        await files.CallAsync("DeleteEmptyOrLessSizeFilesAsync", files.Settings, files.Media, CancellationToken.None);
        var photo = new TL.Photo { id = 1, sizes = [new TL.PhotoCachedSize { type = "m", bytes = [1, 2, 3, 4], w = 1, h = 1 }] };
        await files.CallAsync("DownloadMediaFileFromMessageAsync", files.Settings, new TgParseHelper(), new TL.Message(),
            new TL.MessageMediaPhoto { photo = photo }, new TgMessageSettings(), files.Media, CancellationToken.None);
        Assert(File.ReadAllBytes(files.Media.LocalPathWithNumber).SequenceEqual(new byte[] { 1, 2, 3, 4 }), "Нулевой файл не заменён содержимым");
        Assert(files.Connection.Downloads == 1, "Обработчик не вызвал загрузку основного файла");
        Assert(files.NetworkCalls == 0, "Селфтест попытался обратиться к сети");
    }

    // Нулевой документ должен проходить тот же цикл восстановления, что и фотография
    private static async Task EmptyDocumentDownloadedAsync()
    {
        using var files = new MediaFixture();
        File.WriteAllBytes(files.Media.LocalPathWithNumber, []);
        await files.CallAsync("DeleteEmptyOrLessSizeFilesAsync", files.Settings, files.Media, CancellationToken.None);
        await files.CallAsync("DownloadMediaFileFromMessageAsync", files.Settings, new TgParseHelper(), new TL.Message(),
            new TL.MessageMediaDocument { document = new TL.Document { id = 1, size = 4 } }, new TgMessageSettings(), files.Media, CancellationToken.None);
        Assert(files.Connection.Downloads == 1 && File.ReadAllBytes(files.Media.LocalPathWithNumber).SequenceEqual(new byte[] { 1, 2, 3, 4 }),
            "Нулевой документ не восстановлен при выключенной перезаписи");
    }

    // Ошибка передачи не должна навсегда блокировать повторную загрузку созданным нулевым файлом
    private static async Task FailedDownloadRetriedAsync()
    {
        using var files = new MediaFixture();
        var media = new TL.MessageMediaPhoto { photo = new TL.Photo { id = 1, sizes = [new TL.PhotoSize { type = "m", size = 4 }] } };
        files.Connection.FailNext = true;
        var failed = false;
        try
        {
            await files.CallAsync("DownloadMediaFileFromMessageAsync", files.Settings, new TgParseHelper(), new TL.Message(),
                media, new TgMessageSettings(), files.Media, CancellationToken.None);
        }
        catch (IOException) { failed = true; }
        Assert(failed && File.Exists(files.Media.LocalPathWithNumber) && new FileInfo(files.Media.LocalPathWithNumber).Length == 0,
            "Стенд не воспроизвёл обрыв передачи после создания файла");
        await files.CallAsync("DeleteEmptyOrLessSizeFilesAsync", files.Settings, files.Media, CancellationToken.None);
        await files.CallAsync("DownloadMediaFileFromMessageAsync", files.Settings, new TgParseHelper(), new TL.Message(),
            media, new TgMessageSettings(), files.Media, CancellationToken.None);
        Assert(files.Connection.Downloads == 2 && File.ReadAllBytes(files.Media.LocalPathWithNumber).Length == 4, "Загрузка после ошибки не повторилась");
    }

    // Готовый файл без ID должен переиспользоваться вместо повторного скачивания
    private static async Task ExistingFileWithoutIdReusedAsync()
    {
        using var files = new MediaFixture();
        var original = Path.Combine(files.Directory, files.Media.LocalNameOnly);
        File.WriteAllBytes(original, [1, 2, 3, 4]);
        File.WriteAllBytes(files.Media.LocalPathWithNumber, []);
        await files.CallAsync("MoveExistsFilesAtCurrentDirAsync", files.Settings, files.Media, CancellationToken.None);
        await files.CallAsync("DeleteEmptyOrLessSizeFilesAsync", files.Settings, files.Media, CancellationToken.None);
        await files.CallAsync("DownloadMediaFileFromMessageAsync", files.Settings, new TgParseHelper(), new TL.Message(),
            new TL.MessageMediaDocument { document = new TL.Document { id = 1, size = 4 } }, new TgMessageSettings(), files.Media, CancellationToken.None);
        Assert(!File.Exists(original) && File.ReadAllBytes(files.Media.LocalPathWithNumber).SequenceEqual(new byte[] { 1, 2, 3, 4 }),
            "Совпадающий по имени и размеру файл не переиспользован");
        Assert(files.Connection.Downloads == 0 && files.NetworkCalls == 0, "Готовый файл загружался повторно");
    }

    // Переименование основного файла должно сохранять связь с его обложкой
    private static async Task ExistingFileAndThumbnailMovedTogetherAsync()
    {
        using var files = new MediaFixture();
        File.WriteAllBytes(Path.Combine(files.Directory, files.Media.LocalNameOnly), [1, 2, 3, 4]);
        var originalThumb = Path.Combine(files.Directory, "clip.thumb.jpg");
        File.WriteAllBytes(originalThumb, [4, 3, 2]);
        await files.CallAsync("MoveExistsFilesAtCurrentDirAsync", files.Settings, files.Media, CancellationToken.None);
        Assert(File.Exists(files.Media.LocalPathWithNumber) && File.Exists(files.Media.ThumbnailPathWithNumber)
            && File.ReadAllBytes(files.Media.ThumbnailPathWithNumber).SequenceEqual(new byte[] { 4, 3, 2 }) && !File.Exists(originalThumb),
            "После присоединения ID обложка осталась под прежним именем");
    }

    // Совпадения имени недостаточно, если размер отличается
    private static async Task ExistingFileSizeMismatchPreservedAsync()
    {
        using var files = new MediaFixture();
        var original = Path.Combine(files.Directory, files.Media.LocalNameOnly);
        File.WriteAllBytes(original, [1, 2]);
        await files.CallAsync("MoveExistsFilesAtCurrentDirAsync", files.Settings, files.Media, CancellationToken.None);
        Assert(File.ReadAllBytes(original).Length == 2 && !File.Exists(files.Media.LocalPathWithNumber), "Перенесён файл с другим размером");
    }

    // Полный файл с ID нельзя перезаписывать одноимённым файлом без ID
    private static async Task CompleteNumberedFilePreservedAsync()
    {
        using var files = new MediaFixture();
        File.WriteAllBytes(Path.Combine(files.Directory, files.Media.LocalNameOnly), [1, 2, 3, 4]);
        File.WriteAllBytes(files.Media.LocalPathWithNumber, [4, 3, 2, 1]);
        await files.CallAsync("MoveExistsFilesAtCurrentDirAsync", files.Settings, files.Media, CancellationToken.None);
        Assert(File.ReadAllBytes(files.Media.LocalPathWithNumber).SequenceEqual(new byte[] { 4, 3, 2, 1 }), "Полный файл перезаписан");
    }

    // Готовый основной файл не должен мешать переносу существующей обложки без ID
    private static async Task CompleteFileAdoptsLegacyThumbnailAsync()
    {
        using var files = new MediaFixture();
        File.WriteAllBytes(files.Media.LocalPathWithNumber, [1, 2, 3, 4]);
        File.WriteAllBytes(Path.Combine(files.Directory, "clip.thumb.jpg"), [1, 2, 3]);
        await files.CallAsync("MoveExistsFilesAtCurrentDirAsync", files.Settings, files.Media, CancellationToken.None);
        Assert(File.Exists(files.Media.ThumbnailPathWithNumber) && File.ReadAllBytes(files.Media.ThumbnailPathWithNumber).Length == 3,
            "Наличие основного файла помешало присоединить ID к обложке");
    }

    // Непустой неполный файл обрабатывается согласно настройке перезаписи
    private static async Task PartialFileRewriteSettingAsync()
    {
        using var files = new MediaFixture();
        File.WriteAllBytes(files.Media.LocalPathWithNumber, [1, 2]);
        await files.CallAsync("DeleteEmptyOrLessSizeFilesAsync", files.Settings, files.Media, CancellationToken.None);
        Assert(File.Exists(files.Media.LocalPathWithNumber), "Непустой файл удалён при выключенной перезаписи");
        files.Settings.SourceVm.Dto.IsRewriteFiles = true;
        await files.CallAsync("DeleteEmptyOrLessSizeFilesAsync", files.Settings, files.Media, CancellationToken.None);
        Assert(!File.Exists(files.Media.LocalPathWithNumber), "Неполный файл не подготовлен к повторной загрузке");
    }

    // Проверяем результат, а не отсутствие исключения
    private static void Assert(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}

internal sealed class MediaFixture : IDisposable
{
    private readonly FusionCache _cache = new(new FusionCacheOptions { EnableSyncEventHandlersExecution = true });
    public FusionCache Cache => _cache;
    private readonly LoadStateService _state = new();
    public LoadStateService State => _state;
    private readonly MemoryStream _session = new();
    private readonly WTelegram.Client _client;
    public MediaTestConnection Connection { get; }
    public string Directory { get; } = Path.Combine(Path.GetTempPath(), "OpenTgResearcher-Media-" + Guid.NewGuid().ToString("N"));
    public TgMediaInfoModel Media { get; }
    public TgDownloadSettingsViewModel Settings { get; } = new();
    public int NetworkCalls { get; private set; }

    public MediaFixture()
    {
        System.IO.Directory.CreateDirectory(Directory);
        Connection = new(_cache, _state);
        GC.SuppressFinalize(Connection);
        _client = new(Config, _session);
        _client.TcpHandler = (_, _) => { NetworkCalls++; throw new InvalidOperationException("Сеть запрещена в проверке медиафайлов"); };
        Connection.Client = _client;
        Settings.SourceVm.Dto.IsJoinFileNameWithMessageId = true;
        Settings.SourceVm.Dto.IsDownloadThumbnail = true;
        Settings.SourceVm.Dto.IsRewriteFiles = false;
        Settings.SourceVm.Dto.IsSaveMessages = false;
        Media = new("clip.mp4", 4, DateTime.UtcNow) { LocalPathOnly = Directory, Number = "00042" };
        Media.Normalize(isJoinFileNameWithMessageId: true);
    }

    /// <summary> Вызывает настоящий обработчик файлов без запуска разбора Telegram </summary>
    public async Task CallAsync(string name, params object[] args)
    {
        var method = typeof(TgConnectClientBase).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Не найден обработчик {name}");
        await ((Task)method.Invoke(Connection, args)!).WaitAsync(TimeSpan.FromSeconds(5));
    }

    /// <summary> Освобождает клиент с сессией в памяти и удаляет только созданный тестом каталог </summary>
    public void Dispose()
    {
        _client.Dispose(); _session.Dispose(); _state.Dispose(); _cache.Dispose();
        System.IO.Directory.Delete(Directory, recursive: true);
    }

    // Возвращаем искусственные параметры без чтения пользовательских настроек
    private static string? Config(string name) => name switch { "api_id" => "1", "api_hash" => new string('0', 32), _ => null };
}

// Подменяем только запись байтов из Telegram, сохраняя настоящую логику проверки файлов
internal sealed class MediaTestConnection(FusionCache cache, LoadStateService state)
    : TgConnectClientBase(null!, new TgFloodControlService(), cache, state)
{
    public int Downloads { get; private set; }
    public List<(long ChatId, string Title)> DownloadChats { get; } = [];
    public bool FailNext { get; set; }

    /// <summary> Записывает тестовые байты документа или обложки без сети </summary>
    protected override async Task DownloadDocumentMediaAsync(TL.Document document, Stream output, TL.PhotoSizeBase? thumbSize,
        WTelegram.Client.ProgressCallback? progress, long chatId, string chatTitle, CancellationToken ct = default)
    {
        Downloads++;
        DownloadChats.Add((chatId, chatTitle));
        await output.WriteAsync(thumbSize is null ? new byte[] { 1, 2, 3, 4 } : new byte[] { 1, 2, 3 }, ct);
    }

    /// <summary> Записывает тестовые байты фотографии без сети </summary>
    protected override async Task DownloadPhotoMediaAsync(TL.Photo photo, Stream output, TL.PhotoSizeBase size, long chatId, string chatTitle, CancellationToken ct = default)
    {
        Downloads++;
        DownloadChats.Add((chatId, chatTitle));
        if (FailNext) { FailNext = false; throw new IOException("Тестовый обрыв передачи"); }
        await output.WriteAsync(new byte[] { 1, 2, 3, 4 }, ct);
    }
}
