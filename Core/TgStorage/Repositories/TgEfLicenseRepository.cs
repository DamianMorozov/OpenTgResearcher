namespace TgStorage.Repositories;

public sealed class TgEfLicenseRepository : TgEfRepositoryBase<TgEfLicenseEntity, TgEfLicenseDto>, ITgEfLicenseRepository
{
	#region Fields, properties, constructor

	public TgEfLicenseRepository() : base() { }

	public TgEfLicenseRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfLicenseEntity>> GetAsync(TgEfLicenseEntity item, bool isReadOnly = true, CancellationToken ct = default)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefaultAsync(ct);
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		
        // Find by LicenseKey
		itemFind = await GetQuery(isReadOnly).Where(x => x.LicenseKey == item.LicenseKey).SingleOrDefaultAsync(ct);
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfLicenseEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfLicenseEntity>> GetByDtoAsync(TgEfLicenseDto dto, bool isReadOnly = true, CancellationToken ct = default)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefaultAsync(ct);
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		
        // Find by LicenseKey
		itemFind = await GetQuery(isReadOnly).Where(x => x.LicenseKey == dto.LicenseKey).SingleOrDefaultAsync(ct);
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfLicenseEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfLicenseEntity> Get(TgEfLicenseEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefault();
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		
        // Find by LicenseKey
		itemFind = GetQuery(isReadOnly).Where(x => x.LicenseKey == item.LicenseKey).SingleOrDefault();
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfLicenseEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfLicenseEntity> GetByDto(TgEfLicenseDto dto, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefault();
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by LicenseKey
		itemFind = GetQuery(isReadOnly).Where(x => x.LicenseKey == dto.LicenseKey).SingleOrDefault();
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfLicenseEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
	public override async Task<TgEfStorageResult<TgEfLicenseEntity>> GetListAsync(int take, int skip, bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfLicenseEntity> items = take > 0
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync(ct)
			: await GetQuery(isReadOnly).ToListAsync(ct);
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
	public override async Task<TgEfStorageResult<TgEfLicenseEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfLicenseEntity, bool>> where, 
        bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfLicenseEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync(ct)
			: await GetQuery(isReadOnly).Where(where).ToListAsync(ct);
		
        return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(CancellationToken ct = default) => await EfContext.Licenses.AsNoTracking().CountAsync(ct);

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfLicenseEntity, bool>> where, CancellationToken ct = default) => 
        await EfContext.Licenses.AsNoTracking().Where(where).CountAsync(ct);

    #endregion

    #region Methods - ITgEfAppRepository

    /// <inheritdoc />
    public async Task<TgEfStorageResult<TgEfLicenseEntity>> GetCurrentAppAsync(bool isReadOnly = true)
	{
		var item = await
			EfContext.Licenses.AsTracking()
				.Where(x => x.Uid != Guid.Empty)
				.FirstOrDefaultAsync();
		
        return item is not null
			? new(TgEnumEntityState.IsExists, item)
			: new TgEfStorageResult<TgEfLicenseEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public TgEfStorageResult<TgEfLicenseEntity> GetCurrentApp(bool isReadOnly = true)
	{
        var item = EfContext.Licenses.AsTracking()
                .Where(x => x.Uid != Guid.Empty)
                .FirstOrDefault();

        return item is not null
            ? new(TgEnumEntityState.IsExists, item)
            : new TgEfStorageResult<TgEfLicenseEntity>(TgEnumEntityState.NotExists);
    }

    #endregion
}
