// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Contracts;

public interface ITgEfRepository<TEfEntity> where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
{
	#region Public and private methods

	public IQueryable<TEfEntity> GetQuery(bool isReadOnly = true);

	#endregion

	#region Public and private methods - Read

	public Task<bool> CheckExistsAsync(TEfEntity item);
	public bool CheckExists(TEfEntity item);
	public Task<TgEfStorageResult<TEfEntity>> GetAsync(TEfEntity item, bool isReadOnly = true);
	public Task<TEfEntity> GetItemAsync(TEfEntity item, bool isReadOnly = true);
	public TgEfStorageResult<TEfEntity> Get(TEfEntity item, bool isReadOnly = true);
	public TEfEntity GetItem(TEfEntity item, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetNewAsync(bool isReadOnly = true);
	public TgEfStorageResult<TEfEntity> GetNew(bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetFirstAsync(bool isReadOnly = true);
	public TgEfStorageResult<TEfEntity> GetFirst(bool isReadOnly = true);
	public Task<TEfEntity> GetFirstItemAsync(bool isReadOnly = true);
	public TEfEntity GetFirstItem(bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetListAsync(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true);
	public Task<IEnumerable<TEfEntity>> GetListItemsAsync(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true);
	public TgEfStorageResult<TEfEntity> GetList(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true);
	public IEnumerable<TEfEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetListAsync(int take, int skip, bool isReadOnly = true);
	public Task<IEnumerable<TEfEntity>> GetListItemsAsync(int take, int skip, bool isReadOnly = true);
	public TgEfStorageResult<TEfEntity> GetList(int take, int skip, bool isReadOnly = true);
	public IEnumerable<TEfEntity> GetListItems(int take, int skip, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetListAsync(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public Task<IEnumerable<TEfEntity>> GetListItemsAsync(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public TgEfStorageResult<TEfEntity> GetList(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public IEnumerable<TEfEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetListAsync(int take, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public Task<IEnumerable<TEfEntity>> GetListItemsAsync(int take, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public TgEfStorageResult<TEfEntity> GetList(int take, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public IEnumerable<TEfEntity> GetListItems(int take, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public Task<int> GetCountAsync();
	public int GetCount();
	public Task<int> GetCountAsync(Expression<Func<TEfEntity, bool>> where);
	public int GetCount(Expression<Func<TEfEntity, bool>> where);

	#endregion

	#region Public and private methods - Write

	public Task<TgEfStorageResult<TEfEntity>> SaveAsync(TEfEntity item, bool isFirstTry = true);
	public TgEfStorageResult<TEfEntity> Save(TEfEntity item);
	public Task<bool> SaveListAsync(List<TEfEntity> items, bool isFirstTry = true);
	public bool SaveList(List<TEfEntity> items, bool isFirstTry = true);
	public Task<TgEfStorageResult<TEfEntity>> SaveWithoutTransactionAsync(TEfEntity item);
	public TgEfStorageResult<TEfEntity> SaveWithoutTransaction(TEfEntity item);
	public Task<TgEfStorageResult<TEfEntity>> SaveOrRecreateAsync(TEfEntity item, string tableName);
	public TgEfStorageResult<TEfEntity> SaveOrRecreate(TEfEntity item, string tableName);

	#endregion

	#region Public and private methods - Remove

	public Task<TgEfStorageResult<TEfEntity>> DeleteAsync(TEfEntity item);
	public Task<TgEfStorageResult<TEfEntity>> DeleteNewAsync();
	public Task<TgEfStorageResult<TEfEntity>> DeleteAllAsync();

	#endregion
}