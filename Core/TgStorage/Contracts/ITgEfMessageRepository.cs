namespace TgStorage.Contracts;

public interface ITgEfMessageRepository : ITgEfRepository<TgEfMessageEntity, TgEfMessageDto>, IDisposable
{
    /// <summary> Get last ID </summary>
    public Task<long> GetLastIdAsync(long sourceId);
    /// <summary> Get list of DTOs </summary>
    public Task<List<TgEfMessageDto>> GetListDtosAsync<TKey>(int take, int skip,
        Expression<Func<TgEfMessageEntity, bool>> where, Expression<Func<TgEfMessageEntity, TKey>> order, bool isOrderDesc = false);
    /// <summary> Get list of DTOs without relations </summary>
    public Task<List<TgEfMessageDto>> GetListDtosWithoutRelationsAsync<TKey>(int take, int skip, 
        Expression<Func<TgEfMessageEntity, bool>> where, Expression<Func<TgEfMessageEntity, TKey>> order, bool isOrderDesc = false);
    /// <summary> Save relation between parent and child messages </summary>
    public Task SaveRelationAsync(long parentChatId, int parentMessageId, long childChatId, int childMessageId);
    /// <summary> Get users from messages </summary>
    public Task<List<long>> GetUserIdsFromMessagesAsync(Expression<Func<TgEfMessageEntity, bool>> where);
    /// <summary> Get distinct user IDs by source ID </summary>
    public Task<List<long>> GetDistinctUserIdsBySourceIdAsync(long sourceId, CancellationToken ct = default);
    /// <summary> Save message </summary>
    public Task SaveAsync(TgEfMessageDto dto, CancellationToken ct = default);
    /// <summary> Save message list </summary>
    public Task SaveListAsync(IEnumerable<TgEfMessageDto> dtos, CancellationToken ct = default);
}
