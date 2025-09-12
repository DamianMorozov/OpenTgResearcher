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
