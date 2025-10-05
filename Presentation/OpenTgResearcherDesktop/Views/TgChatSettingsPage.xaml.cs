namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatSettingsPage
{
    #region Fields, properties, constructor

    public override TgChatSettingsViewModel ViewModel { get; }

    public TgChatSettingsPage()
    {
        ViewModel = App.GetService<TgChatSettingsViewModel>();

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
