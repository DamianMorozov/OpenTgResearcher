namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgUsersPage
{
	#region Fields, properties, constructor

	public override TgUsersViewModel ViewModel { get; }

	public TgUsersPage()
	{
		ViewModel = App.GetService<TgUsersViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
		Loaded += PageLoaded;
	}

    #endregion
}
