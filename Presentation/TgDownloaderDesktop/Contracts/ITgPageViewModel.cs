// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Contracts;

public interface ITgPageViewModel
{
	public Task OnNavigatedToAsync(NavigationEventArgs e);
	public void OnLoaded(object parameter);
	public void OnClipboardWriteClick(object sender, RoutedEventArgs e);
	public void OnClipboardSilentWriteClick(object sender, RoutedEventArgs e);
}