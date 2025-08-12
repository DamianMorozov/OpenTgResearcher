// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Contracts;

public interface ITgEfProxyRepository : ITgEfRepository<TgEfProxyEntity, TgEfProxyDto>, IDisposable
{
    public Task<TgEfStorageResult<TgEfProxyEntity>> GetCurrentProxyAsync(Guid? proxyUid);
    public Task<Guid> GetCurrentProxyUidAsync();
    public TgEfProxyDto GetCurrentAppDto();
    public Task<TgEfProxyDto> GetCurrentDtoAsync();
    public Task<TgEfProxyDto> GetDtoAsync(Guid? proxyUid);
}
