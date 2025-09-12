namespace TgStorage.Contracts;

public interface ITgEfProxyRepository : ITgEfRepository<TgEfProxyEntity, TgEfProxyDto>, IDisposable
{
    public Task<TgEfStorageResult<TgEfProxyEntity>> GetCurrentProxyAsync(Guid? proxyUid);
    public Task<Guid> GetCurrentProxyUidAsync();
    public TgEfProxyDto GetCurrentAppDto();
    public Task<TgEfProxyDto> GetCurrentDtoAsync();
    public Task<TgEfProxyDto> GetDtoAsync(Guid? proxyUid);
}
