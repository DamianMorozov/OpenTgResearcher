namespace OpenTgResearcherDesktop.Views;

public partial class TgStorageTablesPage
{
	#region Fields, properties, constructor

	public override TgStorageTablesViewModel ViewModel { get; }

	public TgStorageTablesPage()
	{
		ViewModel = App.GetService<TgStorageTablesViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

    #endregion
}
