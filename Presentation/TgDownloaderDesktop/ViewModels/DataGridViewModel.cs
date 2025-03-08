﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.ViewModels;

public partial class DataGridViewModel(ISampleDataService sampleDataService) : ObservableRecipient, INavigationAware
{
	[ObservableProperty]
	public partial ObservableCollection<SampleOrder> Source { get; set; } = [];

	public void OnNavigatedTo(object parameter)
	{
		Task.Run(async () =>
		{
			try
			{
				Source.Clear();
				// TODO: Replace with real data.
				var data = await sampleDataService.GetGridDataAsync();
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
}
