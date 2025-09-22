namespace TgStorage.Domain;

/// <summary> Blazor DB context </summary>
public sealed class TgEfBlazorContext : TgEfContextBase, ITgEfContext
{
    #region Fields, properties, constructor

    private readonly IWebHostEnvironment? _webHostEnvironment;

    public TgEfBlazorContext() : base() { }

    public TgEfBlazorContext(IWebHostEnvironment webHostEnvironment) : base()
    {
        _webHostEnvironment = webHostEnvironment;
    }

    #endregion

    #region Methods

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(GetDataSource(_webHostEnvironment?.ContentRootPath ?? Environment.ProcessPath ?? string.Empty));
        }
    }

    #endregion
}
