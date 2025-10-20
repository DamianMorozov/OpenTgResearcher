namespace TgStorage.Contracts;

public interface ITgEfStoryRepository : ITgEfRepository<TgEfStoryEntity, TgEfStoryDto>, IDisposable
{
    /// <summary> Save story </summary>
    public Task SaveAsync(TgEfStoryDto dto, CancellationToken ct = default);
    /// <summary> Save story list </summary>
    public Task SaveListAsync(IEnumerable<TgEfStoryDto> dtos, CancellationToken ct = default);
}
