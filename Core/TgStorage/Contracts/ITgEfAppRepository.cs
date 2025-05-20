// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Contracts;

public interface ITgEfAppRepository : ITgEfRepository<TgEfAppEntity>
{
    public Task<TgEfStorageResult<TgEfAppEntity>> GetCurrentAppAsync(bool isReadOnly = true);
    public Task<Guid> GetCurrentAppUidAsync();
    public TgEfStorageResult<TgEfAppEntity> GetCurrentApp(bool isReadOnly = true);

    public Task<TgEfAppDto> GetCurrentDtoAsync();
    public Task<List<TgEfAppDto>> GetListDtosAsync(int take = 0, int skip = 0);
    public Task<List<TgEfAppDto>> GetListDtosAsync(int take, int skip, Expression<Func<TgEfAppEntity, bool>> where);
}