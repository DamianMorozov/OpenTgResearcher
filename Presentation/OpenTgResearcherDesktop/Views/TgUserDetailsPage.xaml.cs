namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgUserDetailsPage
{
	#region Fields, properties, constructor

	public override TgUserDetailsViewModel ViewModel { get; }

	public TgUserDetailsPage()
	{
		ViewModel = App.GetService<TgUserDetailsViewModel>();

		InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

    #endregion
}
