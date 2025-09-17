namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDetailsInfoViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgChatDetailsDto ChatDetailsDto { get; set; } = new();

    public IAsyncRelayCommand UpdateChatSettingsCommand { get; }

    public TgChatDetailsInfoViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILoadStateService loadStateService, 
        ILogger<TgChatDetailsInfoViewModel> logger) : base(settingsService, navigationService, loadStateService, logger, nameof(TgChatDetailsInfoViewModel))
    {
        // Commands
        UpdateChatSettingsCommand = new AsyncRelayCommand(UpdateChatSettingsAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        await LoadDataStorageCoreAsync();
    });

    protected override async Task SetDisplaySensitiveAsync()
    {
        ChatDetailsDto.IsDisplaySensitiveData = IsDisplaySensitiveData;

        await Task.CompletedTask;
    }

    private async Task LoadDataStorageCoreAsync()
    {
        try
        {
            if (!SettingsService.IsExistsAppStorage) return;
        }
        finally
        {
            await ReloadUiAsync();
        }
    }

    private async Task UpdateChatSettingsAsync() => await ContentDialogAsync(UpdateChatSettingsCoreAsync, TgResourceExtensions.AskUpdateChatDetails(), TgEnumLoadDesktopType.Online);

    private async Task UpdateChatSettingsCoreAsync() => await LoadStorageDataAsync(async () =>
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

        ChatDetailsDto = await App.BusinessLogicManager.ConnectClient.GetChatDetailsByClientAsync(Uid);
    });

    #endregion
}
