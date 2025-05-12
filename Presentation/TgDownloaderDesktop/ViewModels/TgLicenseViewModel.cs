// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgLicenseViewModel : TgPageViewModelBase
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial ILicenseService LicenseService { get; private set; } = default!;

	[ObservableProperty]
	public partial string AppVersionFull { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string VersionDescription { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string AppVersionTitle { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string AppVersionShort { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string IsConfirmed { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string LicenseKey { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string LicenseDescription { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string UserId { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string Expiration { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string LicenseLog { get; set; } = string.Empty;

	private ITgEfLicenseRepository LicenseRepository { get; } = new TgEfLicenseRepository();

	public IRelayCommand LicenseShowInfoCommand { get; }
	public IRelayCommand LicenseClearCommand { get; }
	public IRelayCommand LicenseCheckCommand { get; }
	public IRelayCommand LicenseChangeCommand { get; }

	public TgLicenseViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgLicenseViewModel> logger,
		ILicenseService licenseService) 
		: base(settingsService, navigationService, logger, nameof(TgLicenseViewModel))
	{
		LicenseService = licenseService;
		
		AppVersionShort = $"v{TgCommonUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
		AppVersionFull = $"{TgResourceExtensions.GetAppVersion()}: {AppVersionShort}";
		VersionDescription = GetVersionDescription();
		AppVersionTitle =
			$"{TgConstants.AppTitleDesktop} " +
			$"v{TgCommonUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
		// Commands
		LicenseShowInfoCommand = new AsyncRelayCommand(LicenseShowInfoAsync);
		LicenseClearCommand = new AsyncRelayCommand(LicenseClearAsync);
		LicenseCheckCommand = new AsyncRelayCommand(LicenseCheckAsync);
		LicenseChangeCommand = new AsyncRelayCommand(LicenseChangeAsync);
	}

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await LoadDataAsync(async () =>
	{
		await ReloadUiAsync();

		await LicenseService.LicenseActivateAsync();
		await LicenseShowInfoAsync();
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

	private async Task LicenseShowInfoAsync() 
	{
		try
		{
			IsConfirmed = TgLicenseManager.CurrentLicense.IsConfirmed.ToString();
			LicenseKey = TgLicenseManager.CurrentLicense.GetLicenseKeyString();
			LicenseDescription = TgLicenseManager.CurrentLicense.Description;
			UserId = TgLicenseManager.CurrentLicense.GetUserIdString();
			Expiration = TgLicenseManager.CurrentLicense.GetValidToString();
		}
		catch (Exception ex)
		{
			TgLogUtils.LogFatal(ex);
		}
		await Task.CompletedTask;
	}

	private async Task LicenseClearAsync() => await ContentDialogAsync(LicenseClearCoreAsync, TgResourceExtensions.AskDataClear());

	private async Task LicenseClearCoreAsync()
	{
		await LicenseRepository.DeleteAllAsync();
		await LicenseService.LicenseActivateAsync();
		await LicenseShowInfoAsync();
	}

	private async Task LicenseCheckAsync()
	{
		try
		{
			LicenseLog = TgResourceExtensions.GetActionCheckLicenseOnlineMsg() + Environment.NewLine;
			var userId = await GetUserIdAsync();
			if (userId == 0)
				return;

			var apiUrls = new[] { TgLicenseManager.MenuWebSiteGlobalUrl, TgLicenseManager.MenuWebSiteRussianUrl };
			using var httpClient = new HttpClient();
			httpClient.Timeout = TimeSpan.FromSeconds(10);

			foreach (var apiUrl in apiUrls)
			{
				try
				{
					var url = $"{apiUrl}License/Get?userId={userId}";
					var response = await httpClient.GetAsync(url);
					LicenseLog += $"{TgResourceExtensions.GetMenuLicenseCheckServer()}: {apiUrl}" + Environment.NewLine;
					if (!response.IsSuccessStatusCode)
					{
						LicenseLog += $"{TgResourceExtensions.GetMenuLicenseResponseStatusCode()}: {response.StatusCode}" + Environment.NewLine;
						continue;
					}

					var jsonResponse = await response.Content.ReadAsStringAsync();
					var licenseData = JsonSerializer.Deserialize<TgLicenseDto>(jsonResponse, TgJsonSerializerUtils.GetJsonOptions());
					if (licenseData?.IsConfirmed != true)
					{
						LicenseLog += $"{TgResourceExtensions.GetMenuLicenseIsNotCofirmed()}: {response.StatusCode}" + Environment.NewLine;
						continue;
					}

					// Updating an existing license or creating a new license
					var licenseEntity = new TgEfLicenseEntity
					{
						IsConfirmed = licenseData.IsConfirmed,
						LicenseKey = licenseData.LicenseKey,
						LicenseType = licenseData.LicenseType,
						UserId = licenseData.UserId,
						ValidTo = DateTime.Parse($"{licenseData.ValidTo:yyyy-MM-dd}")
					};

					var licenseDtos = await LicenseRepository.GetListDtosAsync();
					var currentLicenseDto = licenseDtos.FirstOrDefault(x => x.IsConfirmed && x.ValidTo >= DateTime.UtcNow);
					if (currentLicenseDto is null)
					{
						await LicenseRepository.SaveAsync(licenseEntity);
					}
					else
					{
						var licenseExists = await LicenseRepository.GetItemAsync(licenseEntity, isReadOnly: false);
						licenseExists.Copy(licenseEntity, isUidCopy: false);
						await LicenseRepository.SaveAsync(licenseEntity);
					}
					LicenseLog += TgResourceExtensions.GetMenuLicenseUpdatedSuccessfully() + Environment.NewLine;
					return;
				}
				catch (Exception ex)
				{
					TgLogUtils.LogFatal(ex);
				}
			}
		}
		finally
		{
			await LicenseService.LicenseActivateAsync();
			await LicenseShowInfoAsync();
		}
	}

	private async Task<long> GetUserIdAsync()
	{
		if (TgGlobalTools.ConnectClient.Me is null)
			await TgGlobalTools.ConnectClient.LoginUserAsync(isProxyUpdate: false);
		var userId = TgGlobalTools.ConnectClient.Me?.ID ?? 0;
		if (userId == 0)
		{
			await LicenseShowInfoAsync();
			LicenseLog += TgResourceExtensions.GetMenuLicenseUserNotLoggedIn();
		}
		return userId;
	}

	private async Task LicenseChangeAsync()
	{
		try
		{
			Process.Start(new ProcessStartInfo(TgLicenseManager.MenuWebSiteGlobalUrl) { UseShellExecute = true });
		}
		catch (Exception ex)
		{
			TgLogUtils.LogFatal(ex);
		}
		finally
		{
			await Task.CompletedTask;
		}

	}

	#endregion
}
