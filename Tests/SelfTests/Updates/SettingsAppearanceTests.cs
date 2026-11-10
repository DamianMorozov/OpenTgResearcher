namespace OpenTgResearcherDesktop.UpdateSelfTests;

internal static partial class Program
{
    // Подписи и списки находятся в одной строке, все элементы выбора акцента расположены рядом
    private static Task SettingsInlineAppearanceAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgSettingsPage.xaml")!;
        var page = XDocument.Load(stream);
        XNamespace l = "using:WinUI3Localizer";
        foreach (var uid in new[] { "ProgramLanguage", "ProgramTheme" })
        {
            var label = page.Descendants().SingleOrDefault(x => x.Name.LocalName == "TextBlock" && x.Attribute(l + "Uids.Uid")?.Value == uid);
            Assert(label is not null, $"Подпись {uid} расположена над списком вместо одной строки");
            var row = label!.Parent!;
            Assert(row.Elements().Any(x => x.Name.LocalName == "TgOutlineIcon" && x.Attribute("Grid.Column") is null)
                && label.Attribute("Grid.Column")?.Value == "1"
                && row.Elements().Any(x => x.Name.LocalName == "ComboBox" && x.Attribute("Grid.Column")?.Value == "2"),
                $"Нарушен порядок значок, подпись, список для {uid}");
        }
        Assert(!page.Descendants().Any(x => x.Name.LocalName == "CheckBox" && x.Attribute(l + "Uids.Uid")?.Value == "ProgramSystemTheme"),
            "Остался отдельный флажок системной темы");
        var themeList = page.Descendants().Single(x => x.Name.LocalName == "ComboBox"
            && x.Attribute("SelectedIndex")?.Value.Contains("Preferences.ThemeIndex") == true);
        Assert(themeList.Attribute("IsEnabled") is null && themeList.Elements().Count(x => x.Name.LocalName == "ComboBoxItem") == 3
            && themeList.Elements().First().Attribute(l + "Uids.Uid")?.Value == "ProgramDefaultTheme",
            "В списке недоступен выбор системной, светлой и тёмной темы");
        var accent = page.Descendants().Single(x => x.Attribute(l + "Uids.Uid")?.Value == "ProgramSystemAccent");
        var accentLabel = page.Descendants().SingleOrDefault(x => x.Name.LocalName == "TextBlock" && x.Attribute(l + "Uids.Uid")?.Value == "ProgramAccent");
        var accentInput = page.Descendants().Single(x => x.Name.LocalName == "TextBox"
            && x.Attribute("Text")?.Value.Contains("Preferences.AccentHex") == true);
        var accentButton = page.Descendants().Single(x => x.Attribute(l + "Uids.Uid")?.Value == "ProgramPickAccent");
        Assert(accentLabel is not null && accent.Parent == accentLabel.Parent && accent.Parent == accentInput.Parent
            && accent.Parent == accentButton.Parent, "Системный акцент, подпись, поле ввода и выбор цвета не размещены в одной строке");
        Assert(accent.Attribute("Grid.Row") is null && accent.Attribute("Grid.Column") is null
            && accentLabel!.Attribute("Grid.Column")?.Value == "1" && accentInput.Attribute("Grid.Column")?.Value == "2"
            && accentButton.Attribute("Grid.Column")?.Value == "3", "Нарушен порядок элементов строки цвета акцента");
        Assert(accentInput.Attribute("IsEnabled")?.Value.Contains("Preferences.UseSystemAccent") == true
            && accentInput.Attribute("IsEnabled")?.Value == accentButton.Attribute("IsEnabled")?.Value,
            "Ручной ввод и палитра не отключаются вместе при выборе системного акцента");
        return Task.CompletedTask;
    }

    // Каждый пункт списка сохраняет свою тему и восстанавливается при повторной загрузке
    private static Task SettingsThemeChoicesAsync()
    {
        var settings = new ITgSettingsService();
        var preferences = new TgProgramPreferences(settings);
        foreach (var theme in new[] { TgEnumTheme.Light, TgEnumTheme.Dark, TgEnumTheme.Default })
        {
            preferences.Load();
            preferences.ThemeIndex = (int)theme;
            preferences.Save("1.0.0", _ => { }, (_, _) => { });
            Assert(settings.AppTheme == theme, $"Выбор {theme} сохранил другую тему");
            var restored = new TgProgramPreferences(settings);
            restored.Load();
            Assert(restored.ThemeIndex == (int)theme, $"Выбор {theme} не восстановлен в списке");
        }
        return Task.CompletedTask;
    }

    // Настройки содержат одну общую карточку приложения со всеми перенесёнными полями и значками
    private static Task AppearanceMovedToSettingsAsync()
    {
        using var settingsStream = typeof(Program).Assembly.GetManifestResourceStream("TgSettingsPage.xaml")!;
        var settings = XDocument.Load(settingsStream);
        XNamespace l = "using:WinUI3Localizer";
        var card = settings.Descendants().Single(x => x.Attribute(l + "Uids.Uid")?.Value == "ExpanderSettingApp");
        foreach (var uid in new[] { "ProgramLanguage", "ProgramTheme", "ProgramAutoStart", "ProgramDefaultTheme", "ProgramSystemAccent", "ProgramAccent" })
        {
            Assert(card.Descendants().Count(x => x.Attribute(l + "Uids.Uid")?.Value == uid) == 1, $"В общей карточке отсутствует или дублируется {uid}");
        }
        foreach (var kind in new[] { "language", "theme" })
            Assert(card.Descendants().Any(x => x.Name.LocalName == "TgOutlineIcon" && x.Attribute("Kind")?.Value == kind),
                $"В общей карточке отсутствует значок {kind}");
        Assert(!card.Descendants().Attributes().Any(x => x.Name.LocalName == "SelectedItem" && x.Value.Contains("SettingsService.App")),
            "Старые поля оформления не объединены с перенесёнными");
        Assert(card.Descendants().Any(x => x.Name.LocalName == "ColorPicker"), "Палитра цвета не перенесена");
        return Task.CompletedTask;
    }

    // Общая команда настроек сохраняет предварительные выпуски вместе с текущим оформлением
    private static async Task SettingsSavesUpdatePreferenceAsync()
    {
        var settings = new ITgSettingsService();
        settings.SaveLanguage(TgEnumLanguage.Russian);
        settings.SaveTheme(TgEnumTheme.Dark);
        var original = new TgProgramPreferences.Values(false, "#123456", false, false);
        settings.SaveSetting(TgProgramPreferences.PreferencesKey, original);
        using var vm = CreateSettingsViewModel(settings);
        await vm.OnNavigatedToAsync(null);
        vm.Preferences.IncludePreview = true;
        await vm.SettingsSaveCommand.ExecuteAsync(null);
        Assert(settings.ReadSetting<TgProgramPreferences.Values>(TgProgramPreferences.PreferencesKey) == original with { IncludePreview = true },
            "Общие настройки не сохранили выбор предварительных выпусков");
        Assert(settings.AppLanguage == TgEnumLanguage.Russian && settings.AppTheme == TgEnumTheme.Dark && vm.Errors.Count == 0,
            "Сохранение выбора обновлений потеряло текущее оформление");
    }

    // При отсутствии сохранённой карточки загрузка убирает черновик и возвращает системное оформление
    private static async Task SettingsLoadsAppearanceDefaultsAsync()
    {
        using var vm = CreateSettingsViewModel();
        await vm.OnNavigatedToAsync(null);
        vm.Preferences.UseSystemAccent = false;
        vm.Preferences.AccentHex = "#123456";
        await vm.SettingsLoadCommand.ExecuteAsync(null);
        Assert(vm.Preferences.UseSystemAccent && vm.Preferences.AccentHex == "#0078D4", "Загрузка оставила несохранённый цвет вместо системного");
    }

    // Загрузка отменяет черновик, а сброс обновляет все перенесённые поля без немедленной записи
    private static async Task SettingsReloadsAndResetsAppearanceAsync()
    {
        var settings = new ITgSettingsService();
        var saved = new TgProgramPreferences.Values(false, "#123456", false, true);
        settings.SaveSetting(TgProgramPreferences.PreferencesKey, saved);
        settings.SaveLanguage(TgEnumLanguage.Russian);
        settings.SaveTheme(TgEnumTheme.Dark);
        using var vm = CreateSettingsViewModel(settings);
        await vm.OnNavigatedToAsync(null);
        vm.Preferences.AccentHex = "#ABCDEF";
        vm.Preferences.LanguageIndex = (int)TgEnumLanguage.English;
        await vm.SettingsLoadCommand.ExecuteAsync(null);
        Assert(vm.Preferences.AccentHex == "#123456" && vm.Preferences.LanguageIndex == (int)TgEnumLanguage.Russian,
            "Загрузка не восстановила перенесённые настройки");
        await vm.SettingsDefaultCommand.ExecuteAsync(null);
        Assert(vm.Preferences.ThemeIndex == (int)TgEnumTheme.Default && vm.Preferences.UseSystemAccent && !vm.Preferences.AutoStart
            && vm.Preferences.AccentHex == "#0078D4" && vm.Preferences.LanguageIndex == (int)TgEnumLanguage.Default,
            "Сброс оставил прежнее оформление в перенесённых полях");
        Assert(settings.ReadSetting<TgProgramPreferences.Values>(TgProgramPreferences.PreferencesKey) == saved && settings.SaveCalls == 0,
            "Сброс сохранил черновик без команды сохранения");
        await vm.SettingsSaveCommand.ExecuteAsync(null);
        Assert(settings.AppLanguage == TgEnumLanguage.Default && settings.AppTheme == TgEnumTheme.Default
            && settings.ReadSetting<TgProgramPreferences.Values>(TgProgramPreferences.PreferencesKey) is
                { UseSystemAccent: true, AccentHex: "#0078D4", IncludePreview: true },
            "Сохранение сброса не применило настройки по умолчанию или изменило выбор предварительных выпусков");
    }

    // Настоящая команда настроек применяет перенесённые параметры и сохраняет существующие настройки файлов
    private static async Task SettingsSavesMovedPreferencesAsync()
    {
        var settings = new ITgSettingsService();
        using var vm = CreateSettingsViewModel(settings);
        await vm.OnNavigatedToAsync(null);
        var preferences = vm.Preferences;
        preferences.LanguageIndex = (int)TgEnumLanguage.Russian;
        preferences.ThemeIndex = (int)TgEnumTheme.Dark;
        preferences.AutoStart = true;
        preferences.UseSystemAccent = false;
        preferences.AccentHex = "#123456";
        var autoStartCalls = 0;
        var accentCalls = 0;
        TgProgramIntegration.AutoStartObserver = enabled => { if (enabled) autoStartCalls++; };
        TgProgramIntegration.AccentObserver = (system, hex) => { if (!system && hex == "#123456") accentCalls++; };
        try { await vm.SettingsSaveCommand.ExecuteAsync(null); }
        finally { TgProgramIntegration.AutoStartObserver = null; TgProgramIntegration.AccentObserver = null; }
        Assert(settings.AppLanguage == TgEnumLanguage.Russian && settings.AppTheme == TgEnumTheme.Dark, "Команда настроек не применила язык и тему");
        Assert(settings.ReadSetting<TgProgramPreferences.Values>(TgProgramPreferences.PreferencesKey) is
            { AutoStart: true, UseSystemAccent: false, AccentHex: "#123456" }, "Команда настроек не сохранила перенесённые поля");
        Assert(settings.ReadSetting<string>(TgProgramPreferences.ConfiguredVersionKey) == TgProgramIntegration.CurrentVersion,
            "Настройка новой версии не завершена после сохранения настроек");
        Assert(settings.SaveCalls == 1 && settings.LoadCalls == 2, "Настройки файлов не сохранены вместе с перенесённым оформлением");
        Assert(autoStartCalls == 1 && accentCalls == 1, "Команда сохранила поля без применения автозапуска и акцента");
    }
}
