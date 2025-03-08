// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Common;

public abstract class TgPageBase : Page
{
	#region Public and private fields, properties, constructor

	public virtual ITgPageViewModel ViewModel => null!;

	#endregion

	#region Public and private methods

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);
		App.MainWindow.DispatcherQueue.TryEnqueueWithLogAsync(async () =>
		{
			await ViewModel.OnNavigatedToAsync(e);
		}, "OnNavigatedTo");
	}

	protected void PageLoaded(object sender, RoutedEventArgs e) => ViewModel.OnLoaded(XamlRoot);

	#endregion
}