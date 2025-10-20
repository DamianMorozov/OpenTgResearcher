namespace TgStorage.Repositories;

/// <summary> EF version repository </summary>
public sealed class TgEfVersionRepository : TgEfRepositoryBase<TgEfVersionEntity, TgEfVersionDto>, ITgEfVersionRepository
{
    #region Fields, properties, constructor

    public TgEfVersionRepository() : base() { }

    public TgEfVersionRepository(IWebHostEnvironment webHostEnvironment) : base(webHostEnvironment) { }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfVersionEntity>> GetAsync(TgEfVersionEntity item, bool isReadOnly = true, CancellationToken ct = default)
    {
        // Find by Uid
        var itemFind = await GetQuery(isReadOnly)
            .Where(x => x.Uid == item.Uid)
            .FirstOrDefaultAsync(ct);
        
        if (itemFind is not null)
            return new(TgEnumEntityState.IsExists, itemFind);
        
        // Find by Version
        itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.Version == item.Version, ct);

        return itemFind is not null
            ? new(TgEnumEntityState.IsExists, itemFind)
            : new TgEfStorageResult<TgEfVersionEntity>(TgEnumEntityState.NotExists);
    }

    /// <inheritdoc />
    public override async Task<TgEfStorageResult<TgEfVersionEntity>> GetByDtoAsync(TgEfVersionDto dto, bool isReadOnly = true, CancellationToken ct = default)
    {
        // Find by Uid
        var itemFind = await GetQuery(isReadOnly)
            .Where(x => x.Uid == dto.Uid)
            .FirstOrDefaultAsync(ct);
        
        if (itemFind is not null)
            return new(TgEnumEntityState.IsExists, itemFind);
        
        // Find by Version
        itemFind = await GetQuery(isReadOnly).SingleOrDefaultAsync(x => x.Version == dto.Version, ct);

        return itemFind is not null
            ? new(TgEnumEntityState.IsExists, itemFind)
            : new TgEfStorageResult<TgEfVersionEntity>(TgEnumEntityState.NotExists);
    }

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfVersionEntity> Get(TgEfVersionEntity item, bool isReadOnly = true)
    {
        // Find by Uid
        var itemFind = GetQuery(isReadOnly)
            .Where(x => x.Uid == item.Uid)
            .FirstOrDefault();
        
        if (itemFind is not null)
            return new(TgEnumEntityState.IsExists, itemFind);
        
        // Find by Version
        itemFind = GetQuery(isReadOnly).SingleOrDefault(x => x.Version == item.Version);

        return itemFind is not null
            ? new(TgEnumEntityState.IsExists, itemFind)
            : new TgEfStorageResult<TgEfVersionEntity>(TgEnumEntityState.NotExists);
    }

    /// <inheritdoc />
    public override TgEfStorageResult<TgEfVersionEntity> GetByDto(TgEfVersionDto dto, bool isReadOnly = true)
    {
        // Find by Uid
        var itemFind = GetQuery(isReadOnly)
            .Where(x => x.Uid == dto.Uid)
            .FirstOrDefault();
        if (itemFind is not null)
            return new(TgEnumEntityState.IsExists, itemFind);
        // Find by Version
        itemFind = GetQuery(isReadOnly).SingleOrDefault(x => x.Version == dto.Version);

        return itemFind is not null
            ? new(TgEnumEntityState.IsExists, itemFind)
            : new TgEfStorageResult<TgEfVersionEntity>(TgEnumEntityState.NotExists);
    }

    /// <inheritdoc />
	public override async Task<TgEfStorageResult<TgEfVersionEntity>> GetListAsync(int take, int skip, bool isReadOnly = true, CancellationToken ct = default)
    {
        IList<TgEfVersionEntity> items = take > 0
            ? await GetQuery(isReadOnly).Skip(skip).Take(take).ToListAsync(ct)
            : await GetQuery(isReadOnly).ToListAsync(ct);
        return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
    }

    /// <inheritdoc />
	public override async Task<TgEfStorageResult<TgEfVersionEntity>> GetListAsync(int take, int skip, Expression<Func<TgEfVersionEntity, bool>> where, 
        bool isReadOnly = true, CancellationToken ct = default)
    {
        IList<TgEfVersionEntity> items = take > 0
            ? await GetQuery(isReadOnly).Where(where).Skip(skip).Take(take).ToListAsync(ct)
            : await GetQuery(isReadOnly).Where(where).ToListAsync(ct);

        return new(items.Any() ? TgEnumEntityState.IsExists : TgEnumEntityState.NotExists, items);
    }

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(CancellationToken ct = default) => await EfContext.Versions.AsNoTracking().CountAsync(ct);

    /// <inheritdoc />
    public override async Task<int> GetCountAsync(Expression<Func<TgEfVersionEntity, bool>> where, CancellationToken ct = default) => 
        await EfContext.Versions.AsNoTracking().Where(where).CountAsync(ct);

    #endregion

    #region Methods - ITgEfVersionRepository

    /// <inheritdoc />
    public short LastVersion => 49;

    /// <inheritdoc />
    public async Task<TgEfVersionEntity> GetLastVersionAsync()
    {
        TgEfVersionEntity versionLast = new();
        var defaultVersion = new TgEfVersionEntity().Version;
        var versions = (await GetListAsync(TgEnumTableTopRecords.All, 0)).Items
            .Where(x => x.Version != defaultVersion).OrderBy(x => x.Version).ToList();
        if (versions.Count != 0)
            versionLast = versions[^1];
        return versionLast;
    }

    /// <inheritdoc />
    public async Task FillTableVersionsAsync()
    {
        await DeleteNewAsync();
        var isLast = false;
        while (!isLast)
        {
            var versionLast = await GetLastVersionAsync();
            if (Equals(versionLast.Version, new TgEfVersionEntity().Version))
                versionLast.Version = 0;
            switch (versionLast.Version)
            {
                case 0:
                    await SaveAsync(new TgEfVersionDto() { Version = 1, Description = "Added versions table" });
                    break;
                case 1:
                    await SaveAsync(new TgEfVersionDto() { Version = 2, Description = "Added apps table" });
                    break;
                case 2:
                    await SaveAsync(new TgEfVersionDto() { Version = 3, Description = "Added documents table" });
                    break;
                case 3:
                    await SaveAsync(new TgEfVersionDto() { Version = 4, Description = "Added filters table" });
                    break;
                case 4:
                    await SaveAsync(new TgEfVersionDto() { Version = 5, Description = "Added messages table" });
                    break;
                case 5:
                    await SaveAsync(new TgEfVersionDto() { Version = 6, Description = "Added proxies table" });
                    break;
                case 6:
                    await SaveAsync(new TgEfVersionDto() { Version = 7, Description = "Added sources table" });
                    break;
                case 7:
                    await SaveAsync(new TgEfVersionDto() { Version = 8, Description = "Updated sources table" });
                    break;
                case 8:
                    await SaveAsync(new TgEfVersionDto() { Version = 9, Description = "Updated versions table" });
                    break;
                case 9:
                    await SaveAsync(new TgEfVersionDto() { Version = 10, Description = "Updated apps table" });
                    break;
                case 10:
                    await SaveAsync(new TgEfVersionDto() { Version = 11, Description = "Upgraded storage to XPO framework" });
                    break;
                case 11:
                    await SaveAsync(new TgEfVersionDto() { Version = 12, Description = "Updated apps table" });
                    break;
                case 12:
                    await SaveAsync(new TgEfVersionDto() { Version = 13, Description = "Updated documents table" });
                    break;
                case 13:
                    await SaveAsync(new TgEfVersionDto() { Version = 14, Description = "Updated filters table" });
                    break;
                case 14:
                    await SaveAsync(new TgEfVersionDto() { Version = 15, Description = "Updated messages table" });
                    break;
                case 15:
                    await SaveAsync(new TgEfVersionDto() { Version = 16, Description = "Updated proxies table" });
                    break;
                case 16:
                    await SaveAsync(new TgEfVersionDto() { Version = 17, Description = "Updated sources table" });
                    break;
                case 17:
                    await SaveAsync(new TgEfVersionDto() { Version = 18, Description = "Updated the UID field in the apps table" });
                    break;
                case 18:
                    await SaveAsync(new TgEfVersionDto() { Version = 19, Description = "Updated the UID field in the documents table" });
                    break;
                case 19:
                    await SaveAsync(new TgEfVersionDto() { Version = 20, Description = "Updated the UID field in the filters table" });
                    break;
                case 20:
                    await SaveAsync(new TgEfVersionDto() { Version = 21, Description = "Updated the UID field in the messages table" });
                    break;
                case 21:
                    await SaveAsync(new TgEfVersionDto() { Version = 22, Description = "Updated the UID field in the proxies table" });
                    break;
                case 22:
                    await SaveAsync(new TgEfVersionDto() { Version = 23, Description = "Updated the UID field in the sources table" });
                    break;
                case 23:
                    await SaveAsync(new TgEfVersionDto() { Version = 24, Description = "Updated the UID field in the versions table" });
                    break;
                case 24:
                    await SaveAsync(new TgEfVersionDto() { Version = 25, Description = "Migrated storage to EF Core" });
                    break;
                case 25:
                    await SaveAsync(new TgEfVersionDto() { Version = 26, Description = "Updated apps table" });
                    break;
                case 26:
                    await SaveAsync(new TgEfVersionDto() { Version = 27, Description = "Added contacts table" });
                    break;
                case 27:
                    await SaveAsync(new TgEfVersionDto() { Version = 28, Description = "Added stories table" });
                    break;
                case 28:
                    await SaveAsync(new TgEfVersionDto() { Version = 29, Description = "Updated sources table" });
                    break;
                case 29:
                    await SaveAsync(new TgEfVersionDto() { Version = 30, Description = "Added RowVersion field" });
                    break;
                case 30:
                    await SaveAsync(new TgEfVersionDto() { Version = 31, Description = "Updated apps table" });
                    break;
                case 31:
                    await SaveAsync(new TgEfVersionDto() { Version = 32, Description = "Updated apps table" });
                    break;
                case 32:
                    await SaveAsync(new TgEfVersionDto() { Version = 33, Description = "Updated messages table" });
                    break;
                case 33:
                    await SaveAsync(new TgEfVersionDto() { Version = 34, Description = "Updated documents table" });
                    break;
                case 34:
                    await SaveAsync(new TgEfVersionDto() { Version = 35, Description = "Updated sources table" });
                    break;
                case 35:
                    await SaveAsync(new TgEfVersionDto() { Version = 36, Description = "Updated sources table" });
                    break;
                case 36:
                    await SaveAsync(new TgEfVersionDto() { Version = 37, Description = "Added licenses table" });
                    break;
                case 37:
                    await SaveAsync(new TgEfVersionDto() { Version = 38, Description = "Updated sources table" });
                    break;
                case 38:
                    await SaveAsync(new TgEfVersionDto() { Version = 39, Description = "Updated licenses table" });
                    break;
                case 39:
                    await SaveAsync(new TgEfVersionDto() { Version = 40, Description = "Updated apps table" });
                    break;
                case 40:
                    await SaveAsync(new TgEfVersionDto() { Version = 41, Description = "Updated sources table" });
                    break;
                case 41:
                    await SaveAsync(new TgEfVersionDto() { Version = 42, Description = "Updated messages table" });
                    break;
                case 42:
                    await SaveAsync(new TgEfVersionDto() { Version = 43, Description = "Added users table" });
                    break;
                case 43:
                    await SaveAsync(new TgEfVersionDto() { Version = 44, Description = "Updated sources table" });
                    break;
                case 44:
                    await SaveAsync(new TgEfVersionDto() { Version = 45, Description = "Updated messages table" });
                    break;
                case 45:
                    await SaveAsync(new TgEfVersionDto() { Version = 46, Description = "Added messages relations table" });
                    break;
                case 46:
                    await SaveAsync(new TgEfVersionDto() { Version = 47, Description = "Updated messages table" });
                    break;
                case 47:
                    await SaveAsync(new TgEfVersionDto() { Version = 48, Description = "Added chat users table" });
                    break;
                case 48:
                    await SaveAsync(new TgEfVersionDto() { Version = 49, Description = "Updated sources table" });
                    break;
            }
            if (versionLast.Version >= LastVersion)
                isLast = true;
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(TgEfVersionDto dto, CancellationToken ct = default)
    {
        TgEfVersionEntity? entity = null;
        try
        {
            if (dto is null)
                return;

            // Try to find existing entity
            entity = await GetQuery(isReadOnly: false).SingleOrDefaultAsync(x => x.Uid == dto.Uid, ct);
            if (entity is null)
                // Find by Version
                entity = await GetQuery(isReadOnly: false).SingleOrDefaultAsync(x => x.Version == dto.Version, ct);

            if (entity is null)
            {
                if (EfContext is DbContext dbContext)
                    dbContext.ChangeTracker.Clear();
                // Create new entity
                entity = TgEfDomainUtils.CreateNewEntity(dto, isUidCopy: true);
                EfContext.Versions.Add(entity);
            }
            else
            {
                // Update existing entity
                TgEfDomainUtils.FillEntity(dto, entity, isUidCopy: true);
            }

            ValidateAndNormalize(entity);
            await EfContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error saving version");
            throw;
        }
        finally
        {
            // Avoid EF tracking issues
            if (entity is not null)
                EfContext.DetachItem(entity);
        }
    }

    /// <inheritdoc />
    public async Task SaveListAsync(IEnumerable<TgEfVersionDto> dtos, CancellationToken ct = default)
    {
        try
        {
            foreach (var dto in dtos)
            {
                await SaveAsync(dto, ct);
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex, "Error saving versions");
            throw;
        }
    }
    
    #endregion
}
