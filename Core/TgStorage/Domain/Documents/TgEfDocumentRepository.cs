// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Documents;

/// <summary> Document repository </summary>
public sealed class TgEfDocumentRepository : TgEfRepositoryBase<TgEfDocumentEntity>
{
	#region Public and private methods

	public override string ToDebugString() => $"{nameof(TgEfDocumentRepository)}";

	public override IQueryable<TgEfDocumentEntity> GetQuery(ITgEfContext efContext, bool isReadOnly = true)
	{
		return isReadOnly ? efContext.Documents.AsNoTracking() : efContext.Documents.AsTracking();
	}

	public override async Task<TgEfStorageResult<TgEfDocumentEntity>> GetAsync(TgEfDocumentEntity item, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		// Find by Uid
		var itemFind = await efContext.Documents.FindAsync(item.Uid);
		if (itemFind is not null)
			return new(TgEnumEntityState.IsExists, itemFind);
		// Find by SourceId and Id and MessageId
		itemFind = await GetQuery(efContext, isReadOnly)
				.Where(x => x != null && x.SourceId == item.SourceId && x.Id == item.Id && x.MessageId == item.MessageId)
				.Include(x => x.Source).SingleOrDefaultAsync();
		return itemFind is not null
			? new(TgEnumEntityState.IsExists, itemFind)
			: new TgEfStorageResult<TgEfDocumentEntity>(TgEnumEntityState.NotExists, item);
	}

	public override async Task<TgEfStorageResult<TgEfDocumentEntity>> GetFirstAsync(bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		var item = await GetQuery(efContext, isReadOnly).Include(x => x.Source).FirstOrDefaultAsync();
		return item is null
			? new(TgEnumEntityState.NotExists)
			: new TgEfStorageResult<TgEfDocumentEntity>(TgEnumEntityState.IsExists, item);
	}

	public override async Task<TgEfStorageResult<TgEfDocumentEntity>> GetListAsync(int take, int skip, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		IList<TgEfDocumentEntity> items = take > 0
			? await GetQuery(efContext, isReadOnly).Include(x => x.Source).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(efContext, isReadOnly).Include(x => x.Source).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<TgEfStorageResult<TgEfDocumentEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfDocumentEntity, bool>> where, bool isReadOnly = true)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		IList<TgEfDocumentEntity> items = take > 0
			? await GetQuery(efContext, isReadOnly).Where(where).Include(x => x.Source).Skip(skip).Take(take).ToListAsync()
			: await GetQuery(efContext, isReadOnly).Where(where).Include(x => x.Source).ToListAsync();
		return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
	}

	public override async Task<int> GetCountAsync()
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		return await efContext.Documents.AsNoTracking().CountAsync();
	}

	public override async Task<int> GetCountAsync(Expression<Func<TgEfDocumentEntity, bool>> where)
	{
		await using var scope = TgGlobalTools.Container.BeginLifetimeScope();
		using var efContext = scope.Resolve<ITgEfContext>();
		return await efContext.Documents.AsNoTracking().Where(where).CountAsync();
	}

	#endregion

	#region Public and private methods - Delete

	public override async Task<TgEfStorageResult<TgEfDocumentEntity>> DeleteAllAsync()
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