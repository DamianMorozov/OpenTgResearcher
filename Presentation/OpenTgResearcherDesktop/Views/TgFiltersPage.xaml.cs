namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgFiltersPage
{
	#region Fields, properties, constructor

	public override TgFiltersViewModel ViewModel { get; }

	public TgFiltersPage()
	{
		ViewModel = App.GetService<TgFiltersViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
