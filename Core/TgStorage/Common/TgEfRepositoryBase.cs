// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using ValidationException = FluentValidation.ValidationException;

namespace TgStorage.Common;

public abstract class TgEfRepositoryBase<TEfEntity, TDto> : TgDisposable, ITgEfRepository<TEfEntity, TDto>, IDisposable
    where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
    where TDto : class, ITgDto<TEfEntity, TDto>, new()
{
    #region Fields, properties, constructor

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

    #region IDisposable

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        Scope.Dispose();
        EfContext.Dispose();
    }

    /// <summary> Release unmanaged resources </summary>
    public override void ReleaseUnmanagedResources()
    {
        //
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public virtual string ToDebugString() => typeof(TEfEntity) switch
    {
        var cls when cls == typeof(TgEfAppEntity) => $"{nameof(TgEfAppRepository)}",
        var cls when cls == typeof(TgEfUserEntity) => $"{nameof(TgEfUserRepository)}",
        var cls when cls == typeof(TgEfDocumentEntity) => $"{nameof(TgEfDocumentRepository)}",
        var cls when cls == typeof(TgEfFilterEntity) => $"{nameof(TgEfFilterRepository)}",
        var cls when cls == typeof(TgEfLicenseEntity) => $"{nameof(TgEfLicenseRepository)}",
        var cls when cls == typeof(TgEfMessageEntity) => $"{nameof(TgEfMessageRepository)}",
        var cls when cls == typeof(TgEfMessageRelationEntity) => $"{nameof(TgEfMessageRelationRepository)}",
        var cls when cls == typeof(TgEfProxyEntity) => $"{nameof(TgEfProxyRepository)}",
        var cls when cls == typeof(TgEfSourceEntity) => $"{nameof(TgEfSourceRepository)}",
        var cls when cls == typeof(TgEfStoryEntity) => $"{nameof(TgEfStoryRepository)}",
        var cls when cls == typeof(TgEfVersionEntity) => $"{nameof(TgEfVersionRepository)}",
        _ => throw new InvalidOperationException($"Unsupported entity type: {typeof(TEfEntity).Name}"),
    };

    /// <inheritdoc />
    public virtual IQueryable<TEfEntity> GetQuery(bool isReadOnly = true)
    {
        var db = EfContext.Database;

        var connection = db.GetDbConnection();
        switch (connection.State)
        {
            case ConnectionState.Closed:
                connection.Open();
                break;
            case ConnectionState.Open:
            case ConnectionState.Connecting:
            case ConnectionState.Executing:
            case ConnectionState.Fetching:
            case ConnectionState.Broken:
                // Connection is already open or in a valid state
                break;
        }

        // Explicitly cast the DbSet to IQueryable<TEfEntity> to resolve the type mismatch
        return typeof(TEfEntity) switch
        {
            var cls when cls == typeof(TgEfAppEntity) => isReadOnly
                ? (IQueryable<TEfEntity>)EfContext.Apps.AsNoTracking()
                : (IQueryable<TEfEntity>)EfContext.Apps,
            var cls when cls == typeof(TgEfUserEntity) => isReadOnly
                ? (IQueryable<TEfEntity>)EfContext.Users.AsNoTracking()
                : (IQueryable<TEfEntity>)EfContext.Users,
            var cls when cls == typeof(TgEfDocumentEntity) => isReadOnly
                ? (IQueryable<TEfEntity>)EfContext.Documents.AsNoTracking()
                : (IQueryable<TEfEntity>)EfContext.Documents,
            var cls when cls == typeof(TgEfFilterEntity) => isReadOnly
                ? (IQueryable<TEfEntity>)EfContext.Filters.AsNoTracking()
                : (IQueryable<TEfEntity>)EfContext.Filters,
            var cls when cls == typeof(TgEfLicenseEntity) => isReadOnly
                ? (IQueryable<TEfEntity>)EfContext.Licenses.AsNoTracking()
                : (IQueryable<TEfEntity>)EfContext.Licenses,
            var cls when cls == typeof(TgEfMessageEntity) => isReadOnly
                ? (IQueryable<TEfEntity>)EfContext.Messages.AsNoTracking()
                : (IQueryable<TEfEntity>)EfContext.Messages,
            var cls when cls == typeof(TgEfMessageRelationEntity) => isReadOnly
                ? (IQueryable<TEfEntity>)EfContext.MessagesRelations.AsNoTracking()
                : (IQueryable<TEfEntity>)EfContext.MessagesRelations,
            var cls when cls == typeof(TgEfProxyEntity) => isReadOnly
                ? (IQueryable<TEfEntity>)EfContext.Proxies.AsNoTracking()
                : (IQueryable<TEfEntity>)EfContext.Proxies,
            var cls when cls == typeof(TgEfSourceEntity) => isReadOnly
                ? (IQueryable<TEfEntity>)EfContext.Sources.AsNoTracking()
                : (IQueryable<TEfEntity>)EfContext.Sources,
            var cls when cls == typeof(TgEfStoryEntity) => isReadOnly
                ? (IQueryable<TEfEntity>)EfContext.Stories.AsNoTracking()
                : (IQueryable<TEfEntity>)EfContext.Stories,
            var cls when cls == typeof(TgEfVersionEntity) => isReadOnly
                ? (IQueryable<TEfEntity>)EfContext.Versions.AsNoTracking()
                : (IQueryable<TEfEntity>)EfContext.Versions,
            _ => throw new InvalidOperationException($"Unsupported entity type: {typeof(TEfEntity).Name}"),
        };
    }

    /// <inheritdoc />
    private static async Task<TgEfStorageResult<TEfEntity>> UseOverrideMethodAsync()
    {
        await Task.CompletedTask;
        throw new NotImplementedException(TgConstants.UseOverrideMethod);
    }

    #endregion

    #region Methods - Read

    /// <inheritdoc />
    public virtual async Task<bool> CheckExistsAsync(TEfEntity item)
    {
        var storageResult = await GetAsync(item, isReadOnly: true);
        return storageResult.IsExists;
    }

    /// <inheritdoc />
    public virtual async Task<bool> CheckExistsByDtoAsync(TDto dto)
    {
        var storageResult = await GetByDtoAsync(dto, isReadOnly: true);
        return storageResult.IsExists;
    }

    /// <inheritdoc />
    public virtual bool CheckExists(TEfEntity item)
    {
        var storageResult = Get(item, isReadOnly: true);
        return storageResult.IsExists;
    }

    /// <inheritdoc />
    public virtual bool CheckExistsByDto(TDto dto)
    {
        var storageResult = GetByDto(dto, isReadOnly: true);
        return storageResult.IsExists;
    }

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> GetAsync(TEfEntity item, bool isReadOnly = true) =>
        await UseOverrideMethodAsync();

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> GetByDtoAsync(TDto dto, bool isReadOnly = true) =>
        await UseOverrideMethodAsync();

    /// <inheritdoc />
    public virtual TgEfStorageResult<TEfEntity> Get(TEfEntity item, bool isReadOnly = true) =>
        throw new NotImplementedException(TgConstants.UseOverrideMethod);

    /// <inheritdoc />
    public virtual TgEfStorageResult<TEfEntity> GetByDto(TDto dto, bool isReadOnly = true) =>
        throw new NotImplementedException(TgConstants.UseOverrideMethod);

    /// <inheritdoc />
    public virtual async Task<TEfEntity> GetItemAsync(TEfEntity item, bool isReadOnly = true) =>
        (await GetAsync(item, isReadOnly)).Item ?? item;

    /// <inheritdoc />
    public async Task<TEfEntity> GetItemWhereAsync(Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true) =>
        await GetQuery(isReadOnly).FirstOrDefaultAsync(where) ?? new();

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> GetNewAsync(bool isReadOnly = true) => await GetAsync(new(), isReadOnly);

    /// <inheritdoc />
    public virtual async Task<TEfEntity> GetNewItemAsync(bool isReadOnly = true) => (await GetNewAsync(isReadOnly)).Item ?? new();

    /// <inheritdoc />
    public TgEfStorageResult<TEfEntity> GetNew(bool isReadOnly = true)
    {
        var task = GetNewAsync(isReadOnly);
        task.Wait();
        return task.Result;
    }

    /// <inheritdoc />
    public TEfEntity GetNewItem(bool isReadOnly = true)
    {
        var task = GetNewItemAsync(isReadOnly);
        task.Wait();
        return task.Result;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IEnumerable<TEfEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true)
    {
        var task = GetListItemsAsync(topRecords, skip, isReadOnly);
        task.Wait();
        return task.Result;
    }

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> GetListAsync(int take, int skip, bool isReadOnly = true) =>
        await UseOverrideMethodAsync();

    /// <inheritdoc />
    public TgEfStorageResult<TEfEntity> GetList(int take, int skip, bool isReadOnly = true)
    {
        var task = GetListAsync(take, skip, isReadOnly);
        task.Wait();
        return task.Result;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public TgEfStorageResult<TEfEntity> GetList(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true)
    {
        var task = GetListAsync(topRecords, skip, where, isReadOnly);
        task.Wait();
        return task.Result;
    }

    /// <inheritdoc />
    public IEnumerable<TEfEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true)
    {
        var task = GetListItemsAsync(topRecords, skip, where, isReadOnly);
        task.Wait();
        return task.Result;
    }

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> GetListAsync(int take, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true) =>
        await UseOverrideMethodAsync();

    /// <inheritdoc />
    public TgEfStorageResult<TEfEntity> GetList(int take, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true)
    {
        var task = GetListAsync(take, skip, where, isReadOnly);
        task.Wait();
        return task.Result;
    }

    /// <inheritdoc />
    public virtual async Task<int> GetCountAsync() => await Task.FromResult(0);

    /// <inheritdoc />
    public int GetCount()
    {
        var task = GetCountAsync();
        task.Wait();
        return task.Result;
    }

    /// <inheritdoc />
    public virtual async Task<int> GetCountAsync(Expression<Func<TEfEntity, bool>> where) => await Task.FromResult(0);

    /// <inheritdoc />
    public int GetCount(Expression<Func<TEfEntity, bool>> where)
    {
        var task = GetCountAsync(where);
        task.Wait();
        return task.Result;
    }

    #endregion

    #region Methods - Read DTO

    /// <inheritdoc />
    public Expression<Func<TEfEntity, TDto>> SelectDto() => item => new TDto().GetNewDto(item);

    /// <inheritdoc />
    public async Task<TDto> GetDtoAsync(Expression<Func<TEfEntity, bool>> where)
    {
        var dto = await GetQuery().Where(where).Select(SelectDto()).SingleOrDefaultAsync() ?? new TDto();
        return dto;
    }

    /// <inheritdoc />
    public async Task<List<TDto>> GetListDtosAsync(int take = 0, int skip = 0)
    {
        var dtos = take > 0
            ? await GetQuery(isReadOnly: true).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
            : await GetQuery(isReadOnly: true).Select(SelectDto()).ToListAsync();
        return dtos;
    }

    /// <inheritdoc />
    public async Task<List<TDto>> GetListDtosAsync(int take, int skip, Expression<Func<TEfEntity, bool>> where)
    {
        var dtos = take > 0
            ? await GetQuery(isReadOnly: true).Where(where).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
            : await GetQuery(isReadOnly: true).Where(where).Select(SelectDto()).ToListAsync();
        return dtos;
    }

    /// <inheritdoc />
    public async Task<List<TDto>> GetListDtosAsync<TKey>(int take, int skip, Expression<Func<TEfEntity, bool>> where, Expression<Func<TEfEntity, TKey>> order)
    {
        var dtos = take > 0
            ? await GetQuery(isReadOnly: true).Where(where).OrderBy(order).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
            : await GetQuery(isReadOnly: true).Where(where).OrderBy(order).Select(SelectDto()).ToListAsync();
        return dtos;
    }

    /// <inheritdoc />
    public async Task<List<TDto>> GetListDtosDescAsync<TKey>(int take, int skip, Expression<Func<TEfEntity, bool>> where, Expression<Func<TEfEntity, TKey>> order)
    {
        var dtos = take > 0
            ? await GetQuery(isReadOnly: true).Where(where).OrderByDescending(order).Skip(skip).Take(take).Select(SelectDto()).ToListAsync()
            : await GetQuery(isReadOnly: true).Where(where).OrderByDescending(order).Select(SelectDto()).ToListAsync();
        return dtos;
    }

    /// <inheritdoc />
    public async Task<int> GetListCountAsync()
    {
        return await GetQuery(isReadOnly: true).CountAsync();
    }

    /// <inheritdoc />
    public async Task<int> GetListCountAsync(Expression<Func<TEfEntity, bool>> where)
    {
        return await GetQuery(isReadOnly: true).Where(where).CountAsync();
    }

    #endregion

    #region Methods - Write

    /// <inheritdoc />
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
                if (!storageResult.IsExists || storageResult.Item is null)
                {
                    storageResult.Item = item;
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
                // Normalize entity
                TgGlobalTools.Normalize(storageResult.Item);
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

    /// <inheritdoc />
    public TgEfStorageResult<TEfEntity> Save(TEfEntity? item)
    {
        var task = SaveAsync(item);
        task.Wait();
        return task.Result;
    }

    /// <inheritdoc />
    public virtual async Task<bool> SaveListAsync(IEnumerable<TEfEntity> items, bool isRewriteEntities, bool isFirstTry = true)
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
                    TEfEntity? itemNew = null;
                    if (!isExists)
                    {
                        itemNew = item;
                    }
                    // Entity is existing - Update
                    else if (isRewriteEntities)
                    {
                        itemNew = await GetItemAsync(item, isReadOnly: false);
                        itemNew.Copy(item, isUidCopy: false);
                    }
                    if (itemNew is null) continue;

                    // Validate entity
                    var validationResult = TgGlobalTools.GetEfValid(itemNew);
                    if (!validationResult.IsValid)
                        throw new ValidationException(validationResult.Errors);
                    // Normalize entity
                    TgGlobalTools.Normalize(itemNew);
                    // Entity is not exists - Create
                    if (!isExists)
                    {
                        await EfContext.AddItemAsync(itemNew);
                    }
                    // Entity is existing - Update
                    else if (isRewriteEntities)
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
                    return await SaveListAsync(array, isRewriteEntities, isFirstTry: false);
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

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> SaveWithoutTransactionAsync(TEfEntity item)
    {
        TgEfStorageResult<TEfEntity> storageResult;
        try
        {
            storageResult = await GetAsync(item, isReadOnly: false);
            // Create
            if (!storageResult.IsExists || storageResult.Item is null)
            {
                TgGlobalTools.Normalize(item);
                await EfContext.AddItemAsync(item);
                await EfContext.SaveChangesAsync();
                storageResult.Item = item;
            }
            // Update
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

    /// <inheritdoc />
    public TgEfStorageResult<TEfEntity> SaveWithoutTransaction(TEfEntity item)
    {
        var task = SaveWithoutTransactionAsync(item);
        task.Wait();
        return task.Result;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public TgEfStorageResult<TEfEntity> SaveOrRecreate(TEfEntity item, string tableName)
    {
        var task = SaveOrRecreateAsync(item, tableName);
        task.Wait();
        return task.Result;
    }

    #endregion

    #region Methods - Remove

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> DeleteAsync(TEfEntity item)
    {
        var transaction = await EfContext.Database.BeginTransactionAsync();
        await using (transaction)
        {
            try
            {
                var storageResult = await GetAsync(item, isReadOnly: false);
                if (!storageResult.IsExists || storageResult.Item is null)
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

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> DeleteNewAsync()
    {
        var storageResult = await GetNewAsync(isReadOnly: false);
        return storageResult.IsExists && storageResult.Item is not null
            ? await DeleteAsync(storageResult.Item)
            : new(TgEnumEntityState.NotDeleted);
    }

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> DeleteAllAsync()
    {
        // Determine table name based on entity type
        var tableName = typeof(TEfEntity) switch
        {
            var cls when cls == typeof(TgEfAppEntity) => TgEfConstants.TableApps,
            var cls when cls == typeof(TgEfUserEntity) => TgEfConstants.TableUsers,
            var cls when cls == typeof(TgEfDocumentEntity) => TgEfConstants.TableDocuments,
            var cls when cls == typeof(TgEfFilterEntity) => TgEfConstants.TableFilters,
            var cls when cls == typeof(TgEfLicenseEntity) => TgEfConstants.TableLicenses,
            var cls when cls == typeof(TgEfMessageEntity) => TgEfConstants.TableMessages,
            var cls when cls == typeof(TgEfMessageRelationEntity) => TgEfConstants.TableMessagesRelations,
            var cls when cls == typeof(TgEfProxyEntity) => TgEfConstants.TableProxies,
            var cls when cls == typeof(TgEfSourceEntity) => TgEfConstants.TableSources,
            var cls when cls == typeof(TgEfStoryEntity) => TgEfConstants.TableStories,
            var cls when cls == typeof(TgEfVersionEntity) => TgEfConstants.TableVersions,
            _ => throw new InvalidOperationException($"Unsupported entity type: {typeof(TEfEntity).Name}")
        };

        // Return early if table name is invalid
        if (string.IsNullOrWhiteSpace(tableName))
            return new(TgEnumEntityState.NotDeleted);

        // Use safe SQL execution to avoid injection
        var sql = FormattableStringFactory.Create($"DELETE FROM [{tableName}]");
        var result = await EfContext.Database.ExecuteSqlAsync(sql);

        // Return result based on affected rows
        return result < 0 ? new(TgEnumEntityState.NotDeleted) : new(TgEnumEntityState.IsDeleted);
    }

    #endregion
}
