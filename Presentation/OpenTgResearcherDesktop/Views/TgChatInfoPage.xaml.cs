namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatInfoPage
{
    #region Fields, properties, constructor

    public override TgChatInfoViewModel ViewModel { get; }

    public TgChatInfoPage()
    {
        ViewModel = App.GetService<TgChatInfoViewModel>();
        
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
    }

    #endregion
}
