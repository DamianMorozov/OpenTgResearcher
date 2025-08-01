// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDetailsViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

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
    public partial bool EmptyData { get; set; } = true;
    [ObservableProperty]
    public partial Action ScrollRequested { get; set; } = () => { };
    [ObservableProperty]
    public partial bool IsImageViewerVisible { get; set; }
    [ObservableProperty]
    public partial TgEfChatStatisticsDto ChatStatisticsDto { get; set; } = new();
    [ObservableProperty]
    public partial TgEfContentStatisticsDto ContentStatisticsDto { get; set; } = new();
    [ObservableProperty]
    public partial Frame ContentFrame { get; set; } = default!;

    public IRelayCommand UpdateOnlineCommand { get; }
    public IRelayCommand ClearDataStorageCommand { get; }
    public IRelayCommand SaveChatSettingsCommand { get; }
    public IRelayCommand StopDownloadingCommand { get; }

    public TgChatDetailsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgChatDetailsViewModel> logger,
        IAppNotificationService appNotificationService)
        : base(settingsService, navigationService, logger, nameof(TgChatDetailsViewModel))
    {
        AppNotificationService = appNotificationService;
        // Commands
        UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
        ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
        SaveChatSettingsCommand = new AsyncRelayCommand(SaveChatSettingsAsync);
        StopDownloadingCommand = new AsyncRelayCommand(StopDownloadingAsync);
        SetDisplaySensitiveCommand = new AsyncRelayCommand(SetDisplaySensitiveAsync);
        // Updates
        App.BusinessLogicManager.ConnectClient.SetupUpdateStateSource(UpdateStateSource);
    }

    #endregion

    #region Public and private methods

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

        await UpdateCurrentFrameAsync();
    }

    /// <summary> Update view-models at current frame </summary>
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
    }

    private async Task ClearDataStorageAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

    private async Task ClearDataStorageCoreAsync()
    {
        Dto = new();
        MessageDtos = [];
        UserDtos = [];
        EmptyData = !MessageDtos.Any();
        await Task.CompletedTask;
    }

    private async Task LoadDataStorageCoreAsync()
    {
        try
        {
            if (!SettingsService.IsExistsAppStorage) return;

            Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid);

            MessageDtos = [.. await App.BusinessLogicManager.StorageManager.MessageRepository.GetListDtosAsync(
            take: 100, skip: 0, where: x => x.SourceId == Dto.Id, order: x => x.Id)];
            foreach (var messageDto in MessageDtos)
            {
                messageDto.Directory = Dto.Directory;
            }
            EmptyData = !MessageDtos.Any();
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
            IsDownloading = true;
            if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

            await SaveChatSettingsCoreAsync();

            StateSourceDirectory = Dto.Directory;

            await App.BusinessLogicManager.ConnectClient.DownloadAllDataAsync(DownloadSettings);
            await DownloadSettings.SourceVm.SaveAsync();
            await LoadDataStorageCoreAsync();
        }
        finally
        {
            IsDownloading = false;
        }
    }, isDisabledContent: true, isPageLoad: false);

    public async Task SaveChatSettingsAsync() => await ContentDialogAsync(SaveChatSettingsCoreAsync, TgResourceExtensions.AskSettingsSave());

    private async Task SaveChatSettingsCoreAsync()
    {
        var entity = Dto.GetNewEntity();
        DownloadSettings.SourceVm.Fill(entity);
        DownloadSettings.SourceVm.Dto.DtChanged = DateTime.Now;
        await DownloadSettings.SourceVm.SaveAsync();
    }

    private async Task StopDownloadingAsync() => await ContentDialogAsync(StopDownloadingCoreAsync, TgResourceExtensions.AskStopDownloading());

    private async Task StopDownloadingCoreAsync()
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
        App.BusinessLogicManager.ConnectClient.SetForceStopDownloading();
    }

    #endregion
}
