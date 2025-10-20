using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TgStorage.Repositories;

/// <summary> EF proxy repository </summary>
public sealed class TgEfProxyRepository : TgEfRepositoryBase<TgEfProxyEntity, TgEfProxyDto>, ITgEfProxyRepository
{
	#region Fields, properties, constructor

	public TgEfProxyRepository() : base() { }

	public TgEfProxyRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfProxyEntity>> GetAsync(TgEfProxyEntity item, bool isReadOnly = true, CancellationToken ct = default)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefaultAsync(ct);

		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);

        // Find by Type and HostName and Port
        itemFind = await GetQuery(isReadOnly)
			.SingleOrDefaultAsync(x => x.Type == item.Type && x.HostName == item.HostName && x.Port == item.Port, ct);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfProxyEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfProxyEntity>> GetByDtoAsync(TgEfProxyDto dto, bool isReadOnly = true, CancellationToken ct = default)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefaultAsync(ct);
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		
        // Find by Type
		itemFind = await GetQuery(isReadOnly)
			.SingleOrDefaultAsync(x => x.Type == dto.Type && x.HostName == dto.HostName && x.Port == dto.Port, ct);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfProxyEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfProxyEntity> Get(TgEfProxyEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefault();
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		
        // Find by Type
		itemFind = GetQuery(isReadOnly)
			.SingleOrDefault(x => x.Type == item.Type && x.HostName == item.HostName && x.Port == item.Port);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfProxyEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfProxyEntity> GetByDto(TgEfProxyDto dto, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefault();
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by Type
		itemFind = GetQuery(isReadOnly)
			.SingleOrDefault(x => x.Type == dto.Type && x.HostName == dto.HostName && x.Port == dto.Port);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfProxyEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfProxyEntity>> GetListAsync(int take, int skip, bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfProxyEntity> items = take > 0 
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync(ct) 
			: await GetQuery(isReadOnly).ToListAsync(ct);
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfProxyEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfProxyEntity, bool>> where, 
        bool isReadOnly = true, CancellationToken ct = default)
	{
		var items = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync(ct)
			: (IList<TgEfProxyEntity>)await GetQuery(isReadOnly).Where(where).ToListAsync(ct);

		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(CancellationToken ct = default) => await EfContext.Proxies.AsNoTracking().CountAsync(ct);

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfProxyEntity, bool>> where, CancellationToken ct = default) => 
        await EfContext.Proxies.AsNoTracking().Where(where).CountAsync(ct);

    #endregion

    #region Methods - ITgEfProxyRepository

    /// <inheritdoc />
    public async Task<TgEfStorageResult<TgEfProxyEntity>> GetCurrentProxyAsync(Guid? uid)
	{
		var storageResultProxy = await GetAsync(new() { Uid = uid ?? Guid.Empty }, isReadOnly: false);
		return storageResultProxy.IsExists ? storageResultProxy : new(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public async Task<Guid> GetCurrentProxyUidAsync()
    {
        var dto = await GetCurrentDtoAsync();
        return dto.Uid;
    }

    /// <inheritdoc />
    public TgEfProxyDto GetCurrentAppDto()
    {
        var task = GetCurrentDtoAsync();
        task.Wait();
        return task.Result;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task CreateDefaultAsync()
    {
        try
        {
            // Build minimal default proxy entity
            await SaveAsync(new TgEfProxyDto());
        }
        catch (Exception ex)
        {
            // Log and swallow to keep application stable
            TgLogUtils.WriteException(ex, "Error creating default proxy!");
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(TgEfProxyDto dto, CancellationToken ct = default)
    {
        TgEfProxyEntity? entity = null;
        try
        {
            if (dto is null)
                return;

            // Try to find existing entity
            entity = await GetQuery(isReadOnly: false).SingleOrDefaultAsync(x => x.Uid == dto.Uid);
            // Find by Type and HostName and Port
            if (entity is null)
                entity = await GetQuery(isReadOnly: false).SingleOrDefaultAsync(x => x.Type == dto.Type && x.HostName == dto.HostName && x.Port == dto.Port);

            if (entity is null)
            {
                if (EfContext is DbContext dbContext)
                    dbContext.ChangeTracker.Clear();
                // Create new entity
                entity = TgEfDomainUtils.CreateNewEntity(dto, isUidCopy: true);
                EfContext.Proxies.Add(entity);
            }
            else
            {
                // Update existing entity
                TgEfDomainUtils.FillEntity(dto, entity, isUidCopy: true);
            }

            ValidateAndNormalize(entity);
            await EfContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error saving proxy");
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
    public async Task SaveListAsync(IEnumerable<TgEfProxyDto> dtos, CancellationToken ct = default)
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
            TgLogUtils.WriteException(ex, "Error saving proxies");
            throw;
        }
    }

    #endregion
}
