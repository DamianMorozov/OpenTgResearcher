namespace TgStorage.Domain.Sources;

/// <summary> Source ViewModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfSourceViewModel : TgEntityViewModelBase<TgEfSourceEntity, TgEfSourceDto>, ITgEfSourceViewModel
{
	#region Fields, properties, constructor

	public override ITgEfSourceRepository Repository { get; }
    [ObservableProperty]
	public partial TgEfSourceDto Dto { get; set; } = null!;

    public TgEfSourceViewModel(Autofac.IContainer container, TgEfSourceEntity item) : base()
    {
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfSourceRepository>();
        Fill(item);
    }

    public TgEfSourceViewModel(Autofac.IContainer container) : base()
    {
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfSourceRepository>();
        TgEfSourceEntity item = new();
		Fill(item);
	}

    #endregion

    #region Methods

    /// <inheritdoc />
    public override string ToString() => Dto.ToString();

    /// <inheritdoc />
    public override string ToDebugString() => Dto.ToDebugString();

    public void Fill(TgEfSourceEntity item) => Dto = TgEfDomainUtils.CreateNewDto(item, isUidCopy: true);

    public async Task<TgEfStorageResult<TgEfSourceEntity>> SaveAsync() => await Repository.SaveAsync(TgEfDomainUtils.CreateNewEntity(Dto, isUidCopy: true));

	#endregion
}
