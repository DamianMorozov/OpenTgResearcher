// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.ViewModels;

public partial class ContentGridViewModel : ObservableRecipient, INavigationAware
{
	private readonly INavigationService _navigationService;
	private readonly ISampleDataService _sampleDataService;

	[ObservableProperty]
	public partial ObservableCollection<SampleOrder> Source { get; set; } = [];
	public IRelayCommand ItemClickCommand { get; }

	public ContentGridViewModel(INavigationService navigationService, ISampleDataService sampleDataService)
	{
		_navigationService = navigationService;
		_sampleDataService = sampleDataService;
		ItemClickCommand = new AsyncRelayCommand<SampleOrder>(OnItemClickAsync);
	}

	public void OnNavigatedTo(object parameter)
	{
		Task.Run(async () =>
		{
			try
			{
				Source.Clear();
				// TODO: Replace with real data.
				var data = await _sampleDataService.GetContentGridDataAsync();
				foreach (var item in data)
				{
					Source.Add(item);
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

	private async Task OnItemClickAsync(SampleOrder? clickedItem)
	{
		if (clickedItem != null)
		{
			_navigationService.SetListDataItemForNextConnectedAnimation(clickedItem);
			_navigationService.NavigateTo(typeof(ContentGridDetailViewModel).FullName!, clickedItem.OrderID);
		}
		await Task.CompletedTask;
	}
}
