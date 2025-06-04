// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	private static TgEnumMenuApp SetMenuApp()
	{
		var prompt = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
				.Title($"  {TgLocale.MenuSwitchNumber}")
				.PageSize(Console.WindowHeight - 17)
				.MoreChoicesText(TgLocale.MoveUpDown)
				.AddChoices(
					TgLocale.MenuReturn,
					TgLocale.MenuAppClearAppData,
					TgLocale.MenuAppLocateStorage,
					TgLocale.MenuAppLocateSession,
					TgLocale.MenuAppUseProxy
			));
		if (prompt.Equals(TgLocale.MenuAppClearAppData))
			return TgEnumMenuApp.ClearData;
		if (prompt.Equals(TgLocale.MenuAppLocateStorage))
			return TgEnumMenuApp.SetEfStorage;
		if (prompt.Equals(TgLocale.MenuAppLocateSession))
			return TgEnumMenuApp.SetFileSession;
		if (prompt.Equals(TgLocale.MenuAppUseProxy))
			return TgEnumMenuApp.SetUseProxy;
		return TgEnumMenuApp.Return;
	}

	public async Task SetupAppSettingsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuApp menu;
		do
		{
			await ShowTableApplicationAsync(tgDownloadSettings);
			menu = SetMenuApp();
			switch (menu)
			{
				case TgEnumMenuApp.ClearData:
					MenuAppClearData();
					break;
				case TgEnumMenuApp.SetEfStorage:
					MenuAppSetEfStorage();
					break;
				case TgEnumMenuApp.SetFileSession:
					MenuAppSetFileSession();
					break;
				case TgEnumMenuApp.SetUseProxy:
 					await MenuAppSetUseProxyAsync(tgDownloadSettings);
					break;
				case TgEnumMenuApp.Return:
					break;
			}
		} while (menu is not TgEnumMenuApp.Return);
	}

	private void SetFileAppSettings()
	{
		TgAppSettings.StoreXmlSettings();
		TgAppSettings.LoadXmlSettings();
	}

	private void MenuAppClearData()
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuAppClearAppData)) return;

		TgAppSettings.AppXml = new();
		SetFileAppSettings();
	}

	private void MenuAppSetFileSession()
	{
		TgAppSettings.AppXml.SetFileSessionPath(AnsiConsole.Ask<string>(
			TgLog.GetLineStampInfo($"{TgLocale.MenuAppLocateSession}:")));
		SetFileAppSettings();
	}

	private void MenuAppSetEfStorage()
	{
		TgAppSettings.AppXml.SetEfStoragePath(AnsiConsole.Ask<string>(
			TgLog.GetLineStampInfo($"{TgLocale.MenuAppLocateStorage}:")));
		SetFileAppSettings();
	}

	private async Task MenuAppSetUseProxyAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgAppSettings.IsUseProxy = AskQuestionYesNoReturnPositive(TgLocale.MenuAppUseProxy);
		SetFileAppSettings();
        await AskClientConnectAsync(tgDownloadSettings, isSilent: false);
    }

	#endregion
}