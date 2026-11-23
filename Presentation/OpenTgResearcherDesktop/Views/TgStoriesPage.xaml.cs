namespace OpenTgResearcherDesktop.Views;

public sealed partial class TgStoriesPage
{
    private readonly DispatcherTimer _expirationTimer = new() { Interval = TimeSpan.FromSeconds(15) };
    private int _mediaRevision;
    private StorageFile? _mediaFile;
	#region Fields, properties, constructor

	public override TgStoriesViewModel ViewModel { get; }

	public TgStoriesPage()
	{
		ViewModel = App.GetService<TgStoriesViewModel>();
		
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnStoriesLoaded;
        Unloaded += OnStoriesUnloaded;
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
        if (e.PropertyName is nameof(ViewModel.SelectedStory) or nameof(ViewModel.ArchiveDirectory) or nameof(ViewModel.IsDisplaySensitiveData))
            _ = UpdateMediaAsync();
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

    private void ClearMedia()
    {
        _mediaRevision++;
        _mediaFile = null;
        StoryPhoto.Source = null;
        StoryVideo.MediaPlayer?.Pause();
        StoryVideo.Source = null;
        StoryPhoto.Visibility = StoryVideo.Visibility = OpenStoryMedia.Visibility = StoryMediaNotice.Visibility = StoryMedia.Visibility = Visibility.Collapsed;
    }

    private async Task UpdateMediaAsync()
    {
        ClearMedia();
        var revision = _mediaRevision;
        if (!ViewModel.IsDisplaySensitiveData || ViewModel.SelectedStory?.Dto.MediaPath is not { Length: > 0 } relativePath)
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
            }
            else if (TgStoryMediaArchive.IsVideo(path))
            {
                StoryVideo.Source = Windows.Media.Core.MediaSource.CreateFromStorageFile(file);
                StoryVideo.Visibility = Visibility.Visible;
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
