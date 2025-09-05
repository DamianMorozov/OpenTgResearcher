// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal sealed partial class TgMenuHelper
{
    #region Methods

    /// <summary> Displays license menu and returns selected action </summary>
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

    /// <summary> Main loop for license setup menu </summary>
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
					await LicenseCheckOnlineAsync(tgDownloadSettings, isSilent: false);
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

	/// <summary> Checks license online and updates local data </summary>
	internal async Task LicenseCheckOnlineAsync(TgDownloadSettingsViewModel tgDownloadSettings, bool isSilent)
	{
        if (!isSilent && AskQuestionYesNoReturnNegative(TgLocale.MenuLicenseCheckWithUserId)) return;

        var userId = await BusinessLogicManager.ConnectClient.GetUserIdAsync();
        if (userId == 0 && !isSilent)
        {
            AnsiConsole.MarkupLine($"  {TgLocale.TgClientUserId} is {userId}, try to connect client first!");
            await ClientConnectAsync(tgDownloadSettings, isSilent);
            userId = await BusinessLogicManager.ConnectClient.GetUserIdAsync();
        }

        try
        {
            var apiURLs = new[] { BusinessLogicManager.LicenseService.MenuWebSiteGlobalUrl };
            using var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

            if (userId > 0)
            {
                foreach (var apiUrl in apiURLs)
                {
                    if (await TryCheckLicenseFromServerAsync(httpClient, apiUrl, userId, tgDownloadSettings, isSilent))
                        break;
                }

                if (!isSilent)
                    await LicenseShowInfoAsync(tgDownloadSettings, []);
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            if (!isSilent)
            {
                AnsiConsole.WriteLine($"  {TgLocale.MenuLicenseRequestError}");
                AnsiConsole.WriteLine($"  {ex.Message}");
            }
        }
    }

    private async Task<bool> TryCheckLicenseFromServerAsync(HttpClient httpClient, string apiUrl, long userId, TgDownloadSettingsViewModel tgDownloadSettings, bool isSilent)
    {
        try
        {
            var url = $"{apiUrl}License/Get?userId={userId}";
            var response = await httpClient.GetAsync(url);
            var checkUrl = $"  {TgLocale.MenuLicenseCheckServer}: {apiUrl}";

            if (!response.IsSuccessStatusCode)
            {
                if (!isSilent)
                    await LicenseShowInfoAsync(tgDownloadSettings, [checkUrl, $"  {TgLocale.MenuLicenseResponseStatusCode}: {response.StatusCode}"]);
                return false;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var licenseDto = JsonSerializer.Deserialize<TgLicenseDto>(jsonResponse, TgJsonSerializerUtils.GetJsonOptions());
            if (licenseDto?.IsConfirmed != true)
            {
                if (!isSilent)
                    await LicenseShowInfoAsync(tgDownloadSettings, [checkUrl, $"  {TgLocale.MenuLicenseIsNotConfirmed}: {response.StatusCode}"]);
                return false;
            }

            // Updating an existing license or creating a new license
            await BusinessLogicManager.LicenseService.LicenseUpdateAsync(licenseDto);

            if (!isSilent)
                await LicenseShowInfoAsync(tgDownloadSettings, [checkUrl, $"  {TgLocale.MenuLicenseUpdatedSuccessfully}"]);
            else
                await BusinessLogicManager.LicenseService.LicenseActivateAsync();

            return true;
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            if (!isSilent)
            {
                AnsiConsole.WriteLine($"  {TgLocale.MenuLicenseRequestError}");
                AnsiConsole.WriteLine($"  {ex.Message}");
            }
            return false;
        }
    }

    /// <summary> Show license </summary>
    private async Task LicenseShowInfoAsync(TgDownloadSettingsViewModel tgDownloadSettings, List<string> messages, bool isWait = true)
	{
		try
		{
            await BusinessLogicManager.LicenseService.LicenseActivateAsync();
			await ShowTableLicenseFullInfoAsync(tgDownloadSettings);

			if (messages.Count != 0)
				foreach (var message in messages)
					AnsiConsole.WriteLine(message);

			if (isWait)
                TgLog.TypeAnyKeyForReturn();
		}
		finally
		{
			await Task.CompletedTask;
		}
	}

	/// <summary> Buy license </summary>
	private async Task LicenseBuyAsync() => await WebSiteOpenAsync(BusinessLogicManager.LicenseService.MenuWebSiteGlobalLicenseBuyUrl);

	#endregion
}
