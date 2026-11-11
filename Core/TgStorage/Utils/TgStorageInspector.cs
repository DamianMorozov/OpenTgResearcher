namespace TgStorage.Utils;

/// <summary> Независимое чтение и обслуживание SQLite-хранилища </summary>
public sealed class TgStorageInspector(ILogger<TgStorageInspector> logger)
{
    private static readonly string[] Tables = ["APPS", "CHAT_USERS", "DOCUMENTS", "FILTERS", "LICENSES", "MESSAGES",
        "MESSAGES_RELATIONS", "PROXIES", "SOURCES", "STORIES", "USERS", "VERSIONS"];
    private static readonly HashSet<string> Secrets = new(StringComparer.OrdinalIgnoreCase)
        { "API_HASH", "API_ID", "ACCESS_HASH", "PHONE_NUMBER", "PASSWORD", "SECRET", "BOT_TOKEN", "BOT_TOKEN_KEY", "LICENSE_KEY" };

    /// <summary> Таблица и количество записей </summary>
    public sealed record Table(string Name, long Count);
    /// <summary> Страница записей с именами столбцов </summary>
    public sealed record Page(IReadOnlyList<string> Columns, IReadOnlyList<IReadOnlyList<string>> Rows, long Total);
    /// <summary> Сводка базы и резервных копий </summary>
    public sealed record Snapshot(long Bytes, IReadOnlyList<Table> Tables, IReadOnlyList<Backup> Backups);
    /// <summary> Файл резервной копии </summary>
    public sealed record Backup(string Path, long Bytes, DateTime Created);

    /// <summary> Читает сводку без изменения базы </summary>
    public Task<Snapshot> InspectAsync(string path, CancellationToken ct = default) => RunAsync(() =>
    {
        using var connection = Open(path);
        var tables = ReadTables(connection).Select(name => new Table(name, Count(connection, name, "", [], ct))).ToArray();
        var file = new FileInfo(path);
        var prefix = Path.GetFileNameWithoutExtension(path) + "_";
        var backups = Directory.EnumerateFiles(file.DirectoryName!, "*.bak").Where(x => Path.GetFileName(x).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(x => new FileInfo(x)).Select(x => new Backup(x.FullName, x.Length, x.LastWriteTime)).OrderByDescending(x => x.Created).ToArray();
        var wal = new FileInfo(path + "-wal");
        return new Snapshot(file.Length + (wal.Exists ? wal.Length : 0), tables, backups);
    }, ct);

    /// <summary> Читает страницу без секретных столбцов, поиск воспринимается как обычный текст </summary>
    public Task<Page> ReadAsync(string path, string table, string search, int offset, int limit = 50, CancellationToken ct = default) => RunAsync(() =>
    {
        if (offset < 0 || limit is < 1 or > 200) throw new ArgumentOutOfRangeException(nameof(offset), "Недопустимый диапазон записей");
        using var connection = Open(path);
        if (!ReadTables(connection).Contains(table, StringComparer.Ordinal)) throw new ArgumentException("Таблица не найдена", nameof(table));
        using var schema = connection.CreateCommand();
        schema.CommandText = $"PRAGMA table_info({Quote(table)})";
        var columns = new List<string>();
        var keys = new SortedDictionary<int, string>();
        using (var reader = schema.ExecuteReader())
            while (reader.Read())
            {
                var name = reader.GetString(1);
                if (!Secrets.Contains(name) && !reader.GetString(2).Equals("BLOB", StringComparison.OrdinalIgnoreCase)) columns.Add(name);
                if (reader.GetInt32(5) > 0) keys.Add(reader.GetInt32(5), name);
            }
        if (columns.Count == 0) return new Page([], [], 0);
        var total = Count(connection, table, search, columns, ct);
        using var command = connection.CreateCommand();
        var order = keys.Count > 0 ? string.Join(", ", keys.Values.Select(Quote)) : "rowid";
        command.CommandText = $"SELECT {string.Join(", ", columns.Select(Quote))} FROM {Quote(table)}{Where(search, columns)} "
            + $"ORDER BY {order} LIMIT $limit OFFSET $offset";
        command.Parameters.AddWithValue("$search", search);
        command.Parameters.AddWithValue("$limit", limit);
        command.Parameters.AddWithValue("$offset", offset);
        var rows = new List<IReadOnlyList<string>>();
        using var values = command.ExecuteReader();
        while (values.Read())
        {
            ct.ThrowIfCancellationRequested();
            rows.Add(columns.Select((_, i) => values.IsDBNull(i) ? "—" : Convert.ToString(values.GetValue(i), CultureInfo.InvariantCulture) ?? "").ToArray());
        }
        return new Page(columns, rows, total);
    }, ct);

    /// <summary> Создаёт согласованную копию средствами SQLite </summary>
    public Task<string> BackupAsync(string path, CancellationToken ct = default) => RunAsync(() => CopyDatabase(path, NewPath(path, "", ".bak"), ct), ct);

    /// <summary> Удаляет только выбранную резервную копию текущего хранилища </summary>
    public async Task DeleteBackupAsync(string path, string backup, CancellationToken ct = default) => await RunAsync(() =>
    {
        var storagePath = Path.GetFullPath(path);
        var backupPath = Path.GetFullPath(backup);
        var prefix = Path.GetFileNameWithoutExtension(storagePath) + "_";
        if (string.Equals(storagePath, backupPath, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(Path.GetDirectoryName(storagePath), Path.GetDirectoryName(backupPath), StringComparison.OrdinalIgnoreCase)
            || !string.Equals(Path.GetExtension(backupPath), ".bak", StringComparison.OrdinalIgnoreCase)
            || !Path.GetFileName(backupPath).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Можно удалить только резервную копию выбранного хранилища из списка копий", nameof(backup));
        if (!File.Exists(backupPath)) throw new FileNotFoundException("Файл резервной копии не найден. Обновите список копий", backupPath);
        ct.ThrowIfCancellationRequested();
        File.Delete(backupPath);
        return true;
    }, ct);

    /// <summary> Проверяет целостность базы </summary>
    public Task<string> CheckAsync(string path, CancellationToken ct = default) => RunAsync(() => Check(path, ct), ct);

    /// <summary> Восстанавливает отдельный файл, сохраняя исходную базу </summary>
    public Task<string> RestoreAsync(string path, string backup, CancellationToken ct = default) => RunAsync(() =>
    {
        if (Check(backup, ct) != "ok") throw new InvalidDataException("Резервная копия не прошла проверку целостности");
        using (var connection = Open(backup))
            if (!ReadTables(connection).Contains("SOURCES")) throw new InvalidDataException("В копии отсутствует таблица чатов SOURCES");
        return CopyDatabase(backup, NewPath(path, "restored_", ".db"), ct);
    }, ct);

    /// <summary> Освобождает неиспользуемые страницы базы </summary>
    public async Task CompactAsync(string path, CancellationToken ct = default) => await RunAsync(() =>
    {
        using var connection = Open(path, writable: true);
        using var command = connection.CreateCommand();
        command.CommandText = "VACUUM";
        command.ExecuteNonQuery();
        return true;
    }, ct);

    /// <summary> Отключает автоматическое обновление всех чатов </summary>
    public async Task DisableAutoUpdateAsync(string path, CancellationToken ct = default) => await RunAsync(() =>
    {
        using var connection = Open(path, writable: true);
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE SOURCES SET IS_AUTO_UPDATE = 0 WHERE IS_AUTO_UPDATE <> 0";
        command.ExecuteNonQuery();
        return true;
    }, ct);

    // Все операции выполняем вне потока интерфейса, ошибки сохраняем и передаём странице
    private Task<T> RunAsync<T>(Func<T> action, CancellationToken ct) => Task.Run(() =>
    {
        try { ct.ThrowIfCancellationRequested(); return action(); }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { logger.LogError(ex, "Не удалось выполнить операцию с хранилищем"); throw; }
    }, ct);

    // Не создаём отсутствующую базу даже при обслуживании
    private static SqliteConnection Open(string path, bool writable = false)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Файл хранилища не найден. Проверьте профиль в настройках", path);
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = path, Mode = writable ? SqliteOpenMode.ReadWrite : SqliteOpenMode.ReadOnly, Pooling = false, DefaultTimeout = 5,
        }.ToString());
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }

    // Ограничиваем проводник таблицами приложения, служебные таблицы SQLite не показываем
    private static string[] ReadTables(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_schema WHERE type = 'table' ORDER BY name";
        using var reader = command.ExecuteReader();
        var result = new List<string>();
        while (reader.Read()) if (Tables.Contains(reader.GetString(0), StringComparer.Ordinal)) result.Add(reader.GetString(0));
        return result.ToArray();
    }

    // Считаем найденные строки с тем же условием, что и при чтении страницы
    private static long Count(SqliteConnection connection, string table, string search, IReadOnlyList<string> columns, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {Quote(table)}{Where(search, columns)}";
        command.Parameters.AddWithValue("$search", search);
        return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    // Параметр поиска не интерпретируется как SQL или шаблон LIKE
    private static string Where(string search, IReadOnlyList<string> columns) => string.IsNullOrEmpty(search) ? "" : " WHERE "
        + string.Join(" OR ", columns.Select(x => $"instr(lower(CAST({Quote(x)} AS TEXT)), lower($search)) > 0"));

    // Экранируем имена, полученные из схемы самой базы
    private static string Quote(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

    // Делаем уникальные имена копий, в том числе при нескольких операциях в одну секунду
    private static string NewPath(string path, string suffix, string extension) => Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!,
        $"{Path.GetFileNameWithoutExtension(path)}_{suffix}{DateTime.Now:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid().ToString("N")[..8]}{extension}");

    // SQLite учитывает все подтверждённые транзакции, включая ещё не перенесённые из WAL
    private static string CopyDatabase(string source, string target, CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            using var from = Open(source);
            using var to = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = target, Pooling = false }.ToString());
            to.Open();
            from.BackupDatabase(to);
            using var command = to.CreateCommand();
            command.CommandText = "PRAGMA journal_mode=DELETE";
            command.ExecuteScalar();
            return target;
        }
        catch
        {
            if (File.Exists(target)) File.Delete(target);
            throw;
        }
    }

    // Полная проверка не меняет содержимое выбранного файла
    private static string Check(string path, CancellationToken ct)
    {
        using var connection = Open(path);
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA integrity_check";
        using var reader = command.ExecuteReader();
        var messages = new List<string>();
        while (reader.Read()) { ct.ThrowIfCancellationRequested(); messages.Add(reader.GetString(0)); }
        return string.Join(Environment.NewLine, messages);
    }
}
