// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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