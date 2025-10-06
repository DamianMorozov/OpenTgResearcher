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

    public TgChatDownloadViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgChatDownloadViewModel> logger)
        : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatDownloadViewModel))
    {
        // Commands
        DefaultSettingsCommand = new AsyncRelayCommand(DefaultSettingsAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(() =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        App.VmLocator.ClearChatViewModel();
        App.VmLocator.ClearStateFile();
    });

    private async Task DefaultSettingsAsync() => await ContentDialogAsync(DefaultSettingsCoreAsync, TgResourceExtensions.AskDefaultSettings(), TgEnumLoadDesktopType.Storage);

    private async Task DefaultSettingsCoreAsync()
    {
        var chatEntity = new TgEfSourceEntity();
        DownloadDto.CountThreads = chatEntity.CountThreads;
        DownloadDto.IsAutoUpdate = chatEntity.IsAutoUpdate;
        DownloadDto.IsSaveMessages = chatEntity.IsSaveMessages;
        DownloadDto.IsRewriteMessages = chatEntity.IsRewriteMessages;
        DownloadDto.IsSaveFiles = chatEntity.IsSaveFiles;
        DownloadDto.IsRewriteFiles = chatEntity.IsRewriteFiles;
        DownloadDto.IsJoinFileNameWithMessageId = chatEntity.IsJoinFileNameWithMessageId;
        DownloadDto.IsDownloadThumbnail = chatEntity.IsDownloadThumbnail;
        DownloadDto.IsParsingComments = chatEntity.IsParsingComments;
        DownloadDto.IsCreatingSubdirectories = chatEntity.IsCreatingSubdirectories;
        DownloadDto.IsFileNamingByMessage = chatEntity.IsFileNamingByMessage;
        
        await Task.CompletedTask;
    }

    #endregion
}
