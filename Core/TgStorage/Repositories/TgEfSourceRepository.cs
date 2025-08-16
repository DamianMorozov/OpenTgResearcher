// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
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

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetAsync(TgEfSourceEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.FirstOrDefaultAsync(x => x.Uid == item.Uid);
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by ID
		if (item.Id > 0)
		{
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
		
		return new(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetByDtoAsync(TgEfSourceDto dto, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.FirstOrDefaultAsync(x => x.Uid == dto.Uid);
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by ID
		if (dto.Id > 0)
		{
			itemFind = await GetQuery(isReadOnly)
				.FirstOrDefaultAsync(x => x.Id == dto.Id);
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
		}
		// Find by UserName
		if (!string.IsNullOrEmpty(dto.UserName))
		{
			itemFind = await GetQuery(isReadOnly).FirstOrDefaultAsync(x => x.UserName == dto.UserName);
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
		}
		
		return new(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfSourceEntity> Get(TgEfSourceEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.FirstOrDefault(x => x.Uid == item.Uid);
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by ID
		if (item.Id > 0)
		{
			itemFind = GetQuery(isReadOnly)
				.FirstOrDefault(x => x.Id == item.Id);
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
		}
		// Find by UserName
		if (!string.IsNullOrEmpty(item.UserName))
		{
			itemFind = GetQuery(isReadOnly).FirstOrDefault(x => x.UserName == item.UserName);
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
		}
		
		return new(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfSourceEntity> GetByDto(TgEfSourceDto dto, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.FirstOrDefault(x => x.Uid == dto.Uid);
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by ID
		if (dto.Id > 0)
		{
			itemFind = GetQuery(isReadOnly)
				.FirstOrDefault(x => x.Id == dto.Id);
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
		}
		// Find by UserName
		if (!string.IsNullOrEmpty(dto.UserName))
		{
			itemFind = GetQuery(isReadOnly).FirstOrDefault(x => x.UserName == dto.UserName);
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
		}
		
		return new(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
	public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfSourceEntity> items = take > 0 
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync() 
			: await GetQuery(isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
	public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfSourceEntity, bool>> where, bool isReadOnly = true)
	{
		var query = isReadOnly ? EfContext.Sources.AsNoTracking() : EfContext.Sources.AsTracking();
		IList<TgEfSourceEntity> items = take > 0 
			? await query.Where(where).Skip(skip).Take(take).ToListAsync() 
			: await query.Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync() => await EfContext.Sources.AsNoTracking().CountAsync();

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfSourceEntity, bool>> where) => await EfContext.Sources.AsNoTracking().Where(where).CountAsync();

    #endregion

    #region Public and private methods - ITgEfSourceRepository

    /// <inheritdoc />
    public async Task ResetAutoUpdateAsync()
    {
        await EfContext.Sources
            .Where(x => x.IsAutoUpdate)
            .ExecuteUpdateAsync(x => x.SetProperty(s => s.IsAutoUpdate, false));
        await EfContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SetIsUserAccessAsync(bool isUserAccess)
    {
        await EfContext.Sources
            .ExecuteUpdateAsync(x => x.SetProperty(s => s.IsUserAccess, isUserAccess));
        await EfContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SetIsUserAccessAsync(List<long> chatIds, bool isUserAccess)
    {
        await EfContext.Sources
            .Where(x => chatIds.Contains(x.Id))
            .ExecuteUpdateAsync(x => x.SetProperty(s => s.IsUserAccess, isUserAccess));
        await EfContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SetIsSubscribeAsync(bool isSubscribe)
    {
        await EfContext.Sources
            .ExecuteUpdateAsync(x => x.SetProperty(s => s.IsSubscribe, isSubscribe));
        await EfContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SetIsSubscribeAsync(List<long> chatIds, bool isSubscribe)
    {
        await EfContext.Sources
            .Where(x => chatIds.Contains(x.Id))
            .ExecuteUpdateAsync(x => x.SetProperty(s => s.IsSubscribe, isSubscribe));
        await EfContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<TgEfSourceDto> FindCommentDtoSourceAsync(long chatId)
    {
        // Find the first relation where the given chatId is used as a child
        var relation = await EfContext.MessagesRelations
            .AsNoTracking()
            .Where(x => x.ParentSourceId == chatId)
            .OrderByDescending(x => x.ChildMessageId)
            .FirstOrDefaultAsync();

        if (relation is null)
            return new TgEfSourceDto(); // Return empty DTO if not found

        // Load the parent source entity
        var sourceEntity = await EfContext.Sources
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == relation.ChildSourceId);

        if (sourceEntity is null)
            return new TgEfSourceDto(); // Return empty DTO if parent source not found

        // Convert to DTO
        return new TgEfSourceDto().Copy(sourceEntity, isUidCopy: true);
    }

    #endregion
}
