namespace OpenTgResearcherDesktop.UpdateSelfTests;

internal static partial class Program
{
    // Возвращение в настройки сохраняет уже запущенную загрузку, её прогресс и журнал
    private static async Task SettingsSharesUpdateOperationAsync()
    {
        UpdateManager.State.Update = new();
        UpdateManager.State.DownloadGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var updates = CreateViewModel();
        using var settings = CreateSettingsViewModel(updates: updates);
        var loading = updates.UpdateReleaseCommand.ExecuteAsync(null);
        try
        {
            await WaitAsync(() => UpdateManager.State.Downloads == 1);
            await settings.OnNavigatedToAsync(null);
            settings.ShowUpdates();
            Assert(ReferenceEquals(settings.Updates, updates) && settings.Updates.IsUpdating && settings.Updates.DownloadProgress == 37,
                "Настройки создали отдельную загрузку или потеряли прогресс");
            Assert(settings.Updates.UpdateLog.Length > 0 && !loading.IsCompleted && settings.IsUpdatesExpanded,
                "Переход к настройкам потерял журнал или завершил ещё выполняемую загрузку");
        }
        finally { UpdateManager.State.DownloadGate.TrySetResult(); await loading; }
        Assert(settings.Updates.DownloadProgress == 100 && !settings.Updates.IsUpdating, "Настройки не получили результат общей загрузки");
    }

    // Повторный запрос прокрутки не перезагружает и не сбрасывает редактируемые настройки
    private static Task RepeatedUpdateRequestKeepsDraftAsync()
    {
        using var settings = CreateSettingsViewModel();
        settings.Preferences.AccentHex = "#123456";
        var requests = 0;
        settings.UpdatesRequested += (_, _) => requests++;
        settings.ShowUpdates();
        settings.ShowUpdates();
        Assert(requests == 2 && settings.IsUpdatesExpanded && settings.Preferences.AccentHex == "#123456",
            "Повторное открытие обновлений не прокрутило блок или сбросило черновик");
        return Task.CompletedTask;
    }

    // Создаём настоящие модели с общими зависимостями без доступа к настройкам пользователя
    private static TgSettingsViewModel CreateSettingsViewModel(ITgSettingsService? settings = null, TgUpdateViewModel? updates = null)
    {
        settings ??= new();
        var state = new LoadStateService();
        var navigation = new INavigationService();
        updates ??= new TgUpdateViewModel(state, settings, navigation, null!);
        return new TgSettingsViewModel(state, settings, navigation, null!, updates);
    }

    // Общая страница содержит команды обновления, прогресс, отмену и журнал той же модели обновлений
    private static Task SettingsContainsUpdatesAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgSettingsPage.xaml")!;
        var page = XDocument.Load(stream);
        var updates = page.Descendants().SingleOrDefault(x => x.Attribute(XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml"))?.Value
            == "UpdatesSection");
        Assert(updates is not null, "В настройках отсутствует блок обновлений");
        foreach (var command in new[] { "UpdateReleaseCommand", "UpdatePreviewCommand", "CancelUpdateCommand" })
            Assert(updates!.Descendants().Any(x => x.Attribute("Command")?.Value == $"{{x:Bind ViewModel.Updates.{command}}}"),
                $"Блок настроек не подключил общую команду {command}");
        Assert(updates!.Descendants().Any(x => x.Attribute("Text")?.Value.Contains("ViewModel.Updates.UpdateLog") == true), "В настройках нет журнала");
        Assert(updates.Descendants().Any(x => x.Attribute("Value")?.Value.Contains("ViewModel.Updates.DownloadProgress") == true), "В настройках нет прогресса");
        Assert(!updates.Descendants().Any(x => x.Name.LocalName == "CheckBox"
            && x.Attribute("IsChecked")?.Value.Contains("IncludePreview") == true), "В настройках остался чекбокс предварительных выпусков");
        foreach (var language in new[] { "ru-RU", "en-US" })
        {
            Localizer.Language = language;
            Assert("ActionUpdateReleaseApp".GetLocalizedResource() == (language == "ru-RU" ? "Проверка обновления" : "Check for updates"),
                "Кнопка проверки обновления использует старую подпись");
            Assert("ActionUpdatePreviewApp".GetLocalizedResource() == (language == "ru-RU" ? "Проверка предварительного выпуска" : "Check preview release"),
                "Кнопка проверки предварительного выпуска использует старую подпись");
        }
        return Task.CompletedTask;
    }

    // Основные действия остаются над прокруткой, а второстепенные секции свёрнуты
    private static Task SettingsHasCompactSectionsAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgSettingsPage.xaml")!;
        var page = XDocument.Load(stream);
        var save = page.Descendants().Single(x => x.Attribute("Command")?.Value.Contains("SettingsSaveCommand") == true);
        Assert(!save.Ancestors().Any(x => x.Name.LocalName is "ScrollViewer" or "ScrollView"), "Сохранение скрывается при прокрутке настроек");
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        foreach (var name in new[] { "SettingsFilesSection", "SettingsDetailsSection" })
            Assert(page.Descendants().SingleOrDefault(x => x.Attribute(xaml + "Name")?.Value == name)?.Attribute("IsExpanded")?.Value == "False",
                $"Секция {name} занимает место до раскрытия");
        return Task.CompletedTask;
    }

    // Верхняя панель использует короткую подпись обновления на обоих языках
    private static Task ToolbarUpdateCaptionAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("ShellPage.xaml")!;
        var page = XDocument.Load(stream);
        var caption = page.Descendants().Single(x => x.Attribute(XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml"))?.Value == "ToolbarUpdateLabel");
        Assert(caption.Attributes().Any(x => x.Name.LocalName.EndsWith("Uid") && x.Value == "ToolbarUpdatesShort"), "Кнопка не использует новую подпись");
        foreach (var language in new[] { "ru-RU", "en-US" })
        {
            Localizer.Language = language;
            Assert("ToolbarUpdatesShort".GetLocalizedResource() == (language == "ru-RU" ? "Обновление" : "Update"),
                "На верхней панели осталось название программы вместо обновления");
        }
        return Task.CompletedTask;
    }
}
