namespace OpenTgResearcherDesktop.Views;

public partial class TgStorageAdvancedPage
{
	#region Fields, properties, constructor

	public override TgStorageAdvancedViewModel ViewModel { get; }

	public TgStorageAdvancedPage()
	{
		ViewModel = App.GetService<TgStorageAdvancedViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
