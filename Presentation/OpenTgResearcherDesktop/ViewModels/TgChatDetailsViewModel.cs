// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDetailsViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto Dto { get; set; } = null!;
    [ObservableProperty]
    public partial TgChatDetailsDto ChatDetailsDto { get; set; } = new();
    [ObservableProperty]
    public partial ObservableCollection<TgEfMessageDto> Messages { get; set; } = null!;
    [ObservableProperty]
    public partial bool EmptyData { get; set; } = true;
    [ObservableProperty]
    public partial Action ScrollRequested { get; set; } = () => { };
    [ObservableProperty]
    public partial bool IsImageViewerVisible { get; set; }

    public IRelayCommand ClearDataStorageCommand { get; }
    public IRelayCommand UpdateOnlineCommand { get; }
    public IRelayCommand UpdateChatDetailsCommand { get; }
    public IRelayCommand StopDownloadingCommand { get; }

    public TgChatDetailsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgChatDetailsViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgChatDetailsViewModel))
    {
        // Commands
        ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
        UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
        UpdateChatDetailsCommand = new AsyncRelayCommand(UpdateChatDetailsAsync);
        StopDownloadingCommand = new AsyncRelayCommand(StopDownloadingAsync);
        // Updates
        App.BusinessLogicManager.ConnectClient.SetupUpdateStateSource(UpdateStateSource);
    }

    #endregion

    #region Public and private methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
        {
            Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
            await LoadDataStorageCoreAsync();
            await ReloadUiAsync();
        });

    private async Task ClearDataStorageAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

    private async Task ClearDataStorageCoreAsync()
    {
        Dto = new();
        Messages = [];
        EmptyData = !Messages.Any();
        await Task.CompletedTask;
    }

    private async Task LoadDataStorageCoreAsync()
    {
        await ReloadUiAsync();
        if (!SettingsService.IsExistsAppStorage) return;
        Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid);
        Messages = [.. await App.BusinessLogicManager.StorageManager.MessageRepository.GetListDtosDescAsync(
            take: 100, skip: 0, where: x => x.SourceId == Dto.Id, order: x => x.Id, isReadOnly: true)];
        foreach (var messageDto in Messages)
        {
            messageDto.Directory = Dto.Directory;
        }
        EmptyData = !Messages.Any();
        ScrollRequested?.Invoke();
    }

    private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline());

    private async Task UpdateOnlineCoreAsync() => await LoadDataAsync(async () =>
    {
        IsDownloading = true;
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
        var entity = Dto.GetNewEntity();
        DownloadSettings.SourceVm.Fill(entity);
        DownloadSettings.SourceVm.Dto.DtChanged = DateTime.Now;
        await DownloadSettings.UpdateSourceWithSettingsAsync();

        StateSourceDirectory = Dto.Directory;

        await App.BusinessLogicManager.ConnectClient.DownloadAllDataAsync(DownloadSettings);
        await DownloadSettings.UpdateSourceWithSettingsAsync();
        //await BusinessLogicManager.ConnectClient.UpdateStateSourceAsync(DownloadSettings.SourceVm.Dto.Id, DownloadSettings.SourceVm.Dto.FirstId, TgLocale.SettingsSource);
        await LoadDataStorageCoreAsync();
        IsDownloading = false;
    });

    private async Task UpdateChatDetailsAsync() => await ContentDialogAsync(UpdateChatDetailsCoreAsync, TgResourceExtensions.AskUpdateChatDetails());

    private async Task UpdateChatDetailsCoreAsync() => await LoadDataAsync(async () =>
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

        ChatDetailsDto = await App.BusinessLogicManager.ConnectClient.GetChatDetailsByClientAsync(Dto.Id);
    });

    private async Task StopDownloadingAsync() => await ContentDialogAsync(StopDownloadingCoreAsync, TgResourceExtensions.AskStopDownloading());

    private async Task StopDownloadingCoreAsync()
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
        App.BusinessLogicManager.ConnectClient.SetForceStopDownloading();
    }

    #endregion
}