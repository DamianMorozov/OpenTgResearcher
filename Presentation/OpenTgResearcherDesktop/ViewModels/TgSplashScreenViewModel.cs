namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgSplashScreenViewModel : TgPageViewModelBase, IDisposable
{
    #region Fields, properties, constructor

    [ObservableProperty] public partial bool IsLoadComplete { get; set; }
    [ObservableProperty] public partial bool IsLoadHardwareResourceMonitoring { get; set; }
    [ObservableProperty] public partial bool IsLoadLicense { get; set; }
    [ObservableProperty] public partial bool IsLoadLogging { get; set; }
    [ObservableProperty] public partial bool IsLoadNotifications { get; set; }
    [ObservableProperty] public partial bool IsLoadSettings { get; set; }
    [ObservableProperty] public partial bool IsLoadStorage { get; set; }
    [ObservableProperty] public partial bool IsLoadVelopack { get; set; }
    [ObservableProperty] public partial string AppVersion { get; set; } = string.Empty;

    private ITgHardwareResourceMonitoringService HardwareResourceMonitoringService { get; }
    private ILifetimeScope Scope { get; } = default!;

    public Action BackToMainWindow { get; internal set; } = () => { };
    public IRelayCommand ContinueCommand { get; }

    public TgSplashScreenViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgSplashScreenViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgSplashScreenViewModel))
    {
        Scope = TgGlobalTools.Container.BeginLifetimeScope();
        HardwareResourceMonitoringService = Scope.Resolve<ITgHardwareResourceMonitoringService>();
        // App version + Storage version + License
        AppVersion = TgResourceExtensions.GetAppDisplayName() + $" v{TgDataUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
        // Commands
        ContinueCommand = new AsyncRelayCommand(ContinueAsync);
    }

    #endregion

    #region IDisposable

    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
	~TgSplashScreenViewModel() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    public void CheckIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary> Release managed resources </summary>
    public void ReleaseManagedResources()
    {
        CheckIfDisposed();

        HardwareResourceMonitoringService.Dispose();

        Scope.Dispose();
    }

    /// <summary> Release unmanaged resources </summary>
    public void ReleaseUnmanagedResources()
    {
        CheckIfDisposed();

        //
    }

    /// <summary> Dispose pattern </summary>
    public void Dispose()
    {
        // Dispose of unmanaged resources
        Dispose(true);
        // Suppress finalization
        GC.SuppressFinalize(this);
    }

    /// <summary> Dispose pattern </summary>
    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        // Release managed resources
        if (disposing)
            ReleaseManagedResources();
        // Release unmanaged resources
        ReleaseUnmanagedResources();
        // Flag
        _disposed = true;
    }

    #endregion

    #region Methods

    private async Task ContinueAsync()
    {
        BackToMainWindow.Invoke();
        // Loading settings
        await LoadingSettingsAsync();
    }

    /// <summary> Loading logging </summary>
    internal async Task LoadingLoggingAsync()
    {
        try
        {
            LogInformation(TgResourceExtensions.GetLoggerLoading());
            IsLoadLogging = true;
            await Task.Delay(250);
        }
        catch (Exception ex)
        {
            LogError(ex);
        }
    }

    /// <summary> Loading Velopack Installer </summary>
    internal async Task LoadingVelopackInstallerAsync()
    {
        try
        {
            VelopackApp.Build()
                .OnFirstRun((v) =>
                {
                    LogInformation($"  Thanks for installing the {TgConstants.OpenTgResearcherDesktop}!");
                })
                .Run();
            IsLoadVelopack = true;
            await Task.Delay(250);
        }
        catch (Exception ex)
        {
            LogError(ex);
        }
    }

    /// <summary> Loading settings </summary>
    internal async Task LoadingSettingsAsync()
    {
        try
        {
            var settingsService = App.GetService<ITgSettingsService>();
            settingsService.SetTheme(settingsService.AppTheme);
            var theme = TgThemeUtils.GetElementTheme(settingsService.AppTheme);
            TgTitleBarHelper.UpdateTitleBar(theme);
            IsLoadSettings = true;
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogError(ex);
        }
    }

    /// <summary> Loading storage </summary>
    internal async Task LoadingStorageAsync()
    {
        try
        {
            // Create and update storage
            await App.BusinessLogicManager.StorageManager.CreateAndUpdateDbAsync();
            // Storage version
            var versionRepository = new TgEfVersionRepository();
            var versionsResult = await versionRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
            var version = versionsResult.Items.Single(x => x.Version == versionRepository.LastVersion);
            TgAppSettingsHelper.Instance.StorageVersion = $"v{version.Version}";
            LogInformation(TgResourceExtensions.GetStorageLoading());
            IsLoadStorage = true;
        }
        catch (Exception ex)
        {
            LogError(ex);
        }
    }

    /// <summary> Loading license </summary>
    internal async Task LoadingLicenseAsync()
    {
        try
        {
            await App.BusinessLogicManager.LicenseService.LicenseActivateAsync();
            LogInformation(TgResourceExtensions.GetLicenseLoading());
            IsLoadLicense = true;
        }
        catch (Exception ex)
        {
            LogError(ex);
        }
    }

    /// <summary> Loading notifications </summary>
    internal async Task LoadingNotificationsAsync()
    {
        try
        {
            var notificationService = App.GetService<IAppNotificationService>();
            notificationService.Initialize();
            notificationService.Show(string.Format("AppNotificationSamplePayload".GetLocalizedResource(), AppContext.BaseDirectory));
            LogInformation(TgResourceExtensions.GetNotificationLoading());
            IsLoadNotifications = true;
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogError(ex);
        }
    }

    /// <summary> Loading hardware resource monitoring </summary>
    internal async Task LoadingHardwareControlAsync()
    {
        try
        {
            await HardwareResourceMonitoringService.StartMonitoringAsync();
            IsLoadHardwareResourceMonitoring = true;
        }
        catch (Exception ex)
        {
            LogError(ex);
            throw;
        }
    }

    /// <summary> Loading complete </summary>
    internal async Task LoadingCompleteAsync()
    {
        IsLoadComplete = true;
        await Task.Delay(250);
    }

    #endregion
}
