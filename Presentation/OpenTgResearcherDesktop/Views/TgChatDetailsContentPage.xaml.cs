// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatDetailsContentPage
{
    #region Fields, properties, constructor

    public override TgChatDetailsContentViewModel ViewModel { get; }

    public TgChatDetailsContentPage()
    {
        ViewModel = App.GetService<TgChatDetailsContentViewModel>();
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
        // Scroll to the last item in the Messages ListView
        if (ListViewMessages is null) return;
        if (ListViewMessages.Items.Any())
            ListViewMessages.ScrollIntoView(ListViewMessages.Items.Last());
    }

    /// <summary> View preview click - open image </summary>
    private void OnImagePreviewClick(object sender, RoutedEventArgs e)
    {
        if (sender is Image img && img.Source is BitmapImage bitmap)
        {
            FullScreenImage.Source = bitmap;
            ViewModel.IsImageViewerVisible = true;
            Bindings.Update();
        }
    }

    /// <summary> Media preview click - open media player </summary>
    private void OnMediaPreviewClick(object sender, TappedRoutedEventArgs e)
    {
        if (sender is Image img && img.Tag is string mediaPath)
        {
            OpenMediaPlayer(mediaPath);
        }
    }

    // Opens a media file in the system's default player
    private void OpenMediaPlayer(string mediaPath)
    {
        if (string.IsNullOrWhiteSpace(mediaPath) || !File.Exists(mediaPath)) return;

        // Launch default app for this file type
        Task.Run((Func<Task?>)(async () => {
            _ = Windows.System.Launcher.LaunchFileAsync(await Windows.Storage.StorageFile.GetFileFromPathAsync(mediaPath));
        }));
    }

    /// <summary> Exit from full screen image view </summary>
    private void OnFullScreenImageTapped(object sender, RoutedEventArgs e)
    {
        ViewModel.IsImageViewerVisible = false;
        FullScreenImage.Source = null;
        Bindings.Update();
    }

    /// <summary> Click on user </summary>
    public void OnUserClick(object sender, RoutedEventArgs e) => ViewModel.OnUserClick(sender, e);

    #endregion
}
