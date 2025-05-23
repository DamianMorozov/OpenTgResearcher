// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	private static TgEnumMenuStorage SetMenuStorage()
	{
		var prompt = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
				.Title($"  {TgLocale.MenuSwitchNumber}")
				.PageSize(Console.WindowHeight - 17)
				.MoreChoicesText(TgLocale.MoveUpDown)
				.AddChoices(TgLocale.MenuMainReturn,
					TgLocale.MenuStorageDbClear,
					TgLocale.MenuStorageDbBackup,
					TgLocale.MenuStorageDbCreateNew,
					TgLocale.MenuStorageTablesVersionsView,
					TgLocale.MenuStorageTablesCompact
				));
		if (prompt.Equals(TgLocale.MenuStorageDbClear))
			return TgEnumMenuStorage.DbClear;
		if (prompt.Equals(TgLocale.MenuStorageDbBackup))
			return TgEnumMenuStorage.DbBackup;
		if (prompt.Equals(TgLocale.MenuStorageDbCreateNew))
			return TgEnumMenuStorage.DbCreateNew;
		if (prompt.Equals(TgLocale.MenuStorageTablesVersionsView))
			return TgEnumMenuStorage.TablesVersionsView;
		if (prompt.Equals(TgLocale.MenuStorageTablesCompact))
			return TgEnumMenuStorage.TablesCompact;
		return TgEnumMenuStorage.Return;
	}

	public async Task SetupStorageAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuStorage menu;
		do
		{
			await ShowTableStorageAsync(tgDownloadSettings);
			menu = SetMenuStorage();
			switch (menu)
			{
				case TgEnumMenuStorage.DbClear:
					ClearStorageData();
					break;
				case TgEnumMenuStorage.DbBackup:
					TgStorageBackupDb();
					break;
				case TgEnumMenuStorage.DbCreateNew:
					await TgStorageCreateNewDbAsync();
					break;
				case TgEnumMenuStorage.TablesVersionsView:
					TgStorageTablesVersionsView();
					break;
				case TgEnumMenuStorage.TablesCompact:
					await TgStorageTablesCompactAsync();
					break;
				case TgEnumMenuStorage.Return:
					break;
			}
		} while (menu is not TgEnumMenuStorage.Return);
	}

	private void TgStorageBackupDb()
	{
		if (AskQuestionTrueFalseReturnNegative(TgLocale.MenuStorageDbBackup)) return;

		TgLog.WriteLine($"{TgLocale.MenuStorageBackupDirectory}: {Path.GetDirectoryName(TgAppSettings.AppXml.XmlEfStorage)}");
		var backupResult = BusinessLogicManager.BackupDbAsync();
		TgLog.WriteLine($"{TgLocale.MenuStorageBackupFile}: {backupResult.FileName}");
		TgLog.WriteLine(backupResult.IsSuccess ? TgLocale.MenuStorageBackupSuccess : TgLocale.MenuStorageBackupFailed);
        TgLog.TypeAnyKeyForReturn();
    }

	private async Task TgStorageCreateNewDbAsync()
	{
		if (AskQuestionTrueFalseReturnNegative(TgLocale.MenuStorageDbCreateNew)) return;

		// Create and update storage
		await BusinessLogicManager.CreateAndUpdateDbAsync();
	}

	private void ClearStorageData()
	{
		AnsiConsole.WriteLine(TgLocale.MenuStoragePerformSteps);
		AnsiConsole.WriteLine($"- {TgLocale.MenuStorageExitProgram}");
		AnsiConsole.WriteLine($"- {TgLocale.MenuStorageDeleteExistsInfo(TgAppSettings.AppXml.XmlEfStorage)}");
        TgLog.TypeAnyKeyForReturn();
    }

	private void TgStorageTablesVersionsView()
	{
		BusinessLogicManager.VersionsView();
        TgLog.TypeAnyKeyForReturn();
    }

	private async Task TgStorageTablesCompactAsync()
	{
		if (AskQuestionTrueFalseReturnNegative(TgLocale.MenuStorageTablesCompact)) return;
		// Shrink storage
		await BusinessLogicManager.ShrinkDbAsync();
		TgLog.WriteLine(TgLocale.MenuStorageTablesCompactFinished);
		Console.ReadKey();
	}

	#endregion
}