// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDetailsContentViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial IAppNotificationService AppNotificationService { get; private set; }

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto Dto { get; set; } = null!;
    [ObservableProperty]
    public partial TgChatDetailsDto ChatDetailsDto { get; set; } = new();
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
    public partial int CurrentSkip { get; set; }
    [ObservableProperty]
    public partial bool HasMoreMessages { get; set; } = true;
    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public IRelayCommand CalcContentStatisticsCommand { get; }
    public IRelayCommand LazyLoadMessagesCommand { get; }
    public IRelayCommand ClearDataStorageCommand { get; }

    public TgChatDetailsContentViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgChatDetailsContentViewModel> logger,
        IAppNotificationService appNotificationService)
        : base(settingsService, navigationService, logger, nameof(TgChatDetailsContentViewModel))
    {
        AppNotificationService = appNotificationService;
        // Commands
        SetDisplaySensitiveCommand = new AsyncRelayCommand(SetDisplaySensitiveAsync);
        CalcContentStatisticsCommand = new AsyncRelayCommand(CalcContentStatisticsAsync);
        LazyLoadMessagesCommand = new AsyncRelayCommand(LazyLoadMessagesAsync, () => HasMoreMessages && !IsLoading);
        ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
        {
            Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
            await LoadDataStorageCoreAsync();
        });

    private async Task SetDisplaySensitiveAsync()
    {
        foreach (var userDto in UserDtos)
        {
            userDto.IsDisplaySensitiveData = IsDisplaySensitiveData;
        }

        foreach (var messageDto in MessageDtos)
        {
            messageDto.IsDisplaySensitiveData = IsDisplaySensitiveData;
        }

        await Task.CompletedTask;
    }

    private async Task LoadDataStorageCoreAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        try
        {
            CurrentSkip = 0;
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
            await SetDisplaySensitiveAsync();
        }
    }

    public async Task LazyLoadMessagesAsync() => await LoadDataAsync(async () =>
    {
        if (IsLoading || !HasMoreMessages) return;

        try
        {
            IsLoading = true;

            if (UserDtos.Count == 0)
                UserDtos = [.. await App.BusinessLogicManager.StorageManager.UserRepository.GetListDtosAsync(take: 0, skip: 0, where: x => x.Id > 0, order: x => x.Id)];

            //var newItems = await GetListLiteDtosAsync(PageSize, CurrentSkip, FilterText, IsFilterBySubscribe, IsFilterById, IsFilterByUserName, IsFilterByTitle);
            var newItems = await App.BusinessLogicManager.StorageManager.MessageRepository.GetListDtosWithoutRelationsAsync(
                PageSize, CurrentSkip, where: x => x.SourceId == Dto.Id, order: x => x.Id);
            if (newItems.Count < PageSize)
                HasMoreMessages = false;

            foreach (var item in newItems)
            {
                item.IsDisplaySensitiveData = IsDisplaySensitiveData;
                item.Directory = Dto.Directory;
                item.UserContact = UserDtos.FirstOrDefault(x => x.Id == item.UserId)?.GetDisplayName() ?? string.Empty;
                MessageDtos.Add(item);
            }

            CurrentSkip += newItems.Count;
        }
        finally
        {
            await AfterDataUpdateAsync();
        }
    });

    private async Task ClearDataStorageAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

    private async Task ClearDataStorageCoreAsync()
    {
        try
        {
            CurrentSkip = 0;
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
        var countAll = await App.BusinessLogicManager.StorageManager.MessageRepository.GetCountAsync(x => x.SourceId == Dto.Id);
        LoadedDataStatistics =
            $"{TgResourceExtensions.GetTextBlockFiltered()} {MessageDtos.Count} | " +
            $"{TgResourceExtensions.GetTextBlockTotalAmount()} {countAll}";

        await Task.CompletedTask;
    }

    private async Task CalcContentStatisticsAsync() => await ContentDialogAsync(CalcContentStatisticsCoreAsync, TgResourceExtensions.AskCalcContentStatistics());
    
    private async Task CalcContentStatisticsCoreAsync()
    {
        ContentStatisticsDto.DefaultValues();

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

    /// <summary> Click on user </summary>
    internal void OnUserClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        var tag = button.Tag.ToString();
        if (string.IsNullOrEmpty(tag)) return;

        Tuple<long, long> tuple = new(long.Parse(tag), Dto.Id);
        NavigationService.NavigateTo(typeof(TgUserDetailsViewModel).FullName!, tuple);
    }

    #endregion
}
