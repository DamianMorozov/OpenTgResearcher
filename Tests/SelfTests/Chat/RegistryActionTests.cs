namespace OpenTgResearcherDesktop.ChatSelfTests;

// Проверяем команды реестра и сохранение списка чатов независимо от настройки парсинга сообщений
internal static partial class Program
{
    private static IEnumerable<(string Name, Func<Task> Run)> GetRegistryActionTests() =>
    [
        ("RegistryDefaultNameSort", RegistryDefaultNameSortAsync),
        ("RegistryStartsWithoutConfirmation", RegistryStartsWithoutConfirmationAsync),
        ("DashboardStartsWithoutConfirmation", DashboardStartsWithoutConfirmationAsync),
        ("RegistryRowMenu", RegistryRowMenuAsync),
        ("RegistryRefreshesWholeList", RegistryRefreshesWholeListAsync),
        ("RegistryScanPersistsWithoutMessageParsing", RegistryScanPersistsWithoutMessageParsingAsync),
        ("RegistryEmptyScopeDoesNotScanAll", RegistryEmptyScopeDoesNotScanAllAsync),
        ("RegistryDisconnectedScanKeepsSubscriptions", RegistryDisconnectedScanKeepsSubscriptionsAsync),
        ("RegistryFailedScanPreservesSubscriptions", RegistryFailedScanPreservesSubscriptionsAsync),
        ("ChatParsingCaptions", ChatParsingCaptionsAsync),
    ];

    // Дополняем отдельную БД синтетического окна второй строкой для проверки цели меню
    private static async Task<int> SeedRegistryMenuAsync(string path)
    {
        try
        {
            await using var context = new TgEfDesktopContext();
            context.Database.SetConnectionString($"Data Source={path};Mode=ReadWrite");
            context.Sources.Add(new() { Uid = Guid.NewGuid(), Id = 200, Title = "ZZ Context menu chat", IsSubscribe = true });
            await context.SaveChangesAsync();
            return 0;
        }
        catch (Exception ex) { TgLogUtils.WriteException(ex); Console.WriteLine(ex.Message); return 1; }
    }

    // Проверяем фактический порядок первой порции, а не только индекс переключателя
    private static async Task RegistryDefaultNameSortAsync()
    {
        using var sources = new TgEfSourceRepository();
        await sources.SaveAsync(new TgEfSourceDto { Id = 200, Uid = Guid.NewGuid(), Title = "Alpha", DtChanged = DateTime.MinValue });
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!);
        await vm.OnNavigatedToAsync(null);
        Assert(vm.RegistrySort == 1 && vm.RegistryRows.Select(x => x.Title).SequenceEqual(new[] { "Alpha", "First" }),
            "Начальная порция не отсортирована по названию");
    }

    // Запуск из реестра сразу передаёт сохранённые настройки парсеру
    private static async Task RegistryStartsWithoutConfirmationAsync()
    {
        _state.IsOnlineReady = true; App.BusinessLogicManager.ConnectClient.Ready = true;
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!);
        await vm.OnNavigatedToAsync(null);
        long parsed = 0;
        App.BusinessLogicManager.ConnectClient.Parse = settings => { parsed = settings.SourceVm.Dto.Id; return Task.CompletedTask; };
        await vm.ToggleSelectedParsingCommand.ExecuteAsync(null);
        Assert(parsed == _source.Id && vm.ConfirmationCount == 0 && vm.Errors.Count == 0, "Запуск из реестра требует подтверждения или не выполнен");
    }

    // Прямой запуск из пульта сохраняет черновик и не показывает диалог
    private static async Task DashboardStartsWithoutConfirmationAsync()
    {
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        App.BusinessLogicManager.ConnectClient.Ready = true;
        Chat.Dto.Directory = "changed-before-parsing"; var parsed = false;
        App.BusinessLogicManager.ConnectClient.Parse = settings =>
        {
            parsed = settings.SourceVm.Dto.Id == _source.Id && settings.SourceVm.Dto.Directory == "changed-before-parsing";
            return Task.CompletedTask;
        };
        await Chat.StartUpdateOnlineCommand.ExecuteAsync(null);
        using var sources = new TgEfSourceRepository();
        var saved = await sources.GetDtoAsync(x => x.Id == _source.Id);
        Assert(parsed && saved.Directory == "changed-before-parsing" && Chat.ConfirmationCount == 0 && Chat.Errors.Count == 0,
            "Пульт не запустил чат без диалога с сохранением настроек");
    }

    // Контекстное меню использует общие команды и динамические подписи выбранной строки
    private static Task RegistryRowMenuAsync()
    {
        var page = ReadMarkup("TgChatsPage.xaml");
        var grid = page.Descendants().Single(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "ChatsRegistry");
        var flyout = grid.Elements().SingleOrDefault(x => x.Name.LocalName.EndsWith(".ContextFlyout"));
        Assert(flyout is not null && (string?)grid.Attribute("RightTapped") == "OnChatRightTapped", "Нет меню строки с выбором чата под указателем");
        var items = flyout!.Descendants().Where(x => x.Name.LocalName == "MenuFlyoutItem").ToArray();
        Assert(items.Length == 2 && ((string?)items[0].Attribute("Command"))?.Contains("ToggleSelectedParsingCommand") == true
            && ((string?)items[0].Attribute("Text"))?.Contains("ParsingCaption") == true
            && ((string?)items[1].Attribute("Command"))?.Contains("PauseSelectedCommand") == true
            && ((string?)items[1].Attribute("Text"))?.Contains("PauseCaption") == true, "Меню не связано с командами и состояниями чата");
        return Task.CompletedTask;
    }

    // Поисковый фильтр не ограничивает сетевую синхронизацию видимой порцией чатов
    private static async Task RegistryRefreshesWholeListAsync()
    {
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!) { FilterText = "First" };
        await vm.OnNavigatedToAsync(null);
        App.BusinessLogicManager.ConnectClient.Ready = true;
        var called = false;
        App.BusinessLogicManager.ConnectClient.SearchSources = async (settings, type, ids) =>
        {
            Assert(type == TgEnumSourceType.Chat && ids is null && !ReferenceEquals(settings, vm.DownloadSettings),
                "Обновление ограничено загруженной порцией или использует общий черновик");
            using var sources = new TgEfSourceRepository();
            await sources.SaveAsync(new TgEfSourceDto { Id = 200, Uid = Guid.NewGuid(), Title = "First remote" });
            called = true;
        };
        var method = typeof(TgChatsViewModel).GetMethod("StartUpdateOnlineCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        await (Task)method.Invoke(vm, null)!;
        Assert(called && vm.RegistryRows.Count == 2 && vm.RegistryRows.Any(x => x.Source.Id == 200) && vm.FilterText == "First",
            "Полученные чаты не появились в реестре с сохранённым фильтром");
    }

    // Настройка сохранения сообщений не должна выключать запись найденных чатов
    private static async Task RegistryScanPersistsWithoutMessageParsingAsync()
    {
        using var storage = new TgStorageService();
        using var files = new MediaFixture((cache, state) => new RegistryScanConnection(storage, cache, state));
        files.Connection.EnumerableSmallGroups.Enqueue(new TL.Chat { id = 200, title = "Remote group" });
        Assert(!files.Settings.SourceVm.Dto.IsSaveMessages, "Нужен сценарий с выключенным сохранением сообщений");
        await files.CallAsync("SearchChatsAsync", files.Settings, new List<long> { 200 }, CancellationToken.None);
        using var sources = new TgEfSourceRepository();
        var saved = await sources.GetDtoAsync(x => x.Id == 200);
        Assert(saved.Id == 200 && saved.Title == "Remote group" && files.NetworkCalls == 0, "Список чатов не сохранён при выключенном парсинге сообщений");
    }

    // Пустой отобранный список не означает сканирование всех известных чатов
    private static async Task RegistryEmptyScopeDoesNotScanAllAsync()
    {
        using var storage = new TgStorageService();
        using var files = new MediaFixture((cache, state) => new RegistryScanConnection(storage, cache, state));
        files.Settings.SourceVm.Dto.IsSaveMessages = true;
        files.Connection.EnumerableSmallGroups.Enqueue(new TL.Chat { id = 200, title = "Outside scope" });
        await files.CallAsync("SearchChatsAsync", files.Settings, new List<long>(), CancellationToken.None);
        using var sources = new TgEfSourceRepository();
        Assert(await sources.GetCountAsync(x => x.Id == 200) == 0, "Пустой список ошибочно обновил посторонний чат");
    }

    // Отказ авторизации не равен успешному ответу с пустым списком чатов
    private static async Task RegistryDisconnectedScanKeepsSubscriptionsAsync()
    {
        using var storage = new TgStorageService();
        _source.IsSubscribe = true; _source.IsUserAccess = true;
        await storage.SourceRepository.SaveAsync(_source);
        using var files = new MediaFixture((cache, state) => new RegistryScanConnection(storage, cache, state));
        await files.Connection.SearchSourcesTgAsync(files.Settings, TgEnumSourceType.Chat);
        using var sources = new TgEfSourceRepository();
        var saved = await sources.GetDtoAsync(x => x.Id == _source.Id);
        Assert(((RegistryScanConnection)files.Connection).LoginCalls == 1 && saved.IsSubscribe && saved.IsUserAccess,
            "Отказ авторизации стёр сохранённые подписку и доступ");
    }

    // Неудачный запрос Telegram не должен стирать сведения о подписках и доступе к чатам
    private static async Task RegistryFailedScanPreservesSubscriptionsAsync()
    {
        using var storage = new TgStorageService();
        _source.IsSubscribe = true; _source.IsUserAccess = true;
        await storage.SourceRepository.SaveAsync(_source);
        using var files = new MediaFixture((cache, state) => new RegistryScanConnection(storage, cache, state));
        typeof(TgConnectClientBase).GetProperty(nameof(TgConnectClientBase.IsReady))!.SetValue(files.Connection, true);
        await files.Connection.SearchSourcesTgAsync(files.Settings, TgEnumSourceType.Chat).WaitAsync(TimeSpan.FromSeconds(8));
        using var sources = new TgEfSourceRepository();
        var saved = await sources.GetDtoAsync(x => x.Id == _source.Id);
        Assert(((RegistryScanConnection)files.Connection).LoginCalls > 1 && saved.IsSubscribe && saved.IsUserAccess, "Неудачное обновление сбросило подписку или доступ к чату");
    }

    // Проверяем точную русскую подпись и перевод обеих кнопок
    private static Task ChatParsingCaptionsAsync()
    {
        foreach (var language in new[] { "ru-RU", "en-US" })
        {
            using var stream = typeof(Program).Assembly.GetManifestResourceStream($"ChatResources.{language}.resw")!;
            var data = XDocument.Load(stream).Descendants("data");
            foreach (var action in new[] { "Start", "Stop" })
            {
                var expected = language == "ru-RU" ? action == "Start" ? "Запустить парсинг чата" : "Остановить парсинг чата"
                    : action == "Start" ? "Start chat parsing" : "Stop chat parsing";
                var value = data.Single(x => (string?)x.Attribute("name") == $"TextBlock{action}ParseTelegram.Text").Element("value")!.Value;
                var chatValue = data.Single(x => (string?)x.Attribute("name") == $"Chat{action}.Text").Element("value")!.Value;
                Assert(chatValue == expected, $"Кнопка пульта {action} не переведена: {chatValue}");
                Assert(value == expected, $"Неверная подпись {action} на языке {language}: {value}");
            }
        }
        return Task.CompletedTask;
    }
}

internal sealed class RegistryScanConnection : MediaTestConnection
{
    public RegistryScanConnection(TgStorageService storage, FusionCache cache, LoadStateService state) : base(cache, state) => StorageManager = storage;

    public int LoginCalls { get; private set; }

    /// <summary> Сохраняет искусственный клиент с запрещённым сетевым транспортом </summary>
    public override Task LoginUserAsync(bool isProxyUpdate) { LoginCalls++; return Task.CompletedTask; }
}
