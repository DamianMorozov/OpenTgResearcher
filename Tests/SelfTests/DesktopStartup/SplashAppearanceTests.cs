namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static partial class Program
{
    // Проверяем доступность управления во время загрузки и привязки к сохраняющим командам
    private static Task SplashAppearanceControlsAsync()
    {
        var page = ReadSplashLayout();
        foreach (var kind in new[] { "Language", "Theme" })
        {
            var button = NamedSplashElement(page, $"Splash{kind}Button");
            Assert(button.Name.LocalName == "Button" && button.Attribute("IsEnabled") is null && button.Attribute("Visibility") is null,
                "Выбор оформления недоступен во время загрузки");
            Assert(button.Descendants().Any(x => x.Attribute("Kind")?.Value == kind.ToLowerInvariant()), "На кнопке нет общей иконки");
            var options = button.Descendants().Where(x => x.Name.LocalName == "RadioMenuFlyoutItem").ToArray();
            var values = kind == "Language" ? new[] { "Default", "English", "Russian" } : new[] { "Default", "Light", "Dark" };
            Assert(options.Select(x => x.Attribute("CommandParameter")?.Value).SequenceEqual(values), "Набор вариантов оформления неполон");
            Assert(options.All(x => x.Attribute("Command")?.Value == $"{{x:Bind ViewModel.Appearance.Change{kind}Command}}"
                && x.Attribute("IsChecked")?.Value.Contains("Mode=OneWay") == true), "Выбор не связан с общей моделью и текущими настройками");
        }
        var toolbar = NamedSplashElement(page, "SplashAppearanceToolbar");
        Assert(toolbar.Attribute("VerticalAlignment")?.Value == "Top" && toolbar.Attribute("HorizontalAlignment")?.Value == "Right",
            "Управление оформлением не закреплено в верхнем правом углу");
        Assert(int.Parse(toolbar.Attribute("Margin")!.Value.Split(',')[1], CultureInfo.InvariantCulture) >= 48,
            "Кнопки оформления перекрываются системными кнопками окна");
        return Task.CompletedTask;
    }

    // Иконка языка содержит контуры перевода вместо прежнего глобуса в обеих темах
    private static Task LanguageIconUsesTranslationSymbolAsync()
    {
        var directory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../../Presentation/OpenTgResearcherDesktop/Assets/Icons/Lucide"));
        foreach (var theme in new[] { "light", "dark" })
        {
            var svg = XDocument.Load(Path.Combine(directory, $"language-{theme}.svg"));
            Assert(!svg.Descendants().Any(x => x.Name.LocalName == "circle"), "Иконка языка всё ещё изображает глобус");
            Assert(svg.Descendants().Count(x => x.Name.LocalName == "path") >= 3, "В иконке нет контуров иероглифа и буквы A");
        }
        return Task.CompletedTask;
    }

    // Команды заставки применяют язык и тему через настоящую службу и сохраняют только выбранные параметры
    private static Task SplashAppearanceSavesPreferencesAsync()
    {
        TgRuntimeHelper.IsMSIX = false;
        var files = new AppearanceFileService();
        var settings = new TgSettingsService(files, Options.Create(new LocalSettingsOptions()));
        App.Settings = settings;
        var root = new FrameworkElement();
        App.MainWindow.Content = root;
        var model = new OpenTgResearcherDesktop.ViewModels.TgAppearanceViewModel(settings);
        var language = model.ChangeLanguageCommand;
        var theme = model.ChangeThemeCommand;
        language.Execute("Russian");
        theme.Execute("Dark");
        Assert(settings.AppLanguage == TgEnumLanguage.Russian && Localizer.Instance.Language == "ru-RU", "Команда не применила русский язык");
        Assert(settings.AppTheme == TgEnumTheme.Dark && Equals(root.RequestedTheme, TgThemeUtils.GetElementTheme(TgEnumTheme.Dark)),
            "Команда не применила тёмную тему к окну");
        Assert(files.SavedSettings[nameof(LocalSettingsOptions.AppLanguage)].ToString() == "Russian"
            && files.SavedSettings[nameof(LocalSettingsOptions.AppTheme)].ToString() == "Dark", "Выбор заставки не сохранён");
        foreach (var invalid in new string?[] { null, "", "Unsupported", "99" }) { language.Execute(invalid); theme.Execute(invalid); }
        Assert(settings.AppLanguage == TgEnumLanguage.Russian && settings.AppTheme == TgEnumTheme.Dark, "Неверный параметр изменил настройки");
        Assert(!files.SavedSettings.ContainsKey(nameof(LocalSettingsOptions.AppStorage))
            && !files.SavedSettings.ContainsKey(nameof(LocalSettingsOptions.AppSession)), "Смена оформления сохранила черновики путей");
        var restored = new TgSettingsService(files, Options.Create(new LocalSettingsOptions()));
        App.Settings = restored;
        restored.Load();
        Assert(restored.AppLanguage == TgEnumLanguage.Russian && restored.AppTheme == TgEnumTheme.Dark,
            "Новый экземпляр службы не восстановил язык и тему заставки");
        Assert(TestLogUtils.Errors.Count == 0, "Смена оформления записала ошибку");
        return Task.CompletedTask;
    }

    // Храним снимок настроек между экземплярами исходной службы вместо пользовательского файла
    private sealed class AppearanceFileService : IFileService
    {
        public Dictionary<string, object> SavedSettings { get; private set; } = [];

        /// <summary> Возвращает сохранённые значения новому экземпляру службы настроек </summary>
        public T Read<T>(string folderPath, string fileName) => (T)(object)new Dictionary<string, object>(SavedSettings);

        /// <summary> Запоминает результат исходной службы без записи в настройки пользователя </summary>
        public void Save<T>(string folderPath, string fileName, T content) =>
            SavedSettings = new Dictionary<string, object>((IDictionary<string, object>)(object)content!);
    }
}
