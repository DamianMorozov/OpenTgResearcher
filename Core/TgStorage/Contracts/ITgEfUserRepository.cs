namespace TgStorage.Contracts;

public interface ITgEfUserRepository : ITgEfRepository<TgEfUserEntity, TgEfUserDto>, IDisposable
{
    /// <summary> Get all distinct users for a given source in a single query </summary>
    public Task<List<TgEfUserDto>> GetUsersByChatIdAsync(long chatId, CancellationToken ct = default);
    /// <summary> Get all distinct users for a given source using join </summary>
    public Task<List<TgEfUserDto>> GetUsersByChatIdJoinAsync(long chatId, long userId, int pageSkip, int pageTake, 
        Expression<Func<ITgEfEntity, bool>>? where, CancellationToken ct = default);
    /// <summary> Get myself user for a given source </summary>
    public Task<TgEfUserDto> GetMyselfUserAsync(long chatId, long userId, CancellationToken ct = default);
    /// <summary> Create missing users </summary>
    public Task CreateMissingUsersByMessagesAsync(long chatId, CancellationToken ct = default);
    /// <summary> Save user </summary>
    public Task SaveAsync(TgEfUserDto dto, CancellationToken ct = default);
    /// <summary> Save user list </summary>
    public Task SaveListAsync(IEnumerable<TgEfUserDto> dtos, CancellationToken ct = default);
}
