// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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
}
