// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Repositories;

/// <summary> Document repository </summary>
public sealed class TgEfDocumentRepository : TgEfRepositoryBase<TgEfDocumentEntity, TgEfDocumentDto>, ITgEfDocumentRepository
{
	#region Public and private fields, properties, constructor

	public TgEfDocumentRepository() : base() { }

	public TgEfDocumentRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

    #endregion

    #region Public and private methods

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfDocumentEntity>> GetAsync(TgEfDocumentEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == item.Uid)
			.FirstOrDefaultAsync();
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by SourceId and ID and MessageId
		itemFind = await GetQuery(isReadOnly)
			.Where(x => x.SourceId == item.SourceId && x.Id == item.Id && x.MessageId == item.MessageId)
			.SingleOrDefaultAsync();
			
        return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfDocumentEntity>(TgEnumEntityState.NotExists);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfDocumentEntity>> GetByDtoAsync(TgEfDocumentDto dto, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			.Where(x => x.Uid == dto.Uid)
			.FirstOrDefaultAsync();
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by SourceId and ID and MessageId
		itemFind = await GetQuery(isReadOnly)
			.Where(x => x.SourceId == dto.SourceId && x.Id == dto.Id && x.MessageId == dto.MessageId)
			.SingleOrDefaultAsync();
			
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
    public override async Task<TgEfStorageResult<TgEfDocumentEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfDocumentEntity> items = take > 0
			? await GetQuery(isReadOnly)
				//.Include(x => x.Source)
				.Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly)
				//.Include(x => x.Source)
				.ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfDocumentEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfDocumentEntity, bool>> where, bool isReadOnly = true)
	{
		IList<TgEfDocumentEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where)
				//.Include(x => x.Source)
				.Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).Where(where)
				//.Include(x => x.Source)
				.ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync() => await EfContext.Documents.AsNoTracking().CountAsync();

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfDocumentEntity, bool>> where) => await EfContext.Documents.AsNoTracking().Where(where).CountAsync();

    #endregion

    #region Public and private methods - Delete

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfDocumentEntity>> DeleteAllAsync()
	{
		var storageResult = await GetListAsync(0, 0, isReadOnly: false);
		if (storageResult.IsExists)
		{
			foreach (var item in storageResult.Items)
			{
				await DeleteAsync(item);
			}
		}
		return new(storageResult.IsExists ? TgEnumEntityState.IsDeleted : TgEnumEntityState.NotDeleted);
	}

	#endregion
}