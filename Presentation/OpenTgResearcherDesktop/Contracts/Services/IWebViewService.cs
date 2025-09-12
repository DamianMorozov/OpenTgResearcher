namespace OpenTgResearcherDesktop.Contracts.Services;

public interface IWebViewService
{
	Uri? Source
	{
		get;
	}

	bool CanGoBack
	{
		get;
	}

	bool CanGoForward
	{
		get;
	}

	event EventHandler<CoreWebView2WebErrorStatus>? NavigationCompleted;

	void Initialize(WebView2 webView);

	void GoBack();

	void GoForward();

	void Reload();

	void UnregisterEvents();
}
