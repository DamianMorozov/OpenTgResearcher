namespace TgStorage.Contracts;

public interface ITgEfVersionRepository : ITgEfRepository<TgEfVersionEntity, TgEfVersionDto>, IDisposable
{
    public short LastVersion { get; }
	public Task<TgEfVersionEntity> GetLastVersionAsync();
    public Task FillTableVersionsAsync();
    /// <summary> Save version </summary>
    public Task SaveAsync(TgEfVersionDto dto, CancellationToken ct = default);
    /// <summary> Save versions list </summary>
    public Task SaveListAsync(IEnumerable<TgEfVersionDto> dtos, CancellationToken ct = default);
}
