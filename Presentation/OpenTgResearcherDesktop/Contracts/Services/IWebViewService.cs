// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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
