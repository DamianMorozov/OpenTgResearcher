// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    #region Public and private fields, properties, constructor

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
    public partial bool IsShowSecretFields { get; set; }

    private NavigationEventArgs? _eventArgs;

    public INavigationService NavigationService { get; }
    public INavigationViewService NavigationViewService { get; }
    public IAppNotificationService AppNotificationService { get; }
    public ITgLicenseService LicenseService => App.BusinessLogicManager.LicenseService;

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
    }

    #endregion

    #region Public and private methods

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

    private void OnClientConnectionChanged(object? sender, bool isClientConnected)
    {
        IsClientConnected = isClientConnected;
    }

    private async Task ShellClientConnectAsync()
    {
        if (_eventArgs is null) return;
        
        await App.MainWindow.DispatcherQueue.TryEnqueueWithLogAsync(async () =>
        {
            // Trying to find an open page with ViewModel
            var frame = NavigationService.Frame;
            if (frame?.Content is TgClientConnectionPage page)
            {
                await page.ViewModel.OnNavigatedToAsync(_eventArgs);
                if (!page.ViewModel.IsClientConnected)
                {
                    await page.ViewModel.ClientConnectAsync();
                    await ShellUpdatePageAsync();
                }
                return;
            }

            // If not found - get through DI
            var vm = App.GetService<TgClientConnectionViewModel>();
            await vm.OnNavigatedToAsync(_eventArgs);
            if (!vm.IsClientConnected)
            {
                await vm.ClientConnectAsync();
                await ShellUpdatePageAsync();
            }
        });
    }

    private async Task ShellClientDisconnectAsync()
    {
        if (_eventArgs is null) return;
        await App.MainWindow.DispatcherQueue.TryEnqueueWithLogAsync(async () =>
        {
            // Trying to find an open page with ViewModel
            var frame = NavigationService.Frame;
            if (frame?.Content is TgClientConnectionPage page)
            {
                await page.ViewModel.OnNavigatedToAsync(_eventArgs);
                if (page.ViewModel.IsClientConnected)
                {
                    page.ViewModel.ClientDisconnectCommand.Execute(null);
                }
                return;
            }

            // If not found - get through DI
            var vm = App.GetService<TgClientConnectionViewModel>();
            await vm.OnNavigatedToAsync(_eventArgs);
            if (vm.IsClientConnected)
            {
                await App.BusinessLogicManager.ConnectClient.DisconnectClientAsync();
                await ShellUpdatePageAsync();
            }
        });
    }

    private async Task ShellUpdatePageAsync()
    {
        if (_eventArgs?.Content is not TgPageBase pageBase) return;
        await pageBase.ViewModel.OnNavigatedToAsync(_eventArgs);
    }

    #endregion
}
