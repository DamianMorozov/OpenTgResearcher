// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.ViewModels;

public partial class ContentGridDetailViewModel(ISampleDataService sampleDataService) : ObservableRecipient, INavigationAware
{
	[ObservableProperty]
	public partial SampleOrder? Item { get; set; }

	public void OnNavigatedTo(object parameter)
	{
		Task.Run(async () =>
		{
			try
			{
				if (parameter is long orderId)
				{
					var data = await sampleDataService.GetContentGridDataAsync();
					Item = data.First(i => i.OrderID == orderId);
				}
			}
			catch (Exception ex)
			{
				TgLogUtils.LogFatal(ex, "An error occurred during navigation!");
			}
		});
	}

	public void OnNavigatedFrom()
	{
	}
}
