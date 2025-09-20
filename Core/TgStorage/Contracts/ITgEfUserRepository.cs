
namespace TgStorage.Contracts;

public interface ITgEfUserRepository : ITgEfRepository<TgEfUserEntity, TgEfUserDto>, IDisposable
{
    /// <summary> Get all distinct users for a given source in a single query </summary>
    public Task<List<TgEfUserDto>> GetUsersBySourceIdAsync(long sourceId, CancellationToken ct = default);
    /// <summary> Get all distinct users for a given source using join </summary>
    public Task<List<TgEfUserDto>> GetUsersBySourceIdJoinAsync(long sourceId, long userId, CancellationToken ct = default);
    /// <summary> Get myself user for a given source </summary>
    public Task<TgEfUserDto> GetMyselfUserAsync(long sourceId, long userId, CancellationToken ct = default);
}
