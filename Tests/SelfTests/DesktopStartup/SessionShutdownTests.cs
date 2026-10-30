namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static partial class Program
{
    // Проверяем освобождение настоящего файла сессии до закрытия окна исходным обработчиком приложения
    private static async Task CloseReleasesSessionFileAsync(bool isAlreadyDisconnected)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "session-shutdown", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var sessionPath = Path.Combine(directory, "test.session");
        var originalLog = WTelegram.Helpers.Log;
        using var state = new LoadStateService();
        using var cache = new FusionCache(new FusionCacheOptions());
        var storage = DispatchProxy.Create<TgBusinessLogic.Contracts.ITgStorageService, ShutdownStorageProxy>();
        using var connection = new TgConnectClientDesktop(storage, new TgFloodControlService(), cache, state);
        try
        {
            WTelegram.Helpers.Log = (_, _) => { };
            using var client = new WTelegram.Client(key => SessionShutdownConfig(key, sessionPath));
            connection.Client = client;
            var session = typeof(WTelegram.Client).GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(client)!;
            session.GetType().GetMethod("Save", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(session, null);
            var locked = false;
            try { using var probe = File.Open(sessionPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException) { locked = true; }
            Assert(locked, "Стенд не воспроизвёл блокировку файла сессии WTelegram");
            if (isAlreadyDisconnected) await connection.DisconnectClientAsync(isAfterClientConnect: false);
            App.BusinessLogicManager = new() { ConnectClient = connection };
            App.MainWindow = new()
            {
                OnClose = () =>
                {
                    Assert(connection.Client is null, "Окно закрывается до освобождения Telegram-клиента");
                    using var probe = File.Open(sessionPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    Assert(probe.Length > 8, "К моменту закрытия окна файл сессии удалён или пуст");
                },
            };
            var app = new App();
            await CloseMainWindowAsync(app).WaitAsync(TimeSpan.FromSeconds(10));
            Assert(App.MainWindow.CloseCalls == 1 && TestLogUtils.Errors.Count == 0, "Закрытие окна не завершилось или зарегистрирована ошибка");
            var saved = await File.ReadAllBytesAsync(sessionPath);
            using (var restored = new WTelegram.Client(key => SessionShutdownConfig(key, sessionPath)))
                Assert(restored.UserId == client.UserId, "Содержимое сессии изменено при завершении приложения");
            var unchanged = await File.ReadAllBytesAsync(sessionPath);
            Assert(saved.SequenceEqual(unchanged), "Повторное чтение изменило сохранённую сессию");
        }
        catch (Exception ex)
        {
            TestLogUtils.WriteException(ex);
            throw;
        }
        finally
        {
            connection.Dispose();
            WTelegram.Helpers.Log = originalLog;
            File.Delete(sessionPath);
            Directory.Delete(directory);
        }
    }

    // Используем только синтетические реквизиты и временный файл без подключения к Telegram
    private static string? SessionShutdownConfig(string name, string sessionPath) => name switch
    {
        "api_id" => "42",
        "api_hash" => "00112233445566778899aabbccddeeff",
        "session_pathname" => sessionPath,
        _ => null,
    };
}

// Запрещаем обращение к пользовательскому хранилищу в проверке закрытия
public class ShutdownStorageProxy : DispatchProxy
{
    /// <summary> Обнаруживает непредусмотренное обращение к БД </summary>
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
        throw new InvalidOperationException("Проверка освобождения сессии не должна обращаться к БД");
}
