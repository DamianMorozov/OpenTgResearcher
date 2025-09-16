namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgMainPage
{
	#region Fields, properties, constructor

	public override TgMainViewModel ViewModel { get; }

	public TgMainPage()
	{
		ViewModel = App.GetService<TgMainViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
