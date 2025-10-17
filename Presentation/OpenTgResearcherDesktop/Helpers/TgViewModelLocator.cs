namespace OpenTgResearcherDesktop.Helpers;

/// <summary> ViewModel locator </summary>
public sealed class TgViewModelLocator
{
    #region Fields, properties, constructor

    private readonly ILoadStateService LoadStateService = default!;
    private CancellationTokenSource? _floodCts;
    private readonly ConcurrentDictionary<string, object> _viewModelsByName = new();
    private readonly ConcurrentDictionary<Type, object> _viewModelsByType = new();
    private ShellViewModel? _shellVm;
    private TgChatDownloadViewModel? _chatDownloadingVm;
    private TgChatsViewModel? _chatsVm;
    private TgClientConnectionViewModel? _clientConnectionVm;

    public TgViewModelLocator()
    {
        var assembly = typeof(TgViewModelLocator).Assembly;

        var viewModelTypes = assembly.GetTypes()
            .Where(t => t.Name.EndsWith("ViewModel") && t.IsClass && !t.IsAbstract);

        foreach (var type in viewModelTypes)
        {
            try
            {
                var instance = App.GetService(type);
                _viewModelsByType[type] = instance;
                _viewModelsByName[type.Name] = instance;
            }
#if DEBUG
            catch (Exception ex)
            {
                TgLogUtils.WriteException(ex);
            }
#else
            catch (Exception)
            {
                //
            }
#endif
        }

        // Shell commands
        _shellVm ??= Get<ShellViewModel>();
        _shellVm.UpdatePageCommand = new AsyncRelayCommand(ShellUpdatePageAsync);
        _shellVm.ClientConnectCommand = new AsyncRelayCommand<bool>(ShellClientConnectAsync);
        _shellVm.ClientDisconnectCommand = new AsyncRelayCommand<bool>(ShellClientDisconnectAsync);

        // Callback updates UI
        App.BusinessLogicManager.ConnectClient.SetupUpdateShellViewModel(UpdateShellViewModelAsync);
        App.BusinessLogicManager.ConnectClient.SetupUpdateChatViewModel(UpdateChatViewModelAsync);
        App.BusinessLogicManager.ConnectClient.SetupUpdateStateFile(UpdateStateFileAsync);
        App.BusinessLogicManager.ConnectClient.SetupUpdateChatsViewModel(UpdateChatsViewModelAsync);

        LoadStateService = App.GetService<ILoadStateService>() ?? throw new InvalidOperationException("Cannot resolve ILoadStateService");
    }

    #endregion

    #region Methods

    /// <summary> Get ViewModel by type </summary>
    public T Get<T>() where T : class =>
        _viewModelsByType.TryGetValue(typeof(T), out var vm)
            ? vm as T ?? throw new InvalidCastException($"Cannot cast {vm?.GetType()} to {typeof(T)}")
            : throw new KeyNotFoundException($"ViewModel of type {typeof(T)} not found.");

    /// <summary> Get ViewModel by type's name </summary>
    public object Get(string name) =>
        _viewModelsByName.TryGetValue(name, out var vm)
            ? vm
            : throw new KeyNotFoundException($"ViewModel named '{name}' not found.");

    /// <summary> Update chat message ViewModel </summary>
    public async Task UpdateChatViewModelAsync(long chatId, int messageId, int count, string message)
    {
        var uid = Guid.NewGuid();
        try
        {
            await LoadStateService.PrepareLocatorTokenAsync(uid);
        
            await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
            {
                UpdateChatViewModelCore(chatId, messageId, count, ref message);
                await Task.CompletedTask;
            }, LoadStateService.LocatorToken);

        }
        finally
        {
            LoadStateService.StopSoftLocatorProcessing(uid);
        }
    }

    private void UpdateChatViewModelCore(long chatId, int messageId, int count, ref string message)
    {
        _chatDownloadingVm ??= App.VmLocator.Get<TgChatDownloadViewModel>();
        if (_chatDownloadingVm is not null)
        {
            if (_chatDownloadingVm.DownloadDto?.Id == chatId && _chatDownloadingVm.DownloadDto?.FirstId <= messageId)
            {
                _chatDownloadingVm.DownloadDto.FirstId = messageId;
                _chatDownloadingVm.ChatProgressMessage = $"{(messageId == 0 || count == 0 ? 0 : (float)messageId * 100 / count):00.00} %";
                _chatDownloadingVm.StateSourceDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
                _chatDownloadingVm.StateSourceMsg = $"{messageId} | {message}";
            }
        }
    }

    /// <summary> Update chat media ViewModel </summary>
    public async Task UpdateStateFileAsync(long chatId, int messageId, string fileName, long fileSize, long transmitted, long fileSpeed,
        bool isStartTask, int threadNumber)
    {
        var uid = Guid.NewGuid();
        try
        {
            await LoadStateService.PrepareLocatorTokenAsync(uid);

            await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
            {
                UpdateStateFileCore(chatId, messageId, fileName, fileSize, transmitted, fileSpeed, isStartTask, threadNumber);
                await Task.CompletedTask;
            }, LoadStateService.LocatorToken);
        }
        finally
        {
            LoadStateService.StopSoftLocatorProcessing(uid);
        }
    }

    private void UpdateStateFileCore(long chatId, int messageId, string fileName, long fileSize, long transmitted, long fileSpeed,
        bool isStartTask, int threadNumber)
    {
        _chatDownloadingVm ??= App.VmLocator.Get<TgChatDownloadViewModel>();
        if (_chatDownloadingVm is not null)
        {
            if (_chatDownloadingVm.DownloadDto?.Id == chatId)
            {
                // Download job
                if (!string.IsNullOrEmpty(fileName))
                {
                    // Download status job
                    _chatDownloadingVm.DownloadSettings.SourceVm.Dto.FirstId = messageId;
                    _chatDownloadingVm.DownloadSettings.SourceVm.Dto.CurrentFileName = fileName;
                    _chatDownloadingVm.DownloadSettings.SourceVm.Dto.CurrentFileSize = fileSize;
                    _chatDownloadingVm.DownloadSettings.SourceVm.Dto.CurrentFileTransmitted = transmitted;
                    _chatDownloadingVm.DownloadSettings.SourceVm.Dto.CurrentFileSpeed = fileSpeed;
                }
                // Download reset
                else
                {
                    // Download status reset
                    _chatDownloadingVm.DownloadSettings.SourceVm.Dto.FirstId = messageId;
                    _chatDownloadingVm.DownloadSettings.SourceVm.Dto.CurrentFileName = string.Empty;
                    _chatDownloadingVm.DownloadSettings.SourceVm.Dto.CurrentFileSize = 0;
                    _chatDownloadingVm.DownloadSettings.SourceVm.Dto.CurrentFileTransmitted = 0;
                    _chatDownloadingVm.DownloadSettings.SourceVm.Dto.CurrentFileSpeed = 0;
                }
                // State
                _chatDownloadingVm.StateSourceMedia = string.IsNullOrEmpty(fileName) ? string.Empty
                    : $"{fileName} | " +
                    $"{TgResourceExtensions.GetTransmitted()} {_chatDownloadingVm.DownloadSettings.SourceVm.Dto.CurrentFileProgressPercentString} | " +
                    $"{TgResourceExtensions.GetSpeed()} {_chatDownloadingVm.DownloadSettings.SourceVm.Dto.CurrentFileSpeedKBString}";
            }
        }
    }

    /// <summary> Update chats ViewModel </summary>
    public async Task UpdateChatsViewModelAsync(int counter, int countAll, TgEnumChatsMessageType chatsMessageType)
    {
        var uid = Guid.NewGuid();
        try
        {
            await LoadStateService.PrepareLocatorTokenAsync(uid);

            await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
            {
                UpdateChatsViewModelCore(ref counter, ref countAll, ref chatsMessageType);
                await Task.CompletedTask;
            }, LoadStateService.LocatorToken);
        }
        finally
        {
            LoadStateService.StopSoftLocatorProcessing(uid);
        }
    }

    private void UpdateChatsViewModelCore(ref int counter, ref int countAll, ref TgEnumChatsMessageType chatsMessageType)
    {
        _chatsVm ??= App.VmLocator.Get<TgChatsViewModel>();
        if (_chatsVm is not null)
        {
            _chatsVm.ChatsProgressCounter = counter;
            _chatsVm.ChatsProgressCountAll = countAll;
            _chatsVm.ChatsProgressString = $"{counter} {TgResourceExtensions.GetFrom()} {countAll}";
            switch (chatsMessageType)
            {
                case TgEnumChatsMessageType.StartScan:
                    _chatsVm.ChatsProgressMessage = TgResourceExtensions.GetStartScan();
                    break;
                case TgEnumChatsMessageType.ProcessingChats:
                    _chatsVm.ChatsProgressMessage = TgResourceExtensions.GetProcessingChats();
                    break;
                case TgEnumChatsMessageType.ProcessingGroups:
                    _chatsVm.ChatsProgressMessage = TgResourceExtensions.GetProcessingGroups();
                    break;
                case TgEnumChatsMessageType.ProcessingDialogs:
                    _chatsVm.ChatsProgressMessage = TgResourceExtensions.GetProcessingDialogs();
                    break;
                case TgEnumChatsMessageType.StopScan:
                    _chatsVm.ChatsProgressMessage = TgResourceExtensions.GetStopScan();
                    break;
            }
        }
    }

    public void ClearChatViewModel()
    {
        _chatDownloadingVm ??= App.VmLocator.Get<TgChatDownloadViewModel>();
        if (_chatDownloadingVm is not null)
        {
            _chatDownloadingVm.ChatProgressMessage = string.Empty;
            _chatDownloadingVm.StateSourceMsg = string.Empty;
        }
    }

    public void ClearStateFile()
    {
        _chatDownloadingVm ??= App.VmLocator.Get<TgChatDownloadViewModel>();
        if (_chatDownloadingVm is not null)
        {
            _chatDownloadingVm.StateSourceMedia = string.Empty;
        }
    }

    private async Task ShellUpdatePageAsync()
    {
        _shellVm ??= App.VmLocator.Get<ShellViewModel>();
        if (_shellVm.EventArgs?.Content is not TgPageBase pageBase) return;
        await pageBase.ViewModel.OnNavigatedToAsync(_shellVm.EventArgs);
    }

    public async Task ShellClientConnectAsync(bool isQuestion)
    {
        var uid = Guid.NewGuid();
        try
        {
            await LoadStateService.PrepareLocatorTokenAsync(uid);

            await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
            {
                _shellVm ??= App.VmLocator.Get<ShellViewModel>();
                
                _clientConnectionVm ??= App.VmLocator.Get<TgClientConnectionViewModel>();
                if (_clientConnectionVm is not null)
                {
                    await _clientConnectionVm.OnNavigatedToAsync(_shellVm.EventArgs);
                    if (!_clientConnectionVm.IsOnlineReady)
                    {
                        await _clientConnectionVm.ClientConnectCommand.ExecuteAsync(isQuestion);
                        await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);
                    }
                }
                
                _shellVm.UidSavedMessages = await App.BusinessLogicManager.ConnectClient.OpenOrCreateSavedMessagesAsync();

                await Task.CompletedTask;
            }, LoadStateService.LocatorToken);
        }
        finally
        {
            LoadStateService.StopSoftLocatorProcessing(uid);
        }
    }

    public async Task ShellClientDisconnectAsync(bool isQuestion)
    {
        var uid = Guid.NewGuid();
        try
        {
            await LoadStateService.PrepareLocatorTokenAsync(uid);

            await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
            {
                _clientConnectionVm ??= App.VmLocator.Get<TgClientConnectionViewModel>();
                if (_clientConnectionVm is not null)
                {
                    if (_clientConnectionVm.IsOnlineReady)
                    {
                        await _clientConnectionVm.ClientDisconnectCommand.ExecuteAsync(isQuestion);
                        await Task.Delay(TgConstants.TimeOutUIShortMilliseconds);
                    }
                }
                await Task.CompletedTask;
            }, LoadStateService.LocatorToken);
        }
        finally
        {
            LoadStateService.StopSoftLocatorProcessing(uid);
        }
    }

    /// <summary> Update shell ViewModel </summary>
    public async Task UpdateShellViewModelAsync(bool isFloodVisible, int seconds, string message)
    {
        var uid = Guid.NewGuid();
        try
        {
            await LoadStateService.PrepareLocatorTokenAsync(uid);

            await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
            {
                UpdateShellViewModelCore(ref isFloodVisible, ref seconds, ref message);
                // Start countdown if visible
                if (isFloodVisible && seconds > 0)
                    StartFloodCountdown(seconds);
                else
                    _floodCts?.Cancel();
                await Task.CompletedTask;
            }, LoadStateService.LocatorToken);
        }
        finally
        {
            LoadStateService.StopSoftLocatorProcessing(uid);
        }
    }

    private void UpdateShellViewModelCore(ref bool isFloodVisible, ref int seconds, ref string message)
    {
        _shellVm ??= App.VmLocator.Get<ShellViewModel>();
        if (_shellVm is not null)
        {
            _shellVm.IsFloodVisible = isFloodVisible;
            _shellVm.FloodWaitSeconds = seconds;
            _shellVm.FloodMessage = message;
        }
    }

    private void StartFloodCountdown(int seconds)
    {
        // Stop old timer if exists
        _floodCts?.Cancel();
        _floodCts = new CancellationTokenSource();

        Task.Run(async () =>
        {
            try
            {
                while (seconds > 0 && !_floodCts.Token.IsCancellationRequested)
                {
                    await Task.Delay(TgConstants.TimeOutUILongMilliseconds, _floodCts.Token);
                    seconds--;

                    if (_shellVm is not null)
                        TgDesktopUtils.InvokeOnUIThread(() => { _shellVm.FloodWaitSeconds = seconds; });
                }

                // Hide when time is up
                if (!_floodCts.Token.IsCancellationRequested)
                {
                    if (_shellVm is not null)
                        TgDesktopUtils.InvokeOnUIThread(() => {
                            _shellVm.IsFloodVisible = false;
                            _shellVm.FloodMessage = string.Empty;
                        });
                }
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
        }, _floodCts.Token);
    }

    #endregion
}
