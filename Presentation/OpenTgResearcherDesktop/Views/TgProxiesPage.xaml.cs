namespace OpenTgResearcherDesktop.Views;

public partial class TgProxiesPage
{
	#region Fields, properties, constructor

	public override TgProxiesViewModel ViewModel { get; }

	public TgProxiesPage()
	{
        ViewModel = App.GetService<TgProxiesViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
