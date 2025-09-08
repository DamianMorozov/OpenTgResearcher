// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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

    public IRelayCommand UpdateOnlineCommand { get; }
    public IRelayCommand ClearDataStorageCommand { get; }
    public IRelayCommand SaveChatSettingsCommand { get; }
    public IRelayCommand StopDownloadingCommand { get; }

    public TgChatViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgChatViewModel> logger,
        IAppNotificationService appNotificationService)
        : base(settingsService, navigationService, logger, nameof(TgChatViewModel))
    {
        AppNotificationService = appNotificationService;
        // Commands
        UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
        ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
        SaveChatSettingsCommand = new AsyncRelayCommand(SaveChatSettingsAsync);
        StopDownloadingCommand = new AsyncRelayCommand(StopDownloadingAsync);
        SetDisplaySensitiveCommand = new AsyncRelayCommand(SetDisplaySensitiveAsync);
        // Callback updates UI
        App.BusinessLogicManager.ConnectClient.SetupUpdateChatsViewModel(UpdateChatsViewModelAsync);
        App.BusinessLogicManager.ConnectClient.SetupUpdateChatViewModel(UpdateChatViewModelAsync);
        App.BusinessLogicManager.ConnectClient.SetupUpdateStateFile(UpdateStateFileAsync);
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

        // Update ViewModels at current frame
        await UpdateCurrentFrameAsync();
    }

    /// <summary> Update ViewModels current frame </summary>
    private async Task UpdateCurrentFrameAsync()
    {
        // Chat details
        if (ContentFrame.GetPageViewModel() is TgChatDetailsInfoViewModel chatDetailsInfoViewModel)
        {
            chatDetailsInfoViewModel.IsDisplaySensitiveData = IsDisplaySensitiveData;
            await chatDetailsInfoViewModel.ReloadUiAsync();
        }

        // Chat participants
        if (ContentFrame.GetPageViewModel() is TgChatDetailsParticipantsViewModel chatDetailsParticipantsViewModel)
        {
            chatDetailsParticipantsViewModel.IsDisplaySensitiveData = IsDisplaySensitiveData;
            await chatDetailsParticipantsViewModel.ReloadUiAsync();
        }

        // Chat statistics
        if (ContentFrame.GetPageViewModel() is TgChatDetailsStatisticsViewModel chatDetailsStatisticsViewModel)
        {
            chatDetailsStatisticsViewModel.IsDisplaySensitiveData = IsDisplaySensitiveData;
            await chatDetailsStatisticsViewModel.ReloadUiAsync();
        }

        // Content
        if (ContentFrame.GetPageViewModel() is TgChatDetailsContentViewModel chatDetailsContentViewModel)
        {
            chatDetailsContentViewModel.IsDisplaySensitiveData = IsDisplaySensitiveData;
            await chatDetailsContentViewModel.ReloadUiAsync();
        }
    }

    private async Task ClearDataStorageAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

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

    private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline());

    private async Task UpdateOnlineCoreAsync() => await ProcessDataAsync(async () =>
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
    }, isDisabledContent: true, isPageLoad: false);

    public async Task SaveChatSettingsAsync() => await ContentDialogAsync(
        async () => await SaveChatSettingsCoreAsync(isLoadDataStorage: true), TgResourceExtensions.AskSettingsSave());

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

    private async Task StopDownloadingAsync() => await ContentDialogAsync(StopDownloadingCoreAsync, TgResourceExtensions.AskStopDownloading());

    private async Task StopDownloadingCoreAsync() => await App.BusinessLogicManager.ConnectClient.SetForceStopDownloadingAsync();

    #endregion
}
