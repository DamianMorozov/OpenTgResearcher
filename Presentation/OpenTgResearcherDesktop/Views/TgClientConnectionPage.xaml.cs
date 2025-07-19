// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgClientConnectionPage
{
	#region Public and private fields, properties, constructor

	public override TgClientConnectionViewModel ViewModel { get; }

	public TgClientConnectionPage()
	{
		ViewModel = App.GetService<TgClientConnectionViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion

	#region Public and private methods

	private void OnApiHashTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnApiHashTextChanged(sender, e);

	private void OnApiIdTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnApiIdTextChanged(sender, e);

	private void OnPhoneTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnPhoneTextChanged(sender, e);

	#endregion
}
