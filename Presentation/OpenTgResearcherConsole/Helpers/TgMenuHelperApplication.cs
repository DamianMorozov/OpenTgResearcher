// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	private static TgEnumMenuAppSettings SetMenuApp()
	{
		var prompt = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
				.Title($"  {TgLocale.MenuSwitchNumber}")
				.PageSize(Console.WindowHeight - 17)
				.MoreChoicesText(TgLocale.MoveUpDown)
				.AddChoices(
					TgLocale.MenuMainReturn,
					TgLocale.MenuMainClearAppData,
					TgLocale.MenuLocateStorage,
					TgLocale.MenuLocateSession,
					TgLocale.MenuAppUseProxy
			));
		if (prompt.Equals(TgLocale.MenuMainClearAppData))
			return TgEnumMenuAppSettings.Clear;
		if (prompt.Equals(TgLocale.MenuLocateStorage))
			return TgEnumMenuAppSettings.SetEfStorage;
		if (prompt.Equals(TgLocale.MenuLocateSession))
			return TgEnumMenuAppSettings.SetFileSession;
		if (prompt.Equals(TgLocale.MenuAppUseProxy))
			return TgEnumMenuAppSettings.SetUseProxy;
		return TgEnumMenuAppSettings.Return;
	}

	public async Task SetupAppSettingsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuAppSettings menu;
		do
		{
			await ShowTableApplicationAsync(tgDownloadSettings);
			menu = SetMenuApp();
			switch (menu)
			{
				case TgEnumMenuAppSettings.Clear:
					MenuAppClearData();
					break;
				case TgEnumMenuAppSettings.SetEfStorage:
					MenuAppSetEfStorage();
					break;
				case TgEnumMenuAppSettings.SetFileSession:
					MenuAppSetFileSession();
					break;
				case TgEnumMenuAppSettings.SetUseProxy:
					MenuAppSetUseProxy();
					await AskClientConnectAsync(tgDownloadSettings, isSilent: false);
					break;
				case TgEnumMenuAppSettings.Return:
					break;
			}
		} while (menu is not TgEnumMenuAppSettings.Return);
	}

	private void SetFileAppSettings()
	{
		TgAppSettings.StoreXmlSettings();
		TgAppSettings.LoadXmlSettings();
	}

	private void MenuAppClearData()
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuMainClearAppData)) return;

		TgAppSettings.AppXml = new();
		SetFileAppSettings();
	}

	private void MenuAppSetFileSession()
	{
		TgAppSettings.AppXml.SetFileSessionPath(AnsiConsole.Ask<string>(
			TgLog.GetLineStampInfo($"{TgLocale.MenuLocateSession}:")));
		SetFileAppSettings();
	}

	private void MenuAppSetEfStorage()
	{
		TgAppSettings.AppXml.SetEfStoragePath(AnsiConsole.Ask<string>(
			TgLog.GetLineStampInfo($"{TgLocale.MenuLocateStorage}:")));
		SetFileAppSettings();
	}

	private void MenuAppSetUseProxy()
	{
		TgAppSettings.IsUseProxy = AskQuestionYesNoReturnNegative(TgLocale.MenuAppUseProxy);
		SetFileAppSettings();
	}

	#endregion
}