// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Sources;

/// <summary> Source repository </summary>
public sealed class TgEfSourceRepository : TgEfRepositoryBase<TgEfSourceEntity>
{
	#region Public and private methods

	public override string ToDebugString() => $"{nameof(TgEfSourceRepository)}";

	public override IQueryable<TgEfSourceEntity> GetQuery(bool isReadOnly = true)
	{
		return isReadOnly ? EfContext.Sources.AsNoTracking() : EfContext.Sources.AsTracking();
	}

	public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetAsync(TgEfSourceEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = await EfContext.Sources.FindAsync(item.Uid);
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by Id
		if (item.Id > 0)
		{
			itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.Id == item.Id);
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
		}
		// Find by UserName
		if (!string.IsNullOrEmpty(item.UserName))
		{
			itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.UserName == item.UserName);
			if (itemFind is not null)
				return new(TgEnumEntityState.IsExists, itemFind);
		}
		// Default
		return new TgEfStorageResult<TgEfSourceEntity>(TgEnumEntityState.NotExists, item);
	}

	public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetFirstAsync(bool isReadOnly = true)
	{
		var item = await GetQuery(isReadOnly).FirstOrDefaultAsync();
		return item is null
			? new(TgEnumEntityState.NotExists)
			: new TgEfStorageResult<TgEfSourceEntity>(TgEnumEntityState.IsExists, item);
	}

	private static Expression<Func<TgEfSourceEntity, TgEfSourceDto>> SelectDto() => item => new TgEfSourceDto().GetDto(item);

	private static Expression<Func<TgEfSourceEntity, TgEfSourceLiteDto>> SelectLiteDto() => item => new TgEfSourceLiteDto().GetDto(item);

	public async Task<TgEfSourceDto> GetDtoAsync(Expression<Func<TgEfSourceEntity, bool>> where)
	{
		var dto = await GetQuery().Where(where).Select(SelectDto()).SingleOrDefaultAsync() ?? new TgEfSourceDto();
		return dto;
	}

	public async Task<List<TgEfSourceDto>> GetListDtosAsync(int take, int skip, bool isReadOnly = true)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(isReadOnly).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public async Task<List<TgEfSourceDto>> GetListDtosAsync(int take, int skip, Expression<Func<TgEfSourceEntity, bool>> where, bool isReadOnly = true)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(isReadOnly).Where(where).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public async Task<List<TgEfSourceLiteDto>> GetListLiteDtosAsync(int take, int skip, bool isReadOnly = true)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly).Skip(skip).Take(take).Select(SelectLiteDto()).ToListAsync()
			: await GetQuery(isReadOnly).Select(SelectLiteDto()).ToListAsync();
		return dtos;
	}

	public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfSourceEntity> items = take > 0 
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync() 
			: await GetQuery(isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfSourceEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfSourceEntity, bool>> where, bool isReadOnly = true)
	{
		var query = isReadOnly ? EfContext.Sources.AsNoTracking() : EfContext.Sources.AsTracking();
		IList<TgEfSourceEntity> items = take > 0 
			? await query.Where(where).Skip(skip).Take(take).ToListAsync() 
			: await query.Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<int> GetCountAsync()
	{
		return await EfContext.Sources.AsNoTracking().CountAsync();
	}

	public override async Task<int> GetCountAsync(Expression<Func<TgEfSourceEntity, bool>> where)
	{
		return await EfContext.Sources.AsNoTracking().Where(where).CountAsync();
	}

	#endregion

	#region Public and private methods - Delete

	public override async Task<TgEfStorageResult<TgEfSourceEntity>> DeleteAllAsync()
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