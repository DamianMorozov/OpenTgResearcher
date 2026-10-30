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
            // Создаём стандартную запись без прокси
            await SaveAsync(new TgEfProxyDto());
        }
        catch (Exception ex)
        {
            // Передаём ошибку вызывающей странице после журналирования
            TgLogUtils.WriteExceptionWithMessage(ex, "Ошибка создания стандартного прокси");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(TgEfProxyDto dto, CancellationToken ct = default)
    {
        TgEfProxyEntity? entity = null;
        try
        {
            ArgumentNullException.ThrowIfNull(dto);
            var candidate = TgEfDomainUtils.CreateNewEntity(dto, isUidCopy: true);
            candidate.HostName = candidate.HostName.Trim();
            ValidateAndNormalize(candidate);
            entity = await GetQuery(isReadOnly: false).SingleOrDefaultAsync(x => x.Uid == dto.Uid, ct);
            var duplicate = await GetQuery().FirstOrDefaultAsync(x => x.Uid != dto.Uid && x.Type == candidate.Type &&
                x.HostName == candidate.HostName && x.Port == candidate.Port, ct);
            if (duplicate is not null)
            {
                if (candidate.Type != TgEnumProxyType.None || entity is not null)
                    throw new InvalidOperationException("Прокси с таким типом, адресом и портом уже существует");
                dto.Uid = duplicate.Uid;
                return;
            }
            if (entity is null)
            {
                entity = candidate;
                EfContext.Proxies.Add(entity);
            }
            else
            {
                TgEfDomainUtils.FillEntity(TgEfDomainUtils.CreateNewDto(candidate, isUidCopy: true), entity, isUidCopy: false);
            }
            await EfContext.SaveChangesAsync(ct);
            dto.Uid = entity.Uid;
            dto.HostName = entity.HostName;
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteExceptionWithMessage(ex, "Ошибка сохранения прокси");
            throw;
        }
        finally
        {
            // Освобождаем только изменённую запись, сохраняя остальные отслеживаемые объекты
            if (entity is not null) EfContext.DetachItem(entity);
        }
    }

    /// <inheritdoc />
    public override Task<TgEfStorageResult<TgEfProxyEntity>> DeleteAsync(TgEfProxyEntity item, CancellationToken ct = default) =>
        DeleteAsync(x => x.Uid == item.Uid, ct);

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfProxyEntity>> DeleteAsync(Expression<Func<TgEfProxyEntity, bool>> where, CancellationToken ct = default)
    {
        try
        {
            await using var transaction = await EfContext.Database.BeginTransactionAsync(ct);
            var proxies = EfContext.Proxies.Where(where);
            await EfContext.Apps.Where(x => x.ProxyUid != null && proxies.Any(p => p.Uid == x.ProxyUid))
                .ExecuteUpdateAsync(x => x.SetProperty(a => a.ProxyUid, (Guid?)null), ct);
            var count = await proxies.ExecuteDeleteAsync(ct);
            await transaction.CommitAsync(ct);
            return new(count > 0 ? TgEnumEntityState.IsDeleted : TgEnumEntityState.NotDeleted);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteExceptionWithMessage(ex, "Ошибка удаления прокси");
            throw;
        }
    }

    /// <inheritdoc />
    public override Task<TgEfStorageResult<TgEfProxyEntity>> DeleteAllAsync(CancellationToken ct = default) => DeleteAsync(x => true, ct);

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
