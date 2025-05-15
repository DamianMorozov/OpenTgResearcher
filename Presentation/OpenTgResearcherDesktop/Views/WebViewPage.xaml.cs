// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

// To learn more about WebView2, see https://docs.microsoft.com/microsoft-edge/webview2/.
public sealed partial class WebViewPage
{
	#region Public and private fields, properties, constructor

	public WebViewViewModel ViewModel { get; }

	public WebViewPage()
	{
		ViewModel = App.GetService<WebViewViewModel>();
		InitializeComponent();
		ViewModel.WebViewService.Initialize(WebView);
	}

	#endregion
}
