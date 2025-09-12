namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgLicensePage
{
	#region Fields, properties, constructor

	public override TgLicenseViewModel ViewModel { get; }

	public TgLicensePage()
	{
		ViewModel = App.GetService<TgLicenseViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
