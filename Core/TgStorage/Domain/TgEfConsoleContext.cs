// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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