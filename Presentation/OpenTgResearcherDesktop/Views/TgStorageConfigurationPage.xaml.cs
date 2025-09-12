namespace OpenTgResearcherDesktop.Views;

public partial class TgStorageConfigurationPage
{
	#region Fields, properties, constructor

	public override TgStorageConfigurationViewModel ViewModel { get; }

	public TgStorageConfigurationPage()
	{
		ViewModel = App.GetService<TgStorageConfigurationViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
