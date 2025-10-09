namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgChatParticipantsViewModel : TgSectionViewModel, IDisposable
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial IAppNotificationService AppNotificationService { get; private set; }

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto Dto { get; set; } = null!;
    [ObservableProperty]
    public partial ObservableCollection<TgEfUserDto> UserDtos { get; set; } = new();

    public IAsyncRelayCommand LoadParticipantsCommand { get; }
    public IAsyncRelayCommand StopParticipantsCommand { get; }
    public IAsyncRelayCommand GetParticipantsCommand { get; }
    public IAsyncRelayCommand GetParticipantsFromMessagesCommand { get; }
    public IAsyncRelayCommand ClearParticipantsCommand { get; }

    public TgChatParticipantsViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgChatParticipantsViewModel> logger, IAppNotificationService appNotificationService)
        : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatParticipantsViewModel))
    {
        AppNotificationService = appNotificationService;
        // Commands
        LoadParticipantsCommand = new AsyncRelayCommand(LoadParticipantsAsync);
        StopParticipantsCommand = new AsyncRelayCommand(StopParticipantsAsync);
        GetParticipantsCommand = new AsyncRelayCommand(GetParticipantsAsync);
        GetParticipantsFromMessagesCommand = new AsyncRelayCommand(GetParticipantsFromMessagesAsync);
        ClearParticipantsCommand = new AsyncRelayCommand(ClearParticipantsAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        //await LoadParticipantsCoreAsync();
        await Task.CompletedTask;
    });

    private async Task LoadParticipantsAsync() => 
        await ContentDialogAsync(() => LazyLoadCoreAsync(isSearch: false), TgResourceExtensions.AskLoading(), TgEnumLoadDesktopType.Storage);

    protected override async Task LazyLoadCoreAsync(bool isSearch)
    {
        if (!HasMoreItems) return;

        var newItems = await GetListDtosAsync();
        if (newItems.Count < PageTake)
            HasMoreItems = false;

        UserDtos = isSearch ? [.. newItems] : [.. UserDtos, .. newItems];

        PageSkip += newItems.Count;
    }

    public async Task<List<TgEfUserDto>> GetListDtosAsync()
    {
        var uid = Guid.NewGuid();
        try
        {
            if (!SettingsService.IsExistsAppStorage) return [];

            await LoadStateService.PrepareMemoryTokenAsync(uid);

            // Load source DTO
            Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid, LoadStateService.MemoryToken);
            await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);

            // Single query to get all distinct users for this source
            var userDtos = await App.BusinessLogicManager.StorageManager.UserRepository.GetUsersByChatIdJoinAsync(
                Dto.Id, App.BusinessLogicManager.ConnectClient.Client?.UserId ?? 0, PageSkip, PageTake, null, LoadStateService.MemoryToken);

            IsEmptyData = UserDtos.Count == 0;
            return userDtos;
        }
        finally
        {
            LoadStateService.StopSoftMemoryProcessing(uid);
            await ReloadUiAsync();
        }
    }

    private async Task StopParticipantsAsync() => 
        await ContentDialogAsync(StopParticipantsCoreAsync, TgResourceExtensions.AskStopLoading(), TgEnumLoadDesktopType.Storage);

    private async Task StopParticipantsCoreAsync()
    {
        try
        {
            LoadStateService.StopHardAllProcessing();
        }
        finally
        {
            await ReloadUiAsync();
        }
    }

    private async Task GetParticipantsAsync() => 
        await ContentDialogAsync(GetParticipantsCoreAsync, TgResourceExtensions.AskGetParticipants(), TgEnumLoadDesktopType.Online);

    private async Task GetParticipantsCoreAsync()
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

        var uid = Guid.NewGuid();
        try
        {
            await LoadStateService.PrepareStorageTokenAsync(uid);

            // Load source DTO
            Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid, LoadStateService.StorageToken);
            await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);

            // Online query: fetch participants into memory
            var participantDtos = await App.BusinessLogicManager.ConnectClient.GetParticipantsAsync(Dto.Id);
            // If no online participants found - still try to fill from storage
            var participantIds = (participantDtos is null || participantDtos.Count == 0) ? new List<long>() : [.. participantDtos.Select(x => x.Id).Distinct()];
            if (participantDtos is null || participantDtos.Count == 0)
            {
                // Nothing to save, just reload from storage
                await LazyLoadCoreAsync(isSearch: false);
                return;
            }

            // Map to user DTOs (in-memory)
            var incomingUserDtos = participantDtos.Select(x => new TgEfUserDto
            {
                Id = x.User.id,
                IsUserActive = x.User.IsActive,
                IsBot = x.User.IsBot,
                LastSeenAgo = x.User.LastSeenAgo,
                UserName = x.User.MainUsername,
                AccessHash = x.User.access_hash,
                FirstName = x.User.first_name,
                LastName = x.User.last_name,
                UserNames = x.User.usernames is null ? string.Empty : string.Join("|", x.User.usernames.ToList()),
                PhoneNumber = x.User.phone,
                Status = x.User.status?.ToString() ?? string.Empty,
                RestrictionReason = x.User.restriction_reason is null ? string.Empty : string.Join("|", x.User.restriction_reason.ToList()),
                LangCode = x.User.lang_code,
                IsContact = false,
            }).ToList();

            // Update local cache of users (safely ignore cancellation here)
            if (incomingUserDtos.Count > 0)
                await App.BusinessLogicManager.ConnectClient.UpdateUsersAsync([.. incomingUserDtos]);

            //// Read user ids from ChatUsers table linked to this chat (storage)
            //var chatUserIds = await App.BusinessLogicManager.StorageManager.EfContext.ChatUsers
            //    .AsNoTracking()
            //    .Where(cu => cu.ChatId == Dto.Id && !cu.IsDeleted)
            //    .Select(cu => cu.UserId)
            //    .Distinct()
            //    .ToListAsync(_loadToken);

            //// Determine which users are present in ChatUsers but missing from the online participants list
            //var missingIds = chatUserIds.Except(participantIds).ToList();
            //if (missingIds.Count > 0)
            //{
            //    var fetchedDtos = new List<TgEfUserDto>();
            //    var chatBase = App.BusinessLogicManager.ConnectClient.DicChats.FirstOrDefault(x => x.Value.ID == Dto.Id).Value;
            //    if (chatBase is TL.Channel channel)
            //    {
            //        InputChannel inputChannel = new TL.InputChannel(channel.ID, channel.access_hash);
            //        // Try to fetch missing users one-by-one from Telegram using ConnectClient.GetParticipantAsync
            //        foreach (var id in missingIds)
            //        {
            //            if (_loadToken.IsCancellationRequested) break;

            //            try
            //            {
            //                await App.BusinessLogicManager.ConnectClient.CheckUserMemberAsync(inputChannel, id, accessHash: 0);
            //                // GetParticipantAsync(chatId, userId, ct) returns TL.User or null
            //                var participantDto = await App.BusinessLogicManager.ConnectClient.GetParticipantsAsync(Dto.Id, id, accessHash: 0, _loadToken);
            //                if (participantDto is null) continue;
            //                var tlUser = participantDto.User;
            //                if (tlUser is null) continue;

            //                var dto = new TgEfUserDto
            //                {
            //                    Id = tlUser.id,
            //                    IsUserActive = tlUser.IsActive,
            //                    IsBot = tlUser.IsBot,
            //                    LastSeenAgo = tlUser.LastSeenAgo,
            //                    UserName = tlUser.MainUsername,
            //                    AccessHash = tlUser.access_hash,
            //                    FirstName = tlUser.first_name,
            //                    LastName = tlUser.last_name,
            //                    UserNames = tlUser.usernames is null ? string.Empty : string.Join("|", tlUser.usernames.ToList()),
            //                    PhoneNumber = tlUser.phone,
            //                    Status = tlUser.status?.ToString() ?? string.Empty,
            //                    RestrictionReason = tlUser.restriction_reason is null ? string.Empty : string.Join("|", tlUser.restriction_reason.ToList()),
            //                    LangCode = tlUser.lang_code,
            //                    IsContact = false,
            //                };

            //                fetchedDtos.Add(dto);
            //            }
            //            catch (OperationCanceledException) { break; }
            //            catch (Exception ex)
            //            {
            //                // ignore individual fetch errors but log if necessary
            //                TgLogUtils.WriteException(ex);
            //            }
            //        }
            //    }

            //    // Persist fetched DTOs if any
            //    if (fetchedDtos.Count > 0)
            //        await App.BusinessLogicManager.ConnectClient.UpdateUsersAsync([.. fetchedDtos]);

            //    // Merge fetched DTOs into incoming list to ensure UI will show them
            //    if (fetchedDtos.Count > 0)
            //    {
            //        // avoid duplicates
            //        var existingIds = incomingUserDtos.Select(d => d.Id).ToHashSet();
            //        foreach (var d in fetchedDtos)
            //            if (!existingIds.Contains(d.Id))
            //                incomingUserDtos.Add(d);
            //    }
            //}

            //// Ensure chat-user link entities exist for all participant ids (online + those present in ChatUsers table)
            //var allParticipantIds = participantIds.Union(chatUserIds).Distinct().ToList();
            //if (allParticipantIds.Count > 0)
            //{
            //    // Build chat-user link entities to persist in ChatUsers table
            //    var now = DateTime.UtcNow;
            //    var chatUserEntities = allParticipantIds
            //        .Select(id => new TgEfChatUserEntity
            //        {
            //            ChatId = Dto.Id,
            //            UserId = id,
            //            JoinedAt = now,
            //            Role = participantIds.Contains(id) ? TgEnumChatUserRole.Admin : TgEnumChatUserRole.Member,
            //            IsMuted = false,
            //            MutedUntil = null,
            //            IsDeleted = false,
            //            DtChanged = now,
            //        })
            //        .ToList();

            //    if (chatUserEntities.Count > 0)
            //    {
            //        // Persist chat-user links via repository; use isRewriteEntities = true to update existing links
            //        // If repository SaveListAsync returns bool, await it; ignore result here but you may log it
            //        await App.BusinessLogicManager.StorageManager.ChatUserRepository.SaveListAsync(chatUserEntities, isRewriteEntities: true, isFirstTry: true, ct: _loadToken);
            //    }
            //}

            //// After persisting, load participants from storage (existing method loads from DB into UserDtos)
            //await LazyLoadCoreAsync(isSearch: false);

            UserDtos = [.. incomingUserDtos];
        }
        finally
        {
            LoadStateService.StopSoftStorageProcessing(uid);
        }
    }

    private async Task GetParticipantsFromMessagesAsync() => 
        await ContentDialogAsync(GetParticipantsFromMessagesCoreAsync, TgResourceExtensions.AskGetParticipantsFromMessages(), TgEnumLoadDesktopType.Online);

    private async Task GetParticipantsFromMessagesCoreAsync()
    {
        var uid = Guid.NewGuid();
        try
        {
            await LoadStateService.PrepareStorageTokenAsync(uid);

            // Load source DTO
            Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid, LoadStateService.StorageToken);
            await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);

            // Create missing users
            await App.BusinessLogicManager.StorageManager.UserRepository.CreateMissingUsersByMessagesAsync(Dto.Id, LoadStateService.StorageToken);
            await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);

            // Create missing chat users
            await App.BusinessLogicManager.StorageManager.ChatUserRepository.CreateMissingChatUsersByMessagesAsync(Dto.Id, LoadStateService.StorageToken);
            await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);

            await LazyLoadCoreAsync(isSearch: false);
        }
        finally
        {
            LoadStateService.StopSoftStorageProcessing(uid);
        }
    }

    private async Task ClearParticipantsAsync() => 
        await ContentDialogAsync(ClearParticipantsCoreAsync, TgResourceExtensions.AskClearParticipants(), TgEnumLoadDesktopType.Online);

    private async Task ClearParticipantsCoreAsync()
    {
        try
        {
            if (!SettingsService.IsExistsAppStorage) return;

            UserDtos.Clear();

            IsEmptyData = false;
        }
        finally
        {
            await ReloadUiAsync();
        }
    }

    /// <summary> Click on user </summary>
    internal void OnUserClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        var tag = button.Tag.ToString();
        if (string.IsNullOrEmpty(tag)) return;

        var uid = Guid.Parse(tag);
        NavigationService.NavigateTo(typeof(TgUserDetailsViewModel).FullName!, uid);
    }

    public void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not DataGrid dataGrid) return;
        if (dataGrid.SelectedItem is not TgEfUserDto dto) return;

        NavigationService.NavigateTo(typeof(TgUserDetailsViewModel).FullName!, dto.Uid);
    }

    #endregion
}
