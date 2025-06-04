// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal sealed partial class TgMenuHelper
{
	#region Public and internal methods

	private static TgEnumMenuUpdate SetMenuUpdate()
	{
		var prompt = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
				.Title($"  {TgLocale.MenuSwitchNumber}")
				.PageSize(Console.WindowHeight - 17)
				.MoreChoicesText(TgLocale.MoveUpDown)
				.AddChoices(TgLocale.MenuReturn,
					TgLocale.MenuUpdateReleaseCheck,
					TgLocale.MenuUpdatePreviewCheck
				));
		if (prompt.Equals(TgLocale.MenuUpdateReleaseCheck))
			return TgEnumMenuUpdate.ReleaseCheck;
		if (prompt.Equals(TgLocale.MenuUpdatePreviewCheck))
			return TgEnumMenuUpdate.PreviewCheck;
		return TgEnumMenuUpdate.Return;
	}

	public async Task SetupUpdateAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuUpdate menu;
		do
		{
			await ShowTableUpdateAsync(tgDownloadSettings);
			menu = SetMenuUpdate();
			switch (menu)
			{
				case TgEnumMenuUpdate.ReleaseCheck:
					await VelopackUpdateAsync(isWait: true, isPreview: false);
					break;
				case TgEnumMenuUpdate.PreviewCheck:
					await VelopackUpdateAsync(isWait: true, isPreview: true);
					break;
				case TgEnumMenuUpdate.Return:
					break;
			}
		} while (menu is not TgEnumMenuUpdate.Return);
	}

	/// <summary> Velopack installer update </summary>
	public async Task VelopackUpdateAsync(bool isWait, bool isPreview = true)
	{
		TgLog.WriteLine("  Update started");
		await Task.Delay(250);
		TgLog.WriteLine("  Checking updates on the link github.com");
		var mgr = new Velopack.UpdateManager(new GithubSource(TgConstants.LinkGitHub, string.Empty, prerelease: isPreview));
		// Check for new version
		try
		{
			var newVersion = await mgr.CheckForUpdatesAsync();
			if (newVersion is null)
			{
				TgLog.WriteLine("  You are using the latest version of the software");
				return;
			}
			// Download new version
			TgLog.WriteLine("  Download new version");
			await mgr.DownloadUpdatesAsync(newVersion);
			// Install new version and restart app
			var prompt = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("  Install new version and restart app?")
					.PageSize(Console.WindowHeight - 5)
					.MoreChoicesText(TgLocale.MoveUpDown)
					.AddChoices(TgLocale.MenuNo, TgLocale.MenuYes));
			var isYes = prompt.Equals(TgLocale.MenuYes);
			if (isYes)
				mgr.ApplyUpdatesAndRestart(newVersion);
		}
		// Cannot perform this operation in an application which is not installed
		catch (Exception ex)
		{
            CatchException(ex);
		}

		if (isWait)
		{
            TgLog.TypeAnyKeyForReturn();
        }
	}

	#endregion
}