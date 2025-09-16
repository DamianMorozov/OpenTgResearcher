namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatDetailsParticipantsPage
{
    #region Fields, properties, constructor

    public override TgChatDetailsParticipantsViewModel ViewModel { get; }

    public TgChatDetailsParticipantsPage()
    {
        ViewModel = App.GetService<TgChatDetailsParticipantsViewModel>();
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
        if (ListViewParticipants is null) return;
        if (ListViewParticipants.Items.Any())
            ListViewParticipants.ScrollIntoView(ListViewParticipants.Items.Last());
    }

    /// <summary> Click on user </summary>
    public void OnUserClick(object sender, RoutedEventArgs e) => ViewModel.OnUserClick(sender, e);

    #endregion
}
