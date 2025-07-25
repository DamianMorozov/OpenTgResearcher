﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
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
    public partial bool IsLoadSettings { get; set; }
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
        ContinueCommand = new AsyncRelayCommand(ContinueAsync);
    }

    #endregion

    #region Public and private methods

    private async Task ContinueAsync()
    {
        BackToMainWindow.Invoke();
        // Loading settings
        await LoadingSettingsAsync();
    }

    /// <summary> Loading logging </summary>
    public async Task LoadingLoggingAsync()
    {
        try
        {
            TgLogUtils.WriteLog(TgResourceExtensions.GetLoggerLoading());
            IsLoadLogging = true;
            await Task.Delay(250);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    /// <summary> Loading Velopack Installer </summary>
    public async Task LoadingVelopackInstallerAsync()
    {
        try
        {
            VelopackApp.Build()
                .OnFirstRun((v) =>
                {
                    TgLogUtils.WriteLog($"  Thanks for installing the {TgConstants.OpenTgResearcherDesktop}!");
                })
                .Run();
            IsLoadVelopack = true;
            await Task.Delay(250);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    /// <summary> Loading settings </summary>
    public async Task LoadingSettingsAsync()
    {
        try
        {
            var settingsService = App.GetService<ITgSettingsService>();
            settingsService.SetTheme(settingsService.AppTheme);
            var theme = TgThemeUtils.GetElementTheme(settingsService.AppTheme);
            TgTitleBarHelper.UpdateTitleBar(theme);
            IsLoadSettings = true;
            await Task.Delay(250);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    /// <summary> Loading storage </summary>
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
            TgLogUtils.WriteLog(TgResourceExtensions.GetStorageLoading());
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
            await App.BusinessLogicManager.LicenseService.LicenseActivateAsync();
            TgLogUtils.WriteLog(TgResourceExtensions.GetLicenseLoading());
            IsLoadLicense = true;
            await Task.Delay(250);

        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    /// <summary> Loading notifications </summary>
    public async Task LoadingNotificationsAsync()
    {
        try
        {
            var notificationService = App.GetService<IAppNotificationService>();
            notificationService.Initialize();
            notificationService.Show(string.Format("AppNotificationSamplePayload".GetLocalizedResource(), AppContext.BaseDirectory));
            TgLogUtils.WriteLog(TgResourceExtensions.GetNotificationLoading());
            IsLoadNotifications = true;
            await Task.Delay(250);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    /// <summary> Loading complete </summary>
    public async Task LoadingCompleteAsync()
    {
        IsLoadComplete = true;
        await Task.Delay(250);
    }

    #endregion
}
