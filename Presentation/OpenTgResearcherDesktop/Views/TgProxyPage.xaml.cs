namespace OpenTgResearcherDesktop.Views;

public partial class TgProxyPage
{
	#region Fields, properties, constructor

	public override TgProxyViewModel ViewModel { get; }

	public TgProxyPage()
	{
        ViewModel = App.GetService<TgProxyViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
