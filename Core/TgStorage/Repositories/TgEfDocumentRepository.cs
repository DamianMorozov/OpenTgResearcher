namespace TgStorage.Repositories;

/// <summary> EF document repository </summary>
public sealed class TgEfDocumentRepository : TgEfRepositoryBase<TgEfDocumentEntity, TgEfDocumentDto>, ITgEfDocumentRepository
{
	#region Fields, properties, constructor

	public TgEfDocumentRepository() : base() { }

	public TgEfDocumentRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfDocumentEntity>> GetAsync(TgEfDocumentEntity item, bool isReadOnly = true, CancellationToken ct = default)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefaultAsync(ct);

		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);

		// Find by SourceId and ID and MessageId
		itemFind = await GetQuery(isReadOnly)
			.Where(x => x.SourceId == item.SourceId && x.Id == item.Id && x.MessageId == item.MessageId)
			.SingleOrDefaultAsync(ct);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfDocumentEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfDocumentEntity>> GetByDtoAsync(TgEfDocumentDto dto, bool isReadOnly = true, CancellationToken ct = default)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefaultAsync(ct);
		
        if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		
        // Find by SourceId and ID and MessageId
		itemFind = await GetQuery(isReadOnly)
			.Where(x => x.SourceId == dto.SourceId && x.Id == dto.Id && x.MessageId == dto.MessageId)
			.SingleOrDefaultAsync(ct);
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfDocumentEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfDocumentEntity> Get(TgEfDocumentEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefault();

		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		
        // Find by SourceId and ID and MessageId
		itemFind = GetQuery(isReadOnly)
			.Where(x => x.SourceId == item.SourceId && x.Id == item.Id && x.MessageId == item.MessageId)
			.SingleOrDefault();
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfDocumentEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfDocumentEntity> GetByDto(TgEfDocumentDto dto, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefault();
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by SourceId and ID and MessageId
		itemFind = GetQuery(isReadOnly)
			.Where(x => x.SourceId == dto.SourceId && x.Id == dto.Id && x.MessageId == dto.MessageId)
			.SingleOrDefault();
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfDocumentEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfDocumentEntity>> GetListAsync(int take, int skip, bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfDocumentEntity> items = take > 0
			? await GetQuery(isReadOnly)
				//.Include(x => x.Source)
				.Skip(skip).Take(take).ToListAsync(ct)
			: await GetQuery(isReadOnly)
				//.Include(x => x.Source)
				.ToListAsync(ct);
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfDocumentEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfDocumentEntity, bool>> where, 
        bool isReadOnly = true, CancellationToken ct = default)
	{
		IList<TgEfDocumentEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where)
				//.Include(x => x.Source)
				.Skip(skip).Take(take).ToListAsync(ct)
			: await GetQuery(isReadOnly).Where(where)
				//.Include(x => x.Source)
				.ToListAsync(ct);

		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(CancellationToken ct = default) => await EfContext.Documents.AsNoTracking().CountAsync(ct);

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfDocumentEntity, bool>> where, CancellationToken ct = default) => 
        await EfContext.Documents.AsNoTracking().Where(where).CountAsync(ct);

    /// <inheritdoc />
    public async Task SaveAsync(TgEfDocumentDto dto, CancellationToken ct = default)
    {
        TgEfDocumentEntity? entity = null;
        try
        {
            if (dto is null)
                return;

            // Try to find existing entity
            entity = await GetQuery(isReadOnly: false).SingleOrDefaultAsync(x => x.Uid == dto.Uid, ct);
            if (entity is null)
                // Find by SourceId and ID and MessageId
                entity = await GetQuery(isReadOnly: false).SingleOrDefaultAsync(x => x.SourceId == dto.SourceId && x.Id == dto.Id && x.MessageId == dto.MessageId, ct);

            if (entity is null)
            {
                if (EfContext is DbContext dbContext)
                    dbContext.ChangeTracker.Clear();
                // Create new entity
                entity = TgEfDomainUtils.CreateNewEntity(dto, isUidCopy: true);
                EfContext.Documents.Add(entity);
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
            TgLogUtils.WriteException(ex, "Error saving document");
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
    public async Task SaveListAsync(IEnumerable<TgEfDocumentDto> dtos, CancellationToken ct = default)
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
            TgLogUtils.WriteException(ex, "Error saving documents");
            throw;
        }
    }

    #endregion
}
