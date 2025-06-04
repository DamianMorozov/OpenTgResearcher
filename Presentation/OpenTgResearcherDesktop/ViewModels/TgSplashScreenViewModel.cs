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
    public partial bool IsLoadVelopack { get; set; }
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

    public TgSplashScreenViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgSplashScreenViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgSplashScreenViewModel))
    {
        // App version + Storage version + License
        AppVersion = TgResourceExtensions.GetAppDisplayName() + $" v{TgDataUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
        // Commands
        ContinueCommand = new RelayCommand(ContinueAction);
    }

    #endregion

    #region Public and private methods

    private Action ContinueAction => () => BackToMainWindow.Invoke();

    /// <summary> Loading Velopack Installer </summary>
    public async Task LoadingVelopackInstallerAsync()
    {
        try
        {
            VelopackApp.Build()
                //.WithBeforeUninstallFastCallback((v) =>
                //{
                //	delete / clean up some files before uninstallation
                //   UpdateLog += $"Uninstalling the {TgConstants.AppTitleConsole}!";
                //})
                //.OnFirstRun((v) =>
                //{
                //	UpdateLog += $"Thanks for installing the {TgConstants.AppTitleConsole}!";
                //})
                .Run();
            IsLoadVelopack = true;
            await Task.Delay(250);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    public async Task LoadingNotificationsAsync()
    {
        try
        {
            var notificationService = App.GetService<IAppNotificationService>();
            notificationService.Initialize();
            notificationService.Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));
            TgLogUtils.LogInformation(TgResourceExtensions.GetNotificationLoading());
            IsLoadNotifications = true;
            await Task.Delay(250);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    public async Task LoadingLoggingAsync()
    {
        try
        {
            var appFolder = App.GetService<ITgSettingsService>().AppFolder;
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(Path.Combine(appFolder, $"{TgFileUtils.LogsDirectory}/Log-.txt"), rollingInterval: RollingInterval.Day, shared: true)
                .CreateLogger();
            TgLogUtils.LogInformation(TgResourceExtensions.GetLoggerLoading());
            IsLoadLogging = true;
            await Task.Delay(250);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    public async Task LoadingStorageAsync()
    {
        try
        {
            // Create and update storage
            await App.BusinessLogicManager.CreateAndUpdateDbAsync();
            // Storage version
            var versionRepository = new TgEfVersionRepository();
            var versionsResult = await versionRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
            var version = versionsResult.Items.Single(x => x.Version == versionRepository.LastVersion);
            TgAppSettingsHelper.Instance.StorageVersion = $"v{version.Version}";
            TgLogUtils.LogInformation(TgResourceExtensions.GetStorageLoading());
            IsLoadStorage = true;
            await Task.Delay(250);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    /// <summary> Loading license </summary>
    public async Task LoadingLicenseAsync()
    {
        try
        {
            // Register TgEfContext as the DbContext for EF Core
            TgGlobalTools.AppStorage = App.GetService<ITgSettingsService>().AppStorage;

            await App.BusinessLogicManager.LicenseService.LicenseActivateAsync();
            TgLogUtils.LogInformation(TgResourceExtensions.GetLicenseLoading());
            IsLoadLicense = true;
            await Task.Delay(250);

        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    public async Task LoadingCompleteAsync()
    {
        IsLoadComplete = true;
        await Task.Delay(250);
    }

    #endregion
}
