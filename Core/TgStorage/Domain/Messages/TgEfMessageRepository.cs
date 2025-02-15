// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Messages;

/// <summary> Message repository </summary>
public sealed class TgEfMessageRepository : TgEfRepositoryBase<TgEfMessageEntity>
{
	#region Public and private methods

	public override string ToDebugString() => $"{nameof(TgEfMessageRepository)}";

	public override IQueryable<TgEfMessageEntity> GetQuery(ITgEfContext efContext, bool isReadOnly = true)
	{
		return isReadOnly ? efContext.Messages.AsNoTracking() : efContext.Messages.AsTracking();
	}

	public override async Task<TgEfStorageResult<TgEfMessageEntity>> GetAsync(TgEfMessageEntity item, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		// Find by Uid
		var itemFind = await efContext.Messages.FindAsync(item.Uid);
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by Id
		itemFind = await GetQuery(efContext, isReadOnly)
			.Where(x => x.SourceId == item.SourceId && x.Id == item.Id)
			.Include(x => x.Source)
			.SingleOrDefaultAsync();
		return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfMessageEntity>(TgEnumEntityState.NotExists, item);
	}

	public override async Task<TgEfStorageResult<TgEfMessageEntity>> GetFirstAsync(bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		var item = await GetQuery(efContext, isReadOnly).Include(x => x.Source).FirstOrDefaultAsync();
		return item is null
			? new(TgEnumEntityState.NotExists)
			: new TgEfStorageResult<TgEfMessageEntity>(TgEnumEntityState.IsExists, item);
	}

	private static Expression<Func<TgEfMessageEntity, TgEfMessageDto>> SelectDto() => item => new TgEfMessageDto().GetDto(item);

	public async Task<TgEfMessageDto> GetDtoAsync(Expression<Func<TgEfMessageEntity, bool>> where)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		var dto = await GetQuery(efContext).Where(where).Select(SelectDto()).SingleOrDefaultAsync() ?? new TgEfMessageDto();
		return dto;
	}

	public async Task<List<TgEfMessageDto>> GetListDtosAsync(int take, int skip, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		var dtos = take > 0
			? await GetQuery(efContext, isReadOnly).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(efContext, isReadOnly).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public async Task<List<TgEfMessageDto>> GetListDtosAsync(int take, int skip, Expression<Func<TgEfMessageEntity, bool>> where, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		var dtos = take > 0
			? await GetQuery(efContext, isReadOnly).Where(where).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
			: await GetQuery(efContext, isReadOnly).Where(where).Select(SelectDto()).ToListAsync();
		return dtos;
	}

	public override async Task<TgEfStorageResult<TgEfMessageEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		IList<TgEfMessageEntity> items = take > 0
			? await GetQuery(efContext, isReadOnly).Include(x => x.Source).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(efContext, isReadOnly).Include(x => x.Source).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfMessageEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfMessageEntity, bool>> where, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		IList<TgEfMessageEntity> items = take > 0
			? await GetQuery(efContext, isReadOnly).Where(where).Include(x => x.Source).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(efContext, isReadOnly).Where(where).Include(x => x.Source).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<int> GetCountAsync()
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		return await efContext.Messages.AsNoTracking().CountAsync();
	}

	public override async Task<int> GetCountAsync(Expression<Func<TgEfMessageEntity, bool>> where)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		return await efContext.Messages.AsNoTracking().Where(where).CountAsync();
	}

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