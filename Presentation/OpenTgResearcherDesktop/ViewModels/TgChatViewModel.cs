namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgChatViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto Dto { get; set; } = default!;
    [ObservableProperty]
    public partial TgEfSourceDto DiscussionDto { get; set; } = default!;
    [ObservableProperty]
    public partial Frame ChatDetailsFrame { get; set; } = default!;

    public IAsyncRelayCommand LoadDataStorageCommand { get; }
    public IAsyncRelayCommand StartUpdateOnlineCommand { get; }
    public IAsyncRelayCommand ClearViewCommand { get; }
    public IAsyncRelayCommand SaveChatSettingsCommand { get; }
    public IAsyncRelayCommand StopUpdateOnlineCommand { get; }

    public TgChatViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgChatViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatViewModel))
    {
        // Commands
        LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageCoreAsync);
        StartUpdateOnlineCommand = new AsyncRelayCommand(StartUpdateOnlineAsync);
        ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
        SaveChatSettingsCommand = new AsyncRelayCommand(SaveChatSettingsAsync);
        StopUpdateOnlineCommand = new AsyncRelayCommand(StopUpdateOnlineAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        await LoadDataStorageCoreAsync();
    });

    private async Task ClearViewAsync() => 
        await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

    private async Task ClearDataStorageCoreAsync()
    {
        Dto = new();
        DiscussionDto = new();
        IsEmptyData = false;
        await Task.CompletedTask;
    }

    private async Task LoadDataStorageCoreAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        await TgDesktopUtils.InvokeOnUIThreadAsync(async () => {
            try
            {
                Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid);
                DiscussionDto = await App.BusinessLogicManager.StorageManager.SourceRepository.FindCommentDtoSourceAsync(Dto.Id);
            
                if (ChatDetailsFrame is not null)
                {
                    var pageViewModel = await ChatDetailsFrame.GetPageViewModelAsync();
                    if (pageViewModel is not null)
                    {
                        if (pageViewModel is TgChatSettingsViewModel chatSettingsViewModel)
                        {
                            chatSettingsViewModel.Uid = Uid;
                            chatSettingsViewModel.Dto = Dto;
                            chatSettingsViewModel.DiscussionDto = DiscussionDto;
                            chatSettingsViewModel.IsDiscussionDtoExists = DiscussionDto.Id > 0;
                            ChatDetailsFrame.Navigate(typeof(TgChatSettingsPage), Uid);
                        }
                        else if (pageViewModel is TgChatDownloadViewModel chatDownloadViewModel)
                        {
                            chatDownloadViewModel.Uid = Uid;
                            chatDownloadViewModel.DownloadDto = Dto;
                            ChatDetailsFrame.Navigate(typeof(TgChatDownloadPage), Uid);
                        }
                    }
                }

                IsEmptyData = false;
            }
            finally
            {
                await ReloadUiAsync();
            }
        });
    }

    private async Task StartUpdateOnlineAsync() => 
        await ContentDialogAsync(StartUpdateOnlineCoreAsync, TgResourceExtensions.AskStartParseTelegram(), TgEnumLoadDesktopType.Online);

    private async Task StartUpdateOnlineCoreAsync() => 
        await LoadOnlineDataAsync(async () =>
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

    private async Task StopUpdateOnlineAsync() => 
        await ContentDialogAsync(StopUpdateOnlineCoreAsync, TgResourceExtensions.AskStopParseTelegram(), TgEnumLoadDesktopType.Online);

    private async Task StopUpdateOnlineCoreAsync()
    {
        LoadStateService.StopHardOnlineProcessing();
        LoadStateService.StopHardDownloadProcessing();
        await Task.CompletedTask;
    }

    #endregion
}
