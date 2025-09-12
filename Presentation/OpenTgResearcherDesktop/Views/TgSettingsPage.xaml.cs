namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgSettingsPage
{
	#region Fields, properties, constructor

	public override TgSettingsViewModel ViewModel { get; }

	public TgSettingsPage()
	{
		ViewModel = App.GetService<TgSettingsViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
		Loaded += PageLoaded;
	}

	#endregion
}
