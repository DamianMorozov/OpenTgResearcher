// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Views;

public partial class TgConnectPage
{
	#region Public and private fields, properties, constructor

	public TgConnectViewModel ViewModel { get; }

	public TgConnectPage()
	{
		ViewModel = App.GetService<TgConnectViewModel>();
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

	private void OnApiHashTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnApiHashTextChanged(sender, e);

	private void OnApiIdTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnApiIdTextChanged(sender, e);

	private void OnPhoneTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnPhoneTextChanged(sender, e);

	#endregion
}
