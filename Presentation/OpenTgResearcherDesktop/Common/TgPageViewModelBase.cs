// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Common;

/// <summary> Base class for TgViewModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public partial class TgPageViewModelBase : TgSensitiveModel, ITgPageViewModel
{
    #region Public and private fields, properties, constructor

    public ITgLicenseService LicenseService => App.BusinessLogicManager.LicenseService;
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
    public partial int StateSourceProgress { get; set; } = 0;
    [ObservableProperty]
    public partial string StateSourceProgressString { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StateSourceDirectory { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StateSourceDirectorySizeString { get; set; } = string.Empty;
    [ObservableProperty]
    public partial XamlRoot? XamlRootVm { get; set; }
    [ObservableProperty]
    public partial bool IsPageLoad { get; set; }
    [ObservableProperty]
    public partial bool IsOnlineReady { get; set; }
    [ObservableProperty]
    public partial bool IsEnabledContent { get; set; }
    [ObservableProperty]
    public partial bool IsDownloading { get; set; }
    [ObservableProperty]
    public partial TgDownloadSettingsViewModel DownloadSettings { get; set; } = new();

    public IRelayCommand OnClipboardWriteCommand { get; }
    public IRelayCommand OnClipboardSilentWriteCommand { get; }

    public TgPageViewModelBase(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgPageViewModelBase> logger, string name) : base()
    {
        SettingsService = settingsService;
        NavigationService = navigationService;
        Logger = logger;
        Name = name;
        IsDisplaySensitiveData = NavigationService.IsDisplaySensitiveData;

        // Commands
        OnClipboardWriteCommand = new AsyncRelayCommand<object>(OnClipboardWriteAsync);
        OnClipboardSilentWriteCommand = new AsyncRelayCommand<object>(OnClipboardSilentWriteAsync);
    }

    #endregion

    #region Public and private methods

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

    /// <summary> Update state source message </summary>
    public async Task UpdateStateSource(long sourceId, int messageId, int count, string message)
    {
        //if (App.MainWindow?.DispatcherQueue is not null)
        //{
            var tcs = new TaskCompletionSource();
            //App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            //{
                try
                {
                    float progress = messageId == 0 || count == 0 ? 0 : (float)messageId * 100 / count;
                    StateSourceProgress = (int)progress;
                    StateSourceProgressString = progress == 0 ? "{0:00.00} %" : $"{progress:#00.00} %";
                    StateSourceDt = TgDataFormatUtils.GetDtFormat(DateTime.Now);
                    StateSourceMsg = $"{messageId} | {message}";
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            //});
            await tcs.Task;
        //}
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
        }
    }

    protected async Task ProcessDataAsync(Func<Task> task, bool isDisabledContent, bool isPageLoad)
    {
        try
        {
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
        }
    }

    /// <summary> Core write text to clipboard </summary>
    private async Task OnClipboardWriteCoreAsync(object? param, bool isSilent)
    {
        if (param is null) return;
        var tag = param.ToString();
        if (string.IsNullOrEmpty(tag)) return;

        var dataPackage = new DataPackage();
        dataPackage.SetText(tag);
        Clipboard.SetContent(dataPackage);
        if (!isSilent)
            await ContentDialogAsync(TgResourceExtensions.GetClipboard(), tag);
    }

    /// <summary> Write text to clipboard </summary>
    private async Task OnClipboardWriteAsync(object? param) => await OnClipboardWriteCoreAsync(param, isSilent: false);

    /// <summary> Silent write text to clipboard </summary>
    private async Task OnClipboardSilentWriteAsync(object? param) => await OnClipboardWriteCoreAsync(param, isSilent: true);

    #endregion
}