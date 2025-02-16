// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Filters;

/// <summary> Filter repository </summary>
public sealed class TgEfFilterRepository : TgEfRepositoryBase<TgEfFilterEntity>
{
	#region Public and private methods

	public override string ToDebugString() => $"{nameof(TgEfFilterRepository)}";

	public override IQueryable<TgEfFilterEntity> GetQuery(bool isReadOnly = true)
	{
		return isReadOnly ? EfContext.Filters.AsNoTracking() : EfContext.Filters.AsTracking();
	}

	public override async Task<TgEfStorageResult<TgEfFilterEntity>> GetAsync(TgEfFilterEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = await EfContext.Filters.FindAsync(item.Uid);
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by FilterType and Name
		itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.FilterType == item.FilterType && x.Name == item.Name);
		return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfFilterEntity>(TgEnumEntityState.NotExists, item);
	}

	public override async Task<TgEfStorageResult<TgEfFilterEntity>> GetFirstAsync(bool isReadOnly = true)
	{
		var item = await GetQuery(isReadOnly).FirstOrDefaultAsync();
		return item is null
			? new(TgEnumEntityState.NotExists)
			: new TgEfStorageResult<TgEfFilterEntity>(TgEnumEntityState.IsExists, item);
	}

	private static Expression<Func<TgEfFilterEntity, TgEfFilterDto>> SelectDto() => item => new TgEfFilterDto().GetDto(item);

	public async Task<TgEfFilterDto> GetDtoAsync(Expression<Func<TgEfFilterEntity, bool>> where)
	{
		var dto = await GetQuery().Where(where).Select(SelectDto()).SingleOrDefaultAsync() ?? new TgEfFilterDto();
		return dto;
	}

	public async Task<List<TgEfFilterDto>> GetListDtosAsync(int take, int skip, bool isReadOnly = true)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(isReadOnly).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public async Task<List<TgEfFilterDto>> GetListDtosAsync(int take, int skip, Expression<Func<TgEfFilterEntity, bool>> where, bool isReadOnly = true)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(isReadOnly).Where(where).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public override async Task<TgEfStorageResult<TgEfFilterEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfFilterEntity> items = take > 0 
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync() 
			: await GetQuery(isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfFilterEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfFilterEntity, bool>> where, bool isReadOnly = true)
	{
		IList<TgEfFilterEntity> items = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(isReadOnly).Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<int> GetCountAsync()
	{
		return await EfContext.Filters.AsNoTracking().CountAsync();
	}

	public override async Task<int> GetCountAsync(Expression<Func<TgEfFilterEntity, bool>> where)
	{
		return await EfContext.Filters.AsNoTracking().Where(where).CountAsync();
	}

	#endregion

	#region Public and private methods - Delete

	public override async Task<TgEfStorageResult<TgEfFilterEntity>> DeleteAllAsync()
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