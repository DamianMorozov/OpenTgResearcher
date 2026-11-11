namespace OpenTgResearcherDesktop.UpdateSelfTests;

internal static partial class Program
{
    // Запускаем исходную модель страницы на временной базе с несколькими страницами записей
    private sealed class StorageCase : IDisposable
    {
        public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), "otr-passport-" + Guid.NewGuid());
        public string DatabasePath => Path.Combine(DirectoryPath, "storage.db");
        public LoadStateService State { get; } = new();
        public ITgSettingsService Settings { get; }
        public TgStorageViewModel ViewModel { get; }

        public StorageCase()
        {
            Directory.CreateDirectory(DirectoryPath);
            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={DatabasePath};Pooling=False"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE SOURCES (ID INTEGER PRIMARY KEY, TITLE TEXT, IS_AUTO_UPDATE INTEGER); "
                    + "CREATE TABLE MESSAGES (ID INTEGER PRIMARY KEY, MESSAGE TEXT); "
                    + "WITH RECURSIVE n(x) AS (SELECT 1 UNION ALL SELECT x+1 FROM n WHERE x < 123) "
                    + "INSERT INTO SOURCES SELECT x, 'Chat ' || x, 1 FROM n; INSERT INTO MESSAGES VALUES (1, 'Message text');";
                command.ExecuteNonQuery();
            }
            Settings = new() { AppStorage = DatabasePath, AppSession = Path.Combine(DirectoryPath, "session") };
            _ = new TgAccountProfiles(Settings, "Test profile");
            State.SetIsDisplaySensitiveData(isDisplaySensitiveData: true);
            ViewModel = new(State, Settings, new(), null!);
        }

        public void Dispose()
        {
            ViewModel.Dispose();
            State.Dispose();
            Directory.Delete(DirectoryPath, recursive: true);
        }
    }

    // Команды должны менять реально отображаемые строки и границы страниц без обращения к Telegram
    private static async Task StoragePassportCommandsAsync()
    {
        using var test = new StorageCase();
        var vm = test.ViewModel;
        await vm.OnNavigatedToAsync(null);
        Assert(vm.Tables.First().Name == "SOURCES", "Каталог не начинается с чатов, выбранная таблица скрыта ниже списка");
        Assert(vm.ProfileName == "Test profile" && vm.ChatCount == "123" && vm.MessageCount == "1", "Паспорт не прочитал профиль и реальные счётчики");
        Assert(vm.Records.Count == 50 && vm.SelectedRecord?.Title == "Chat 1", "Не загружена первая страница чатов");
        Assert(vm.TelegramChecks == 0 && vm.Errors.Count == 0, "Паспорт вызвал Telegram или скрыл ошибку");
        await vm.NextCommand.ExecuteAsync(null);
        Assert(vm.Records.Count == 50 && vm.SelectedRecord?.Id == "51", "Не загружена вторая страница");
        await vm.NextCommand.ExecuteAsync(null);
        Assert(vm.Records.Count == 23 && vm.SelectedRecord?.Id == "101" && !vm.NextCommand.CanExecute(null), "Неверна последняя страница");
        await vm.PreviousCommand.ExecuteAsync(null);
        Assert(vm.SelectedRecord?.Id == "51", "Не работает переход назад");
        vm.Search = "Chat 123";
        await vm.SearchCommand.ExecuteAsync(null);
        Assert(vm.Records.Single().Id == "123" && !vm.PreviousCommand.CanExecute(null), "Поиск не сбросил смещение");
        vm.Search = "absent";
        await vm.SearchCommand.ExecuteAsync(null);
        Assert(!vm.HasRecords && vm.SelectedRecord is null, "Пустая выборка оставила прежнюю карточку");
        vm.SelectedTable = vm.Tables.Single(x => x.Name == "MESSAGES");
        vm.Search = "";
        await vm.LoadRecordsAsync();
        Assert(vm.Records.Single().Fields.Any(x => x.Value == "Message text"), "Нет полей сообщения в карточке");
        await vm.ReloadUiAsync();
        Assert(vm.SelectedTable.Name == "MESSAGES" && vm.Records.Count == 1, "Обновление сбросило выбранную таблицу");
    }

    // Удаляем только подтверждённую копию и сразу обновляем список, выбор и паспорт
    private static async Task StorageBackupDeletionAsync()
    {
        using var test = new StorageCase();
        var vm = test.ViewModel;
        await vm.ReloadUiAsync();
        var command = vm.DeleteBackupCommand;
        Assert(!command.CanExecute(null), "Удаление доступно без выбранной копии");
        await vm.CreateBackupCommand.ExecuteAsync(null);
        await vm.CreateBackupCommand.ExecuteAsync(null);
        var selected = vm.Backups.Last();
        var remaining = vm.Backups.First();
        vm.SelectedBackup = selected;
        Assert(command.CanExecute(null), "Удаление недоступно при выборе копии");
        test.State.SetIsDisplaySensitiveData(isDisplaySensitiveData: false);
        vm.NotifyPrivacyChanged();
        Assert(!command.CanExecute(null), "Удаление доступно при скрытом выборе копии");
        test.State.SetIsDisplaySensitiveData(isDisplaySensitiveData: true);
        vm.NotifyPrivacyChanged();
        ContentDialog.Confirm = false;
        await command.ExecuteAsync(null);
        Assert(File.Exists(selected.Path) && vm.Backups.Count == 2, "Отмена подтверждения удалила копию");
        Assert(ContentDialog.Contents.Last().Contains(selected.Name), "Подтверждение не показывает имя выбранной копии");
        ContentDialog.Confirm = true;
        await command.ExecuteAsync(null);
        Assert(!File.Exists(selected.Path) && File.Exists(remaining.Path), "Удалена другая копия или выбранный файл остался");
        Assert(vm.Backups.Single().Path == remaining.Path && vm.SelectedBackup?.Path == remaining.Path, "Список и выбор не обновлены");
        await command.ExecuteAsync(null);
        Assert(vm.Backups.Count == 0 && vm.SelectedBackup is null && !vm.HasBackups && !command.CanExecute(null), "Последняя копия осталась в интерфейсе");
        Assert(vm.LastBackup == "Копий пока нет" && vm.Message == "Резервная копия удалена.", "Паспорт или результат удаления не обновлены");
        Assert(File.Exists(test.DatabasePath) && vm.Errors.Count == 0, "Удаление повредило базу или вызвало ошибку");
        await vm.CreateBackupCommand.ExecuteAsync(null);
        var lockedBackup = vm.SelectedBackup!;
        if (OperatingSystem.IsWindows())
        {
            using var locked = new FileStream(lockedBackup.Path, FileMode.Open, FileAccess.Read, FileShare.None);
            await command.ExecuteAsync(null);
            Assert(File.Exists(lockedBackup.Path) && vm.Backups.Count == 1 && vm.Errors.Count == 1
                && vm.Message.Contains("Не удалось выполнить операцию"), "Ошибка удаления скрыта или показан успех");
        }
        Localizer.Language = "en-US";
        await command.ExecuteAsync(null);
        Assert(!File.Exists(lockedBackup.Path) && vm.Message == "Backup deleted." && ContentDialog.Titles.Last() == "Delete backup",
            "Удаление или подтверждение не локализовано на английский");
        using var markup = Assembly.GetExecutingAssembly().GetManifestResourceStream("TgStoragePage.xaml")!;
        Assert(XDocument.Load(markup).Descendants().Any(x => x.Name.LocalName == "Button"
            && (string?)x.Attribute("Command") == "{x:Bind ViewModel.DeleteBackupCommand}"), "Команда не связана с кнопкой");
    }

    // Отказ от подтверждения и активный парсинг не должны менять базу, восстановление сохраняет исходную
    private static async Task StorageMaintenanceAndRestoreAsync()
    {
        using var test = new StorageCase();
        var vm = test.ViewModel;
        await vm.ReloadUiAsync();
        ContentDialog.Confirm = false;
        await vm.CompactCommand.ExecuteAsync(null);
        Assert(Directory.GetFiles(test.DirectoryPath, "*.bak").Length == 0, "Копия создана до подтверждения");
        ContentDialog.Confirm = true;
        var parsing = Guid.NewGuid();
        test.State.NetworkActivity.Start(parsing, TgNetworkOperationKind.Parsing);
        await vm.DisableAutoUpdateCommand.ExecuteAsync(null);
        Assert(vm.Errors.Count == 1 && vm.Message.Contains("Сначала завершите"), "Обслуживание допустило активный парсинг");
        test.State.NetworkActivity.Finish(parsing, TgNetworkOperationState.Completed);
        await vm.DisableAutoUpdateCommand.ExecuteAsync(null);
        var reader = new TgStorageInspector(Microsoft.Extensions.Logging.Abstractions.NullLogger<TgStorageInspector>.Instance);
        var flags = await reader.ReadAsync(test.DatabasePath, "SOURCES", "", 0);
        Assert(flags.Rows.All(x => x[2] == "0") && vm.Backups.Count == 1, "Флаги не сброшены или нет копии перед изменением");
        vm.BackupBeforeMaintenance = false;
        Assert(test.Settings.ReadSetting<bool?>("StorageBackupBeforeMaintenance") == false, "Настройка копии не сохранена");
        await vm.CheckCommand.ExecuteAsync(null);
        Assert(vm.Message.Contains("целостность SQLite не нарушена"), "Нет результата проверки целостности");
        await vm.RestoreCommand.ExecuteAsync(null);
        var profile = TgAccountProfiles.ReadActive(test.Settings)!;
        Assert(App.Restarts == 1 && profile.StoragePath != test.DatabasePath && File.Exists(profile.StoragePath), "Восстановленный файл не применён профилем");
        Assert(File.Exists(test.DatabasePath) && (await reader.ReadAsync(test.DatabasePath, "SOURCES", "", 0)).Rows.All(x => x[2] == "0"),
            "Восстановление перезаписало текущую базу");
        Assert((await reader.ReadAsync(profile.StoragePath, "SOURCES", "", 0)).Rows.All(x => x[2] == "1"), "Восстановлена не выбранная копия");
    }

    // Приватность меняется без переоткрытия, все подписи доступны в обоих языках
    private static async Task StoragePrivacyAndLocalizationAsync()
    {
        using var test = new StorageCase();
        var vm = test.ViewModel;
        await vm.ReloadUiAsync();
        test.State.SetIsDisplaySensitiveData(isDisplaySensitiveData: false);
        vm.NotifyPrivacyChanged();
        Assert(vm.StoragePath == test.State.SensitiveData && vm.ChatCount == test.State.SensitiveData, "Скрытие данных оставило путь или счётчик открытым");
        using var markup = Assembly.GetExecutingAssembly().GetManifestResourceStream("TgStoragePage.xaml")!;
        var page = XDocument.Load(markup);
        Assert(page.Descendants().Any(x => x.Name.LocalName == "Grid"
            && ((string?)x.Attribute("Visibility"))?.Contains("ViewModel.IsDisplaySensitiveData") == true), "Проводник раскрывает записи при скрытых данных");
        foreach (var language in new[] { "ru-RU", "en-US" })
        {
            Localizer.Language = language;
            await vm.ReloadUiAsync();
            Assert(vm.Tables.All(x => !string.IsNullOrWhiteSpace(x.Title)), "Не переведены таблицы");
            var resources = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Strings", language, "Resources.resw"));
            var keys = resources.Root!.Elements("data").Where(x => x.Attribute("name")!.Value.StartsWith("Storage", StringComparison.Ordinal))
                .ToDictionary(x => x.Attribute("name")!.Value, x => x.Element("value")!.Value);
            foreach (var element in page.Descendants())
            {
                var uid = element.Attributes().FirstOrDefault(x => x.Name.LocalName == "Uid")?.Value;
                if (uid is null) continue;
                var suffix = element.Name.LocalName switch { "TextBlock" => ".Text", "SelectorBarItem" => ".Text", "AutoSuggestBox" => ".PlaceholderText", _ => ".Content" };
                Assert(keys.TryGetValue(uid + suffix, out var value) && !string.IsNullOrWhiteSpace(value), $"Нет строки {language}: {uid + suffix}");
            }
        }
    }
}
