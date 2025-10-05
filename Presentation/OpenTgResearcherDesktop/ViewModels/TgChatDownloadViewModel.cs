namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDownloadViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto DownloadDto { get; set; } = default!;
    [ObservableProperty]
    public partial string ChatProgressMessage { get; set; } = string.Empty;

    public IAsyncRelayCommand DefaultSettingsCommand { get; }

    public TgChatDownloadViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILoadStateService loadStateService, 
        ILogger<TgChatDownloadViewModel> logger)
        : base(settingsService, navigationService, loadStateService, logger, nameof(TgChatDownloadViewModel))
    {
        // Callback updates UI
        App.BusinessLogicManager.ConnectClient.SetupUpdateChatViewModel(UpdateChatViewModelAsync);
        App.BusinessLogicManager.ConnectClient.SetupUpdateStateFile(UpdateStateFileAsync);
        // Commands
        DefaultSettingsCommand = new AsyncRelayCommand(DefaultSettingsAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(() =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        ClearChatViewModel();
        ClearStateFile();
    });

    private async Task DefaultSettingsAsync() => await ContentDialogAsync(DefaultSettingsCoreAsync, TgResourceExtensions.AskDefaultSettings(), TgEnumLoadDesktopType.Storage);

    private async Task DefaultSettingsCoreAsync()
    {
        var chatEntity = new TgEfSourceEntity();
        DownloadDto.CountThreads = chatEntity.CountThreads;
        
        await Task.CompletedTask;
    }

    #endregion
}
