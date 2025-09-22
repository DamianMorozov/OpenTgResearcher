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

    #endregion
}
