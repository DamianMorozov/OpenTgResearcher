// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgSplashScreenViewModel : TgPageViewModelBase
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial string LoadingText { get; set; } = string.Empty;
	[ObservableProperty]
	public partial int LoadingProgress { get; set; } = 0;

	public TgSplashScreenViewModel(ITgSettingsService settingsService, INavigationService navigationService, ITgLicenseService licenseService, ILogger<TgSplashScreenViewModel> logger) 
		: base(settingsService, navigationService, licenseService, logger, nameof(TgSplashScreenViewModel))
	{
		//
	}

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await LoadDataAsync(async () =>
	{
		await ReloadUiAsync();
	});

	public async Task LoadAsync()
	{
        await Task.Delay(250);

        // Notification loading
        LoadingText = $"{TgResourceExtensions.GetNotificationLoading()}...  10%";
        var notificationService = App.GetService<IAppNotificationService>();
        notificationService.Initialize();
        await Task.Delay(250);
        TgLogUtils.LogInformation(LoadingText);
        await Task.Delay(250);
        notificationService.Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));
        
        // Logger loading
        LoadingText = $"{TgResourceExtensions.GetLoggerLoading()}...  20%";
        TgLogUtils.LogInformation(LoadingText);
        await Task.Delay(250);

        LoadingText = $"{TgResourceExtensions.GetLoggerLoading()}...  30%";
        var appFolder = App.GetService<ITgSettingsService>().AppFolder;
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(Path.Combine(appFolder, $"{TgFileUtils.LogsDirectory}/Log-.txt"), rollingInterval: RollingInterval.Day, shared: true)
            .CreateLogger();
        await Task.Delay(250);

        // Default License loading
        LoadingText = $"{TgResourceExtensions.GetLicenseLoading()}...  40%";
        App.GetService<ITgLicenseService>().ActivateLicenseWithDescriptions(
            TgResourceExtensions.GetLicenseFreeDescription(), TgResourceExtensions.GetLicenseTestDescription(),
            TgResourceExtensions.GetLicensePaidDescription(), TgResourceExtensions.GetLicensePremiumDescription());
        await Task.Delay(250);

        // Register TgEfContext as the DbContext for EF Core
        LoadingText = $"{TgResourceExtensions.GetStorageLoading()}...  50%";
        TgLogUtils.LogInformation(LoadingText);
        TgEfUtils.AppStorage = App.GetService<ITgSettingsService>().AppStorage;
        await Task.Delay(250);

        // Create and update storage
        LoadingText = $"{TgResourceExtensions.GetStorageLoading()}...  60%";
        await TgEfUtils.CreateAndUpdateDbAsync();
        await Task.Delay(250);

        // Storage version
        LoadingText = $"{TgResourceExtensions.GetStorageLoading()}...  70%";
        var versionRepository = new TgEfVersionRepository();
        var versionsResult = await versionRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
        var version = versionsResult.Items.Single(x => x.Version == versionRepository.LastVersion);
        TgAppSettingsHelper.Instance.StorageVersion = $"v{version.Version}";
        await Task.Delay(250);

        // License loading
        LoadingText = $"{TgResourceExtensions.GetLicenseLoading()}...  90%";
        await App.GetService<ITgLicenseService>().LicenseActivateAsync();
        await Task.Delay(250);
    }

    #endregion
}
