namespace OpenTgResearcherDesktop.ProxySelfTests;

internal static partial class Program
{
    // Проверяем реальную авторизацию только по явному параметру запуска, не изменяя исходную БД и сессию
    private static async Task<int> RunLiveConnectionAsync(string[] args)
    {
        var outputDirectory = Path.Combine(AppContext.BaseDirectory, "live", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDirectory);
        var logPath = Path.Combine(outputDirectory, "connection.log");
        await File.WriteAllTextAsync(logPath, "");
        typeof(TgInfrastructure.Helpers.TgLogUtils).GetField("_startupLog", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, logPath);
        TgGlobalTools.SetAppType(TgEnumAppType.Test);
        try
        {
            if (args.Length != 3) throw new ArgumentException("Укажите --live, путь БД и путь сохранённой сессии");
            var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
            {
                DataSource = Path.GetFullPath(args[1]),
                Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly,
            }.ToString();
            await using var context = new TestContext(new DbContextOptionsBuilder().UseSqlite(connectionString).Options);
            var builder = new ContainerBuilder();
            builder.RegisterInstance(context).As<ITgEfContext>().ExternallyOwned();
            using var container = builder.Build();
            TgGlobalTools.Container = container;
            using var appRepository = new TgEfAppRepository();
            using var proxyRepository = new TgEfProxyRepository();
            App.BusinessLogicManager.StorageManager.ProxyRepository = proxyRepository;
            var appDto = await appRepository.GetCurrentDtoAsync();
            if (!appDto.UseClient || appDto.UseBot || appDto.ApiId <= 0 || appDto.ApiHash == Guid.Empty)
                throw new InvalidOperationException("В БД отсутствует подходящий профиль клиента");
            var proxy = (await proxyRepository.GetListDtosAsync()).Where(x => x.Type != TgEnumProxyType.None)
                .OrderBy(x => x.HostName).ThenBy(x => x.Type).ThenBy(x => x.Port).FirstOrDefault()
                ?? throw new InvalidOperationException("В БД нет записи прокси для проверки");
            byte[] sessionBytes;
            await using (var source = new FileStream(args[2], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using var buffer = new MemoryStream();
                await source.CopyToAsync(buffer);
                sessionBytes = buffer.ToArray();
            }
            if (sessionBytes.Length == 0) throw new InvalidOperationException("Сохранённая сессия пуста");
            Console.WriteLine($"Сессия найдена; БД открыта только для чтения; первый прокси: тип {proxy.Type}, UID {proxy.Uid}");
            var direct = await RunLiveCaseAsync("direct", appDto, new(), sessionBytes, outputDirectory);
            var throughProxy = await RunLiveCaseAsync("proxy", appDto, proxy, sessionBytes, outputDirectory);
            Console.WriteLine($"Результат подключения: {(direct ? 1 : 0) + (throughProxy ? 1 : 0)}/2; исходные БД и сессия не изменялись");
            Console.WriteLine($"Журнал проверки: {logPath}");
            return direct && throughProxy ? 0 : 1;
        }
        catch (Exception ex)
        {
            TgInfrastructure.Helpers.TgLogUtils.WriteException(ex);
            Console.WriteLine($"FAIL: подготовка подключения: {ex.GetType().Name}; стек: {ex.StackTrace}");
            return 1;
        }
    }

    // Подключаем настоящий Desktop-клиент с отдельной копией сессии без запроса кодов и паролей
    private static async Task<bool> RunLiveCaseAsync(string name, TgEfAppDto appDto, TgEfProxyDto proxy, byte[] sessionBytes, string outputDirectory)
    {
        var sessionPath = Path.Combine(outputDirectory, name + ".session");
        using var state = new LoadStateService();
        using var cache = new FusionCache(new FusionCacheOptions());
        var storage = DispatchProxy.Create<TgBusinessLogic.Contracts.ITgStorageService, StorageProxy>();
        var connection = new TgConnectClientDesktop(storage, new TgFloodControlService(), cache, state);
        GC.SuppressFinalize(connection);
        Exception? error = null;
        connection.SetupUpdateException(ex => error = ex);
        var configuration = new LiveConfiguration(appDto, sessionPath);
        var timer = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await File.WriteAllBytesAsync(sessionPath, sessionBytes);
            var operation = connection.ConnectSessionAsync(configuration.Get, proxy, isDesktop: true);
            await operation.WaitAsync(TimeSpan.FromSeconds(45));
            if (error is not null) throw new InvalidOperationException("Не удалось восстановить авторизацию", error);
            if (connection.Client is null) throw new InvalidOperationException("Клиент не создан");
            await connection.Client.ConnectAsync().WaitAsync(TimeSpan.FromSeconds(30));
            var users = await TL.SchemaExtensions.Users_GetUsers(connection.Client, [new TL.InputUserSelf()]).WaitAsync(TimeSpan.FromSeconds(30));
            if (users.SingleOrDefault() is not TL.User user || user.id != connection.Me?.id)
                throw new InvalidOperationException("Telegram не подтвердил пользователя сохранённой сессии");
            if (!await connection.CheckClientConnectionReadyAsync() || connection.Me is null || !connection.Me.IsActive)
                throw new InvalidOperationException("Авторизованный пользователь не получен");
            if (connection.IsProxyUsage != (proxy.Type != TgEnumProxyType.None))
                throw new InvalidOperationException("Использованный транспорт не соответствует сценарию");
            await Task.Run(connection.Dispose).WaitAsync(TimeSpan.FromSeconds(15));
            if (connection.Client is not null || connection.Me is not null || connection.IsReady)
                throw new InvalidOperationException("Клиент не завершил отключение при освобождении ресурсов");
            using (var savedSession = File.Open(sessionPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                if (savedSession.Length == 0) throw new InvalidOperationException("Сессия не сохранена после отключения");
            }
            var result = $"PASS: {name}; авторизация подтверждена; клиент завершён, сессия сохранена и освобождена; {timer.Elapsed.TotalSeconds:F1} с";
            Console.WriteLine(result);
            TgInfrastructure.Helpers.TgLogUtils.WriteLog(result);
            return true;
        }
        catch (Exception ex)
        {
            TgInfrastructure.Helpers.TgLogUtils.WriteException(ex);
            var cause = ex.GetBaseException();
            Console.WriteLine($"FAIL: {name}; {cause.GetType().Name}; {timer.Elapsed.TotalSeconds:F1} с; стек: {cause.StackTrace}");
            return false;
        }
        finally
        {
            connection.Dispose();
            // Удаляем только созданную проверкой копию сессии, исходный файл открыт исключительно для чтения
            if (File.Exists(sessionPath)) File.Delete(sessionPath);
        }
    }
}

// Ограничиваем конфигурацию восстановлением существующей авторизации
internal sealed class LiveConfiguration(TgEfAppDto appDto, string sessionPath)
{
    /// <summary> Возвращает настройки из БД без вывода реквизитов в консоль </summary>
    public string? Get(string name) => name switch
    {
        "api_id" => appDto.ApiIdString,
        "api_hash" => appDto.ApiHashString,
        "phone_number" => appDto.PhoneNumber,
        "session_pathname" => sessionPath,
        "verification_code" or "password" or "email_verification_code" =>
            throw new InvalidOperationException("Сохранённая сессия не восстановлена: требуется интерактивная авторизация"),
        _ => null,
    };
}
