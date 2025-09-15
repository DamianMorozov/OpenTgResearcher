namespace OpenTgResearcherDesktop.Common;

/// <summary> Base class for TgViewModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public partial class TgPageViewModelBase : TgSensitiveModel, ITgPageViewModel
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial ITgSettingsService SettingsService { get; private set; }
    [ObservableProperty]
    public partial INavigationService NavigationService { get; private set; }
    [ObservableProperty]
    public partial ILogger<TgPageViewModelBase> Logger { get; private set; }
    [ObservableProperty]
    public partial string Name { get; private set; }
    [ObservableProperty]
    public partial TgExceptionViewModel Exception { get; set; } = new();
    [ObservableProperty]
    public partial string ConnectionDt { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsClientConnected { get; set; }
    [ObservableProperty]
    public partial string ConnectionMsg { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StateProxyDt { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StateProxyMsg { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StateSourceDt { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StateSourceMsg { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StateSourceMedia { get; set; } = string.Empty;
    [ObservableProperty]
    public partial XamlRoot? XamlRootVm { get; set; }
    [ObservableProperty]
    public partial bool IsPageLoad { get; set; }
    [ObservableProperty]
    public partial bool IsOnlineReady { get; set; }
    [ObservableProperty]
    public partial bool IsOnlineProcessing { get; set; }
    [ObservableProperty]
    public partial TgDownloadSettingsViewModel DownloadSettings { get; set; } = new();
    [ObservableProperty]
    public partial bool IsEmptyData { get; set; } = true;
    [ObservableProperty]
    public partial bool IsPaidLicense { get; set; }
    [ObservableProperty]
    public partial TgEnumLicenseType LicenseType { get; set; } = TgEnumLicenseType.No;

    private TgChatViewModel? _chatVm;
    private TgChatsViewModel? _chatsVm;

    public IAsyncRelayCommand ShowPurchaseLicenseCommand { get; }

    public TgPageViewModelBase(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgPageViewModelBase> logger, string name) : base()
    {
        SettingsService = settingsService;
        NavigationService = navigationService;
        Logger = logger;
        Name = name;
        IsDisplaySensitiveData = NavigationService.IsDisplaySensitiveData;
        // Commands
        ShowPurchaseLicenseCommand = new AsyncRelayCommand(ShowPurchaseLicenseAsync);
    }

    #endregion

    #region Methods

    public virtual string ToDebugString() => TgObjectUtils.ToDebugString(this);

    public virtual void OnLoaded(object parameter)
    {
        if (parameter is XamlRoot xamlRoot)
        {
            XamlRootVm = xamlRoot;
            LogInformation("Page loaded");
        }
        else
            LogWarning("Page loaded without XamlRoot");
    }

    public virtual async Task OnNavigatedToAsync(NavigationEventArgs? e) => 
        await LoadStorageDataAsync(() => { IsDisplaySensitiveData = NavigationService.IsDisplaySensitiveData; });

    public virtual async Task ReloadUiAsync()
    {
        ConnectionDt = string.Empty;
        ConnectionMsg = string.Empty;
        Exception.Default();
        await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync();
        IsClientConnected = IsOnlineReady = App.BusinessLogicManager.ConnectClient.IsReady;
    }

    /// <summary> Open url </summary>
    public void OpenHyperlink(object sender, RoutedEventArgs e)
    {
        if (sender is not HyperlinkButton hyperlinkButton)
            return;
        if (hyperlinkButton.Tag is not string tag)
            return;
        var url = TgDesktopUtils.ExtractUrl(tag);
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }

    /// <summary> Update state client message </summary>
    public virtual async Task UpdateStateProxyAsync(string message) => await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
    {
        StateProxyDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
        StateProxyMsg = message;
        await Task.CompletedTask;
    });

    /// <summary> Update exception message </summary>
    public virtual async Task UpdateExceptionAsync(Exception ex)
    {
        Exception = new(ex);
        await Task.CompletedTask;
    }

    /// <summary> Update chats ViewModel </summary>
    public async Task UpdateChatsViewModelAsync(int counter, int countAll, TgEnumChatsMessageType chatsMessageType) =>
        await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
        {
            UpdateChatsViewModelCore(ref counter, ref countAll, ref chatsMessageType);
            await Task.CompletedTask;
        });

    private void UpdateChatsViewModelCore(ref int counter, ref int countAll, ref TgEnumChatsMessageType chatsMessageType)
    {
        _chatsVm ??= App.Locator?.Get<TgChatsViewModel>();
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

    /// <summary> Update chat message ViewModel </summary>
    public async Task UpdateChatViewModelAsync(long chatId, int messageId, int count, string message) => await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
    {
        UpdateChatViewModelCore(chatId, messageId, count, ref message);
        await Task.CompletedTask;
    });

    private void UpdateChatViewModelCore(long chatId, int messageId, int count, ref string message)
    {
        _chatVm ??= App.Locator?.Get<TgChatViewModel>();
        if (_chatVm is not null)
        {
            if (_chatVm.Dto?.Id == chatId && _chatVm.Dto?.FirstId < messageId)
            {
                _chatVm.Dto.FirstId = messageId;
                _chatVm.ChatProgressMessage = $"{(messageId == 0 || count == 0 ? 0 : (float)messageId * 100 / count):00.00} %";
                _chatVm.StateSourceDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
                _chatVm.StateSourceMsg = $"{messageId} | {message}";
            }
        }
    }

    /// <summary> Update chat media ViewModel </summary>
    public async Task UpdateStateFileAsync(long chatId, int messageId, string fileName, long fileSize, long transmitted, long fileSpeed,
        bool isStartTask, int threadNumber) => await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
        {
            UpdateStateFileCore(chatId, messageId, fileName, fileSize, transmitted, fileSpeed, isStartTask, threadNumber);
            await Task.CompletedTask;
        });

    private void UpdateStateFileCore(long chatId, int messageId, string fileName, long fileSize, long transmitted, long fileSpeed,
        bool isStartTask, int threadNumber)
    {
        _chatVm ??= App.Locator?.Get<TgChatViewModel>();
        if (_chatVm is not null)
        {
            if (_chatVm.Dto?.Id == chatId)
            {
                // Download job
                if (!string.IsNullOrEmpty(fileName))
                {
                    // Download status job
                    DownloadSettings.SourceVm.Dto.FirstId = messageId;
                    DownloadSettings.SourceVm.Dto.CurrentFileName = fileName;
                    DownloadSettings.SourceVm.Dto.CurrentFileSize = fileSize;
                    DownloadSettings.SourceVm.Dto.CurrentFileTransmitted = transmitted;
                    DownloadSettings.SourceVm.Dto.CurrentFileSpeed = fileSpeed;
                }
                // Download reset
                else
                {
                    // Download status reset
                    DownloadSettings.SourceVm.Dto.FirstId = messageId;
                    DownloadSettings.SourceVm.Dto.CurrentFileName = string.Empty;
                    DownloadSettings.SourceVm.Dto.CurrentFileSize = 0;
                    DownloadSettings.SourceVm.Dto.CurrentFileTransmitted = 0;
                    DownloadSettings.SourceVm.Dto.CurrentFileSpeed = 0;
                }
                // State
                _chatVm.StateSourceMedia = string.IsNullOrEmpty(fileName) ? string.Empty
                    : $"{fileName} | " +
                    $"{TgResourceExtensions.GetTransmitted()} {DownloadSettings.SourceVm.Dto.CurrentFileProgressPercentString} | " +
                    $"{TgResourceExtensions.GetSpeed()} {DownloadSettings.SourceVm.Dto.CurrentFileSpeedKBString}";
            }
        }
    }

    /// <summary> Creates a base ContentDialog with common settings </summary>
    private ContentDialog CreateContentDialog(string title) => new() { XamlRoot = XamlRootVm, Title = title };

    protected async Task ContentDialogAsync(string title, string content)
    {
        if (XamlRootVm is null) return;

        try
        {
            var dialog = CreateContentDialog(title);
            dialog.Content = content;
            dialog.CloseButtonText = TgResourceExtensions.GetOkButton();
            dialog.DefaultButton = ContentDialogButton.Close;

            _ = await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, "Error showing content dialog");
        }
    }

    /// <summary> Shows a simple content dialog with exception handling </summary>
    protected async Task ContentDialogAsync(Func<Task> task, string title, TgEnumLoadDesktopType loadType, ContentDialogButton defaultButton = ContentDialogButton.Close)
    {
        if (XamlRootVm is null) return;

        try
        {
            var dialog = CreateContentDialog(title);
            dialog.PrimaryButtonText = TgResourceExtensions.GetYesButton();
            dialog.CloseButtonText = TgResourceExtensions.GetCancelButton();
            dialog.DefaultButton = defaultButton;
            switch (loadType)
            {
                case TgEnumLoadDesktopType.Storage:
                    dialog.PrimaryButtonCommand = new AsyncRelayCommand(() => LoadStorageDataAsync(task));
                    break;
                case TgEnumLoadDesktopType.Online:
                    dialog.PrimaryButtonCommand = new AsyncRelayCommand(() => LoadOnlineDataAsync(task));
                    break;
            }

            _ = await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, "Error showing content dialog");
        }
    }

    /// <summary> Shows a simple content dialog with exception handling </summary>
    protected async Task ContentDialogAsync(Action action, string title, TgEnumLoadDesktopType loadType, ContentDialogButton defaultButton = ContentDialogButton.Close)
    {
        if (XamlRootVm is null) return;

        try
        {
            var dialog = CreateContentDialog(title);
            dialog.PrimaryButtonText = TgResourceExtensions.GetYesButton();
            dialog.CloseButtonText = TgResourceExtensions.GetCancelButton();
            dialog.DefaultButton = defaultButton;
            switch (loadType)
            {
                case TgEnumLoadDesktopType.Storage:
                    dialog.PrimaryButtonCommand = new AsyncRelayCommand(() => LoadStorageDataAsync(action));
                    break;
                case TgEnumLoadDesktopType.Online:
                    dialog.PrimaryButtonCommand = new AsyncRelayCommand(() => LoadOnlineDataAsync(action));
                    break;
            }

            _ = await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, "Error showing content dialog");
        }
    }

    /// <summary> Load storage data with async task </summary>
    protected async Task LoadStorageDataAsync(Func<Task> task)
    {
        try
        {
            if (!IsPageLoad)
            {
                IsPageLoad = true;
                await Task.Delay(250);
            }

            await TgDesktopUtils.InvokeOnUIThreadAsync(task);
        }
        catch (Exception ex)
        {
            LogError(ex, $"Unhandled exception in {nameof(LoadStorageDataAsync)}");
        }
        finally
        {
            if (IsPageLoad)
                IsPageLoad = false;
            RefreshLicenseInfo();
        }
    }

    /// <summary> Load storage data with action </summary>
    protected async Task LoadStorageDataAsync(Action action)
    {
        try
        {
            if (!IsPageLoad)
            {
                IsPageLoad = true;
                await Task.Delay(250);
            }

            TgDesktopUtils.InvokeOnUIThread(action);
        }
        catch (Exception ex)
        {
            LogError(ex, $"Unhandled exception in {nameof(LoadStorageDataAsync)}");
        }
        finally
        {
            if (IsPageLoad)
                IsPageLoad = false;
            RefreshLicenseInfo();
        }
    }

    /// <summary> Load online data with async task </summary>
    protected async Task LoadOnlineDataAsync(Func<Task> task)
    {
        try
        {
            if (!IsOnlineProcessing)
            {
                IsOnlineProcessing = true;
                await Task.Delay(250);
            }

            await TgDesktopUtils.InvokeOnUIThreadAsync(task);
        }
        finally
        {
            if (IsOnlineProcessing)
                IsOnlineProcessing = false;
            RefreshLicenseInfo();
        }
    }

    /// <summary> Load online data with action </summary>
    protected async Task LoadOnlineDataAsync(Action action)
    {
        try
        {
            if (!IsOnlineProcessing)
            {
                IsOnlineProcessing = true;
                await Task.Delay(250);
            }

            TgDesktopUtils.InvokeOnUIThread(action);
        }
        finally
        {
            if (IsOnlineProcessing)
                IsOnlineProcessing = false;
            RefreshLicenseInfo();
        }
    }

    /// <summary> Refresh license </summary>
    public void RefreshLicenseInfo()
    {
        IsPaidLicense = TgDesktopUtils.VerifyPaidLicense();
        LicenseType = TgDesktopUtils.GetLicenseType();
    }

    /// <summary> Show required license dialog </summary>
    private async Task ShowPurchaseLicenseAsync() => 
        await ContentDialogAsync(TgResourceExtensions.GetFeatureLicenseManager, TgResourceExtensions.GetFeatureLicensePurchase);

    public void LogInformation(string message,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        Logger?.LogInformation($"[{Name}] | {message}");
        TgLogUtils.WriteLogWithCallerCore($"{Name} | {message}", filePath, lineNumber, memberName);
    }

    public void LogDebug(string message,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        Logger?.LogDebug($"{Name} | {message}");
        TgLogUtils.WriteLogWithCallerCore($"{Name} | {message}", filePath, lineNumber, memberName);
    }

    public void LogWarning(string message,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        Logger?.LogWarning($"{Name} | {message}");
        TgLogUtils.WriteLogWithCallerCore($"{Name} | {message}", filePath, lineNumber, memberName);
    }

    public void LogError(Exception ex, string message = "",
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        Logger?.LogError(ex, $"{Name} | {message}");
        TgLogUtils.WriteExceptionWithMessageCore(ex, $"{Name} | {message}", filePath, lineNumber, memberName);
    }

    #endregion
}
