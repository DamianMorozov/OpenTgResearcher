﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Sources;

public sealed class TgEfSourceRepository(TgEfContext efContext) : TgEfRepositoryBase<TgEfSourceEntity>(efContext)
{
	#region Public and private methods

	public override string ToDebugString() => $"{nameof(TgEfSourceRepository)}";

	public override TgEfStorageResult<TgEfSourceEntity> Get(TgEfSourceEntity item, bool isNoTracking)
	{
		TgEfStorageResult<TgEfSourceEntity> storageResult = base.Get(item, isNoTracking);
		if (storageResult.IsExists)
			return storageResult;
		TgEfSourceEntity? itemFind = isNoTracking
			? EfContext.Sources.AsNoTracking()
				.Where(x => x.Id == item.Id)
				.SingleOrDefault()
			: EfContext.Sources.AsTracking()
				.Where(x => x.Id == item.Id)
				.SingleOrDefault();
		return itemFind is not null
			? new TgEfStorageResult<TgEfSourceEntity>(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfSourceEntity>(TgEnumEntityState.NotExists, item);
	}

	public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetAsync(TgEfSourceEntity item, bool isNoTracking)
	{
		TgEfStorageResult<TgEfSourceEntity> storageResult = await base.GetAsync(item, isNoTracking).ConfigureAwait(false);
		if (storageResult.IsExists)
			return storageResult;
		TgEfSourceEntity? itemFind = isNoTracking
			? await EfContext.Sources.AsNoTracking()
				.Where(x => x.Id == item.Id)
				.SingleOrDefaultAsync()
				.ConfigureAwait(false)
			: await EfContext.Sources.AsTracking()
				.Where(x => x.Id == item.Id)
				.SingleOrDefaultAsync()
				.ConfigureAwait(false);
		return itemFind is not null
			? new TgEfStorageResult<TgEfSourceEntity>(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfSourceEntity>(TgEnumEntityState.NotExists, item);
	}

	public override TgEfStorageResult<TgEfSourceEntity> GetFirst(bool isNoTracking)
	{
		TgEfSourceEntity? item = isNoTracking
			? EfContext.Sources.AsNoTracking().FirstOrDefault()
			: EfContext.Sources.AsTracking().FirstOrDefault();
		return item is null
			? new TgEfStorageResult<TgEfSourceEntity>(TgEnumEntityState.NotExists)
			: new TgEfStorageResult<TgEfSourceEntity>(TgEnumEntityState.IsExists, item);
	}

	public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetFirstAsync(bool isNoTracking)
	{
		TgEfSourceEntity? item = isNoTracking
			? await EfContext.Sources.AsNoTracking().FirstOrDefaultAsync().ConfigureAwait(false)
			: await EfContext.Sources.AsTracking().FirstOrDefaultAsync().ConfigureAwait(false);
		return item is null
			? new TgEfStorageResult<TgEfSourceEntity>(TgEnumEntityState.NotExists)
			: new TgEfStorageResult<TgEfSourceEntity>(TgEnumEntityState.IsExists, item);
	}

	public override TgEfStorageResult<TgEfSourceEntity> GetList(int take, int skip, bool isNoTracking)
	{
		IList<TgEfSourceEntity> items;
		if (take > 0)
		{
			items = isNoTracking
				? EfContext.Sources.AsNoTracking().Skip(skip).Take(take).ToList()
				: EfContext.Sources.AsTracking().Skip(skip).Take(take).ToList();
		}
		else
		{
			items = isNoTracking
				? EfContext.Sources.AsNoTracking().ToList()
				: EfContext.Sources.AsTracking().ToList();
		}
		return new TgEfStorageResult<TgEfSourceEntity>(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetListAsync(int take, int skip, bool isNoTracking)
	{
		await Task.Delay(TimeSpan.FromMilliseconds(1)).ConfigureAwait(false);
		IList<TgEfSourceEntity> items;
		if (take > 0)
		{
			items = isNoTracking
				? EfContext.Sources.AsNoTracking().Skip(skip).Take(take).ToList()
				: EfContext.Sources.AsTracking().Skip(skip).Take(take).ToList();
		}
		else
		{
			items = isNoTracking
				? EfContext.Sources.AsNoTracking().ToList()
				: EfContext.Sources.AsTracking().ToList();
		}
		return new TgEfStorageResult<TgEfSourceEntity>(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override TgEfStorageResult<TgEfSourceEntity> GetList(int take, int skip, Expression<Func<TgEfSourceEntity, bool>> where, bool isNoTracking)
	{
		IList<TgEfSourceEntity> items;
		if (take > 0)
		{
			items = isNoTracking
				? EfContext.Sources.AsNoTracking()
					.Where(where)
					.Skip(skip).Take(take).ToList()
				: EfContext.Sources.AsTracking()
					.Where(where)
					.Skip(skip).Take(take).ToList();
		}
		else
		{
			items = isNoTracking
				? EfContext.Sources.AsNoTracking()
					.Where(where)
					.ToList()
				: EfContext.Sources.AsTracking()
					.Where(where)
					.ToList();
		}
		return new TgEfStorageResult<TgEfSourceEntity>(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfSourceEntity, bool>> where, bool isNoTracking)
	{
		await Task.Delay(TimeSpan.FromMilliseconds(1)).ConfigureAwait(false);
		IList<TgEfSourceEntity> items;
		if (take > 0)
		{
			items = isNoTracking
				? EfContext.Sources.AsNoTracking()
					.Where(where)
					.Skip(skip).Take(take).ToList()
				: EfContext.Sources.AsTracking()
					.Where(where)
					.Skip(skip).Take(take).ToList();
		}
		else
		{
			items = isNoTracking
				? EfContext.Sources.AsNoTracking()
					.Where(where)
					.ToList()
				: EfContext.Sources.AsTracking()
					.Where(where)
					.ToList();
		}
		return new TgEfStorageResult<TgEfSourceEntity>(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override int GetCount() => EfContext.Sources.AsNoTracking().Count();

	public override async Task<int> GetCountAsync() => await EfContext.Sources.AsNoTracking().CountAsync().ConfigureAwait(false);

	public override int GetCount(Expression<Func<TgEfSourceEntity, bool>> where) => 
		EfContext.Sources.AsNoTracking().Where(where).Count();

	public override async Task<int> GetCountAsync(Expression<Func<TgEfSourceEntity, bool>> where) => 
		await EfContext.Sources.AsNoTracking().Where(where).CountAsync().ConfigureAwait(false);

	#endregion

	#region Public and private methods - Write

	//

	#endregion

	#region Public and private methods - Delete

	public override TgEfStorageResult<TgEfSourceEntity> DeleteAll()
	{
		TgEfStorageResult<TgEfSourceEntity> storageResult = GetList(0, 0, isNoTracking: false);
		if (storageResult.IsExists)
		{
			foreach (TgEfSourceEntity item in storageResult.Items)
			{
				Delete(item, isSkipFind: true);
			}
		}
		return new TgEfStorageResult<TgEfSourceEntity>(storageResult.IsExists ? TgEnumEntityState.IsDeleted : TgEnumEntityState.NotDeleted);
	}

	public override async Task<TgEfStorageResult<TgEfSourceEntity>> DeleteAllAsync()
	{
		TgEfStorageResult<TgEfSourceEntity> storageResult = await GetListAsync(0, 0, isNoTracking: false).ConfigureAwait(false);
		if (storageResult.IsExists)
		{
			foreach (TgEfSourceEntity item in storageResult.Items)
			{
				await DeleteAsync(item, isSkipFind: true).ConfigureAwait(false);
			}
		}
		return new TgEfStorageResult<TgEfSourceEntity>(storageResult.IsExists ? TgEnumEntityState.IsDeleted : TgEnumEntityState.NotDeleted);
	}

	#endregion
}