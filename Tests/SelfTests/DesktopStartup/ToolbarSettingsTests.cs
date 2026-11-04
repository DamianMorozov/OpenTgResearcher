namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static partial class Program
{
    // Проверяем сохранение языка и применение культуры без сохранения черновиков путей
    private static Task ToolbarSavesOnlyLanguageAsync()
    {
        TgRuntimeHelper.IsMSIX = false;
        var files = new MemoryFileService("English");
        var settings = new TgSettingsService(files, Options.Create(new LocalSettingsOptions()));
        App.Settings = settings;
        var root = new FrameworkElement();
        App.MainWindow.Content = root;
        var method = typeof(TgSettingsService).GetMethod("SaveLanguage") ?? throw new InvalidOperationException("Нет сохранения языка из панели");
        method.Invoke(settings, [TgEnumLanguage.Russian]);
        Assert(settings.AppLanguage == TgEnumLanguage.Russian, "Служба не обновила выбранный язык");
        Assert(files.SavedSettings[nameof(LocalSettingsOptions.AppLanguage)].ToString() == "Russian", "Язык не сохранён");
        Assert(!files.SavedSettings.ContainsKey(nameof(LocalSettingsOptions.AppStorage)) &&
            !files.SavedSettings.ContainsKey(nameof(LocalSettingsOptions.AppSession)), "Выбор языка сохранил черновики путей");
        Assert(Localizer.Instance.Language == "ru-RU" && root.LayoutCalls > 0, "Интерфейс не обновился после выбора языка");
        Assert(CultureInfo.CurrentUICulture.Name == "ru-RU" && TestLogUtils.Errors.Count == 0, "Смена языка зарегистрировала ошибку");
        return Task.CompletedTask;
    }

    // Проверяем сохранение темы и фактическое оформление корня окна
    private static Task ToolbarSavesOnlyThemeAsync()
    {
        TgRuntimeHelper.IsMSIX = false;
        var files = new MemoryFileService("English");
        var settings = new TgSettingsService(files, Options.Create(new LocalSettingsOptions()));
        App.Settings = settings;
        var root = new FrameworkElement();
        App.MainWindow.Content = root;
        var method = typeof(TgSettingsService).GetMethod("SaveTheme") ?? throw new InvalidOperationException("Нет сохранения темы из панели");
        foreach (var theme in Enum.GetValues<TgEnumTheme>())
        {
            method.Invoke(settings, [theme]);
            Assert(settings.AppTheme == theme && Equals(root.RequestedTheme, TgThemeUtils.GetElementTheme(theme)), "Тема не применена к окну");
            Assert(files.SavedSettings[nameof(LocalSettingsOptions.AppTheme)].ToString() == theme.ToString(), "Тема не сохранена");
        }
        Assert(!files.SavedSettings.ContainsKey(nameof(LocalSettingsOptions.AppStorage)) &&
            !files.SavedSettings.ContainsKey(nameof(LocalSettingsOptions.AppSession)), "Выбор темы сохранил черновики путей");
        Assert(TestLogUtils.Errors.Count == 0, "Смена темы зарегистрировала ошибку");
        return Task.CompletedTask;
    }
}
