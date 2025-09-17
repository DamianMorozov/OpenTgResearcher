namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgClientConnectionPage
{
	#region Fields, properties, constructor

	public override TgClientConnectionViewModel ViewModel { get; }

	public TgClientConnectionPage()
	{
		ViewModel = App.GetService<TgClientConnectionViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion

	#region Methods

	private void OnApiHashTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnApiHashTextChanged(sender, e);

	private void OnApiIdTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnApiIdTextChanged(sender, e);

	private void OnPhoneTextChanged(object sender, TextChangedEventArgs e) => ViewModel.OnPhoneTextChanged(sender, e);

    #endregion
}
