// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Views;

public partial class TgLoadDataPage
{
	#region Public and private fields, properties, constructor

	public TgLoadDataViewModel ViewModel { get; }

	public TgLoadDataPage()
	{
		ViewModel = App.GetService<TgLoadDataViewModel>();
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

	protected override void OnNavigatedFrom(NavigationEventArgs e)
	{
		base.OnNavigatedFrom(e);
	}

	#endregion
}
