namespace TgStorage.Domain;

/// <summary> Memory DB context </summary>
public sealed class TgEfMemoryContext : TgEfContextBase, ITgEfContext
{
    #region Methods

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(GetDataSource());
        }
    }

	#endregion
}
