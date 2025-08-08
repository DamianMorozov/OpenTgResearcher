// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Repositories;

/// <summary> Message repository </summary>
public sealed class TgEfMessageRepository : TgEfRepositoryBase<TgEfMessageEntity, TgEfMessageDto>, ITgEfMessageRepository
{	
	#region Public and private fields, properties, constructor

	public TgEfMessageRepository() : base() { }

	public TgEfMessageRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

    #endregion

    #region Public and private methods

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
				//.Include(x => x.Source)
				.Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly)
				//.Include(x => x.Source)
				.ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfMessageEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfMessageEntity, bool>> where, bool isReadOnly = true)
	{
		IList<TgEfMessageEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where)
				//.Include(x => x.Source)
				.Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).Where(where)
				//.Include(x => x.Source)
				.ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

    /// <inheritdoc />
    public override async Task<int> GetCountAsync() => await EfContext.Messages.AsNoTracking().CountAsync();

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfMessageEntity, bool>> where) => await EfContext.Messages.AsNoTracking().Where(where).CountAsync();

    /// <inheritdoc />
    public async Task<long> GetLastIdAsync(long sourceId) => await EfContext.Messages
        .AsNoTracking()
        .Where(x => x.SourceId == sourceId)
        .OrderByDescending(x => x.Id)
        .Select(x => x.Id)
        .FirstOrDefaultAsync();

    #endregion

    #region Public and private methods - Delete

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfMessageEntity>> DeleteAllAsync()
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