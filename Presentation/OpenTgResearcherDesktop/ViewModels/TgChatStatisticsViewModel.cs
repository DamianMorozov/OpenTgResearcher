namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatStatisticsViewModel : TgPageViewModelBase
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
    [ObservableProperty]
    public partial Action ScrollRequested { get; set; } = () => { };
    [ObservableProperty]
    public partial TgEfChatStatisticsDto ChatStatisticsDto { get; set; } = new();

    public IAsyncRelayCommand CalcChatStatisticsCommand { get; }

    public TgChatStatisticsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILoadStateService loadStateService, 
        ILogger<TgChatStatisticsViewModel> logger, IAppNotificationService appNotificationService)
        : base(settingsService, navigationService, loadStateService, logger, nameof(TgChatStatisticsViewModel))
    {
        AppNotificationService = appNotificationService;
        // Commands
        CalcChatStatisticsCommand = new AsyncRelayCommand(CalcChatStatisticsAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        await LoadDataStorageCoreAsync();
    });

    private async Task LoadDataStorageCoreAsync()
    {
        try
        {
            if (!SettingsService.IsExistsAppStorage) return;

            Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid);

            IsEmptyData = false;
            ScrollRequested?.Invoke();
        }
        finally
        {
            await ReloadUiAsync();
        }
    }

    private async Task CalcChatStatisticsAsync() => 
        await ContentDialogAsync(CalcChatStatisticsCoreAsync, TgResourceExtensions.AskCalcChatStatistics(), TgEnumLoadDesktopType.Online);
    
    private async Task CalcChatStatisticsCoreAsync()
    {
        ChatStatisticsDto.Default();
        ChatStatisticsDto.UserWithCountDtos.Clear();
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
        
        await MergeUsersWithPlaceholdersAsync();
        await App.BusinessLogicManager.ConnectClient.UpdateUsersAsync([.. UserDtos]);

        // Unique users
        var distinctUsers = UserDtos.GroupBy(u => u.Id).Select(g => g.First()).ToList();

        ChatStatisticsDto.UsersCount = UserDtos.Where(x => !x.IsBot).Count();
        ChatStatisticsDto.BotsCount = UserDtos.Where(x => x.IsBot).Count();

        var dtStartRaw = ChatStatisticsDto.DtStart ?? TgEfChatStatisticsDto.SafeMinDate;
        var dtEndRaw = ChatStatisticsDto.DtEnd ?? TgEfChatStatisticsDto.SafeMaxDate;
        // If the minimum date is still obtained (which may contain a dangerous offset) replace
        var dtStart = dtStartRaw < TgEfChatStatisticsDto.SafeMinDate ? TgEfChatStatisticsDto.SafeMinDate : dtStartRaw;
        var dtEnd = dtEndRaw > TgEfChatStatisticsDto.SafeMaxDate ? TgEfChatStatisticsDto.SafeMaxDate : dtEndRaw;
        DateTimeOffset startDate = dtStart.ToUniversalTime().Date;
        DateTimeOffset tmpEnd = dtEnd.ToUniversalTime().Date;
        DateTimeOffset endDate = (tmpEnd < TgEfChatStatisticsDto.SafeMaxDate.Date) ? tmpEnd.AddDays(1).AddTicks(-1) : tmpEnd;

        
        var list = await Task.WhenAll(distinctUsers.Select(async u => new TgEfUserWithCountDto { UserDto = u,
            Count = await App.BusinessLogicManager.StorageManager.MessageRepository
                .GetCountAsync(x =>
                    x.SourceId == Dto.Id &&
                    x.UserId == u.Id &&
                    x.DtCreated >= startDate.UtcDateTime &&
                    x.DtCreated <= endDate.UtcDateTime)
        }));

        foreach (var dto in list.OrderByDescending(x => x.Count).ThenBy(x => x.UserDto.DisplayName, StringComparer.CurrentCultureIgnoreCase))
        {
            ChatStatisticsDto.UserWithCountDtos.Add(dto);
        }
    }

    private async Task MergeUsersWithPlaceholdersAsync()
    {
        var participantDtos = await App.BusinessLogicManager.ConnectClient.GetParticipantsAsync(Dto.Id);
        var userIds = await App.BusinessLogicManager.StorageManager.MessageRepository.GetUserIdsFromMessagesAsync(x => x.SourceId == Dto.Id);
        var usersDtos = await App.BusinessLogicManager.StorageManager.UserRepository.GetListDtosAsync(take: 0, skip: 0, x => userIds.Contains(x.Id));

        // Collect all existing IDs
        var existingIds = new HashSet<long>(participantDtos.Select(p => p.Id).Concat(usersDtos.Select(u => u.Id)));
        // Find missing IDs
        var missingIds = userIds.Where(id => !existingIds.Contains(id));
        // Create placeholder users for missing IDs
        var placeholderUsers = missingIds.Select(id => new TgEfUserDto
        {
            Id = id,
            IsBot = false,
            IsContact = false
        }).ToList();

        UserDtos = [.. 
            // Participants from Telegram
            participantDtos.Select(x => new TgEfUserDto()
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
            }), 
            // Users from Users table
            ..usersDtos,
            // Users from Messages table
            ..placeholderUsers
            ];
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

    #endregion
}
