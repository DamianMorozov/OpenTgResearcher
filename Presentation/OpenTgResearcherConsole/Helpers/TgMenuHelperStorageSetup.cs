// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Methods

	private static TgEnumMenuStorageSetup SetMenuStorageSetup()
	{
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(
            TgLocale.MenuReturn,
            TgLocale.MenuStorageDbBackup,
            TgLocale.MenuStorageDbShrink,
            TgLocale.MenuStorageDbClear
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
		if (prompt.Equals(TgLocale.MenuStorageDbBackup))
			return TgEnumMenuStorageSetup.DbBackup;
		if (prompt.Equals(TgLocale.MenuStorageDbShrink))
			return TgEnumMenuStorageSetup.DbCompact;
		if (prompt.Equals(TgLocale.MenuStorageDbClear))
			return TgEnumMenuStorageSetup.DbClear;

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
                case TgEnumMenuStorageSetup.DbBackup:
                    StorageDbBackup();
                    break;
                case TgEnumMenuStorageSetup.DbCompact:
                    await StorageDbShrinkAsync();
                    break;
                case TgEnumMenuStorageSetup.DbClear:
                    StorageDbClear();
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
		var backupResult = BusinessLogicManager.StorageManager.BackupDb();
		TgLog.WriteLine($"  {TgLocale.MenuStorageBackupFile}: {backupResult.FileName}");
		TgLog.WriteLine(backupResult.IsSuccess ? $"  {TgLocale.MenuStorageBackupSuccess}" : $"  {TgLocale.MenuStorageBackupFailed}");
        TgLog.TypeAnyKeyForReturn();
    }

    /// <summary> Shrink storage database </summary>
	private async Task StorageDbShrinkAsync()
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuStorageDbShrink)) return;

        await BusinessLogicManager.StorageManager.ShrinkDbAsync();
		TgLog.WriteLine($"  {TgLocale.MenuStorageTablesShrinkFinished}");
        TgLog.TypeAnyKeyForReturn();
	}

    #endregion
}