namespace OpenTgResearcherDesktop.ViewModels;

public partial class ShellViewModel : TgSensitiveModel
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
    public partial int FloodWaitSeconds { get; set; }
    [ObservableProperty]
    public partial string FloodMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsFloodVisible { get; set; }
    [ObservableProperty]
    public partial Guid UidSavedMessages { get; set; } = Guid.Empty;

    public NavigationEventArgs? EventArgs { get; private set; }

    public IAppNotificationService AppNotificationService { get; }
    public INavigationService NavigationService { get; }
    public INavigationViewService NavigationViewService { get; }

    public IAsyncRelayCommand ClientConnectCommand { get; set; } = default!;
    public IAsyncRelayCommand ClientDisconnectCommand { get; set; } = default!;
    public IAsyncRelayCommand UpdatePageCommand { get; set; } = default!;

    public ShellViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        INavigationViewService navigationViewService, IAppNotificationService appNotificationService) : base(loadStateService, settingsService)
    {
        AppNotificationService = appNotificationService;
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
    }

    #endregion

    #region Methods

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        EventArgs = e;
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

    #endregion
}
