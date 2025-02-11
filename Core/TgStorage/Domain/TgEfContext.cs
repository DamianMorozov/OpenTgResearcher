// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
#pragma warning disable S2589

namespace TgStorage.Domain;

/// <summary> EF DB context </summary>
public class TgEfContext : DbContext
{
    #region Public and private fields, properties, constructor

    /// <summary> App queries </summary>
    public DbSet<TgEfAppEntity> Apps { get; set; } = default!;
	/// <summary> Contact queries </summary>
	public DbSet<TgEfContactEntity> Contacts { get; set; } = default!;
	/// <summary> Document queries </summary>
	public DbSet<TgEfDocumentEntity> Documents { get; set; } = default!;
    /// <summary> Filter queries </summary>
    public DbSet<TgEfFilterEntity> Filters { get; set; } = default!;
    /// <summary> Message queries </summary>
    public DbSet<TgEfMessageEntity> Messages { get; set; } = default!;
    /// <summary> Proxy queries </summary>
    public DbSet<TgEfProxyEntity> Proxies { get; set; } = default!;
    /// <summary> Source queries </summary>
    public DbSet<TgEfSourceEntity> Sources { get; set; } = default!;
    /// <summary> Stories queries </summary>
    public DbSet<TgEfStoryEntity> Stories { get; set; } = default!;
    /// <summary> Version queries </summary>
    public DbSet<TgEfVersionEntity> Versions { get; set; } = default!;
    public static TgAppSettingsHelper TgAppSettings => TgAppSettingsHelper.Instance;

	public static bool IsXmlReady => TgAppSettings.AppXml.IsExistsEfStorage;

    #endregion

    #region Public and private methods

    public TgEfContext()
    {
#if DEBUG
        Debug.WriteLine($"Created TgEfContext with {nameof(ContextId)} {ContextId}", TgConstants.LogTypeStorage);
#endif
    }

    /// <summary> Inject options </summary>
    // For using: services.AddDbContextFactory<TgEfContext>
    public TgEfContext(DbContextOptions options) : base(options)
    {
#if DEBUG
        Debug.WriteLine($"Created TgEfContext with {nameof(ContextId)} {ContextId}", TgConstants.LogTypeStorage);
#endif
    }

    /// <summary> Inject options </summary>
    // For using: services.AddDbContextFactory<TgEfContext>
    public TgEfContext(DbContextOptions<TgEfContext> options) : base(options)
    {
#if DEBUG
        Debug.WriteLine($"Created TgEfContext with {nameof(ContextId)} {ContextId}", TgConstants.LogTypeStorage);
#endif
    }

	private string GetStoragePath()
	{
		var storagePath = string.Empty;
		// Console app
		if (TgAsyncUtils.AppType == TgEnumAppType.Default || TgAsyncUtils.AppType == TgEnumAppType.Console)
		{
			storagePath = File.Exists(TgAppSettingsHelper.Instance.AppXml.XmlEfStorage)
				? $"{TgLocaleHelper.Instance.SqliteDataSource}={TgAppSettingsHelper.Instance.AppXml.XmlEfStorage}"
				: $"{TgLocaleHelper.Instance.SqliteDataSource}={TgEfUtils.FileEfStorage}";
		}
		// Desktop app
		if (TgAsyncUtils.AppType == TgEnumAppType.Desktop)
		{
			storagePath = File.Exists(TgEfUtils.AppStorage)
				? $"{TgLocaleHelper.Instance.SqliteDataSource}={TgEfUtils.AppStorage}"
				: $"{TgLocaleHelper.Instance.SqliteDataSource}={TgEfUtils.FileEfStorage}";
		}
		return storagePath;
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        LoggerFactory factory = new();
		optionsBuilder
#if DEBUG
            .LogTo(message => Debug.WriteLine($"{nameof(ContextId)} {ContextId}: {message}", TgConstants.LogTypeStorage), LogLevel.Debug)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
#endif
            .EnableThreadSafetyChecks()
            .UseLoggerFactory(factory)
        ;
		optionsBuilder.UseSqlite(GetStoragePath());
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
		modelBuilder.Entity<TgEfMessageEntity>().Property(x => x.RowVersion).IsRowVersion();
		modelBuilder.Entity<TgEfProxyEntity>().Property(x => x.RowVersion).IsRowVersion();
		modelBuilder.Entity<TgEfSourceEntity>().Property(x => x.RowVersion).IsRowVersion();
		modelBuilder.Entity<TgEfStoryEntity>().Property(x => x.RowVersion).IsRowVersion();
		modelBuilder.Entity<TgEfVersionEntity>().Property(x => x.RowVersion).IsRowVersion();
		// Ignore
		modelBuilder.Entity<TgCommonEntity>().Ignore(TgEfConstants.ColumnRowVersion);
		modelBuilder.Entity<TgEfAppEntity>().Ignore(TgEfConstants.ColumnRowVersion);
		modelBuilder.Entity<TgEfContactEntity>().Ignore(TgEfConstants.ColumnRowVersion);
		modelBuilder.Entity<TgEfDocumentEntity>().Ignore(TgEfConstants.ColumnRowVersion);
		modelBuilder.Entity<TgEfFilterEntity>().Ignore(TgEfConstants.ColumnRowVersion);
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

    public (bool IsSuccess, string FileName) BackupDb()
    {
		if (string.IsNullOrEmpty(GetStoragePath()))
			return (false, string.Empty);
		// Console app
		if (TgAsyncUtils.AppType == TgEnumAppType.Default || TgAsyncUtils.AppType == TgEnumAppType.Console)
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

	/// <summary> Shrink storage </summary>
	public async Task CompactDbAsync()
	{
		if (string.IsNullOrEmpty(GetStoragePath()))
			return;
		await Database.ExecuteSqlRawAsync("VACUUM;");
	}

	/// <summary> Create and update storage </summary>
	public async Task CreateAndUpdateDbAsync()
	{
		if (string.IsNullOrEmpty(GetStoragePath()))
			return;
		await Database.MigrateAsync();
	}

	#endregion
}
