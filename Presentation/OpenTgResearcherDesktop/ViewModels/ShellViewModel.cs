// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial bool IsBackEnabled { get; set; }
    [ObservableProperty]
    public partial object? Selected { get; set; }
    [ObservableProperty]
    public partial string AppVersion { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string StorageVersion { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string License { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsClientConnected { get; set; }
    [ObservableProperty]
    public partial int FloodWaitSeconds { get; set; }
    [ObservableProperty]
    public partial string FloodMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsFloodVisible { get; set; }

    private NavigationEventArgs? _eventArgs;
    private ShellViewModel? _shellVm;
    private CancellationTokenSource? _floodCts;

    public INavigationService NavigationService { get; }
    public INavigationViewService NavigationViewService { get; }
    public IAppNotificationService AppNotificationService { get; }

    public IRelayCommand ClientConnectCommand { get; }
    public IRelayCommand ClientDisconnectCommand { get; }
    public IRelayCommand UpdatePageCommand { get; }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService, IAppNotificationService appNotificationService)
    {
        AppNotificationService = appNotificationService;
        AppNotificationService.ClientConnectionChanged += OnClientConnectionChanged;
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
        // Commands
        ClientConnectCommand = new AsyncRelayCommand(ShellClientConnectAsync);
        ClientDisconnectCommand = new AsyncRelayCommand(ShellClientDisconnectAsync);
        UpdatePageCommand = new AsyncRelayCommand(ShellUpdatePageAsync);
        // Callback updates UI
        App.BusinessLogicManager.ConnectClient.SetupUpdateShellViewModel(UpdateShellViewModelAsync);
    }

    #endregion

    #region Methods

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        _eventArgs = e;
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(TgSettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        SetMenuItemByPageAndParameter(e.SourcePageType, e.Parameter);

        // App version + Storage version + License
        AppVersion = TgResourceExtensions.GetAppDisplayName() + $" v{TgDataUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
        StorageVersion = $"{TgResourceExtensions.GetStorage()} {TgAppSettingsHelper.Instance.StorageVersion}";
        if (App.BusinessLogicManager.LicenseService.CurrentLicense is not null)
        {
            License = App.BusinessLogicManager.LicenseService.CurrentLicense.Description;
        }
    }

    private void SetMenuItemByPageAndParameter(Type pageType, object? parameter)
    {
        if (NavigationViewService.MenuItems is null) return;

        var userTypeStr = parameter?.ToString();
        foreach (var item in NavigationViewService.MenuItems)
        {
            if (item is NavigationViewItem nvi)
            {
                var fullName = TgNavigationHelper.GetNavigateTo(nvi);
                var navParam = TgNavigationHelper.GetNavigationParameter(nvi)?.ToString();
                if (IsViewModelAndPageNamesMatching(fullName, pageType.FullName))
                {
                    if (string.Equals(navParam, userTypeStr, StringComparison.OrdinalIgnoreCase))
                    {
                        if (nvi is not null)
                        {
                            Selected = nvi;
                            return;
                        }
                    }
                }
            }
        }

        var selectedItem = NavigationViewService.GetSelectedItem(pageType);
        if (selectedItem is not null)
            Selected = selectedItem;
    }

    private bool IsViewModelAndPageNamesMatching(string fullName, string? pageTypeFullName)
    {
        if (string.IsNullOrEmpty(pageTypeFullName)) return false;

        // Normalize namespace and class name for ViewModel
        if (fullName == null || pageTypeFullName == null)
            return false;

        // Replace "ViewModels" with "Views"
        var normalizedFullName = fullName.Replace(".ViewModels.", ".Views.");

        // Replace "ViewModel" suffix with empty
        if (normalizedFullName.EndsWith("ViewModel", StringComparison.Ordinal))
            normalizedFullName = normalizedFullName.Substring(0, normalizedFullName.Length - "ViewModel".Length);

        // Replace "Page" suffix from page name with empty
        var normalizedPageName = pageTypeFullName;
        if (pageTypeFullName.EndsWith("Page", StringComparison.Ordinal))
            normalizedPageName = pageTypeFullName.Substring(0, pageTypeFullName.Length - "Page".Length);

        // Compare normalized strings
        return string.Equals(normalizedFullName, normalizedPageName, StringComparison.Ordinal);
    }

    private void OnClientConnectionChanged(object? sender, bool isClientConnected) => IsClientConnected = isClientConnected;

    private async Task ShellClientConnectAsync() => await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
    {
        var clientConnectionVm = App.Locator?.Get<TgClientConnectionViewModel>();
        if (clientConnectionVm is not null)
        {
            await clientConnectionVm.OnNavigatedToAsync(_eventArgs);
            if (!clientConnectionVm.IsClientConnected)
            {
                clientConnectionVm.ClientConnectCommand.Execute(null);
            }
        }
        await Task.CompletedTask;
    });

    private async Task ShellClientDisconnectAsync() => await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
    {
        var clientConnectionVm = App.Locator?.Get<TgClientConnectionViewModel>();
        if (clientConnectionVm is not null)
        {
            if (clientConnectionVm.IsClientConnected)
            {
                clientConnectionVm.ClientDisconnectCommand.Execute(null);
            }
        }
        await Task.CompletedTask;
    });

    private async Task ShellUpdatePageAsync()
    {
        if (_eventArgs?.Content is not TgPageBase pageBase) return;
        await pageBase.ViewModel.OnNavigatedToAsync(_eventArgs);
    }

    /// <summary> Update shell ViewModel </summary>
    public async Task UpdateShellViewModelAsync(bool isFloodVisible, int seconds, string message) => await TgDesktopUtils.InvokeOnUIThreadAsync(async () =>
    {
        UpdateShellViewModelCore(ref isFloodVisible, ref seconds, ref message);
        // Start countdown if visible
        if (isFloodVisible && seconds > 0)
            StartFloodCountdown(seconds);
        else
            _floodCts?.Cancel();
        await Task.CompletedTask;
    });

    private void UpdateShellViewModelCore(ref bool isFloodVisible, ref int seconds, ref string message)
    {
        _shellVm ??= App.Locator?.Get<ShellViewModel>();
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
                    await Task.Delay(1000, _floodCts.Token);
                    seconds--;

                    TgDesktopUtils.InvokeOnUIThread(() => { FloodWaitSeconds = seconds; });
                }

                // Hide when time is up
                if (!_floodCts.Token.IsCancellationRequested)
                {
                    TgDesktopUtils.InvokeOnUIThread(() => {
                        IsFloodVisible = false;
                        FloodMessage = string.Empty;
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
