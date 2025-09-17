namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatDetailsMyMessagesViewModel : TgPageViewModelBase, IDisposable
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

    public IAsyncRelayCommand LoadMyMessagesCommand { get; }
    public IAsyncRelayCommand StopMyMessagesCommand { get; }

    public TgChatDetailsMyMessagesViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILoadStateService loadStateService, 
        ILogger<TgChatDetailsMyMessagesViewModel> logger, IAppNotificationService appNotificationService)
        : base(settingsService, navigationService, loadStateService, logger, nameof(TgChatDetailsMyMessagesViewModel))
    {
        AppNotificationService = appNotificationService;
        // Commands
        LoadMyMessagesCommand = new AsyncRelayCommand(LoadMyMessagesAsync);
        StopMyMessagesCommand = new AsyncRelayCommand(StopMyMessagesAsync);
    }

    #endregion

    #region IDisposable

    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~TgChatDetailsMyMessagesViewModel() => Dispose(false);

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
        await LoadMyMessagesCoreAsync();
        await Task.CompletedTask;
    });

    protected override async Task SetDisplaySensitiveAsync()
    {
        foreach (var userDto in UserDtos)
        {
            userDto.IsDisplaySensitiveData = IsDisplaySensitiveData;
        }

        await Task.CompletedTask;
    }

    private async Task LoadMyMessagesAsync() => await ContentDialogAsync(LoadMyMessagesCoreAsync, TgResourceExtensions.AskLoading(), TgEnumLoadDesktopType.Storage);

    private async Task LoadMyMessagesCoreAsync()
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

            // Get user Id
            var userId = App.BusinessLogicManager.ConnectClient.Client?.UserId ?? 0;
            if (userId == 0)
            {
                var licenseDto = await App.BusinessLogicManager.StorageManager.LicenseRepository.GetDtoAsync(x => x.UserId > 0);
                if (licenseDto is not null)
                    userId = licenseDto.UserId;
            }

            // Single query to get all distinct users for this source
            UserDtos.Clear();

            var userDto = await App.BusinessLogicManager.StorageManager.UserRepository.GetMyselfUserAsync(Dto.Id, userId, _loadToken);
            UserDtos.Add(userDto);

            IsEmptyData = false;
            ScrollRequested?.Invoke();
        }
        finally
        {
            await ReloadUiAsync();
            await SetDisplaySensitiveAsync();
        }
    }

    private async Task StopMyMessagesAsync() => await ContentDialogAsync(StopMyMessagesCoreAsync, TgResourceExtensions.AskStopLoading(), TgEnumLoadDesktopType.Storage);

    private async Task StopMyMessagesCoreAsync()
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
            await SetDisplaySensitiveAsync();
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
