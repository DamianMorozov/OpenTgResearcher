// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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

	public override string ToString() => Dto.ToString();

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfProxyEntity item)
	{
		Dto ??= new();
		Dto.Copy(item, isUidCopy: true);
	}

	public async Task<TgEfStorageResult<TgEfProxyEntity>> SaveAsync() =>
		await Repository.SaveAsync(Dto.GetNewEntity());

    /// <summary> Check if the proxy is empty </summary>
    public bool IsEmptyProxy => Dto.Type == TgEnumProxyType.None && (Dto.UserName == "No user" || Dto.Password == "No password");

    #endregion
}