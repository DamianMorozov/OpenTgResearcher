namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgHardwareResourcePage
{
	#region Fields, properties, constructor

	public override TgHardwareResourceViewModel ViewModel { get; }

	public TgHardwareResourcePage()
	{
		ViewModel = App.GetService<TgHardwareResourceViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
