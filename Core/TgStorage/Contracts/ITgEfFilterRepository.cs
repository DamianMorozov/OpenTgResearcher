namespace TgStorage.Contracts;

public interface ITgEfFilterRepository : ITgEfRepository<TgEfFilterEntity, TgEfFilterDto>, IDisposable
{
    /// <summary> Save filter </summary>
    public Task SaveAsync(TgEfFilterDto dto, CancellationToken ct = default);
    /// <summary> Save filter list </summary>
    public Task SaveListAsync(IEnumerable<TgEfFilterDto> dtos, CancellationToken ct = default);
}
