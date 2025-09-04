// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Repositories;

/// <summary> Filter repository </summary>
public sealed class TgEfFilterRepository : TgEfRepositoryBase<TgEfFilterEntity, TgEfFilterDto>, ITgEfFilterRepository
{
	#region Fields, properties, constructor

	public TgEfFilterRepository() : base() { }

	public TgEfFilterRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfFilterEntity>> GetAsync(TgEfFilterEntity item, bool isReadOnly = true, CancellationToken ct = default)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefaultAsync(ct);

		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);

		// Find by FilterType and Name
		itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.FilterType == item.FilterType && x.Name == item.Name, ct);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfFilterEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfFilterEntity>> GetByDtoAsync(TgEfFilterDto dto, bool isReadOnly = true, CancellationToken ct = default)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefaultAsync(ct);
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		
        // Find by FilterType and Name
		itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.FilterType == dto.FilterType && x.Name == dto.Name, ct);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfFilterEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfFilterEntity> Get(TgEfFilterEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefault();
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		
        // Find by FilterType and Name
		itemFind = GetQuery(isReadOnly).SingleOrDefault(x => x.FilterType == item.FilterType && x.Name == item.Name);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfFilterEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfFilterEntity> GetByDto(TgEfFilterDto dto, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefault();
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by FilterType and Name
		itemFind = GetQuery(isReadOnly).SingleOrDefault(x => x.FilterType == dto.FilterType && x.Name == dto.Name);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfFilterEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfFilterEntity>> GetListAsync(int take, int skip, bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfFilterEntity> items = take > 0 
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync(ct) 
			: await GetQuery(isReadOnly).ToListAsync(ct);
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfFilterEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfFilterEntity, bool>> where, 
        bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfFilterEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync(ct)
			: await GetQuery(isReadOnly).Where(where).ToListAsync(ct);
		
        return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(CancellationToken ct = default) => await EfContext.Filters.AsNoTracking().CountAsync(ct);

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfFilterEntity, bool>> where, CancellationToken ct = default) => 
        await EfContext.Filters.AsNoTracking().Where(where).CountAsync(ct);

    #endregion
}
