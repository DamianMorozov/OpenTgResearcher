namespace TgStorage.Repositories;

/// <summary> EF user repository </summary>
public sealed class TgEfUserRepository : TgEfRepositoryBase<TgEfUserEntity, TgEfUserDto>, ITgEfUserRepository
{
	#region Fields, properties, constructor

	public TgEfUserRepository() : base() { }

	public TgEfUserRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfUserEntity>> GetAsync(TgEfUserEntity item, bool isReadOnly = true, CancellationToken ct = default)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefaultAsync(ct);
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		
        // Find by ID
		itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.Id == item.Id, ct);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfUserEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfUserEntity>> GetByDtoAsync(TgEfUserDto dto, bool isReadOnly = true, CancellationToken ct = default)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefaultAsync(ct);
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		
        // Find by ID
		itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.Id == dto.Id, ct);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfUserEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfUserEntity> Get(TgEfUserEntity item, bool isReadOnly = true)
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
			: new TgEfStorageResult<TgEfUserEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfUserEntity> GetByDto(TgEfUserDto dto, bool isReadOnly = true)
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
			: new TgEfStorageResult<TgEfUserEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfUserEntity>> GetListAsync(int take, int skip, bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfUserEntity> items = take > 0 
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync(ct) 
			: await GetQuery(isReadOnly).ToListAsync(ct);
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfUserEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfUserEntity, bool>> where, 
        bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfUserEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync(ct)
			: await GetQuery(isReadOnly).Where(where).ToListAsync(ct);

		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(CancellationToken ct = default) => await EfContext.Users.AsNoTracking().CountAsync(ct);

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfUserEntity, bool>> where, CancellationToken ct = default) => 
        await EfContext.Users.AsNoTracking().Where(where).CountAsync(ct);

    /// <inheritdoc />
    public async Task<List<TgEfUserDto>> GetUsersBySourceIdAsync(long sourceId, CancellationToken ct = default) => await EfContext.Users
        .AsNoTracking()
        .Where(u => EfContext.Messages.Any(m => m.SourceId == sourceId && m.UserId == u.Id))
        .Select(SelectDto())
        .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<List<TgEfUserDto>> GetUsersBySourceIdJoinAsync(long sourceId, long userId, CancellationToken ct = default)
    {
        var tmp = await EfContext.Users
            .AsNoTracking()
            .Select(u => new
            {
                User = u,
                MsgCount = EfContext.Messages.Count(m => m.SourceId == sourceId && m.UserId == u.Id)
            })
            .Where(x => x.MsgCount > 0)
            .OrderByDescending(x => x.User.Id == userId)
            .ToListAsync(ct);

        return [.. tmp.Select(x => TgEfDomainUtils.CreateNewDto(x.User, x.MsgCount, isUidCopy: true))];
    }

    /// <inheritdoc />
    public async Task<TgEfUserDto> GetMyselfUserAsync(long sourceId, long userId, CancellationToken ct = default)
    {
        var countMessages = await EfContext.Messages.AsNoTracking().CountAsync(m => m.SourceId == sourceId && m.UserId == userId, ct);
        var userDto = await EfContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(SelectDto()).FirstOrDefaultAsync(ct);
        if (userDto is not null)
            userDto.CountMessages = countMessages;
        return userDto ?? new();
    }

    #endregion
}
