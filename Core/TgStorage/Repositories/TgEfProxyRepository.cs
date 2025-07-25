﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Repositories;

/// <summary> Proxy repository </summary>
public sealed class TgEfProxyRepository : TgEfRepositoryBase<TgEfProxyEntity, TgEfProxyDto>, ITgEfProxyRepository
{
	#region Public and private fields, properties, constructor

	public TgEfProxyRepository() : base() { }

	public TgEfProxyRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

	#endregion

	#region Public and private methods

	public override async Task<TgEfStorageResult<TgEfProxyEntity>> GetAsync(TgEfProxyEntity item, bool isReadOnly = true)
	{
		try
		{
			// Find by Uid
			var itemFind = await GetQuery(isReadOnly)
				.Where(x => x.Uid == item.Uid)
				.FirstOrDefaultAsync();
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
			// Find by Type
			itemFind = await GetQuery(isReadOnly)
				.SingleOrDefaultAsync(x => x.Type == item.Type && x.HostName == item.HostName && x.Port == item.Port);
			return itemFind is not null
				? new(TgEnumEntityState.IsExists, itemFind)
				: new TgEfStorageResult<TgEfProxyEntity>(TgEnumEntityState.NotExists, item);
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

	public override async Task<TgEfStorageResult<TgEfProxyEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfProxyEntity> items = take > 0 
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync() 
			: await GetQuery(isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfProxyEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfProxyEntity, bool>> where, bool isReadOnly = true)
	{
		var items = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync()
			: (IList<TgEfProxyEntity>)await GetQuery(isReadOnly).Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    public override async Task<int> GetCountAsync() => await EfContext.Proxies.AsNoTracking().CountAsync();

    public override async Task<int> GetCountAsync(Expression<Func<TgEfProxyEntity, bool>> where) => await EfContext.Proxies.AsNoTracking().Where(where).CountAsync();

    #endregion

    #region Public and private methods - Delete

    public override async Task<TgEfStorageResult<TgEfProxyEntity>> DeleteAllAsync()
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

	#region Public and private methods - ITgEfProxyRepository

	public async Task<TgEfStorageResult<TgEfProxyEntity>> GetCurrentProxyAsync(Guid? uid)
	{
		var storageResultProxy = await GetAsync(new() { Uid = uid ?? Guid.Empty }, isReadOnly: false);
		return storageResultProxy.IsExists ? storageResultProxy : new(TgEnumEntityState.NotExists);
	}

    public async Task<Guid> GetCurrentProxyUidAsync()
    {
        var dto = await GetCurrentDtoAsync();
        return dto.Uid;
    }

    public TgEfProxyDto GetCurrentAppDto()
    {
        var task = GetCurrentDtoAsync();
        task.Wait();
        return task.Result;
    }

    public async Task<TgEfProxyDto> GetCurrentDtoAsync() => await
        EfContext.Proxies.AsTracking()
            .Where(x => x.Uid != Guid.Empty)
            .Select(x => new TgEfProxyDto()
            {
                Uid = x.Uid,
                Type = x.Type,
                HostName = x.HostName,
                Port = x.Port,
                UserName = x.UserName,
                Password = x.Password,
                Secret = x.Secret
            })
            .FirstOrDefaultAsync()
        ?? new();

    public async Task<TgEfProxyDto> GetDtoAsync(Guid? proxyUid) => await
        EfContext.Proxies.AsTracking()
            .Where(x => x.Uid == proxyUid)
            .Select(x => new TgEfProxyDto()
            {
                Uid = x.Uid,
                Type = x.Type,
                HostName = x.HostName,
                Port = x.Port,
                UserName = x.UserName,
                Password = x.Password,
                Secret = x.Secret
            })
            .FirstOrDefaultAsync()
        ?? new();

    #endregion
}