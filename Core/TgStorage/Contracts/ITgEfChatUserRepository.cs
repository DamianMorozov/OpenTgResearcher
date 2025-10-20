namespace TgStorage.Contracts;

public interface ITgEfChatUserRepository : ITgEfRepository<TgEfChatUserEntity, TgEfChatUserDto>, IDisposable
{
    /// <summary> Create missing chat users </summary>
    public Task CreateMissingChatUsersByMessagesAsync(long sourceId, CancellationToken ct = default);
    /// <summary> Save chat user </summary>
    public Task SaveAsync(TgEfChatUserDto dto, CancellationToken ct = default);
    /// <summary> Save chat user list </summary>
    public Task SaveListAsync(IEnumerable<TgEfChatUserDto> dtos, CancellationToken ct = default);
}
