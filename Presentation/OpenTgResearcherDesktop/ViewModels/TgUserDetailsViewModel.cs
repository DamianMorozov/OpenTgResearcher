namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgUserDetailsViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfUserDto Dto { get; set; } = default!;
    [ObservableProperty]
    public partial List<long> ListIds { get; set; } = [];
    [ObservableProperty]
    public partial ObservableCollection<TgEfChatWithCountDto> ChatsDtos { get; set; } = [];
    [ObservableProperty]
    public partial bool IsImageViewerVisible { get; set; }

    public IAsyncRelayCommand LoadDataStorageCommand { get; }
    public IAsyncRelayCommand ClearViewCommand { get; }
    public IAsyncRelayCommand StartUpdateOnlineCommand { get; }
    public IAsyncRelayCommand<TgEfSourceDto> LoadUserMessagesCommand { get; }

    public TgUserDetailsViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgUserDetailsViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgUserDetailsViewModel))
    {
        // Commands
        ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
        LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
        StartUpdateOnlineCommand = new AsyncRelayCommand(StartUpdateOnlineAsync);
        LoadUserMessagesCommand = new AsyncRelayCommand<TgEfSourceDto>(LoadUserMessagesAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
        {
            Uid = Guid.Empty;
            if (e?.Parameter is Guid uid)
            {
                Uid = uid;
            }
            await LoadDataStorageCoreAsync();
            await ReloadUiAsync();
        });

    private async Task ClearViewAsync() => 
        await ContentDialogAsync(ClearDataStorageCore, TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

    private void ClearDataStorageCore() => Dto = new();

    private async Task LoadDataStorageAsync() => 
        await ContentDialogAsync(LoadDataStorageCoreAsync, TgResourceExtensions.AskLoading(), TgEnumLoadDesktopType.Storage);

    private async Task LoadDataStorageCoreAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        ChatsDtos.Clear();
        ListIds.Clear();

        Dto = await App.BusinessLogicManager.StorageManager.UserRepository.GetDtoAsync(x => x.Uid == Uid);
        if (Dto is null) return;

        ListIds = await App.BusinessLogicManager.StorageManager.MessageRepository
            .GetListDtosAsync(0, 0, x => x.UserId == Dto.Id)
            .ContinueWith(t => t.Result.Select(m => m.SourceId).Distinct().ToList());
        if (ListIds.Count == 0) return;

        foreach (var chatId in ListIds)
        {
            var chatDto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Id == chatId);
            if (chatDto is null) continue;
            var messagesCount = await App.BusinessLogicManager.StorageManager.MessageRepository.GetCountAsync(x => x.UserId == Dto.Id && x.SourceId == chatDto.Id);
            ChatsDtos.Add(new TgEfChatWithCountDto(Dto, chatDto, messagesCount));
        }

        // Order
        ChatsDtos = [.. ChatsDtos.OrderBy(x => x.ChatDto.UserName).ThenBy(x => x.ChatDto.Title)];
    }

    private async Task StartUpdateOnlineAsync() => 
        await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline(), TgEnumLoadDesktopType.Online);

    private async Task UpdateOnlineCoreAsync() => await LoadOnlineDataAsync(async () =>
    {
        try
        {
            if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

            await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.UserContact, [Dto.Id]);

            await LoadDataStorageCoreAsync();
        }
        finally
        {
            await LoadDataStorageCoreAsync();
        }
    });

    private async Task LoadUserMessagesAsync(TgEfSourceDto? chat) =>
        await ContentDialogAsync(async () => await LoadUserMessagesCoreAsync(chat), TgResourceExtensions.AskLoading(), TgEnumLoadDesktopType.Storage);

    /// <summary> Load user messages in chat </summary>
    private async Task LoadUserMessagesCoreAsync(TgEfSourceDto? chat)
    {
        if (chat is null) return;

        try
        {
            var chatDto = ChatsDtos.FirstOrDefault(x => x.ChatDto.Id == chat.Id);
            if (chatDto is null) return;

            chatDto.Messages.Clear();
            chatDto.Messages = [.. await App.BusinessLogicManager.StorageManager.MessageRepository
                .GetListDtosAsync(0, 0, where: x => x.SourceId == chat.Id && x.UserId == Dto.Id,
                order: x => x.Id)];
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    internal async Task OnLoadMessagesClickAsync(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is TgEfSourceDto chatDto)
        {
            await LoadUserMessagesCommand.ExecuteAsync(chatDto);
            return;
        }

        if (sender is TgLicenseButton licenseButton && licenseButton.Tag is TgEfSourceDto chatDto2)
        {
            await LoadUserMessagesCommand.ExecuteAsync(chatDto2);
            return;
        }
    }

    #endregion
}
