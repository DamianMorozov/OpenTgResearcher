namespace TgStorage.Contracts;

public interface ITgEfVersionRepository : ITgEfRepository<TgEfVersionEntity, TgEfVersionDto>, IDisposable
{
    public short LastVersion { get; }
	public Task<TgEfVersionEntity> GetLastVersionAsync();
    public Task FillTableVersionsAsync();
}
