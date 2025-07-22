// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatDetailsPage
{
    #region Public and private fields, properties, constructor

    public override TgChatDetailsViewModel ViewModel { get; }

    public TgChatDetailsPage()
    {
        ViewModel = App.GetService<TgChatDetailsViewModel>();
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

        ViewModel.ContentFrame = ContentFrame;
    }

    private void ScrollRequested()
    {
        // Scroll to the last item in the Messages ListView
        if (ListViewMessages is null) return;
        if (ListViewMessages.Items.Any())
            ListViewMessages.ScrollIntoView(ListViewMessages.Items.Last());

        // Scroll to the last item in the Participants ListView
        if (ListViewParticipants is null) return;
        if (ListViewParticipants.Items.Any())
            ListViewParticipants.ScrollIntoView(ListViewParticipants.Items.Last());
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

    /// <summary> Chat user </summary>
    public void OnChatUserClick(object sender, RoutedEventArgs e) => ViewModel.OnChatUserClick(sender, e);

    /// <summary> Change chat details page </summary>
    private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        var selectedItem = sender.SelectedItem;
        int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
        switch (currentSelectedIndex)
        {
            case 0:
                ContentFrame.Navigate(typeof(TgChatDetailsInfoPage), ViewModel.Dto.Id);
                break;
            //case 1:
            //    pageType = typeof(SamplePage2);
            //    break;
            //case 2:
            //    pageType = typeof(SamplePage3);
            //    break;
            //case 3:
            //    pageType = typeof(SamplePage4);
            //    break;
            default:
                break;
        }
        //var slideNavigationTransitionEffect = currentSelectedIndex - previousSelectedIndex > 0 ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;
        //ContentFrame.Navigate(pageType, null, new SlideNavigationTransitionInfo() { Effect = slideNavigationTransitionEffect });
        //previousSelectedIndex = currentSelectedIndex;
    }

    #endregion
}
