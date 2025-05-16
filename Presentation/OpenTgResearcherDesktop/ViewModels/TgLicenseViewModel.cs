// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgLicenseViewModel : TgPageViewModelBase
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial string AppVersionFull { get; set; } = string.Empty;
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

	public IRelayCommand LicenseShowInfoCommand { get; }
	public IRelayCommand LicenseClearCommand { get; }
	public IRelayCommand LicenseCheckCommand { get; }
	public IRelayCommand LicenseChangeCommand { get; }

	public TgLicenseViewModel(ITgSettingsService settingsService, INavigationService navigationService, ITgLicenseService licenseService, ILogger<TgLicenseViewModel> logger) 
		: base(settingsService, navigationService, licenseService, logger, nameof(TgLicenseViewModel))
	{
		AppVersionShort = $"v{TgCommonUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
		AppVersionFull = $"{TgResourceExtensions.GetAppVersion()}: {AppVersionShort}";
		AppVersionTitle =
			$"{TgConstants.OpenTgResearcherDesktop} " +
			$"v{TgCommonUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
		// Commands
		LicenseShowInfoCommand = new AsyncRelayCommand(LicenseShowInfoAsync);
		LicenseClearCommand = new AsyncRelayCommand(LicenseClearAsync);
		LicenseCheckCommand = new AsyncRelayCommand(LicenseCheckAsync);
		LicenseChangeCommand = new AsyncRelayCommand(LicenseChangeAsync);
	}

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
	{
		await ReloadUiAsync();

        await LicenseService.LicenseActivateAsync();
		await LicenseShowInfoAsync();
	});

	private async Task LicenseShowInfoAsync() 
	{
		try
		{
			IsConfirmed = LicenseService.CurrentLicense.IsConfirmed.ToString();
			LicenseKey = LicenseService.CurrentLicense.GetLicenseKeyString();
			LicenseDescription = LicenseService.CurrentLicense.Description;
			UserId = LicenseService.CurrentLicense.GetUserIdString();
			Expiration = LicenseService.CurrentLicense.GetValidToString();
		}
		catch (Exception ex)
		{
			TgLogUtils.LogFatal(ex);
		}
		await Task.CompletedTask;
	}

	private async Task LicenseClearAsync() => await ContentDialogAsync(LicenseClearCoreAsync, TgResourceExtensions.AskLicenseClear());

	private async Task LicenseClearCoreAsync()
	{
		await LicenseService.LicenseClearAsync();
        await LicenseService.LicenseActivateAsync();
		await LicenseShowInfoAsync();
	}

	private async Task LicenseCheckAsync() => await ContentDialogAsync(LicenseCheckCoreAsync, TgResourceExtensions.AskLicenseCheck());

	private async Task LicenseCheckCoreAsync()
	{
        try
		{
			LicenseLog = TgResourceExtensions.GetActionCheckLicenseMsg() + Environment.NewLine;
            var userId = await LicenseService.GetUserIdAsync();
            if (userId == 0)
            {
                await LicenseShowInfoAsync();
                LicenseLog += TgResourceExtensions.GetMenuLicenseUserNotLoggedIn();
                return;
            }

            var apiUrls = new[] { LicenseService.MenuWebSiteGlobalUrl, LicenseService.MenuWebSiteRussianUrl };
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
					var licenseDto = JsonSerializer.Deserialize<TgLicenseDto>(jsonResponse, TgJsonSerializerUtils.GetJsonOptions());
					if (licenseDto?.IsConfirmed != true)
					{
						LicenseLog += $"{TgResourceExtensions.GetMenuLicenseIsNotCofirmed()}: {response.StatusCode}" + Environment.NewLine;
						continue;
					}

					// Updating an existing license or creating a new license
					await LicenseService.LicenseUpdateAsync(licenseDto);

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

	private async Task LicenseChangeAsync()
	{
		try
		{
			Process.Start(new ProcessStartInfo(LicenseService.MenuWebSiteGlobalLicenseBuyUrl) { UseShellExecute = true });
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
