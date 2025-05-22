// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Contracts;

public interface ITgEfProxyRepository : ITgEfRepository<TgEfProxyEntity, TgEfProxyDto>, IDisposable
{
    Task<TgEfStorageResult<TgEfProxyEntity>> GetCurrentProxyAsync(Guid? proxyUid);
    Task<Guid> GetCurrentProxyUidAsync(TgEfStorageResult<TgEfAppEntity> storageResult);
}