namespace OpenTgResearcherDesktop.Common;

/// <summary> Base class for TgViewModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public abstract partial class TgPageViewModelBase : TgSensitiveModel, ITgPageViewModel
{
    #region Fields, properties, constructor

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
    public partial TgDownloadSettingsViewModel DownloadSettings { get; set; } = new();
    [ObservableProperty]
    public partial bool IsEmptyData { get; set; } = true;
    [ObservableProperty]
    public partial bool IsLicense { get; set; }
    [ObservableProperty]
    public partial TgEnumLicenseType LicenseType { get; set; } = TgEnumLicenseType.No;

    public IAsyncRelayCommand ShowPurchaseLicenseCommand { get; }

    public TgPageViewModelBase(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgPageViewModelBase> logger, string name) : base(loadStateService, settingsService)
    {
        NavigationService = navigationService;
        Logger = logger;
        Name = name;
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

    public virtual async Task OnNavigatedToAsync(NavigationEventArgs? e)
    {
        await Task.CompletedTask;
    }

    public virtual async Task ReloadUiAsync()
    {
        ConnectionDt = string.Empty;
        ConnectionMsg = string.Empty;
        Exception.Default();
        await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync();
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
    public virtual void UpdateException(Exception ex) => Exception = new(ex);

    /// <summary> Create a base ContentDialog with common settings </summary>
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
    public async Task LoadStorageDataAsync(Func<Task> task)
    {
        try
        {
            if (!LoadStateService.IsStorageProcessing)
            {
                LoadStateService.IsStorageProcessing = true;
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
            if (LoadStateService.IsStorageProcessing)
            {
                LoadStateService.IsStorageProcessing = false;
                await Task.Delay(250);
            }
            RefreshLicenseInfo();
        }
    }

    /// <summary> Load storage data with action </summary>
    public async Task LoadStorageDataAsync(Action action)
    {
        try
        {
            if (!LoadStateService.IsStorageProcessing)
            {
                LoadStateService.IsStorageProcessing = true;
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
            if (LoadStateService.IsStorageProcessing)
            {
                LoadStateService.IsStorageProcessing = false;
                await Task.Delay(250);
            }
            RefreshLicenseInfo();
        }
    }

    /// <summary> Load online data with async task </summary>
    protected async Task LoadOnlineDataAsync(Func<Task> task)
    {
        try
        {
            if (!LoadStateService.IsOnlineProcessing)
            {
                LoadStateService.IsOnlineProcessing = true;
                await Task.Delay(250);
            }

            await TgDesktopUtils.InvokeOnUIThreadAsync(task);
        }
        finally
        {
            if (LoadStateService.IsOnlineProcessing)
            {
                LoadStateService.IsOnlineProcessing = false;
                await Task.Delay(250);
            }
            RefreshLicenseInfo();
        }
    }

    /// <summary> Load online data with action </summary>
    protected async Task LoadOnlineDataAsync(Action action)
    {
        try
        {
            if (!LoadStateService.IsOnlineProcessing)
            {
                LoadStateService.IsOnlineProcessing = true;
                await Task.Delay(250);
            }

            TgDesktopUtils.InvokeOnUIThread(action);
        }
        finally
        {
            if (LoadStateService.IsOnlineProcessing)
            {
                LoadStateService.IsOnlineProcessing = false;
                await Task.Delay(250);
            }
            RefreshLicenseInfo();
        }
    }

    /// <summary> Refresh license </summary>
    public void RefreshLicenseInfo()
    {
        IsLicense = TgDesktopUtils.VerifyLicense();
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
