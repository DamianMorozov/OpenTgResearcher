// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Repositories;

/// <summary> Message repository </summary>
public sealed class TgEfMessageRelationRepository : TgEfRepositoryBase<TgEfMessageRelationEntity, TgEfMessageRelationDto>, ITgEfMessageRelationRepository
{	
	#region Public and private fields, properties, constructor

	public TgEfMessageRelationRepository() : base() { }

	public TgEfMessageRelationRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

    #endregion

    #region Public and private methods

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfMessageRelationEntity>> GetAsync(TgEfMessageRelationEntity item, bool isReadOnly = true)
	{
        // Find by Uid
        var itemFind = await GetQuery(isReadOnly)
            .FirstOrDefaultAsync(x => x.Uid == item.Uid);
        if (itemFind is not null)
            return new(TgEnumEntityState.IsExists, itemFind);
        // Find by Id
        itemFind = await GetQuery(isReadOnly)
            .FirstOrDefaultAsync(x => 
                x.ParentSourceId == item.ParentSourceId && x.ParentMessageId == item.ParentMessageId && 
                x.ChildSourceId == item.ChildSourceId && x.ChildMessageId == item.ChildMessageId);

        return itemFind is not null
            ? new(TgEnumEntityState.IsExists, itemFind)
            : new TgEfStorageResult<TgEfMessageRelationEntity>(TgEnumEntityState.NotExists);
    }

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfMessageRelationEntity>> GetByDtoAsync(TgEfMessageRelationDto dto, bool isReadOnly = true)
	{
        // Find by Uid
        var itemFind = await GetQuery(isReadOnly)
            .FirstOrDefaultAsync(x => x.Uid == dto.Uid);
        if (itemFind is not null)
            return new(TgEnumEntityState.IsExists, itemFind);
        // Find by Id
        itemFind = await GetQuery(isReadOnly)
            .FirstOrDefaultAsync(x =>
                x.ParentSourceId == dto.ParentSourceId && x.ParentMessageId == dto.ParentMessageId &&
                x.ChildSourceId == dto.ChildSourceId && x.ChildMessageId == dto.ChildMessageId);

        return itemFind is not null
            ? new(TgEnumEntityState.IsExists, itemFind)
            : new TgEfStorageResult<TgEfMessageRelationEntity>(TgEnumEntityState.NotExists);
    }

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfMessageRelationEntity> Get(TgEfMessageRelationEntity item, bool isReadOnly = true)
	{
        // Find by Uid
        var itemFind = GetQuery(isReadOnly)
            .FirstOrDefault(x => x.Uid == item.Uid);
        if (itemFind is not null)
            return new(TgEnumEntityState.IsExists, itemFind);
        // Find by Id
        itemFind = GetQuery(isReadOnly)
            .FirstOrDefault(x =>
                x.ParentSourceId == item.ParentSourceId && x.ParentMessageId == item.ParentMessageId &&
                x.ChildSourceId == item.ChildSourceId && x.ChildMessageId == item.ChildMessageId);

        return itemFind is not null
            ? new(TgEnumEntityState.IsExists, itemFind)
            : new TgEfStorageResult<TgEfMessageRelationEntity>(TgEnumEntityState.NotExists);
    }

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfMessageRelationEntity> GetByDto(TgEfMessageRelationDto dto, bool isReadOnly = true)
	{
        // Find by Uid
        var itemFind = GetQuery(isReadOnly)
            .FirstOrDefault(x => x.Uid == dto.Uid);
        if (itemFind is not null)
            return new(TgEnumEntityState.IsExists, itemFind);
        // Find by Id
        itemFind = GetQuery(isReadOnly)
            .FirstOrDefault(x =>
                x.ParentSourceId == dto.ParentSourceId && x.ParentMessageId == dto.ParentMessageId &&
                x.ChildSourceId == dto.ChildSourceId && x.ChildMessageId == dto.ChildMessageId);

        return itemFind is not null
            ? new(TgEnumEntityState.IsExists, itemFind)
            : new TgEfStorageResult<TgEfMessageRelationEntity>(TgEnumEntityState.NotExists);
    }

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfMessageRelationEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfMessageRelationEntity> items = take > 0
			? await GetQuery(isReadOnly)
				.Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly)
				.ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfMessageRelationEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfMessageRelationEntity, bool>> where, bool isReadOnly = true)
	{
		IList<TgEfMessageRelationEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where)
				.Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).Where(where)
				.ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync() => await EfContext.MessagesRelations.AsNoTracking().CountAsync();

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfMessageRelationEntity, bool>> where) => 
        await EfContext.MessagesRelations.AsNoTracking().Where(where).CountAsync();

    #endregion
}
