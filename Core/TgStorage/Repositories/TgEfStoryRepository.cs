// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Repositories;

/// <summary> Story repository </summary>
public sealed class TgEfStoryRepository : TgEfRepositoryBase<TgEfStoryEntity, TgEfStoryDto>, ITgEfStoryRepository
{
	#region Fields, properties, constructor

	public TgEfStoryRepository() : base() { }

	public TgEfStoryRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfStoryEntity>> GetAsync(TgEfStoryEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefaultAsync();
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by ID
		itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.Id == item.Id);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfStoryEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfStoryEntity>> GetByDtoAsync(TgEfStoryDto dto, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefaultAsync();
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by ID
		itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.Id == dto.Id);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfStoryEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfStoryEntity> Get(TgEfStoryEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefault();
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by ID
		itemFind = GetQuery(isReadOnly).SingleOrDefault(x => x.Id == item.Id);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfStoryEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfStoryEntity> GetByDto(TgEfStoryDto dto, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefault();
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by ID
		itemFind = GetQuery(isReadOnly).SingleOrDefault(x => x.Id == dto.Id);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfStoryEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfStoryEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfStoryEntity> items = take > 0 
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync() 
			: await GetQuery(isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfStoryEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfStoryEntity, bool>> where, bool isReadOnly = true)
	{
		IList<TgEfStoryEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync() => await EfContext.Stories.AsNoTracking().CountAsync();

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfStoryEntity, bool>> where) => await EfContext.Stories.AsNoTracking().Where(where).CountAsync();

    #endregion
}
