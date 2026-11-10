namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static partial class Program
{
    // Повторный запуск читает недавние чаты из настоящего файла настроек Desktop
    private static Task RecentChatsSurviveSettingsFileRoundTripAsync()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "recent-roundtrip-" + Guid.NewGuid().ToString("N"));
        var options = Options.Create(new LocalSettingsOptions { ApplicationDataFolder = directory });
        var files = new ProfileDiskFileService();
        try
        {
            var settings = new TgSettingsService(files, options) { AppStorage = Path.Combine(directory, "sample.db") };
            settings.SaveSetting(nameof(LocalSettingsOptions.AppStorage), settings.AppStorage);
            var uid = Guid.NewGuid();
            TgRecentChats.Remember(settings, uid, 100, "Сохранённый чат");
            var reopened = new TgSettingsService(files, options);
            reopened.LoadStorage();
            var recent = TgRecentChats.Read(reopened);
            Assert(recent.Count == 1 && recent[0].Uid == uid && recent[0].Title == "Сохранённый чат" && TestLogUtils.Errors.Count == 0,
                "После повторного чтения файла настроек потеряны недавние чаты или возникла ошибка JSON");
            reopened.AppStorage = Path.Combine(directory, "different.db");
            Assert(TgRecentChats.Read(reopened).Count == 0, "Другая база показывает историю прежнего аккаунта");
        }
        finally
        {
            var path = Path.Combine(directory, "TgLocalSettings.json");
            if (File.Exists(path)) File.Delete(path);
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
        return Task.CompletedTask;
    }
}
