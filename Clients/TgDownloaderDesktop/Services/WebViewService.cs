﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Services;

public sealed class WebViewService : IWebViewService
{
	private WebView2? _webView;

	public Uri? Source => _webView?.Source;

	[MemberNotNullWhen(true, nameof(_webView))]
	public bool CanGoBack => _webView != null && _webView.CanGoBack;

	[MemberNotNullWhen(true, nameof(_webView))]
	public bool CanGoForward => _webView != null && _webView.CanGoForward;

	public event EventHandler<CoreWebView2WebErrorStatus>? NavigationCompleted;

	public WebViewService()
	{
	}

	[MemberNotNull(nameof(_webView))]
	public void Initialize(WebView2 webView)
	{
		_webView = webView;
		_webView.NavigationCompleted += OnWebViewNavigationCompleted;
	}

	public void GoBack() => _webView?.GoBack();

	public void GoForward() => _webView?.GoForward();

	public void Reload() => _webView?.Reload();

	public void UnregisterEvents()
	{
		if (_webView != null)
		{
			_webView.NavigationCompleted -= OnWebViewNavigationCompleted;
		}
	}

	private void OnWebViewNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args) => NavigationCompleted?.Invoke(this, args.WebErrorStatus);
}
