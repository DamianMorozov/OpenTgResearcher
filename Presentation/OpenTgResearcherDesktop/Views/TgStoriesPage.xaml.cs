namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgStoriesPage
{
    private readonly DispatcherTimer _expirationTimer = new() { Interval = TimeSpan.FromSeconds(15) };
    private int _mediaRevision;
    private StorageFile? _mediaFile;
    private Microsoft.UI.Xaml.Controls.Primitives.Popup? _mediaPopup;
	#region Fields, properties, constructor

	public override TgStoriesViewModel ViewModel { get; }

	public TgStoriesPage()
	{
		ViewModel = App.GetService<TgStoriesViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnStoriesLoaded;
        Unloaded += OnStoriesUnloaded;
        StoryMediaSurface.AddHandler(TappedEvent, new TappedEventHandler(OnStoryMediaTapped), handledEventsToo: true);
        var closeMedia = new KeyboardAccelerator { Key = VirtualKey.Escape };
        closeMedia.Invoked += OnCloseMediaInvoked;
        KeyboardAccelerators.Add(closeMedia);
        _expirationTimer.Tick += (_, _) => ViewModel.RefreshExpiration();
	}

	#endregion

    private void OnStoriesLoaded(object sender, RoutedEventArgs e)
    {
        PageLoaded(sender, e);
        ViewModel.RefreshJournal();
        Localizer.Get().LanguageChanged += OnLanguageChanged;
        ViewModel.PropertyChanged += OnStoryChanged;
        _expirationTimer.Start();
        _ = UpdateMediaAsync();
    }

    private void OnStoriesUnloaded(object sender, RoutedEventArgs e)
    {
        _expirationTimer.Stop();
        Localizer.Get().LanguageChanged -= OnLanguageChanged;
        ViewModel.PropertyChanged -= OnStoryChanged;
        ClearMedia();
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e) => DispatcherQueue.TryEnqueue(ViewModel.RefreshJournal);

    private void OnStoryChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ViewModel.SelectedStory) or nameof(ViewModel.ArchiveDirectory) or nameof(ViewModel.IsDisplaySensitiveData)
            or nameof(ViewModel.IsClearingArchive))
            _ = UpdateMediaAsync();
    }

    // Запрашиваем подтверждение очистки и отдельно предлагаем удалить файлы
    private async void OnClearArchiveClick(object sender, RoutedEventArgs e)
    {
        if (XamlRoot is null || !ViewModel.IsArchiveIdle) return;

        var includeFiles = new CheckBox { Content = "StoriesClearIncludeFiles".GetLocalizedResource() };
        var content = new StackPanel { Spacing = 8 };
        content.Children.Add(new TextBlock { Text = "StoriesClearConfirm".GetLocalizedResource(), TextWrapping = TextWrapping.Wrap });
        content.Children.Add(includeFiles);

        try
        {
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = "StoriesClearTitle".GetLocalizedResource(),
                Content = content,
                PrimaryButtonText = "StoriesClearConfirmButton".GetLocalizedResource(),
                CloseButtonText = TgResourceExtensions.GetCancelButton(),
                DefaultButton = ContentDialogButton.Close,
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                await ViewModel.ClearArchiveAsync(includeFiles: includeFiles.IsChecked == true);
        }
        catch (Exception ex) { ViewModel.LogError(ex); }
    }

    private async void OnChooseFolderClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new Microsoft.Windows.Storage.Pickers.FolderPicker(App.MainWindow.AppWindow.Id);
            var folder = await picker.PickSingleFolderAsync();
            if (folder is not null && ViewModel.IsArchiveIdle && ViewModel.IsDisplaySensitiveData)
                ViewModel.ArchiveDirectoryInput = folder.Path;
        }
        catch (Exception ex) { ViewModel.LogError(ex); }
    }

    // Переключаем просмотр по области изображения, оставляя кнопки и ползунки плеера доступными
    private void OnStoryMediaTapped(object sender, TappedRoutedEventArgs e)
    {
        for (var element = e.OriginalSource as DependencyObject; element is not null && element != StoryMediaSurface;
            element = VisualTreeHelper.GetParent(element))
        {
            if (element is Microsoft.UI.Xaml.Controls.Primitives.ButtonBase or Slider) return;
        }

        if (_mediaPopup is not null)
            CloseMediaPreview();
        else if (XamlRoot is { } root && _mediaFile is not null && ViewModel.IsDisplaySensitiveData)
            OpenMediaPreview(root);
        e.Handled = true;
    }

    // Возвращаем медиа в карточку по Esc только при открытом просмотре
    private void OnCloseMediaInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (_mediaPopup is not { IsOpen: true }) return;

        CloseMediaPreview();
        args.Handled = true;
    }

    // Переносим ту же поверхность просмотра, чтобы сохранить источник и позицию видео
    private void OpenMediaPreview(XamlRoot root)
    {
        StoryMediaHost.Children.Remove(StoryMediaSurface);
        StoryPhoto.MaxHeight = double.PositiveInfinity;
        StoryVideo.Height = double.NaN;
        StoryMediaSurface.Background = new SolidColorBrush(Microsoft.UI.Colors.Black);
        _mediaPopup = new() { XamlRoot = root, Child = StoryMediaSurface };
        ResizeMediaPreview(root, null);
        root.Changed += ResizeMediaPreview;
        _mediaPopup.IsOpen = true;
    }

    // Подгоняем просмотр под клиентскую область при изменении размеров окна
    private void ResizeMediaPreview(XamlRoot root, XamlRootChangedEventArgs? args)
    {
        StoryMediaSurface.Width = root.Size.Width;
        StoryMediaSurface.Height = root.Size.Height;
    }

    // Возвращаем поверхность и её исходные размеры в карточке сторис
    private void CloseMediaPreview()
    {
        if (_mediaPopup is not { } popup) return;

        _mediaPopup = null;
        if (popup.XamlRoot is { } root) root.Changed -= ResizeMediaPreview;
        popup.IsOpen = false;
        popup.Child = null;
        StoryMediaSurface.ClearValue(WidthProperty);
        StoryMediaSurface.ClearValue(HeightProperty);
        StoryMediaSurface.ClearValue(Panel.BackgroundProperty);
        StoryPhoto.ClearValue(MaxHeightProperty);
        StoryVideo.ClearValue(HeightProperty);
        StoryMediaHost.Children.Add(StoryMediaSurface);
    }

    private void ClearMedia()
    {
        CloseMediaPreview();
        _mediaRevision++;
        _mediaFile = null;
        StoryPhoto.Source = null;
        StoryVideo.MediaPlayer?.Pause();
        StoryVideo.Source = null;
        StoryMediaHost.Visibility = Visibility.Collapsed;
        StoryPhoto.Visibility = StoryVideo.Visibility = OpenStoryMedia.Visibility = StoryMediaNotice.Visibility = StoryMedia.Visibility = Visibility.Collapsed;
    }

    private async Task UpdateMediaAsync()
    {
        ClearMedia();
        var revision = _mediaRevision;
        if (ViewModel.IsClearingArchive || !ViewModel.IsDisplaySensitiveData || ViewModel.SelectedStory?.Dto.MediaPath is not { Length: > 0 } relativePath)
            return;
        try
        {
            var path = TgStoryMediaArchive.ResolvePath(ViewModel.ArchiveDirectory, relativePath);
            if (!File.Exists(path))
            {
                ShowMediaNotice("StoriesMissingMedia");
                return;
            }
            if (!TgStoryMediaArchive.IsPhoto(path) && !TgStoryMediaArchive.IsVideo(path))
            {
                ShowMediaNotice("StoriesUnsupportedMedia");
                return;
            }
            var file = await StorageFile.GetFileFromPathAsync(path);
            if (revision != _mediaRevision) return;
            _mediaFile = file;
            OpenStoryMedia.Visibility = StoryMedia.Visibility = Visibility.Visible;
            if (TgStoryMediaArchive.IsPhoto(path))
            {
                using var stream = await file.OpenReadAsync();
                var photo = new BitmapImage { DecodePixelHeight = 640 };
                await photo.SetSourceAsync(stream);
                if (revision != _mediaRevision) return;
                StoryPhoto.Source = photo;
                StoryPhoto.Visibility = Visibility.Visible;
                StoryMediaHost.Visibility = Visibility.Visible;
            }
            else if (TgStoryMediaArchive.IsVideo(path))
            {
                StoryVideo.Source = Windows.Media.Core.MediaSource.CreateFromStorageFile(file);
                StoryVideo.Visibility = Visibility.Visible;
                StoryMediaHost.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            ViewModel.LogError(ex);
            if (revision == _mediaRevision) ShowMediaNotice("StoriesMediaError");
        }
    }

    private void ShowMediaNotice(string resource)
    {
        StoryMediaNotice.Text = resource.GetLocalizedResource();
        StoryMedia.Visibility = StoryMediaNotice.Visibility = Visibility.Visible;
    }

    private async void OnOpenMediaClick(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.IsDisplaySensitiveData || _mediaFile is not { } file) return;
        try
        {
            if (!await Launcher.LaunchFileAsync(file) && ReferenceEquals(_mediaFile, file))
                ShowMediaNotice("StoriesMediaError");
        }
        catch (Exception ex)
        {
            ViewModel.LogError(ex);
            if (ReferenceEquals(_mediaFile, file)) ShowMediaNotice("StoriesMediaError");
        }
    }
}
