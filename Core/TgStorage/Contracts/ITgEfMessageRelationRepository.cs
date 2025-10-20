namespace TgStorage.Contracts;

public interface ITgEfMessageRelationRepository : ITgEfRepository<TgEfMessageRelationEntity, TgEfMessageRelationDto>, IDisposable
{
    /// <summary> Save message relation </summary>
    public Task SaveAsync(TgEfMessageRelationDto dto, CancellationToken ct = default);
    /// <summary> Save message relation list </summary>
    public Task SaveListAsync(IEnumerable<TgEfMessageRelationDto> dtos, CancellationToken ct = default);

}
