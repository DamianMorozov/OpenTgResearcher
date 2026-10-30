namespace OpenTgResearcherDesktop.ConnectionSelfTests;

internal static partial class Program
{
    // Проверяем диагностику на настоящем формате файла WTelegram, включая служебный заголовок
    private static async Task SessionInspectionUsesFileFormatAsync()
    {
        using var fixture = new SessionResetFixture();
        File.Delete(fixture.Settings.AppSession);
        var databasePath = Path.Combine(Path.GetDirectoryName(fixture.Settings.AppSession)!, "test.db");
        var originalLog = WTelegram.Helpers.Log;
        try
        {
            using (var client = new WTelegram.Client(key => key switch
            {
                "api_id" => fixture.Profile.ApiIdString,
                "api_hash" => fixture.Profile.ApiHashString,
                "session_pathname" => fixture.Settings.AppSession,
                _ => null,
            }))
            {
                var sessionField = typeof(WTelegram.Client).GetField("_session", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
                var session = sessionField.GetValue(client)!;
                session.GetType().GetMethod("Save",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(session, null);
            }
            var original = await File.ReadAllBytesAsync(fixture.Settings.AppSession);
            Assert(original.Length > 8, "WTelegram не записал тестовую сессию");
            var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = databasePath, Pooling = false }.ToString();
            await using (var database = new Microsoft.Data.Sqlite.SqliteConnection(connectionString))
            {
                await database.OpenAsync();
                await using var command = database.CreateCommand();
                command.CommandText = "CREATE TABLE APPS (UID TEXT, API_ID INTEGER, API_HASH TEXT); INSERT INTO APPS VALUES ($uid, $id, $hash)";
                command.Parameters.AddWithValue("$uid", Guid.NewGuid());
                command.Parameters.AddWithValue("$id", fixture.Profile.ApiId);
                command.Parameters.AddWithValue("$hash", fixture.Profile.ApiHash);
                await command.ExecuteNonQueryAsync();
            }
            Assert(await InspectSessionAsync(["--inspect-session", databasePath, fixture.Settings.AppSession]) == 0,
                "Диагностика отвергла читаемый файл WTelegram");
            var unchanged = await File.ReadAllBytesAsync(fixture.Settings.AppSession);
            Assert(original.SequenceEqual(unchanged), "Диагностика изменила исходную сессию");
            Assert(await InspectSessionAsync(["--inspect-session", Path.Combine(databasePath, "missing.db"), fixture.Settings.AppSession]) == 1,
                "Ошибка открытия БД не обработана");
        }
        finally
        {
            WTelegram.Helpers.Log = originalLog;
            File.Delete(databasePath);
        }
    }

    // Воспроизводим реальный отказ конструктора WTelegram на диске и проверяем восстановление без перезапуска процесса
    private static async Task CorruptSessionCanBeResetAfterFailedConstructorAsync()
    {
        using var fixture = new SessionResetFixture();
        var bytes = new byte[248];
        BitConverter.GetBytes(8).CopyTo(bytes, 0);
        BitConverter.GetBytes(240).CopyTo(bytes, 4);
        await File.WriteAllBytesAsync(fixture.Settings.AppSession, bytes);
        fixture.Connection.OnConnect = config =>
        {
            using var client = new WTelegram.Client(config);
            return Task.CompletedTask;
        };
        await fixture.ViewModel.ClientConnectCommand.ExecuteAsync(false);
        Assert(fixture.ViewModel.Exception.IsExist && fixture.ViewModel.Errors.Count == 1,
            "WTelegram не воспроизвёл отказ чтения существующей сессии");
        fixture.ViewModel.ConfirmDialog = true;
        await GetSessionResetCommand(fixture.ViewModel).ExecuteAsync(null);
        Assert(!File.Exists(fixture.Settings.AppSession) && !fixture.ViewModel.Exception.IsExist,
            "После ошибки конструктора файл заблокирован или сброс не завершён");
        await fixture.ViewModel.ClientConnectCommand.ExecuteAsync(false);
        Assert(fixture.Connection.ConnectCalls == 2 && !fixture.ViewModel.Exception.IsExist,
            "После сброса WTelegram не создал новую сессию по прежнему пути");
    }

    // Проверяем реальное удаление выбранного файла после освобождения блокировки клиентом
    private static async Task DeleteRemovesSessionAfterDisconnectAsync()
    {
        using var fixture = new SessionResetFixture();
        fixture.OpenSession();
        fixture.ViewModel.ConfirmDialog = true;
        await fixture.ViewModel.AppDeleteCommand.ExecuteAsync(null);
        Assert(!File.Exists(fixture.Settings.AppSession), "Кнопка удаления оставила файл сессии");
        Assert(fixture.Connection.DisconnectCalls == 1 && fixture.Repository.DeleteCalls == 1, "Удаление не отключило клиента или не удалило профиль");
        Assert(!ContentDialog.IsOpen && ContentDialog.LastTitle.Contains(fixture.Settings.AppSession)
            && ContentDialog.LastTitle.Contains("профиль"), "Подтверждение не объясняет удаление профиля и файла сессии");
    }

    // Проверяем отмену удаления без изменения реального файла
    private static async Task CancelledDeleteKeepsSessionAsync()
    {
        using var fixture = new SessionResetFixture();
        await fixture.ViewModel.AppDeleteCommand.ExecuteAsync(null);
        fixture.AssertUnchanged();
        Assert(fixture.Connection.DisconnectCalls == 0, "Отменённое удаление отключило клиента");
    }

    // Проверяем сброс без потери API-параметров и создание WTelegram новой сессии по тому же пути без сети
    private static async Task SessionResetPreservesProfileAndAllowsNewSessionAsync()
    {
        using var fixture = new SessionResetFixture();
        fixture.ViewModel.ConfirmDialog = true;
        fixture.OpenSession();
        fixture.ViewModel.Password = "old-password";
        fixture.ViewModel.VerificationCode = "12345";
        fixture.ViewModel.DataRequest = "old-session";
        fixture.ViewModel.Exception.Set(new IOException("Old session failure"));
        var selectedProxy = fixture.ViewModel.ProxyVm;
        await GetSessionResetCommand(fixture.ViewModel).ExecuteAsync(null);
        Assert(!File.Exists(fixture.Settings.AppSession), "Сброс не удалил сессию");
        Assert(ReferenceEquals(fixture.ViewModel.AppDto, fixture.Profile) && fixture.Repository.DeleteCalls == 0
            && fixture.Repository.SaveCalls == 0, "Сброс сессии изменил сохранённый профиль");
        Assert(ReferenceEquals(fixture.ViewModel.ProxyVm, selectedProxy), "Сброс изменил выбранный в форме прокси");
        Assert(fixture.Profile.ApiId == 42 && fixture.Profile.PhoneNumber == "+10000000000"
            && fixture.Profile.ApiHash == Guid.Parse("00112233-4455-6677-8899-aabbccddeeff") && fixture.Profile.ProxyUid != Guid.Empty,
            "Сброс потерял параметры повторного входа");
        Assert(fixture.ViewModel.Password.Length == 0 && fixture.ViewModel.VerificationCode.Length == 0
            && fixture.ViewModel.DataRequest.Length == 0 && !fixture.ViewModel.Exception.IsExist, "Данные прежнего входа не очищены");
        Assert(fixture.Settings.LoadCalls == 1 && fixture.ViewModel.ConnectionMsg.Contains("Подключить Telegram"),
            "Признак сессии и инструкция повторного входа не обновлены");
        fixture.Connection.OnConnect = config =>
        {
            using var client = new WTelegram.Client(config);
            return Task.CompletedTask;
        };
        await fixture.ViewModel.ClientConnectCommand.ExecuteAsync(false);
        Assert(fixture.Connection.ConnectCalls == 1 && !fixture.ViewModel.Exception.IsExist
            && fixture.Connection.Configuration["api_id"] == "42", "Новая сессия не создаётся с прежними API-параметрами");
    }

    // Проверяем отмену отдельного сброса сессии
    private static async Task CancelledSessionResetKeepsSessionAsync()
    {
        using var fixture = new SessionResetFixture();
        await GetSessionResetCommand(fixture.ViewModel).ExecuteAsync(null);
        fixture.AssertUnchanged();
        Assert(fixture.Connection.DisconnectCalls == 0, "Отмена сброса отключила клиента");
    }

    // Проверяем ожидание завершения клиента до удаления файла
    private static async Task SessionResetWaitsForDisconnectAsync()
    {
        using var fixture = new SessionResetFixture();
        fixture.ViewModel.ConfirmDialog = true;
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        fixture.Connection.OnDisconnect = () => gate.Task;
        var operation = GetSessionResetCommand(fixture.ViewModel).ExecuteAsync(null);
        try
        {
            Assert(!operation.IsCompleted && File.Exists(fixture.Settings.AppSession), "Сброс не дождался отключения клиента");
        }
        finally { gate.TrySetResult(); await operation.WaitAsync(TimeSpan.FromSeconds(5)); }
        Assert(!File.Exists(fixture.Settings.AppSession), "После отключения файл не удалён");
    }

    // Проверяем настоящий отказ удаления заблокированного другим процессом файла
    private static async Task SessionResetFailureIsShownAndLoggedAsync()
    {
        using var fixture = new SessionResetFixture();
        fixture.ViewModel.ConfirmDialog = true;
        using var externalLock = File.Open(fixture.Settings.AppSession, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        await GetSessionResetCommand(fixture.ViewModel).ExecuteAsync(null);
        Assert(File.Exists(fixture.Settings.AppSession) && fixture.ViewModel.Exception.IsExist && fixture.ViewModel.Errors.Count == 1,
            "Ошибка удаления скрыта или не зарегистрирована");
        Assert(fixture.Repository.DeleteCalls == 0 && fixture.Settings.LoadCalls == 0, "Ошибка удаления изменила профиль или показала успех");
    }

    // Проверяем прекращение удаления, если служба не смогла завершить клиента
    private static async Task DisconnectFailureStopsDeletionAsync()
    {
        using var fixture = new SessionResetFixture();
        fixture.ViewModel.ConfirmDialog = true;
        fixture.Connection.Client = new();
        fixture.Connection.KeepClientOnDisconnect = true;
        await fixture.ViewModel.AppDeleteCommand.ExecuteAsync(null);
        fixture.AssertUnchanged();
        Assert(fixture.ViewModel.Exception.IsExist && fixture.ViewModel.Errors.Count == 1, "Отказ отключения не показан и не записан в журнал");
    }

    // Проверяем сброс без существующего файла и сохранение параметров
    private static async Task MissingSessionResetSucceedsAsync()
    {
        using var fixture = new SessionResetFixture();
        File.Delete(fixture.Settings.AppSession);
        fixture.ViewModel.ConfirmDialog = true;
        await GetSessionResetCommand(fixture.ViewModel).ExecuteAsync(null);
        Assert(!fixture.ViewModel.Exception.IsExist && fixture.Repository.DeleteCalls == 0 && fixture.Settings.LoadCalls == 1,
            "Отсутствующий файл препятствует сбросу или профиль удалён");
    }

    // Проверяем наличие действия в интерфейсе, а не только метода модели
    private static Task SessionResetButtonIsBoundAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("ConnectionPage.xaml")!;
        var page = System.Xml.Linq.XDocument.Load(stream);
        System.Xml.Linq.XNamespace controls = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        Assert(page.Descendants(controls + "Button").Any(x => (string?)x.Attribute("Command") == "{x:Bind ViewModel.SessionResetCommand}"),
            "На странице нет кнопки сброса сессии");
        return Task.CompletedTask;
    }

    // Получаем команду модели для выполнения тех же действий, что и кнопка страницы
    private static IAsyncRelayCommand GetSessionResetCommand(TgClientConnectionViewModel vm) => vm.SessionResetCommand;
}

// Создаём только собственные временные файлы и профиль, пользовательские данные не открываем
internal sealed class SessionResetFixture : IDisposable
{
    private readonly string _directory = Path.Combine(AppContext.BaseDirectory, "session-reset", Guid.NewGuid().ToString("N"));
    private FileStream? _session;
    public TestSettingsService Settings { get; }
    public TgClientConnectionViewModel ViewModel { get; }
    public TestConnection Connection => App.BusinessLogicManager.ConnectClient;
    public TestAppRepository Repository => App.BusinessLogicManager.StorageManager.AppRepository;
    public TgEfAppDto Profile { get; } = new()
    {
        ApiId = 42, ApiHash = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff"), PhoneNumber = "+10000000000", ProxyUid = Guid.NewGuid(),
    };

    public SessionResetFixture()
    {
        Directory.CreateDirectory(_directory);
        Settings = new() { AppSession = Path.Combine(_directory, "test.session") };
        File.WriteAllBytes(Settings.AppSession, [1, 2, 3, 4]);
        Repository.Current = Profile;
        ViewModel = new(new TestLoadStateService(), Settings, null!, null!) { AppDto = Profile };
    }

    /// <summary> Имитирует блокировку файла настоящим клиентом до его отключения </summary>
    public void OpenSession()
    {
        _session = File.Open(Settings.AppSession, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Connection.Client = new();
        Connection.OnDisconnect = () => { _session.Dispose(); _session = null; return Task.CompletedTask; };
    }

    /// <summary> Проверяет сохранение исходного файла и записи профиля </summary>
    public void AssertUnchanged()
    {
        if (!File.ReadAllBytes(Settings.AppSession).SequenceEqual(new byte[] { 1, 2, 3, 4 }) || Repository.DeleteCalls != 0)
            throw new InvalidOperationException("Файл сессии или профиль изменён без завершённого подтверждённого действия");
    }

    /// <summary> Удаляет только файлы из собственного временного каталога </summary>
    public void Dispose()
    {
        _session?.Dispose();
        File.Delete(Settings.AppSession);
        Directory.Delete(_directory);
    }
}
