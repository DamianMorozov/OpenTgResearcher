namespace OpenTgResearcherDesktop.UpdateSelfTests;

// Проверяем компактную панель на настоящем учёте операций без Telegram и пользовательской БД
internal static partial class Program
{
    // Четыре кнопки открывают подробности, а длинные подписи скрываются в компактном окне
    private static Task StatusBarLayoutAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgStatusBar.xaml")!;
        var root = XDocument.Load(stream);
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var bar = root.Descendants().SingleOrDefault(e => (string?)e.Attribute(x + "Name") == "StatusBar");
        Assert(bar is not null, "Нижняя панель «Тихая строка» отсутствует");
        foreach (var name in new[] { "StatusTelegramButton", "StatusStorageButton", "StatusActivityButton", "StatusFloodButton" })
        {
            var button = bar!.Descendants().SingleOrDefault(e => (string?)e.Attribute(x + "Name") == name);
            Assert(button?.Descendants().Any(e => e.Name.LocalName == "Flyout") == true, $"Нет подробностей кнопки {name}");
            Assert(button!.Attribute("AutomationProperties.AutomationId") is not null, $"Кнопка {name} недоступна для проверки окна");
        }
        Assert(bar!.Descendants().Any(e => e.Name.LocalName == "AdaptiveTrigger"), "Панель не адаптируется к узкому окну");
        Assert(bar.Descendants().Any(e => e.Name.LocalName == "ItemsControl" && e.Attribute("ItemsSource")?.Value.Contains("StatusChatGroups") == true),
            "Подробности не связаны с активностью чатов");
        Assert(root.Descendants().Where(e => e.Name.LocalName == "ScrollViewer")
            .All(e => (string?)e.Attribute("HorizontalScrollBarVisibility") == "Disabled"), "Подробности допускают лишнюю горизонтальную прокрутку");
        return Task.CompletedTask;
    }

    // Счётчик учитывает чаты один раз, сохраняет прогресс файлов и удаляет завершённые операции
    private static Task StatusBarActivityAsync()
    {
        var vm = new ShellViewModel();
        var activity = vm.LoadStateService.NetworkActivity;
        var parsing = StartStatusOperation(activity, TgNetworkOperationKind.Parsing, 100);
        var file = StartStatusOperation(activity, TgNetworkOperationKind.Download, 100);
        var second = StartStatusOperation(activity, TgNetworkOperationKind.Parsing, 200);
        activity.ReportTransfer(file, "clip.mp4", 25, 100);
        RefreshStatus(vm);
        Assert(Read<string>(vm, "StatusActivityCount") == "Чаты: 2", "Парсинг и файл одного чата посчитаны как разные чаты");
        var groups = Read<ObservableCollection<TgNetworkChatGroupModel>>(vm, "StatusChatGroups");
        Assert(groups.Count == 2 && groups.Single(g => g.ChatId == 100).Operations.Count == 2, "Потеряны параллельные чаты или загрузка");
        Assert(groups.SelectMany(g => g.Operations).Any(o => o.Detail == "clip.mp4" && o.Progress == 25), "Потерян прогресс файла");
        var group = groups.Single(g => g.ChatId == 100); group.IsExpanded = true;
        RefreshStatus(vm);
        Assert(ReferenceEquals(group, groups.Single(g => g.ChatId == 100)) && group.IsExpanded, "Обновление сбросило раскрытие чата");
        activity.Finish(file, TgNetworkOperationState.Completed); activity.Finish(second, TgNetworkOperationState.Canceled);
        RefreshStatus(vm);
        Assert(groups.Count == 1 && groups[0].Operations.Count == 1 && Read<string>(vm, "StatusActivityCount") == "Чаты: 1",
            "Завершённые действия остались в нижней панели");
        activity.Finish(parsing, TgNetworkOperationState.Completed); RefreshStatus(vm);
        Assert(groups.Count == 0 && Read<bool>(vm, "IsStatusActivityEmpty") && Read<string>(vm, "StatusActivityText") == "Нет операций",
            "После завершения панель продолжает показывать парсинг");
        var general = Guid.NewGuid(); activity.Start(general, TgNetworkOperationKind.Updates); RefreshStatus(vm);
        Assert(Read<string>(vm, "StatusActivityCount") == "Операции: 1" && groups.Single().ChatId is null,
            "Общая операция показана как ноль активных чатов");
        return Task.CompletedTask;
    }

    // Готовность Telegram не зависит от занятости, ожидание отображается понятным текстом
    private static async Task StatusBarStatesAsync()
    {
        var vm = new ShellViewModel(); var state = vm.LoadStateService;
        RefreshStatus(vm);
        Assert(Read<string>(vm, "StatusTelegramText") == "Нет подключения", "Отключённый клиент показан подключённым");
        state.SetIsOnlineReady(isReady: true); RefreshStatus(vm);
        Assert(Read<string>(vm, "StatusTelegramText") == "Telegram", "Готовность соединения потеряна без сетевых операций");
        var storageUid = Guid.NewGuid(); await state.PrepareStorageTokenAsync(storageUid); RefreshStatus(vm);
        Assert(Read<string>(vm, "StatusStorageText") == "База занята", "Занятая база показана готовой");
        state.StopSoftStorageProcessing(storageUid); RefreshStatus(vm);
        Assert(Read<string>(vm, "StatusStorageText") == "База готова", "Завершённая операция оставила базу занятой");
        vm.IsFloodVisible = true; vm.FloodWaitSeconds = 24; RefreshStatus(vm);
        Assert(Read<string>(vm, "StatusFloodText") == "Ожидание · 24 с" && Read<string>(vm, "StatusFloodDetail").Contains("автоматически"),
            "Нет понятного сообщения об ожидании Telegram");
        vm.IsFloodVisible = false; RefreshStatus(vm);
        Assert(Read<string>(vm, "StatusFloodText") == "Без ограничений", "Истёкшее ожидание осталось на панели");
        vm.SettingsService.IsExistsAppStorage = false; RefreshStatus(vm);
        Assert(Read<string>(vm, "StatusStorageText") == "База не выбрана", "Отсутствующая база показана готовой");
    }

    // Названия скрываются сразу, а смена языка переводит и статусы, и подробности
    private static Task StatusBarPrivacyAndLanguageAsync()
    {
        var vm = new ShellViewModel(); var state = vm.LoadStateService;
        state.SetIsDisplaySensitiveData(isOn: true);
        var uid = StartStatusOperation(state.NetworkActivity, TgNetworkOperationKind.Download, 100);
        state.NetworkActivity.ReportTransfer(uid, "private.mp4", 10, 100); RefreshStatus(vm);
        var groups = Read<ObservableCollection<TgNetworkChatGroupModel>>(vm, "StatusChatGroups");
        Assert(groups[0].Title == "Чат 100" && groups[0].Operations[0].Detail == "private.mp4", "Нет подробностей выбранного чата");
        state.SetIsDisplaySensitiveData(isOn: false); RefreshStatus(vm);
        Assert(groups[0].Title == state.SensitiveData && !groups[0].Operations[0].Caption.Contains("private.mp4"), "Панель раскрывает скрытые данные");
        Localizer.Language = "en-US"; RefreshStatus(vm);
        Assert(Read<string>(vm, "StatusActivityText") == "Downloading" && Read<string>(vm, "StatusActivityCount") == "Chats: 1"
            && Read<string>(vm, "StatusTelegramText") == "Disconnected", "Динамические статусы не переведены");
        Assert(Read<string>(vm, "StatusFloodDetail").Contains("No active"), "Подробности ограничений не переведены");
        return Task.CompletedTask;
    }

    // Переход к полной сетевой активности не подключает Telegram и не меняет операции
    private static Task StatusBarNavigationAsync()
    {
        var vm = new ShellViewModel();
        Read<IRelayCommand>(vm, "OpenStatusActivityCommand").Execute(null);
        Assert(vm.NavigationService.LastPage == typeof(TgNetworkActivityViewModel).FullName && vm.NavigationService.Navigations == 1,
            "Кнопка нижней панели не открыла сетевую активность");
        Assert(vm.LoadStateService.NetworkActivity.Operations.Count == 0, "Переход создал сетевую операцию");
        return Task.CompletedTask;
    }

    // Вызываем исходный метод через отражение, чтобы до реализации тест падал по причине отсутствия поведения
    private static void RefreshStatus(ShellViewModel vm)
    {
        var method = typeof(ShellViewModel).GetMethod("RefreshStatus");
        Assert(method is not null, "Обновление нижней панели ещё не реализовано");
        method!.Invoke(vm, null);
    }

    // Регистрируем настоящую операцию с независимым идентификатором и контекстом чата
    private static Guid StartStatusOperation(TgNetworkActivityService activity, TgNetworkOperationKind kind, long chatId)
    {
        var uid = Guid.NewGuid(); activity.Start(uid, kind); activity.SetChat(uid, chatId, $"Чат {chatId}"); return uid;
    }
}
