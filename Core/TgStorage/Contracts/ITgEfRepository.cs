// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Contracts;

public interface ITgEfRepository<TEntity> where TEntity : ITgDbFillEntity<TEntity>, new()
{
	#region Public and private fields, properties, constructor

	//public ITgEfContext EfContext { get; }

	#endregion

	#region Public and private methods

	public IQueryable<TEntity> GetQuery(ITgEfContext efContext, bool isReadOnly = true);

	#endregion

	#region Public and private methods - Read

	public Task<TgEfStorageResult<TEntity>> GetAsync(TEntity item, bool isReadOnly = true);
	public Task<TEntity> GetItemAsync(TEntity item, bool isReadOnly = true);
	public TgEfStorageResult<TEntity> Get(TEntity item, bool isReadOnly = true);
	public TEntity GetItem(TEntity item, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEntity>> GetNewAsync(bool isReadOnly = true);
	public TgEfStorageResult<TEntity> GetNew(bool isReadOnly = true);
	public Task<TgEfStorageResult<TEntity>> GetFirstAsync(bool isReadOnly = true);
	public TgEfStorageResult<TEntity> GetFirst(bool isReadOnly = true);
	public Task<TEntity> GetFirstItemAsync(bool isReadOnly = true);
	public TEntity GetFirstItem(bool isReadOnly = true);
	public Task<TgEfStorageResult<TEntity>> GetListAsync(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true);
	public Task<IEnumerable<TEntity>> GetListItemsAsync(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true);
	public TgEfStorageResult<TEntity> GetList(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true);
	public IEnumerable<TEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEntity>> GetListAsync(int take, int skip, bool isReadOnly = true);
	public Task<IEnumerable<TEntity>> GetListItemsAsync(int take, int skip, bool isReadOnly = true);
	public TgEfStorageResult<TEntity> GetList(int take, int skip, bool isReadOnly = true);
	public IEnumerable<TEntity> GetListItems(int take, int skip, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEntity>> GetListAsync(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEntity, bool>> where, bool isReadOnly = true);
	public Task<IEnumerable<TEntity>> GetListItemsAsync(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEntity, bool>> where, bool isReadOnly = true);
	public TgEfStorageResult<TEntity> GetList(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEntity, bool>> where, bool isReadOnly = true);
	public IEnumerable<TEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEntity, bool>> where, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEntity>> GetListAsync(int take, int skip, Expression<Func<TEntity, bool>> where, bool isReadOnly = true);
	public Task<IEnumerable<TEntity>> GetListItemsAsync(int take, int skip, Expression<Func<TEntity, bool>> where, bool isReadOnly = true);
	public TgEfStorageResult<TEntity> GetList(int take, int skip, Expression<Func<TEntity, bool>> where, bool isReadOnly = true);
	public IEnumerable<TEntity> GetListItems(int take, int skip, Expression<Func<TEntity, bool>> where, bool isReadOnly = true);
	public Task<int> GetCountAsync();
	public int GetCount();
	public Task<int> GetCountAsync(Expression<Func<TEntity, bool>> where);
	public int GetCount(Expression<Func<TEntity, bool>> where);

	#endregion

	#region Public and private methods - Write

	public Task<TgEfStorageResult<TEntity>> SaveAsync(TEntity item, bool isFirstTry = true);
	public TgEfStorageResult<TEntity> Save(TEntity item);
	public Task<TgEfStorageResult<TEntity>> SaveListAsync(List<TEntity> items);
	public TgEfStorageResult<TEntity> SaveList(List<TEntity> items);
	public Task<TgEfStorageResult<TEntity>> SaveWithoutTransactionAsync(TEntity item);
	public TgEfStorageResult<TEntity> SaveWithoutTransaction(TEntity item);
	public Task<TgEfStorageResult<TEntity>> SaveOrRecreateAsync(TEntity item, string tableName);
	public TgEfStorageResult<TEntity> SaveOrRecreate(TEntity item, string tableName);
	public Task<TgEfStorageResult<TEntity>> CreateNewAsync();
	public TgEfStorageResult<TEntity> CreateNew();

	#endregion

	#region Public and private methods - Remove

	public Task<TgEfStorageResult<TEntity>> DeleteAsync(TEntity item);
	public Task<TgEfStorageResult<TEntity>> DeleteNewAsync();
	public Task<TgEfStorageResult<TEntity>> DeleteAllAsync();

	#endregion
}