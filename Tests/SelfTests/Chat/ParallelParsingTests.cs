namespace OpenTgResearcherDesktop.ChatSelfTests;

// Проверяем независимость настоящих запусков парсера без подключения к Telegram
internal static partial class Program
{
    private static IEnumerable<(string Name, Func<Task> Run)> GetParallelParsingTests() =>
    [
        ("ParallelParsingSettingsIsolation", ParallelParsingSettingsIsolationAsync),
        ("ParallelParsingRejectsDuplicateChat", ParallelParsingRejectsDuplicateChatAsync),
        ("ParallelParsingSavesSeparateChats", ParallelParsingSavesSeparateChatsAsync),
        ("ParallelParsingPauseAndStopAreIndependent", ParallelParsingPauseAndStopAreIndependentAsync),
        ("ParallelTelegramCallsOverlap", ParallelTelegramCallsOverlapAsync),
        ("ParallelFloodWaitSurvivesOneCancellation", ParallelFloodWaitSurvivesOneCancellationAsync),
        ("ParallelParsingGlobalStopCompletesAll", ParallelParsingGlobalStopCompletesAllAsync),
        ("ParallelParsingShutdownWaitsForCleanup", ParallelParsingShutdownWaitsForCleanupAsync),
        ("ParallelDashboardKeepsSelectedChat", ParallelDashboardKeepsSelectedChatAsync),
        ("ParallelParsingConcurrentWrites", ParallelParsingConcurrentWritesAsync),
        ("ConsoleParsingDoesNotSaveStaleDraft", ConsoleParsingDoesNotSaveStaleDraftAsync),
        ("ChatEditingUsesSelectedParsingState", ChatEditingUsesSelectedParsingStateAsync),
        ("ChatEditsPersistWhileOtherChatParses", ChatEditsPersistWhileOtherChatParsesAsync),
    ];

    // Поля, переключатели и сохранение зависят только от парсинга открытого чата
    private static async Task ChatEditingUsesSelectedParsingStateAsync()
    {
        var uid = Guid.NewGuid();
        _state.IsOnlineProcessing = true;
        _state.NetworkActivity.Start(uid, TgNetworkOperationKind.Parsing);
        _state.NetworkActivity.SetChat(uid, _source.Id, _source.Title);
        _state.NetworkActivity.RegisterCancellation(uid, () => { });
        var other = new TgEfSourceDto { Id = 200, Uid = Guid.NewGuid() };
        await Download.OnNavigatedToAsync(new() { Parameter = other });
        Assert(!Download.HasRunningParsing, "Другой чат ошибочно отмечен работающим");
        var page = ReadMarkup("TgChatDownloadPage.xaml");
        var controls = page.Descendants().Where(x => x.Name.LocalName is "TextBox" or "NumberBox" or "ToggleSwitch" or "Button")
            .Where(x => x.Attributes().Any(a => a.Name.LocalName is "IsReadOnly" or "IsEnabled"
                && a.Value.Contains("ViewModel.IsOnlineProcessing", StringComparison.Ordinal))).ToArray();
        Assert(controls.Length == 0, "Редактирование другого чата блокируется общей сетевой активностью");
        var directory = page.Descendants().Single(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "ChatDirectory");
        Assert(((string?)directory.Attribute("IsReadOnly"))?.Contains("ViewModel.HasRunningParsing") == true,
            "Каталог не учитывает парсинг выбранного чата");
        foreach (var control in page.Descendants().Where(x => x.Name.LocalName is "NumberBox" or "ToggleSwitch"))
            Assert(((string?)control.Attribute("IsEnabled"))?.Contains("ViewModel.HasRunningParsing") == true,
                "Параметр не защищён во время парсинга выбранного чата");
        foreach (var command in new[] { "SaveChatSettingsCommand", "ResetChatSettingsCommand" })
        {
            var button = page.Descendants().Single(x => ((string?)x.Attribute("Command"))?.Contains(command) == true);
            Assert(((string?)button.Attribute("IsEnabled"))?.Contains("ViewModel.HasRunningParsing") == true,
                "Действие настроек не связано с парсингом выбранного чата");
        }
        await Download.OnNavigatedToAsync(new() { Parameter = _source });
        Assert(Download.HasRunningParsing, "Работающий чат разрешил редактирование");
        _state.NetworkActivity.ToggleParsingPause(_source.Id); Download.RefreshActivity();
        Assert(Download.HasRunningParsing, "Пауза разрешила менять настройки выполняющегося парсинга");
        _state.NetworkActivity.CancelParsing(_source.Id); Download.RefreshActivity();
        Assert(Download.HasRunningParsing, "Остановка разрешила редактирование до завершения задач");
        _state.NetworkActivity.Finish(uid, TgNetworkOperationState.Canceled); Download.RefreshActivity();
        Assert(!Download.HasRunningParsing, "После завершения настройки не разблокированы");
    }

    // Сохранение другого чата не использует занятый контекст и не меняет источник парсинга
    private static async Task ChatEditsPersistWhileOtherChatParsesAsync()
    {
        var other = new TgEfSourceDto { Id = 200, Uid = Guid.NewGuid(), Directory = "old-directory", IsSaveMessages = true };
        using var sources = new TgEfSourceRepository();
        await sources.SaveAsync(other);
        _state.IsOnlineProcessing = true;
        var uid = Guid.NewGuid();
        _state.NetworkActivity.Start(uid, TgNetworkOperationKind.Parsing);
        _state.NetworkActivity.SetChat(uid, _source.Id, _source.Title);
        await Chat.OnNavigatedToAsync(new() { Parameter = other.Uid });
        await Download.OnNavigatedToAsync(new() { Parameter = Chat.Dto });
        Download.DownloadDto.Directory = "edited-directory"; Download.DownloadDto.FirstId = 73; Download.DownloadDto.CountThreads = 3;
        Download.DownloadDto.IsAutoUpdate = true; Download.DownloadDto.IsSaveFiles = true;
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        DelayedQueryInterceptor.HoldNext = 1; DelayedQueryInterceptor.Release = release.Task;
        var occupied = App.BusinessLogicManager.StorageManager.SourceRepository.GetCountAsync();
        try
        {
            await Chat.SaveChatSettingsCommand.ExecuteAsync(null).WaitAsync(TimeSpan.FromSeconds(3));
            var saved = await sources.GetDtoAsync(x => x.Id == 200);
            Assert(Chat.Errors.Count == 0 && saved.Directory == "edited-directory" && saved.FirstId == 73 && saved.CountThreads == 3
                && saved.IsAutoUpdate && saved.IsSaveFiles, "Настройки другого чата не сохранены во время парсинга");
            var parsingSource = await sources.GetDtoAsync(x => x.Id == _source.Id);
            Assert(parsingSource.Directory == _source.Directory && parsingSource.FirstId == _source.FirstId
                && _state.NetworkActivity.Operations.Single().State == TgNetworkOperationState.Running,
                "Сохранение другого чата изменило настройки или состояние работающего");
        }
        finally { release.TrySetResult(); await occupied; DelayedQueryInterceptor.Release = null; DelayedQueryInterceptor.HoldNext = 0; }
    }

    // Console не должен перезаписывать сохранённую парсером позицию устаревшим снимком ввода
    private static Task ConsoleParsingDoesNotSaveStaleDraftAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("ConsoleDownload.cs")!;
        using var reader = new StreamReader(stream);
        var source = reader.ReadToEnd();
        var start = source.IndexOf("private async Task DownloadManualAsync", StringComparison.Ordinal);
        var end = source.IndexOf("private async Task MarkHistoryReadCoreAsync", start, StringComparison.Ordinal);
        var method = source[start..end];
        var parsing = method.IndexOf("await BusinessLogicManager.ConnectClient.ParseChatAsync", StringComparison.Ordinal);
        Assert(parsing > 0 && method.LastIndexOf("await DownloadSettingsSaveAsync(tgDownloadSettings)", StringComparison.Ordinal) < parsing,
            "После парсинга Console сохраняет устаревший черновик поверх позиции и сведений парсера");
        return Task.CompletedTask;
    }

    // Три одновременно освобождённые порции записывают настоящие документы и сообщения в одну SQLite
    private static async Task ParallelParsingConcurrentWritesAsync()
    {
        using var files = new MediaFixture((cache, state) => new ParallelParsingConnection(cache, state));
        var client = (ParallelParsingConnection)files.Connection;
        var ids = new long[] { 101, 202, 303 };
        var tasks = ids.Select(id => client.ParseChatAsync(CreateParallelSettings(id, files.Directory))).ToArray();
        try
        {
            await Task.WhenAll(ids.Select(client.Prepared)).WaitAsync(TimeSpan.FromSeconds(5));
            foreach (var id in ids) client.Release(id);
            await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(5));
            using var messages = new TgEfMessageRepository();
            using var documents = new TgEfDocumentRepository();
            foreach (var id in ids)
            {
                Assert(await messages.GetCountAsync(x => x.SourceId == id && x.Id == 1) == 1
                    && await documents.GetCountAsync(x => x.SourceId == id && x.MessageId == 1) == 1,
                    "Одновременная запись потеряла сообщение или документ чата");
                Assert(File.ReadAllBytes(Path.Combine(files.Directory, $"{id}", "sample.bin")).Length == 4, "Потерян файл параллельного чата");
            }
            Assert(files.State.NetworkActivity.Operations.All(x => x.State == TgNetworkOperationState.Completed) && client.Downloads == 3,
                "Одновременная запись завершилась ошибкой или потеряла загрузку");
        }
        finally { foreach (var id in ids) client.Release(id); await Task.WhenAll(tasks); }
    }

    // Закрытие приложения отменяет все запуски и дожидается освобождения каждого контекста
    private static async Task ParallelParsingGlobalStopCompletesAllAsync()
    {
        using var files = new MediaFixture((cache, state) => new ParallelParsingConnection(cache, state));
        var client = (ParallelParsingConnection)files.Connection;
        var first = client.ParseChatAsync(CreateParallelSettings(101, files.Directory));
        var second = client.ParseChatAsync(CreateParallelSettings(202, files.Directory));
        try
        {
            await Task.WhenAll(client.Prepared(101), client.Prepared(202)).WaitAsync(TimeSpan.FromSeconds(5));
            files.State.StopHardDownloadProcessing();
            try { await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(5)); } catch (OperationCanceledException) { }
            Assert(first.IsCanceled && second.IsCanceled && !files.State.IsDownloadProcessing
                && files.State.NetworkActivity.Operations.All(x => x.State == TgNetworkOperationState.Canceled),
                "Общая остановка оставила незавершённый парсинг");
        }
        finally
        {
            client.Release(101); client.Release(202);
            try { await Task.WhenAll(first, second); } catch (OperationCanceledException) { }
        }
    }

    // Закрытие должно дождаться завершающей обработки обоих чатов, включая приостановленный
    private static async Task ParallelParsingShutdownWaitsForCleanupAsync()
    {
        using var files = new MediaFixture((cache, state) => new ShutdownParsingConnection(cache, state));
        var client = (ShutdownParsingConnection)files.Connection;
        var first = client.ParseChatAsync(CreateParallelSettings(101, files.Directory));
        var second = client.ParseChatAsync(CreateParallelSettings(202, files.Directory));
        Task closing = Task.CompletedTask;
        try
        {
            await Task.WhenAll(client.Entered[101].Task, client.Entered[202].Task).WaitAsync(TimeSpan.FromSeconds(5));
            files.State.NetworkActivity.ToggleParsingPause(202);
            var method = client.GetType().GetMethod("StopParsingAsync");
            closing = method is null ? Task.Run(client.Dispose) : (Task)method.Invoke(client, [CancellationToken.None])!;
            await Task.WhenAll(client.Cancelled[101].Task, client.Cancelled[202].Task).WaitAsync(TimeSpan.FromSeconds(5));
            await Task.Delay(100);
            Assert(!closing.IsCompleted, "Завершение приложения не дожидается завершающей обработки всех чатов");
            client.Finish.TrySetResult();
            await closing.WaitAsync(TimeSpan.FromSeconds(5));
            try { await Task.WhenAll(first, second); } catch (OperationCanceledException) { }
            Assert(first.IsCanceled && second.IsCanceled && !files.State.IsDownloadProcessing
                && files.State.NetworkActivity.Operations.All(x => x.State == TgNetworkOperationState.Canceled),
                "Закрытие оставило незавершённый чат");
            var newParse = client.ParseChatAsync(CreateParallelSettings(303, files.Directory));
            await newParse.WaitAsync(TimeSpan.FromSeconds(2));
            Assert(client.PreparedCount == 2, "После начала закрытия запущен ещё один чат");
        }
        finally
        {
            client.Finish.TrySetResult(); files.State.StopHardDownloadProcessing();
            try { await Task.WhenAll(first, second, closing); } catch (OperationCanceledException) { }
        }
    }

    // Завершение старого запуска не меняет открытый пульт нового чата и его черновик
    private static async Task ParallelDashboardKeepsSelectedChatAsync()
    {
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        var other = new TgEfSourceDto { Id = 200, Uid = Guid.NewGuid(), Directory = "other-media", IsSaveMessages = true };
        await App.BusinessLogicManager.StorageManager.SourceRepository.SaveAsync(other);
        App.BusinessLogicManager.ConnectClient.Ready = true;
        var entered = new Dictionary<long, TaskCompletionSource> { [100] = new(), [200] = new() };
        var released = new Dictionary<long, TaskCompletionSource> { [100] = new(), [200] = new() };
        var settingsByChat = new Dictionary<long, TgDownloadSettingsViewModel>();
        App.BusinessLogicManager.ConnectClient.Parse = async settings =>
        {
            var id = settings.SourceVm.Dto.Id; settingsByChat[id] = settings;
            entered[id].TrySetResult(); await released[id].Task;
        };
        var first = Chat.StartUpdateOnlineCommand.ExecuteAsync(null);
        Task second = Task.CompletedTask;
        try
        {
            await entered[100].Task.WaitAsync(TimeSpan.FromSeconds(3));
            await Chat.OnNavigatedToAsync(new() { Parameter = other.Uid });
            second = Chat.StartUpdateOnlineCommand.ExecuteAsync(null);
            await entered[200].Task.WaitAsync(TimeSpan.FromSeconds(3));
            Chat.Dto.Directory = "unsaved-new-directory";
            released[100].TrySetResult(); await first.WaitAsync(TimeSpan.FromSeconds(3));
            Assert(Chat.Dto.Id == 200 && Chat.Dto.Directory == "unsaved-new-directory" && !second.IsCompleted
                && settingsByChat[100].SourceVm.Dto.Id == 100 && settingsByChat[200].SourceVm.Dto.Directory == "other-media",
                "Завершение первого чата сбросило пульт второго или черновик изменил активный запуск");
        }
        finally { released[100].TrySetResult(); released[200].TrySetResult(); await Task.WhenAll(first, second); }
        Assert(Chat.Errors.Count == 0, "Параллельный запуск пульта завершился ошибкой");
    }

    // Долгая передача одного чата не удерживает сетевой запрос другого
    private static async Task ParallelTelegramCallsOverlapAsync()
    {
        var service = new TgFloodControlService(); GC.SuppressFinalize(service);
        var firstEntered = new TaskCompletionSource(); var secondEntered = new TaskCompletionSource(); var release = new TaskCompletionSource();
        var first = service.ExecuteWithFloodControlAsync(async _ => { firstEntered.TrySetResult(); await release.Task; return 1; },
            isThrow: true, CancellationToken.None);
        await firstEntered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var second = service.ExecuteWithFloodControlAsync(_ => { secondEntered.TrySetResult(); return Task.FromResult(2); },
            isThrow: true, CancellationToken.None);
        try
        {
            await secondEntered.Task.WaitAsync(TimeSpan.FromMilliseconds(500));
            Assert(await second == 2 && !first.IsCompleted, "Второй запрос не завершился независимо от первого");
        }
        finally { release.TrySetResult(); await Task.WhenAll(first, second); }
    }

    // Отмена ожидающего чата не отменяет ограничение Telegram для остальных чатов
    private static async Task ParallelFloodWaitSurvivesOneCancellationAsync()
    {
        var service = new TgFloodControlService(); GC.SuppressFinalize(service);
        using var stop = new CancellationTokenSource();
        var flood = service.WaitIfFloodAsync("FLOOD_WAIT_0", stop.Token);
        var calls = 0;
        var pending = service.ExecuteWithFloodControlAsync(_ => { calls++; return Task.FromResult(42); }, isThrow: true, CancellationToken.None);
        stop.Cancel();
        await flood;
        await Task.Delay(100);
        Assert(calls == 0 && !pending.IsCompleted, "Отмена первого чата сняла общую защиту FLOOD_WAIT");
        Assert(await pending.WaitAsync(TimeSpan.FromSeconds(3)) == 42 && calls == 1, "После общей паузы запрос не возобновился");
    }

    // Настоящие обработчики сообщений, файлов и буферов сохраняют результаты только своего чата
    private static async Task ParallelParsingSavesSeparateChatsAsync()
    {
        using var files = new MediaFixture((cache, state) => new ParallelParsingConnection(cache, state));
        var client = (ParallelParsingConnection)files.Connection;
        var first = CreateParallelSettings(101, files.Directory); var second = CreateParallelSettings(202, files.Directory);
        var a = client.ParseChatAsync(first); var b = client.ParseChatAsync(second);
        try
        {
            await Task.WhenAll(client.Prepared(101), client.Prepared(202)).WaitAsync(TimeSpan.FromSeconds(5));
            Assert(!ReferenceEquals(client.Contexts[101], client.Contexts[202])
                && !ReferenceEquals(client.Buffers[101], client.Buffers[202]), "Параллельные чаты используют общий контекст БД или буфер");
            client.Release(101);
            await a.WaitAsync(TimeSpan.FromSeconds(5));
            Assert(!b.IsCompleted && files.State.IsDownloadProcessing, "Завершение первого чата завершило второй");
            using var reader = new TgEfMessageRepository();
            Assert(await reader.GetCountAsync(x => x.SourceId == 101) == 1 && await reader.GetCountAsync(x => x.SourceId == 202) == 0,
                "Первый чат потерял сообщение или записал чужой буфер");
            client.Release(202);
            await b.WaitAsync(TimeSpan.FromSeconds(5));
            Assert(await reader.GetCountAsync(x => x.SourceId == 202) == 1, "Второй чат потерял сообщение после завершения первого");
            using var sources = new TgEfSourceRepository();
            foreach (var settings in new[] { first, second })
            {
                var source = await sources.GetDtoAsync(x => x.Id == settings.SourceVm.Dto.Id);
                Assert(source.FirstId == 2 && source.Directory == settings.SourceVm.Dto.Directory, "Позиция или каталог записаны в другой чат");
                Assert(File.ReadAllBytes(Path.Combine(source.Directory, "sample.bin")).SequenceEqual(new byte[] { 1, 2, 3, 4 }),
                    "Файл скачан в другой каталог или не записан");
            }
            Assert(files.NetworkCalls == 0 && client.Downloads == 2 && !files.State.IsDownloadProcessing,
                "Проверка обратилась к сети, потеряла передачу или не завершила состояние");
        }
        finally { client.Release(101); client.Release(202); await Task.WhenAll(a, b); }
    }

    // Пауза одного чата позволяет другому закончить; остановленный чат можно запустить снова
    private static async Task ParallelParsingPauseAndStopAreIndependentAsync()
    {
        using var files = new MediaFixture((cache, state) => new ParallelParsingConnection(cache, state));
        var client = (ParallelParsingConnection)files.Connection;
        var first = CreateParallelSettings(101, files.Directory); var second = CreateParallelSettings(202, files.Directory);
        var a = client.ParseChatAsync(first); var b = client.ParseChatAsync(second);
        try
        {
            await Task.WhenAll(client.Prepared(101), client.Prepared(202)).WaitAsync(TimeSpan.FromSeconds(5));
            files.State.NetworkActivity.ToggleParsingPause(101);
            client.Release(101); client.Release(202);
            await b.WaitAsync(TimeSpan.FromSeconds(5));
            Assert(!a.IsCompleted && !File.Exists(Path.Combine(first.SourceVm.Dto.Directory, "sample.bin")), "Приостановленный чат продолжил скачивание");
            files.State.NetworkActivity.CancelParsing(101);
            try { await a.WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (OperationCanceledException) { }
            var operations = files.State.NetworkActivity.Operations.Where(x => x.Kind == TgNetworkOperationKind.Parsing).ToArray();
            Assert(operations.Single(x => x.ChatId == 101).State == TgNetworkOperationState.Canceled
                && operations.Single(x => x.ChatId == 202).State == TgNetworkOperationState.Completed
                && !files.State.DownloadToken.IsCancellationRequested, "Остановка одного чата затронула второй или общий токен");
            await client.ParseChatAsync(first).WaitAsync(TimeSpan.FromSeconds(5));
            Assert(File.Exists(Path.Combine(first.SourceVm.Dto.Directory, "sample.bin")) && client.Downloads == 2,
                "Остановленный чат не удалось запустить повторно");
        }
        finally
        {
            client.Release(101); client.Release(202); files.State.NetworkActivity.CancelParsing();
            try { await Task.WhenAll(a, b); } catch (OperationCanceledException) { }
        }
    }

    // Фиксируем одинаковые ID сообщений и имена файлов в разных чатах для проверки изоляции
    private static TgDownloadSettingsViewModel CreateParallelSettings(long chatId, string directory)
    {
        var settings = new TgDownloadSettingsViewModel();
        settings.SourceVm.Dto = new() { Id = chatId, Uid = Guid.NewGuid(), Title = $"Chat {chatId}", UserName = $"chat_{chatId}",
            Directory = Path.Combine(directory, $"{chatId}"),
            FirstId = 1, Count = 2, IsSaveMessages = true, IsSaveFiles = true, IsDownloadThumbnail = false, IsJoinFileNameWithMessageId = false };
        Directory.CreateDirectory(settings.SourceVm.Dto.Directory);
        settings.Chat.Base = new TL.Channel { id = chatId, access_hash = 123 };
        return settings;
    }

    // Перекрывающиеся запуски сохраняют свои настройки даже после завершения соседнего чата
    private static async Task ParallelParsingSettingsIsolationAsync()
    {
        using var files = new MediaFixture(); files.Connection.Client = null;
        var first = new TgDownloadSettingsViewModel(); first.SourceVm.Dto.Id = 101;
        var second = new TgDownloadSettingsViewModel(); second.SourceVm.Dto.Id = 202;
        var entered = new TaskCompletionSource(); var release = new TaskCompletionSource();
        var visits = 0;
        TgDownloadSettingsViewModel? before = null, after = null;
        files.Connection.SetupUpdateTasksViewModel(async _ =>
        {
            if (Interlocked.Increment(ref visits) != 1) return;
            before = ReadParsingSettings(files.Connection);
            entered.TrySetResult();
            await release.Task;
            after = ReadParsingSettings(files.Connection);
        });
        var firstRun = files.Connection.ParseChatAsync(first);
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
            first.SourceVm.Dto.Id = 999;
            await files.Connection.ParseChatAsync(second).WaitAsync(TimeSpan.FromSeconds(3));
            Assert(files.State.IsDownloadProcessing, "Завершение второго чата сбросило активность первого");
        }
        finally { release.TrySetResult(); await firstRun; }
        Assert(before?.SourceVm.Dto.Id == 101 && after?.SourceVm.Dto.Id == 101,
            "Настройки работающего чата изменены черновиком или завершением другого парсинга");
        Assert(!files.State.IsDownloadProcessing && visits == 2, "Не завершились оба независимых запуска");
    }

    // Один чат не должен одновременно записываться двумя парсерами
    private static async Task ParallelParsingRejectsDuplicateChatAsync()
    {
        using var files = new MediaFixture(); files.Connection.Client = null; files.Settings.SourceVm.Dto.Id = 101;
        var entered = new TaskCompletionSource(); var release = new TaskCompletionSource(); var visits = 0;
        files.Connection.SetupUpdateTasksViewModel(async _ =>
        {
            if (Interlocked.Increment(ref visits) == 1) { entered.TrySetResult(); await release.Task; }
        });
        var first = files.Connection.ParseChatAsync(files.Settings);
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
            await files.Connection.ParseChatAsync(files.Settings).WaitAsync(TimeSpan.FromSeconds(3));
            Assert(visits == 1 && files.State.NetworkActivity.Operations.Count == 1, "Один чат запущен повторно до завершения первого прохода");
        }
        finally { release.TrySetResult(); await first; }
    }

    // Читаем текущие настройки исполнения, сохраняя проверку старого общего поля до исправления
    private static TgDownloadSettingsViewModel? ReadParsingSettings(TgConnectClientBase connection)
    {
        var type = typeof(TgConnectClientBase);
        return (TgDownloadSettingsViewModel?)(type.GetProperty("DownloadSettings", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(connection)
            ?? type.GetField("_downloadSettings", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(connection));
    }
}

// Подменяем получение порции Telegram, сохраняя настоящий цикл разбора и запись файлов и сообщений
internal sealed class ParallelParsingConnection(FusionCache cache, LoadStateService state) : MediaTestConnection(cache, state)
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<long, TaskCompletionSource> _entered = new();
    private readonly System.Collections.Concurrent.ConcurrentDictionary<long, TaskCompletionSource> _release = new();
    public System.Collections.Concurrent.ConcurrentDictionary<long, object> Contexts { get; } = new();
    public System.Collections.Concurrent.ConcurrentDictionary<long, object> Buffers { get; } = new();

    /// <summary> Ожидает подготовки отдельного контекста чата </summary>
    public Task Prepared(long chatId) => _entered.GetOrAdd(chatId, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).Task;

    /// <summary> Разрешает обработку подготовленной порции </summary>
    public void Release(long chatId) => _release.GetOrAdd(chatId, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).TrySetResult();

    /// <summary> Готовит ответ в кеше текущего запуска и удерживает оба парсера до проверки </summary>
    protected override async Task PrepareChatAsync(TgDownloadSettingsViewModel settings, CancellationToken ct)
    {
        var id = settings.SourceVm.Dto.Id;
        var type = typeof(TgConnectClientBase);
        var ownCache = (IFusionCache)type.GetProperty("Cache", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(this)!;
        Contexts[id] = StorageManager.EfContext;
        Buffers[id] = type.GetProperty("MessageBuffer", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(this)!;
        await StorageManager.SourceRepository.SaveAsync(settings.SourceVm.Dto, ct: ct);
        var message = new TL.Message { id = 1, peer_id = new TL.PeerChannel { channel_id = id }, date = DateTime.UtcNow, message = $"Chat {id}",
            flags = TL.Message.Flags.has_media,
            media = new TL.MessageMediaDocument { flags = TL.MessageMediaDocument.Flags.has_document,
                document = new TL.Document { id = id, size = 4, date = DateTime.UtcNow, mime_type = "application/octet-stream",
                attributes = [new TL.DocumentAttributeFilename { file_name = "sample.bin" }] } } };
        await ownCache.SetAsync<TL.Messages_MessagesBase>(TgBusinessLogic.Helpers.TgCacheUtils.GetCacheKeyMessages(id, 1, 2),
            new TL.Messages_Messages { messages = [message] }, token: ct);
        _entered.GetOrAdd(id, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).TrySetResult();
        await _release.GetOrAdd(id, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).Task.WaitAsync(ct);
    }

    /// <summary> Возвращает границы тестовой порции без авторизации Telegram </summary>
    protected override Task<TgParseHelper> CheckAccessToMessagesAsync(TgDownloadSettingsViewModel settings, CancellationToken ct) =>
        Task.FromResult(new TgParseHelper { IsAccess = true, SourceFirstId = 1, SourceLastId = 2,
            Channel = settings.Chat.Base as TL.Channel, AppDto = new() { UseClient = true } });
}

// Удерживаем настоящие задачи парсинга после отмены, имитируя завершающую запись или освобождение файла
internal sealed class ShutdownParsingConnection(FusionCache cache, LoadStateService state) : MediaTestConnection(cache, state)
{
    public Dictionary<long, TaskCompletionSource> Entered { get; } = new() { [101] = new(), [202] = new() };
    public Dictionary<long, TaskCompletionSource> Cancelled { get; } = new() { [101] = new(), [202] = new() };
    public TaskCompletionSource Finish { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _preparedCount;
    public int PreparedCount => Volatile.Read(ref _preparedCount);

    /// <summary> Воспроизводит медленное завершение работы после отмены токена </summary>
    protected override async Task PrepareChatAsync(TgDownloadSettingsViewModel settings, CancellationToken ct)
    {
        Interlocked.Increment(ref _preparedCount);
        Entered[settings.SourceVm.Dto.Id].TrySetResult();
        try { await Task.Delay(Timeout.Infinite, ct); }
        finally { Cancelled[settings.SourceVm.Dto.Id].TrySetResult(); await Finish.Task; }
    }
}
