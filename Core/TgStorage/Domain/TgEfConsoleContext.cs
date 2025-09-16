namespace TgStorage.Domain;

/// <summary> Console DB context </summary>
public sealed class TgEfConsoleContext : TgEfContextBase, ITgEfContext
{
    #region Fields, properties, constructor

    public TgEfConsoleContext() : base() { }

    public TgEfConsoleContext(DbContextOptions<TgEfConsoleContext> options) : base(options) { }

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
