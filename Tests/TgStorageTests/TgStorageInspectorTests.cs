namespace TgStorageTests;

// Проверяем проводник и обслуживание на отдельной базе, включая незаписанный в основной файл журнал WAL
[TestFixture]
public sealed class TgStorageInspectorTests
{
    private string _directory = string.Empty;
    private string _path = string.Empty;
    private SqliteConnection _writer = default!;
    private readonly TgStorageInspector _service = new(Microsoft.Extensions.Logging.Abstractions.NullLogger<TgStorageInspector>.Instance);

    [SetUp]
    public void Setup()
    {
        _directory = Path.Combine(Path.GetTempPath(), "otr-storage-" + Guid.NewGuid());
        Directory.CreateDirectory(_directory);
        _path = Path.Combine(_directory, "storage.db");
        _writer = new SqliteConnection($"Data Source={_path};Pooling=False");
        _writer.Open();
        Execute("PRAGMA journal_mode=WAL; PRAGMA wal_autocheckpoint=0; "
            + "CREATE TABLE SOURCES (UID TEXT PRIMARY KEY, ID INTEGER, TITLE TEXT, IS_AUTO_UPDATE INTEGER); "
            + "CREATE TABLE CHAT_USERS (ID INTEGER); CREATE TABLE APPS (ID INTEGER, API_HASH TEXT, PHONE_NUMBER TEXT); "
            + "INSERT INTO APPS VALUES (1, 'private-hash', 'private-phone'); "
            + "INSERT INTO SOURCES VALUES ('b', 2, 'Beta', 1), ('a', 1, 'Alpha', 1), ('c', 3, '100%', 0);");
    }

    [TearDown]
    public void Cleanup()
    {
        _writer.Dispose();
        Directory.Delete(_directory, recursive: true);
    }

    [Test]
    public void Storage_page_opens_passport_and_has_a_data_section()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Directory.Build.props"))) directory = directory.Parent;
        Assert.That(directory, Is.Not.Null);
        var xaml = System.Xml.Linq.XDocument.Load(Path.Combine(directory!.FullName,
            "Presentation", "OpenTgResearcherDesktop", "Views", "TgStoragePage.xaml"));
        var tabs = xaml.Descendants().Where(x => x.Name.LocalName == "SelectorBarItem").ToArray();
        Assert.That(tabs.Select(x => (string?)x.Attribute("Tag")), Is.EqualTo(new[] { "Overview", "Data", "Backups", "Maintenance" }));
        Assert.That((string?)tabs[0].Attribute("IsSelected"), Is.EqualTo("True"));
        Assert.That(xaml.Descendants().Any(x => (string?)x.Attribute("ItemsSource") == "{x:Bind ViewModel.Records, Mode=OneWay}"), Is.True);
    }

    [Test]
    public async Task Snapshot_includes_all_tables_and_live_counts()
    {
        var result = await _service.InspectAsync(_path);
        Assert.That(result.Tables.Single(x => x.Name == "SOURCES").Count, Is.EqualTo(3));
        Assert.That(result.Tables.Any(x => x.Name == "CHAT_USERS"), Is.True);
        Assert.That(result.Bytes, Is.GreaterThan(0));
        Assert.That(result.Backups, Is.Empty);
    }

    [Test]
    public async Task Explorer_pages_orders_and_searches_literal_text_without_secrets()
    {
        var first = await _service.ReadAsync(_path, "SOURCES", "", 0, 2);
        var next = await _service.ReadAsync(_path, "SOURCES", "", 2, 2);
        Assert.That(first.Total, Is.EqualTo(3));
        Assert.That(first.Rows.Count, Is.EqualTo(2));
        Assert.That(next.Rows.Count, Is.EqualTo(1));
        Assert.That(first.Rows[0][0], Is.EqualTo("a"));
        var search = await _service.ReadAsync(_path, "SOURCES", "%", 0);
        Assert.That(search.Total, Is.EqualTo(1));
        Assert.That(search.Rows.Single()[2], Is.EqualTo("100%"));
        var apps = await _service.ReadAsync(_path, "APPS", "", 0);
        Assert.That(apps.Columns, Is.EqualTo(new[] { "ID" }));
        Assert.That(apps.Rows.Single().Single(), Is.EqualTo("1"));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ReadAsync(_path, "SOURCES; DROP TABLE APPS", "", 0));
    }

    [Test]
    public async Task Backup_keeps_WAL_and_restore_preserves_current_database()
    {
        var backup = await _service.BackupAsync(_path);
        Execute("INSERT INTO SOURCES VALUES ('d', 4, 'Delta', 1)");
        var restored = await _service.RestoreAsync(_path, backup);
        Assert.That(restored, Is.Not.EqualTo(_path));
        Assert.That((await _service.ReadAsync(restored, "SOURCES", "", 0)).Total, Is.EqualTo(3));
        Assert.That((await _service.ReadAsync(_path, "SOURCES", "", 0)).Total, Is.EqualTo(4));
        Assert.That(await _service.CheckAsync(backup), Is.EqualTo("ok"));
        Assert.That((await _service.InspectAsync(_path)).Backups.Single().Path, Is.EqualTo(backup));
    }

    [Test]
    public async Task Delete_removes_only_selected_backup_and_preserves_database()
    {
        var first = await _service.BackupAsync(_path);
        var second = await _service.BackupAsync(_path);
        await DeleteBackupAsync(first);
        Assert.That(File.Exists(first), Is.False);
        Assert.That(File.Exists(second), Is.True);
        Assert.That((await _service.InspectAsync(_path)).Backups.Single().Path, Is.EqualTo(second));
        Assert.That((await _service.ReadAsync(_path, "SOURCES", "", 0)).Total, Is.EqualTo(3));
    }

    [Test]
    public async Task Delete_rejects_database_unrelated_files_and_missing_backups()
    {
        var unrelated = Path.Combine(_directory, "another_backup.bak");
        await File.WriteAllTextAsync(unrelated, "unrelated file");
        await Assert.ThrowsAsync<ArgumentException>(() => DeleteBackupAsync(_path));
        await Assert.ThrowsAsync<ArgumentException>(() => DeleteBackupAsync(unrelated));
        Assert.That(File.Exists(unrelated), Is.True);
        var outsideDirectory = Path.Combine(_directory, "other");
        Directory.CreateDirectory(outsideDirectory);
        var outside = Path.Combine(outsideDirectory, "storage_copy.bak");
        await File.WriteAllTextAsync(outside, "another folder");
        await Assert.ThrowsAsync<ArgumentException>(() => DeleteBackupAsync(outside));
        Assert.That(File.Exists(outside), Is.True);
        await Assert.ThrowsAsync<FileNotFoundException>(() => DeleteBackupAsync(Path.Combine(_directory, "storage_missing.bak")));
        Assert.That((await _service.ReadAsync(_path, "SOURCES", "", 0)).Total, Is.EqualTo(3));
    }

    // Передаём в удаление путь текущего временного хранилища
    private Task DeleteBackupAsync(string backup) => _service.DeleteBackupAsync(_path, backup);

    [Test]
    public async Task Missing_or_invalid_files_are_not_created_or_restored()
    {
        var missing = Path.Combine(_directory, "absent.db");
        await Assert.ThrowsAsync<FileNotFoundException>(() => _service.InspectAsync(missing));
        Assert.That(File.Exists(missing), Is.False);
        var invalid = Path.Combine(_directory, "invalid.bak");
        await File.WriteAllTextAsync(invalid, "broken database");
        await Assert.ThrowsAsync<SqliteException>(() => _service.RestoreAsync(_path, invalid));
        Assert.That(Directory.GetFiles(_directory, "*restored*"), Is.Empty);
        Assert.That((await _service.ReadAsync(_path, "SOURCES", "", 0)).Total, Is.EqualTo(3));
    }

    [Test]
    public async Task Maintenance_changes_flags_and_leaves_a_valid_database()
    {
        await _service.DisableAutoUpdateAsync(_path);
        var result = await _service.ReadAsync(_path, "SOURCES", "", 0);
        Assert.That(result.Rows.All(x => x[3] == "0"), Is.True);
        await _service.CompactAsync(_path);
        Assert.That(await _service.CheckAsync(_path), Is.EqualTo("ok"));
    }

    // Выполняем подготовку данных в открытом соединении, удерживающем журнал WAL
    private void Execute(string sql)
    {
        using var command = _writer.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
