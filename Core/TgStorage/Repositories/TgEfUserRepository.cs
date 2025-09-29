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
    public async Task<List<TgEfUserDto>> GetUsersByChatIdAsync(long chatId, CancellationToken ct = default) => await EfContext.Users
        .AsNoTracking()
        .Where(u => EfContext.Messages.Any(m => m.SourceId == chatId && m.UserId == u.Id))
        .Select(SelectDto())
        .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<List<TgEfUserDto>> GetUsersByChatIdJoinAsync(long chatId, long userId, int pageSkip, int pageTake, Expression<Func<ITgEfEntity, bool>>? where, 
        CancellationToken ct = default)
    {
        // Query ChatUsers for given chat, join to Users and Messages, group messages per user and count
        var baseQuery =
                from cu in EfContext.ChatUsers.AsNoTracking()
                where cu.ChatId == chatId && !cu.IsDeleted
                join u in EfContext.Users.AsNoTracking() on cu.UserId equals u.Id
                // left join messages for this source by user
                join m in EfContext.Messages.AsNoTracking().Where(m => m.SourceId == chatId)
                on u.Id equals m.UserId into msgs
            select new
            {
                User = u,
                MsgCount = msgs.Count()
            };

        // Filter: keep only users with at least one message (if you want all participants remove this filter)
        baseQuery = baseQuery.Where(x => x.MsgCount > 0);

        // Sort: current user first, then by username for stable ordering
        var ordered = baseQuery
            .OrderByDescending(x => x.MsgCount)
            .ThenByDescending(x => x.User.Id == userId)
            .ThenBy(x => x.User.UserName);

        // Pagination: apply on a separate variable to avoid IOrderedQueryable vs IQueryable mismatch
        var safeSkip = Math.Max(0, pageSkip);
        IQueryable<dynamic> paged = ordered.Skip(safeSkip); // inferred as IQueryable<anon>, but stored as IQueryable<dynamic> for reuse
        if (pageTake > 0)
            paged = paged.Take(pageTake);

        // Materialize
        var list = await paged.ToListAsync(ct);

        // Map to DTOs using domain util (preserves isUidCopy behavior)
        return [.. list.Select(x => TgEfDomainUtils.CreateNewDto(x.User, x.MsgCount, isUidCopy: true))];
    }

    /// <inheritdoc />
    public async Task<TgEfUserDto> GetMyselfUserAsync(long chatId, long userId, CancellationToken ct = default)
    {
        var messagesCount = await EfContext.Messages.AsNoTracking().CountAsync(m => m.SourceId == chatId && m.UserId == userId, ct);
        var userDto = await EfContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(SelectDto()).FirstOrDefaultAsync(ct);
        if (userDto is not null)
            userDto.MessagesCount = messagesCount;
        return userDto ?? new();
    }

    /// <inheritdoc />
    public async Task CreateMissingUsersByMessagesAsync(long chatId, CancellationToken ct = default)
    {
        // Get distinct user ids that have messages in this source
        var messageUserIds = await EfContext.Messages
            .AsNoTracking()
            .Where(m => m.SourceId == chatId && m.UserId > 0)
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync(ct);

        if (messageUserIds == null || messageUserIds.Count == 0) return;

        // Existing users in Users table
        var existingUsers = await EfContext.Users
            .AsNoTracking()
            .Where(u => messageUserIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(ct);

        var missingUserIds = messageUserIds.Except(existingUsers).ToList();

        // Create minimal user entities for missing users to satisfy FK when creating chat links
        if (missingUserIds.Count > 0)
        {
            var newUsers = missingUserIds.Select(id => new TgEfUserEntity
            {
                Id = id,
                // fill minimal safe defaults, adjust property names if needed
                UserName = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                PhoneNumber = string.Empty,
                IsBot = false,
                DtChanged = DateTime.UtcNow,
                IsDeleted = false
            }).ToList();

            // Persist missing users (use repository to respect validation/normalization)
            await SaveListAsync(newUsers, isRewriteEntities: false, isFirstTry: true, ct: ct);
        }
    }

    #endregion
}
