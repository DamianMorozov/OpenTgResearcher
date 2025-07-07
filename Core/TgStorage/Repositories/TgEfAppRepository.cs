// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Repositories;

/// <summary> App repository </summary>
public sealed class TgEfAppRepository : TgEfRepositoryBase<TgEfAppEntity, TgEfAppDto>, ITgEfAppRepository
{
	#region Public and private fields, properties, constructor

	public TgEfAppRepository() : base() { }

	public TgEfAppRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

	#endregion

	#region Public and private methods

	public override async Task<TgEfStorageResult<TgEfAppEntity>> GetAsync(TgEfAppEntity item, bool isReadOnly = true)
	{
		try
		{
			// Find by Uid
			var itemFind = await GetQuery(isReadOnly)
				.Where(x => x.Uid == item.Uid)
				.FirstOrDefaultAsync();
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
			// Find by ApiHash
			itemFind = await GetQuery(isReadOnly).Where(x => x.ApiHash == item.ApiHash).Include(x => x.Proxy).SingleOrDefaultAsync();
			return itemFind is not null
				? new(TgEnumEntityState.IsExists, itemFind)
				: new TgEfStorageResult<TgEfAppEntity>(TgEnumEntityState.NotExists, item);
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

	public override async Task<TgEfStorageResult<TgEfAppEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfAppEntity> items = take > 0
			? await GetQuery(isReadOnly).Include(x => x.Proxy).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).Include(x => x.Proxy).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfAppEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfAppEntity, bool>> where, bool isReadOnly = true)
	{
		IList<TgEfAppEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where).Include(x => x.Proxy).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).Where(where).Include(x => x.Proxy).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<int> GetCountAsync()
	{
		return await EfContext.Apps.AsNoTracking().CountAsync();
	}

	public override async Task<int> GetCountAsync(Expression<Func<TgEfAppEntity, bool>> where)
	{
		return await EfContext.Apps.AsNoTracking().Where(where).CountAsync();
	}

	#endregion

	#region Public and private methods - Delete

	public override async Task<TgEfStorageResult<TgEfAppEntity>> DeleteAllAsync()
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

	public async Task<TgEfStorageResult<TgEfAppEntity>> GetCurrentAppAsync(bool isReadOnly = true)
	{
		var item = await
			EfContext.Apps.AsTracking()
				.Where(x => x.Uid != Guid.Empty)
				.Include(x => x.Proxy)
				.FirstOrDefaultAsync();
		return item is not null
			? new(TgEnumEntityState.IsExists, item)
			: new TgEfStorageResult<TgEfAppEntity>(TgEnumEntityState.NotExists, new TgEfAppEntity());
	}

	public TgEfAppDto GetCurrentAppDto()
	{
		var task = GetCurrentDtoAsync();
		task.Wait();
		return task.Result;
	}

	public async Task<TgEfAppDto> GetCurrentDtoAsync() => await
		EfContext.Apps.AsTracking()
			.Where(x => x.Uid != Guid.Empty)
			.Select(x => new TgEfAppDto() {
                Uid = x.Uid,
                ApiHash = x.ApiHash, 
                ApiId = x.ApiId, 
                FirstName = x.FirstName, 
                LastName = x.LastName, 
                PhoneNumber = x.PhoneNumber,
				ProxyUid = x.ProxyUid ?? Guid.Empty,
                UseBot = x.UseBot,
                BotTokenKey = x.BotTokenKey, 
                UseClient = x.UseClient,
            })
			.FirstOrDefaultAsync()
		?? new();

	public TgEfStorageResult<TgEfAppEntity> GetCurrentApp(bool isReadOnly = true)
	{
		var task = GetCurrentAppAsync(isReadOnly);
		task.Wait();
		return task.Result;
	}

	public async Task<Guid> GetCurrentAppUidAsync() => (await GetCurrentAppAsync()).Item.Uid;

    /// <inheritdoc />
    public async Task<bool> SetUseBotAsync(bool useBot)
    {
        var result = await EfContext.Apps
            .ExecuteUpdateAsync(x => x.SetProperty(a => a.UseBot, useBot));
        if (result > 0)
            return true;

        // Create app entity
        var app = new TgEfAppEntity { UseBot = useBot };
        EfContext.Apps.Add(app);
        result = await EfContext.SaveChangesAsync();
        return result > 0;
    }

    /// <inheritdoc />
    public async Task<bool> SetBotTokenKeyAsync(string botTokenKey)
    {
        var result = await EfContext.Apps
            .ExecuteUpdateAsync(x => x.SetProperty(a => a.BotTokenKey, botTokenKey));
        if (result > 0)
            return true;

        // Create app entity
        var app = new TgEfAppEntity { BotTokenKey = botTokenKey };
        EfContext.Apps.Add(app);
        result = await EfContext.SaveChangesAsync();
        return result > 0;
    }

    /// <inheritdoc />
    public async Task<bool> SetUseClientAsync(bool useClient)
    {
        var result = await EfContext.Apps
            .ExecuteUpdateAsync(x => x.SetProperty(a => a.UseClient, useClient));
        if (result > 0)
            return true;

        // Create app entity
        var app = new TgEfAppEntity { UseClient = useClient };
        EfContext.Apps.Add(app);
        result = await EfContext.SaveChangesAsync();
        return result > 0;
    }

    #endregion
}