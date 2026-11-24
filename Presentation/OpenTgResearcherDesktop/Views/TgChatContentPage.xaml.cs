namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgChatContentPage
{
    private const double VideoHeight = 270;
    private const double PlayerWidth = 480;
    private int _mediaRevision;
    private Grid? _mediaHost;
    private MediaPlayerElement? _player;
    private TgWindowMediaPopup? _videoPopup;

    #region Fields, properties, constructor

    public override TgChatContentViewModel ViewModel { get; }

    public TgChatContentPage()
    {
        ViewModel = App.GetService<TgChatContentViewModel>();
        ViewModel.ScrollRequested = ScrollRequested;

        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnPageLoaded;
        Unloaded += OnPageUnloaded;
        var closeMedia = new KeyboardAccelerator { Key = VirtualKey.Escape };
        closeMedia.Invoked += OnCloseMediaInvoked;
        KeyboardAccelerators.Add(closeMedia);
    }

    #endregion

    #region Methods

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        PageLoaded(sender, e);
        ViewModel.PropertyChanged += OnViewModelChanged;
    }

    // Освобождаем проигрыватель и изображение при уходе с вкладки
    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.PropertyChanged -= OnViewModelChanged;
        CloseInlinePlayer();
        CloseImagePreview();
    }

    // Сбрасываем медиа при скрытии данных и снимок при смене чата или очистке ленты
    private void OnViewModelChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.IsDisplaySensitiveData) && !ViewModel.IsDisplaySensitiveData)
        {
            CloseInlinePlayer();
            CloseImagePreview();
        }
        if (e.PropertyName == nameof(ViewModel.IsImageViewerVisible) && !ViewModel.IsImageViewerVisible)
            FullScreenImage.Source = null;
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
        if (ViewModel.IsDisplaySensitiveData && sender is Image img && img.Source is BitmapImage bitmap)
        {
            CloseInlinePlayer();
            FullScreenImage.Source = bitmap;
            ViewModel.IsImageViewerVisible = true;
            Bindings.Update();
        }
    }

    // Открываем оригинал из обложки во встроенном проигрывателе, для других файлов сохраняем системное открытие
    private async void OnMediaPreviewClick(object sender, TappedRoutedEventArgs e)
    {
        if (sender is Image { Tag: string path } image)
        {
            if (TgChatMediaPlayback.CanPlay(path)) await PlayInlineAsync(image, path);
            else await OpenMediaPlayerAsync(path);
        }
    }

    // Открываем встроенный проигрыватель с кнопки, в том числе если у файла нет обложки
    private async void OnPlayMediaClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string path } button) await PlayInlineAsync(button, path);
    }

    // Создаём один проигрыватель для выбранного сообщения и проверяем, что контейнер не переиспользован
    private async Task PlayInlineAsync(FrameworkElement trigger, string path)
    {
        if (!ViewModel.IsDisplaySensitiveData || trigger.Parent is not StackPanel { Parent: Grid host } poster) return;

        CloseInlinePlayer();
        MediaError.IsOpen = false;
        _mediaHost = host;
        var revision = _mediaRevision;
        try
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            if (revision != _mediaRevision) return;

            var isVideo = TgChatMediaPlayback.IsVideo(path);
            var player = new MediaPlayerElement
            {
                AutoPlay = true, AreTransportControlsEnabled = true, Stretch = Stretch.Uniform,
                MaxWidth = PlayerWidth, Height = isVideo ? VideoHeight : 108,
            };
            _player = player;
            if (isVideo) player.AddHandler(TappedEvent, new TappedEventHandler(OnVideoTapped), handledEventsToo: true);
            host.Children.Add(player);
            poster.Visibility = Visibility.Collapsed;
            player.Source = Windows.Media.Core.MediaSource.CreateFromStorageFile(file);
            player.MediaPlayer.MediaFailed += OnInlineMediaFailed;
        }
        catch (Exception ex)
        {
            ViewModel.LogError(ex, "Не удалось открыть встроенный проигрыватель");
            if (revision == _mediaRevision) ShowInlineMediaError();
        }
    }

    // Обрабатываем ошибки декодирования, которые приходят с потока медиаплеера
    private void OnInlineMediaFailed(Windows.Media.Playback.MediaPlayer sender, Windows.Media.Playback.MediaPlayerFailedEventArgs args)
    {
        var message = args.ErrorMessage;
        DispatcherQueue.TryEnqueue(() =>
        {
            if (_player?.MediaPlayer != sender) return;
            ViewModel.LogWarning(message);
            ShowInlineMediaError();
        });
    }

    // Возвращаем обложку при ошибке и оставляем доступным системное открытие файла
    private void ShowInlineMediaError()
    {
        CloseInlinePlayer();
        MediaError.Message = "ChatInlineMediaFailed".GetLocalizedResource();
        MediaError.IsOpen = true;
    }

    // Переключаем размер видео по клику на кадр, сохраняя проигрыватель и его позицию
    private void OnVideoTapped(object sender, TappedRoutedEventArgs e)
    {
        if (_player is not { } player || _mediaHost is not { } host || TgWindowMediaPopup.IsPlayerControl(e, player)) return;

        if (_videoPopup is not null)
            CloseExpandedVideo();
        else if (XamlRoot is { } root)
        {
            host.Children.Remove(player);
            player.MaxWidth = double.PositiveInfinity;
            player.Height = double.NaN;
            _videoPopup = new(root, player);
        }
        e.Handled = true;
    }

    // Возвращаем видео из увеличенного просмотра в сообщение
    private void CloseExpandedVideo()
    {
        if (_videoPopup is not { } popup || _player is not { } player) return;

        _videoPopup = null;
        popup.Close();
        player.MaxWidth = PlayerWidth;
        player.Height = VideoHeight;
        _mediaHost?.Children.Add(player);
    }

    // Останавливаем воспроизведение и освобождаем файл при закрытии карточки
    private void CloseInlinePlayer()
    {
        _mediaRevision++;
        _videoPopup?.Close();
        _videoPopup = null;
        if (_player is { } player)
        {
            _player = null;
            if (player.MediaPlayer is { } playback)
            {
                playback.MediaFailed -= OnInlineMediaFailed;
                playback.Pause();
            }
            player.Source = null;
            _mediaHost?.Children.Remove(player);
        }
        if (_mediaHost?.Children.FirstOrDefault() is StackPanel poster) poster.Visibility = Visibility.Visible;
        _mediaHost = null;
    }

    // Сбрасываем проигрыватель, когда виртуализация убирает сообщение или подставляет другое
    private void OnMediaHostUnloaded(object sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(_mediaHost, sender)) CloseInlinePlayer();
    }

    // При повторном использовании контейнера отменяем даже ещё не начатое воспроизведение
    private void OnMediaHostContextChanged(FrameworkElement sender, DataContextChangedEventArgs e)
    {
        if (ReferenceEquals(_mediaHost, sender)) CloseInlinePlayer();
    }

    // Esc закрывает фото, возвращает увеличенное видео в сообщение или сворачивает вложенный проигрыватель
    private void OnCloseMediaInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (_videoPopup is not null) CloseExpandedVideo();
        else if (ViewModel.IsImageViewerVisible) CloseImagePreview();
        else if (_mediaHost is not null) CloseInlinePlayer();
        else return;
        args.Handled = true;
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
    private void OnFullScreenImageTapped(object sender, RoutedEventArgs e) => CloseImagePreview();

    // Убираем слой увеличенного изображения и освобождаем его источник
    private void CloseImagePreview()
    {
        ViewModel.IsImageViewerVisible = false;
        FullScreenImage.Source = null;
        Bindings.Update();
    }

    #endregion
}
