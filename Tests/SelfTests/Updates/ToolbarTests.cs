namespace OpenTgResearcherDesktop.UpdateSelfTests;

internal static partial class Program
{
    // Заголовок и заставка используют короткое название, имя установленного пакета сохраняется
    private static Task WindowAndSplashTitleAsync(string language)
    {
        Localizer.Language = language;
        Assert(TgResourceExtensions.GetAppDisplayName() == "OpenTgResearcher", "В заголовке и на заставке осталось название OpenTgResearcherDesktop");
        Assert("AppDisplayName".GetLocalizedResource() == "OpenTgResearcherDesktop", "Изменилось имя приложения в манифесте пакета");
        return Task.CompletedTask;
    }

    // Выбор языка выполняет действие и не сохраняет остальные настройки
    private static Task ToolbarLanguageChoiceAsync()
    {
        var vm = new ShellViewModel();
        var command = Read<IRelayCommand>(vm, "ChangeLanguageCommand");
        foreach (var language in Enum.GetValues<TgEnumLanguage>())
        {
            command.Execute(language.ToString());
            Assert(vm.SettingsService.AppLanguage == language, "Выбранный язык не применён");
        }
        Assert(vm.SettingsService.LanguageWrites == 3 && vm.SettingsService.ThemeWrites == 0, "Язык не сохранён отдельно от темы");
        return Task.CompletedTask;
    }

    // Выбор темы выполняет действие для всех поддерживаемых вариантов
    private static Task ToolbarThemeChoiceAsync()
    {
        var vm = new ShellViewModel();
        var command = Read<IRelayCommand>(vm, "ChangeThemeCommand");
        foreach (var theme in Enum.GetValues<TgEnumTheme>())
        {
            command.Execute(theme.ToString());
            Assert(vm.SettingsService.AppTheme == theme, "Выбранная тема не применена");
        }
        Assert(vm.SettingsService.ThemeWrites == 3 && vm.SettingsService.LanguageWrites == 0, "Тема не сохранена отдельно от языка");
        return Task.CompletedTask;
    }

    // Ошибочный параметр команды не изменяет настройки
    private static Task ToolbarRejectsInvalidChoiceAsync()
    {
        var vm = new ShellViewModel();
        foreach (var value in new string?[] { null, "", "Unsupported", "99" })
        {
            Read<IRelayCommand>(vm, "ChangeLanguageCommand").Execute(value);
            Read<IRelayCommand>(vm, "ChangeThemeCommand").Execute(value);
        }
        Assert(vm.SettingsService.LanguageWrites == 0 && vm.SettingsService.ThemeWrites == 0, "Ошибочный выбор сохранён");
        return Task.CompletedTask;
    }

    // Верхняя панель открывает страницу и ожидает исходную команду с общим состоянием обновления
    private static async Task ToolbarWaitsForSharedUpdateAsync()
    {
        var vm = new ShellViewModel();
        var root = new XamlRoot();
        UpdateManager.State.CheckGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var action = Read<IAsyncRelayCommand>(vm, "CheckForUpdatesCommand").ExecuteAsync(root);
        await WaitAsync(() => UpdateManager.State.Checks == 1);
        Assert(!action.IsCompleted && vm.Updates.IsUpdating, "Команда панели завершилась до результата проверки");
        Assert(vm.NavigationService.LastPage == typeof(TgSettingsViewModel).FullName, "Не открыта страница настроек с результатом обновления");
        Assert(vm.Settings.IsUpdatesExpanded && ReferenceEquals(vm.Updates, vm.Settings.Updates), "Верхняя панель не раскрыла общий блок обновлений");
        Assert(ReferenceEquals(vm.Updates.XamlRootVm, root), "Не передано окно для подтверждения");
        Assert(!vm.Updates.UpdatePreviewCommand.CanExecute(null), "Предварительная проверка доступна во время проверки панели");
        UpdateManager.State.CheckGate.SetResult();
        await action;
        Assert(vm.Updates.UpdateLog.Contains("последняя версия") && !vm.Updates.IsUpdating, "Результат не появился на общей странице");
    }

    // Отказ от подтверждения из верхней панели не запускает сеть
    private static async Task ToolbarCancelConfirmationAsync()
    {
        var vm = new ShellViewModel();
        ContentDialog.Confirm = false;
        await Read<IAsyncRelayCommand>(vm, "CheckForUpdatesCommand").ExecuteAsync(new XamlRoot());
        Assert(vm.NavigationService.Navigations == 1 && UpdateManager.State.Created == 0, "Отказ не остановил обновление");
    }

    // Разметка оставляет кнопки вне перетаскивания и поддерживает узкое окно
    private static Task ToolbarControlsLayoutAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("ShellPage.xaml")!;
        var root = XDocument.Load(stream);
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var controls = root.Descendants().Where(element => element.Attribute(x + "Name") is not null)
            .ToDictionary(element => (string)element.Attribute(x + "Name")!, element => element);
        Assert(controls.ContainsKey("TitleBarDragRegion"), "Отсутствует отдельная область перетаскивания окна");
        foreach (var name in new[] { "ToolbarLanguageButton", "ToolbarThemeButton", "ToolbarUpdateButton" })
        {
            Assert(controls.TryGetValue(name, out var button), $"Нет кнопки {name}");
            Assert(!button!.Ancestors().Contains(controls["TitleBarDragRegion"]), "Кнопка попала в область перетаскивания");
            Assert(button.Attribute("AutomationProperties.AutomationId") is not null, "У кнопки нет идентификатора доступности");
        }
        Assert(root.Descendants().Any(element => element.Name.LocalName == "AdaptiveTrigger"), "Нет компактной разметки верхней панели");
        Assert(controls["ToolbarUpdateButton"].Attribute("Command")?.Value.Contains("CheckForUpdatesCommand") == true,
            "Кнопка не выполняет проверку обновлений");
        return Task.CompletedTask;
    }

    // Общий переключатель принадлежит панели окна и читает текущее состояние вместо сброса при навигации
    private static Task ToolbarSensitiveDataLayoutAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("ShellPage.xaml")!;
        var root = XDocument.Load(stream);
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var toggles = root.Descendants().Where(element => (string?)element.Attribute(x + "Name") == "ToggleSwitchShowSensitiveData").ToArray();
        Assert(toggles.Length == 1, "Должен остаться один общий переключатель конфиденциальных данных");
        var toggle = toggles[0];
        Assert(toggle.Ancestors().Any(element => (string?)element.Attribute(x + "Name") == "AppTitleBar"),
            "Переключатель конфиденциальных данных остался в заголовке страницы вместо верхней панели");
        Assert(!toggle.Ancestors().Any(element => element.Name.LocalName == "DataTemplate"), "Переключатель пересоздаётся при навигации");
        Assert(!toggle.Ancestors().Any(element => (string?)element.Attribute(x + "Name") == "TitleBarDragRegion"),
            "Переключатель попал в область перетаскивания окна");
        Assert(toggle.Attribute("IsChecked")?.Value.Contains("ViewModel.IsDisplaySensitiveData, Mode=OneWay") == true,
            "Начальное состояние переключателя не связано с общим состоянием приложения");
        Assert(toggle.Attribute("AutomationProperties.AutomationId")?.Value == "ToggleSwitchShowSensitiveData",
            "Потерян идентификатор существующей проверки скрытия обложек");
        return Task.CompletedTask;
    }

    // Проверяем реальные подписи и варианты меню для обоих языков
    private static Task ToolbarStringsAsync(string language, string languageCaption, string themeCaption, string updateCaption)
    {
        Localizer.Language = language;
        Assert("ToolbarLanguage".GetLocalizedResource() == languageCaption, "Не переведена кнопка языка");
        Assert("ToolbarTheme".GetLocalizedResource() == themeCaption, "Не переведена кнопка темы");
        Assert("ToolbarUpdate".GetLocalizedResource() == updateCaption, "Не переведена проверка обновлений");
        Assert("ToolbarSensitiveData".GetLocalizedResource() == (language == "ru-RU" ? "Данные" : "Data"),
            "Не переведена краткая подпись переключателя данных");
        Assert("ToolbarSensitiveDataToolTip".GetLocalizedResource() == (language == "ru-RU"
            ? "Показывать конфиденциальные данные на всех страницах" : "Show sensitive data on all pages"),
            "Нет полной подсказки переключателя данных");
        foreach (var key in new[] { "ToolbarSystemLanguage", "ToolbarSystemTheme", "ToolbarLightTheme", "ToolbarDarkTheme" })
            Assert(!string.IsNullOrWhiteSpace(key.GetLocalizedResource()), $"Нет перевода {key}");
        return Task.CompletedTask;
    }
}
