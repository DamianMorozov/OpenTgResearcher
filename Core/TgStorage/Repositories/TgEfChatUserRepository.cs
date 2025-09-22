namespace TgStorage.Repositories;

/// <summary> EF chat user repository </summary>
public sealed class TgEfChatUserRepository : TgEfRepositoryBase<TgEfChatUserEntity, TgEfChatUserDto>, ITgEfChatUserRepository
{
	#region Fields, properties, constructor

	public TgEfChatUserRepository() : base() { }

	public TgEfChatUserRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfChatUserEntity>> GetAsync(TgEfChatUserEntity item, bool isReadOnly = true, CancellationToken ct = default)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefaultAsync(ct);

		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);

		// Find by ChatId and UserId
		itemFind = await GetQuery(isReadOnly)
			.Where(x => x.ChatId == item.ChatId && x.UserId == item.UserId)
			.SingleOrDefaultAsync(ct);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfChatUserEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfChatUserEntity>> GetByDtoAsync(TgEfChatUserDto dto, bool isReadOnly = true, CancellationToken ct = default)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefaultAsync(ct);
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);

        // Find by ChatId and UserId
        itemFind = await GetQuery(isReadOnly)
			.Where(x => x.ChatId == dto.ChatId && x.UserId == dto.UserId)
			.SingleOrDefaultAsync(ct);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfChatUserEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfChatUserEntity> Get(TgEfChatUserEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefault();

		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);

        // Find by ChatId and UserId
        itemFind = GetQuery(isReadOnly)
			.Where(x => x.ChatId == item.ChatId && x.UserId == item.UserId)
			.SingleOrDefault();
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfChatUserEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfChatUserEntity> GetByDto(TgEfChatUserDto dto, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefault();
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);

        // Find by ChatId and UserId
        itemFind = GetQuery(isReadOnly)
			.Where(x => x.ChatId == dto.ChatId && x.UserId == dto.UserId)
			.SingleOrDefault();
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfChatUserEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfChatUserEntity>> GetListAsync(int take, int skip, bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfChatUserEntity> items = take > 0
			? await GetQuery(isReadOnly)
				.Skip(skip).Take(take).ToListAsync(ct)
			: await GetQuery(isReadOnly)
				.ToListAsync(ct);
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfChatUserEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfChatUserEntity, bool>> where, 
        bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfChatUserEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where)
				.Skip(skip).Take(take).ToListAsync(ct)
			: await GetQuery(isReadOnly).Where(where)
				.ToListAsync(ct);

		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(CancellationToken ct = default) => await EfContext.Documents.AsNoTracking().CountAsync(ct);

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfChatUserEntity, bool>> where, CancellationToken ct = default) => 
        await EfContext.ChatUsers.AsNoTracking().Where(where).CountAsync(ct);

    #endregion
}
