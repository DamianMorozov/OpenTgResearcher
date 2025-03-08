// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgLicenseViewModel : TgPageViewModelBase
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial string AppVersionFull { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string VersionDescription { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string AppVersionTitle { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string AppVersionShort { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string LicenseDescription { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string LicenseLog { get; set; } = string.Empty;
	public IRelayCommand CheckLicenseOnlineCommand { get; }
	public IRelayCommand ChangeLicenseOnlineCommand { get; }

	public TgLicenseViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgLicenseViewModel> logger) 
		: base(settingsService, navigationService, logger)
	{
		AppVersionShort = $"v{TgCommonUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
		AppVersionFull = $"{TgResourceExtensions.GetAppVersion()}: {AppVersionShort}";
		VersionDescription = GetVersionDescription();
		AppVersionTitle =
			$"{TgConstants.AppTitleDesktop} " +
			$"v{TgCommonUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
		LicenseDescription = LicenseManager.CurrentLicense.Description;
		// Commands
		CheckLicenseOnlineCommand = new AsyncRelayCommand(CheckLicenseOnlineAsync);
		ChangeLicenseOnlineCommand = new AsyncRelayCommand(ChangeLicenseOnlineAsync);
	}

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await LoadDataAsync(async () =>
	{
		await ReloadUiAsync();
	});

	private static string GetVersionDescription()
	{
		Version version;
		if (TgRuntimeHelper.IsMSIX)
		{
			var packageVersion = Package.Current.Id.Version;
			version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
		}
		else
		{
			version = Assembly.GetExecutingAssembly().GetName().Version!;
		}
		return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
	}

	private async Task CheckLicenseOnlineAsync() 
	{
		LicenseLog = string.Empty;
		try
		{
			LicenseLog = TgResourceExtensions.GetActionCheckLicenseOnlineMsg() + Environment.NewLine;
			LicenseLog += TgResourceExtensions.GetInDevelopment();
		}
		catch (Exception ex)
		{
			TgLogUtils.LogFatal(ex);
		}
		await Task.CompletedTask;
	}

	private async Task ChangeLicenseOnlineAsync()
	{
		LicenseLog = string.Empty;
		try
		{
			LicenseLog = TgResourceExtensions.GetActionChangeLicenseOnlineMsg() + Environment.NewLine;
			LicenseLog += TgResourceExtensions.GetInDevelopment();
		}
		catch (Exception ex)
		{
			TgLogUtils.LogFatal(ex);
		}
		await Task.CompletedTask;
	}

	#endregion
}
