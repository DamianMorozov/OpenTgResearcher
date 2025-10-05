namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatStatisticsPage
{
    #region Fields, properties, constructor

    public override TgChatStatisticsViewModel ViewModel { get; }

    public TgChatStatisticsPage()
    {
        ViewModel = App.GetService<TgChatStatisticsViewModel>();

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
