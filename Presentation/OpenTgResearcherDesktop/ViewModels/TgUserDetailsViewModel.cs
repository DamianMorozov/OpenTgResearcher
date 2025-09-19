namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
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
    public partial bool IsMessagesLoaded { get; set; }
    [ObservableProperty]
    public partial TgEfChatWithCountDto? SelectedChat { get; set; }

    public IAsyncRelayCommand LoadDataStorageCommand { get; }
    public IAsyncRelayCommand ClearViewCommand { get; }
    public IAsyncRelayCommand UpdateOnlineCommand { get; }
    public IAsyncRelayCommand<TgEfChatWithCountDto> LoadMessagesCommand { get; }

    public TgUserDetailsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILoadStateService loadStateService,
        ILogger<TgUserDetailsViewModel> logger) : base(settingsService, navigationService, loadStateService, logger, nameof(TgUserDetailsViewModel))
    {
        // Commands
        ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
        LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
        UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
        LoadMessagesCommand = new AsyncRelayCommand<TgEfChatWithCountDto>(LoadMessagesAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
        {
            Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
            await LoadDataStorageCoreAsync();
            await ReloadUiAsync();
        });

    private async Task ClearViewAsync() => await ContentDialogAsync(ClearDataStorageCore, TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

    private void ClearDataStorageCore() => Dto = new();

    private async Task LoadDataStorageAsync() => await ContentDialogAsync(LoadDataStorageCoreAsync, TgResourceExtensions.AskLoading(), TgEnumLoadDesktopType.Storage);

    // TODO: Add new table CHATS_USERS
    private async Task LoadDataStorageCoreAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        ChatsDtos.Clear();
        ListIds.Clear();

        Dto = await App.BusinessLogicManager.StorageManager.UserRepository.GetDtoAsync(x => x.Uid == Uid);
        if (Dto is null) return;

        //ListIds = await App.BusinessLogicManager.StorageManager.MessageRepository.GetListDtosAsync(0, 0, x => x.UserId == Dto.Id)).Select(x => x.SourceId).Distinct();
        ListIds = await App.BusinessLogicManager.StorageManager.MessageRepository
            .GetListDtosAsync(0, 0, x => x.UserId == Dto.Id)
            .ContinueWith(t => t.Result.Select(m => m.SourceId).Distinct().ToList());
        if (ListIds.Count == 0) return;

        foreach (var chatId in ListIds)
        {
            var chatDto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Id == chatId);
            if (chatDto is null) continue;
            var countMessages = await App.BusinessLogicManager.StorageManager.MessageRepository.GetCountAsync(x => x.UserId == Dto.Id && x.SourceId == chatDto.Id);
            ChatsDtos.Add(new TgEfChatWithCountDto(Dto, chatDto, countMessages));
        }

        // Order
        ChatsDtos = [.. ChatsDtos.OrderBy(x => x.ChatDto.UserName).ThenBy(x => x.ChatDto.Title)];
    }

    private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline(), TgEnumLoadDesktopType.Online);

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

    private async Task LoadMessagesAsync(TgEfChatWithCountDto? chat)
    {
        if (chat is null) return;

        //var parent = ChatsDtos.FirstOrDefault(u => ChatsDtos.Contains(chat));
        //if (parent == null || parent.IsMessagesLoaded) return;

        //parent.SelectedChat = chat;

        //var msgs = await App.BusinessLogicManager.StorageManager.MessageRepository
        //    .GetListDtosAsync(0, 0, x => x.SourceId == chat.Id && x.UserId == parent.UserDto.Id);

        //parent.MessageDtos.Clear();
        //foreach (var m in msgs.OrderBy(m => m.Id))
        //    parent.MessageDtos.Add(m);

        //parent.IsMessagesLoaded = true;
        await Task.CompletedTask;
    }

    #endregion
}
