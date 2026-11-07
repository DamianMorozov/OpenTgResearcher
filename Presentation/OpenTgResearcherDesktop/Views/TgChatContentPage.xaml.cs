namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatContentPage
{
    #region Fields, properties, constructor

    public override TgChatContentViewModel ViewModel { get; }

    public TgChatContentPage()
    {
        ViewModel = App.GetService<TgChatContentViewModel>();
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
        // Прокручиваем ленту до последнего сообщения
        if (ListViewMessages is null) return;
        if (ListViewMessages.Items.Any())
            ListViewMessages.ScrollIntoView(ListViewMessages.Items.Last());
    }

    // Открываем изображение поверх ленты
    private void OnImagePreviewClick(object sender, RoutedEventArgs e)
    {
        if (sender is Image img && img.Source is BitmapImage bitmap)
        {
            FullScreenImage.Source = bitmap;
            ViewModel.IsImageViewerVisible = true;
            Bindings.Update();
        }
    }

    // Открываем оригинал медиафайла вместо просмотра его обложки
    private async void OnMediaPreviewClick(object sender, TappedRoutedEventArgs e)
    {
        if (sender is Image img && img.Tag is string mediaPath)
            await OpenMediaPlayerAsync(mediaPath);
    }

    // Позволяем открыть файл также с клавиатуры и при отсутствии обложки
    private async void OnOpenMediaClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string mediaPath }) await OpenMediaPlayerAsync(mediaPath);
    }

    // Ожидаем результат открытия в системном приложении и показываем понятную ошибку
    private async Task OpenMediaPlayerAsync(string mediaPath, CancellationToken ct = default)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            MediaError.IsOpen = false;
            if (string.IsNullOrWhiteSpace(mediaPath) || !File.Exists(mediaPath))
                throw new FileNotFoundException("Медиафайл не найден", mediaPath);
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(mediaPath);
            if (!await Windows.System.Launcher.LaunchFileAsync(file)) throw new InvalidOperationException("Не удалось открыть медиафайл в системном приложении");
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            MediaError.Message = "ChatOpenMediaFailed".GetLocalizedResource();
            MediaError.IsOpen = true;
        }
    }

    // Закрываем увеличенное изображение
    private void OnFullScreenImageTapped(object sender, RoutedEventArgs e)
    {
        ViewModel.IsImageViewerVisible = false;
        FullScreenImage.Source = null;
        Bindings.Update();
    }

    #endregion
}
