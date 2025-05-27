// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Contracts;

/// <summary> Storage context </summary>
public interface ITgEfContext : IDisposable
{
    #region DbContext

    /// <summary> Provides access to database related information and operations for this context </summary>
    public DatabaseFacade Database { get; }
	public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	public ValueTask<EntityEntry<TEfEntity>> AddItemAsync<TEfEntity>(TEfEntity entity, CancellationToken cancellationToken = default)
		where TEfEntity : class, ITgEfEntity<TEfEntity>, new();
	public EntityEntry<TEfEntity> AddItem<TEfEntity>(TEfEntity entity) where TEfEntity : class, ITgEfEntity<TEfEntity>, new();
	/// <summary> Update entity </summary>
	public void UpdateItem<TEfEntity>(TEfEntity item) where TEfEntity : class, ITgEfEntity<TEfEntity>, new();
	public EntityEntry<TEfEntity> RemoveItem<TEfEntity>(TEfEntity entity) where TEfEntity : class, ITgEfEntity<TEfEntity>, new();
	/// <summary> Detach entity </summary>
	public void DetachItem(object item);

    #endregion

    /// <summary> App queries </summary>
    public DbSet<TgEfAppEntity> Apps { get; set; }
	/// <summary> Contact queries </summary>
	public DbSet<TgEfContactEntity> Contacts { get; set; }
	/// <summary> Document queries </summary>
	public DbSet<TgEfDocumentEntity> Documents { get; set; }
	/// <summary> Filter queries </summary>
	public DbSet<TgEfFilterEntity> Filters { get; set; }
	/// <summary> License queries </summary>
	public DbSet<TgEfLicenseEntity> Licenses { get; set; }
	/// <summary> Message queries </summary>
	public DbSet<TgEfMessageEntity> Messages { get; set; }
	/// <summary> Proxy queries </summary>
	public DbSet<TgEfProxyEntity> Proxies { get; set; }
	/// <summary> Source queries </summary>
	public DbSet<TgEfSourceEntity> Sources { get; set; }
	/// <summary> Stories queries </summary>
	public DbSet<TgEfStoryEntity> Stories { get; set; }
	/// <summary> Version queries </summary>
	public DbSet<TgEfVersionEntity> Versions { get; set; }

	/// <summary> Get storage path </summary>
	protected string GetStoragePath(string contentRootPath = "");

    /// <summary> Backup storage </summary>
    public (bool IsSuccess, string FileName) BackupDb();

	/// <summary> Shrink storage </summary>
	public Task ShrinkDbAsync();

	/// <summary> Migrate storage </summary>
	public Task MigrateDbAsync();
}