// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using ValidationException = FluentValidation.ValidationException;

namespace TgStorage.Common;

public class TgEfRepositoryBase<TEntity> : TgCommonBase, ITgEfRepository<TEntity>, IDisposable
	where TEntity : class, ITgDbFillEntity<TEntity>, new()
{
	#region Public and private fields, properties, constructor

	protected ILifetimeScope Scope { get; }
	protected ITgEfContext EfContext { get; }

	public TgEfRepositoryBase()
	{
		Scope = TgGlobalTools.Container.BeginLifetimeScope();
		EfContext = Scope.Resolve<ITgEfContext>();
	}

	#endregion

	#region IDisposable

	public void Dispose()
	{
		Scope.Dispose();
		EfContext.Dispose();
	}

	#endregion


	#region Public and private methods

	public override string ToDebugString() => $"{nameof(TgEfRepositoryBase<TEntity>)}";

	public virtual IQueryable<TEntity> GetQuery(bool isReadOnly = true) => 
		throw new NotImplementedException(TgLocaleHelper.Instance.UseOverrideMethod);

	private static TgEfStorageResult<TEntity> UseOverrideMethod() => throw new NotImplementedException(TgLocaleHelper.Instance.UseOverrideMethod);

	private static async Task<TgEfStorageResult<TEntity>> UseOverrideMethodAsync()
	{
		await Task.CompletedTask;
		throw new NotImplementedException(TgLocaleHelper.Instance.UseOverrideMethod);
	}

	private static async Task<IEnumerable<TEntity>> UseOverrideMethodItemsAsync()
	{
		await Task.CompletedTask;
		throw new NotImplementedException(TgLocaleHelper.Instance.UseOverrideMethod);
	}

	#endregion

	#region Public and private methods - Read

	public virtual async Task<TgEfStorageResult<TEntity>> GetAsync(TEntity item, bool isReadOnly = true) =>
		await TgEfRepositoryBase<TEntity>.UseOverrideMethodAsync();

	public virtual async Task<TEntity> GetItemAsync(TEntity item, bool isReadOnly = true) =>
		(await GetAsync(item, isReadOnly)).Item;

	public TgEfStorageResult<TEntity> Get(TEntity item, bool isReadOnly = true) => GetAsync(item, isReadOnly).GetAwaiter().GetResult();

	public TEntity GetItem(TEntity item, bool isReadOnly = true) => GetItemAsync(item, isReadOnly).GetAwaiter().GetResult();

	public virtual async Task<TgEfStorageResult<TEntity>> GetNewAsync(bool isReadOnly = true) => await GetAsync(new(), isReadOnly);
	
	public virtual async Task<TEntity> GetNewItemAsync(bool isReadOnly = true) => (await GetNewAsync(isReadOnly)).Item;

	public TgEfStorageResult<TEntity> GetNew(bool isReadOnly = true) => GetNewAsync(isReadOnly).GetAwaiter().GetResult();

	public TEntity GetNewItem(bool isReadOnly = true) => GetNewItemAsync(isReadOnly).GetAwaiter().GetResult();

	public virtual async Task<TgEfStorageResult<TEntity>> GetFirstAsync(bool isReadOnly = true) => await TgEfRepositoryBase<TEntity>.UseOverrideMethodAsync();

	public TgEfStorageResult<TEntity> GetFirst(bool isReadOnly = true) => GetFirstAsync(isReadOnly).GetAwaiter().GetResult();

	public virtual async Task<TEntity> GetFirstItemAsync(bool isReadOnly = true) => (await GetFirstAsync(isReadOnly)).Item;

	public TEntity GetFirstItem(bool isReadOnly = true) => GetFirst(isReadOnly).Item;

	public virtual async Task<TgEfStorageResult<TEntity>> GetListAsync(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true) =>
		topRecords switch
		{
			TgEnumTableTopRecords.Top1 => await GetListAsync(1, skip, isReadOnly),
			TgEnumTableTopRecords.Top20 => await GetListAsync(20, skip, isReadOnly),
			TgEnumTableTopRecords.Top100 => await GetListAsync(200, skip, isReadOnly),
			TgEnumTableTopRecords.Top1000 => await GetListAsync(1_000, skip, isReadOnly),
			TgEnumTableTopRecords.Top10000 => await GetListAsync(10_000, skip, isReadOnly),
			TgEnumTableTopRecords.Top100000 => await GetListAsync(100_000, skip, isReadOnly),
			TgEnumTableTopRecords.Top1000000 => await GetListAsync(1_000_000, skip, isReadOnly),
			_ => await GetListAsync(0, skip, isReadOnly),
		};

	public virtual async Task<IEnumerable<TEntity>> GetListItemsAsync(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true) =>
		topRecords switch
		{
			TgEnumTableTopRecords.Top1 => await GetListItemsAsync(1, skip, isReadOnly),
			TgEnumTableTopRecords.Top20 => await GetListItemsAsync(20, skip, isReadOnly),
			TgEnumTableTopRecords.Top100 => await GetListItemsAsync(200, skip, isReadOnly),
			TgEnumTableTopRecords.Top1000 => await GetListItemsAsync(1_000, skip, isReadOnly),
			TgEnumTableTopRecords.Top10000 => await GetListItemsAsync(10_000, skip, isReadOnly),
			TgEnumTableTopRecords.Top100000 => await GetListItemsAsync(100_000, skip, isReadOnly),
			TgEnumTableTopRecords.Top1000000 => await GetListItemsAsync(1_000_000, skip, isReadOnly),
			_ => await GetListItemsAsync(0, skip, isReadOnly),
		};

	public TgEfStorageResult<TEntity> GetList(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true) => 
		GetListAsync(topRecords, skip, isReadOnly).GetAwaiter().GetResult();

	public IEnumerable<TEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true) => 
		GetListItemsAsync(topRecords, skip, isReadOnly).GetAwaiter().GetResult();

	public virtual async Task<TgEfStorageResult<TEntity>> GetListAsync(int take, int skip, bool isReadOnly = true) => 
		await TgEfRepositoryBase<TEntity>.UseOverrideMethodAsync();

	public virtual async Task<IEnumerable<TEntity>> GetListItemsAsync(int take, int skip, bool isReadOnly = true) => 
		await TgEfRepositoryBase<TEntity>.UseOverrideMethodItemsAsync();

	public TgEfStorageResult<TEntity> GetList(int take, int skip, bool isReadOnly = true) =>
		GetListAsync(take, skip, isReadOnly).GetAwaiter().GetResult();

	public IEnumerable<TEntity> GetListItems(int take, int skip, bool isReadOnly = true) =>
		GetListItemsAsync(take, skip, isReadOnly).GetAwaiter().GetResult();

	public virtual async Task<TgEfStorageResult<TEntity>> GetListAsync(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEntity, bool>> where, 
		bool isReadOnly = true) =>
		topRecords switch
		{
			TgEnumTableTopRecords.Top1 => await GetListAsync(1, skip, where, isReadOnly),
			TgEnumTableTopRecords.Top20 => await GetListAsync(20, skip, where, isReadOnly),
			TgEnumTableTopRecords.Top100 => await GetListAsync(200, skip, where, isReadOnly),
			TgEnumTableTopRecords.Top1000 => await GetListAsync(1_000, skip, where, isReadOnly),
			TgEnumTableTopRecords.Top10000 => await GetListAsync(10_000, skip, where, isReadOnly),
			TgEnumTableTopRecords.Top100000 => await GetListAsync(100_000, skip, where, isReadOnly),
			TgEnumTableTopRecords.Top1000000 => await GetListAsync(1_000_000, skip, where, isReadOnly),
			_ => await GetListAsync(0, skip, where, isReadOnly),
		};

	public virtual async Task<IEnumerable<TEntity>> GetListItemsAsync(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEntity, bool>> where, 
		bool isReadOnly = true) =>
		topRecords switch
		{
			TgEnumTableTopRecords.Top1 => await GetListItemsAsync(1, skip, where, isReadOnly),
			TgEnumTableTopRecords.Top20 => await GetListItemsAsync(20, skip, where, isReadOnly),
			TgEnumTableTopRecords.Top100 => await GetListItemsAsync(200, skip, where, isReadOnly),
			TgEnumTableTopRecords.Top1000 => await GetListItemsAsync(1_000, skip, where, isReadOnly),
			TgEnumTableTopRecords.Top10000 => await GetListItemsAsync(10_000, skip, where, isReadOnly),
			TgEnumTableTopRecords.Top100000 => await GetListItemsAsync(100_000, skip, where, isReadOnly),
			TgEnumTableTopRecords.Top1000000 => await GetListItemsAsync(1_000_000, skip, where, isReadOnly),
			_ => await GetListItemsAsync(0, skip, where, isReadOnly),
		};

	public TgEfStorageResult<TEntity> GetList(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEntity, bool>> where, bool isReadOnly = true) =>
		GetListAsync(topRecords, skip, where, isReadOnly).GetAwaiter().GetResult();

	public IEnumerable<TEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEntity, bool>> where, bool isReadOnly = true) =>
		GetListItemsAsync(topRecords, skip, where, isReadOnly).GetAwaiter().GetResult();

	public virtual async Task<TgEfStorageResult<TEntity>> GetListAsync(int take, int skip, Expression<Func<TEntity, bool>> where, bool isReadOnly = true) =>
		await TgEfRepositoryBase<TEntity>.UseOverrideMethodAsync();

	public virtual async Task<IEnumerable<TEntity>> GetListItemsAsync(int take, int skip, Expression<Func<TEntity, bool>> where, bool isReadOnly = true) =>
		await TgEfRepositoryBase<TEntity>.UseOverrideMethodItemsAsync();

	public TgEfStorageResult<TEntity> GetList(int take, int skip, Expression<Func<TEntity, bool>> where, bool isReadOnly = true) =>
		GetListAsync(take, skip, where, isReadOnly).GetAwaiter().GetResult();

	public IEnumerable<TEntity> GetListItems(int take, int skip, Expression<Func<TEntity, bool>> where, bool isReadOnly = true) =>
		GetListItemsAsync(take, skip, where, isReadOnly).GetAwaiter().GetResult();

	public virtual async Task<int> GetCountAsync() => await Task.FromResult(0);

	public int GetCount() => GetCountAsync().GetAwaiter().GetResult();

	public virtual async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> where) => await Task.FromResult(0);

	public int GetCount(Expression<Func<TEntity, bool>> where) => GetCountAsync(where).GetAwaiter().GetResult();

	#endregion

	#region Public and private methods - Write

	public virtual async Task<TgEfStorageResult<TEntity>> SaveAsync(TEntity? item, bool isFirstTry = true)
	{
		var transaction = await EfContext.Database.BeginTransactionAsync();
		TgEfStorageResult<TEntity> storageResult = new(TgEnumEntityState.Unknown, item);
		await using (transaction)
		{
			if (item is null) return storageResult;
			try
			{
				// Load actual entity
				storageResult = await GetAsync(item, isReadOnly: false);
				// Entity is not exists - Create
				if (!storageResult.IsExists)
				{
					await EfContext.AddItemAsync(storageResult.Item);
				}
				// Entity is existing - Update
				else
				{
					storageResult.Item.Fill(item, isUidCopy: false);
				}
				// Validate entity
				var validationResult = TgEfUtils.GetEfValid(storageResult.Item);
				if (!validationResult.IsValid)
					throw new ValidationException(validationResult.Errors);
				// Normilize entity
				TgEfUtils.Normilize(storageResult.Item);
				// Save entity
				//await EfContext.SaveChangesAsync();
				// Entity is not exists - Create
				if (!storageResult.IsExists)
				{
					await EfContext.SaveChangesAsync();
				}
				// Entity is existing - Update
				else
				{
					EfContext.UpdateItem(storageResult.Item);
				}
				EfContext.DetachItem(storageResult.Item);
				await transaction.CommitAsync();
				storageResult.State = TgEnumEntityState.IsSaved;
			}
			catch (DbUpdateConcurrencyException ex)
			{
				await transaction.RollbackAsync();
#if DEBUG
				Debug.WriteLine(ex, TgConstants.LogTypeStorage);
#endif
				// Retry
				if (isFirstTry)
				{
					var entry = ex.Entries.Single();
					var databaseValues = await entry.GetDatabaseValuesAsync() ?? throw new Exception("The record you attempted to edit was deleted!");
					entry.OriginalValues.SetValues(databaseValues);
					await SaveAsync(item, isFirstTry: false);
				}
				else
					throw;
			}
#if DEBUG
			catch (Exception ex)
#else
			catch (Exception)
#endif
			{
				await transaction.RollbackAsync();
#if DEBUG
				Debug.WriteLine(ex, TgConstants.LogTypeStorage);
#endif
				throw;
			}
			return storageResult;
		}
	}

	public TgEfStorageResult<TEntity> Save(TEntity? item) => SaveAsync(item).GetAwaiter().GetResult();

	public virtual async Task<TgEfStorageResult<TEntity>> SaveListAsync(List<TEntity> items)
	{
		var transaction = await EfContext.Database.BeginTransactionAsync();
		await using (transaction)
		{
			TgEfStorageResult<TEntity> storageResult = new(TgEnumEntityState.Unknown, items);
			try
			{
				var uniqueItems = items.Distinct().ToList();
				foreach (var item in uniqueItems)
				{
					TgEfUtils.Normilize(item);
					await EfContext.AddItemAsync(item);
				}
				await EfContext.SaveChangesAsync();
				await transaction.CommitAsync();
				storageResult.State = TgEnumEntityState.IsSaved;
			}
#if DEBUG
			catch (Exception ex)
#else
			catch (Exception)
#endif
			{
				await transaction.RollbackAsync();
#if DEBUG
				Debug.WriteLine(ex, TgConstants.LogTypeStorage);
#endif
				throw;
			}
			return storageResult;
		}
	}

	public TgEfStorageResult<TEntity> SaveList(List<TEntity> items) => SaveListAsync(items).GetAwaiter().GetResult();

	public virtual async Task<TgEfStorageResult<TEntity>> SaveWithoutTransactionAsync(TEntity item)
	{
		TgEfStorageResult<TEntity> storageResult;
		try
		{
			storageResult = await GetAsync(item, isReadOnly: false);
			// Create.
			if (!storageResult.IsExists)
			{
				TgEfUtils.Normilize(storageResult.Item);
				await EfContext.AddItemAsync(storageResult.Item);
				await EfContext.SaveChangesAsync();
			}
			// Update.
			else
			{
				storageResult.Item.Fill(item, false);
				var validationResult = TgEfUtils.GetEfValid(storageResult.Item);
				if (!validationResult.IsValid)
					throw new ValidationException(validationResult.Errors);
				await EfContext.SaveChangesAsync();
			}
			storageResult.State = TgEnumEntityState.IsSaved;
		}
#if DEBUG
		catch (Exception ex)
#else
		catch (Exception)
#endif
		{
#if DEBUG
			Debug.WriteLine(ex, TgConstants.LogTypeStorage);
#endif
			throw;
		}
		return storageResult;
	}

	public TgEfStorageResult<TEntity> SaveWithoutTransaction(TEntity item) => SaveWithoutTransactionAsync(item).GetAwaiter().GetResult();

	public virtual async Task<TgEfStorageResult<TEntity>> SaveOrRecreateAsync(TEntity item, string tableName)
	{
		try
		{
			return await SaveAsync(item);
		}
#if DEBUG
		catch (Exception ex)
#else
		catch (Exception)
#endif
		{
			var itemBackup = item;
			await DeleteAsync(item);
#if DEBUG
			Debug.WriteLine(ex, TgConstants.LogTypeStorage);
#endif
			try
			{
				return await SaveAsync(itemBackup);
			}
			catch (Exception)
			{
				throw;
			}
		}
	}

	public TgEfStorageResult<TEntity> SaveOrRecreate(TEntity item, string tableName) => SaveOrRecreateAsync(item, tableName).GetAwaiter().GetResult();

	#endregion

	#region Public and private methods - Remove

	public virtual async Task<TgEfStorageResult<TEntity>> DeleteAsync(TEntity item)
	{
		var transaction = await EfContext.Database.BeginTransactionAsync();
		await using (transaction)
		{
			try
			{
				var storageResult = await GetAsync(item, isReadOnly: false);
				if (!storageResult.IsExists)
					return storageResult;
				EfContext.RemoveItem(storageResult.Item);
				await EfContext.SaveChangesAsync();
				await transaction.CommitAsync();
				return new(TgEnumEntityState.IsDeleted);
			}
#if DEBUG
			catch (Exception ex)
#else
			catch (Exception)
#endif
			{
				await transaction.RollbackAsync();
#if DEBUG
				Debug.WriteLine(ex, TgConstants.LogTypeStorage);
#endif
				throw;
			}
		}
	}

	public virtual async Task<TgEfStorageResult<TEntity>> DeleteNewAsync()
	{
		var storageResult = await GetNewAsync(isReadOnly: false);
		return storageResult.IsExists
			? await DeleteAsync(storageResult.Item)
			: new(TgEnumEntityState.NotDeleted);
	}

	public virtual async Task<TgEfStorageResult<TEntity>> DeleteAllAsync()
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