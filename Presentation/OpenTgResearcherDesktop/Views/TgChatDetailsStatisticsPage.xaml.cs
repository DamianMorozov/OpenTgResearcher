namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatDetailsStatisticsPage
{
    #region Fields, properties, constructor

    public override TgChatDetailsStatisticsViewModel ViewModel { get; }

    public TgChatDetailsStatisticsPage()
    {
        ViewModel = App.GetService<TgChatDetailsStatisticsViewModel>();

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

    /// <summary> Click on user </summary>
    public void OnUserClick(object sender, RoutedEventArgs e) => ViewModel.OnUserClick(sender, e);

    #endregion
}
