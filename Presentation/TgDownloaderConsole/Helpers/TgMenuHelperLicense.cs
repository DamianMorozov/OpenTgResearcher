// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace TgDownloaderConsole.Helpers;

[DebuggerDisplay("{ToDebugString()}")]
internal sealed partial class TgMenuHelper
{
	#region Public and internal methods

	private static TgEnumMenuLicense SetMenuLicense()
	{
		var prompt = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
				.Title($"  {TgLocale.MenuSwitchNumber}")
				.PageSize(Console.WindowHeight - 17)
				.MoreChoicesText(TgLocale.MoveUpDown)
				.AddChoices(TgLocale.MenuMainReturn,
					TgLocale.MenuLicenseInfo,
					TgLocale.MenuLicenseCheck,
					TgLocale.MenuLicenseChange,
					TgLocale.MenuWebSiteOpen
				));
		if (prompt.Equals(TgLocale.MenuLicenseInfo))
			return TgEnumMenuLicense.LicenseInfo;
		if (prompt.Equals(TgLocale.MenuLicenseCheck))
			return TgEnumMenuLicense.LicenseCheck;
		if (prompt.Equals(TgLocale.MenuLicenseChange))
			return TgEnumMenuLicense.LicenseChange;
		if (prompt.Equals(TgLocale.MenuWebSiteOpen))
			return TgEnumMenuLicense.LicenseWebSiteOpen;
		return TgEnumMenuLicense.Return;
	}

	public async Task SetupLicenseAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuLicense menu;
		do
		{
			await ShowTableLicenseShortInfoAsync(tgDownloadSettings);
			menu = SetMenuLicense();
			switch (menu)
			{
				case TgEnumMenuLicense.LicenseInfo:
					await LicenseInfoAsync(tgDownloadSettings);
					break;
				case TgEnumMenuLicense.LicenseCheck:
					await LicenseCheckAsync(tgDownloadSettings);
					break;
				case TgEnumMenuLicense.LicenseChange:
					await LicenseChangeAsync(tgDownloadSettings);
					break;
				case TgEnumMenuLicense.LicenseWebSiteOpen:
					await WebSiteOpenAsync(TgLocale.MenuWebSiteGlobalUrl);
					break;
				case TgEnumMenuLicense.Return:
					break;
			}
		} while (menu is not TgEnumMenuLicense.Return);
	}

	/// <summary> View info </summary>
	private async Task LicenseInfoAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await LicenseShowInfo(tgDownloadSettings);
	}

	/// <summary> Check license </summary>
	private async Task LicenseCheckAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		try
		{
			const int timeoutSeconds = 10;
			var licenseDtos = await LicenseRepository.GetListDtosAsync();
			var currentLicenseDto = licenseDtos.FirstOrDefault(x => x.IsConfirmed);

			var userId = await GetUserId();
			if (userId == 0)
			{
				await LicenseShowInfo(tgDownloadSettings, TgLocale.MenuLicenseUserNotLoggedIn);
				return;
			}

			var apiUrls = new[] { TgLocale.MenuWebSiteGlobalUrl, TgLocale.MenuWebSiteRussianUrl };

			using var httpClient = new HttpClient();
			httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

			foreach (var apiUrl in apiUrls)
			{
				try
				{
					var url = $"{apiUrl}License/Get?userId={userId}";
					var response = await httpClient.GetAsync(url);
					AnsiConsole.WriteLine($"{TgLocale.MenuLicenseCheckServer}: {apiUrl}");
					AnsiConsole.WriteLine();
					if (!response.IsSuccessStatusCode)
					{
						await LicenseShowInfo(tgDownloadSettings, $"{TgLocale.MenuLicenseResponseStatusCode}: {response.StatusCode}");
						continue;
					}

					var jsonResponse = await response.Content.ReadAsStringAsync();
					var licenseData = JsonSerializer.Deserialize<TgLicenseApiResponse>(jsonResponse, GetJsonOptions());
					if (licenseData?.IsConfirmed != true)
					{
						await LicenseShowInfo(tgDownloadSettings, $"{TgLocale.MenuLicenseIsNotCofirmed}: {response.StatusCode}");
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

					await LicenseShowInfo(tgDownloadSettings, TgLocale.MenuLicenseUpdatedSuccessfully);
					return;
				}
#if DEBUG
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
					Debug.WriteLine(ex.StackTrace);
#else
				catch (Exception)
				{
#endif
					AnsiConsole.WriteLine(TgLocale.MenuLicenseRequestError);
					AnsiConsole.WriteLine(ex.Message);
				}
			}

			await LicenseShowInfo(tgDownloadSettings);
		}
#if DEBUG
		catch (Exception ex)
		{
			Debug.WriteLine(ex);
			Debug.WriteLine(ex.StackTrace);
			AnsiConsole.WriteLine(ex.Message);
#else
		catch (Exception)
		{
#endif
			AnsiConsole.WriteLine(TgLocale.MenuLicenseRequestError);
			AnsiConsole.WriteLine(ex.Message);
		}
		finally
		{
			await Task.CompletedTask;
		}
	}

	private static JsonSerializerOptions GetJsonOptions() => new()
	{
		PropertyNameCaseInsensitive = true,
		Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
	};

	private static async Task<long> GetUserId()
	{
		if (TgGlobalTools.ConnectClient.Me is null)
			await TgGlobalTools.ConnectClient.LoginUserAsync(isProxyUpdate: false);
		var userId = TgGlobalTools.ConnectClient.Me?.ID ?? 0;
		if (userId == 0)
		{
			AnsiConsole.WriteLine(TgLocale.MenuLicenseUserNotLoggedIn);
		}
		return userId;
	}

	private async Task LicenseShowInfo(TgDownloadSettingsViewModel tgDownloadSettings, string message = "")
	{
		try
		{
			var licenseDtos = await LicenseRepository.GetListDtosAsync();
			var currentLicenseDto = licenseDtos.FirstOrDefault(x => x.IsConfirmed);
			if (currentLicenseDto is not null)
				TgLicense.ActivateLicense(currentLicenseDto.LicenseKey, currentLicenseDto.LicenseType, currentLicenseDto.UserId, currentLicenseDto.ValidTo);

			await ShowTableLicenseFullInfoAsync(tgDownloadSettings);
			if (!string.IsNullOrEmpty(message))
				AnsiConsole.WriteLine(message);
			TgLog.WriteLine(TgLocale.TypeAnyKeyForReturn);
			Console.ReadKey();
		}
		finally
		{
			await Task.CompletedTask;
		}
	}

	/// <summary> Change license </summary>
	private async Task LicenseChangeAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		try
		{
			AnsiConsole.WriteLine(TgLocale.InDevelopment);
		}
		finally
		{
			await ShowTableLicenseFullInfoAsync(tgDownloadSettings);
			TgLog.WriteLine(TgLocale.TypeAnyKeyForReturn);
			Console.ReadKey();
			await Task.CompletedTask;
		}
	}

	/// <summary> Open a web-site </summary>
	private async Task WebSiteOpenAsync(string url)
	{
		try
		{
			Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
		}
#if DEBUG
		catch (Exception ex)
		{
			Debug.WriteLine(ex, TgConstants.LogTypeConsole);
			Debug.WriteLine(ex.StackTrace);
			Console.WriteLine($"Opening error URL: {ex.Message}");
#else
		catch (Exception)
		{
#endif
		}
		await Task.CompletedTask;
	}

	#endregion
}