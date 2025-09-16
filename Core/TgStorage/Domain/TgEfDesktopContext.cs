namespace TgStorage.Domain;

/// <summary> Desktop DB context </summary>
public sealed class TgEfDesktopContext : TgEfContextBase, ITgEfContext
{
    #region Fields, properties, constructor

    public TgEfDesktopContext() : base() { }
    
    public TgEfDesktopContext(DbContextOptions<TgEfDesktopContext> options) : base() { }

    #endregion

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
