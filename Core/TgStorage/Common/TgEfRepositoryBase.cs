// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using System.Linq;
using ValidationException = FluentValidation.ValidationException;

namespace TgStorage.Common;

public class TgEfRepositoryBase<TEfEntity, TDto> : TgCommonBase, ITgEfRepository<TEfEntity, TDto>, IDisposable
    where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
    where TDto : class, ITgDto<TEfEntity, TDto>, new()
{
    #region Public and private fields, properties, constructor

    protected ILifetimeScope Scope { get; }
    protected ITgEfContext EfContext { get; }

    public TgEfRepositoryBase()
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();
        EfContext = Scope.Resolve<ITgEfContext>();
    }

    public TgEfRepositoryBase(IWebHostEnvironment webHostEnvironment)
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();
        EfContext = Scope.Resolve<ITgEfContext>(new TypedParameter(typeof(IWebHostEnvironment), webHostEnvironment));
    }

    #endregion

    // TODO: IDisposable pattern is not implemented correctly. Dispose should be called only once.
    #region IDisposable

    private bool _isDisposed;

    ~TgEfRepositoryBase() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        // Release managed resources
        if (disposing)
        {
            //
        }

        // Release unmanaged resources
        Scope.Dispose();
        EfContext.Dispose();

        _isDisposed = true;
    }

    #endregion

    #region Public and private methods

    public override string ToDebugString() => $"{nameof(TgEfRepositoryBase<TEfEntity, TDto>)}";

    public virtual IQueryable<TEfEntity> GetQuery(bool isReadOnly = true) =>
        throw new NotImplementedException(TgConstants.UseOverrideMethod);

    private static TgEfStorageResult<TEfEntity> UseOverrideMethod() => 
        throw new NotImplementedException(TgConstants.UseOverrideMethod);

    private static async Task<TgEfStorageResult<TEfEntity>> UseOverrideMethodAsync()
    {
        await Task.CompletedTask;
        throw new NotImplementedException(TgConstants.UseOverrideMethod);
    }

    private static async Task<IEnumerable<TEfEntity>> UseOverrideMethodItemsAsync()
    {
        await Task.CompletedTask;
        throw new NotImplementedException(TgConstants.UseOverrideMethod);
    }

    #endregion

    #region Public and private methods - Read

    public virtual async Task<bool> CheckExistsAsync(TEfEntity item)
    {
        var storageResult = await GetAsync(item, isReadOnly: true);
        return storageResult.IsExists;
    }

    public virtual bool CheckExists(TEfEntity item) =>
        throw new NotImplementedException(TgConstants.UseOverrideMethod);

    public virtual async Task<TgEfStorageResult<TEfEntity>> GetAsync(TEfEntity item, bool isReadOnly = true) =>
        await UseOverrideMethodAsync();

    public virtual async Task<TEfEntity> GetItemAsync(TEfEntity item, bool isReadOnly = true) =>
        (await GetAsync(item, isReadOnly)).Item;

    public async Task<TEfEntity> GetItemWhereAsync(Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true) =>
        await GetQuery(isReadOnly).FirstOrDefaultAsync(where) ?? new();

    public TgEfStorageResult<TEfEntity> Get(TEfEntity item, bool isReadOnly = true)
    {
        var task = GetAsync(item, isReadOnly);
        task.Wait();
        return task.Result;
    }

    public TEfEntity GetItem(TEfEntity item, bool isReadOnly = true)
    {
        var task = GetItemAsync(item, isReadOnly);
        task.Wait();
        return task.Result;
    }

    public virtual async Task<TgEfStorageResult<TEfEntity>> GetNewAsync(bool isReadOnly = true) => await GetAsync(new(), isReadOnly);

    public virtual async Task<TEfEntity> GetNewItemAsync(bool isReadOnly = true) => (await GetNewAsync(isReadOnly)).Item;

    public TgEfStorageResult<TEfEntity> GetNew(bool isReadOnly = true)
    {
        var task = GetNewAsync(isReadOnly);
        task.Wait();
        return task.Result;
    }

    public TEfEntity GetNewItem(bool isReadOnly = true)
    {
        var task = GetNewItemAsync(isReadOnly);
        task.Wait();
        return task.Result;
    }

    public virtual async Task<TgEfStorageResult<TEfEntity>> GetListAsync(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true) =>
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

    public virtual async Task<IEnumerable<TEfEntity>> GetListItemsAsync(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true) =>
        topRecords switch
        {
            TgEnumTableTopRecords.Top1 => (await GetListAsync(1, skip, isReadOnly)).Items,
            TgEnumTableTopRecords.Top20 => (await GetListAsync(20, skip, isReadOnly)).Items,
            TgEnumTableTopRecords.Top100 => (await GetListAsync(200, skip, isReadOnly)).Items,
            TgEnumTableTopRecords.Top1000 => (await GetListAsync(1_000, skip, isReadOnly)).Items,
            TgEnumTableTopRecords.Top10000 => (await GetListAsync(10_000, skip, isReadOnly)).Items,
            TgEnumTableTopRecords.Top100000 => (await GetListAsync(100_000, skip, isReadOnly)).Items,
            TgEnumTableTopRecords.Top1000000 => (await GetListAsync(1_000_000, skip, isReadOnly)).Items,
            _ => (await GetListAsync(0, skip, isReadOnly)).Items,
        };

    public TgEfStorageResult<TEfEntity> GetList(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true)
    {
        var task = GetListAsync(topRecords, skip, isReadOnly);
        task.Wait();
        return task.Result;
    }

    public IEnumerable<TEfEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true)
    {
        var task = GetListItemsAsync(topRecords, skip, isReadOnly);
        task.Wait();
        return task.Result;
    }

    public virtual async Task<TgEfStorageResult<TEfEntity>> GetListAsync(int take, int skip, bool isReadOnly = true) =>
        await UseOverrideMethodAsync();

    public TgEfStorageResult<TEfEntity> GetList(int take, int skip, bool isReadOnly = true)
    {
        var task = GetListAsync(take, skip, isReadOnly);
        task.Wait();
        return task.Result;
    }

    public virtual async Task<TgEfStorageResult<TEfEntity>> GetListAsync(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where,
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

    public virtual async Task<IEnumerable<TEfEntity>> GetListItemsAsync(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where,
        bool isReadOnly = true) =>
        topRecords switch
        {
            TgEnumTableTopRecords.Top1 => (await GetListAsync(1, skip, where, isReadOnly)).Items,
            TgEnumTableTopRecords.Top20 => (await GetListAsync(20, skip, where, isReadOnly)).Items,
            TgEnumTableTopRecords.Top100 => (await GetListAsync(200, skip, where, isReadOnly)).Items,
            TgEnumTableTopRecords.Top1000 => (await GetListAsync(1_000, skip, where, isReadOnly)).Items,
            TgEnumTableTopRecords.Top10000 => (await GetListAsync(10_000, skip, where, isReadOnly)).Items,
            TgEnumTableTopRecords.Top100000 => (await GetListAsync(100_000, skip, where, isReadOnly)).Items,
            TgEnumTableTopRecords.Top1000000 => (await GetListAsync(1_000_000, skip, where, isReadOnly)).Items,
            _ => (await GetListAsync(0, skip, where, isReadOnly)).Items,
        };

    public TgEfStorageResult<TEfEntity> GetList(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true)
    {
        var task = GetListAsync(topRecords, skip, where, isReadOnly);
        task.Wait();
        return task.Result;
    }

    public IEnumerable<TEfEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true)
    {
        var task = GetListItemsAsync(topRecords, skip, where, isReadOnly);
        task.Wait();
        return task.Result;
    }

    public virtual async Task<TgEfStorageResult<TEfEntity>> GetListAsync(int take, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true) =>
        await UseOverrideMethodAsync();

    public TgEfStorageResult<TEfEntity> GetList(int take, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true)
    {
        var task = GetListAsync(take, skip, where, isReadOnly);
        task.Wait();
        return task.Result;
    }

    public virtual async Task<int> GetCountAsync() => await Task.FromResult(0);

    public int GetCount()
    {
        var task = GetCountAsync();
        task.Wait();
        return task.Result;
    }

    public virtual async Task<int> GetCountAsync(Expression<Func<TEfEntity, bool>> where) => await Task.FromResult(0);

    public int GetCount(Expression<Func<TEfEntity, bool>> where)
    {
        var task = GetCountAsync(where);
        task.Wait();
        return task.Result;
    }

    #endregion

    #region Public and private methods - Read DTO

    public Expression<Func<TEfEntity, TDto>> SelectDto() => item => new TDto().GetNewDto(item);

    public async Task<TDto> GetDtoAsync(Expression<Func<TEfEntity, bool>> where)
    {
        var dto = await GetQuery().Where(where).Select(SelectDto()).SingleOrDefaultAsync() ?? new TDto();
        return dto;
    }

    public async Task<List<TDto>> GetListDtosAsync(int take = 0, int skip = 0)
    {
        var dtos = take > 0
            ? await GetQuery(isReadOnly: true).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
            : await GetQuery(isReadOnly: true).Select(SelectDto()).ToListAsync();
        return dtos;
    }

    public async Task<List<TDto>> GetListDtosAsync(int take, int skip, Expression<Func<TEfEntity, bool>> where)
    {
        var dtos = take > 0
            ? await GetQuery(isReadOnly: true).Where(where).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
            : await GetQuery(isReadOnly: true).Where(where).Select(SelectDto()).ToListAsync();
        return dtos;
    }

    #endregion

    #region Public and private methods - Write

    public virtual async Task<TgEfStorageResult<TEfEntity>> SaveAsync(TEfEntity? item, bool isFirstTry = true)
    {
        var transaction = await EfContext.Database.BeginTransactionAsync();
        TgEfStorageResult<TEfEntity> storageResult = new(TgEnumEntityState.Unknown, item);
        await using (transaction)
        {
            if (item is null)
                return storageResult;
            try
            {
                // Load actual entity
                storageResult = await GetAsync(item, isReadOnly: false);
                // Entity is not exists - Create
                if (!storageResult.IsExists)
                {
                    //
                }
                // Entity is existing - Update
                else
                {
                    storageResult.Item.Copy(item, isUidCopy: false);
                }
                // Validate entity
                var validationResult = TgGlobalTools.GetEfValid(storageResult.Item);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);
                // Normilize entity
                TgGlobalTools.Normilize(storageResult.Item);
                // Entity is not exists - Create
                if (!storageResult.IsExists)
                {
                    await EfContext.AddItemAsync(storageResult.Item);
                }
                // Entity is existing - Update
                else
                {
                    EfContext.UpdateItem(storageResult.Item);
                }
                await EfContext.SaveChangesAsync();
                EfContext.DetachItem(storageResult.Item);
                await transaction.CommitAsync();
                storageResult.State = TgEnumEntityState.IsSaved;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
#if DEBUG
                Debug.WriteLine(ex, TgConstants.LogTypeStorage);
                Debug.WriteLine(ex.StackTrace);
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
            {
                Debug.WriteLine(ex, TgConstants.LogTypeStorage);
                Debug.WriteLine(ex.StackTrace);
#else
			catch (Exception)
			{
#endif
                await transaction.RollbackAsync();
                throw;
            }
            return storageResult;
        }
    }

    public TgEfStorageResult<TEfEntity> Save(TEfEntity? item)
    {
        var task = SaveAsync(item);
        task.Wait();
        return task.Result;
    }

    public virtual async Task<bool> SaveListAsync(IEnumerable<TEfEntity> items, bool isFirstTry = true)
    {
        var transaction = await EfContext.Database.BeginTransactionAsync();
        await using (transaction)
        {
            var array = items as TEfEntity[] ?? [.. items];
            try
            {
                var uniqueItems = array.Distinct().ToArray();
                var storageItems = new List<TEfEntity>();
                foreach (var item in uniqueItems)
                {
                    // Load actual entity
                    var isExists = await CheckExistsAsync(item);
                    // Entity is not exists - Create
                    TEfEntity itemNew;
                    if (!isExists)
                    {
                        itemNew = item;
                    }
                    // Entity is existing - Update
                    else
                    {
                        itemNew = await GetItemAsync(item, isReadOnly: false);
                        itemNew.Copy(item, isUidCopy: false);
                    }
                    // Validate entity
                    var validationResult = TgGlobalTools.GetEfValid(itemNew);
                    if (!validationResult.IsValid)
                        throw new ValidationException(validationResult.Errors);
                    // Normilize entity
                    TgGlobalTools.Normilize(itemNew);
                    // Entity is not exists - Create
                    if (!isExists)
                    {
                        await EfContext.AddItemAsync(itemNew);
                    }
                    // Entity is existing - Update
                    else
                    {
                        EfContext.UpdateItem(itemNew);
                    }
                    storageItems.Add(itemNew);
                }
                await EfContext.SaveChangesAsync();
                foreach (var storageItem in storageItems)
                {
                    EfContext.DetachItem(storageItem);
                }
                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
#if DEBUG
                Debug.WriteLine(ex, TgConstants.LogTypeStorage);
                Debug.WriteLine(ex.StackTrace);
#endif
                // Retry
                if (isFirstTry)
                {
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync() ?? throw new Exception("The record you attempted to edit was deleted!");
                    entry.OriginalValues.SetValues(databaseValues);
                    return await SaveListAsync(array, isFirstTry: false);
                }
                throw;
            }
#if DEBUG
            catch (Exception ex)
            {
                Debug.WriteLine(ex, TgConstants.LogTypeStorage);
                Debug.WriteLine(ex.StackTrace);
#else
			catch (Exception)
			{
#endif
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public bool SaveList(List<TEfEntity> items, bool isFirstTry = true)
    {
        var task = SaveListAsync(items);
        task.Wait();
        return task.Result;
    }

    public virtual async Task<TgEfStorageResult<TEfEntity>> SaveWithoutTransactionAsync(TEfEntity item)
    {
        TgEfStorageResult<TEfEntity> storageResult;
        try
        {
            storageResult = await GetAsync(item, isReadOnly: false);
            // Create.
            if (!storageResult.IsExists)
            {
                TgGlobalTools.Normilize(storageResult.Item);
                await EfContext.AddItemAsync(storageResult.Item);
                await EfContext.SaveChangesAsync();
            }
            // Update.
            else
            {
                storageResult.Item.Copy(item, false);
                var validationResult = TgGlobalTools.GetEfValid(storageResult.Item);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);
                await EfContext.SaveChangesAsync();
            }
            storageResult.State = TgEnumEntityState.IsSaved;
        }
#if DEBUG
        catch (Exception ex)
        {
            Debug.WriteLine(ex, TgConstants.LogTypeStorage);
            Debug.WriteLine(ex.StackTrace);
#else
		catch (Exception)
		{
#endif
            throw;
        }
        return storageResult;
    }

    public TgEfStorageResult<TEfEntity> SaveWithoutTransaction(TEfEntity item)
    {
        var task = SaveWithoutTransactionAsync(item);
        task.Wait();
        return task.Result;
    }

    public virtual async Task<TgEfStorageResult<TEfEntity>> SaveOrRecreateAsync(TEfEntity item, string tableName)
    {
        try
        {
            return await SaveAsync(item);
        }
#if DEBUG
        catch (Exception ex)
        {
            Debug.WriteLine(ex, TgConstants.LogTypeStorage);
            Debug.WriteLine(ex.StackTrace);
#else
		catch (Exception)
		{
#endif
            var itemBackup = item;
            await DeleteAsync(item);
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

    public TgEfStorageResult<TEfEntity> SaveOrRecreate(TEfEntity item, string tableName)
    {
        var task = SaveOrRecreateAsync(item, tableName);
        task.Wait();
        return task.Result;
    }

    #endregion

    #region Public and private methods - Remove

    public virtual async Task<TgEfStorageResult<TEfEntity>> DeleteAsync(TEfEntity item)
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
            {
                Debug.WriteLine(ex, TgConstants.LogTypeStorage);
                Debug.WriteLine(ex.StackTrace);
#else
			catch (Exception)
			{
#endif
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public virtual async Task<TgEfStorageResult<TEfEntity>> DeleteNewAsync()
    {
        var storageResult = await GetNewAsync(isReadOnly: false);
        return storageResult.IsExists
            ? await DeleteAsync(storageResult.Item)
            : new(TgEnumEntityState.NotDeleted);
    }

    public virtual async Task<TgEfStorageResult<TEfEntity>> DeleteAllAsync()
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