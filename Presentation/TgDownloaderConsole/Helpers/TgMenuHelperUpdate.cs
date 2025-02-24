// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace TgDownloaderConsole.Helpers;

[DebuggerDisplay("{ToDebugString()}")]
internal sealed partial class TgMenuHelper
{
	#region Public and internal methods

	/// <summary> Velopack installer update </summary>
	public async Task VelopackUpdateAsync(bool isWait)
	{
		Console.OutputEncoding = Encoding.UTF8;
		Console.Title = TgConstants.AppTitleConsoleShort;
		TgLog.SetMarkupLine(AnsiConsole.WriteLine);
		TgLog.SetMarkupLineStamp(AnsiConsole.MarkupLine);
		TgLog.WriteLine($"Update started");
		TgAppSettingsHelper.Instance.SetVersion(Assembly.GetExecutingAssembly());
		TgLog.WriteLine($"{TgConstants.AppTitleConsole} {TgAppSettingsHelper.Instance.AppVersion}");

		VelopackApp.Build()
#if WINDOWS
		.WithBeforeUninstallFastCallback((v) => {
			// delete / clean up some files before uninstallation
			tgLog.WriteLine($"Uninstalling the {TgConstants.AppTitleConsole}!");
		})
#endif
			.WithFirstRun((v) => {
				TgLog.WriteLine($"Thanks for installing the {TgConstants.AppTitleConsole}!");
			})
			.Run();
		TgLog.WriteLine($"Checking updates on the link github.com");
		var mgr = new UpdateManager(new GithubSource(TgConstants.LinkGitHub, string.Empty, prerelease: false));
		// Check for new version
		try
		{
			var newVersion = await mgr.CheckForUpdatesAsync();
			if (newVersion is null)
			{
				TgLog.WriteLine("You are using the latest version of the software");
				return;
			}
			// Download new version
			TgLog.WriteLine("Download new version");
			await mgr.DownloadUpdatesAsync(newVersion);
			// Install new version and restart app
			var prompt = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("Install new version and restart app?")
					.PageSize(Console.WindowHeight - 5)
					.MoreChoicesText(TgLocale.MoveUpDown)
					.AddChoices(TgLocale.MenuNo, TgLocale.MenuYes));
			var isInstall = prompt.Equals(TgLocale.MenuYes);
			if (isInstall)
				mgr.ApplyUpdatesAndRestart(newVersion);
		}
		// Cannot perform this operation in an application which is not installed
		catch (Exception ex)
		{
			TgLog.WriteLine(ex.Message);
		}

		if (isWait)
		{
			TgLog.WriteLine(TgLocale.TypeAnyKeyForReturn);
			Console.ReadKey();
		}
	}

	#endregion
}