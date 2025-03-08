// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Views;

public sealed partial class TgContactsPage
{
	#region Public and private fields, properties, constructor

	public override TgContactsViewModel ViewModel { get; }

	public TgContactsPage()
	{
		ViewModel = App.GetService<TgContactsViewModel>();
		InitializeComponent();
		Loaded += PageLoaded;
	}

	#endregion
}
