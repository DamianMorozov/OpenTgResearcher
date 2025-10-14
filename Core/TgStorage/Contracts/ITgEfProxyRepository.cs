namespace TgStorage.Contracts;

public interface ITgEfProxyRepository : ITgEfRepository<TgEfProxyEntity, TgEfProxyDto>, IDisposable
{
    public Task<TgEfStorageResult<TgEfProxyEntity>> GetCurrentProxyAsync(Guid? proxyUid);
    public Task<Guid> GetCurrentProxyUidAsync();
    public TgEfProxyDto GetCurrentAppDto();
    public Task<TgEfProxyDto> GetCurrentDtoAsync();
    public Task<TgEfProxyDto> GetDtoAsync(Guid? proxyUid);
    /// <summary> Create a default proxy entity if none exists </summary>
    public Task CreateDefaultAsync();
    /// <summary> Save proxy dto </summary>
    public Task SaveAsync(TgEfProxyDto dto);
}
