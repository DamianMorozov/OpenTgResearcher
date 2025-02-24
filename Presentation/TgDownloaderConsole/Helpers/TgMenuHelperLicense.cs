// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace TgDownloaderConsole.Helpers;

[DebuggerDisplay("{ToDebugString()}")]
internal sealed partial class TgMenuHelper
{
	#region Public and internal methods

	private TgEnumMenuLicense SetMenuLicense()
	{
		var prompt = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
				.Title($"  {TgLocale.MenuSwitchNumber}")
				.PageSize(Console.WindowHeight - 17)
				.MoreChoicesText(TgLocale.MoveUpDown)
				.AddChoices(TgLocale.MenuMainReturn,
					TgLocale.MenuLicenseCheck,
					TgLocale.MenuLicenseChange
				));
		if (prompt.Equals(TgLocale.MenuLicenseCheck))
			return TgEnumMenuLicense.LicenseCheck;
		if (prompt.Equals(TgLocale.MenuLicenseChange))
			return TgEnumMenuLicense.LicenseChange;
		return TgEnumMenuLicense.Return;
	}

	public async Task SetupLicenseAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuLicense menu;
		do
		{
			await ShowTableLicenseSettingsAsync(tgDownloadSettings);
			menu = SetMenuLicense();
			switch (menu)
			{
				case TgEnumMenuLicense.LicenseCheck:
					await LicenseCheckAsync();
					break;
				case TgEnumMenuLicense.LicenseChange:
					await LicenseChangeAsync();
					break;
			}
		} while (menu is not TgEnumMenuLicense.Return);
	}

	/// <summary> Check license </summary>
	private async Task LicenseCheckAsync()
	{
		AnsiConsole.WriteLine(TgLocale.InDevelopment);
		TgLog.WriteLine(TgLocale.TypeAnyKeyForReturn);
		Console.ReadKey();
		await Task.CompletedTask;
	}

	/// <summary> Change license </summary>
	private async Task LicenseChangeAsync()
	{
		AnsiConsole.WriteLine(TgLocale.InDevelopment);
		TgLog.WriteLine(TgLocale.TypeAnyKeyForReturn);
		Console.ReadKey();
		await Task.CompletedTask;
	}

	#endregion
}