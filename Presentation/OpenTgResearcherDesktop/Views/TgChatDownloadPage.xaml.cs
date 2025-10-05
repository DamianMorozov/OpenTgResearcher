namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatDownloadPage
{
    #region Fields, properties, constructor

    public override TgChatDownloadViewModel ViewModel { get; }

    public TgChatDownloadPage()
    {
        ViewModel = App.GetService<TgChatDownloadViewModel>();

        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnPageLoaded;
    }

    #endregion

    #region Methods

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        PageLoaded(sender, e);
    }

    #endregion
}
