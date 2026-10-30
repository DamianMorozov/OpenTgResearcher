namespace OpenTgResearcherDesktop.ConnectionSelfTests;

internal static partial class Program
{
    // Проверяем расшифровку временной копии штатным файловым хранилищем WTelegram без подключения к Telegram
    private static async Task<int> InspectSessionAsync(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Требуются путь БД и путь сессии: --inspect-session <database> <session>");
            return 1;
        }
        WTelegram.Helpers.Log = (_, _) => { };
        var temporaryDirectory = Path.Combine(AppContext.BaseDirectory, "session-inspection", Guid.NewGuid().ToString("N"));
        var temporaryPath = Path.Combine(temporaryDirectory, "test.session");
        try
        {
            var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
                { DataSource = Path.GetFullPath(args[1]), Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly, Pooling = false }.ToString();
            await using var database = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            await database.OpenAsync();
            await using var query = database.CreateCommand();
            query.CommandText = "SELECT a.API_ID, a.API_HASH FROM APPS a WHERE a.UID <> $empty LIMIT 1";
            query.Parameters.AddWithValue("$empty", Guid.Empty);
            await using var reader = await query.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) throw new InvalidOperationException("В выбранной БД нет профиля");
            var profile = new TgEfAppDto { ApiId = reader.GetInt32(0), ApiHash = reader.GetGuid(1) };
            if (profile.ApiId <= 0 || profile.ApiHash == Guid.Empty) throw new InvalidOperationException("Профиль не заполнен");
            var bytes = await File.ReadAllBytesAsync(args[2]);
            if (bytes.Length == 0) throw new InvalidOperationException("Файл сессии пуст");
            Directory.CreateDirectory(temporaryDirectory);
            await File.WriteAllBytesAsync(temporaryPath, bytes);
            using var client = new WTelegram.Client(key => key switch
            {
                "api_id" => profile.ApiIdString,
                "api_hash" => profile.ApiHashString,
                "session_pathname" => temporaryPath,
                _ => null,
            });
            Console.WriteLine($"PASS: файл сессии расшифрован; сохранён пользователь: {client.UserId > 0}; Telegram не вызывался");
            return 0;
        }
        catch (Exception ex)
        {
            var sessionFailure = ex.Message.Contains("Exception while reading session file", StringComparison.OrdinalIgnoreCase);
            Console.WriteLine(sessionFailure
                ? "FAIL: файл сессии не читается с API-параметрами выбранной БД; несовместимые реквизиты или повреждение файла"
                : $"FAIL: проверка сессии: {ex.GetType().Name}");
            return 1;
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
            {
                File.Delete(temporaryPath);
                Directory.Delete(temporaryDirectory);
            }
        }
    }
}
