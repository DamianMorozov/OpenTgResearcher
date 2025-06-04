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
					TgLocale.MenuStorageTablesCompact,
                    TgLocale.MenuStorageClearChats,
                    TgLocale.MenuStorageResetAutoDownload,
                    TgLocale.MenuStorageViewVersions,
                    TgLocale.MenuStorageViewContacts,
                    TgLocale.MenuStorageViewStories,
                    TgLocale.MenuStorageViewChats
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
        if (prompt.Equals(TgLocale.MenuStorageClearChats))
            return TgEnumMenuStorage.ClearChats;
        if (prompt.Equals(TgLocale.MenuStorageResetAutoDownload))
            return TgEnumMenuStorage.ResetAutoDownload;
        if (prompt.Equals(TgLocale.MenuStorageViewVersions))
            return TgEnumMenuStorage.ViewVersions;
        if (prompt.Equals(TgLocale.MenuStorageViewContacts))
            return TgEnumMenuStorage.ViewContacts;
        if (prompt.Equals(TgLocale.MenuStorageViewStories))
            return TgEnumMenuStorage.ViewStories;
        if (prompt.Equals(TgLocale.MenuStorageViewChats))
            return TgEnumMenuStorage.ViewChats;
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
                    StorageDbClear();
					break;
				case TgEnumMenuStorage.DbBackup:
					StorageDbBackup();
					break;
				case TgEnumMenuStorage.DbCreateNew:
					await StorageDbCreateNewAsync();
					break;
				case TgEnumMenuStorage.TablesVersionsView:
					StorageTablesVersionsView();
					break;
				case TgEnumMenuStorage.TablesCompact:
					await StorageTablesCompactAsync();
					break;
                case TgEnumMenuStorage.ClearChats:
                    await StorageClearChatsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorage.ResetAutoDownload:
                    await RunTaskStatusAsync(tgDownloadSettings, StorageResetAutoDownloadAsync,
                        isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
                    break;
                case TgEnumMenuStorage.ViewVersions:
                    await StorageViewVersionsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorage.ViewContacts:
                    await StorageViewContactsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorage.ViewStories:
                    await StorageViewStoriesAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorage.ViewChats:
                    await StorageViewChatsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorage.Return:
					break;
			}
		} while (menu is not TgEnumMenuStorage.Return);
	}

    private void StorageDbClear()
    {
        throw new NotImplementedException();
    }

    private void StorageDbBackup()
	{
		if (AskQuestionTrueFalseReturnNegative(TgLocale.MenuStorageDbBackup)) return;

		TgLog.WriteLine($"  {TgLocale.MenuStorageBackupDirectory}: {Path.GetDirectoryName(TgAppSettings.AppXml.XmlEfStorage)}");
		var backupResult = BusinessLogicManager.BackupDbAsync();
		TgLog.WriteLine($"  {TgLocale.MenuStorageBackupFile}: {backupResult.FileName}");
		TgLog.WriteLine(backupResult.IsSuccess ? TgLocale.MenuStorageBackupSuccess : TgLocale.MenuStorageBackupFailed);
        TgLog.TypeAnyKeyForReturn();
    }

	private async Task StorageDbCreateNewAsync()
	{
		if (AskQuestionTrueFalseReturnNegative(TgLocale.MenuStorageDbCreateNew)) return;

		// Create and update storage
		await BusinessLogicManager.CreateAndUpdateDbAsync();
	}

	private void ClearStorageData()
	{
		AnsiConsole.WriteLine(TgLocale.MenuStoragePerformSteps);
		AnsiConsole.WriteLine($"  - {TgLocale.MenuStorageExitProgram}");
		AnsiConsole.WriteLine($"  - {TgLocale.MenuStorageDeleteExistsInfo(TgAppSettings.AppXml.XmlEfStorage)}");
        TgLog.TypeAnyKeyForReturn();
    }

	private void StorageTablesVersionsView()
	{
		BusinessLogicManager.VersionsView();
        TgLog.TypeAnyKeyForReturn();
    }

	private async Task StorageTablesCompactAsync()
	{
		if (AskQuestionTrueFalseReturnNegative(TgLocale.MenuStorageTablesCompact)) return;
		// Shrink storage
		await BusinessLogicManager.ShrinkDbAsync();
		TgLog.WriteLine(TgLocale.MenuStorageTablesCompactFinished);
		Console.ReadKey();
	}

    private async Task StorageClearChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuStorageClearChats)) return;

        await ShowTableViewChatsAsync(tgDownloadSettings);
        await BusinessLogicManager.StorageManager.SourceRepository.DeleteAllAsync();
    }

    private async Task StorageResetAutoDownloadAsync(TgDownloadSettingsViewModel _)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuStorageResetAutoDownload)) return;

        var chats = (await BusinessLogicManager.StorageManager.SourceRepository.GetListAsync(TgEnumTableTopRecords.All, 0)).Items;
        chats = [.. chats.Where(sourceSetting => sourceSetting.IsAutoUpdate)];
        foreach (var chat in chats)
        {
            chat.IsAutoUpdate = false;
            await BusinessLogicManager.StorageManager.SourceRepository.SaveAsync(chat);
        }
    }

    private async Task StorageViewContactsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ShowTableViewContactsAsync(tgDownloadSettings);
        var storageResult = await BusinessLogicManager.StorageManager.ContactRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
        var contact = await GetContactFromEnumerableAsync(TgLocale.MenuStorageViewContacts, storageResult.Items);
        if (contact.Uid != Guid.Empty)
        {
            Value = TgEnumMenuMain.ClientDownload;
            tgDownloadSettings = await SetupDownloadSourceByIdAsync(contact.Id);
            await SetupDownloadAsync(tgDownloadSettings);
        }
    }

    private async Task StorageViewChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ShowTableViewChatsAsync(tgDownloadSettings);
        var storageResult = await BusinessLogicManager.StorageManager.SourceRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
        var chat = await GetChatFromEnumerableAsync(TgLocale.MenuStorageViewChats, storageResult.Items);
        if (chat.Uid != Guid.Empty)
        {
            Value = TgEnumMenuMain.ClientDownload;
            tgDownloadSettings = await SetupDownloadSourceByIdAsync(chat.Id);
            await SetupDownloadAsync(tgDownloadSettings);
        }
    }

    private async Task StorageViewStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ShowTableViewStoriesAsync(tgDownloadSettings);
        var storageResult = await BusinessLogicManager.StorageManager.StoryRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
        var story = await GetStoryFromEnumerableAsync(TgLocale.MenuStorageViewChats, storageResult.Items);
        if (story.Uid != Guid.Empty)
        {
            Value = TgEnumMenuMain.ClientDownload;
            tgDownloadSettings = await SetupDownloadSourceByIdAsync(story.Id);
            await SetupDownloadAsync(tgDownloadSettings);
        }
    }

    private async Task StorageViewVersionsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ShowTableViewVersionsAsync(tgDownloadSettings);
        GetVersionFromEnumerable(TgLocale.MenuStorageViewChats, (await BusinessLogicManager.StorageManager.VersionRepository.GetListAsync(TgEnumTableTopRecords.All, 0)).Items);
    }

    #endregion
}