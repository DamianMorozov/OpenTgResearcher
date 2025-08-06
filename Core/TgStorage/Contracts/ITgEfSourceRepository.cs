// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Contracts;

public interface ITgEfSourceRepository : ITgEfRepository<TgEfSourceEntity, TgEfSourceDto>, IDisposable
{
    /// <summary> Reset auto update field </summary>
    public Task ResetAutoUpdateAsync();
    /// <summary> Set user access </summary>
    public Task SetIsUserAccess(bool isUserAccess);
    /// <summary> Set user access </summary>
    public Task SetIsUserAccess(List<long> chatIds, bool isUserAccess);
    /// <summary> Set subscribe </summary>
    public Task SetIsSubscribe(bool isSubscribe);
    /// <summary> Set subscribe </summary>
    public Task SetIsSubscribe(List<long> chatIds, bool isSubscribe);
}