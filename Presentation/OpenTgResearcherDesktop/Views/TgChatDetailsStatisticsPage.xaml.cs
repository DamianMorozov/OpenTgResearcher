// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatDetailsStatisticsPage
{
    #region Public and private fields, properties, constructor

    public override TgChatDetailsStatisticsViewModel ViewModel { get; }

    public TgChatDetailsStatisticsPage()
    {
        ViewModel = App.GetService<TgChatDetailsStatisticsViewModel>();
        ViewModel.ScrollRequested = ScrollRequested;

        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnPageLoaded;
    }

    #endregion

    #region Public and private methods

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        PageLoaded(sender, e);
    }

    private void ScrollRequested()
    {
        // Scroll to the last item in the Messages ListView
        if (ListViewMessages is null) return;
        if (ListViewMessages.Items.Any())
            ListViewMessages.ScrollIntoView(ListViewMessages.Items.Last());
    }

    /// <summary> View image </summary>
    private void OnImagePreviewClick(object sender, RoutedEventArgs e)
    {
        if (sender is Image img && img.Source is BitmapImage bitmap)
        {
            FullScreenImage.Source = bitmap;
            ViewModel.IsImageViewerVisible = true;
            Bindings.Update();
        }
    }

    /// <summary> Exit from full screen image view </summary>
    private void OnFullScreenImageTapped(object sender, RoutedEventArgs e)
    {
        ViewModel.IsImageViewerVisible = false;
        //ViewModel.EmptyData = !ViewModel.Messages.Any();
        FullScreenImage.Source = null;
        Bindings.Update();
    }

    /// <summary> Click on user </summary>
    public void OnUserClick(object sender, RoutedEventArgs e) => ViewModel.OnUserClick(sender, e);

    #endregion
}
