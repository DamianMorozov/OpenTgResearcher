// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Filters;

/// <summary> Filter repository </summary>
public sealed class TgEfFilterRepository : TgEfRepositoryBase<TgEfFilterEntity, TgEfFilterDto>, ITgEfFilterRepository
{
	#region Public and private fields, properties, constructor

	public TgEfFilterRepository() : base() { }

	public TgEfFilterRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

	#endregion

	#region Public and private methods

	public override string ToDebugString() => $"{nameof(TgEfFilterRepository)}";

	public override IQueryable<TgEfFilterEntity> GetQuery(bool isReadOnly = true) => 
		isReadOnly ? EfContext.Filters.AsNoTracking() : EfContext.Filters.AsTracking();

	public override async Task<TgEfStorageResult<TgEfFilterEntity>> GetAsync(TgEfFilterEntity item, bool isReadOnly = true)
	{
		try
		{
			// Find by Uid
			var itemFind = await GetQuery(isReadOnly)
				.Where(x => x.Uid == item.Uid)
				.FirstOrDefaultAsync();
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
			// Find by FilterType and Name
			itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.FilterType == item.FilterType && x.Name == item.Name);
			return itemFind is not null
				? new(TgEnumEntityState.IsExists, itemFind)
				: new TgEfStorageResult<TgEfFilterEntity>(TgEnumEntityState.NotExists, item);
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

	private static Expression<Func<TgEfFilterEntity, TgEfFilterDto>> SelectDto() => item => new TgEfFilterDto().GetNewDto(item);

	public async Task<TgEfFilterDto> GetDtoAsync(Expression<Func<TgEfFilterEntity, bool>> where)
	{
		var dto = await GetQuery().Where(where).Select(SelectDto()).SingleOrDefaultAsync() ?? new TgEfFilterDto();
		return dto;
	}

	public async Task<List<TgEfFilterDto>> GetListDtosAsync(int take = 0, int skip = 0)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly: true).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(isReadOnly: true).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public async Task<List<TgEfFilterDto>> GetListDtosAsync(int take, int skip, Expression<Func<TgEfFilterEntity, bool>> where)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly: true).Where(where).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(isReadOnly: true).Where(where).Select(SelectDto()).ToListAsync();
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