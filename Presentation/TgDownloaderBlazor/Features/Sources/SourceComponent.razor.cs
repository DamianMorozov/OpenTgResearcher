// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderBlazor.Features.Sources;

public partial class SourceComponent : TgPageComponentEnumerable<TgEfSourceDto, TgEfSourceEntity>
{
	#region Public and private fields, properties, constructor

	private TgEfSourceRepository SourceRepository { get; } = new();

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

		Dtos = await SourceRepository.GetListDtosAsync(0, 0);
		Dtos = Dtos.OrderBy(x => x.UserName).ThenBy(x => x.Title);
        ItemsCount = await SourceRepository.GetCountAsync();

        IsBlazorLoading = false;
	}

    #endregion
}