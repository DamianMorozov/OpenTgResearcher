// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	private static TgEnumMenuStorageSetup SetMenuStorageSetup()
	{
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(
            TgLocale.MenuReturn,
            TgLocale.MenuStorageDbClear,
            TgLocale.MenuStorageDbBackup,
            TgLocale.MenuStorageTablesShrink,
            TgLocale.MenuStorageResetAutoDownload
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
		if (prompt.Equals(TgLocale.MenuStorageDbClear))
			return TgEnumMenuStorageSetup.DbClear;
		if (prompt.Equals(TgLocale.MenuStorageDbBackup))
			return TgEnumMenuStorageSetup.DbBackup;
		if (prompt.Equals(TgLocale.MenuStorageTablesShrink))
			return TgEnumMenuStorageSetup.DbCompact;

        return TgEnumMenuStorageSetup.Return;
	}

	public async Task SetupStorageSetupAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuStorageSetup menu;
		do
		{
			await ShowTableStorageSetupAsync(tgDownloadSettings);
			menu = SetMenuStorageSetup();
            switch (menu)
            {
                case TgEnumMenuStorageSetup.DbClear:
                    StorageDbClear();
                    break;
                case TgEnumMenuStorageSetup.DbBackup:
                    StorageDbBackup();
                    break;
                case TgEnumMenuStorageSetup.DbCompact:
                    await StorageDbCompactAsync();
                    break;
                case TgEnumMenuStorageSetup.Return:
                    break;
            }
        } while (menu is not TgEnumMenuStorageSetup.Return);
    }

    /// <summary> Clear storage database </summary>
    private void StorageDbClear()
    {
        AnsiConsole.WriteLine($"  {TgLocale.MenuStoragePerformSteps}");
        AnsiConsole.WriteLine($"  - {TgLocale.MenuStorageExitProgram}");
        AnsiConsole.WriteLine($"  - {TgLocale.MenuStorageDeleteExistsInfo(TgAppSettings.AppXml.XmlEfStorage)}");
        TgLog.TypeAnyKeyForReturn();
    }

    /// <summary> Backup storage database </summary>
    private void StorageDbBackup()
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuStorageDbBackup)) return;

		TgLog.WriteLine($"  {TgLocale.MenuStorageBackupDirectory}: {Path.GetDirectoryName(TgAppSettings.AppXml.XmlEfStorage)}");
		var backupResult = BusinessLogicManager.BackupDb();
		TgLog.WriteLine($"  {TgLocale.MenuStorageBackupFile}: {backupResult.FileName}");
		TgLog.WriteLine(backupResult.IsSuccess ? $"  {TgLocale.MenuStorageBackupSuccess}" : $"  {TgLocale.MenuStorageBackupFailed}");
        TgLog.TypeAnyKeyForReturn();
    }

    /// <summary> Compact storage database </summary>
	private async Task StorageDbCompactAsync()
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuStorageTablesShrink)) return;

        await BusinessLogicManager.ShrinkDbAsync();
		TgLog.WriteLine($"  {TgLocale.MenuStorageTablesShrinkFinished}");
        TgLog.TypeAnyKeyForReturn();
	}

    #endregion
}