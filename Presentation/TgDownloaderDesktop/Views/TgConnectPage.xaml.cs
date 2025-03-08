// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Views;

public sealed partial class TgConnectPage
{
	#region Public and private fields, properties, constructor

	public override TgConnectViewModel ViewModel { get; }

	public TgConnectPage()
	{
		ViewModel = App.GetService<TgConnectViewModel>();
		InitializeComponent();
		Loaded += PageLoaded;
	}

	#endregion

	#region Public and private methods

	private void OnApiHashTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnApiHashTextChanged(sender, e);

	private void OnApiIdTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnApiIdTextChanged(sender, e);

	private void OnPhoneTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnPhoneTextChanged(sender, e);

	#endregion
}
