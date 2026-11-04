namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgChatInfoViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgChatDetailsDto ChatDetailsDto { get; set; } = new();

    public IAsyncRelayCommand UpdateChatSettingsCommand { get; }

    public TgChatInfoViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgChatInfoViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatInfoViewModel))
    {
        // Команда обновления сведений
        UpdateChatSettingsCommand = new AsyncRelayCommand(UpdateChatSettingsAsync);
    }

    #endregion

    #region Methods

    /// <summary> Открывает чат со сбросом сведений предыдущего чата </summary>
    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        ChatDetailsDto = new();
        await LoadDataStorageCoreAsync();
        if (Uid != Guid.Empty)
            await UpdateChatSettingsCoreAsync();
    });

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

    private async Task UpdateChatSettingsAsync() => 
        await ContentDialogAsync(UpdateChatSettingsCoreAsync, TgResourceExtensions.AskUpdateChatDetails(), TgEnumLoadDesktopType.Online);

    // Сообщаем о сетевой загрузке и при автоматическом открытии, без диалога команды
    private async Task UpdateChatSettingsCoreAsync() =>
        await LoadOnlineDataAsync(async () =>
        {
            if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

            ChatDetailsDto = await App.BusinessLogicManager.ConnectClient.GetChatDetailsByClientAsync(Uid);
        });

    #endregion
}
