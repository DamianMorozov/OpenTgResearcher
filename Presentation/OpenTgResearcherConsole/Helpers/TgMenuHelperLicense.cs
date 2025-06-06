// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal sealed partial class TgMenuHelper
{
	#region Public and private methods

	private static TgEnumMenuLicense SetMenuLicense()
	{
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(TgLocale.MenuReturn,
            TgLocale.MenuLicenseClear,
            TgLocale.MenuLicenseCheck,
            TgLocale.MenuLicenseBuy
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
		if (prompt.Equals(TgLocale.MenuLicenseClear))
			return TgEnumMenuLicense.LicenseClear;
		if (prompt.Equals(TgLocale.MenuLicenseCheck))
			return TgEnumMenuLicense.LicenseCheck;
		if (prompt.Equals(TgLocale.MenuLicenseBuy))
			return TgEnumMenuLicense.LicenseBuy;
		return TgEnumMenuLicense.Return;
	}

	public async Task SetupLicenseAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuLicense menu;
		do
		{
			await LicenseShowInfoAsync(tgDownloadSettings, [], isWait: false);
			menu = SetMenuLicense();
			switch (menu)
			{
				case TgEnumMenuLicense.LicenseClear:
					await LicenseClearAsync(tgDownloadSettings, isSilent: false);
					break;
				case TgEnumMenuLicense.LicenseCheck:
					await LicenseCheckAsync(tgDownloadSettings, isSilent: false);
					break;
				case TgEnumMenuLicense.LicenseBuy:
					await LicenseBuyAsync();
					break;
				case TgEnumMenuLicense.Return:
					break;
			}
		} while (menu is not TgEnumMenuLicense.Return);
	}

	/// <summary> Clear license </summary>
	internal async Task LicenseClearAsync(TgDownloadSettingsViewModel tgDownloadSettings, bool isSilent)
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuLicenseClear))
			return;

		await BusinessLogicManager.LicenseService.LicenseClearAsync();
		await LicenseShowInfoAsync(tgDownloadSettings, [], isWait: false);
	}

	/// <summary> Check license </summary>
	internal async Task LicenseCheckAsync(TgDownloadSettingsViewModel tgDownloadSettings, bool isSilent)
	{
        if (!isSilent && AskQuestionYesNoReturnNegative(TgLocale.MenuLicenseCheckWithUserId))
            return;

        try
        {
            var apiUrls = new[] { BusinessLogicManager.LicenseService.MenuWebSiteGlobalUrl, BusinessLogicManager.LicenseService.MenuWebSiteRussianUrl };
			using var httpClient = new HttpClient();
			httpClient.Timeout = TimeSpan.FromSeconds(10);
            var userId = await BusinessLogicManager.ConnectClient.GetUserIdAsync();

            if (userId == 0 && !isSilent)
            {
                AnsiConsole.MarkupLine($"  {TgLocale.TgClientUserId} is {userId}, try to connect client first!");
                await AskClientConnectAsync(tgDownloadSettings, isSilent);
                userId = await BusinessLogicManager.ConnectClient.GetUserIdAsync();
            }

            if (userId > 0)
            {
                foreach (var apiUrl in apiUrls)
                {
                    try
                    {
                        var url = $"{apiUrl}License/Get?userId={userId}";
                        var response = await httpClient.GetAsync(url);
                        var checkUrl = $"{TgLocale.MenuLicenseCheckServer}: {apiUrl}";
                        if (!response.IsSuccessStatusCode)
                        {
                            if (!isSilent)
                                await LicenseShowInfoAsync(tgDownloadSettings, [checkUrl, $"{TgLocale.MenuLicenseResponseStatusCode}: {response.StatusCode}"]);
                            continue;
                        }

                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var licenseDto = JsonSerializer.Deserialize<TgLicenseDto>(jsonResponse, TgJsonSerializerUtils.GetJsonOptions());
                        if (licenseDto?.IsConfirmed != true)
                        {
                            if (!isSilent)
                                await LicenseShowInfoAsync(tgDownloadSettings, [checkUrl, $"{TgLocale.MenuLicenseIsNotCofirmed}: {response.StatusCode}"]);
                            continue;
                        }

                        // Updating an existing license or creating a new license
                        await BusinessLogicManager.LicenseService.LicenseUpdateAsync(licenseDto);

                        if (!isSilent)
                            await LicenseShowInfoAsync(tgDownloadSettings, [checkUrl, TgLocale.MenuLicenseUpdatedSuccessfully]);
                        else
                            await BusinessLogicManager.LicenseService.LicenseActivateAsync();
                        return;
                    }
                    catch (Exception ex)
                    {
                        TgDebugUtils.WriteExceptionToDebug(ex);
                        if (!isSilent)
                        {
                            AnsiConsole.WriteLine($"  {TgLocale.MenuLicenseRequestError}");
                            AnsiConsole.WriteLine($"  {ex.Message}");
                        }
                    }
                }
                if (!isSilent)
                    await LicenseShowInfoAsync(tgDownloadSettings, []);
            }
        }
		catch (Exception ex)
		{
            TgDebugUtils.WriteExceptionToDebug(ex);
			if (!isSilent)
			{
				AnsiConsole.WriteLine($"  {TgLocale.MenuLicenseRequestError}");
				AnsiConsole.WriteLine($"  {ex.Message}");
			}
		}
		finally
		{
			await Task.CompletedTask;
		}
	}

	/// <summary> Show license </summary>
	private async Task LicenseShowInfoAsync(TgDownloadSettingsViewModel tgDownloadSettings, List<string> messages, bool isWait = true)
	{
		try
		{
            await BusinessLogicManager.LicenseService.LicenseActivateAsync();

			await ShowTableLicenseFullInfoAsync(tgDownloadSettings);
			if (messages.Any())
				foreach (var message in messages)
					AnsiConsole.WriteLine(message);

			if (isWait)
			{
                TgLog.TypeAnyKeyForReturn();
            }
		}
		finally
		{
			await Task.CompletedTask;
		}
	}

	/// <summary> Buy license </summary>
	private async Task LicenseBuyAsync() => await WebSiteOpenAsync(BusinessLogicManager.LicenseService.MenuWebSiteGlobalLicenseBuyUrl);

	/// <summary> Open web-site </summary>
	private async Task WebSiteOpenAsync(string url)
	{
		try
		{
			Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
		}
		catch (Exception ex)
		{
            TgDebugUtils.WriteExceptionToDebug(ex);
			AnsiConsole.WriteLine($"  Opening error URL: {ex.Message}");
		}
		finally
		{
			await Task.CompletedTask;
		}
	}

	#endregion
}