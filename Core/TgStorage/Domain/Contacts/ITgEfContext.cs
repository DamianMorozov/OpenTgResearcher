// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Contacts;

/// <summary> Storage context </summary>
public interface ITgEfContext : IDisposable
	//IInfrastructure<IServiceProvider>,
	//IDbContextDependencies
	//IDbSetCache,
	//IDbContextPoolable
{
	#region DbContext

	/// <summary> Provides access to database related information and operations for this context </summary>
	public DatabaseFacade Database { get; }
	public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	public ValueTask<EntityEntry<TEntity>> AddItemAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
	public EntityEntry<TEntity> RemoveItem<TEntity>(TEntity entity) where TEntity : class;

	#endregion

	/// <summary> App queries </summary>
	public DbSet<TgEfAppEntity> Apps { get; set; }
	/// <summary> Contact queries </summary>
	public DbSet<TgEfContactEntity> Contacts { get; set; }
	/// <summary> Document queries </summary>
	public DbSet<TgEfDocumentEntity> Documents { get; set; }
	/// <summary> Filter queries </summary>
	public DbSet<TgEfFilterEntity> Filters { get; set; }
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
	protected string GetStoragePath(TgEnumAppType appType);

	/// <summary> Backup storage </summary>
	public (bool IsSuccess, string FileName) BackupDb();

	/// <summary> Shrink storage </summary>
	public Task ShrinkDbAsync();

	/// <summary> Migrate storage </summary>
	public Task MigrateDbAsync();

	/// <summary> Detach entity </summary>
	public void DetachItem(object item);

	/// <summary> Update entity </summary>
	public void UpdateItem(object item);
}