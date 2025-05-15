// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

// TODO: Review best practices and distribution guidelines for WebView2.
// https://docs.microsoft.com/microsoft-edge/webview2/get-started/winui
// https://docs.microsoft.com/microsoft-edge/webview2/concepts/developer-guide
// https://docs.microsoft.com/microsoft-edge/webview2/concepts/distribution
public partial class WebViewViewModel : ObservableRecipient, INavigationAware
{
	// TODO: Set the default URL to display.
	[ObservableProperty]
	public partial Uri Source { get; set; } = new("https://docs.microsoft.com/windows/apps/");

	[ObservableProperty]
	public partial bool IsLoading { get; set; } = true;

	[ObservableProperty]
	public partial bool HasFailures { get; set; }

	public IWebViewService WebViewService { get; }

	public WebViewViewModel(IWebViewService webViewService)
	{
		WebViewService = webViewService;
	}

	[RelayCommand]
	private async Task OpenInBrowser()
	{
		if (WebViewService.Source != null)
		{
			await Windows.System.Launcher.LaunchUriAsync(WebViewService.Source);
		}
	}

	[RelayCommand]
	private void Reload()
	{
		WebViewService.Reload();
	}

	[RelayCommand(CanExecute = nameof(BrowserCanGoForward))]
	private void BrowserForward()
	{
		if (WebViewService.CanGoForward)
		{
			WebViewService.GoForward();
		}
	}

	private bool BrowserCanGoForward()
	{
		return WebViewService.CanGoForward;
	}

	[RelayCommand(CanExecute = nameof(BrowserCanGoBack))]
	private void BrowserBack()
	{
		if (WebViewService.CanGoBack)
		{
			WebViewService.GoBack();
		}
	}

	private bool BrowserCanGoBack()
	{
		return WebViewService.CanGoBack;
	}

	public void OnNavigatedTo(object parameter)
	{
		WebViewService.NavigationCompleted += OnNavigationCompleted;
	}

	public void OnNavigatedFrom()
	{
		WebViewService.UnregisterEvents();
		WebViewService.NavigationCompleted -= OnNavigationCompleted;
	}

	private void OnNavigationCompleted(object? sender, CoreWebView2WebErrorStatus webErrorStatus)
	{
		IsLoading = false;
		BrowserBackCommand.NotifyCanExecuteChanged();
		BrowserForwardCommand.NotifyCanExecuteChanged();

		if (webErrorStatus != default)
		{
			HasFailures = true;
		}
	}

	[RelayCommand]
	private void OnRetry()
	{
		HasFailures = false;
		IsLoading = true;
		WebViewService?.Reload();
	}
}
