namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgLoadDataPage
{
	#region Fields, properties, constructor

	public override TgLoadDataViewModel ViewModel { get; }

	public TgLoadDataPage()
	{
		ViewModel = App.GetService<TgLoadDataViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
