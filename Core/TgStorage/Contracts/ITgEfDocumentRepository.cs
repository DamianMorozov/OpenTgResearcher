namespace TgStorage.Contracts;

public interface ITgEfDocumentRepository : ITgEfRepository<TgEfDocumentEntity, TgEfDocumentDto>, IDisposable
{
    /// <summary> Save document </summary>
    public Task SaveAsync(TgEfDocumentDto dto, CancellationToken ct = default);
    /// <summary> Save document list </summary>
    public Task SaveListAsync(IEnumerable<TgEfDocumentDto> dtos, CancellationToken ct = default);
}
