namespace TgStorage.Contracts;

public interface ITgEfChatUserRepository : ITgEfRepository<TgEfChatUserEntity, TgEfChatUserDto>, IDisposable
{
    /// <summary> Create missing chat users </summary>
    public Task CreateMissingChatUsersByMessagesAsync(long sourceId, CancellationToken ct = default);
}
