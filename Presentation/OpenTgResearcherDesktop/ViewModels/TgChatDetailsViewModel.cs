// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDetailsViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto Dto { get; set; } = null!;
    [ObservableProperty]
    public partial TgChatDetailsDto ChatDetailsDto { get; set; } = new();
    [ObservableProperty]
    public partial ObservableCollection<TgEfMessageDto> MessageDtos { get; set; } = new();
    [ObservableProperty]
    public partial ObservableCollection<TgEfUserDto> UserDtos { get; set; } = new();
    [ObservableProperty]
    public partial bool EmptyData { get; set; } = true;
    [ObservableProperty]
    public partial Action ScrollRequested { get; set; } = () => { };
    [ObservableProperty]
    public partial bool IsImageViewerVisible { get; set; }
    [ObservableProperty]
    public partial TgEfChatStatisticsDto ChatStatisticsDto { get; set; } = new();

    public IRelayCommand ClearDataStorageCommand { get; }
    public IRelayCommand UpdateOnlineCommand { get; }
    public IRelayCommand UpdateChatDetailsCommand { get; }
    public IRelayCommand StopDownloadingCommand { get; }
    public IRelayCommand GetParticipantsCommand { get; }
    public IRelayCommand CalcChatStatisticsCommand { get; }

    public TgChatDetailsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgChatDetailsViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgChatDetailsViewModel))
    {
        // Commands
        ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
        UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
        UpdateChatDetailsCommand = new AsyncRelayCommand(UpdateChatDetailsAsync);
        StopDownloadingCommand = new AsyncRelayCommand(StopDownloadingAsync);
        GetParticipantsCommand = new AsyncRelayCommand(GetParticipantsAsync);
        IsDisplaySensitiveCommand = new AsyncRelayCommand(IsDisplaySensitiveAsync);
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
            await ReloadUiAsync();
        });

    private async Task ClearDataStorageAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

    private async Task ClearDataStorageCoreAsync()
    {
        Dto = new();
        MessageDtos = [];
        UserDtos = [];
        EmptyData = !MessageDtos.Any();
        await Task.CompletedTask;
    }

    private async Task LoadDataStorageCoreAsync()
    {
        await ReloadUiAsync();
        if (!SettingsService.IsExistsAppStorage) return;
        Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid);
        MessageDtos = [.. await App.BusinessLogicManager.StorageManager.MessageRepository.GetListDtosDescAsync(
            take: 100, skip: 0, where: x => x.SourceId == Dto.Id, order: x => x.Id, isReadOnly: true)];
        foreach (var messageDto in MessageDtos)
        {
            messageDto.Directory = Dto.Directory;
        }
        EmptyData = !MessageDtos.Any();
        ScrollRequested?.Invoke();
    }

    private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline());

    private async Task UpdateOnlineCoreAsync() => await LoadDataAsync(async () =>
    {
        IsDownloading = true;
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
        var entity = Dto.GetNewEntity();
        DownloadSettings.SourceVm.Fill(entity);
        DownloadSettings.SourceVm.Dto.DtChanged = DateTime.Now;
        await DownloadSettings.UpdateSourceWithSettingsAsync();

        StateSourceDirectory = Dto.Directory;

        await App.BusinessLogicManager.ConnectClient.DownloadAllDataAsync(DownloadSettings);
        await DownloadSettings.UpdateSourceWithSettingsAsync();
        await LoadDataStorageCoreAsync();
        IsDownloading = false;
    });

    private async Task UpdateChatDetailsAsync() => await ContentDialogAsync(UpdateChatDetailsCoreAsync, TgResourceExtensions.AskUpdateChatDetails());

    private async Task UpdateChatDetailsCoreAsync() => await LoadDataAsync(async () =>
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

        ChatDetailsDto = await App.BusinessLogicManager.ConnectClient.GetChatDetailsByClientAsync(Dto.Id);
    });

    private async Task StopDownloadingAsync() => await ContentDialogAsync(StopDownloadingCoreAsync, TgResourceExtensions.AskStopDownloading());

    private async Task StopDownloadingCoreAsync()
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
        App.BusinessLogicManager.ConnectClient.SetForceStopDownloading();
    }

    private async Task GetParticipantsAsync() => await ContentDialogAsync(GetParticipantsCoreAsync, TgResourceExtensions.AskGetParticipants());

    private async Task GetParticipantsCoreAsync()
    {
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
        }).ToList()];
    }

    private async Task IsDisplaySensitiveAsync()
    {
        foreach (var userDto in UserDtos)
        {
            userDto.IsDisplaySensitiveData = IsDisplaySensitiveData;
        }
        await Task.CompletedTask;
    }

    private async Task CalcChatStatisticsAsync()
    {
        ChatStatisticsDto.DefaultValues();
        await GetParticipantsCoreAsync();

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

    #endregion
}