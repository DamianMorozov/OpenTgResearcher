namespace TgStorage.Common;

/// <summary> Base DB context </summary>
public abstract class TgEfContextBase : DbContext, ITgEfContext, ITgDisposable
{
    #region Fields, properties, constructor

    /// <inheritdoc />
    public DbSet<TgEfAppEntity> Apps { get; set; } = null!;
    /// <inheritdoc />
    public DbSet<TgEfDocumentEntity> Documents { get; set; } = null!;
    /// <inheritdoc />
    public DbSet<TgEfChatUserEntity> ChatUsers { get; set; } = null!;
    /// <inheritdoc />
    public DbSet<TgEfFilterEntity> Filters { get; set; } = null!;
    /// <inheritdoc />
    public DbSet<TgEfLicenseEntity> Licenses { get; set; } = null!;
    /// <inheritdoc />
    public DbSet<TgEfMessageEntity> Messages { get; set; } = null!;
    /// <inheritdoc />
    public DbSet<TgEfMessageRelationEntity> MessagesRelations { get; set; } = null!;
    /// <inheritdoc />
    public DbSet<TgEfProxyEntity> Proxies { get; set; } = null!;
    /// <inheritdoc />
    public DbSet<TgEfSourceEntity> Sources { get; set; } = null!;
    /// <inheritdoc />
    public DbSet<TgEfStoryEntity> Stories { get; set; } = null!;
    /// <inheritdoc />
    public DbSet<TgEfUserEntity> Users { get; set; } = null!;
    /// <inheritdoc />
    public DbSet<TgEfVersionEntity> Versions { get; set; } = null!;

#if DEBUG
    private bool _isCreated;
#endif

    public static TgAppSettingsHelper TgAppSettings => TgAppSettingsHelper.Instance;

    // Public constructor need for resolve: The exception 'A suitable constructor for type 'TgStorage.Domain.TgEfContextBase' could not be located
    public TgEfContextBase()
    {
#if DEBUG
        if (!_isCreated)
        {
            Debug.WriteLine($"{nameof(TgEfContextBase)} is created", TgConstants.LogTypeStorage);
            _isCreated = true;
        }
#endif
    }


    /// <summary> Inject options </summary>
    // For using: services.AddDbContextFactory<TgEfContextBase>
    public TgEfContextBase(DbContextOptions options) : base(options)
    {
#if DEBUG
        if (!_isCreated)
        {
            Debug.WriteLine($"{nameof(TgEfContextBase)} is created with {nameof(options)}", TgConstants.LogTypeStorage);
            _isCreated = true;
        }
#endif
    }


    /// <summary> Inject options </summary>
    // For using: services.AddDbContextFactory<TgEfContextBase>
    public TgEfContextBase(DbContextOptions<TgEfContextBase> options) : base(options)
    {
#if DEBUG
        if (!_isCreated)
        {
            Debug.WriteLine($"{nameof(TgEfContextBase)} is created", TgConstants.LogTypeStorage);
            _isCreated = true;
        }
#endif
    }

#endregion

    #region IDisposable

    /// <summary> Locker object </summary>
    private static readonly Lock _locker = new();
    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~TgEfContextBase() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary> Release managed resources </summary>
    public virtual void ReleaseManagedResources()
    {
        //
    }

    /// <summary> Release unmanaged resources </summary>
    public virtual void ReleaseUnmanagedResources()
    {
        //
    }

    /// <summary> Dispose pattern </summary>
    public override void Dispose()
    {
        // Dispose of unmanaged resources
        Dispose(true);
        // Suppress finalization
        GC.SuppressFinalize(this);
        // Base dispose
        base.Dispose();
    }

    /// <summary> Dispose pattern </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        using (_locker.EnterScope())
        {
            // Release managed resources
            if (disposing)
                ReleaseManagedResources();
            // Release unmanaged resources
            ReleaseUnmanagedResources();
            // Flag
            _disposed = true;
        }
    }

    #endregion

    #region Methods - EF Core

    public async ValueTask<EntityEntry<TEfEntity>> AddItemAsync<TEfEntity>(TEfEntity entity, CancellationToken ct = default)
        where TEfEntity : class, ITgEfEntity, new() =>
        await AddAsync(entity, ct);

    public EntityEntry<TEfEntity> AddItem<TEfEntity>(TEfEntity entity)
        where TEfEntity : class, ITgEfEntity, new() => Add(entity);

    /// <inheritdoc />
    public void UpdateItem<TEfEntity>(TEfEntity entity) where TEfEntity : class, ITgEfEntity, new() => Update(entity);

    public EntityEntry<TEfEntity> RemoveItem<TEfEntity>(TEfEntity entity) where TEfEntity : class, ITgEfEntity, new() => Remove(entity);

    /// <inheritdoc />
    public void DetachItem(object item) => Entry(item).State = EntityState.Detached;

    #endregion

    #region Methods

    /// <inheritdoc />
    public string GetDataSource(string contentRootPath = "")
    {
        var storagePath = TgGlobalTools.AppType switch
        {
            TgEnumAppType.Memory => ":memory:", // In-memory database
            TgEnumAppType.Console =>
                File.Exists(TgAppSettingsHelper.Instance.AppXml.XmlEfStorage)
                ? TgAppSettingsHelper.Instance.AppXml.XmlEfStorage : TgGlobalTools.AppStorage,
            TgEnumAppType.Blazor => Path.Combine(contentRootPath, TgGlobalTools.AppStorage),
            TgEnumAppType.Desktop => TgGlobalTools.AppStorage,
            TgEnumAppType.Test => @"c:\OpenTgResearcher\TgStorage\TgStorageTests.db",
            _ => throw new ArgumentOutOfRangeException(nameof(TgGlobalTools.AppType), TgGlobalTools.AppType, null)
        };
        // Concatenation
        storagePath = $"{TgLocaleHelper.Instance.SqliteDataSource}={storagePath}";
#if DEBUG
        Debug.WriteLine(storagePath, TgConstants.LogTypeStorage);
#endif
        return storagePath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        LoggerFactory factory = new();
        optionsBuilder
#if DEBUG
            .LogTo(message => Debug.WriteLine($"{TgGlobalTools.AppType}{nameof(ContextId)} {ContextId}: {message}", TgConstants.LogTypeStorage), LogLevel.Debug)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
#endif
            .EnableThreadSafetyChecks()
            .UseLoggerFactory(factory)
        ;
        // This type need for resolve: The exception 'No database provider has been configured for this DbContext.
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apps
        modelBuilder.Entity<TgEfAppEntity>(entity =>
        {
            entity.ToTable(TgEfConstants.TableApps);
            // Shadow property for concurrency
            entity.Property<byte[]>(TgEfConstants.ColumnRowVersion).IsRowVersion();
            // Unique key
            entity.HasAlternateKey(x => new { x.ApiHash });
            // Relationship to proxy by single key
            entity.HasOne(app => app.Proxy)
                .WithMany(proxy => proxy.Apps)
                .HasForeignKey(app => app.ProxyUid)
                .HasPrincipalKey(proxy => proxy.Uid);
        });

        // ChatUsers
        modelBuilder.Entity<TgEfChatUserEntity>(entity =>
        {
            entity.ToTable(TgEfConstants.TableChatUsers);
            // Shadow property for concurrency
            entity.Property<byte[]>(TgEfConstants.ColumnRowVersion).IsRowVersion();
            // Relationship to chat (TgEfSourceEntity)
            entity.HasOne(x => x.Chat)
                .WithMany(c => c.ChatUsers)
                .HasForeignKey(x => x.ChatId)
                .HasPrincipalKey(c => c.Id)
                .OnDelete(DeleteBehavior.Cascade);
            // Relationship to user (TgEfUserEntity)
            entity.HasOne(x => x.User)
                .WithMany(u => u.ChatUsers)
                .HasForeignKey(x => x.UserId)
                .HasPrincipalKey(u => u.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Documents
        modelBuilder.Entity<TgEfDocumentEntity>(entity =>
        {
            entity.ToTable(TgEfConstants.TableDocuments);
            // Shadow property for concurrency
            entity.Property<byte[]>(TgEfConstants.ColumnRowVersion).IsRowVersion();
            // Relationship to source by single key
            entity.HasOne(document => document.Source)
                .WithMany(source => source.Documents)
                .HasForeignKey(document => document.SourceId)
                .HasPrincipalKey(source => source.Id);
        });

        // Filters
        modelBuilder.Entity<TgEfFilterEntity>(entity =>
        {
            entity.ToTable(TgEfConstants.TableFilters);
            // Shadow property for concurrency
            entity.Property<byte[]>(TgEfConstants.ColumnRowVersion).IsRowVersion();
        });

        // Licenses
        modelBuilder.Entity<TgEfLicenseEntity>(entity =>
        {
            entity.ToTable(TgEfConstants.TableLicenses);
            // Shadow property for concurrency
            entity.Property<byte[]>(TgEfConstants.ColumnRowVersion).IsRowVersion();
            // Unique key
            entity.HasAlternateKey(x => new { x.LicenseKey });
        });

        // Messages
        modelBuilder.Entity<TgEfMessageEntity>(entity =>
        {
            entity.ToTable(TgEfConstants.TableMessages);
            // Shadow property for concurrency
            entity.Property<byte[]>(TgEfConstants.ColumnRowVersion).IsRowVersion();
            // Unique key
            entity.HasAlternateKey(x => new { x.SourceId, x.Id });
            // Relationship to source by single key
            entity.HasOne(x => x.Source)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.SourceId)
                .HasPrincipalKey(x => x.Id);
        });

        // Messages relations
        modelBuilder.Entity<TgEfMessageRelationEntity>(entity =>
        {
            entity.ToTable(TgEfConstants.TableMessagesRelations);
            // Shadow property for concurrency
            entity.Property<byte[]>(TgEfConstants.ColumnRowVersion).IsRowVersion();
            // Relationship to parent message by composite key (SourceId + Id)
            entity.HasOne(x => x.ParentMessage)
                .WithMany()
                .HasForeignKey(x => new { x.ParentSourceId, x.ParentMessageId })
                .HasPrincipalKey(x => new { x.SourceId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            // Relationship to child message by composite key (SourceId + Id)
            entity.HasOne(x => x.ChildMessage)
                .WithMany()
                .HasForeignKey(x => new { x.ChildSourceId, x.ChildMessageId })
                .HasPrincipalKey(x => new { x.SourceId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            // Relationship to parent source by single key
            entity.HasOne(x => x.ParentSource)
                .WithMany()
                .HasForeignKey(x => x.ParentSourceId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
            // Relationship to child source by single key
            entity.HasOne(x => x.ChildSource)
                .WithMany()
                .HasForeignKey(x => x.ChildSourceId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
            // Index
            entity.HasIndex(x => new { x.ParentSourceId, x.ParentMessageId, x.ChildSourceId, x.ChildMessageId })
                .IsUnique()
                .HasDatabaseName($"IX_{TgEfConstants.TableMessagesRelations}_UNIQUE_LINK");
        });

        // Proxies
        modelBuilder.Entity<TgEfProxyEntity>(entity =>
        {
            entity.ToTable(TgEfConstants.TableProxies);
            // Shadow property for concurrency
            entity.Property<byte[]>(TgEfConstants.ColumnRowVersion).IsRowVersion();
        });

        // Sources
        modelBuilder.Entity<TgEfSourceEntity>(entity =>
        {
            entity.ToTable(TgEfConstants.TableSources);
            // Shadow property for concurrency
            entity.Property<byte[]>(TgEfConstants.ColumnRowVersion).IsRowVersion();
            // Unique key
            entity.HasAlternateKey(x => new { x.Id });
        });

        // Stories
        modelBuilder.Entity<TgEfStoryEntity>(entity =>
        {
            entity.ToTable(TgEfConstants.TableStories);
            // Shadow property for concurrency
            entity.Property<byte[]>(TgEfConstants.ColumnRowVersion).IsRowVersion();
            // Unique key
            entity.HasAlternateKey(x => new { x.Id, x.FromId });
        });

        // Users
        modelBuilder.Entity<TgEfUserEntity>(entity =>
        {
            entity.ToTable(TgEfConstants.TableUsers);
            // Shadow property for concurrency
            entity.Property<byte[]>(TgEfConstants.ColumnRowVersion).IsRowVersion();
            // Unique key
            entity.HasAlternateKey(x => new { x.Id });
        });

        // Versions
        modelBuilder.Entity<TgEfVersionEntity>(entity =>
        {
            entity.ToTable(TgEfConstants.TableVersions);
            // Shadow property for concurrency
            entity.Property<byte[]>(TgEfConstants.ColumnRowVersion).IsRowVersion();
            // Unique key
            entity.HasAlternateKey(x => new { x.Version });
        });
    }

    /// <inheritdoc />
    public async Task ShrinkDbAsync() => await Database.ExecuteSqlRawAsync("VACUUM;");

    /// <inheritdoc />
    public async Task MigrateDbAsync() => await Database.MigrateAsync();

    /// <inheritdoc />
    public (bool IsSuccess, string FileName) BackupDb(string storagePath = "")
    {
        if (string.IsNullOrEmpty(storagePath))
            storagePath = TgAppSettings.AppXml.XmlEfStorage;

        if (File.Exists(storagePath))
        {
            var dt = DateTime.UtcNow;
            var fileBackup =
                $"{Path.GetDirectoryName(storagePath)}\\" +
                $"{Path.GetFileNameWithoutExtension(storagePath)}_{dt:yyyyMMdd}_{dt:HHmmss}.bak";
            File.Copy(storagePath, fileBackup);
            return (File.Exists(fileBackup), fileBackup);
        }

        return (false, string.Empty);
    }

    public ObservableCollection<TgStorageBackupDto> LoadStorageBackupDtos(string storagePath = "")
    {
        if (string.IsNullOrEmpty(storagePath))
            storagePath = TgAppSettings.AppXml.XmlEfStorage;

        var dtos = new ObservableCollection<TgStorageBackupDto>();
        if (File.Exists(storagePath))
        {
            dtos.Add(new TgStorageBackupDto(storagePath, new FileInfo(storagePath).Length));
            var fileNames = Directory.GetFiles(
                Path.GetDirectoryName(storagePath) ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(storagePath)}_*.bak",
                SearchOption.TopDirectoryOnly);
            foreach (var fileName in fileNames)
            {
                dtos.Add(new TgStorageBackupDto(fileName, new FileInfo(fileName).Length));
            }
        }

        // Order
        dtos = [.. dtos.OrderBy(x => x.FileName)];

        return dtos;
    }

    #endregion
}
