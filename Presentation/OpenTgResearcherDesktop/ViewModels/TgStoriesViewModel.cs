namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgStoriesViewModel : TgPageViewModelBase
{
    private List<TgEfStoryDto> _stories = [];
    private DateTime _nextExpirationUtc = DateTime.MaxValue;
    private string DirectorySetting => "StoriesDirectory-" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
        Encoding.UTF8.GetBytes(Path.GetFullPath(SettingsService.AppStorage).ToUpperInvariant())));

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsJournalEmpty), nameof(LoadedCount))]
    public partial List<TgStoryJournalRow> Rows { get; private set; } = [];

    [ObservableProperty]
    public partial List<TgStoryJournalGroup> Groups { get; private set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    public partial TgStoryJournalRow? SelectedStory { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int FilterIndex { get; set; }

    [ObservableProperty]
    public partial int SortIndex { get; set; }

    [ObservableProperty]
    public partial string AuthorUsername { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveArchiveDirectoryCommand))]
    public partial string ArchiveDirectory { get; private set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasArchiveDirectoryInput))]
    [NotifyCanExecuteChangedFor(nameof(SaveArchiveDirectoryCommand))]
    public partial string ArchiveDirectoryInput { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ArchiveMessage { get; private set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsArchiveIdle))]
    [NotifyCanExecuteChangedFor(nameof(SaveArchiveDirectoryCommand))]
    public partial bool IsArchiveBusy { get; private set; }

    public bool IsArchiveIdle => !IsArchiveBusy;
    public bool HasArchiveDirectoryInput => !string.IsNullOrWhiteSpace(ArchiveDirectoryInput);
    public bool IsJournalEmpty => Rows.Count == 0;
    public bool HasSelection => SelectedStory is not null;
    public string LoadedCount => string.Format("StoriesShown".GetLocalizedResource(), Rows.Count, _stories.Count);
    public IAsyncRelayCommand LoadDataStorageCommand { get; }
    public IAsyncRelayCommand ClearViewCommand { get; }
    public IAsyncRelayCommand StartUpdateOnlineCommand { get; }
    public IAsyncRelayCommand DownloadAuthorCommand { get; }
    public IRelayCommand SaveArchiveDirectoryCommand { get; }
    public IRelayCommand CancelArchiveCommand { get; }

    public TgStoriesViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService,
        ILogger<TgStoriesViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgStoriesViewModel))
    {
        ClearViewCommand = new AsyncRelayCommand(() =>
            ContentDialogAsync(ClearView, TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage));
        LoadDataStorageCommand = new AsyncRelayCommand(() => LoadStorageDataAsync(LoadDataStorageCoreAsync));
        StartUpdateOnlineCommand = new AsyncRelayCommand(ct => SyncArchiveAsync(forAuthor: false, ct));
        DownloadAuthorCommand = new AsyncRelayCommand(ct => SyncArchiveAsync(forAuthor: true, ct));
        SaveArchiveDirectoryCommand = new RelayCommand(SaveArchiveDirectory, CanSaveArchiveDirectory);
        CancelArchiveCommand = new RelayCommand(() =>
        {
            StartUpdateOnlineCommand.Cancel();
            DownloadAuthorCommand.Cancel();
        });
    }

    public override Task OnNavigatedToAsync(NavigationEventArgs? e) => ReloadUiAsync();

    public override Task ReloadUiAsync() => LoadStorageDataAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(ArchiveDirectory) && !string.IsNullOrWhiteSpace(SettingsService.AppStorage))
        {
            var storage = Path.GetFullPath(SettingsService.AppStorage);
            ArchiveDirectoryInput = ArchiveDirectory = SettingsService.ReadSetting<string>(DirectorySetting)
                ?? Path.Combine(Path.GetDirectoryName(storage)!, Path.GetFileNameWithoutExtension(storage) + ".Stories");
        }
        await LoadDataStorageCoreAsync();
        await base.ReloadUiAsync();
    });

    private bool CanSaveArchiveDirectory() =>
        IsArchiveIdle && !string.IsNullOrWhiteSpace(SettingsService.AppStorage) && ArchiveDirectoryInput != ArchiveDirectory;

    private void SaveArchiveDirectory()
    {
        if (!CanSaveArchiveDirectory() || !IsDisplaySensitiveData) return;

        var path = ArchiveDirectoryInput.Trim().Trim('"').Trim();
        if (!Path.IsPathFullyQualified(path) || path.IndexOfAny(['<', '>', '"', '|', '?', '*']) >= 0)
        {
            ArchiveMessage = "StoriesDirectoryInvalid".GetLocalizedResource();
            return;
        }

        try
        {
            path = Path.GetFullPath(path);
            if (File.Exists(path))
            {
                ArchiveMessage = "StoriesDirectoryIsFile".GetLocalizedResource();
                return;
            }
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            ArchiveMessage = "StoriesDirectoryInvalid".GetLocalizedResource();
            return;
        }

        try
        {
            SettingsService.SaveSettingOrThrow(DirectorySetting, path);
            ArchiveDirectoryInput = ArchiveDirectory = path;
            ArchiveMessage = "StoriesDirectorySaved".GetLocalizedResource();
        }
        catch (Exception ex)
        {
            LogError(ex);
            ArchiveMessage = "StoriesDirectorySaveError".GetLocalizedResource();
        }
    }

    public void RefreshExpiration()
    {
        if (DateTime.UtcNow >= _nextExpirationUtc)
            RefreshJournal();
    }

    public void RefreshJournal()
    {
        var now = DateTime.UtcNow;
        var query = SearchText.Trim();
        var rows = new List<TgStoryJournalRow>();
        var selectedUid = SelectedStory?.Uid;
        _nextExpirationUtc = DateTime.MaxValue;
        foreach (var story in _stories)
        {
            var expiresUtc = TgStoryJournalRow.ToUtc(story.ExpireDate);
            if (expiresUtc > now && expiresUtc < _nextExpirationUtc)
                _nextExpirationUtc = expiresUtc;
            var isActive = expiresUtc > now;
            var isExpired = expiresUtc != DateTime.MinValue && !isActive;
            if ((FilterIndex == 1 && !isActive) || (FilterIndex == 2 && !isExpired) || (FilterIndex == 3 && !story.IsDeleted))
                continue;
            if (story.IsDeleted && FilterIndex is 1 or 2)
                continue;
            var row = new TgStoryJournalRow(story, isActive, isExpired);
            if (query.Length == 0 || row.Author.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                || story.Caption.Contains(query, StringComparison.CurrentCultureIgnoreCase))
                rows.Add(row);
        }
        var groups = rows.GroupBy(row => row.Dto.FromId).Select(group =>
            new TgStoryJournalGroup(group.First().Author, SortIndex == 2 ? group.OrderByDescending(row => row.Dto.Id) : group)).ToList();
        Groups = SortIndex switch
        {
            1 => [.. groups.OrderBy(group => group.Header, StringComparer.CurrentCultureIgnoreCase)],
            2 => [.. groups.OrderByDescending(group => group[0].Dto.Id)],
            _ => groups,
        };
        Rows = [.. Groups.SelectMany(group => group)];
        SelectedStory = Rows.FirstOrDefault(row => row.Uid == selectedUid) ?? Rows.FirstOrDefault();
    }

    partial void OnSearchTextChanged(string value) => RefreshJournal();
    partial void OnFilterIndexChanged(int value) => RefreshJournal();
    partial void OnSortIndexChanged(int value) => RefreshJournal();

    private void ClearView()
    {
        _stories = [];
        RefreshJournal();
    }

    private async Task LoadDataStorageCoreAsync()
    {
        if (!SettingsService.IsExistsAppStorage)
        {
            ClearView();
            return;
        }

        // Для чтения во время фонового парсинга нужен отдельный контекст
        using var reader = IsOnlineProcessing ? new TgEfStoryRepository() : null;
        var stories = await (reader ?? App.BusinessLogicManager.StorageManager.StoryRepository).GetListDtosAsync();
        _stories = [.. stories.OrderByDescending(story => TgStoryJournalRow.ToUtc(story.Date))
            .ThenByDescending(story => story.DtChanged).ThenByDescending(story => story.Id).ThenBy(story => story.FromId)];
        RefreshJournal();
    }

    private async Task SyncArchiveAsync(bool forAuthor, CancellationToken ct)
    {
        if (IsArchiveBusy || !SettingsService.IsExistsAppStorage || ArchiveDirectory.Length == 0) return;
        var selected = forAuthor ? SelectedStory?.Dto : null;
        var username = forAuthor ? AuthorUsername.Trim() : null;
        if (forAuthor && string.IsNullOrEmpty(username) && (selected?.FromId ?? 0) <= 0)
        {
            ArchiveMessage = "StoriesChooseAuthor".GetLocalizedResource();
            return;
        }

        IsArchiveBusy = true;
        ArchiveMessage = string.Empty;
        try
        {
            await LoadOnlineDataAsync(async () =>
            {
                var client = App.BusinessLogicManager.ConnectClient;
                if (!await client.CheckClientConnectionReadyAsync())
                {
                    ArchiveMessage = "StoriesConnectFirst".GetLocalizedResource();
                    return;
                }
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, LoadStateService.OnlineToken);
                var result = await client.SyncStoryArchiveAsync(ArchiveDirectory, selected, username, linked.Token);
                ArchiveMessage = string.Format("StoriesSyncResult".GetLocalizedResource(), result.Stories, result.Downloaded, result.Failed);
            });
        }
        catch (OperationCanceledException)
        {
            ArchiveMessage = "StoriesSyncCancelled".GetLocalizedResource();
        }
        catch (Exception ex)
        {
            LogError(ex);
            ArchiveMessage = IsDisplaySensitiveData ? ex.Message : "StoriesSyncError".GetLocalizedResource();
        }
        finally
        {
            try { await LoadStorageDataAsync(LoadDataStorageCoreAsync); }
            finally { IsArchiveBusy = false; }
        }
    }
}

public sealed class TgStoryJournalGroup(string header, IEnumerable<TgStoryJournalRow> rows) : ObservableCollection<TgStoryJournalRow>(rows)
{
    public string Header { get; } = header;
}

public sealed class TgStoryJournalRow(TgEfStoryDto story, bool isActive, bool isExpired)
{
    public TgEfStoryDto Dto => story;
    public Guid Uid => story.Uid;
    public string Id => story.Id.ToString(CultureInfo.InvariantCulture);
    public bool IsActive => isActive && !story.IsDeleted;
    public string Author => !string.IsNullOrWhiteSpace(story.FromName) ? story.FromName
        : story.FromId > 0 ? $"ID {story.FromId}" : "StoriesUnknownAuthor".GetLocalizedResource();
    public string Caption => string.IsNullOrWhiteSpace(story.Caption) ? "StoriesNoCaption".GetLocalizedResource() : story.Caption;
    public string Published => FormatDate(story.Date);
    public string Expires => FormatDate(story.ExpireDate);
    public string Status => (story.IsDeleted ? "StoriesDeleted" : isActive ? "StoriesActive" : isExpired ? "StoriesExpired" : "StoriesUnknownExpiry")
        .GetLocalizedResource();

    public override string ToString() => $"{Author}, {Published}, {Status}";

    // SQLite может вернуть даты Telegram без признака UTC
    internal static DateTime ToUtc(DateTime date) => date.Kind == DateTimeKind.Local
        ? date.ToUniversalTime() : DateTime.SpecifyKind(date, DateTimeKind.Utc);

    private static string FormatDate(DateTime date) => date == DateTime.MinValue
        ? "—" : ToUtc(date).ToLocalTime().ToString("g", CultureInfo.CurrentCulture);
}
