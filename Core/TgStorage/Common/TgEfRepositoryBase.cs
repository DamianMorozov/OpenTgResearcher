namespace TgStorage.Common;

public abstract class TgEfRepositoryBase<TEfEntity, TDto> : TgDisposable, ITgEfRepository<TEfEntity, TDto>, IDisposable
    where TEfEntity : class, ITgEfEntity, new()
    where TDto : class, ITgDto, new()
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
        var cls when cls == typeof(TgEfChatUserEntity) => $"{nameof(TgEfChatUserRepository)}",
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
            var cls when cls == typeof(TgEfChatUserEntity) => isReadOnly
                ? (IQueryable<TEfEntity>)EfContext.ChatUsers.AsNoTracking()
                : (IQueryable<TEfEntity>)EfContext.ChatUsers,
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

    /// <inheritdoc />
    private static TgEfStorageResult<TEfEntity> UseOverrideMethod()
    {
        throw new NotImplementedException(TgConstants.UseOverrideMethod);
    }

    #endregion

    #region Methods - Read

    /// <inheritdoc />
    public virtual async Task<bool> CheckExistsAsync(TEfEntity item, CancellationToken ct = default)
    {
        var storageResult = await GetAsync(item, isReadOnly: true, ct);
        return storageResult.IsExists;
    }

    /// <inheritdoc />
    public virtual async Task<bool> CheckExistsByDtoAsync(TDto dto, CancellationToken ct = default)
    {
        var storageResult = await GetByDtoAsync(dto, isReadOnly: true, ct);
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
    public virtual async Task<TgEfStorageResult<TEfEntity>> GetAsync(TEfEntity item, bool isReadOnly = true, CancellationToken ct = default) =>
        await UseOverrideMethodAsync();

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> GetByDtoAsync(TDto dto, bool isReadOnly = true, CancellationToken ct = default) =>
        await UseOverrideMethodAsync();

    /// <inheritdoc />
    public virtual TgEfStorageResult<TEfEntity> Get(TEfEntity item, bool isReadOnly = true) =>
        throw new NotImplementedException(TgConstants.UseOverrideMethod);

    /// <inheritdoc />
    public virtual TgEfStorageResult<TEfEntity> GetByDto(TDto dto, bool isReadOnly = true) =>
        throw new NotImplementedException(TgConstants.UseOverrideMethod);

    /// <inheritdoc />
    public virtual async Task<TEfEntity> GetItemAsync(TEfEntity item, bool isReadOnly = true, CancellationToken ct = default) =>
        (await GetAsync(item, isReadOnly, ct)).Item ?? item;

    /// <inheritdoc />
    public async Task<TEfEntity> GetItemWhereAsync(Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true, CancellationToken ct = default) =>
        await GetQuery(isReadOnly).FirstOrDefaultAsync(where, ct) ?? new();

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> GetNewAsync(bool isReadOnly = true, CancellationToken ct = default) => 
        await GetAsync(new(), isReadOnly, ct);

    /// <inheritdoc />
    public virtual async Task<TEfEntity> GetNewItemAsync(bool isReadOnly = true, CancellationToken ct = default) => 
        (await GetNewAsync(isReadOnly, ct)).Item ?? new();

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
    public virtual async Task<TgEfStorageResult<TEfEntity>> GetListAsync(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true, CancellationToken ct = default) =>
        topRecords switch
        {
            TgEnumTableTopRecords.Top1 => await GetListAsync(1, skip, isReadOnly, ct),
            TgEnumTableTopRecords.Top20 => await GetListAsync(20, skip, isReadOnly, ct),
            TgEnumTableTopRecords.Top100 => await GetListAsync(200, skip, isReadOnly, ct),
            TgEnumTableTopRecords.Top1000 => await GetListAsync(1_000, skip, isReadOnly, ct),
            TgEnumTableTopRecords.Top10000 => await GetListAsync(10_000, skip, isReadOnly, ct),
            TgEnumTableTopRecords.Top100000 => await GetListAsync(100_000, skip, isReadOnly, ct),
            TgEnumTableTopRecords.Top1000000 => await GetListAsync(1_000_000, skip, isReadOnly, ct),
            _ => await GetListAsync(0, skip, isReadOnly, ct),
        };

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEfEntity>> GetListItemsAsync(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true, CancellationToken ct = default) =>
        topRecords switch
        {
            TgEnumTableTopRecords.Top1 => (await GetListAsync(1, skip, isReadOnly, ct)).Items,
            TgEnumTableTopRecords.Top20 => (await GetListAsync(20, skip, isReadOnly, ct)).Items,
            TgEnumTableTopRecords.Top100 => (await GetListAsync(200, skip, isReadOnly, ct)).Items,
            TgEnumTableTopRecords.Top1000 => (await GetListAsync(1_000, skip, isReadOnly, ct)).Items,
            TgEnumTableTopRecords.Top10000 => (await GetListAsync(10_000, skip, isReadOnly, ct)).Items,
            TgEnumTableTopRecords.Top100000 => (await GetListAsync(100_000, skip, isReadOnly, ct)).Items,
            TgEnumTableTopRecords.Top1000000 => (await GetListAsync(1_000_000, skip, isReadOnly, ct)).Items,
            _ => (await GetListAsync(0, skip, isReadOnly, ct)).Items,
        };

    /// <inheritdoc />
    public IEnumerable<TEfEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, bool isReadOnly = true) =>
        topRecords switch
        {
            TgEnumTableTopRecords.Top1 => GetList(1, skip, isReadOnly).Items,
            TgEnumTableTopRecords.Top20 => GetList(20, skip, isReadOnly).Items,
            TgEnumTableTopRecords.Top100 => GetList(200, skip, isReadOnly).Items,
            TgEnumTableTopRecords.Top1000 => GetList(1_000, skip, isReadOnly).Items,
            TgEnumTableTopRecords.Top10000 => GetList(10_000, skip, isReadOnly).Items,
            TgEnumTableTopRecords.Top100000 => GetList(100_000, skip, isReadOnly).Items,
            TgEnumTableTopRecords.Top1000000 => GetList(1_000_000, skip, isReadOnly).Items,
            _ => GetList(0, skip, isReadOnly).Items,
        };
    
    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> GetListAsync(int take, int skip, bool isReadOnly = true, CancellationToken ct = default) =>
        await UseOverrideMethodAsync();

    /// <inheritdoc />
    public TgEfStorageResult<TEfEntity> GetList(int take, int skip, bool isReadOnly = true) =>
        UseOverrideMethod();

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> GetListAsync(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where,
        bool isReadOnly = true, CancellationToken ct = default) =>
        topRecords switch
        {
            TgEnumTableTopRecords.Top1 => await GetListAsync(take: 1, skip, where, isReadOnly, ct),
            TgEnumTableTopRecords.Top20 => await GetListAsync(take: 20, skip, where, isReadOnly, ct),
            TgEnumTableTopRecords.Top100 => await GetListAsync(take: 200, skip, where, isReadOnly, ct),
            TgEnumTableTopRecords.Top1000 => await GetListAsync(take: 1_000, skip, where, isReadOnly, ct),
            TgEnumTableTopRecords.Top10000 => await GetListAsync(take: 10_000, skip, where, isReadOnly, ct),
            TgEnumTableTopRecords.Top100000 => await GetListAsync(take: 100_000, skip, where, isReadOnly, ct),
            TgEnumTableTopRecords.Top1000000 => await GetListAsync(take: 1_000_000, skip, where, isReadOnly, ct),
            _ => await GetListAsync(take: 0, skip, where, isReadOnly, ct),
        };

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEfEntity>> GetListItemsAsync(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where,
        bool isReadOnly = true, CancellationToken ct = default) =>
        topRecords switch
        {
            TgEnumTableTopRecords.Top1 => (await GetListAsync(take: 1, skip, where, isReadOnly, ct)).Items,
            TgEnumTableTopRecords.Top20 => (await GetListAsync(take: 20, skip, where, isReadOnly, ct)).Items,
            TgEnumTableTopRecords.Top100 => (await GetListAsync(take: 200, skip, where, isReadOnly, ct)).Items,
            TgEnumTableTopRecords.Top1000 => (await GetListAsync(take: 1_000, skip, where, isReadOnly, ct)).Items,
            TgEnumTableTopRecords.Top10000 => (await GetListAsync(take: 10_000, skip, where, isReadOnly, ct)).Items,
            TgEnumTableTopRecords.Top100000 => (await GetListAsync(take: 100_000, skip, where, isReadOnly, ct)).Items,
            TgEnumTableTopRecords.Top1000000 => (await GetListAsync(take: 1_000_000, skip, where, isReadOnly, ct)).Items,
            _ => (await GetListAsync(take: 0, skip, where, isReadOnly, ct)).Items,
        };

    /// <inheritdoc />
    public TgEfStorageResult<TEfEntity> GetList(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true) =>
        topRecords switch
        {
            TgEnumTableTopRecords.Top1 => GetList(take: 1, skip, where, isReadOnly),
            TgEnumTableTopRecords.Top20 => GetList(take: 20, skip, where, isReadOnly),
            TgEnumTableTopRecords.Top100 => GetList(take: 200, skip, where, isReadOnly),
            TgEnumTableTopRecords.Top1000 => GetList(take: 1_000, skip, where, isReadOnly),
            TgEnumTableTopRecords.Top10000 => GetList(take: 10_000, skip, where, isReadOnly),
            TgEnumTableTopRecords.Top100000 => GetList(take: 100_000, skip, where, isReadOnly),
            TgEnumTableTopRecords.Top1000000 => GetList(take: 1_000_000, skip, where, isReadOnly),
            _ => GetList(take: 0, skip, where, isReadOnly),
        };

    /// <inheritdoc />
    public IEnumerable<TEfEntity> GetListItems(TgEnumTableTopRecords topRecords, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true) =>
        topRecords switch
        {
            TgEnumTableTopRecords.Top1 => GetList(take: 1, skip, where, isReadOnly).Items,
            TgEnumTableTopRecords.Top20 => GetList(take: 20, skip, where, isReadOnly).Items,
            TgEnumTableTopRecords.Top100 => GetList(take: 200, skip, where, isReadOnly).Items,
            TgEnumTableTopRecords.Top1000 => GetList(take: 1_000, skip, where, isReadOnly).Items,
            TgEnumTableTopRecords.Top10000 => GetList(take: 10_000, skip, where, isReadOnly).Items,
            TgEnumTableTopRecords.Top100000 => GetList(take: 100_000, skip, where, isReadOnly).Items,
            TgEnumTableTopRecords.Top1000000 => GetList(take: 1_000_000, skip, where, isReadOnly).Items,
            _ => GetList(take: 0, skip, where, isReadOnly).Items,
        };

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> GetListAsync(int take, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true, CancellationToken ct = default) =>
        await UseOverrideMethodAsync();

    /// <inheritdoc />
    public TgEfStorageResult<TEfEntity> GetList(int take, int skip, Expression<Func<TEfEntity, bool>> where, bool isReadOnly = true) =>
        UseOverrideMethod();

    /// <inheritdoc />
    public virtual async Task<int> GetCountAsync(CancellationToken ct = default) => await Task.FromResult(0);

    /// <inheritdoc />
    public virtual int GetCount() => 0;

    /// <inheritdoc />
    public virtual async Task<int> GetCountAsync(Expression<Func<TEfEntity, bool>> where, CancellationToken ct = default) => await Task.FromResult(0);

    /// <inheritdoc />
    public int GetCount(Expression<Func<TEfEntity, bool>> where) => 0;

    #endregion

    #region Methods - Read DTO

    /// <inheritdoc />
    public Expression<Func<TEfEntity, TDto>> SelectDto() => item => (TDto)TgEfDomainUtils.CreateNewDto(item, true);

    /// <inheritdoc />
    public async Task<TDto> GetDtoAsync(Expression<Func<TEfEntity, bool>> where, CancellationToken ct = default)
    {
        var dto = await GetQuery().Where(where).Select(SelectDto()).SingleOrDefaultAsync(ct) ?? new TDto();
        return dto;
    }

    /// <inheritdoc />
    public async Task<int> DeleteDtoAsync(Expression<Func<TEfEntity, bool>> where, CancellationToken ct = default)
    {
        int result = await GetQuery().Where(where).ExecuteDeleteAsync(ct);
        return result;
    }

    /// <inheritdoc />
    public async Task<List<TDto>> GetListDtosAsync(int take = 0, int skip = 0, CancellationToken ct = default)
    {
        var dtos = take > 0
            ? await GetQuery(isReadOnly: true).Skip(skip).Take(take).Select(SelectDto()).ToListAsync(ct)
            : await GetQuery(isReadOnly: true).Select(SelectDto()).ToListAsync(ct);
        return dtos;
    }

    /// <inheritdoc />
    public async Task<List<TDto>> GetListDtosAsync(int take, int skip, Expression<Func<TEfEntity, bool>> where, CancellationToken ct = default)
    {
        var dtos = take > 0
            ? await GetQuery(isReadOnly: true).Where(where).Skip(skip).Take(take).Select(SelectDto()).ToListAsync(ct)
            : await GetQuery(isReadOnly: true).Where(where).Select(SelectDto()).ToListAsync(ct);
        return dtos;
    }

    /// <inheritdoc />
    public async Task<List<TDto>> GetListDtosAsync<TKey>(int take, int skip, Expression<Func<TEfEntity, bool>> where, Expression<Func<TEfEntity, TKey>> order, 
        CancellationToken ct = default)
    {
        var dtos = take > 0
            ? await GetQuery(isReadOnly: true).Where(where).OrderBy(order).Skip(skip).Take(take).Select(SelectDto()).ToListAsync(ct)
            : await GetQuery(isReadOnly: true).Where(where).OrderBy(order).Select(SelectDto()).ToListAsync(ct);
        return dtos;
    }

    /// <inheritdoc />
    public async Task<List<TDto>> GetListDtosDescAsync<TKey>(int take, int skip, Expression<Func<TEfEntity, bool>> where, Expression<Func<TEfEntity, TKey>> order, 
        CancellationToken ct = default)
    {
        var dtos = take > 0
            ? await GetQuery(isReadOnly: true).Where(where).OrderByDescending(order).Skip(skip).Take(take).Select(SelectDto()).ToListAsync(ct)
            : await GetQuery(isReadOnly: true).Where(where).OrderByDescending(order).Select(SelectDto()).ToListAsync(ct);
        return dtos;
    }

    /// <inheritdoc />
    public async Task<int> GetListCountAsync(CancellationToken ct = default) => await GetQuery(isReadOnly: true).CountAsync(ct);

    /// <inheritdoc />
    public async Task<int> GetListCountAsync(Expression<Func<TEfEntity, bool>> where, CancellationToken ct = default)
    {
        return await GetQuery(isReadOnly: true).Where(where).CountAsync(ct);
    }

    /// <inheritdoc />
    public async Task<TDto?> GetFirstOrDefaultAsync<TKey>(Expression<Func<TEfEntity, bool>> where, Expression<Func<TEfEntity, TKey>> order, CancellationToken ct = default)
    {
        var dto = await GetQuery(isReadOnly: true).Where(where).OrderBy(order).Select(SelectDto()).FirstOrDefaultAsync(ct);
        return dto;
    }
    
    /// <inheritdoc />
    public async Task<TDto?> GetFirstOrDefaultDescAsync<TKey>(Expression<Func<TEfEntity, bool>> where, Expression<Func<TEfEntity, TKey>> order, CancellationToken ct = default)
    {
        var dto = await GetQuery(isReadOnly: true).Where(where).OrderByDescending(order).Select(SelectDto()).FirstOrDefaultAsync(ct);
        return dto;
    }
    
    #endregion

    #region Methods - Write

    /// <summary> Prepares entity for saving: loads existing, copies data, validates and normalizes </summary>
    private async Task<TgEfStorageResult<TEfEntity>> PrepareEntityForSaveAsync(TEfEntity item, bool isRewrite, CancellationToken ct)
    {
        // Load actual entity
        var storageResult = await GetAsync(item, isReadOnly: false, ct);

        // Entity is not exists - Create
        if (!storageResult.IsExists || storageResult.Item is null)
        {
            storageResult.Item = item;
            await EfContext.AddItemAsync(storageResult.Item, ct);
        }
        // Entity is existing - Update
        else if (isRewrite)
        {
            TgEfDomainUtils.UpdateEntity(storageResult.Item, item, isUidCopy: false);
            // Simple property assignment
            //((DbContext)EfContext).Entry(storageResult.Item!).CurrentValues.SetValues(item);
            EfContext.UpdateItem(storageResult.Item);
        }
        else
        {
            return storageResult;
        }

        ValidateAndNormalize(storageResult.Item);
        return storageResult;
    }

    /// <summary> Validates and normalizes entity before saving </summary>
    protected void ValidateAndNormalize(TEfEntity entity)
    {
        var validationResult = TgGlobalTools.GetEfValid(entity);
        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);

        TgGlobalTools.Normalize(entity);
    }

    #endregion

    #region Methods - Remove

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> DeleteAsync(TEfEntity item, CancellationToken ct = default)
    {
        var transaction = await EfContext.Database.BeginTransactionAsync(ct);
        await using (transaction)
        {
            try
            {
                var storageResult = await GetAsync(item, isReadOnly: false, ct);
                if (!storageResult.IsExists || storageResult.Item is null)
                    return storageResult;
                EfContext.RemoveItem(storageResult.Item);
                await EfContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
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
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> DeleteAsync(Expression<Func<TEfEntity, bool>> where, CancellationToken ct = default)
    {
        var transaction = await EfContext.Database.BeginTransactionAsync(ct);
        await using (transaction)
        {
            try
            {
                var result = await DeleteDtoAsync(where, ct);
                await EfContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return result > 0 ? new(TgEnumEntityState.IsDeleted) : new(TgEnumEntityState.NotDeleted);
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
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> DeleteNewAsync(CancellationToken ct = default)
    {
        var storageResult = await GetNewAsync(isReadOnly: false, ct);
        
        return storageResult.IsExists && storageResult.Item is not null
            ? await DeleteAsync(storageResult.Item, ct)
            : new(TgEnumEntityState.NotDeleted);
    }

    /// <inheritdoc />
    public virtual async Task<TgEfStorageResult<TEfEntity>> DeleteAllAsync(CancellationToken ct = default)
    {
        // Determine table name based on entity type
        var tableName = typeof(TEfEntity) switch
        {
            var cls when cls == typeof(TgEfAppEntity) => TgEfConstants.TableApps,
            var cls when cls == typeof(TgEfUserEntity) => TgEfConstants.TableUsers,
            var cls when cls == typeof(TgEfChatUserEntity) => TgEfConstants.TableChatUsers,
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
        var result = await EfContext.Database.ExecuteSqlAsync(sql, ct);

        // Return result based on affected rows
        return result < 0 ? new(TgEnumEntityState.NotDeleted) : new(TgEnumEntityState.IsDeleted);
    }

    #endregion
}
