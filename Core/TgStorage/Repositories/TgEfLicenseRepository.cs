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

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfLicenseEntity>> GetAsync(TgEfLicenseEntity item, bool isReadOnly = true)
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
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfLicenseEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfLicenseEntity>> GetByDtoAsync(TgEfLicenseDto dto, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefaultAsync();
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by LicenseKey
		itemFind = await GetQuery(isReadOnly).Where(x => x.LicenseKey == dto.LicenseKey).SingleOrDefaultAsync();
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
	public override async Task<TgEfStorageResult<TgEfLicenseEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfLicenseEntity> items = take > 0
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
	public override async Task<TgEfStorageResult<TgEfLicenseEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfLicenseEntity, bool>> where, bool isReadOnly = true)
	{
		IList<TgEfLicenseEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync() => await EfContext.Licenses.AsNoTracking().CountAsync();

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfLicenseEntity, bool>> where) => await EfContext.Licenses.AsNoTracking().Where(where).CountAsync();

    #endregion

    #region Public and private methods - ITgEfAppRepository

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
