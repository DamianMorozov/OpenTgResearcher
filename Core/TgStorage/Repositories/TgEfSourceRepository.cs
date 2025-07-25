﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Repositories;

/// <summary> Source repository </summary>
public sealed class TgEfSourceRepository : TgEfRepositoryBase<TgEfSourceEntity, TgEfSourceDto>, ITgEfSourceRepository
{
	#region Public and private fields, properties, constructor

	public TgEfSourceRepository() : base() { }

	public TgEfSourceRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

	#endregion

	#region Public and private methods

	public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetAsync(TgEfSourceEntity item, bool isReadOnly = true)
	{
		try
		{
			// Too fast, read slower
			try
			{
				return await GetCoreAsync(item, isReadOnly);
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
				await Task.Delay(500);
				return await GetCoreAsync(item, isReadOnly);
			}
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

	private async Task<TgEfStorageResult<TgEfSourceEntity>> GetCoreAsync(TgEfSourceEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		//var itemFind = await GetQuery(isReadOnly)
		//	.Where(x => x.Uid == item.Uid)
		//	.FirstOrDefaultAsync();
		var itemFind = await GetQuery(isReadOnly)
			.FirstOrDefaultAsync(x => x.Uid == item.Uid);
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by ID
		if (item.Id > 0)
		{
			//itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.Id == item.Id);
			itemFind = await GetQuery(isReadOnly)
				.FirstOrDefaultAsync(x => x.Id == item.Id);
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
		}
		// Find by UserName
		if (!string.IsNullOrEmpty(item.UserName))
		{
			itemFind = await GetQuery(isReadOnly).FirstOrDefaultAsync(x => x.UserName == item.UserName);
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
		}
		// Default
		return new(TgEnumEntityState.NotExists, item);
	}

	public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfSourceEntity> items = take > 0 
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync() 
			: await GetQuery(isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfSourceEntity, bool>> where, bool isReadOnly = true)
	{
		var query = isReadOnly ? EfContext.Sources.AsNoTracking() : EfContext.Sources.AsTracking();
		IList<TgEfSourceEntity> items = take > 0 
			? await query.Where(where).Skip(skip).Take(take).ToListAsync() 
			: await query.Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    public override async Task<int> GetCountAsync() => await EfContext.Sources.AsNoTracking().CountAsync();

    public override async Task<int> GetCountAsync(Expression<Func<TgEfSourceEntity, bool>> where) => await EfContext.Sources.AsNoTracking().Where(where).CountAsync();

    #endregion

    #region Public and private methods - Delete

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfSourceEntity>> DeleteAllAsync()
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

    /// <inheritdoc />
    public async Task ResetAutoUpdateAsync()
    {
        await EfContext.Sources
            .Where(x => x.IsAutoUpdate)
            .ExecuteUpdateAsync(x => x.SetProperty(s => s.IsAutoUpdate, false));
        await EfContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SetIsUserAccess(bool isUserAccess)
    {
        await EfContext.Sources
            .ExecuteUpdateAsync(x => x.SetProperty(s => s.IsUserAccess, isUserAccess));
        await EfContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SetIsUserAccess(List<long> chatIds, bool isUserAccess)
    {
        await EfContext.Sources
            .Where(x => chatIds.Contains(x.Id))
            .ExecuteUpdateAsync(x => x.SetProperty(s => s.IsUserAccess, isUserAccess));
        await EfContext.SaveChangesAsync();
    }

    #endregion
}