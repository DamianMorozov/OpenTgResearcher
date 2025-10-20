namespace TgStorage.Contracts;

public interface ITgEfLicenseRepository : ITgEfRepository<TgEfLicenseEntity, TgEfLicenseDto>, IDisposable
{
    /// <summary> Save license </summary>
    public Task SaveAsync(TgLicenseDto dto, CancellationToken ct = default);
    /// <summary> Save license </summary>
    public Task SaveAsync(TgEfLicenseDto dto, CancellationToken ct = default);
    /// <summary> Save license list </summary>
    public Task SaveListAsync(IEnumerable<TgLicenseDto> dtos, CancellationToken ct = default);
    /// <summary> Save license list </summary>
    public Task SaveListAsync(IEnumerable<TgEfLicenseDto> dtos, CancellationToken ct = default);
}
