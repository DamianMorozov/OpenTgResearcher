// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Contracts;

public interface ITgEfMessageRepository : ITgEfRepository<TgEfMessageEntity, TgEfMessageDto>, IDisposable
{
    /// <summary> Get last ID </summary>
    public Task<long> GetLastIdAsync(long sourceId);
    /// <summary> Get list of DTOs without relations </summary>
    public Task<List<TgEfMessageDto>> GetListDtosWithoutRelationsAsync<TKey>(int take, int skip, 
        Expression<Func<TgEfMessageEntity, bool>> where, Expression<Func<TgEfMessageEntity, TKey>> order, bool isOrderDesc = false);
    /// <summary> Save relation between parent and child messages </summary>
    public Task SaveRelationAsync(long parentChatId, int parentMessageId, long childChatId, int childMessageId);
    /// <summary> Get users from messages </summary>
    public Task<List<long>> GetUserIdsFromMessagesAsync(Expression<Func<TgEfMessageEntity, bool>> where);
}
