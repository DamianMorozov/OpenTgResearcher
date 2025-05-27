// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Repositories;

public sealed class TgEfLicenseRepository : TgEfRepositoryBase<TgEfLicenseEntity, TgEfLicenseDto>, ITgEfLicenseRepository
{
	#region Public and private fields, properties, constructor

	public TgEfLicenseRepository() : base() { }

	public TgEfLicenseRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

	#endregion

	#region Public and private methods

    public override async Task<TgEfStorageResult<TgEfLicenseEntity>> GetAsync(TgEfLicenseEntity item, bool isReadOnly = true)
	{
		try
		{
			// Find by Uid
			var itemFind = await GetQuery(isReadOnly)
				.Where(x => x.Uid == item.Uid)
				.FirstOrDefaultAsync();
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
			// Find by LicenseKey
			itemFind = await GetQuery(isReadOnly).Where(x => x.LicenseKey == item.LicenseKey).SingleOrDefaultAsync();
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
			// Find by UserId
			//itemFind = await GetQuery(isReadOnly).Where(x => x.UserId == item.UserId).SingleOrDefaultAsync();
			return itemFind is not null
				? new(TgEnumEntityState.IsExists, itemFind)
				: new TgEfStorageResult<TgEfLicenseEntity>(TgEnumEntityState.NotExists, item);
		}
#if DEBUG
		catch (Exception ex)
		{
			Debug.WriteLine(ex, TgConstants.LogTypeStorage);
			Debug.WriteLine(ex.StackTrace);
#else
		catch (Exception)
		{
#endif
			throw;
		}
	}

	public override async Task<TgEfStorageResult<TgEfLicenseEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfLicenseEntity> items = take > 0
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfLicenseEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfLicenseEntity, bool>> where, bool isReadOnly = true)
	{
		IList<TgEfLicenseEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<int> GetCountAsync()
	{
		return await EfContext.Licenses.AsNoTracking().CountAsync();
	}

	public override async Task<int> GetCountAsync(Expression<Func<TgEfLicenseEntity, bool>> where)
	{
		return await EfContext.Licenses.AsNoTracking().Where(where).CountAsync();
	}

	#endregion

	#region Public and private methods - Delete

	public override async Task<TgEfStorageResult<TgEfLicenseEntity>> DeleteAllAsync()
	{
		var storageResult = await GetListAsync(0, 0, isReadOnly: false);
		if (storageResult.IsExists)
		{
			foreach (var item in storageResult.Items)
			{
				await DeleteAsync(item);
			}
		}
		return new(storageResult.IsExists ? TgEnumEntityState.IsDeleted : TgEnumEntityState.NotDeleted);
	}

	#endregion

	#region Public and private methods - ITgEfAppRepository

	public async Task<TgEfStorageResult<TgEfLicenseEntity>> GetCurrentAppAsync(bool isReadOnly = true)
	{
		var item = await
			EfContext.Licenses.AsTracking()
				.Where(x => x.Uid != Guid.Empty)
				.FirstOrDefaultAsync();
		return item is not null
			? new(TgEnumEntityState.IsExists, item)
			: new TgEfStorageResult<TgEfLicenseEntity>(TgEnumEntityState.NotExists, new TgEfLicenseEntity());
	}

	public TgEfStorageResult<TgEfLicenseEntity> GetCurrentApp(bool isReadOnly = true)
	{
		var task = GetCurrentAppAsync(isReadOnly);
		task.Wait();
		return task.Result;
	}

	public async Task<Guid> GetCurrentAppUidAsync() => (await GetCurrentAppAsync()).Item.Uid;

    #endregion
}