namespace TgStorage.Repositories;

/// <summary> EF app repository </summary>
public sealed class TgEfAppRepository : TgEfRepositoryBase<TgEfAppEntity, TgEfAppDto>, ITgEfAppRepository
{
	#region Fields, properties, constructor

	public TgEfAppRepository() : base() { }

	public TgEfAppRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfAppEntity>> GetAsync(TgEfAppEntity item, bool isReadOnly = true, CancellationToken ct = default)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefaultAsync(ct);
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		
        // Find by ApiHash
		itemFind = await GetQuery(isReadOnly).Where(x => x.ApiHash == item.ApiHash).Include(x => x.Proxy).SingleOrDefaultAsync(ct);
		
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfAppEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfAppEntity>> GetByDtoAsync(TgEfAppDto dto, bool isReadOnly = true, CancellationToken ct = default)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
            .Where(x => x.Uid == dto.Uid)
            .FirstOrDefaultAsync(ct);
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by ApiHash
		itemFind = await GetQuery(isReadOnly).Where(x => x.ApiHash == dto.ApiHash).Include(x => x.Proxy).SingleOrDefaultAsync();
		return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfAppEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfAppEntity> Get(TgEfAppEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefault();

		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);

		// Find by ApiHash
		itemFind = GetQuery(isReadOnly).Where(x => x.ApiHash == item.ApiHash).Include(x => x.Proxy).SingleOrDefault();
		
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfAppEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfAppEntity> GetByDto(TgEfAppDto dto, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefault();
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by ApiHash
		itemFind = GetQuery(isReadOnly).Where(x => x.ApiHash == dto.ApiHash).Include(x => x.Proxy).SingleOrDefault();
		return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfAppEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfAppEntity>> GetListAsync(int take, int skip, bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfAppEntity> items = take > 0
			? await GetQuery(isReadOnly).Include(x => x.Proxy).Skip(skip).Take(take).ToListAsync(ct)
			: await GetQuery(isReadOnly).Include(x => x.Proxy).ToListAsync(ct);
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfAppEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfAppEntity, bool>> where, 
        bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfAppEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where).Include(x => x.Proxy).Skip(skip).Take(take).ToListAsync(ct)
			: await GetQuery(isReadOnly).Where(where).Include(x => x.Proxy).ToListAsync(ct);
		
        return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(CancellationToken ct = default) => await EfContext.Apps.AsNoTracking().CountAsync(ct);

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfAppEntity, bool>> where, CancellationToken ct = default) => 
        await EfContext.Apps.AsNoTracking().Where(where).CountAsync(ct);

    #endregion

    #region Methods - ITgEfAppRepository

    /// <inheritdoc />
    public async Task<TgEfStorageResult<TgEfAppEntity>> GetCurrentAppAsync(bool isReadOnly = true)
	{
		var item = await
			EfContext.Apps.AsTracking()
				.Where(x => x.Uid != Guid.Empty)
				.Include(x => x.Proxy)
				.FirstOrDefaultAsync();
		
        return item is not null
			? new(TgEnumEntityState.IsExists, item)
			: new TgEfStorageResult<TgEfAppEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public async Task<TgEfAppDto> GetCurrentAppDtoAsync() => await
		EfContext.Apps.AsTracking()
			.Where(x => x.Uid != Guid.Empty)
			.Include(x => x.Proxy)
            .Select(x => TgEfDomainUtils.CreateNewDto(x, true))
			.FirstOrDefaultAsync()
        ?? new();

    /// <inheritdoc />
    public async Task<TgEfAppDto> GetCurrentDtoAsync(CancellationToken ct = default) => await
		EfContext.Apps.AsTracking()
			.Where(x => x.Uid != Guid.Empty)
            .Select(x => TgEfDomainUtils.CreateNewDto(x, true))
            .FirstOrDefaultAsync(ct)
		?? new();

    /// <inheritdoc />
    public TgEfStorageResult<TgEfAppEntity> GetCurrentApp(bool isReadOnly = true)
	{
		var task = GetCurrentAppAsync(isReadOnly);
		task.Wait();
		return task.Result;
	}

    /// <inheritdoc />
    public async Task<Guid> GetCurrentAppUidAsync() => (await GetCurrentAppAsync()).Item?.Uid ?? Guid.Empty;

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

    /// <inheritdoc />
    public async Task ClearProxyAsync()
    {
        await EfContext.Apps
            .ExecuteUpdateAsync(updates => updates.SetProperty(a => a.ProxyUid, a => (Guid?)null));
    }

    /// <inheritdoc />
    public async Task SaveAsync(TgEfAppDto dto, CancellationToken ct = default)
    {
        TgEfAppEntity? entity = null;
        try
        {
            if (dto is null)
                return;

            // Try to find existing entity
            entity = await GetQuery(isReadOnly: false).SingleOrDefaultAsync(x => x.Uid == dto.Uid, ct);
            if (entity is null)
                // Find by ApiHash
                entity = await GetQuery(isReadOnly: false).SingleOrDefaultAsync(x => x.ApiHash == dto.ApiHash, ct);

            if (entity is null)
            {
                if (EfContext is DbContext dbContext)
                    dbContext.ChangeTracker.Clear();
                // Create new entity
                entity = TgEfDomainUtils.CreateNewEntity(dto, isUidCopy: true);
                EfContext.Apps.Add(entity);
            }
            else
            {
                // Update existing entity
                TgEfDomainUtils.FillEntity(dto, entity, isUidCopy: false);
            }

            ValidateAndNormalize(entity);
            await EfContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error saving app");
            throw;
        }
        finally
        {
            // Avoid EF tracking issues
            if (entity is not null)
                EfContext.DetachItem(entity);
        }
    }

    /// <inheritdoc />
    public async Task SaveListAsync(IEnumerable<TgEfAppDto> dtos, CancellationToken ct = default)
    {
        try
        {
            foreach (var dto in dtos)
            {
                await SaveAsync(dto, ct);
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error saving apps");
            throw;
        }
    }

    #endregion
}
