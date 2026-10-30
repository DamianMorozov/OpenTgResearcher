namespace OpenTgResearcherDesktop.ProxySelfTests;

internal static partial class Program
{
    // Проверяем вызов прямого транспорта так же, как его вызывает WTelegram при авторизации
    private static async Task DirectTransportConnectsAfterResetAsync()
    {
        using var fixture = new ClientFixture();
        using var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start();
        await fixture.Connection.ConnectThroughProxyAsync(CreateProxy(), isDesktop: true);
        await fixture.Connection.ConnectThroughProxyAsync(new(), isDesktop: true);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var acceptedTask = listener.AcceptTcpClientAsync(timeout.Token);
        using var socket = await fixture.Client.TcpHandler("127.0.0.1", ((IPEndPoint)listener.LocalEndpoint).Port).WaitAsync(timeout.Token);
        using var accepted = await acceptedTask;
        Assert(socket.Connected && accepted.Connected && !fixture.Connection.IsProxyUsage, "Прямой транспорт не открыл TCP-соединение");
    }

    // Проверяем настоящий HTTP CONNECT на сервере только в loopback
    private static async Task HttpUsesConnectProtocolAsync()
    {
        using var fixture = new ClientFixture();
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var dto = CreateProxy(); dto.Type = TgEnumProxyType.Http; dto.HostName = "127.0.0.1";
        dto.Port = (ushort)((IPEndPoint)listener.LocalEndpoint).Port;
        await fixture.Connection.ConnectThroughProxyAsync(dto, isDesktop: true);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var server = ReplyHttpAsync(listener, timeout.Token);
        using var socket = await fixture.Client.TcpHandler("127.0.0.1", 443).WaitAsync(timeout.Token);
        var request = await server;
        Assert(socket.Connected && request.StartsWith("CONNECT 127.0.0.1:443 HTTP/"), "Тип HTTP использует другой протокол");
    }

    // Читаем запрос HTTP и подтверждаем открытие туннеля без внешнего соединения
    private static async Task<string> ReplyHttpAsync(TcpListener listener, CancellationToken ct)
    {
        using var accepted = await listener.AcceptTcpClientAsync(ct);
        var stream = accepted.GetStream();
        var bytes = new byte[4096];
        var count = 0;
        while (count < bytes.Length)
        {
            count += await stream.ReadAsync(bytes.AsMemory(count), ct);
            if (Encoding.ASCII.GetString(bytes, 0, count).Contains("\r\n\r\n")) break;
        }
        await stream.WriteAsync(Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection established\r\n\r\n"), ct);
        return Encoding.ASCII.GetString(bytes, 0, count);
    }

    // Проверяем настоящий SOCKS5 handshake на loopback
    private static async Task SocksUsesSocks5ProtocolAsync()
    {
        using var fixture = new ClientFixture();
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var dto = CreateProxy(); dto.HostName = "127.0.0.1"; dto.Port = (ushort)((IPEndPoint)listener.LocalEndpoint).Port;
        await fixture.Connection.ConnectThroughProxyAsync(dto, isDesktop: true);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var server = ReplySocksAsync(listener, requiresAuthentication: false, timeout.Token);
        using var socket = await fixture.Client.TcpHandler("127.0.0.1", 443).WaitAsync(timeout.Token);
        await server;
        Assert(socket.Connected && fixture.Connection.IsProxyUsage, "SOCKS5 туннель не открыт");
    }

    // Выполняем минимальный SOCKS5 обмен без обращения к адресу назначения
    private static async Task ReplySocksAsync(TcpListener listener, bool requiresAuthentication, CancellationToken ct)
    {
        using var accepted = await listener.AcceptTcpClientAsync(ct);
        var stream = accepted.GetStream();
        var greeting = new byte[2]; await stream.ReadExactlyAsync(greeting, ct);
        Assert(greeting[0] == 5, "Ожидался протокол SOCKS5");
        var methods = new byte[greeting[1]]; await stream.ReadExactlyAsync(methods, ct);
        await stream.WriteAsync(new byte[] { 5, requiresAuthentication ? (byte)2 : (byte)0 }, ct);
        if (requiresAuthentication) await ReplySocksAuthenticationAsync(stream, ct);
        var header = new byte[4]; await stream.ReadExactlyAsync(header, ct);
        Assert(header[0] == 5 && header[1] == 1, "Не получена команда SOCKS5 CONNECT");
        var addressLength = header[3] switch
        {
            1 => 4,
            4 => 16,
            3 => await ReadAddressLengthAsync(stream, ct),
            _ => throw new InvalidOperationException("Неизвестный тип адреса SOCKS5"),
        };
        var destination = new byte[addressLength + 2]; await stream.ReadExactlyAsync(destination, ct);
        await stream.WriteAsync(new byte[] { 5, 0, 0, 1, 127, 0, 0, 1, 1, 187 }, ct);
    }

    // Читаем длину доменного имени SOCKS5
    private static async Task<int> ReadAddressLengthAsync(NetworkStream stream, CancellationToken ct)
    {
        var length = new byte[1]; await stream.ReadExactlyAsync(length, ct); return length[0];
    }

    // Проверяем передачу реквизитов в заголовке настоящего HTTP CONNECT
    private static async Task HttpPassesCredentialsAsync()
    {
        using var fixture = new ClientFixture();
        using var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start();
        var dto = CreateProxy(); dto.Type = TgEnumProxyType.Http; dto.HostName = "127.0.0.1";
        dto.Port = (ushort)((IPEndPoint)listener.LocalEndpoint).Port; dto.UserName = "user"; dto.Password = "pass";
        await fixture.Connection.ConnectThroughProxyAsync(dto, isDesktop: true);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var server = ReplyHttpAsync(listener, timeout.Token);
        using var socket = await fixture.Client.TcpHandler("127.0.0.1", 443).WaitAsync(timeout.Token);
        var request = await server;
        Assert(request.Contains("Proxy-Authorization: Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes("user:pass")),
            StringComparison.OrdinalIgnoreCase), "Реквизиты HTTP прокси не переданы");
    }

    // Проверяем передачу реквизитов в настоящем протоколе SOCKS5
    private static async Task SocksPassesCredentialsAsync()
    {
        using var fixture = new ClientFixture();
        using var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start();
        var dto = CreateProxy(); dto.HostName = "127.0.0.1"; dto.Port = (ushort)((IPEndPoint)listener.LocalEndpoint).Port;
        dto.UserName = "user"; dto.Password = "pass";
        await fixture.Connection.ConnectThroughProxyAsync(dto, isDesktop: true);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var server = ReplySocksAsync(listener, requiresAuthentication: true, timeout.Token);
        using var socket = await fixture.Client.TcpHandler("127.0.0.1", 443).WaitAsync(timeout.Token);
        await server;
        Assert(socket.Connected, "SOCKS5 с реквизитами не открыл туннель");
    }

    // Подтверждаем только ожидаемые тестовые имя пользователя и пароль
    private static async Task ReplySocksAuthenticationAsync(NetworkStream stream, CancellationToken ct)
    {
        var auth = new byte[2]; await stream.ReadExactlyAsync(auth, ct);
        Assert(auth[0] == 1, "Неверная версия аутентификации SOCKS5");
        var username = new byte[auth[1]]; await stream.ReadExactlyAsync(username, ct);
        var passwordLength = await ReadAddressLengthAsync(stream, ct);
        var password = new byte[passwordLength]; await stream.ReadExactlyAsync(password, ct);
        Assert(Encoding.ASCII.GetString(username) == "user" && Encoding.ASCII.GetString(password) == "pass", "Реквизиты SOCKS5 изменены");
        await stream.WriteAsync(new byte[] { 1, 0 }, ct);
    }

    // Проверяем установку MTProto без авторизации и без ложного сообщения о соединении
    private static async Task MtProtoConfiguredBeforeLoginAsync()
    {
        using var fixture = new ClientFixture();
        var notifications = 0;
        fixture.Connection.SetupUpdateStateProxy(_ => { notifications++; return Task.CompletedTask; });
        var dto = CreateProxy(); dto.Type = TgEnumProxyType.MtProto; dto.Secret = new string('a', 32);
        await fixture.Connection.ConnectThroughProxyAsync(dto, isDesktop: true);
        Assert(fixture.Connection.IsProxyUsage && fixture.Client.MTProxyUrl.Contains(dto.Secret) && notifications == 0,
            "MTProto не настроен или сообщил об успехе без подключения");
    }

    // Проверяем сброс старого транспорта при выборе прямого соединения
    private static async Task NoneClearsPreviousTransportAsync()
    {
        using var fixture = new ClientFixture();
        await fixture.Connection.ConnectThroughProxyAsync(CreateProxy(), isDesktop: true);
        await fixture.Connection.ConnectThroughProxyAsync(new(), isDesktop: true);
        Assert(!fixture.Connection.IsProxyUsage && fixture.Client.TcpHandler is not null && string.IsNullOrEmpty(fixture.Client.MTProxyUrl),
            "Прямое соединение потеряло штатный обработчик TCP");
    }

    // Проверяем готовность авторизованного клиента при выбранном прокси
    private static async Task ConfiguredProxyDoesNotPreventReadinessAsync()
    {
        using var fixture = new ClientFixture();
        var dto = CreateProxy(); await Repository.SaveAsync(dto);
        Context.Apps.Add(new TgEfAppEntity { Uid = Guid.NewGuid(), ApiHash = Guid.NewGuid(), ProxyUid = dto.Uid, UseClient = true });
        await Context.SaveChangesAsync();
        TgAppSettingsHelper.Instance.IsUseProxy = true;
        typeof(TgConnectClientBase).GetProperty(nameof(TgConnectClientBase.Me))!.SetValue(fixture.Connection, new TL.User { id = 42 });
        try
        {
            Assert(!fixture.Client.Disconnected, "Тестовый клиент отмечен отключённым до первой попытки соединения");
            Assert(await fixture.Connection.CheckClientConnectionReadyAsync(), "Сохранённый прокси запрещает готовность клиента");
            fixture.Connection.ProxyException.Set(new InvalidOperationException("Тестовая ошибка прокси"));
            Assert(!await fixture.Connection.CheckClientConnectionReadyAsync(), "Реальная ошибка прокси проигнорирована");
        }
        finally { TgAppSettingsHelper.Instance.IsUseProxy = false; }
    }

    // Проверяем отказ до попытки прямого подключения при ошибочном выбранном прокси
    private static async Task InvalidProxyFailsBeforeConnectionAsync()
    {
        using var fixture = new ClientFixture();
        var dto = CreateProxy(); dto.HostName = "";
        var failed = false;
        try { await fixture.Connection.ConnectThroughProxyAsync(dto, isDesktop: true); }
        catch (ArgumentException) { failed = true; }
        Assert(failed && fixture.Connection.ProxyException.IsExist && !fixture.Connection.IsProxyUsage, "Ошибочный прокси молча заменён прямым соединением");
    }
}

// Создаём настоящий клиент с сессией в памяти и управляемыми зависимостями
internal sealed class ClientFixture : IDisposable
{
    private readonly FusionCache _cache = new(new FusionCacheOptions());
    private readonly LoadStateService _state = new();
    private readonly MemoryStream _session = new();
    public WTelegram.Client Client { get; }
    public TgConnectClientTest Connection { get; }
    public bool IsSessionOpen => _session.CanWrite;

    public ClientFixture()
    {
        var storage = DispatchProxy.Create<TgBusinessLogic.Contracts.ITgStorageService, StorageProxy>();
        Connection = new(storage, new TgFloodControlService(), _cache, _state);
        GC.SuppressFinalize(Connection);
        Client = new(Config, _session);
        Connection.Client = Client;
    }

    /// <summary> Освобождает сессию и фоновые службы стенда без завершения пользовательского клиента </summary>
    public void Dispose() { Client.Dispose(); _session.Dispose(); _state.Dispose(); _cache.Dispose(); }
    // Возвращаем тестовые ключи без чтения диска
    private static string? Config(string name) => name switch { "api_id" => "1", "api_hash" => new string('0', 32), _ => null };
}

// Подменяем только выдачу репозиториев для проверки старой ветки готовности
public class StorageProxy : DispatchProxy
{
    // Возвращаем настоящие репозитории изолированной SQLite
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => targetMethod?.Name switch
    {
        "get_AppRepository" => new TgEfAppRepository(),
        "get_ProxyRepository" => App.BusinessLogicManager.StorageManager.ProxyRepository,
        _ => throw new InvalidOperationException("Непредусмотренный вызов службы хранения в селфтесте"),
    };
}
