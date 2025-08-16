// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Contracts;

public interface ITgEfSourceRepository : ITgEfRepository<TgEfSourceEntity, TgEfSourceDto>, IDisposable
{
    /// <summary> Reset auto update field </summary>
    public Task ResetAutoUpdateAsync();
    /// <summary> Set user access </summary>
    public Task SetIsUserAccessAsync(bool isUserAccess);
    /// <summary> Set user access </summary>
    public Task SetIsUserAccessAsync(List<long> chatIds, bool isUserAccess);
    /// <summary> Set subscribe </summary>
    public Task SetIsSubscribeAsync(bool isSubscribe);
    /// <summary> Set subscribe </summary>
    public Task SetIsSubscribeAsync(List<long> chatIds, bool isSubscribe);
    /// <summary> Find comment source by chat id </summary>
    public Task<TgEfSourceDto> FindCommentDtoSourceAsync(long chatId);
}
