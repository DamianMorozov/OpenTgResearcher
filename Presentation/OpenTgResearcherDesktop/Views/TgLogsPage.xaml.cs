namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgLogsPage
{
	#region Fields, properties, constructor

	public override TgLogsViewModel ViewModel { get; }

	public TgLogsPage()
	{
		ViewModel = App.GetService<TgLogsViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
