// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

public partial class ListDetailsViewModel(ISampleDataService sampleDataService) : ObservableRecipient, INavigationAware
{
	[ObservableProperty]
	public partial SampleOrder? Selected { get; set; }

	[ObservableProperty]
	public partial ObservableCollection<SampleOrder> SampleItems { get; set; } = [];

	public void OnNavigatedTo(object parameter)
	{
        Task.Run((Func<Task?>)(async () =>
		{
			try
			{
                SampleItems.Clear();
				// TODO: Replace with real data.
				var data = await sampleDataService.GetListDetailsDataAsync();
				foreach (var item in data)
				{
                    SampleItems.Add(item);
				}
			}
			catch (Exception ex)
			{
                TgLogUtils.WriteExceptionWithMessage(ex, "An error occurred during navigation!");
			}
		}));
	}

	public void OnNavigatedFrom()
	{
	}

	public void EnsureItemSelected()
	{
		Selected ??= SampleItems.First();
	}
}
