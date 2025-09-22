namespace TgStorage.Domain.Versions;

/// <summary> Version ViewModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfVersionViewModel : TgEntityViewModelBase<TgEfVersionEntity, TgEfVersionDto>, ITgDtoViewModel
{
	#region Fields, properties, constructor

	public override ITgEfVersionRepository Repository { get; }
	[ObservableProperty]
	public partial TgEfVersionDto Dto { get; set; } = null!;

	public TgEfVersionViewModel(Autofac.IContainer container, TgEfVersionEntity item) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfVersionRepository>();
        Fill(item);
	}

	public TgEfVersionViewModel(Autofac.IContainer container) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfVersionRepository>();
        TgEfVersionEntity item = new();
		Fill(item);
	}

    #endregion

    #region Methods

    /// <inheritdoc />
    public override string ToString() => Dto.ToString() ?? string.Empty;

    /// <inheritdoc />
    public override string ToDebugString() => Dto.ToDebugString();

    /// <inheritdoc />
    public void Fill(TgEfVersionEntity item) => Dto = TgEfDomainUtils.CreateNewDto(item, isUidCopy: true);

    /// <inheritdoc />
    public async Task<TgEfStorageResult<TgEfVersionEntity>> SaveAsync() => await Repository.SaveAsync(TgEfDomainUtils.CreateNewEntity(Dto, isUidCopy: true));

    #endregion
}
