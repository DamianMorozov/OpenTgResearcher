// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderBlazor.Features.Versions;

public partial class VersionComponent : TgPageComponentEnumerable<TgEfVersionDto, TgEfVersionEntity>
{
	#region Public and private fields, properties, constructor

	private TgEfVersionRepository VersionRepository { get; } = new();

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

		Dtos = await VersionRepository.GetListDtosAsync(0, 0);
		Dtos = Dtos.OrderByDescending(x => x.Version);
        ItemsCount = await VersionRepository.GetCountAsync();

        IsBlazorLoading = false;
	}

    #endregion
}