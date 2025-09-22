
namespace TgStorage.Contracts;

/// <summary> Storage context </summary>
public interface ITgEfContext : IDisposable
{
    #region DbContext

    /// <summary> Provides access to database related information and operations for this context </summary>
    public DatabaseFacade Database { get; }
	public Task<int> SaveChangesAsync(CancellationToken ct = default);
	public ValueTask<EntityEntry<TEfEntity>> AddItemAsync<TEfEntity>(TEfEntity entity, CancellationToken ct = default)
		where TEfEntity : class, ITgEfEntity, new();
	public EntityEntry<TEfEntity> AddItem<TEfEntity>(TEfEntity entity) where TEfEntity : class, ITgEfEntity, new();
	/// <summary> Update entity </summary>
	public void UpdateItem<TEfEntity>(TEfEntity item) where TEfEntity : class, ITgEfEntity, new();
	public EntityEntry<TEfEntity> RemoveItem<TEfEntity>(TEfEntity entity) where TEfEntity : class, ITgEfEntity, new();
	/// <summary> Detach entity </summary>
	public void DetachItem(object item);

    #endregion

    /// <summary> App queries </summary>
    public DbSet<TgEfAppEntity> Apps { get; set; }
	/// <summary> Contact queries </summary>
	public DbSet<TgEfUserEntity> Users { get; set; }
	/// <summary> Document queries </summary>
	public DbSet<TgEfDocumentEntity> Documents { get; set; }
    /// <summary> Chat users queries </summary>
    public DbSet<TgEfChatUserEntity> ChatUsers { get; set; }
	/// <summary> Filter queries </summary>
	public DbSet<TgEfFilterEntity> Filters { get; set; }
	/// <summary> License queries </summary>
	public DbSet<TgEfLicenseEntity> Licenses { get; set; }
	/// <summary> Message queries </summary>
	public DbSet<TgEfMessageEntity> Messages { get; set; }
    /// <summary> Messages relations queries </summary>
    public DbSet<TgEfMessageRelationEntity> MessagesRelations { get; set; }
    /// <summary> Proxy queries </summary>
    public DbSet<TgEfProxyEntity> Proxies { get; set; }
	/// <summary> Source queries </summary>
	public DbSet<TgEfSourceEntity> Sources { get; set; }
	/// <summary> Stories queries </summary>
	public DbSet<TgEfStoryEntity> Stories { get; set; }
	/// <summary> Version queries </summary>
	public DbSet<TgEfVersionEntity> Versions { get; set; }

    /// <summary> Get data source storage path </summary>
    protected string GetDataSource(string contentRootPath = "");
	/// <summary> Shrink storage </summary>
	public Task ShrinkDbAsync();
	/// <summary> Migrate storage </summary>
	public Task MigrateDbAsync();
    /// <summary> Backup storage </summary>
    public (bool IsSuccess, string FileName) BackupDb(string storagePath = "");
    /// <summary> Load storage backup dtos </summary>
    public ObservableCollection<TgStorageBackupDto> LoadStorageBackupDtos(string storagePath = "");
}
