namespace TgStorage.Repositories;

/// <summary> EF message relation repository </summary>
public sealed class TgEfMessageRelationRepository : TgEfRepositoryBase<TgEfMessageRelationEntity, TgEfMessageRelationDto>, ITgEfMessageRelationRepository
{	
	#region Fields, properties, constructor

	public TgEfMessageRelationRepository() : base() { }

	public TgEfMessageRelationRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfMessageRelationEntity>> GetAsync(TgEfMessageRelationEntity item, bool isReadOnly = true, CancellationToken ct = default)
	{
        // Find by Uid
        var itemFind = await GetQuery(isReadOnly)
            .FirstOrDefaultAsync(x => x.Uid == item.Uid, ct);

        if (itemFind is not null)
            return new(TgEnumEntityState.IsExists, itemFind);

        // Find by ParentSourceId and ParentMessageId and ChildSourceId and ChildMessageId
        itemFind = await GetQuery(isReadOnly)
            .FirstOrDefaultAsync(x => 
                x.ParentSourceId == item.ParentSourceId && x.ParentMessageId == item.ParentMessageId && 
                x.ChildSourceId == item.ChildSourceId && x.ChildMessageId == item.ChildMessageId, ct);

        return itemFind is not null
            ? new(TgEnumEntityState.IsExists, itemFind)
            : new TgEfStorageResult<TgEfMessageRelationEntity>(TgEnumEntityState.NotExists);
    }

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfMessageRelationEntity>> GetByDtoAsync(TgEfMessageRelationDto dto, bool isReadOnly = true, CancellationToken ct = default)
	{
        // Find by Uid
        var itemFind = await GetQuery(isReadOnly)
            .FirstOrDefaultAsync(x => x.Uid == dto.Uid, ct);

        if (itemFind is not null)
            return new(TgEnumEntityState.IsExists, itemFind);

        // Find by Id
        itemFind = await GetQuery(isReadOnly)
            .FirstOrDefaultAsync(x =>
                x.ParentSourceId == dto.ParentSourceId && x.ParentMessageId == dto.ParentMessageId &&
                x.ChildSourceId == dto.ChildSourceId && x.ChildMessageId == dto.ChildMessageId, ct);

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
    public override async Task<TgEfStorageResult<TgEfMessageRelationEntity>> GetListAsync(int take, int skip, bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfMessageRelationEntity> items = take > 0
			? await GetQuery(isReadOnly)
				.Skip(skip).Take(take).ToListAsync(ct)
			: await GetQuery(isReadOnly)
				.ToListAsync(ct);
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfMessageRelationEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfMessageRelationEntity, bool>> where, 
        bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfMessageRelationEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where)
				.Skip(skip).Take(take).ToListAsync(ct)
			: await GetQuery(isReadOnly).Where(where)
				.ToListAsync(ct);
		
        return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(CancellationToken ct = default) => await EfContext.MessagesRelations.AsNoTracking().CountAsync(ct);

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfMessageRelationEntity, bool>> where, CancellationToken ct = default) => 
        await EfContext.MessagesRelations.AsNoTracking().Where(where).CountAsync(ct);

    /// <inheritdoc />
    public async Task SaveAsync(TgEfMessageRelationDto dto, CancellationToken ct = default)
    {
        TgEfMessageRelationEntity? entity = null;
        try
        {
            if (dto is null)
                return;

            // Try to find existing entity
            entity = await GetQuery(isReadOnly: false).SingleOrDefaultAsync(x => x.Uid == dto.Uid, ct);
            if (entity is null)
                // Find by ParentSourceId and ParentMessageId and ChildSourceId and ChildMessageId
                entity = await GetQuery(isReadOnly: false).SingleOrDefaultAsync(x => x.ParentSourceId == dto.ParentSourceId && x.ParentMessageId == dto.ParentMessageId &&
                    x.ChildSourceId == dto.ChildSourceId && x.ChildMessageId == dto.ChildMessageId, ct);

            if (entity is null)
            {
                if (EfContext is DbContext dbContext)
                    dbContext.ChangeTracker.Clear();
                // Create new entity
                entity = TgEfDomainUtils.CreateNewEntity(dto, isUidCopy: true);
                EfContext.MessagesRelations.Add(entity);
            }
            else
            {
                // Update existing entity
                TgEfDomainUtils.FillEntity(dto, entity, isUidCopy: false);
            }

            ValidateAndNormalize(entity);
            await EfContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error saving message relation");
            throw;
        }
        finally
        {
            // Avoid EF tracking issues
            if (entity is not null)
                EfContext.DetachItem(entity);
        }
    }

    /// <inheritdoc />
    public async Task SaveListAsync(IEnumerable<TgEfMessageRelationDto> dtos, CancellationToken ct = default)
    {
        try
        {
            foreach (var dto in dtos)
            {
                await SaveAsync(dto, ct);
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error saving message relations");
            throw;
        }
    }

    #endregion
}
