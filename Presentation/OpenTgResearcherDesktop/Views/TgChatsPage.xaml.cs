namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatsPage
{
	#region Fields, properties, constructor

	public override TgChatsViewModel ViewModel { get; }

	public TgChatsPage()
	{
		ViewModel = App.GetService<TgChatsViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
	}

	#endregion
}
