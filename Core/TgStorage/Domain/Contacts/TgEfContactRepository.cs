// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Contacts;

/// <summary> Contact repository </summary>
public sealed class TgEfContactRepository : TgEfRepositoryBase<TgEfContactEntity>
{
	#region Public and private methods

	public override string ToDebugString() => $"{nameof(TgEfContactRepository)}";

	public override IQueryable<TgEfContactEntity> GetQuery(ITgEfContext efContext, bool isReadOnly = true)
	{
		return isReadOnly ? efContext.Contacts.AsNoTracking() : efContext.Contacts.AsTracking();
	}

	public override async Task<TgEfStorageResult<TgEfContactEntity>> GetAsync(TgEfContactEntity item, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		// Find by Uid
		var itemFind = await efContext.Contacts.FindAsync(item.Uid);
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by Id
		itemFind = await GetQuery(efContext, isReadOnly).SingleOrDefaultAsync(x => x.Id == item.Id);
		return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfContactEntity>(TgEnumEntityState.NotExists, item);
	}

	public override async Task<TgEfStorageResult<TgEfContactEntity>> GetFirstAsync(bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		var item = await GetQuery(efContext, isReadOnly).FirstOrDefaultAsync();
		return item is null
			? new(TgEnumEntityState.NotExists)
			: new TgEfStorageResult<TgEfContactEntity>(TgEnumEntityState.IsExists, item);
	}

	private static Expression<Func<TgEfContactEntity, TgEfContactDto>> SelectDto() => item => new TgEfContactDto().GetDto(item);

	public async Task<TgEfContactDto> GetDtoAsync(Expression<Func<TgEfContactEntity, bool>> where)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		var dto = await GetQuery(efContext).Where(where).Select(SelectDto()).SingleOrDefaultAsync() ?? new TgEfContactDto();
		return dto;
	}

	public async Task<List<TgEfContactDto>> GetListDtosAsync(int take, int skip, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		var dtos = take > 0
			? await GetQuery(efContext, isReadOnly).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(efContext, isReadOnly).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public async Task<List<TgEfContactDto>> GetListDtosAsync(int take, int skip, Expression<Func<TgEfContactEntity, bool>> where, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		var dtos = take > 0
			? await GetQuery(efContext, isReadOnly).Where(where).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(efContext, isReadOnly).Where(where).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public override async Task<TgEfStorageResult<TgEfContactEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		IList<TgEfContactEntity> items = take > 0 
			? await GetQuery(efContext, isReadOnly).Skip(skip).Take(take).ToListAsync() 
			: await GetQuery(efContext, isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfContactEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfContactEntity, bool>> where, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		IList<TgEfContactEntity> items = take > 0
			? await GetQuery(efContext, isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(efContext, isReadOnly).Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<int> GetCountAsync()
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		return await efContext.Contacts.AsNoTracking().CountAsync();
	}

	public override async Task<int> GetCountAsync(Expression<Func<TgEfContactEntity, bool>> where)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		return await efContext.Contacts.AsNoTracking().Where(where).CountAsync();
	}

	#endregion

	#region Public and private methods - Delete

	public override async Task<TgEfStorageResult<TgEfContactEntity>> DeleteAllAsync()
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