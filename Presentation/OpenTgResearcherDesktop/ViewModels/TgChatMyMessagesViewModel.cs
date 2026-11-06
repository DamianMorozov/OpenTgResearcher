namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgChatMyMessagesViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial IAppNotificationService AppNotificationService { get; private set; }

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto Dto { get; set; } = new();
    [ObservableProperty]
    public partial ObservableCollection<TgEfUserDto> UserDtos { get; set; } = new();
    [ObservableProperty]
    public partial Action ScrollRequested { get; set; } = () => { };

    public IAsyncRelayCommand StartStorageMyMessagesCommand { get; }
    public IAsyncRelayCommand StopStorageMyMessagesCommand { get; }

    public TgChatMyMessagesViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgChatMyMessagesViewModel> logger, IAppNotificationService appNotificationService)
        : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatMyMessagesViewModel))
    {
        AppNotificationService = appNotificationService;
        // Commands
        StartStorageMyMessagesCommand = new AsyncRelayCommand(StartStorageMyMessagesAsync);
        StopStorageMyMessagesCommand = new AsyncRelayCommand(StopStorageMyMessagesAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        await StartStorageMyMessagesCoreAsync();
        await Task.CompletedTask;
    });

    private async Task StartStorageMyMessagesAsync() => 
        await ContentDialogAsync(StartStorageMyMessagesCoreAsync, TgResourceExtensions.AskLoading(), TgEnumLoadDesktopType.Storage);

    // Читаем сведения текущего пользователя, если его идентификатор известен
    private async Task StartStorageMyMessagesCoreAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        var uid = Guid.NewGuid();
        try
        {
            await LoadStateService.PrepareMemoryTokenAsync(uid);

            // Локальный профиль читается независимо от репозиториев и сетевого токена парсера
            using var sourceReader = IsOnlineProcessing ? new TgEfSourceRepository() : null;
            using var userReader = IsOnlineProcessing ? new TgEfUserRepository() : null;
            using var licenseReader = IsOnlineProcessing ? new TgEfLicenseRepository() : null;
            Dto = await (sourceReader ?? App.BusinessLogicManager.StorageManager.SourceRepository).GetDtoAsync(x => x.Uid == Uid, LoadStateService.MemoryToken);

            // Определяем идентификатор пользователя по клиенту или лицензии
            var userId = App.BusinessLogicManager.ConnectClient.Client?.UserId ?? 0;
            if (userId == 0)
            {
                var licenseDto = await (licenseReader ?? App.BusinessLogicManager.StorageManager.LicenseRepository).GetDtoAsync(x => x.UserId > 0);
                if (licenseDto is not null)
                    userId = licenseDto.UserId;
            }

            // Показываем текущего пользователя, только если он найден в хранилище
            UserDtos.Clear();
            if (userId > 0)
            {
                var userDto = await (userReader ?? App.BusinessLogicManager.StorageManager.UserRepository).GetMyselfUserAsync(Dto.Id, userId, LoadStateService.MemoryToken);
                if (userDto.Id > 0) UserDtos.Add(userDto);
            }
            IsEmptyData = UserDtos.Count == 0;
            ScrollRequested?.Invoke();
        }
        finally
        {
            LoadStateService.StopSoftMemoryProcessing(uid);

            await ReloadUiAsync();
        }
    }

    private async Task StopStorageMyMessagesAsync() => 
        await ContentDialogAsync(StopStorageMyMessagesCoreAsync, TgResourceExtensions.AskStopLoading(), TgEnumLoadDesktopType.Storage);

    private async Task StopStorageMyMessagesCoreAsync()
    {
        try
        {
            LoadStateService.StopHardOnlineProcessing();
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

    #endregion
}
