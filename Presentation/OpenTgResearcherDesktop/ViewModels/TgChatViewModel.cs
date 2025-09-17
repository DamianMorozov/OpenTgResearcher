namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial IAppNotificationService AppNotificationService { get; private set; }

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto Dto { get; set; } = default!;
    [ObservableProperty]
    public partial TgEfSourceDto DiscussionDto { get; set; } = default!;
    [ObservableProperty]
    public partial bool IsDiscussionDtoExists { get; set; }
    [ObservableProperty]
    public partial TgChatDetailsDto ChatDetailsDto { get; set; } = new();
    [ObservableProperty]
    public partial ObservableCollection<TgEfUserDto> UserDtos { get; set; } = new();
    [ObservableProperty]
    public partial Action ScrollRequested { get; set; } = () => { };
    [ObservableProperty]
    public partial Frame ContentFrame { get; set; } = default!;
    [ObservableProperty]
    public partial string ChatProgressMessage { get; set; } = string.Empty;

    public IAsyncRelayCommand UpdateOnlineCommand { get; }
    public IAsyncRelayCommand ClearViewCommand { get; }
    public IAsyncRelayCommand SaveChatSettingsCommand { get; }
    public IAsyncRelayCommand StopDownloadingCommand { get; }

    public TgChatViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILoadStateService loadStateService, 
        ILogger<TgChatViewModel> logger, IAppNotificationService appNotificationService)
        : base(settingsService, navigationService, loadStateService, logger, nameof(TgChatViewModel))
    {
        AppNotificationService = appNotificationService;
        // Commands
        UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
        ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
        SaveChatSettingsCommand = new AsyncRelayCommand(SaveChatSettingsAsync);
        StopDownloadingCommand = new AsyncRelayCommand(StopDownloadingAsync);
        // Callback updates UI
        App.BusinessLogicManager.ConnectClient.SetupUpdateChatsViewModel(UpdateChatsViewModelAsync);
        App.BusinessLogicManager.ConnectClient.SetupUpdateChatViewModel(UpdateChatViewModelAsync);
        App.BusinessLogicManager.ConnectClient.SetupUpdateStateFile(UpdateStateFileAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        await LoadDataStorageCoreAsync();
    });

    public override async Task ReloadUiAsync()
    {
        await base.ReloadUiAsync();

        if (ContentFrame is not null && ContentFrame.GetPageViewModel() is TgPageViewModelBase pageViewModelBase)
        {
            await LoadStorageDataAsync(pageViewModelBase.ReloadUiAsync);
        }
    }

    protected override async Task SetDisplaySensitiveAsync()
    {
        foreach (var userDto in UserDtos)
        {
            userDto.IsDisplaySensitiveData = IsDisplaySensitiveData;
        }

        if (ContentFrame.GetPageViewModel() is TgPageViewModelBase pageViewModelBase)
        {
            pageViewModelBase.IsDisplaySensitiveData = IsDisplaySensitiveData;
            await pageViewModelBase.ReloadUiAsync();
        }

        await Task.CompletedTask;
    }

    private async Task ClearViewAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

    private async Task ClearDataStorageCoreAsync()
    {
        Dto = new();
        DiscussionDto = new();
        IsDiscussionDtoExists = DiscussionDto.Id > 0;
        UserDtos = [];
        IsEmptyData = false;
        await Task.CompletedTask;
    }

    private async Task LoadDataStorageCoreAsync()
    {
        try
        {
            if (!SettingsService.IsExistsAppStorage) return;

            Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid);
            DiscussionDto = await App.BusinessLogicManager.StorageManager.SourceRepository.FindCommentDtoSourceAsync(Dto.Id);
            IsDiscussionDtoExists = DiscussionDto.Id > 0;

            IsEmptyData = false;
            ScrollRequested?.Invoke();
        }
        finally
        {
            await ReloadUiAsync();
            await SetDisplaySensitiveAsync();
        }
    }

    private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline(), TgEnumLoadDesktopType.Online);

    private async Task UpdateOnlineCoreAsync() => await LoadOnlineDataAsync(async () =>
    {
        try
        {
            if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

            await SaveChatSettingsCoreAsync(isLoadDataStorage: false);

            await App.BusinessLogicManager.ConnectClient.ParseChatAsync(DownloadSettings);
        }
        finally
        {
            await LoadDataStorageCoreAsync();
        }
    });

    public async Task SaveChatSettingsAsync() => 
        await ContentDialogAsync(() => SaveChatSettingsCoreAsync(isLoadDataStorage: true), TgResourceExtensions.AskSettingsSave(), TgEnumLoadDesktopType.Storage);

    private async Task SaveChatSettingsCoreAsync(bool isLoadDataStorage)
    {
        try
        {
            // Update and save current chat
            Dto.DtChanged = DateTime.UtcNow;
            DownloadSettings.SourceVm.Dto = Dto;
            var entity = Dto.GetEntity();
            await App.BusinessLogicManager.StorageManager.SourceRepository.SaveAsync(entity);

            // Update and save discussion chat if exists
            if (IsDiscussionDtoExists)
            {
                DiscussionDto.DtChanged = DateTime.UtcNow;
                var comment = DiscussionDto.GetEntity();
                await App.BusinessLogicManager.StorageManager.SourceRepository.SaveAsync(comment);
            }
        }
        finally
        {
            if (isLoadDataStorage)
                await LoadDataStorageCoreAsync();
        }
    }

    private async Task StopDownloadingAsync() => await ContentDialogAsync(StopDownloadingCoreAsync, TgResourceExtensions.AskStopLoading(), TgEnumLoadDesktopType.Online);

    private async Task StopDownloadingCoreAsync() => await App.BusinessLogicManager.ConnectClient.SetForceStopDownloadingAsync();

    #endregion
}
