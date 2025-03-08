// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Views;

public partial class TgStoriesPage
{
	#region Public and private fields, properties, constructor

	public override TgStoriesViewModel ViewModel { get; }

	public TgStoriesPage()
	{
		ViewModel = App.GetService<TgStoriesViewModel>();
		InitializeComponent();
		Loaded += PageLoaded;
	}

	#endregion
}
