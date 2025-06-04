// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Proxies;

/// <summary> Proxy view-model </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfProxyViewModel : TgEntityViewModelBase<TgEfProxyEntity, TgEfProxyDto>, ITgDtoViewModel
{
	#region Public and private fields, properties, constructor

	public override TgEfProxyRepository Repository { get; } = new();
	[ObservableProperty]
	public partial TgEfProxyDto Dto { get; set; } = null!;
	public Action<TgEfProxyViewModel> UpdateAction { get; set; } = _ => { };


	public TgEfProxyViewModel(TgEfProxyEntity item) : base()
	{
		Fill(item);
	}

	public TgEfProxyViewModel() : base()
	{
		TgEfProxyEntity item = new();
		Fill(item);
	}

	#endregion

	#region Public and private methods

	public override string ToString() => Dto.ToString() ?? string.Empty;

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfProxyEntity item)
	{
		Dto ??= new();
		Dto.Copy(item, isUidCopy: true);
	}

	public async Task<TgEfStorageResult<TgEfProxyEntity>> SaveAsync() =>
		await Repository.SaveAsync(Dto.GetNewEntity());

	#endregion
}