// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDetailsStatisticsViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial IAppNotificationService AppNotificationService { get; private set; }

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto Dto { get; set; } = null!;
    [ObservableProperty]
    public partial TgChatDetailsDto ChatDetailsDto { get; set; } = new();
    [ObservableProperty]
    public partial ObservableCollection<TgEfUserDto> UserDtos { get; set; } = new();
    [ObservableProperty]
    public partial Action ScrollRequested { get; set; } = () => { };
    [ObservableProperty]
    public partial bool IsImageViewerVisible { get; set; }
    [ObservableProperty]
    public partial TgEfChatStatisticsDto ChatStatisticsDto { get; set; } = new();

    public IRelayCommand CalcChatStatisticsCommand { get; }

    public TgChatDetailsStatisticsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgChatDetailsStatisticsViewModel> logger,
        IAppNotificationService appNotificationService)
        : base(settingsService, navigationService, logger, nameof(TgChatDetailsStatisticsViewModel))
    {
        AppNotificationService = appNotificationService;
        // Commands
        SetDisplaySensitiveCommand = new AsyncRelayCommand(SetDisplaySensitiveAsync);
        CalcChatStatisticsCommand = new AsyncRelayCommand(CalcChatStatisticsAsync);
        // Updates
        App.BusinessLogicManager.ConnectClient.SetupUpdateStateSource(UpdateStateSource);
    }

    #endregion

    #region Public and private methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
        {
            Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
            await LoadDataStorageCoreAsync();
        });

    private async Task SetDisplaySensitiveAsync()
    {
        foreach (var userDto in UserDtos)
        {
            userDto.IsDisplaySensitiveData = IsDisplaySensitiveData;
        }

        await Task.CompletedTask;
    }

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
            await SetDisplaySensitiveAsync();
        }
    }

    private async Task CalcChatStatisticsAsync() => await ContentDialogAsync(CalcChatStatisticsCoreAsync, TgResourceExtensions.AskCalcChatStatistics());
    
    private async Task CalcChatStatisticsCoreAsync()
    {
        ChatStatisticsDto.DefaultValues();
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

        var participants = await App.BusinessLogicManager.ConnectClient.GetParticipantsAsync(Dto.Id);
        UserDtos = [.. participants.Select(x => new TgEfUserDto()
        {
            IsDisplaySensitiveData = IsDisplaySensitiveData,
            Id = x.id,
            IsContactActive = x.IsActive,
            IsBot = x.IsBot,
            LastSeenAgo = x.LastSeenAgo,
            UserName = x.MainUsername,
            AccessHash = x.access_hash,
            FirstName = x.first_name,
            LastName = x.last_name,
            UserNames = x.usernames is null ? string.Empty : string.Join("|", x.usernames.ToList()),
            PhoneNumber = x.phone,
            Status = x.status?.ToString() ?? string.Empty,
            RestrictionReason = x.restriction_reason is null ? string.Empty : string.Join("|", x.restriction_reason.ToList()),
            LangCode = x.lang_code,
            IsContact = false,
        })];

        await App.BusinessLogicManager.ConnectClient.UpdateUsersAsync([.. UserDtos]);

        ChatStatisticsDto.UsersCount = UserDtos.Where(x => !x.IsBot).Count();
        ChatStatisticsDto.BotsCount = UserDtos.Where(x => x.IsBot).Count();

        var dtStartRaw = ChatStatisticsDto.DtStart ?? TgEfChatStatisticsDto.SafeMinDate;
        var dtEndRaw = ChatStatisticsDto.DtEnd ?? TgEfChatStatisticsDto.SafeMaxDate;
        // If the minimum date is still obtained (which may contain a dangerous offset) replace
        var dtStart = dtStartRaw < TgEfChatStatisticsDto.SafeMinDate ? TgEfChatStatisticsDto.SafeMinDate : dtStartRaw;
        var dtEnd = dtEndRaw > TgEfChatStatisticsDto.SafeMaxDate ? TgEfChatStatisticsDto.SafeMaxDate : dtEndRaw;
        DateTimeOffset startDate = dtStart.ToUniversalTime().Date;
        DateTimeOffset tmpEnd = dtEnd.ToUniversalTime().Date;
        DateTimeOffset endDate;
        if (tmpEnd < TgEfChatStatisticsDto.SafeMaxDate.Date)
        {
            endDate = tmpEnd.AddDays(1).AddTicks(-1);
        }
        else
        {
            endDate = tmpEnd;
        }

        foreach (var userDto in UserDtos)
        {
            var userWithCountDto = new TgEfUserWithCountDto
            {
                UserDto = userDto,
                Count = await App.BusinessLogicManager.StorageManager.MessageRepository
                    .GetCountAsync(x => x.SourceId == Dto.Id && x.UserId == userDto.Id &&
                    x.DtCreated >= startDate.UtcDateTime && x.DtCreated <= endDate.UtcDateTime)
            };
            ChatStatisticsDto.UserWithCountDtos.Add(userWithCountDto);
        }
        // Order
        var orderedUsers = ChatStatisticsDto.UserWithCountDtos.OrderByDescending(x => x.Count).ToList();
        ChatStatisticsDto.UserWithCountDtos.Clear();
        foreach (var user in orderedUsers)
        {
            ChatStatisticsDto.UserWithCountDtos.Add(user);
        }
    }

    /// <summary> Click on user </summary>
    internal void OnUserClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        var tag = button.Tag.ToString();
        if (string.IsNullOrEmpty(tag)) return;

        Tuple<long, long> tuple = new(long.Parse(tag), Dto.Id);
        NavigationService.NavigateTo(typeof(TgUserDetailsViewModel).FullName!, tuple);
    }

    #endregion
}
