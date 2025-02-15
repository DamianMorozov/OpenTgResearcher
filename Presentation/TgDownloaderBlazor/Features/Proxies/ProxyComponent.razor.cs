// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderBlazor.Features.Proxies;

public partial class ProxyComponent : TgPageComponentEnumerable<TgEfProxyDto, TgEfProxyEntity>
{
	#region Public and private fields, properties, constructor

	private TgEfProxyRepository ProxyRepository { get; } = new();

	#endregion

	#region Public and private methods

	protected override async Task OnInitializedAsync()
    {
	    await base.OnInitializedAsync();
	    if (!IsBlazorLoading)
		    return;

	    if (!AppSettings.AppXml.IsExistsEfStorage)
	    {
		    IsBlazorLoading = false;
		    return;
	    }

	    Dtos = await ProxyRepository.GetListDtosAsync(0, 0);
        ItemsCount = await ProxyRepository.GetCountAsync();

        IsBlazorLoading = false;
	}

    #endregion
}