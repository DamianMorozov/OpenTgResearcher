namespace OpenTgResearcherDesktop.Views;

public partial class TgStoriesPage
{
	#region Fields, properties, constructor

	public override TgStoriesViewModel ViewModel { get; }

	public TgStoriesPage()
	{
		ViewModel = App.GetService<TgStoriesViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
