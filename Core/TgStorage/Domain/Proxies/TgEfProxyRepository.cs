// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Proxies;

/// <summary> Proxy repository </summary>
public sealed class TgEfProxyRepository : TgEfRepositoryBase<TgEfProxyEntity>, ITgEfProxyRepository
{
	#region Public and private methods

	public override string ToDebugString() => $"{nameof(TgEfProxyRepository)}";

	public override IQueryable<TgEfProxyEntity> GetQuery(bool isReadOnly = true)
	{
		return isReadOnly ? EfContext.Proxies.AsNoTracking() : EfContext.Proxies.AsTracking();
	}

	public override async Task<TgEfStorageResult<TgEfProxyEntity>> GetAsync(TgEfProxyEntity item, bool isReadOnly = true)
	{
		// Find by Uid
		var itemFind = await EfContext.Proxies.FindAsync(item.Uid);
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by Type
		itemFind = await GetQuery(isReadOnly)
			.SingleOrDefaultAsync(x => x.Type == item.Type && x.HostName == item.HostName && x.Port == item.Port);
		return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfProxyEntity>(TgEnumEntityState.NotExists, item);
	}

	public override async Task<TgEfStorageResult<TgEfProxyEntity>> GetFirstAsync(bool isReadOnly = true)
	{
		var item = await GetQuery(isReadOnly).FirstOrDefaultAsync();
		return item is null
			? new(TgEnumEntityState.NotExists)
			: new TgEfStorageResult<TgEfProxyEntity>(TgEnumEntityState.IsExists, item);
	}

	private static Expression<Func<TgEfProxyEntity, TgEfProxyDto>> SelectDto() => item => new TgEfProxyDto().GetDto(item);

	public async Task<TgEfProxyDto> GetDtoAsync(Expression<Func<TgEfProxyEntity, bool>> where)
	{
		var dto = await GetQuery().Where(where).Select(SelectDto()).SingleOrDefaultAsync() ?? new TgEfProxyDto();
		return dto;
	}

	public async Task<List<TgEfProxyDto>> GetListDtosAsync(int take, int skip, bool isReadOnly = true)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(isReadOnly).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public async Task<List<TgEfProxyDto>> GetListDtosAsync(int take, int skip, Expression<Func<TgEfProxyEntity, bool>> where, bool isReadOnly = true)
	{
		var dtos = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(isReadOnly).Where(where).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public override async Task<TgEfStorageResult<TgEfProxyEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		IList<TgEfProxyEntity> items = take > 0 
			? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync() 
			: await GetQuery(isReadOnly).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfProxyEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfProxyEntity, bool>> where, bool isReadOnly = true)
	{
		var items = take > 0
			? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync()
			: (IList<TgEfProxyEntity>)await GetQuery(isReadOnly).Where(where).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<int> GetCountAsync()
	{
		return await EfContext.Proxies.AsNoTracking().CountAsync();
	}

	public override async Task<int> GetCountAsync(Expression<Func<TgEfProxyEntity, bool>> where)
	{
		return await EfContext.Proxies.AsNoTracking().Where(where).CountAsync();
	}

	#endregion

	#region Public and private methods - Delete

	public override async Task<TgEfStorageResult<TgEfProxyEntity>> DeleteAllAsync()
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

	#region Public and private methods - ITgEfProxyRepository

	public async Task<TgEfStorageResult<TgEfProxyEntity>> GetCurrentProxyAsync(TgEfStorageResult<TgEfAppEntity> storageResult)
	{
		if (!storageResult.IsExists)
			return new(TgEnumEntityState.NotExists);

		var storageResultProxy = await GetAsync(
			new() { Uid = storageResult.Item.ProxyUid ?? Guid.Empty }, isReadOnly: false);
		return storageResultProxy.IsExists ? storageResultProxy : new(TgEnumEntityState.NotExists);
	}

	public async Task<Guid> GetCurrentProxyUidAsync(TgEfStorageResult<TgEfAppEntity> storageResult) => (await GetCurrentProxyAsync(storageResult)).Item.Uid;

	#endregion
}