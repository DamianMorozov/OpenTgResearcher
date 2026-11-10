namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static partial class Program
{
    // Перезапуск службы читает настоящий файл тем же файловым сервисом, что и Desktop
    private static Task ProfilesSurviveSettingsFileRoundTripAsync()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "profile-roundtrip-" + Guid.NewGuid().ToString("N"));
        var options = Options.Create(new LocalSettingsOptions { ApplicationDataFolder = directory });
        var files = new ProfileDiskFileService();
        try
        {
            var settings = new TgSettingsService(files, options);
            var catalog = new TgAccountProfiles(settings, "Профиль по умолчанию");
            var first = catalog.Profiles[0];
            first.Name = "Основной изменённый";
            first.StoragePath = Path.Combine(directory, "first.db");
            first.SessionPath = Path.Combine(directory, "first.session");
            var second = catalog.Create("Второй", Path.Combine(directory, "second.db"), Path.Combine(directory, "second.session"));
            catalog.Save(second.Id);
            var expected = settings.ReadSetting<TgAccountProfiles.State>(TgAccountProfiles.SettingsKey)!;
            Assert(File.Exists(settings.SettingFile), "Сохранение не создало файл настроек");
            foreach (var useJsonObject in new[] { false, true })
            {
                if (useJsonObject)
                    files.Save(directory, Path.GetFileName(settings.SettingFile), new Dictionary<string, object>
                        { [TgAccountProfiles.SettingsKey] = JsonSerializer.SerializeToElement(expected) });
                var reopened = new TgSettingsService(files, options);
                var restored = new TgAccountProfiles(reopened, "Профиль по умолчанию");
                Assert(restored.Profiles.Count == 2 && restored.ActiveProfileId == second.Id,
                    "После чтения файла настроек потеряны созданный профиль или выбранный аккаунт");
                foreach (var profile in expected.Profiles)
                {
                    var actual = restored.Profiles.Single(x => x.Id == profile.Id);
                    Assert(actual.Name == profile.Name && actual.StoragePath == profile.StoragePath && actual.SessionPath == profile.SessionPath,
                        "Повторное чтение файла потеряло название, хранилище или сессию профиля");
                }
                App.Settings = reopened;
                reopened.Load();
                Assert(reopened.AppStorage == second.StoragePath && reopened.AppSession == second.SessionPath,
                    "Запуск использует другую пару файлов вместо сохранённого профиля");
                var inactive = restored.Profiles.Single(x => x.Id == first.Id);
                inactive.Name = "Неактивный изменённый";
                inactive.StoragePath = Path.Combine(directory, "edited.db");
                inactive.SessionPath = Path.Combine(directory, "edited.session");
                restored.Save(restored.ActiveProfileId);
                var next = new TgSettingsService(files, options);
                var nextCatalog = new TgAccountProfiles(next, "Профиль по умолчанию");
                var edited = nextCatalog.Profiles.Single(x => x.Id == first.Id);
                Assert(nextCatalog.ActiveProfileId == second.Id && edited.Name == inactive.Name
                    && edited.StoragePath == inactive.StoragePath && edited.SessionPath == inactive.SessionPath,
                    "Изменения неактивного профиля потеряны при следующем запуске или сменили аккаунт");
                Assert(TestLogUtils.Errors.Count == 0, "Чтение профилей записало исключение в журнал");
            }
        }
        finally
        {
            var path = Path.Combine(directory, "TgLocalSettings.json");
            if (File.Exists(path)) File.Delete(path);
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
        return Task.CompletedTask;
    }

    // Ошибка записи каталога возвращается команде и не выбирает новый аккаунт в памяти
    private static Task ProfileSaveFailureDoesNotActivateAsync()
    {
        var files = new ProfileWriteFileService();
        var settings = new TgSettingsService(files, Options.Create(new LocalSettingsOptions()));
        var profiles = new TgAccountProfiles(settings, "Профиль по умолчанию");
        var original = profiles.ActiveProfileId;
        var second = profiles.Create("Другой", Path.Combine(AppContext.BaseDirectory, "other.db"), Path.Combine(AppContext.BaseDirectory, "other.session"));
        files.FailWrites = true;
        var failed = false;
        try { profiles.Save(second.Id); }
        catch (IOException) { failed = true; }
        Assert(failed && profiles.ActiveProfileId == original && TgAccountProfiles.ReadActive(settings)!.Id == original,
            "Ошибка записи профиля скрыта или новый аккаунт выбран без сохранения на диск");
        return Task.CompletedTask;
    }

    // Недоступный каталог профиля не должен подменять базу другого аккаунта базой по умолчанию
    private static Task MissingProfileDirectoryKeepsAccountPathsAsync()
    {
        var files = new AppearanceFileService();
        var settings = new TgSettingsService(files, Options.Create(new LocalSettingsOptions()));
        App.Settings = settings;
        var directory = Path.Combine(AppContext.BaseDirectory, "missing-profile", Guid.NewGuid().ToString("N"));
        var id = Guid.NewGuid();
        var value = new TgAccountProfiles.Values(id, "Недоступный", Path.Combine(directory, "missing.db"), Path.Combine(directory, "missing.session"));
        settings.SaveSetting(TgAccountProfiles.SettingsKey, new TgAccountProfiles.State(id, [value]));
        settings.Load();
        Assert(settings.AppStorage == value.StoragePath && settings.AppSession == value.SessionPath,
            "Недоступная база профиля подменена базой по умолчанию при сохранении чужой сессии");
        return Task.CompletedTask;
    }

    // Настоящая служба читает обе составляющие профиля при новом запуске, старые ключи не подменяют выбранный аккаунт
    private static Task ProfilesRestoreStorageAndSessionAsync()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "profiles", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var files = new AppearanceFileService();
            var settings = new TgSettingsService(files, Options.Create(new LocalSettingsOptions()));
            App.Settings = settings;
            settings.AppStorage = Path.Combine(directory, "first.db");
            settings.AppSession = Path.Combine(directory, "first.session");
            settings.Save();
            var profiles = new TgAccountProfiles(settings, "Основной");
            var profile = profiles.Create("Другой аккаунт", Path.Combine(directory, "second.db"), Path.Combine(directory, "second.session"));
            profiles.Save(profile.Id);
            var restored = new TgSettingsService(files, Options.Create(new LocalSettingsOptions()));
            App.Settings = restored;
            restored.Load();
            Assert(restored.AppStorage == profile.StoragePath && restored.AppSession == profile.SessionPath,
                "Запуск смешал базу и сессию из разных профилей или использовал старые ключи");
            Assert(!File.Exists(profile.StoragePath) && !File.Exists(profile.SessionPath), "Чтение профиля создало или перезаписало файлы аккаунта");
        }
        finally { Directory.Delete(directory); }
        return Task.CompletedTask;
    }

    // Исходный обработчик сначала освобождает прежнюю сессию, затем сохраняет профиль и запрашивает перезапуск
    private static async Task ProfileRestartWaitsForSessionAsync()
    {
        var client = new ParsingStopConnection();
        App.BusinessLogicManager = new() { ConnectClient = client };
        var committed = false;
        var restarted = false;
        AppInstance.RestartHandler = () =>
        {
            Assert(client.DisposeCalls == 1 && committed, "Перезапуск начался до освобождения сессии и сохранения профиля");
            restarted = true;
            return AppRestartFailureReason.RestartPending;
        };
        try
        {
            var close = App.RestartForProfileAsync(() => committed = true, () => committed = false);
            await client.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));
            Assert(!committed && !restarted && App.MainWindow.CloseCalls == 0, "Профиль применён до окончания отключения Telegram");
            client.Finished.TrySetResult();
            await close.WaitAsync(TimeSpan.FromSeconds(2));
            Assert(committed && restarted && App.MainWindow.CloseCalls == 1, "Не завершено применение профиля и закрытие старого окна");
        }
        finally { client.Finished.TrySetResult(); AppInstance.RestartHandler = null; }
    }

    // Отказ Windows в перезапуске восстанавливает прежний профиль и оставляет окно для сообщения об ошибке
    private static async Task ProfileRestartFailureRestoresSelectionAsync()
    {
        var client = new ParsingStopConnection();
        client.Finished.TrySetResult();
        App.BusinessLogicManager = new() { ConnectClient = client };
        var committed = false;
        var restored = false;
        AppInstance.RestartHandler = () => AppRestartFailureReason.Other;
        var failed = false;
        try { await App.RestartForProfileAsync(() => committed = true, () => { committed = false; restored = true; }); }
        catch (InvalidOperationException) { failed = true; }
        finally { AppInstance.RestartHandler = null; }
        Assert(failed && restored && !committed && !App.IsClosing && App.MainWindow.CloseCalls == 0,
            "Неудачный перезапуск закрыл окно или оставил выбранным неприменённый профиль");
    }

    // Подключаем исходный файловый сервис к платформенному окружению проверки
    private sealed class ProfileDiskFileService : IFileService
    {
        private readonly OpenTgResearcherDesktopCore.Services.FileService _files = new();
        /// <summary> Читает настоящий файл настроек </summary>
        public T Read<T>(string folderPath, string fileName) => _files.Read<T>(folderPath, fileName);
        /// <summary> Записывает настоящий файл настроек </summary>
        public void Save<T>(string folderPath, string fileName, T content) => _files.Save(folderPath, fileName, content);
    }

    // Проверяем обработку ошибки записи без файлов пользователя
    private sealed class ProfileWriteFileService : IFileService
    {
        private Dictionary<string, object> _values = [];
        public bool FailWrites { get; set; }
        /// <summary> Возвращает сохранённый снимок </summary>
        public T Read<T>(string folderPath, string fileName) => (T)(object)new Dictionary<string, object>(_values);
        /// <summary> Имитирует отсутствие доступа при сохранении каталога </summary>
        public void Save<T>(string folderPath, string fileName, T content)
        {
            if (FailWrites) throw new IOException("Нет доступа к записи профилей");
            _values = new Dictionary<string, object>((IDictionary<string, object>)(object)content!);
        }
    }
}
