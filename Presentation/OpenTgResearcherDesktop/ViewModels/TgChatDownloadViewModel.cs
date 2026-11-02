namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgChatDownloadViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto DownloadDto { get; set; } = new();
    [ObservableProperty]
    public partial string ChatProgressMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string ChatProgressInfo { get; set; } = string.Empty;

    public IAsyncRelayCommand DefaultSettingsCommand { get; }

    public TgChatDownloadViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgChatDownloadViewModel> logger)
        : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatDownloadViewModel))
    {
        // Команда восстановления настроек загрузки
        DefaultSettingsCommand = new AsyncRelayCommand(DefaultSettingsAsync);
    }

    #endregion

    #region Methods

    /// <summary> Подготавливает настройки загрузки до обращения к прогрессу чата </summary>
    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => 
        await LoadStorageDataAsync(async () =>
        {
            DownloadDto = e?.Parameter as TgEfSourceDto ?? new();
            Uid = e?.Parameter is Guid uid ? uid : DownloadDto.Uid;
            if (SettingsService.IsExistsAppStorage && e?.Parameter is not TgEfSourceDto)
                DownloadDto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid);
            App.VmLocator.ClearChatViewModel();
            App.VmLocator.ClearStateDownloadingChat();
        });

    private async Task DefaultSettingsAsync() => 
        await ContentDialogAsync(DefaultSettingsCoreAsync, TgResourceExtensions.AskDefaultSettings(), TgEnumLoadDesktopType.Storage);

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
