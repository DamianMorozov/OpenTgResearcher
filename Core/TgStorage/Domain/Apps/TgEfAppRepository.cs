﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Apps;

public sealed class TgEfAppRepository(TgEfContext efContext) : TgEfRepositoryBase<TgEfAppEntity>(efContext), ITgEfAppRepository
{
	#region Public and private methods

	public override string ToDebugString() => $"{nameof(TgEfAppRepository)}";

	public override IQueryable<TgEfAppEntity> GetQuery(bool isNoTracking) =>
		isNoTracking ? EfContext.Apps.AsNoTracking() : EfContext.Apps.AsTracking();
	
	public override async Task<TgEfStorageResult<TgEfAppEntity>> GetAsync(TgEfAppEntity item, bool isNoTracking)
	{
		TgEfStorageResult<TgEfAppEntity> storageResult = await base.GetAsync(item, isNoTracking);
		if (storageResult.IsExists)
			return storageResult;
		TgEfAppEntity? itemFind = await GetQuery(isNoTracking).Where(x => x.ApiHash == item.ApiHash).Include(x => x.Proxy).SingleOrDefaultAsync();
		return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfAppEntity>(TgEnumEntityState.NotExists, item);
	}

	public override async Task<TgEfStorageResult<TgEfAppEntity>> GetFirstAsync(bool isNoTracking)
	{
		TgEfAppEntity? item = await GetQuery(isNoTracking).Include(x => x.Proxy).FirstOrDefaultAsync();
		return item is null
			? new(TgEnumEntityState.NotExists)
			: new TgEfStorageResult<TgEfAppEntity>(TgEnumEntityState.IsExists, item);
	}

	public override async Task<TgEfStorageResult<TgEfAppEntity>> GetListAsync(int take, int skip, bool isNoTracking)
	{
		IList<TgEfAppEntity> items = take > 0
			? await GetQuery(isNoTracking).Include(x => x.Proxy).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isNoTracking).Include(x => x.Proxy).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfAppEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfAppEntity, bool>> where, bool isNoTracking)
	{
		IList<TgEfAppEntity> items = take > 0
			? await GetQuery(isNoTracking).Where(where).Include(x => x.Proxy).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isNoTracking).Where(where).Include(x => x.Proxy).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<int> GetCountAsync() => await EfContext.Apps.AsNoTracking().CountAsync();

	public override async Task<int> GetCountAsync(Expression<Func<TgEfAppEntity, bool>> where) => 
		await EfContext.Apps.AsNoTracking().Where(where).CountAsync();

	#endregion

	#region Public and private methods - Delete

	public override async Task<TgEfStorageResult<TgEfAppEntity>> DeleteAllAsync()
	{
		TgEfStorageResult<TgEfAppEntity> storageResult = await GetListAsync(0, 0, isNoTracking: false);
		if (storageResult.IsExists)
		{
			foreach (TgEfAppEntity item in storageResult.Items)
			{
				await DeleteAsync(item);
			}
		}
		return new(storageResult.IsExists ? TgEnumEntityState.IsDeleted : TgEnumEntityState.NotDeleted);
	}

	#endregion

	#region Public and private methods - ITgEfAppRepository

	public async Task<TgEfStorageResult<TgEfAppEntity>> GetCurrentAppAsync()
	{
		TgEfAppEntity? item = await
			EfContext.Apps.AsTracking()
				.Where(x => x.Uid != Guid.Empty)
				.Include(x => x.Proxy)
				.FirstOrDefaultAsync();
		return item is not null
			? new(TgEnumEntityState.IsExists, item)
			: new TgEfStorageResult<TgEfAppEntity>(TgEnumEntityState.NotExists, new TgEfAppEntity());
	}

	public async Task<Guid> GetCurrentAppUidAsync() => (await GetCurrentAppAsync()).Item.Uid;

	#endregion
}