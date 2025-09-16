namespace TgStorage.Domain;

/// <summary> Test DB context </summary>
public sealed class TgEfTestContext : TgEfContextBase, ITgEfContext
{
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(GetDataSource());
        }
	}
}
