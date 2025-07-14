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

	public override async Task<TgEfStorageResult<TgEfMessageEntity>> GetAsync(TgEfMessageEntity item, bool isReadOnly = true)
	{
		try
		{
			// Too fast, read slower
			try
			{
				return await GetCoreAsync(item, isReadOnly);
			}
#if DEBUG
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
#else
			catch (Exception)
			{
#endif
				await Task.Delay(500);
				return await GetCoreAsync(item, isReadOnly);
			}
		}
#if DEBUG
		catch (Exception ex)
		{
			Debug.WriteLine(ex, TgConstants.LogTypeStorage);
			Debug.WriteLine(ex.StackTrace);
#else
		catch (Exception)
		{
#endif
			throw;
		}
	}

	private async Task<TgEfStorageResult<TgEfMessageEntity>> GetCoreAsync(TgEfMessageEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		//var itemFind = await GetQuery(isReadOnly)
		//	.Where(x => x.Source != null && x.Uid == item.Uid)
		//	.Include(x => x.Source)
		//	.FirstOrDefaultAsync();
		// Find by Uid
		var itemFind = await GetQuery(isReadOnly)
			//.Include(x => x.Source)
			//.FirstOrDefaultAsync(x => x.Source != null && x.Uid == item.Uid);
			.FirstOrDefaultAsync(x => x.Uid == item.Uid);
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by ID
		//itemFind = await GetQuery(isReadOnly)
		//	.Where(x => x.Source != null && x.SourceId == item.SourceId && x.Id == item.Id)
		//	.Include(x => x.Source)
		//	.SingleOrDefaultAsync();
		// Find by Uid
		itemFind = await GetQuery(isReadOnly)
			//.Include(x => x.Source)
			//.FirstOrDefaultAsync(x => x.Source != null && x.SourceId == item.SourceId && x.Id == item.Id);
			.FirstOrDefaultAsync(x => x.SourceId == item.SourceId && x.Id == item.Id);
		// Result
		return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfMessageEntity>(TgEnumEntityState.NotExists, item);
	}

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

	public override async Task<int> GetCountAsync()
	{
		return await EfContext.Messages.AsNoTracking().CountAsync();
	}

	public override async Task<int> GetCountAsync(Expression<Func<TgEfMessageEntity, bool>> where)
	{
		return await EfContext.Messages.AsNoTracking().Where(where).CountAsync();
	}

    /// <inheritdoc />
    public async Task<long> GetLastIdAsync(long sourceId) => await EfContext.Messages
        .AsNoTracking()
        .Where(x => x.SourceId == sourceId)
        .OrderByDescending(x => x.Id)
        .Select(x => x.Id)
        .FirstOrDefaultAsync();

    #endregion

    #region Public and private methods - Delete

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