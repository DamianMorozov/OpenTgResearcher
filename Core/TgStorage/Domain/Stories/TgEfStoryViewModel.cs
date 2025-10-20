namespace TgStorage.Domain.Stories;

/// <summary> Story ViewModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfStoryViewModel : TgEntityViewModelBase<TgEfStoryEntity, TgEfStoryDto>, ITgDtoViewModel
{
	#region Fields, properties, constructor

	public override ITgEfStoryRepository Repository { get; }
	[ObservableProperty]
	public partial TgEfStoryDto Dto { get; set; } = null!;

	public TgEfStoryViewModel(Autofac.IContainer container, TgEfStoryEntity item) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfStoryRepository>();
        Fill(item);
	}

	public TgEfStoryViewModel(Autofac.IContainer container) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfStoryRepository>();
        TgEfStoryEntity item = new();
		Fill(item);
	}

    #endregion

    #region Methods

    /// <inheritdoc />
    public override string ToString() => Dto.ToString() ?? string.Empty;

    /// <inheritdoc />
    public override string ToDebugString() => Dto.ToDebugString();

    /// <inheritdoc />
    public void Fill(TgEfStoryEntity item) => Dto = TgEfDomainUtils.CreateNewDto(item, isUidCopy: true);

	#endregion
}
