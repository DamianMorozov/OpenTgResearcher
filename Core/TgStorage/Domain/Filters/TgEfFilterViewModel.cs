namespace TgStorage.Domain.Filters;

/// <summary> Contact ViewModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfFilterViewModel : TgEntityViewModelBase<TgEfFilterEntity, TgEfFilterDto>, ITgDtoViewModel
{
	#region Fields, properties, constructor

	public override ITgEfFilterRepository Repository { get; }
	[ObservableProperty]
	public partial TgEfFilterDto Dto { get; set; } = null!;


	public TgEfFilterViewModel(Autofac.IContainer container, TgEfFilterEntity item) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfFilterRepository>();
        Fill(item);
	}

	public TgEfFilterViewModel(Autofac.IContainer container) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfFilterRepository>();
        TgEfFilterEntity item = new();
		Fill(item);
	}

    #endregion

    #region Methods

    /// <inheritdoc />
    public override string ToString() => Dto.ToString() ?? string.Empty;

    /// <inheritdoc />
    public override string ToDebugString() => Dto.ToDebugString();

    /// <inheritdoc />
    public void Fill(TgEfFilterEntity item) => Dto = TgEfDomainUtils.CreateNewDto(item, isUidCopy: true);

    /// <inheritdoc />
    public async Task<TgEfStorageResult<TgEfFilterEntity>> SaveAsync() => await Repository.SaveAsync(TgEfDomainUtils.CreateNewEntity(Dto, isUidCopy: true));

	#endregion
}
