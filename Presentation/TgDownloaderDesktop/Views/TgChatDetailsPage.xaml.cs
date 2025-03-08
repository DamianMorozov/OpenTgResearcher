// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Views;

public sealed partial class TgChatDetailsPage
{
	#region Public and private fields, properties, constructor

	public override TgChatDetailsViewModel ViewModel { get; }

	public TgChatDetailsPage()
	{
		ViewModel = App.GetService<TgChatDetailsViewModel>();
		ViewModel.ScrollRequested = ScrollRequested;
		InitializeComponent();
		Loaded += PageLoaded;
	}

	#endregion

	#region Public and private methods

	private void ScrollRequested()
	{
		if (ListViewData is null) return;
		if (ListViewData.Items.Any())
			ListViewData.ScrollIntoView(ListViewData.Items.Last());
	}

	#endregion
}
