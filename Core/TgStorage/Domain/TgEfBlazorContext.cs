namespace TgStorage.Domain;

/// <summary> Blazor DB context </summary>
public sealed class TgEfBlazorContext(IWebHostEnvironment webHostEnvironment) : TgEfContextBase, ITgEfContext
{
    #region Methods

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
		    optionsBuilder.UseSqlite(GetDataSource(webHostEnvironment.ContentRootPath));
        }
    }

	#endregion
}
