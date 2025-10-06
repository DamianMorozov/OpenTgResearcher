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
    public partial Frame ChatDetailsFrame { get; set; } = default!;

    public IAsyncRelayCommand LoadDataStorageCommand { get; }
    public IAsyncRelayCommand UpdateOnlineCommand { get; }
    public IAsyncRelayCommand ClearViewCommand { get; }
    public IAsyncRelayCommand SaveChatSettingsCommand { get; }
    public IAsyncRelayCommand StopDownloadingCommand { get; }

    public TgChatViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgChatViewModel> logger, IAppNotificationService appNotificationService)
        : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatViewModel))
    {
        AppNotificationService = appNotificationService;
        // Commands
        LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageCoreAsync);
        UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
        ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
        SaveChatSettingsCommand = new AsyncRelayCommand(SaveChatSettingsAsync);
        StopDownloadingCommand = new AsyncRelayCommand(StopDownloadingAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        await LoadDataStorageCoreAsync();
    });

    private async Task ClearViewAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

    private async Task ClearDataStorageCoreAsync()
    {
        Dto = new();
        DiscussionDto = new();
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
            
            if (ChatDetailsFrame is not null && ChatDetailsFrame.GetPageViewModel() is TgChatSettingsViewModel chatSettingsViewModel)
            {
                chatSettingsViewModel.Dto = Dto;
                chatSettingsViewModel.DiscussionDto = DiscussionDto;
                chatSettingsViewModel.IsDiscussionDtoExists = DiscussionDto.Id > 0;
            }
            if (ChatDetailsFrame is not null && ChatDetailsFrame.GetPageViewModel() is TgChatDownloadViewModel chatDetailsDownloadViewModel)
                chatDetailsDownloadViewModel.DownloadDto = Dto;


            IsEmptyData = false;
        }
        finally
        {
            await ReloadUiAsync();
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
            var sourceEntity = TgEfDomainUtils.CreateNewEntity(Dto, isUidCopy: true);
            await App.BusinessLogicManager.StorageManager.SourceRepository.SaveAsync(sourceEntity);
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
