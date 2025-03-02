// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Stories;

/// <summary> Story repository </summary>
public sealed class TgEfStoryRepository : TgEfRepositoryBase<TgEfStoryEntity, TgEfStoryDto>, ITgEfStoryRepository
{
	#region Public and private methods

	public override string ToDebugString() => $"{nameof(TgEfStoryRepository)}";

	public override IQueryable<TgEfStoryEntity> GetQuery(bool isReadOnly = true)
	{
		return isReadOnly ? EfContext.Stories.AsNoTracking() : EfContext.Stories.AsTracking();
	}

	public override async Task<TgEfStorageResult<TgEfStoryEntity>> GetAsync(TgEfStoryEntity item, bool isReadOnly = true)
	{
		try
		{
			// Find by Uid
			var itemFind = await GetQuery(isReadOnly)
				.Where(x => x.Uid == item.Uid)
				.FirstOrDefaultAsync();
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
			// Find by ID
			itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.Id == item.Id);
			return itemFind is not null
				? new(TgEnumEntityState.IsExists, itemFind)
				: new TgEfStorageResult<TgEfStoryEntity>(TgEnumEntityState.NotExists, item);
		}
#if DEBUG
		catch (Exception ex)
#else
		catch (Exception)
#endif
		{
#if DEBUG
			Debug.WriteLine(ex, TgConstants.LogTypeStorage);
			Debug.WriteLine(ex.StackTrace);
#endif
			throw;
		}
	}

	public override async Task<TgEfStorageResult<TgEfStoryEntity>> GetFirstAsync(bool isReadOnly = true)
	{
		var item = await GetQuery(isReadOnly).FirstOrDefaultAsync();
		return item is null
			? new(TgEnumEntityState.NotExists)
			: new TgEfStorageResult<TgEfStoryEntity>(TgEnumEntityState.IsExists, item);
	}

	private static Expression<Func<TgEfStoryEntity, TgEfStoryDto>> SelectDto() => item => new TgEfStoryDto().GetNewDto(item);

	public async Task<TgEfStoryDto> GetDtoAsync(Expression<Func<TgEfStoryEntity, bool>> where)
	{
		var dto = await GetQuery().Where(where).Select(SelectDto()).SingleOrDefaultAsync() ?? new TgEfStoryDto();
		return dto;
	}

	public async Task<List<TgEfStoryDto>> GetListDtosAsync(int take, int skip, bool isReadOnly = true)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(isReadOnly).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public async Task<List<TgEfStoryDto>> GetListDtosAsync(int take, int skip, Expression<Func<TgEfStoryEntity, bool>> where, bool isReadOnly = true)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(isReadOnly).Where(where).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public override async Task<TgEfStorageResult<TgEfStoryEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfStoryEntity> items = take > 0 
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync() 
			: await GetQuery(isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfStoryEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfStoryEntity, bool>> where, bool isReadOnly = true)
	{
		IList<TgEfStoryEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<int> GetCountAsync()
	{
		return await EfContext.Stories.AsNoTracking().CountAsync();
	}

	public override async Task<int> GetCountAsync(Expression<Func<TgEfStoryEntity, bool>> where)
	{
		return await EfContext.Stories.AsNoTracking().Where(where).CountAsync();
	}

	#endregion

	#region Public and private methods - Delete

	public override async Task<TgEfStorageResult<TgEfStoryEntity>> DeleteAllAsync()
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