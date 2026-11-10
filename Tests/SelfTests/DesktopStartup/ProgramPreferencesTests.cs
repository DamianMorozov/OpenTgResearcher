namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static partial class Program
{
    // Исходная служба должна восстановить параметры из JSON и не записывать черновики путей к данным
    private static Task ProgramPreferencesSerializationAsync()
    {
        TgRuntimeHelper.IsMSIX = false;
        var files = new MemoryFileService("English");
        var settings = new TgSettingsService(files, Options.Create(new LocalSettingsOptions()));
        App.Settings = settings;
        var preferences = new TgProgramPreferences(settings) { AutoStart = true, IncludePreview = true, AccentHex = "#123456", UseSystemAccent = false };
        preferences.Save("1.2.3", _ => { }, (_, _) => { });
        var saved = settings.ReadSetting<TgProgramPreferences.Values>(TgProgramPreferences.PreferencesKey);
        Assert(saved is { AutoStart: true, IncludePreview: true, AccentHex: "#123456", UseSystemAccent: false },
            "Исходная служба настроек не восстановила параметры программы из JSON");
        Assert(!preferences.NeedsSetup("1.2.3", installed: true), "Отметка настроенной версии не сохранена");
        Assert(!files.SavedSettings.ContainsKey(nameof(LocalSettingsOptions.AppStorage)) &&
            !files.SavedSettings.ContainsKey(nameof(LocalSettingsOptions.AppSession)), "Параметры программы сохранили черновики путей к данным");
        Assert(TestLogUtils.Errors.Count == 0, "Сохранение параметров зарегистрировало ошибку");
        return Task.CompletedTask;
    }
}
