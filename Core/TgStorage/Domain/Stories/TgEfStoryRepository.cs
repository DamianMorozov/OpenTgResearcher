// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Stories;

/// <summary> Story repository </summary>
public sealed class TgEfStoryRepository : TgEfRepositoryBase<TgEfStoryEntity>
{
	#region Public and private methods

	public override string ToDebugString() => $"{nameof(TgEfStoryRepository)}";

	public override IQueryable<TgEfStoryEntity> GetQuery(ITgEfContext efContext, bool isReadOnly = true)
	{
		return isReadOnly ? efContext.Stories.AsNoTracking() : efContext.Stories.AsTracking();
	}

	public override async Task<TgEfStorageResult<TgEfStoryEntity>> GetAsync(TgEfStoryEntity item, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		// Find by Uid
		var itemFind = await efContext.Stories.FindAsync(item.Uid);
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by Id
		itemFind = await GetQuery(efContext, isReadOnly).SingleOrDefaultAsync(x => x.Id == item.Id);
		return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfStoryEntity>(TgEnumEntityState.NotExists, item);
	}

	public override async Task<TgEfStorageResult<TgEfStoryEntity>> GetFirstAsync(bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		var item = await GetQuery(efContext, isReadOnly).FirstOrDefaultAsync();
		return item is null
			? new(TgEnumEntityState.NotExists)
			: new TgEfStorageResult<TgEfStoryEntity>(TgEnumEntityState.IsExists, item);
	}

	private static Expression<Func<TgEfStoryEntity, TgEfStoryDto>> SelectDto() => item => new TgEfStoryDto().GetDto(item);

	public async Task<TgEfStoryDto> GetDtoAsync(Expression<Func<TgEfStoryEntity, bool>> where)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		var dto = await GetQuery(efContext).Where(where).Select(SelectDto()).SingleOrDefaultAsync() ?? new TgEfStoryDto();
		return dto;
	}

	public async Task<List<TgEfStoryDto>> GetListDtosAsync(int take, int skip, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		var dtos = take > 0
			? await GetQuery(efContext, isReadOnly).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(efContext, isReadOnly).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public async Task<List<TgEfStoryDto>> GetListDtosAsync(int take, int skip, Expression<Func<TgEfStoryEntity, bool>> where, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		var dtos = take > 0
			? await GetQuery(efContext, isReadOnly).Where(where).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(efContext, isReadOnly).Where(where).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public override async Task<TgEfStorageResult<TgEfStoryEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		IList<TgEfStoryEntity> items = take > 0 
			? await GetQuery(efContext, isReadOnly).Skip(skip).Take(take).ToListAsync() 
			: await GetQuery(efContext, isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfStoryEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfStoryEntity, bool>> where, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		IList<TgEfStoryEntity> items = take > 0
			? await GetQuery(efContext, isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(efContext, isReadOnly).Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<int> GetCountAsync()
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		return await efContext.Stories.AsNoTracking().CountAsync();
	}

	public override async Task<int> GetCountAsync(Expression<Func<TgEfStoryEntity, bool>> where)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		return await efContext.Stories.AsNoTracking().Where(where).CountAsync();
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