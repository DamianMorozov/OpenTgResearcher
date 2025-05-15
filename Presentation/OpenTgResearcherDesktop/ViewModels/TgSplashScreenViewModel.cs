// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgSplashScreenViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial string AppVersion { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsLoadNotifications { get; set; }
    [ObservableProperty]
    public partial bool IsLoadLogging { get; set; }
    [ObservableProperty]
    public partial bool IsLoadStorage { get; set; }
    [ObservableProperty]
    public partial bool IsLoadLicense { get; set; }
    [ObservableProperty]
    public partial bool IsLoadComplete { get; set; }

    public Action BackToMainWindow { get; internal set; } = () => { };
    public IRelayCommand ContinueCommand { get; }

    public TgSplashScreenViewModel(ITgSettingsService settingsService, INavigationService navigationService, ITgLicenseService licenseService, ILogger<TgSplashScreenViewModel> logger)
        : base(settingsService, navigationService, licenseService, logger, nameof(TgSplashScreenViewModel))
    {
        // App version + Storage version + License
        AppVersion = TgResourceExtensions.GetAppDisplayName() + $" v{TgCommonUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
        // Commands
        ContinueCommand = new RelayCommand(ContinueAction);
    }

    #endregion

    #region Public and private methods

    private Action ContinueAction => () => BackToMainWindow.Invoke();

    public async Task LoadingNotificationsAsync()
    {
        await Task.Delay(250);
        var notificationService = App.GetService<IAppNotificationService>();
        notificationService.Initialize();
        notificationService.Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));
        TgLogUtils.LogInformation(TgResourceExtensions.GetNotificationLoading());
        IsLoadNotifications = true;
    }

    public async Task LoadingLoggingAsync()
    {
        await Task.Delay(250);
        var appFolder = App.GetService<ITgSettingsService>().AppFolder;
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(Path.Combine(appFolder, $"{TgFileUtils.LogsDirectory}/Log-.txt"), rollingInterval: RollingInterval.Day, shared: true)
            .CreateLogger();
        TgLogUtils.LogInformation(TgResourceExtensions.GetLoggerLoading());
        IsLoadLogging = true;
        await Task.CompletedTask;
    }

    public async Task LoadingStorageAsync()
    {
        // Default License loading
        App.GetService<ITgLicenseService>().ActivateLicenseWithDescriptions(
            TgResourceExtensions.GetLicenseFreeDescription(), TgResourceExtensions.GetLicenseTestDescription(),
            TgResourceExtensions.GetLicensePaidDescription(), TgResourceExtensions.GetLicensePremiumDescription());
        await Task.Delay(250);
        // Loading storage
        // Register TgEfContext as the DbContext for EF Core
        TgEfUtils.AppStorage = App.GetService<ITgSettingsService>().AppStorage;
        // Create and update storage
        await TgEfUtils.CreateAndUpdateDbAsync();
        // Storage version
        var versionRepository = new TgEfVersionRepository();
        var versionsResult = await versionRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
        var version = versionsResult.Items.Single(x => x.Version == versionRepository.LastVersion);
        TgAppSettingsHelper.Instance.StorageVersion = $"v{version.Version}";
        TgLogUtils.LogInformation(TgResourceExtensions.GetStorageLoading());
        IsLoadStorage = true;
    }

    public async Task LoadingLicenseAsync()
    {
        await Task.Delay(250);
        await App.GetService<ITgLicenseService>().LicenseActivateAsync();
        TgLogUtils.LogInformation(TgResourceExtensions.GetLicenseLoading());
        IsLoadLicense = true;
    }

    public async Task LoadingCompleteAsync()
    {
        await Task.Delay(250);
        IsLoadComplete = true;
    }

    #endregion
}
