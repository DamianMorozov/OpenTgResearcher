namespace TgStorage.Contracts;

public interface ITgEfAppRepository : ITgEfRepository<TgEfAppEntity, TgEfAppDto>, IDisposable
{
    public Task<TgEfStorageResult<TgEfAppEntity>> GetCurrentAppAsync(bool isReadOnly = true);
    public Task<Guid> GetCurrentAppUidAsync();
    public TgEfStorageResult<TgEfAppEntity> GetCurrentApp(bool isReadOnly = true);
    public Task<TgEfAppDto> GetCurrentDtoAsync(CancellationToken ct = default);
    /// <summary> Set use bot </summary>
    public Task<bool> SetUseBotAsync(bool useBot);
    /// <summary> Set bot token </summary>
    public Task<bool> SetBotTokenKeyAsync(string botTokenKey);
    /// <summary> Set use client </summary>
    public Task<bool> SetUseClientAsync(bool useClient);
    /// <summary> Clear proxy settings </summary>
    public Task ClearProxyAsync();
}
