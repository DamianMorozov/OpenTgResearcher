namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatContentViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial IAppNotificationService AppNotificationService { get; private set; }

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto Dto { get; set; } = null!;
    [ObservableProperty]
    public partial ObservableCollection<TgEfMessageDto> MessageDtos { get; set; } = new();
    [ObservableProperty]
    public partial ObservableCollection<TgEfUserDto> UserDtos { get; set; } = new();
    [ObservableProperty]
    public partial Action ScrollRequested { get; set; } = () => { };
    [ObservableProperty]
    public partial bool IsImageViewerVisible { get; set; }
    [ObservableProperty]
    public partial TgEfContentStatisticsDto ContentStatisticsDto { get; set; } = new();
    [ObservableProperty]
    public partial string LoadedDataStatistics { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int PageSize { get; set; } = 100;
    [ObservableProperty]
    public partial int PageSkip { get; set; }
    [ObservableProperty]
    public partial bool HasMoreMessages { get; set; } = true;
    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public IAsyncRelayCommand CalcContentStatisticsCommand { get; }
    public IAsyncRelayCommand LazyLoadMessagesCommand { get; }
    public IAsyncRelayCommand ClearViewCommand { get; }

    public TgChatContentViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgChatContentViewModel> logger, IAppNotificationService appNotificationService)
        : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatContentViewModel))
    {
        AppNotificationService = appNotificationService;
        // Commands
        CalcContentStatisticsCommand = new AsyncRelayCommand(CalcContentStatisticsAsync);
        LazyLoadMessagesCommand = new AsyncRelayCommand(LazyLoadMessagesAsync, () => HasMoreMessages && !IsLoading);
        ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        await LoadDataStorageCoreAsync();
    });

    private async Task LoadDataStorageCoreAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        try
        {
            PageSkip = 0;
            HasMoreMessages = true;
            MessageDtos.Clear();
            UserDtos.Clear();

            Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid);

            await LazyLoadMessagesAsync();
        }
        finally
        {
            IsEmptyData = !MessageDtos.Any();
            ScrollRequested?.Invoke();

            await ReloadUiAsync();
        }
    }

    public async Task LazyLoadMessagesAsync() => await LoadStorageDataAsync(async () =>
    {
        if (IsLoading || !HasMoreMessages) return;

        try
        {
            IsLoading = true;

            if (UserDtos.Count == 0)
                UserDtos = [.. await App.BusinessLogicManager.StorageManager.UserRepository.GetListDtosAsync(take: 0, skip: 0, where: x => x.Id > 0, order: x => x.Id)];

            // Get total count of messages for current chat
            var totalCount = await App.BusinessLogicManager.StorageManager.MessageRepository.GetCountAsync(x => x.SourceId == Dto.Id);

            // Load newest messages first by ordering descending
            var newItems = await App.BusinessLogicManager.StorageManager.MessageRepository.GetListDtosAsync(take: PageSize, skip: PageSkip,
                where: x => x.SourceId == Dto.Id, order: x => x.Id, isOrderDesc: true);

            // Update skip counter for next portion
            PageSkip += newItems.Count;

            // Check if there are more messages to load
            HasMoreMessages = PageSkip < totalCount;

            // Insert new items at the beginning to keep newest at top
            foreach (var item in newItems)
            {
                item.Directory = Dto.Directory;
                item.UserContact = UserDtos.FirstOrDefault(u => u.Id == item.UserId)?.DisplayName ?? string.Empty;

                MessageDtos.Insert(0, item);
            }

            (LazyLoadMessagesCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        }
        finally
        {
            await AfterDataUpdateAsync();
        }
    });

    private async Task ClearViewAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

    private async Task ClearDataStorageCoreAsync()
    {
        try
        {
            PageSkip = 0;
            HasMoreMessages = true;
            MessageDtos.Clear();
        }
        finally
        {
            await AfterDataUpdateAsync();
        }
    }

    private async Task AfterDataUpdateAsync()
    {
        IsLoading = false;
        (LazyLoadMessagesCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();

        // Update loaded data statistics
        var totalCount = await App.BusinessLogicManager.StorageManager.MessageRepository.GetCountAsync(x => x.SourceId == Dto.Id);
        LoadedDataStatistics = $"{TgResourceExtensions.GetTextBlockFiltered()} {MessageDtos.Count} | " + $"{TgResourceExtensions.GetTextBlockTotalAmount()} {totalCount}";

        await Task.CompletedTask;
    }

    private async Task CalcContentStatisticsAsync() => 
        await ContentDialogAsync(CalcContentStatisticsCoreAsync, TgResourceExtensions.AskCalcContentStatistics(), TgEnumLoadDesktopType.Storage);
    
    private async Task CalcContentStatisticsCoreAsync()
    {
        ContentStatisticsDto.Default();

        if (Directory.Exists(Dto.Directory))
        {
            var files = Directory.GetFiles(Dto.Directory);
            if (files.Length != 0)
            {
                var imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" };
                var audioExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a", ".wma", ".alac", ".aiff", ".ape", ".opus" };
                var videoExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm" };

                ContentStatisticsDto.ImagesCount = files.Count(file => imageExtensions.Contains(Path.GetExtension(file)));
                ContentStatisticsDto.AudiosCount = files.Count(file => audioExtensions.Contains(Path.GetExtension(file)));
                ContentStatisticsDto.VideosCount = files.Count(file => videoExtensions.Contains(Path.GetExtension(file)));
            }
        }

        await Task.CompletedTask;
    }

    #endregion
}
