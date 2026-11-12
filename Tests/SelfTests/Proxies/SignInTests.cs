namespace OpenTgResearcherDesktop.ProxySelfTests;

// Проверяем исходный сервис подключения с реальным WTelegram-клиентом и управляемым ответом авторизации без внешней сети
internal static partial class Program
{
    private static async Task SessionStepsKeepClientAsync()
    {
        using var fixture = new StepLoginFixture();
        var service = fixture.Connection;
        var notifications = 0;
        service.SetupAfterClientConnect(() => { notifications++; return Task.CompletedTask; });
        service.Reply = "verification_code";
        Assert(await service.ConnectSessionStepAsync(fixture.Config, new()) == "verification_code", "Первый шаг потерял запрос кода");
        var firstClient = service.Client;
        var firstConfigCalls = fixture.ConfigCalls;
        service.Reply = "password";
        Assert(await service.ConnectSessionStepAsync(fixture.Config, new(), "12345") == "password", "Код не перевёл вход к паролю");
        Assert(ReferenceEquals(firstClient, service.Client) && firstConfigCalls == fixture.ConfigCalls
            && service.Inputs.SequenceEqual(new[] { "+10000000000", "12345" }), "Ввод кода пересоздал клиент или повторно отправил телефон");
        service.Reply = null;
        service.Authorize = true;
        Assert(await service.ConnectSessionStepAsync(fixture.Config, new(), "sample-password") is null && service.Me?.ID == 42,
            "Завершение входа не получило пользователя");
        Assert(notifications == 3 && await service.CheckClientConnectionReadyAsync(), "Сервис не обновил состояние каждого шага");
        var steps = service.Inputs.Count;
        Assert(await service.ConnectSessionStepAsync(fixture.Config, new()) is null && service.Inputs.Count == steps
            && ReferenceEquals(firstClient, service.Client), "Готовая сессия повторно запросила телефон");
        await service.DisconnectClientAsync(isAfterClientConnect: false);
        using var exclusive = File.Open(fixture.SessionPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert(service.Client is null, "Отключение оставило клиент или удерживает сессию");
    }

    private static async Task SessionStepRejectsFalseSuccessAsync()
    {
        using var fixture = new StepLoginFixture();
        var service = fixture.Connection;
        service.Client = new(fixture.Config, new MemoryStream());
        Exception? error = null;
        try { await service.ConnectSessionStepAsync(_ => null, new(), "12345"); }
        catch (InvalidOperationException ex) { error = ex; }
        Assert(error is not null && service.Client is null && service.Me is null && service.ClientException.IsExist,
            "Пустой ответ без пользователя принят как успешный вход или не освобождён клиент");
    }
}

internal sealed class StepLoginFixture : IDisposable
{
    private readonly FusionCache _cache = new(new FusionCacheOptions());
    private readonly LoadStateService _state = new();
    public string SessionPath { get; } = Path.Combine(Path.GetTempPath(), $"otr-step-{Guid.NewGuid():N}.session");
    public StepLoginClient Connection { get; }
    public int ConfigCalls { get; private set; }
    /// <summary> Возвращает синтетические реквизиты и отдельный файл сессии </summary>
    public string? Config(string key)
    {
        ConfigCalls++;
        return key switch
        {
            "api_id" => "1", "api_hash" => new string('0', 32), "phone_number" => "+10000000000", "session_pathname" => SessionPath, _ => null,
        };
    }
    public StepLoginFixture()
    {
        Connection = new(DispatchProxy.Create<ITgStorageService, StorageProxy>(), new TgFloodControlService(), _cache, _state);
        GC.SuppressFinalize(Connection);
    }
    /// <summary> Освобождает только клиент и файл изолированного стенда </summary>
    public void Dispose() { Connection.Client?.Dispose(); _state.Dispose(); _cache.Dispose(); File.Delete(SessionPath); }
}

internal sealed class StepLoginClient(ITgStorageService storage, ITgFloodControlService flood, IFusionCache cache, ILoadStateService state)
    : TgConnectClientBase(storage, flood, cache, state)
{
    public string? Reply { get; set; }
    public bool Authorize { get; set; }
    public List<string?> Inputs { get; } = [];
    /// <summary> Подменяет ответ сервера, сохраняя настоящий транспорт, сессию и подготовку сервиса </summary>
    protected override Task<string?> LoginSessionStepAsync(string? input)
    {
        Inputs.Add(input);
        if (Authorize) typeof(WTelegram.Client).GetProperty("User")!.SetValue(Client, new TL.User { id = 42 });
        return Task.FromResult(Reply);
    }
}
