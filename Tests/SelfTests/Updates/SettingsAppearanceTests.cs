namespace OpenTgResearcherDesktop.UpdateSelfTests;

internal static partial class Program
{
    // Настройки содержат одну общую карточку приложения со всеми перенесёнными полями и значками
    private static Task AppearanceMovedToSettingsAsync()
    {
        using var settingsStream = typeof(Program).Assembly.GetManifestResourceStream("TgSettingsPage.xaml")!;
        using var programStream = typeof(Program).Assembly.GetManifestResourceStream("TgUpdatePage.xaml")!;
        var settings = XDocument.Load(settingsStream);
        var program = XDocument.Load(programStream);
        XNamespace l = "using:WinUI3Localizer";
        var card = settings.Descendants().Single(x => x.Attribute(l + "Uids.Uid")?.Value == "ExpanderSettingApp");
        foreach (var uid in new[] { "ProgramLanguage", "ProgramTheme", "ProgramAutoStart", "ProgramSystemTheme", "ProgramSystemAccent", "ProgramAccent" })
        {
            Assert(card.Descendants().Count(x => x.Attribute(l + "Uids.Uid")?.Value == uid) == 1, $"В общей карточке отсутствует или дублируется {uid}");
            Assert(!program.Descendants().Any(x => x.Attribute(l + "Uids.Uid")?.Value == uid), $"На странице программы осталось поле {uid}");
        }
        foreach (var kind in new[] { "language", "theme" })
            Assert(card.Descendants().Any(x => x.Name.LocalName == "TgOutlineIcon" && x.Attribute("Kind")?.Value == kind),
                $"В общей карточке отсутствует значок {kind}");
        Assert(!card.Descendants().Attributes().Any(x => x.Name.LocalName == "SelectedItem" && x.Value.Contains("SettingsService.App")),
            "Старые поля оформления не объединены с перенесёнными");
        Assert(card.Descendants().Any(x => x.Name.LocalName == "ColorPicker"), "Палитра цвета не перенесена");
        Assert(!program.Descendants().Any(x => x.Name.LocalName == "ColorPicker"), "На странице программы осталась палитра");
        return Task.CompletedTask;
    }

    // Сохранение проверки предварительных выпусков не применяет скрытый черновик оформления
    private static Task UpdateSavePreservesAppearanceAsync()
    {
        var settings = new ITgSettingsService { AppLanguage = TgEnumLanguage.Russian, AppTheme = TgEnumTheme.Dark };
        var original = new TgProgramPreferences.Values(false, "#123456", true, false);
        settings.SaveSetting(TgProgramPreferences.PreferencesKey, original);
        var vm = new TgUpdateViewModel(new LoadStateService(), settings, new(), null!);
        vm.Preferences.IncludePreview = true;
        vm.Preferences.LanguageIndex = (int)TgEnumLanguage.English;
        vm.Preferences.ThemeIndex = 0;
        vm.Preferences.AccentHex = "invalid";
        Read<IRelayCommand>(vm, "SaveProgramSettingsCommand").Execute(null);
        Assert(settings.ReadSetting<TgProgramPreferences.Values>(TgProgramPreferences.PreferencesKey) == original with { IncludePreview = true },
            "Страница программы не сохранила выбор выпусков отдельно от оформления");
        Assert(settings.LanguageWrites == 0 && settings.ThemeWrites == 0 && vm.Errors.Count == 0,
            "Сохранение обновлений применило скрытое оформление или зарегистрировало ошибку");
        Assert(settings.ReadSetting<string>(TgProgramPreferences.ConfiguredVersionKey) is null,
            "Сохранение обновлений ошибочно завершило настройку оформления новой версии");
        return Task.CompletedTask;
    }

    // При отсутствии сохранённой карточки загрузка убирает черновик и возвращает системное оформление
    private static async Task SettingsLoadsAppearanceDefaultsAsync()
    {
        using var vm = new TgSettingsViewModel(new LoadStateService(), new(), new(), null!);
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
        using var vm = new TgSettingsViewModel(new LoadStateService(), settings, new(), null!);
        await vm.OnNavigatedToAsync(null);
        vm.Preferences.AccentHex = "#ABCDEF";
        vm.Preferences.LanguageIndex = (int)TgEnumLanguage.English;
        await vm.SettingsLoadCommand.ExecuteAsync(null);
        Assert(vm.Preferences.AccentHex == "#123456" && vm.Preferences.LanguageIndex == (int)TgEnumLanguage.Russian,
            "Загрузка не восстановила перенесённые настройки");
        await vm.SettingsDefaultCommand.ExecuteAsync(null);
        Assert(vm.Preferences.UseSystemTheme && vm.Preferences.UseSystemAccent && !vm.Preferences.AutoStart
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
        using var vm = new TgSettingsViewModel(new LoadStateService(), settings, new(), null!);
        await vm.OnNavigatedToAsync(null);
        var preferences = vm.Preferences;
        preferences.LanguageIndex = (int)TgEnumLanguage.Russian;
        preferences.UseSystemTheme = false;
        preferences.ThemeIndex = 1;
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
