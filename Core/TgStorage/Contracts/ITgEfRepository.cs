// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Contracts;

public interface ITgEfRepository<TEfEntity, TDto> 
	where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
    where TDto : class, ITgDto<TEfEntity, TDto>, new()
{
    #region Public and private methods

    public IQueryable<TEfEntity> GetQuery(bool isReadOnly = true);

    #endregion

    #region Public and private methods - Read

    /// <summary> Check if the item exists in the storage </summary>
    public Task<bool> CheckExistsAsync(TEfEntity item);
    /// <summary> Check if the item exists in the storage </summary>
	public Task<bool> CheckExistsByDtoAsync(TDto dto);
    /// <summary> Check if the item exists in the storage </summary>
	public bool CheckExists(TEfEntity item);
    /// <summary> Check if the item exists in the storage </summary>
	public bool CheckExistsByDto(TDto dto);
    /// <summary> Get result of the item from the storage </summary>
	public Task<TgEfStorageResult<TEfEntity>> GetAsync(TEfEntity item, bool isReadOnly = true);
    /// <summary> Get result of the item from the storage </summary>
	public Task<TgEfStorageResult<TEfEntity>> GetByDtoAsync(TDto dto, bool isReadOnly = true);
    /// <summary> Get item from the storage </summary>
	public Task<TEfEntity> GetItemAsync(TEfEntity item, bool isReadOnly = true);
    /// <summary> Get item from the storage </summary>
	public Task<TEfEntity> GetItemWhereAsync(Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
    /// <summary> Get result of the item from the storage </summary>
	public TgEfStorageResult<TEfEntity> Get(TEfEntity item, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetNewAsync(bool isReadOnly = true);
	public TgEfStorageResult<TEfEntity> GetNew(bool isReadOnly = true);
    public TEfEntity GetNewItem(bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetListAsync(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true);
	public Task<IEnumerable<TEfEntity>> GetListItemsAsync(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true);
    public IEnumerable<TEfEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetListAsync(int take, int skip, bool isReadOnly = true);
	public TgEfStorageResult<TEfEntity> GetList(int take, int skip, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetListAsync(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public Task<IEnumerable<TEfEntity>> GetListItemsAsync(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public TgEfStorageResult<TEfEntity> GetList(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public IEnumerable<TEfEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetListAsync(int take, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public TgEfStorageResult<TEfEntity> GetList(int take, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public Task<int> GetCountAsync();
	public int GetCount();
	public Task<int> GetCountAsync(Expression<Func<TEfEntity, bool>> where);
	public int GetCount(Expression<Func<TEfEntity, bool>> where);

    #endregion

    #region Public and private methods - Read DTO

    public Expression<Func<TEfEntity, TDto>> SelectDto();
	public Task<TDto> GetDtoAsync(Expression<Func<TEfEntity, bool>> where);
    public Task<List<TDto>> GetListDtosAsync(int take = 0, int skip = 0);
    public Task<List<TDto>> GetListDtosAsync(int take, int skip, Expression<Func<TEfEntity, bool>> where);
    public Task<List<TDto>> GetListDtosAsync<TKey>(int take, int skip, Expression<Func<TEfEntity, bool>> where, Expression<Func<TEfEntity, TKey>> order);
    public Task<List<TDto>> GetListDtosDescAsync<TKey>(int take, int skip, Expression<Func<TEfEntity, bool>> where, Expression<Func<TEfEntity, TKey>> order);
    public Task<int> GetListCountAsync();
	public Task<int> GetListCountAsync(Expression<Func<TEfEntity, bool>> where);

	#endregion

	#region Public and private methods - Write

	public Task<TgEfStorageResult<TEfEntity>> SaveAsync(TEfEntity item, bool isFirstTry = true);
	public TgEfStorageResult<TEfEntity> Save(TEfEntity item);
	public Task<bool> SaveListAsync(IEnumerable<TEfEntity> items, bool isFirstTry = true);
	public bool SaveList(List<TEfEntity> items, bool isFirstTry = true);
	public Task<TgEfStorageResult<TEfEntity>> SaveWithoutTransactionAsync(TEfEntity item);
	public TgEfStorageResult<TEfEntity> SaveWithoutTransaction(TEfEntity item);
	public Task<TgEfStorageResult<TEfEntity>> SaveOrRecreateAsync(TEfEntity item, string tableName);
	public TgEfStorageResult<TEfEntity> SaveOrRecreate(TEfEntity item, string tableName);

    #endregion

    #region Public and private methods - Remove

    /// <summary> Delete item from the storage table </summary>
    public Task<TgEfStorageResult<TEfEntity>> DeleteAsync(TEfEntity item);
    /// <summary> Delete new item from the storage table </summary>
	public Task<TgEfStorageResult<TEfEntity>> DeleteNewAsync();
    /// <summary> Delete all items from the storage table </summary>
	public Task<TgEfStorageResult<TEfEntity>> DeleteAllAsync();

	#endregion
}
