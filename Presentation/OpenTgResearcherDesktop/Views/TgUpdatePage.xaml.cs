namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgUpdatePage
{
	#region Fields, properties, constructor

	public override TgUpdateViewModel ViewModel { get; }

	public TgUpdatePage()
	{
		ViewModel = App.GetService<TgUpdateViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
