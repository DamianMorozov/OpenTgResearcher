namespace TgStorage.Domain.Proxies;

/// <summary> Proxy ViewModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfProxyViewModel : TgEntityViewModelBase<TgEfProxyEntity, TgEfProxyDto>, ITgDtoViewModel
{
	#region Fields, properties, constructor

	public override ITgEfProxyRepository Repository { get; } 
	[ObservableProperty]
	public partial TgEfProxyDto Dto { get; set; } = null!;
	public Action<TgEfProxyViewModel> UpdateAction { get; set; } = _ => { };


	public TgEfProxyViewModel(Autofac.IContainer container, TgEfProxyEntity item) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfProxyRepository>();
        Fill(item);
	}

	public TgEfProxyViewModel(Autofac.IContainer container) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfProxyRepository>();
        TgEfProxyEntity item = new();
		Fill(item);
	}

    #endregion

    #region Methods

    /// <inheritdoc />
    public override string ToString() => Dto.ToString();

    /// <inheritdoc />
    public override string ToDebugString() => Dto.ToDebugString();

    /// <inheritdoc />
    public void Fill(TgEfProxyEntity item) => Dto = TgEfDomainUtils.CreateNewDto(item, isUidCopy: true);

    /// <inheritdoc />
    public async Task<TgEfStorageResult<TgEfProxyEntity>> SaveAsync() => await Repository.SaveAsync(TgEfDomainUtils.CreateNewEntity(Dto, isUidCopy: true));

    /// <summary> Check if the proxy is empty </summary>
    public bool IsEmptyProxy => Dto.Type == TgEnumProxyType.None && (Dto.UserName == "No user" || Dto.Password == "No password");

    #endregion
}
