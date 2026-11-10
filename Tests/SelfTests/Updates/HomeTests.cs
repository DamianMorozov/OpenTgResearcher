namespace OpenTgResearcherDesktop.UpdateSelfTests;

internal static partial class Program
{
    // Основное действие зависит от готовности клиента, остальные переходы работают и без Telegram
    private static async Task HomeNavigationAndConnectionAsync()
    {
        using var state = new LoadStateService();
        var settings = new ITgSettingsService();
        var navigation = new INavigationService();
        using var vm = new TgMainViewModel(state, settings, navigation, null!);
        var profiles = new TgAccountProfiles(settings, "Основной");
        profiles.Profiles[0].Name = "Рабочий профиль";
        profiles.Save(profiles.ActiveProfileId);
        vm.Activate();
        Assert(vm.ProfileName == "Рабочий профиль" && vm.PrimaryActionCaption == "Подключить Telegram", "Не загружены профиль и действие первого входа");
        vm.PrimaryActionCommand.Execute(null);
        Assert(navigation.LastPage == typeof(TgClientConnectionViewModel).FullName, "Нет перехода к подключению без Telegram");
        vm.OpenChatsCommand.Execute(null);
        Assert(navigation.LastPage == typeof(TgChatsViewModel).FullName, "Локальные чаты недоступны без Telegram");
        vm.OpenNetworkCommand.Execute(null);
        Assert(navigation.LastPage == typeof(TgNetworkActivityViewModel).FullName, "Нет перехода к сетевой активности");
        vm.OpenSettingsCommand.Execute(null);
        Assert(navigation.LastPage == typeof(TgSettingsViewModel).FullName, "Нет перехода к настройкам");
        var operation = StartStatusOperation(state.NetworkActivity, TgNetworkOperationKind.Parsing, 100);
        state.SetIsOnlineReady(isReady: true);
        Assert(vm.ConnectionCaption == "Telegram подключён" && vm.PrimaryActionCaption == "Открыть чаты", "Кнопка не отразила подключение");
        vm.PrimaryActionCommand.Execute(null);
        Assert(navigation.LastPage == typeof(TgChatsViewModel).FullName, "Подключённый клиент не открыл чаты");
        await vm.ReloadUiAsync();
        Assert(state.NetworkActivity.Operations.Single(x => x.Id == operation).State == TgNetworkOperationState.Running
            && vm.TelegramChecks == 0 && vm.StorageCalls == 0, "Главная вмешалась в парсинг или вызвала сеть");
    }

    // История упорядочена по открытию, ограничена тремя чатами и разделена по хранилищам
    private static Task HomeRecentHistoryIsolationAsync()
    {
        var settings = new ITgSettingsService();
        var firstStorage = settings.AppStorage;
        var ids = Enumerable.Range(1, 4).Select(_ => Guid.NewGuid()).ToArray();
        for (var i = 0; i < ids.Length; i++) TgRecentChats.Remember(settings, ids[i], i + 1, $"Чат {i + 1}");
        Assert(TgRecentChats.Read(settings).Select(x => x.Uid).SequenceEqual(ids.Reverse().Take(3)), "Потерян порядок или лимит истории");
        TgRecentChats.Remember(settings, ids[1], 2, "Переименованный чат");
        Assert(TgRecentChats.Read(settings).First().Title == "Переименованный чат" && TgRecentChats.Read(settings).Count == 3,
            "Повторное открытие не обновило название или продублировало чат");
        settings.AppStorage = Path.Combine(AppContext.BaseDirectory, "another-account.db");
        Assert(TgRecentChats.Read(settings).Count == 0, "Другой аккаунт видит чужую историю");
        var otherUid = Guid.NewGuid();
        TgRecentChats.Remember(settings, otherUid, 50, "Другой аккаунт");
        TgRecentChats.Remember(settings, Guid.Empty, 0, "Отсутствующий чат");
        Assert(TgRecentChats.Read(settings).Single().Uid == otherUid, "В историю попал неоткрытый чат");
        settings.AppStorage = firstStorage;
        Assert(TgRecentChats.Read(settings).First().Uid == ids[1] && TgRecentChats.Read(settings).Count == 3, "Переключение базы стёрло историю");
        return Task.CompletedTask;
    }

    // Названия скрываются сразу, язык обновляется, подписки прекращают работу после ухода со страницы
    private static Task HomePrivacyLanguageAndLifetimeAsync()
    {
        using var state = new LoadStateService();
        state.SetIsDisplaySensitiveData(isDisplaySensitiveData: true);
        var settings = new ITgSettingsService();
        var navigation = new INavigationService();
        var uid = Guid.NewGuid();
        TgRecentChats.Remember(settings, uid, 100, "Личный чат");
        using var vm = new TgMainViewModel(state, settings, navigation, null!);
        vm.Activate();
        Assert(!vm.IsRecentEmpty && vm.RecentChats.Single().Title == "Личный чат", "Главная не прочитала историю");
        var row = vm.RecentChats.Single();
        row.OpenCommand.Execute(row);
        Assert(navigation.LastPage == typeof(TgChatViewModel).FullName && Equals(navigation.LastParameter, uid), "История открывает другой чат");
        state.SetIsDisplaySensitiveData(isDisplaySensitiveData: false);
        Assert(vm.RecentChats.Single().Title == state.SensitiveData, "Название чата не скрыто общим переключателем");
        Localizer.Language = "en-US";
        Assert(vm.PrimaryActionCaption == "Connect Telegram" && vm.RecentChats.Single().Detail == "Last opened chat", "Вычисляемые строки остались на русском");
        vm.Deactivate();
        var caption = vm.ConnectionCaption;
        state.SetIsOnlineReady(isReady: true);
        Localizer.Language = "ru-RU";
        Assert(vm.ConnectionCaption == caption, "Скрытая главная продолжает обрабатывать события");
        vm.Activate();
        Assert(vm.ConnectionCaption == "Telegram подключён" && vm.PrimaryActionCaption == "Открыть чаты", "Повторное открытие показывает устаревшее состояние");
        return Task.CompletedTask;
    }

    // Каждый видимый текст переведён, прежние справочные ссылки сохранены
    private static Task HomeLocalizationAndLinksAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgMainPage.xaml")!;
        var page = XDocument.Load(stream);
        var uids = page.Descendants().Select(x => new { Element = x.Name.LocalName,
            Uid = x.Attributes().FirstOrDefault(a => a.Name.LocalName.EndsWith("Uid", StringComparison.Ordinal))?.Value }).ToArray();
        foreach (var language in new[] { "ru-RU", "en-US" })
        {
            var resources = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Strings", language, "Resources.resw"));
            var values = resources.Root!.Elements("data").Where(x => x.Attribute("name")!.Value.StartsWith("Home", StringComparison.Ordinal))
                .ToDictionary(x => x.Attribute("name")!.Value, x => x.Element("value")!.Value);
            foreach (var control in uids.Where(x => x.Uid?.StartsWith("Home", StringComparison.Ordinal) == true))
            {
                var key = control.Uid + (control.Element == "TextBlock" ? ".Text" : ".Content");
                Assert(values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value), $"Отсутствует перевод {language}: {key}");
            }
        }
        foreach (var uid in new[] { "SettingsPage_OfficialSite_OpenTgResearcher", "SettingsPage_Releases_Latest", "SettingsPage_DownloadStatistics",
            "SettingsPage_GitHub_Link", "SettingsPage_GitHub_ReportBug", "SettingsPage_GitHub_Discussions", "SettingsPage_GitHub_OpenTgResearcher",
            "SettingsPage_GitHub_DockerConsole", "SettingsPage_GitHub_InstallRequirementsRegisterTelegram" })
            Assert(uids.Any(x => x.Uid == uid), $"При замене главной потеряна ссылка {uid}");
        return Task.CompletedTask;
    }

    // Главная доступна без сети и не вмешивается в загрузки других страниц
    private static async Task HomeOpensWithoutTelegramProbeAsync()
    {
        using var state = new LoadStateService();
        using var vm = new TgMainViewModel(state, new ITgSettingsService(), new INavigationService(), null!);
        await vm.OnNavigatedToAsync(null);
        Assert(vm.TelegramChecks == 0 && vm.StorageCalls == 0, "Открытие главной запускает проверку Telegram или блокировку хранилища");
    }

    // Главная содержит действующие переходы, недавние чаты и описание без собственной копии меню и статуса
    private static Task HomeLaunchLayoutAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgMainPage.xaml")!;
        var page = XDocument.Load(stream);
        foreach (var command in new[] { "PrimaryActionCommand", "OpenChatsCommand", "OpenNetworkCommand", "OpenSettingsCommand", "OpenCommand" })
            Assert(page.Descendants().Any(x => x.Attribute("Command")?.Value.Contains(command) == true), $"Главная не содержит переход {command}");
        Assert(page.Descendants().Any(x => x.Attribute("ItemsSource")?.Value.Contains("RecentChats") == true), "Нет списка недавних чатов");
        Assert(page.Descendants().Any(x => x.Name.LocalName == "AdaptiveTrigger"), "Главная не адаптируется к ширине окна");
        Assert(!page.Descendants().Any(x => x.Name.LocalName is "NavigationView" or "TgStatusBar"), "Главная дублирует оболочку приложения");
        return Task.CompletedTask;
    }
}
