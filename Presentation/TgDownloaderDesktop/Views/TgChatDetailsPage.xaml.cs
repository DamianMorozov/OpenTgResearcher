// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Views;

public partial class TgChatDetailsPage
{
	#region Public and private fields, properties, constructor

	public TgChatDetailsViewModel ViewModel { get; }

	public TgChatDetailsPage()
	{
		ViewModel = App.GetService<TgChatDetailsViewModel>();
		ViewModel.ScrollRequested = ScrollRequested;
		InitializeComponent();
		Loaded += PageLoaded;
	}

	#endregion

	#region Public and private methods

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		try
		{
			base.OnNavigatedTo(e);
			await ViewModel.OnNavigatedToAsync(e);
		}
		catch (Exception ex)
		{
			await TgDesktopUtils.FileLogAsync(ex, "An error occurred during navigation.");
		}
	}

	private void PageLoaded(object sender, RoutedEventArgs e) => ViewModel.OnLoaded(XamlRoot);

	private void ScrollRequested()
	{
		if (ListViewData is null) return;
		if (ListViewData.Items.Any())
			ListViewData.ScrollIntoView(ListViewData.Items.Last());
	}

	#endregion
}
