namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgChatDownloadViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto DownloadDto { get; set; } = new();
    [ObservableProperty]
    public partial string ChatProgressMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string ChatProgressInfo { get; set; } = string.Empty;

    public IRelayCommand OpenNetworkActivityCommand { get; }
    public TgChatViewModel Chat => App.GetService<TgChatViewModel>();
    public IRelayCommand PauseParsingCommand { get; }
    public IAsyncRelayCommand StopParsingCommand { get; }
    public IAsyncRelayCommand LoadDetailsCommand { get; }
    public IRelayCommand OpenDiscussionCommand { get; }

    [ObservableProperty]
    public partial double ParsingProgress { get; private set; }
    [ObservableProperty]
    public partial string ParsingStatus { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string ConnectionStatus { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string CurrentFile { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string CurrentTransfer { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial double FileProgress { get; private set; }
    [ObservableProperty]
    public partial bool HasCurrentFile { get; private set; }
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PauseParsingCommand), nameof(StopParsingCommand))]
    public partial bool CanControlParsing { get; private set; }
    [ObservableProperty]
    public partial bool HasRunningParsing { get; private set; }
    [ObservableProperty]
    public partial string PauseCaption { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial TgChatDetailsDto ChatDetailsDto { get; private set; } = new();
    [ObservableProperty]
    public partial bool HasOnlineDetails { get; private set; }
    [ObservableProperty]
    public partial bool HasDiscussion { get; private set; }
    [ObservableProperty]
    public partial bool CanStartParsing { get; private set; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDetailsError))]
    public partial string DetailsError { get; private set; } = string.Empty;
    public bool HasDetailsError => !string.IsNullOrEmpty(DetailsError);

    public TgChatDownloadViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgChatDownloadViewModel> logger)
        : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatDownloadViewModel))
    {
        // Команда перехода к общей активности остаётся доступной во время парсинга
        OpenNetworkActivityCommand = new RelayCommand(OpenNetworkActivity);
        PauseParsingCommand = new RelayCommand(PauseParsing, () => CanControlParsing);
        StopParsingCommand = new AsyncRelayCommand(StopParsingAsync, () => CanControlParsing);
        LoadDetailsCommand = new AsyncRelayCommand(LoadDetailsAsync);
        OpenDiscussionCommand = new RelayCommand(OpenDiscussion);
    }

    #endregion

    #region Methods

    /// <summary> Подготавливает настройки загрузки до обращения к прогрессу чата </summary>
    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => 
        await LoadStorageDataAsync(async () =>
        {
            var previousUid = Uid;
            DownloadDto = e?.Parameter as TgEfSourceDto ?? new();
            Uid = e?.Parameter is Guid uid ? uid : DownloadDto.Uid;
            if (SettingsService.IsExistsAppStorage && e?.Parameter is not TgEfSourceDto)
            {
                using var reader = IsOnlineProcessing ? new TgEfSourceRepository() : null;
                DownloadDto = await (reader ?? App.BusinessLogicManager.StorageManager.SourceRepository).GetDtoAsync(x => x.Uid == Uid);
            }
            if (previousUid != Uid)
            {
                ChatDetailsDto = new();
                HasOnlineDetails = false;
                DetailsError = string.Empty;
                App.VmLocator.ClearChatViewModel();
                App.VmLocator.ClearStateDownloadingChat();
            }
            RefreshActivity();
        });

    /// <summary> Обновляет снимок выбранного чата без сброса выполняющихся операций </summary>
    public void RefreshActivity()
    {
        HasDiscussion = Chat.DiscussionDto.Id > 0;
        var operations = LoadStateService.NetworkActivity.Operations.Where(x => x.ChatId == DownloadDto.Id).ToArray();
        var parsing = operations.FirstOrDefault(x => x.Kind == TgNetworkOperationKind.Parsing);
        HasRunningParsing = parsing?.State == TgNetworkOperationState.Running;
        CanStartParsing = DownloadDto.Id > 0 && IsOnlineReady && !HasRunningParsing;
        CanControlParsing = HasRunningParsing && parsing!.CanCancel;
        ParsingProgress = parsing?.Progress ?? TgDataUtils.CalcSourceProgress(DownloadDto.Count, DownloadDto.FirstId);
        ChatProgressMessage = $"{ParsingProgress:00.00} %";
        ChatProgressInfo = parsing?.CurrentMessageId is long current
            ? parsing.LastMessageId is long last ? string.Format("NetworkParsingMessageIds".GetLocalizedResource(), current, last)
                : string.Format("NetworkParsingMessageId".GetLocalizedResource(), current)
            : $"{DownloadDto.FirstId:N0} / {DownloadDto.Count:N0}";
        ParsingStatus = (parsing is null ? "ChatReady" : parsing.State == TgNetworkOperationState.Running
            ? parsing.IsCancellationRequested ? "NetworkStateStopping" : parsing.IsPaused ? "NetworkStatePaused" : "NetworkStateRunning"
            : $"NetworkState{parsing.State}").GetLocalizedResource();
        PauseCaption = (parsing?.IsPaused == true ? "NetworkResume" : "NetworkPause").GetLocalizedResource();
        ConnectionStatus = (IsOnlineReady ? "NetworkConnected" : "NetworkDisconnected").GetLocalizedResource();
        var file = operations.FirstOrDefault(x => x.Kind == TgNetworkOperationKind.Download && x.State == TgNetworkOperationState.Running);
        HasCurrentFile = file is not null;
        CurrentFile = file is null ? string.Empty : IsDisplaySensitiveData ? file.Detail : SensitiveData;
        FileProgress = file?.Progress ?? 0;
        CurrentTransfer = file is null ? string.Empty
            : $"{FileProgress:0.00} % · {TgFileUtils.GetFileSizeAsString(file.TransferredBytes)} / {TgFileUtils.GetFileSizeAsString(file.TotalBytes)}";
    }

    // Переключаем паузу только выбранного чата
    private void PauseParsing()
    {
        LoadStateService.NetworkActivity.ToggleParsingPause(DownloadDto.Id);
        RefreshActivity();
    }

    // Подтверждаем прерывание только выбранного чата
    private Task StopParsingAsync() => ContentDialogAsync(() =>
    {
        LoadStateService.NetworkActivity.CancelParsing(DownloadDto.Id);
        RefreshActivity();
        return Task.CompletedTask;
    }, TgResourceExtensions.AskStopParseTelegram(), TgEnumLoadDesktopType.Storage);

    // Запрашиваем расширенные сведения при раскрытии секции или явном обновлении
    private async Task LoadDetailsAsync(CancellationToken ct = default)
    {
        DetailsError = string.Empty;
        try
        {
            await LoadOnlineDataAsync(async () =>
            {
                if (Uid == Guid.Empty || !await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
                ChatDetailsDto = await App.BusinessLogicManager.ConnectClient.GetChatDetailsByClientAsync(Uid, ct);
                HasOnlineDetails = true;
            });
        }
        catch (Exception ex)
        {
            LogError(ex);
            DetailsError = string.Format("ChatDetailsFailed".GetLocalizedResource(), ex.Message);
        }
    }

    // Открываем найденное обсуждение этого чата
    private void OpenDiscussion()
    {
        if (Chat.DiscussionDto.Id > 0) NavigationService.NavigateTo(typeof(TgChatViewModel).FullName!, Chat.DiscussionDto.Uid);
    }

    // Открываем общую страницу сетевых операций без остановки текущего парсинга
    private void OpenNetworkActivity() => NavigationService.NavigateTo(typeof(TgNetworkActivityViewModel).FullName!);

    #endregion
}
