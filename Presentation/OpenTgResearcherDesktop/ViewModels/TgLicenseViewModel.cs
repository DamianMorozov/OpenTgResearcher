namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgLicenseViewModel : TgPageViewModelBase
{
	#region Fields, properties, constructor

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
	public IRelayCommand LicenseRequestCommunityCommand { get; }
	public IRelayCommand LicenseBuyCommand { get; }

	public TgLicenseViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgLicenseViewModel> logger) 
		: base(settingsService, navigationService, logger, nameof(TgLicenseViewModel))
	{
		AppVersionShort = $"v{TgDataUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
		AppVersionFull = $"{TgResourceExtensions.GetAppVersion()}: {AppVersionShort}";
		AppVersionTitle =
			$"{TgConstants.OpenTgResearcherDesktop} " +
			$"v{TgDataUtils.GetTrimVersion(Assembly.GetExecutingAssembly().GetName().Version)}";
		// Commands
		LicenseShowInfoCommand = new AsyncRelayCommand(LicenseShowInfoAsync);
		LicenseClearCommand = new AsyncRelayCommand(LicenseClearAsync);
		LicenseCheckCommand = new AsyncRelayCommand(LicenseCheckAsync);
        LicenseRequestCommunityCommand = new AsyncRelayCommand(LicenseRequestCommunityAsync);
		LicenseBuyCommand = new AsyncRelayCommand(LicenseBuyAsync);
	}

	#endregion

	#region Methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
	{
		await ReloadUiAsync();

        await App.BusinessLogicManager.LicenseService.LicenseActivateAsync();
		await LicenseShowInfoAsync();
	});

	private async Task LicenseShowInfoAsync()
	{
		try
		{
			IsConfirmed = App.BusinessLogicManager.LicenseService.CurrentLicense.IsConfirmed.ToString();
			LicenseKey = App.BusinessLogicManager.LicenseService.CurrentLicense.GetLicenseKeyString;
			LicenseDescription = App.BusinessLogicManager.LicenseService.CurrentLicense.Description;
			UserId = App.BusinessLogicManager.LicenseService.CurrentLicense.GetUserIdString;
			Expiration = App.BusinessLogicManager.LicenseService.CurrentLicense.GetValidToString;
		}
		catch (Exception ex)
		{
			LogError(ex);
		}
		await Task.CompletedTask;
	}

	private async Task LicenseClearAsync() => await ContentDialogAsync(LicenseClearCoreAsync, TgResourceExtensions.AskLicenseClear(), TgEnumLoadDesktopType.Online);

	private async Task LicenseClearCoreAsync()
	{
		await App.BusinessLogicManager.LicenseService.LicenseClearAsync();
        await App.BusinessLogicManager.LicenseService.LicenseActivateAsync();
		await LicenseShowInfoAsync();
	}

    /// <summary> Check current license </summary>
	private async Task LicenseCheckAsync() => await ContentDialogAsync(LicenseCheckCoreAsync, TgResourceExtensions.AskLicenseCheck(), TgEnumLoadDesktopType.Online);

	private async Task LicenseCheckCoreAsync()
	{
        var userId = await App.BusinessLogicManager.ConnectClient.GetUserIdAsync();

        try
        {
			LicenseLog = TgResourceExtensions.GetActionCheckLicenseMsg() + Environment.NewLine;

            var apiURLs = new[] { App.BusinessLogicManager.LicenseService.MenuWebSiteGlobalUrl };
            using var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

            foreach (var apiUrl in apiURLs)
			{
                if (await TryCheckLicenseFromServerAsync(httpClient, apiUrl, $"{apiUrl}License/{TgGlobalTools.RouteGet}?userId={userId}", userId, isPost: false))
                    break;
			}
		}
        catch (Exception ex)
        {
            LogError(ex);
        }
        finally
		{
			await App.BusinessLogicManager.LicenseService.LicenseActivateAsync();
			await LicenseShowInfoAsync();
		}
	}

    private async Task<bool> TryCheckLicenseFromServerAsync(HttpClient httpClient, string apiUrl, string url, long userId, bool isPost)
    {
        try
        {
            var response = isPost ? await httpClient.PostAsync(url, null) : await httpClient.GetAsync(url);
            LicenseLog += $"{TgResourceExtensions.GetMenuLicenseCheckServer()}: {apiUrl}" + Environment.NewLine;
            
            if (!response.IsSuccessStatusCode)
            {
                LicenseLog += $"{TgResourceExtensions.GetMenuLicense()}: {response.StatusCode}" + Environment.NewLine;
                return false;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var licenseDto = JsonSerializer.Deserialize<TgLicenseDto>(jsonResponse, TgJsonSerializerUtils.GetJsonOptions());
            if (licenseDto?.IsConfirmed != true)
            {
                var licenseEfDto = JsonSerializer.Deserialize<TgEfLicenseDto>(jsonResponse, TgJsonSerializerUtils.GetJsonOptions());
                if (licenseEfDto?.IsConfirmed != true)
                {
                    LicenseLog += $"{TgResourceExtensions.GetMenuLicenseIsNotConfirmed()}: {response.StatusCode}" + Environment.NewLine;
                    return false;
                }
                else
                {
                    licenseDto = new TgLicenseDto(licenseEfDto.LicenseKey, licenseEfDto.LicenseType, licenseEfDto.UserId, licenseEfDto.ValidTo, licenseEfDto.IsConfirmed);
                }
            }

            // Updating an existing license or creating a new license
            await App.BusinessLogicManager.LicenseService.LicenseUpdateAsync(licenseDto);

            LicenseLog += TgResourceExtensions.GetMenuLicenseUpdatedSuccessfully() + Environment.NewLine;
            return true;
        }
        catch (Exception ex)
        {
            LogError(ex);
            return false;
        }
    }

    /// <summary> Request community license </summary>
	private async Task LicenseRequestCommunityAsync() => 
        await ContentDialogAsync(LicenseRequestCommunityCoreAsync, TgResourceExtensions.AskLicenseRequestCommunity(), TgEnumLoadDesktopType.Online);

    private async Task LicenseRequestCommunityCoreAsync()
    {
        var userId = await App.BusinessLogicManager.ConnectClient.GetUserIdAsync();

        try
        {
            LicenseLog = TgResourceExtensions.GetActionLicenseRequestCommunityMsg() + Environment.NewLine;

            var apiURLs = new[] { App.BusinessLogicManager.LicenseService.MenuWebSiteGlobalUrl };
            using var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

            foreach (var apiUrl in apiURLs)
            {
                if (await TryCheckLicenseFromServerAsync(httpClient, apiUrl, $"{apiUrl}License/{TgGlobalTools.RouteCreateCommunity}?userId={userId}", userId, isPost: true))
                    break;
            }
        }
        catch (Exception ex)
        {
            LogError(ex);
        }
        finally
        {
            await App.BusinessLogicManager.LicenseService.LicenseActivateAsync();
            await LicenseShowInfoAsync();
        }
    }

    /// <summary> Buy license </summary>
    private async Task LicenseBuyAsync()
	{
		try
		{
			Process.Start(new ProcessStartInfo(App.BusinessLogicManager.LicenseService.MenuWebSiteGlobalLicenseBuyUrl) { UseShellExecute = true });
		}
		catch (Exception ex)
		{
			LogError(ex);
		}
		finally
		{
			await Task.CompletedTask;
		}

	}

	#endregion
}
