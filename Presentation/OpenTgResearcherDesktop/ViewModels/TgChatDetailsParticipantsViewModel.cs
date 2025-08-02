// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDetailsParticipantsViewModel : TgPageViewModelBase
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
    public partial bool EmptyData { get; set; } = true;
    [ObservableProperty]
    public partial Action ScrollRequested { get; set; } = () => { };

    public IRelayCommand GetParticipantsCommand { get; }

    public TgChatDetailsParticipantsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgChatDetailsParticipantsViewModel> logger,
        IAppNotificationService appNotificationService)
        : base(settingsService, navigationService, logger, nameof(TgChatDetailsParticipantsViewModel))
    {
        AppNotificationService = appNotificationService;
        // Commands
        GetParticipantsCommand = new AsyncRelayCommand(GetParticipantsAsync);
        SetDisplaySensitiveCommand = new AsyncRelayCommand(SetDisplaySensitiveAsync);
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
            
            EmptyData = false;
            ScrollRequested?.Invoke();
        }
        finally
        {
            await ReloadUiAsync();
            await SetDisplaySensitiveAsync();
        }
    }

    private async Task GetParticipantsAsync() => await ContentDialogAsync(GetParticipantsCoreAsync, TgResourceExtensions.AskGetParticipants(), useLoadData: true);

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
            IsContact = false,
        })];

        await App.BusinessLogicManager.ConnectClient.UpdateUsersAsync([.. UserDtos]);
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
