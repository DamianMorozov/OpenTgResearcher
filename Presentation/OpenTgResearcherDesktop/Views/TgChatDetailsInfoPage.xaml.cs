namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatDetailsInfoPage
{
    #region Fields, properties, constructor

    public override TgChatDetailsInfoViewModel ViewModel { get; }

    public TgChatDetailsInfoPage()
    {
        ViewModel = App.GetService<TgChatDetailsInfoViewModel>();
        
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PageLoaded;
    }

    #endregion
}
