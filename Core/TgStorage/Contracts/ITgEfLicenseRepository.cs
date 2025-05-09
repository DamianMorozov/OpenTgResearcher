// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com


namespace TgStorage.Contracts;

public interface ITgEfLicenseRepository : ITgEfRepository<TgEfLicenseEntity>
{
	public Task<List<TgEfLicenseDto>> GetListDtosAsync(int take = 0, int skip = 0);
	public Task<List<TgEfLicenseDto>> GetListDtosAsync(int take, int skip, Expression<Func<TgEfLicenseEntity, bool>> where);
}