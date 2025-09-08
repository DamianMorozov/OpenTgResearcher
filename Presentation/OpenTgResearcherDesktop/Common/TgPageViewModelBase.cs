// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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
    public partial bool IsEnabledContent { get; set; }
    [ObservableProperty]
    public partial bool IsOnlineProcessing { get; set; }
    [ObservableProperty]
    public partial TgDownloadSettingsViewModel DownloadSettings { get; set; } = new();
    [ObservableProperty]
    public partial bool IsEmptyData { get; set; } = true;
    [ObservableProperty]
    public partial bool IsPaidLicense { get; set; }

    private TgChatViewModel? _chatVm;
    private TgChatsViewModel? _chatsVm;

    public TgPageViewModelBase(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgPageViewModelBase> logger, string name) : base()
    {
        SettingsService = settingsService;
        NavigationService = navigationService;
        Logger = logger;
        Name = name;
        IsDisplaySensitiveData = NavigationService.IsDisplaySensitiveData;
    }

    #endregion

    #region Methods

    public virtual string ToDebugString() => TgObjectUtils.ToDebugString(this);

    public virtual void OnLoaded(object parameter)
    {
        if (parameter is XamlRoot xamlRoot)
        {
            XamlRootVm = xamlRoot;
            Logger.LogInformation("Page loaded.");
        }
        else
            Logger.LogInformation("Page loaded without XamlRoot.");
    }

    public virtual async Task OnNavigatedToAsync(NavigationEventArgs? e) =>
        await LoadDataAsync(async () =>
        {
            IsDisplaySensitiveData = NavigationService.IsDisplaySensitiveData;
            await Task.CompletedTask;
        });

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
    public virtual async Task UpdateStateProxyAsync(string message)
    {
        if (App.MainWindow?.DispatcherQueue is not null)
        {
            var tcs = new TaskCompletionSource();
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    StateProxyDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
                    StateProxyMsg = message;
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            await tcs.Task;
        }
    }

    /// <summary> Update exception message </summary>
    public virtual async Task UpdateExceptionAsync(Exception ex)
    {
        Exception = new(ex);
        await Task.CompletedTask;
    }

    /// <summary> Update chats ViewModel </summary>
    public async Task UpdateChatsViewModelAsync(int counter, int countAll, TgEnumChatsMessageType chatsMessageType)
    {
        if (App.MainWindow?.DispatcherQueue is not null)
        {
            var tcs = new TaskCompletionSource<bool>();
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    UpdateChatsViewModelCore(ref counter, ref countAll, ref chatsMessageType);
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            await tcs.Task;
        }
        else
        {
            UpdateChatsViewModelCore(ref counter, ref countAll, ref chatsMessageType);
        }
    }

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
    public async Task UpdateChatViewModelAsync(long chatId, int messageId, int count, string message)
    {
        if (App.MainWindow?.DispatcherQueue is not null)
        {
            var tcs = new TaskCompletionSource<bool>();
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    UpdateChatViewModelCore(chatId, messageId, count, ref message);
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            await tcs.Task;
        }
        else
        {
            UpdateChatViewModelCore(chatId, messageId, count, ref message);
        }
    }

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
        bool isStartTask, int threadNumber)
    {
        if (App.MainWindow?.DispatcherQueue is not null)
        {
            var tcs = new TaskCompletionSource<bool>();
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    UpdateStateFileCore(chatId, messageId, fileName, fileSize, transmitted, fileSpeed, isStartTask, threadNumber);
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            await tcs.Task;
        }
        else
        {
            UpdateStateFileCore(chatId, messageId, fileName, fileSize, transmitted, fileSpeed, isStartTask, threadNumber);
        }
    }

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

    protected async Task ContentDialogAsync(string title, string content)
    {
        if (XamlRootVm is null) return;
        ContentDialog dialog = new()
        {
            XamlRoot = XamlRootVm,
            Title = title,
            Content = content,
            CloseButtonText = TgResourceExtensions.GetOkButton(),
            DefaultButton = ContentDialogButton.Close,
        };
        _ = await dialog.ShowAsync();
    }

    protected async Task ContentDialogAsync(Func<Task> task, string title, ContentDialogButton defaultButton = ContentDialogButton.Close,
        bool useLoadData = false)
    {
        if (XamlRootVm is null) return;
        ContentDialog dialog = new()
        {
            XamlRoot = XamlRootVm,
            Title = title,
            PrimaryButtonText = TgResourceExtensions.GetYesButton(),
            CloseButtonText = TgResourceExtensions.GetCancelButton(),
            DefaultButton = defaultButton,
            PrimaryButtonCommand = new AsyncRelayCommand(useLoadData ? async () => await LoadDataAsync(task) : task)
        };
        _ = await dialog.ShowAsync();
    }

    protected async Task ContentDialogAsync(Action action, string title, ContentDialogButton defaultButton = ContentDialogButton.Close)
    {
        if (XamlRootVm is null) return;
        ContentDialog dialog = new()
        {
            XamlRoot = XamlRootVm,
            Title = title,
            PrimaryButtonText = TgResourceExtensions.GetYesButton(),
            CloseButtonText = TgResourceExtensions.GetCancelButton(),
            DefaultButton = defaultButton,
            PrimaryButtonCommand = new RelayCommand(action)
        };
        _ = await dialog.ShowAsync();
    }

    protected async Task LoadDataAsync(Func<Task> task)
    {
        try
        {
            IsEnabledContent = false;
            IsPageLoad = true;
            if (App.BusinessLogicManager.LicenseService.CurrentLicense is not null)
            {
                DownloadSettings.LimitThreads = TgGlobalTools.DownloadCountThreadsLimit;
            }
            await Task.Delay(250);
            await task();
        }
        finally
        {
            IsEnabledContent = true;
            IsPageLoad = false;
            IsPaidLicense = App.BusinessLogicManager.LicenseService.CurrentLicense?.CheckPaidLicense() ?? false;
        }
    }

    protected async Task ProcessDataAsync(Func<Task> task, bool isDisabledContent, bool isPageLoad)
    {
        try
        {
            if (!IsOnlineProcessing)
                IsOnlineProcessing = true;

            if (isDisabledContent)
                IsEnabledContent = false;
            if (isPageLoad)
                IsPageLoad = true;
            if (App.BusinessLogicManager.LicenseService.CurrentLicense is not null)
            {
                DownloadSettings.LimitThreads = TgGlobalTools.DownloadCountThreadsLimit;
            }
            await Task.Delay(250);
            await task();
        }
        finally
        {
            if (isDisabledContent)
                IsEnabledContent = true;
            if (isPageLoad)
                IsPageLoad = false;
            if (IsOnlineProcessing)
                IsOnlineProcessing = false;
            IsPaidLicense = App.BusinessLogicManager.LicenseService.CurrentLicense?.CheckPaidLicense() ?? false;
        }
    }

    #endregion
}
