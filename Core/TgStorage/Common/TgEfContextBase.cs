// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Common;

/// <summary> Base DB context </summary>
public abstract class TgEfContextBase : DbContext, ITgEfContext, ITgDisposable
{
	#region Public and private fields, properties, constructor

	/// <summary> App queries </summary>
	public DbSet<TgEfAppEntity> Apps { get; set; } = null!;
	/// <summary> Contact queries </summary>
	public DbSet<TgEfContactEntity> Contacts { get; set; } = null!;
	/// <summary> Document queries </summary>
	public DbSet<TgEfDocumentEntity> Documents { get; set; } = null!;
	/// <summary> Filter queries </summary>
	public DbSet<TgEfFilterEntity> Filters { get; set; } = null!;
	/// <summary> License queries </summary>
	public DbSet<TgEfLicenseEntity> Licenses { get; set; } = null!;
	/// <summary> Message queries </summary>
	public DbSet<TgEfMessageEntity> Messages { get; set; } = null!;
	/// <summary> Proxy queries </summary>
	public DbSet<TgEfProxyEntity> Proxies { get; set; } = null!;
	/// <summary> Source queries </summary>
	public DbSet<TgEfSourceEntity> Sources { get; set; } = null!;
	/// <summary> Stories queries </summary>
	public DbSet<TgEfStoryEntity> Stories { get; set; } = null!;
	/// <summary> Version queries </summary>
	public DbSet<TgEfVersionEntity> Versions { get; set; } = null!;

	public static TgAppSettingsHelper TgAppSettings => TgAppSettingsHelper.Instance;

	// Public constructor need for resolve: The exception 'A suitable constructor for type 'TgStorage.Domain.TgEfContextBase' could not be located
	protected TgEfContextBase()
	{
#if DEBUG
		Debug.WriteLine($"  {nameof(TgEfContextBase)} is created", TgConstants.LogTypeStorage);
#endif
	}

	/// <summary> Inject options </summary>
	// For using: services.AddDbContextFactory<TgEfContextBase>
	protected TgEfContextBase(DbContextOptions options) : base(options)
	{
#if DEBUG
		Debug.WriteLine($"  {nameof(TgEfContextBase)} is created", TgConstants.LogTypeStorage);
#endif
	}

	/// <summary> Inject options </summary>
	// For using: services.AddDbContextFactory<TgEfContextBase>
	protected TgEfContextBase(DbContextOptions<TgEfContextBase> options) : base(options)
	{
#if DEBUG
		Debug.WriteLine($"  {nameof(TgEfContextBase)} is created", TgConstants.LogTypeStorage);
#endif
	}

    #endregion

    #region IDisposable

    /// <summary> Locker object </summary>
    public object Locker { get; } = new();
    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~TgEfContextBase() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException($"{nameof(TgDisposable)}: {TgConstants.ObjectHasBeenDisposedOff}!");
    }

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
		base.Dispose();
        // Dispose of unmanaged resources
        Dispose(true);
        // Suppress finalization
        GC.SuppressFinalize(this);
    }

    /// <summary> Dispose pattern </summary>
    protected void Dispose(bool disposing)
    {
        if (_disposed) return;
        lock (Locker)
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

    #region Public and private methods - EF Core

    public async ValueTask<EntityEntry<TEfEntity>> AddItemAsync<TEfEntity>(TEfEntity entity, CancellationToken cancellationToken = default)
		where TEfEntity : class, ITgEfEntity<TEfEntity>, new() => 
		await AddAsync(entity, cancellationToken);

	public EntityEntry<TEfEntity> AddItem<TEfEntity>(TEfEntity entity)
		where TEfEntity : class, ITgEfEntity<TEfEntity>, new() => Add(entity);

	/// <inheritdoc />
	public void UpdateItem<TEfEntity>(TEfEntity entity) where TEfEntity : class, ITgEfEntity<TEfEntity>, new() => Update(entity);

	public EntityEntry<TEfEntity> RemoveItem<TEfEntity>(TEfEntity entity) where TEfEntity : class, ITgEfEntity<TEfEntity>, new() => Remove(entity);

	/// <inheritdoc />
	public void DetachItem(object item) => Entry(item).State = EntityState.Detached;

	#endregion

	#region Public and private methods

	/// <inheritdoc />
	public string GetStoragePath(string contentRootPath = "")
	{
		var storagePath = TgGlobalTools.AppType switch
		{
			TgEnumAppType.Memory => ":memory:", // In-memory database
			TgEnumAppType.Console =>
				File.Exists(TgAppSettingsHelper.Instance.AppXml.XmlEfStorage) 
				? TgAppSettingsHelper.Instance.AppXml.XmlEfStorage : TgGlobalTools.FileEfStorage,
			TgEnumAppType.Blazor => Path.Combine(contentRootPath, TgGlobalTools.FileEfStorage),
			TgEnumAppType.Desktop => File.Exists(TgGlobalTools.AppStorage) ? TgGlobalTools.AppStorage : TgGlobalTools.FileEfStorage,
			TgEnumAppType.Test => @"d:\DATABASES\SQLITE\TgStorageTest.db",
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
			.LogTo(message => Debug.WriteLine($"  {TgGlobalTools.AppType}{nameof(ContextId)} {ContextId}: {message}", TgConstants.LogTypeStorage), LogLevel.Debug)
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
		// Magic string - Define the model - Concurrency tokens
		// https://learn.microsoft.com/en-us/ef/core/modeling/table-splitting
		// https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/concurrency?view=aspnetcore-9.0&source=docs
		// This property isn't on the C# class, so we set it up as a "shadow" property and use it for concurrency.
		modelBuilder.Entity<TgEfAppEntity>().Property(x => x.RowVersion).IsRowVersion();
		modelBuilder.Entity<TgEfContactEntity>().Property(x => x.RowVersion).IsRowVersion();
		modelBuilder.Entity<TgEfDocumentEntity>().Property(x => x.RowVersion).IsRowVersion();
		modelBuilder.Entity<TgEfFilterEntity>().Property(x => x.RowVersion).IsRowVersion();
		modelBuilder.Entity<TgEfLicenseEntity>().Property(x => x.RowVersion).IsRowVersion();
		modelBuilder.Entity<TgEfMessageEntity>().Property(x => x.RowVersion).IsRowVersion();
		modelBuilder.Entity<TgEfProxyEntity>().Property(x => x.RowVersion).IsRowVersion();
		modelBuilder.Entity<TgEfSourceEntity>().Property(x => x.RowVersion).IsRowVersion();
		modelBuilder.Entity<TgEfStoryEntity>().Property(x => x.RowVersion).IsRowVersion();
		modelBuilder.Entity<TgEfVersionEntity>().Property(x => x.RowVersion).IsRowVersion();
		// Ignore
		modelBuilder.Entity<TgEfAppEntity>().Ignore(TgEfConstants.ColumnRowVersion);
		modelBuilder.Entity<TgEfContactEntity>().Ignore(TgEfConstants.ColumnRowVersion);
		modelBuilder.Entity<TgEfDocumentEntity>().Ignore(TgEfConstants.ColumnRowVersion);
		modelBuilder.Entity<TgEfFilterEntity>().Ignore(TgEfConstants.ColumnRowVersion);
		modelBuilder.Entity<TgEfLicenseEntity>().Ignore(TgEfConstants.ColumnRowVersion);
		modelBuilder.Entity<TgEfMessageEntity>().Ignore(TgEfConstants.ColumnRowVersion);
		modelBuilder.Entity<TgEfProxyEntity>().Ignore(TgEfConstants.ColumnRowVersion);
		modelBuilder.Entity<TgEfSourceEntity>().Ignore(TgEfConstants.ColumnRowVersion);
		modelBuilder.Entity<TgEfStoryEntity>().Ignore(TgEfConstants.ColumnRowVersion);
		modelBuilder.Entity<TgEfVersionEntity>().Ignore(TgEfConstants.ColumnRowVersion);

		// Apps
		modelBuilder.Entity<TgEfAppEntity>(entity =>
		{
			entity.ToTable(TgEfConstants.TableApps);
		});
		modelBuilder.Entity<TgEfAppEntity>()
			.HasOne(app => app.Proxy)
			.WithMany(proxy => proxy.Apps)
			.HasForeignKey(app => app.ProxyUid)
			.HasPrincipalKey(proxy => proxy.Uid);

		// Contacts
		modelBuilder.Entity<TgEfContactEntity>(entity =>
		{
			entity.ToTable(TgEfConstants.TableContacts);
		});

		// Documents
		modelBuilder.Entity<TgEfDocumentEntity>(entity =>
		{
			entity.ToTable(TgEfConstants.TableDocuments);
		});
		modelBuilder.Entity<TgEfDocumentEntity>()
			.HasOne(document => document.Source)
			.WithMany(source => source.Documents)
			.HasForeignKey(document => document.SourceId)
			.HasPrincipalKey(source => source.Id);

		// Filters
		modelBuilder.Entity<TgEfFilterEntity>(entity =>
		{
			entity.ToTable(TgEfConstants.TableFilters);
		});

		// Licenses
		modelBuilder.Entity<TgEfLicenseEntity>(entity =>
		{
			entity.ToTable(TgEfConstants.TableLicenses);
		});
		modelBuilder.Entity<TgEfLicenseEntity>();

		// Messages
		modelBuilder.Entity<TgEfMessageEntity>(entity =>
		{
			entity.ToTable(TgEfConstants.TableMessages);
		});
		modelBuilder.Entity<TgEfMessageEntity>()
			.HasOne(message => message.Source)
			.WithMany(source => source.Messages)
			.HasForeignKey(message => message.SourceId)
			.HasPrincipalKey(source => source.Id);

		// Proxies
		modelBuilder.Entity<TgEfProxyEntity>(entity =>
		{
			entity.ToTable(TgEfConstants.TableProxies);
		});

		// Sources
		modelBuilder.Entity<TgEfSourceEntity>(entity =>
		{
			entity.ToTable(TgEfConstants.TableSources);
		});

		// Stories
		modelBuilder.Entity<TgEfStoryEntity>(entity =>
		{
			entity.ToTable(TgEfConstants.TableStories);
		});

		// Versions
		modelBuilder.Entity<TgEfVersionEntity>(entity =>
		{
			entity.ToTable(TgEfConstants.TableVersions);
		});
	}

	/// <inheritdoc />
	public (bool IsSuccess, string FileName) BackupDb()
	{
		// Console app
		if (TgGlobalTools.AppType == TgEnumAppType.Memory || TgGlobalTools.AppType == TgEnumAppType.Console)
		{
			if (File.Exists(TgAppSettings.AppXml.XmlEfStorage))
			{
				var dt = DateTime.Now;
				var fileBackup =
					$"{Path.GetDirectoryName(TgAppSettings.AppXml.XmlEfStorage)}\\" +
					$"{Path.GetFileNameWithoutExtension(TgAppSettings.AppXml.XmlEfStorage)}_{dt:yyyyMMdd}_{dt:HHmmss}.bak";
				File.Copy(TgAppSettings.AppXml.XmlEfStorage, fileBackup);
				return (File.Exists(fileBackup), fileBackup);
			}
		}
		return (false, string.Empty);
	}

	/// <inheritdoc />
	public async Task ShrinkDbAsync() => await Database.ExecuteSqlRawAsync("VACUUM;");

	/// <inheritdoc />
	public async Task MigrateDbAsync() => await Database.MigrateAsync();

	#endregion
}