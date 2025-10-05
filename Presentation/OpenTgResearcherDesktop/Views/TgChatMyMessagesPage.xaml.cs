namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatMyMessagesPage
{
    #region Fields, properties, constructor

    public override TgChatMyMessagesViewModel ViewModel { get; }

    public TgChatMyMessagesPage()
    {
        ViewModel = App.GetService<TgChatMyMessagesViewModel>();
        ViewModel.ScrollRequested = ScrollRequested;

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

    private void ScrollRequested()
    {
        // Scroll to the last item in the Participants ListView
        if (ListViewMessages is null) return;
        if (ListViewMessages.Items.Any())
            ListViewMessages.ScrollIntoView(ListViewMessages.Items.Last());
    }

    /// <summary> Click on user </summary>
    public void OnUserClick(object sender, RoutedEventArgs e) => ViewModel.OnUserClick(sender, e);

    #endregion
}
