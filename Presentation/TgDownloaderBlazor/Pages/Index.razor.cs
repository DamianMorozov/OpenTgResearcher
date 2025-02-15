// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderBlazor.Pages;

public sealed partial class Index : TgPageComponentBase
{
	#region Public and private fields, properties, constructor

	//

	#endregion

	#region Public and private methods

	protected override async Task OnInitializedAsync()
    {
		await base.OnInitializedAsync();
		if (!IsBlazorLoading)
			return;
		
		await Task.Delay(1);
		IsBlazorLoading = false;
	}

	private async Task ResetToDefault(MouseEventArgs arg)
	{
		await Task.Delay(1);
		AppSettings.DefaultXmlSettings();
	}

	private async Task SaveToXml(MouseEventArgs arg)
	{
		await Task.Delay(1);
		AppSettings.StoreXmlSettings();
		AppSettings.LoadXmlSettings();
	}

	private async Task CreateNewStorage(MouseEventArgs arg)
	{
		await Task.Delay(1);
		ConfirmOptions confirmOptions = GetConfirmOptions();
		bool? result = await DialogService.Confirm(TgLocale.MenuStorageDbCreateNew, "", confirmOptions);
		if (result == true)
		{
			// Create and update storage
			await TgEfUtils.CreateAndUpdateDbAsync();
			DialogService.Close();
		}
	}

	#endregion
}