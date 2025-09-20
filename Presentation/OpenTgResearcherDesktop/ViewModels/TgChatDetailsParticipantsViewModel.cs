namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDetailsParticipantsViewModel : TgPageViewModelBase, IDisposable
{
    #region Fields, properties, constructor

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

    /// <summary> Cancellation token for download session </summary>
    private CancellationTokenSource? _loadCts;
    private CancellationToken _loadToken;

    public IAsyncRelayCommand LoadParticipantsCommand { get; }
    public IAsyncRelayCommand StopParticipantsCommand { get; }
    public IAsyncRelayCommand GetParticipantsCommand { get; }

    public TgChatDetailsParticipantsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILoadStateService loadStateService, 
        ILogger<TgChatDetailsParticipantsViewModel> logger, IAppNotificationService appNotificationService)
        : base(settingsService, navigationService, loadStateService, logger, nameof(TgChatDetailsParticipantsViewModel))
    {
        AppNotificationService = appNotificationService;
        // Commands
        LoadParticipantsCommand = new AsyncRelayCommand(LoadParticipantsAsync);
        StopParticipantsCommand = new AsyncRelayCommand(StopParticipantsAsync);
        GetParticipantsCommand = new AsyncRelayCommand(GetParticipantsAsync);
    }

    #endregion

    #region IDisposable

    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~TgChatDetailsParticipantsViewModel() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary> Release managed resources </summary>
    public void ReleaseManagedResources()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadToken = CancellationToken.None;
    }

    /// <summary> Release unmanaged resources </summary>
    public void ReleaseUnmanagedResources()
    {
        //
    }

    /// <summary> Dispose pattern </summary>
    public void Dispose()
    {
        // Dispose of unmanaged resources
        Dispose(true);
        // Suppress finalization
        GC.SuppressFinalize(this);
    }

    /// <summary> Dispose pattern </summary>
    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        // Release managed resources
        if (disposing)
            ReleaseManagedResources();
        // Release unmanaged resources
        ReleaseUnmanagedResources();
        // Flag
        _disposed = true;
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        //await LoadParticipantsCoreAsync();
        await Task.CompletedTask;
    });

    private async Task LoadParticipantsAsync() => await ContentDialogAsync(LoadParticipantsCoreAsync, TgResourceExtensions.AskLoading(), TgEnumLoadDesktopType.Storage);

    private async Task LoadParticipantsCoreAsync()
    {
        try
        {
            if (!SettingsService.IsExistsAppStorage) return;

            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = new CancellationTokenSource();
            _loadToken = _loadCts.Token;

            Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid, _loadToken);
            await Task.Delay(250);

            // Single query to get all distinct users for this source
            UserDtos = [.. await App.BusinessLogicManager.StorageManager.UserRepository.GetUsersBySourceIdJoinAsync(
                Dto.Id, App.BusinessLogicManager.ConnectClient.Client?.UserId ?? 0, _loadToken)];

            IsEmptyData = false;
            ScrollRequested?.Invoke();
        }
        finally
        {
            await ReloadUiAsync();
        }
    }

    private async Task StopParticipantsAsync() => await ContentDialogAsync(StopParticipantsCoreAsync, TgResourceExtensions.AskStopLoading(), TgEnumLoadDesktopType.Storage);

    private async Task StopParticipantsCoreAsync()
    {
        try
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadToken = CancellationToken.None;
        }
        finally
        {
            await ReloadUiAsync();
        }
    }

    private async Task GetParticipantsAsync() => await ContentDialogAsync(GetParticipantsCoreAsync, TgResourceExtensions.AskGetParticipants(), TgEnumLoadDesktopType.Online);

    private async Task GetParticipantsCoreAsync()
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = new CancellationTokenSource();
        _loadToken = _loadCts.Token;

        Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid, _loadToken);
        await Task.Delay(250);

        // Online query
        var participants = await App.BusinessLogicManager.ConnectClient.GetParticipantsAsync(Dto.Id);
        
        UserDtos = [.. participants.Select(x => new TgEfUserDto()
        {
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

        var uid = Guid.Parse(tag);
        NavigationService.NavigateTo(typeof(TgUserDetailsViewModel).FullName!, uid);
    }

    #endregion
}
