namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgHardwareControlPage
{
	#region Fields, properties, constructor

	public override TgHardwareControlViewModel ViewModel { get; }

	public TgHardwareControlPage()
	{
		ViewModel = App.GetService<TgHardwareControlViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
