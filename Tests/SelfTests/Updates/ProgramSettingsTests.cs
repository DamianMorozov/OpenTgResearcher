namespace OpenTgResearcherDesktop.UpdateSelfTests;

internal static partial class Program
{
    // Числовая версия без суффикса тоже требует предупреждения при флаге предварительного выпуска GitHub
    private static async Task GithubPreviewFlagNeedsConfirmationAsync()
    {
        UpdateManager.State.Update = new()
        {
            TargetFullRelease = new GitBase<GithubRelease>.GitBaseAsset { Release = new() { Prerelease = true } },
        };
        ContentDialog.Answers.Enqueue(true);
        ContentDialog.Answers.Enqueue(false);
        await CreateViewModel().UpdateReleaseCommand.ExecuteAsync(null);
        Assert(ContentDialog.Titles.Count == 2 && UpdateManager.State.Downloads == 0 && App.Restarts == 0,
            "Флаг предварительного выпуска GitHub проигнорирован для числовой версии");
    }

    // Проверяем подписи реальных контролов формы на обоих языках
    private static Task ProgramSettingsLocalizationAsync()
    {
        using var settingsStream = typeof(Program).Assembly.GetManifestResourceStream("TgSettingsPage.xaml")!;
        var page = XDocument.Load(settingsStream);
        foreach (var language in new[] { "ru-RU", "en-US" })
        {
            var resources = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Strings", language, "Resources.resw"))
                .Root!.Elements("data").Where(x => ((string)x.Attribute("name")!).StartsWith("Program", StringComparison.Ordinal))
                .ToDictionary(x => (string)x.Attribute("name")!, x => x.Element("value")!.Value);
            foreach (var control in page.Descendants())
            {
                var uid = control.Attributes().FirstOrDefault(x => x.Name.LocalName.EndsWith("Uid"))?.Value;
                if (uid?.StartsWith("Program", StringComparison.Ordinal) != true) continue;
                var property = control.Name.LocalName switch { "TextBlock" => "Text", "ComboBox" or "TextBox" => "Header", _ => "Content" };
                Assert(resources.TryGetValue($"{uid}.{property}", out var text) && !string.IsNullOrWhiteSpace(text),
                    $"Отсутствует {language}: {uid}.{property}");
            }
            Localizer.Language = language;
            Assert("ToolbarUpdatesShort".GetLocalizedResource() == (language == "ru-RU" ? "Обновление" : "Update"), "Кнопка обновления не переименована");
            foreach (var key in new[] { "ProgramPreviewWarning", "ProgramPreviewTitle", "ProgramSetupWelcome", "ProgramSettingsSaved" })
                Assert(!string.IsNullOrWhiteSpace(key.GetLocalizedResource()), $"Отсутствует {language}: {key}");
        }
        return Task.CompletedTask;
    }

    // Включённый поиск предварительных выпусков не меняет статус найденного стабильного выпуска
    private static async Task StableCandidateInPreviewSearchAsync()
    {
        UpdateManager.State.Update = new() { TargetFullRelease = new GitBase<GithubRelease>.GitBaseAsset { Release = new() } };
        await CreateViewModel().UpdatePreviewCommand.ExecuteAsync(null);
        Assert(ContentDialog.Titles.Count == 1 && App.Restarts == 1, "Стабильный выпуск ошибочно назван предварительным");
    }

    // Предварительная семантическая версия требует отдельного согласия независимо от режима поиска
    private static async Task SemanticPreviewNeedsConfirmationAsync()
    {
        UpdateManager.State.Update = new();
        UpdateManager.State.Update.TargetFullRelease.Version.IsPrerelease = true;
        ContentDialog.Answers.Enqueue(true);
        ContentDialog.Answers.Enqueue(false);
        await CreateViewModel().UpdateReleaseCommand.ExecuteAsync(null);
        Assert(ContentDialog.Titles.Count == 2 && UpdateManager.State.Downloads == 0, "Предварительная версия скачивается без предупреждения");
    }

    // Кнопка верхней панели учитывает сохранённое предпочтение предварительных выпусков
    private static async Task ToolbarUsesPreviewPreferenceAsync()
    {
        var shell = new ShellViewModel();
        shell.SettingsService.SaveSetting(TgProgramPreferences.PreferencesKey, new TgProgramPreferences.Values(true, "#0078D4", false, true));
        await Read<IAsyncRelayCommand>(shell, "CheckForUpdatesCommand").ExecuteAsync(new XamlRoot());
        Assert(UpdateManager.State.Preview && UpdateManager.State.Checks == 1, "Верхняя панель игнорирует выбранный источник обновлений");
    }

    // Смена версии открывает настройку повторно, но сохраняет пользовательские параметры
    private static Task ProgramPreferencesLifecycleAsync()
    {
        var settings = new ITgSettingsService { AppLanguage = TgEnumLanguage.Russian, AppTheme = TgEnumTheme.Dark };
        var preferences = new TgProgramPreferences(settings);
        preferences.Load();
        Assert(preferences.NeedsSetup("1.0.0", installed: true) && !preferences.NeedsSetup("1.0.0", installed: false),
            "Первый запуск установленной программы не открывает настройку");
        preferences.AutoStart = true;
        preferences.UseSystemAccent = false;
        preferences.AccentHex = "#123456";
        preferences.IncludePreview = true;
        var startupCalls = 0;
        var accentCalls = 0;
        preferences.Save("1.0.0", enabled => { if (enabled) startupCalls++; },
            (system, hex) => { if (!system && hex == "#123456") accentCalls++; });
        var restored = new TgProgramPreferences(settings);
        restored.Load();
        Assert(startupCalls == 1 && accentCalls == 1 && restored.AutoStart && restored.IncludePreview && restored.AccentHex == "#123456",
            "Параметры не применены или не сохранены");
        Assert(!restored.NeedsSetup("1.0.0", installed: true) && restored.NeedsSetup("1.1.0", installed: true),
            "Настройка повторяется на прежней версии или пропущена после обновления");
        Assert(settings.AppLanguage == TgEnumLanguage.Russian && settings.AppTheme == TgEnumTheme.Dark, "Прежние язык и тема сброшены");
        return Task.CompletedTask;
    }

    // Ошибка автозапуска не должна отмечать настройку завершённой
    private static Task ProgramPreferencesFailedApplyAsync()
    {
        var preferences = new TgProgramPreferences(new());
        var failed = false;
        try { preferences.Save("1.0.0", _ => throw new IOException("Нет доступа"), (_, _) => { }); }
        catch (IOException) { failed = true; }
        Assert(failed && preferences.NeedsSetup("1.0.0", installed: true), "Ошибка применения скрыта или версия отмечена настроенной");
        return Task.CompletedTask;
    }

    // Завершённая загрузка должна освободить сетевое состояние и запросить перезапуск
    private static async Task RestartAfterDownloadAsync()
    {
        UpdateManager.State.Update = new();
        var vm = CreateViewModel();
        App.RestartObserver = () => Assert(!vm.LoadStateService.IsDownloadProcessing, "Перезапуск запрошен до освобождения сетевой операции");
        await vm.UpdateReleaseCommand.ExecuteAsync(null);
        Assert(App.Restarts == 1 && UpdateManager.State.Downloads == 1 && !vm.LoadStateService.IsDownloadProcessing,
            "После загрузки не запрошен перезапуск или состояние скачивания не освобождено");
    }

    // Отказ от найденного предварительного выпуска не должен скачивать пакет
    private static async Task PreviewRejectedAfterDiscoveryAsync()
    {
        UpdateManager.State.Update = new();
        ContentDialog.Answers.Enqueue(true);
        ContentDialog.Answers.Enqueue(false);
        var vm = CreateViewModel();
        await vm.UpdatePreviewCommand.ExecuteAsync(null);
        Assert(UpdateManager.State.Checks == 1 && UpdateManager.State.Downloads == 0 && App.Restarts == 0,
            "Предварительный выпуск загружается без подтверждения найденной версии");
        Assert(ContentDialog.Titles.Last().Contains("0.8.81") && ContentDialog.Contents.Last().Contains("не релиз", StringComparison.OrdinalIgnoreCase),
            "Предупреждение не объясняет статус найденной версии");
    }

    // Обновления открываются общей кнопкой, отдельная страница программы не дублирует настройки
    private static Task ProgramNavigationIconAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("ShellPage.xaml")!;
        var document = XDocument.Load(stream);
        Assert(!document.Descendants().Any(x => x.Name.LocalName == "NavigationViewItem"
            && x.Attributes().Any(a => a.Name.LocalName.EndsWith("Uid") && a.Value == "Shell_Update")), "Остался пункт отдельной страницы программы");
        Assert(document.Descendants().Any(x => x.Name.LocalName == "TgOutlineIcon" && x.Attribute("Kind")?.Value == "app-update"),
            "В верхней панели отсутствует значок обновления");
        return Task.CompletedTask;
    }
}
