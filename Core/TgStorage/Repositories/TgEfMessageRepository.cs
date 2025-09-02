// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Repositories;

/// <summary> Message repository </summary>
public sealed class TgEfMessageRepository : TgEfRepositoryBase<TgEfMessageEntity, TgEfMessageDto>, ITgEfMessageRepository
{	
	#region Fields, properties, constructor

	public TgEfMessageRepository() : base() { }

	public TgEfMessageRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfMessageEntity>> GetAsync(TgEfMessageEntity item, bool isReadOnly = true)
	{
        // Find by Uid
        var itemFind = await GetQuery(isReadOnly)
            .FirstOrDefaultAsync(x => x.Uid == item.Uid);
        if (itemFind is not null)
            return new(TgEnumEntityState.IsExists, itemFind);
        // Find by Id
        itemFind = await GetQuery(isReadOnly)
            .FirstOrDefaultAsync(x => x.SourceId == item.SourceId && x.Id == item.Id);

        return itemFind is not null
            ? new(TgEnumEntityState.IsExists, itemFind)
            : new TgEfStorageResult<TgEfMessageEntity>(TgEnumEntityState.NotExists);
    }

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfMessageEntity>> GetByDtoAsync(TgEfMessageDto dto, bool isReadOnly = true)
	{
        // Find by Uid
        var itemFind = await GetQuery(isReadOnly)
            .FirstOrDefaultAsync(x => x.Uid == dto.Uid);
        if (itemFind is not null)
            return new(TgEnumEntityState.IsExists, itemFind);
        // Find by Id
        itemFind = await GetQuery(isReadOnly)
            .FirstOrDefaultAsync(x => x.SourceId == dto.SourceId && x.Id == dto.Id);

        return itemFind is not null
            ? new(TgEnumEntityState.IsExists, itemFind)
            : new TgEfStorageResult<TgEfMessageEntity>(TgEnumEntityState.NotExists);
    }

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfMessageEntity> Get(TgEfMessageEntity item, bool isReadOnly = true)
	{
        // Find by Uid
        var itemFind = GetQuery(isReadOnly)
            .FirstOrDefault(x => x.Uid == item.Uid);
        if (itemFind is not null)
            return new(TgEnumEntityState.IsExists, itemFind);
        // Find by Id
        itemFind = GetQuery(isReadOnly)
            .FirstOrDefault(x => x.SourceId == item.SourceId && x.Id == item.Id);

        return itemFind is not null
            ? new(TgEnumEntityState.IsExists, itemFind)
            : new TgEfStorageResult<TgEfMessageEntity>(TgEnumEntityState.NotExists);
    }

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfMessageEntity> GetByDto(TgEfMessageDto dto, bool isReadOnly = true)
	{
        // Find by Uid
        var itemFind = GetQuery(isReadOnly)
            .FirstOrDefault(x => x.Uid == dto.Uid);
        if (itemFind is not null)
            return new(TgEnumEntityState.IsExists, itemFind);
        // Find by Id
        itemFind = GetQuery(isReadOnly)
            .FirstOrDefault(x => x.SourceId == dto.SourceId && x.Id == dto.Id);

        return itemFind is not null
            ? new(TgEnumEntityState.IsExists, itemFind)
            : new TgEfStorageResult<TgEfMessageEntity>(TgEnumEntityState.NotExists);
    }

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfMessageEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfMessageEntity> items = take > 0
			? await GetQuery(isReadOnly)
				.Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly)
				.ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfMessageEntity>> GetListAsync(int take, int skip, 
        Expression<Func<TgEfMessageEntity, bool>> where, bool isReadOnly = true)
	{
		IList<TgEfMessageEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where)
				.Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).Where(where)
				.ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync() => await EfContext.Messages.AsNoTracking().CountAsync();

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfMessageEntity, bool>> where) => await EfContext.Messages.AsNoTracking().Where(where).CountAsync();

    #endregion

    #region Methods - ITgEfMessageRepository

    /// <inheritdoc />
    public async Task<long> GetLastIdAsync(long sourceId) => await EfContext.Messages
        .AsNoTracking()
        .Where(x => x.SourceId == sourceId)
        .OrderByDescending(x => x.Id)
        .Select(x => x.Id)
        .FirstOrDefaultAsync();

    /// <inheritdoc />
    public async Task<List<TgEfMessageDto>> GetListDtosWithoutRelationsAsync<TKey>(int take, int skip, 
        Expression<Func<TgEfMessageEntity, bool>> where, Expression<Func<TgEfMessageEntity, TKey>> order, bool isOrderDesc = false)
    {
        // Get IDs of messages that are referenced as children in relations
        var relatedChildIds = await EfContext.MessagesRelations
            .AsNoTracking()
            .Select(r => r.ChildMessageId)
            .Distinct()
            .ToListAsync();

        // Filter messages that match the condition and are not in the relatedChildIds
        var query = (IQueryable<TgEfMessageEntity>)EfContext.Messages
            .AsNoTracking()
            .Where(where)
            .Where(m => !relatedChildIds.Contains(m.Id));

        // Order
        query = !isOrderDesc ? query.OrderBy(order) : query.OrderByDescending(order);

        // Apply pagination
        if (skip > 0)
            query = query.Skip(skip);
        if (take > 0)
            query = query.Take(take);

        // Project to DTOs using SelectDto expression
        var dtoQuery = query.Select(SelectDto());

        // Execute query and return result
        return await dtoQuery.ToListAsync();
    }

    /// <inheritdoc />
    public async Task SaveRelationAsync(long parentChatId, int parentMessageId, long childChatId, int childMessageId)
    {
        var exists = await EfContext.MessagesRelations
            .AnyAsync(x =>
                x.ParentSourceId == parentChatId &&
                x.ParentMessageId == parentMessageId &&
                x.ChildSourceId == childChatId &&
                x.ChildMessageId == childMessageId);

        if (!exists)
        {
            var relation = new TgEfMessageRelationEntity
            {
                ParentSourceId = parentChatId,
                ParentMessageId = parentMessageId,
                ChildSourceId = childChatId,
                ChildMessageId = childMessageId
            };

            await EfContext.MessagesRelations.AddAsync(relation);
            await EfContext.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task<List<long>> GetUserIdsFromMessagesAsync(Expression<Func<TgEfMessageEntity, bool>> where) => 
        await GetQuery(isReadOnly: true)
            .Where(where)
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync();

    #endregion
}
