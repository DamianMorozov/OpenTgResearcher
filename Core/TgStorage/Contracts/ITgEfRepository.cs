namespace TgStorage.Contracts;

public interface ITgEfRepository<TEfEntity, TDto> 
	where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
    where TDto : class, ITgDto<TEfEntity, TDto>, new()
{
    #region Methods

    public IQueryable<TEfEntity> GetQuery(bool isReadOnly = true);

    #endregion

    #region Methods - Read

    /// <summary> Check if the item exists in the storage </summary>
    public Task<bool> CheckExistsAsync(TEfEntity item, CancellationToken ct = default);
    /// <summary> Check if the item exists in the storage </summary>
	public Task<bool> CheckExistsByDtoAsync(TDto dto, CancellationToken ct = default);
    /// <summary> Check if the item exists in the storage </summary>
	public bool CheckExists(TEfEntity item);
    /// <summary> Check if the item exists in the storage </summary>
	public bool CheckExistsByDto(TDto dto);
    /// <summary> Get result of the item from the storage </summary>
	public Task<TgEfStorageResult<TEfEntity>> GetAsync(TEfEntity item, bool isReadOnly = true, CancellationToken ct = default);
    /// <summary> Get result of the item from the storage </summary>
	public Task<TgEfStorageResult<TEfEntity>> GetByDtoAsync(TDto dto, bool isReadOnly = true, CancellationToken ct = default);
    /// <summary> Get item from the storage </summary>
	public Task<TEfEntity> GetItemAsync(TEfEntity item, bool isReadOnly = true, CancellationToken ct = default);
    /// <summary> Get item from the storage </summary>
	public Task<TEfEntity> GetItemWhereAsync(Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true, CancellationToken ct = default);
    /// <summary> Get result of the item from the storage </summary>
	public TgEfStorageResult<TEfEntity> Get(TEfEntity item, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetNewAsync(bool isReadOnly = true, CancellationToken ct = default);
	public TgEfStorageResult<TEfEntity> GetNew(bool isReadOnly = true);
    public TEfEntity GetNewItem(bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetListAsync(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true, CancellationToken ct = default);
	public Task<IEnumerable<TEfEntity>> GetListItemsAsync(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true, CancellationToken ct = default);
    public IEnumerable<TEfEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetListAsync(int take, int skip, bool isReadOnly = true, CancellationToken ct = default);
	public TgEfStorageResult<TEfEntity> GetList(int take, int skip, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetListAsync(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true, CancellationToken ct = default);
	public Task<IEnumerable<TEfEntity>> GetListItemsAsync(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true, CancellationToken ct = default);
	public TgEfStorageResult<TEfEntity> GetList(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public IEnumerable<TEfEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public Task<TgEfStorageResult<TEfEntity>> GetListAsync(int take, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true, CancellationToken ct = default);
	public TgEfStorageResult<TEfEntity> GetList(int take, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true);
	public Task<int> GetCountAsync(CancellationToken ct = default);
	public int GetCount();
	public Task<int> GetCountAsync(Expression<Func<TEfEntity, bool>> where, CancellationToken ct = default);
	public int GetCount(Expression<Func<TEfEntity, bool>> where);

    #endregion

    #region Methods - Read DTO

    public Expression<Func<TEfEntity, TDto>> SelectDto();
	public Task<TDto> GetDtoAsync(Expression<Func<TEfEntity, bool>> where, CancellationToken ct = default);
    public Task<int> DeleteDtoAsync(Expression<Func<TEfEntity, bool>> where, CancellationToken ct = default);
    public Task<List<TDto>> GetListDtosAsync(int take = 0, int skip = 0, CancellationToken ct = default);
    public Task<List<TDto>> GetListDtosAsync(int take, int skip, Expression<Func<TEfEntity, bool>> where, CancellationToken ct = default);
    public Task<List<TDto>> GetListDtosAsync<TKey>(int take, int skip, Expression<Func<TEfEntity, bool>> where, Expression<Func<TEfEntity, TKey>> order, CancellationToken ct = default);
    public Task<List<TDto>> GetListDtosDescAsync<TKey>(int take, int skip, Expression<Func<TEfEntity, bool>> where, Expression<Func<TEfEntity, TKey>> order, CancellationToken ct = default);
    public Task<int> GetListCountAsync(CancellationToken ct = default);
	public Task<int> GetListCountAsync(Expression<Func<TEfEntity, bool>> where, CancellationToken ct = default);
    public Task<TDto?> GetFirstOrDefaultAsync<TKey>(Expression<Func<TEfEntity, bool>> where, Expression<Func<TEfEntity, TKey>> order, CancellationToken ct = default);
    public Task<TDto?> GetFirstOrDefaultDescAsync<TKey>(Expression<Func<TEfEntity, bool>> where, Expression<Func<TEfEntity, TKey>> order, CancellationToken ct = default);

    #endregion

    #region Methods - Write

    public Task<TgEfStorageResult<TEfEntity>> SaveAsync(TEfEntity item, bool isFirstTry = true, CancellationToken ct = default);
	public TgEfStorageResult<TEfEntity> Save(TEfEntity item);
	public Task<bool> SaveListAsync(IEnumerable<TEfEntity> items, bool isRewriteEntities, bool isFirstTry = true, CancellationToken ct = default);

    #endregion

    #region Methods - Remove

    /// <summary> Delete item from the storage table </summary>
    public Task<TgEfStorageResult<TEfEntity>> DeleteAsync(TEfEntity item, CancellationToken ct = default);
    /// <summary> Delete item from the storage table </summary>
    public Task<TgEfStorageResult<TEfEntity>> DeleteAsync(Expression<Func<TEfEntity, bool>> where, CancellationToken ct = default);
    /// <summary> Delete new item from the storage table </summary>
	public Task<TgEfStorageResult<TEfEntity>> DeleteNewAsync(CancellationToken ct = default);
    /// <summary> Delete all items from the storage table </summary>
	public Task<TgEfStorageResult<TEfEntity>> DeleteAllAsync(CancellationToken ct = default);

	#endregion
}
