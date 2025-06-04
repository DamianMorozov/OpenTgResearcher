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
				.AddChoices(TgLocale.MenuReturn,
					TgLocale.MenuStorageDbClear,
					TgLocale.MenuStorageDbBackup,
					TgLocale.MenuStorageTablesCompact,
                    TgLocale.MenuStorageClearChats,
                    TgLocale.MenuStorageResetAutoDownload,
                    // View
                    TgLocale.MenuFiltersView,
                    TgLocale.MenuStorageViewChats,
                    TgLocale.MenuStorageViewContacts,
                    TgLocale.MenuStorageViewStories,
                    TgLocale.MenuStorageViewVersions,
                    // Filters
                    TgLocale.MenuFiltersClear,
                    TgLocale.MenuFiltersAdd,
                    TgLocale.MenuFiltersEdit,
                    TgLocale.MenuFiltersRemove
                ));
		if (prompt.Equals(TgLocale.MenuStorageDbClear))
			return TgEnumMenuStorage.DbClear;
		if (prompt.Equals(TgLocale.MenuStorageDbBackup))
			return TgEnumMenuStorage.DbBackup;
		if (prompt.Equals(TgLocale.MenuStorageTablesCompact))
			return TgEnumMenuStorage.TablesCompact;
        if (prompt.Equals(TgLocale.MenuStorageClearChats))
            return TgEnumMenuStorage.ClearChats;
        if (prompt.Equals(TgLocale.MenuStorageResetAutoDownload))
            return TgEnumMenuStorage.ResetAutoDownload;
        // View
        if (prompt.Equals(TgLocale.MenuFiltersView))
            return TgEnumMenuStorage.FiltersView;
        if (prompt.Equals(TgLocale.MenuStorageViewChats))
            return TgEnumMenuStorage.ViewChats;
        if (prompt.Equals(TgLocale.MenuStorageViewContacts))
            return TgEnumMenuStorage.ViewContacts;
        if (prompt.Equals(TgLocale.MenuStorageViewStories))
            return TgEnumMenuStorage.ViewStories;
        if (prompt.Equals(TgLocale.MenuStorageViewVersions))
            return TgEnumMenuStorage.ViewVersions;
        // Filters
        if (prompt.Equals(TgLocale.MenuFiltersClear))
            return TgEnumMenuStorage.FiltersClear;
        if (prompt.Equals(TgLocale.MenuFiltersAdd))
            return TgEnumMenuStorage.FiltersAdd;
        if (prompt.Equals(TgLocale.MenuFiltersEdit))
            return TgEnumMenuStorage.FiltersEdit;
        if (prompt.Equals(TgLocale.MenuFiltersRemove))
            return TgEnumMenuStorage.FiltersRemove;
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
                // View
                case TgEnumMenuStorage.ViewChats:
                    await StorageViewChatsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorage.FiltersView:
                    await FiltersViewAsync();
                    break;
                case TgEnumMenuStorage.ViewContacts:
                    await StorageViewContactsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorage.ViewStories:
                    await StorageViewStoriesAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorage.ViewVersions:
                    await StorageViewVersionsAsync(tgDownloadSettings);
                    break;
                // Filters
                case TgEnumMenuStorage.FiltersClear:
                    await ClearFiltersAsync();
                    break;
                case TgEnumMenuStorage.FiltersAdd:
                    await SetFiltersAddAsync();
                    break;
                case TgEnumMenuStorage.FiltersEdit:
                    await SetFiltersEditAsync();
                    break;
                case TgEnumMenuStorage.FiltersRemove:
                    await SetFiltersRemoveAsync();
                    break;
                case TgEnumMenuStorage.Return:
					break;
			}
		} while (menu is not TgEnumMenuStorage.Return);
	}

    private void StorageDbClear()
    {
        AnsiConsole.WriteLine($"  {TgLocale.MenuStoragePerformSteps}");
        AnsiConsole.WriteLine($"  - {TgLocale.MenuStorageExitProgram}");
        AnsiConsole.WriteLine($"  - {TgLocale.MenuStorageDeleteExistsInfo(TgAppSettings.AppXml.XmlEfStorage)}");
        TgLog.TypeAnyKeyForReturn();
    }

    private void StorageDbBackup()
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuStorageDbBackup)) return;

		TgLog.WriteLine($"  {TgLocale.MenuStorageBackupDirectory}: {Path.GetDirectoryName(TgAppSettings.AppXml.XmlEfStorage)}");
		var backupResult = BusinessLogicManager.BackupDbAsync();
		TgLog.WriteLine($"  {TgLocale.MenuStorageBackupFile}: {backupResult.FileName}");
		TgLog.WriteLine(backupResult.IsSuccess ? $"  {TgLocale.MenuStorageBackupSuccess}" : $"  {TgLocale.MenuStorageBackupFailed}");
        TgLog.TypeAnyKeyForReturn();
    }

	private async Task StorageTablesCompactAsync()
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuStorageTablesCompact)) return;
		// Shrink storage
		await BusinessLogicManager.ShrinkDbAsync();
		TgLog.WriteLine($"  {TgLocale.MenuStorageTablesCompactFinished}");
        TgLog.TypeAnyKeyForReturn();
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

    /// <summary> View filters </summary>
    private async Task FiltersViewAsync()
    {
        var storageResult = await BusinessLogicManager.StorageManager.FilterRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
        if (storageResult.IsExists)
        {
            foreach (var filter in storageResult.Items)
            {
                TgLog.WriteLine(filter.ToConsoleString());
            }
        }
        TgLog.TypeAnyKeyForReturn();
    }

    private async Task StorageViewStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ShowTableViewStoriesAsync(tgDownloadSettings);
        var storageResult = await BusinessLogicManager.StorageManager.StoryRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
        var story = await GetStoryFromEnumerableAsync(TgLocale.MenuStorageViewStories, storageResult.Items);
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
        GetVersionFromEnumerable(TgLocale.MenuStorageViewVersions, 
            (await BusinessLogicManager.StorageManager.VersionRepository.GetListAsync(TgEnumTableTopRecords.All, 0)).Items);
    }

    private async Task SetFiltersAddAsync()
    {
        var filter = new TgEfFilterEntity();
        var type = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuFiltersSetType}")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(TgLocale.MenuReturn, TgLocale.MenuFiltersSetSingleName, TgLocale.MenuFiltersSetSingleExtension,
                TgLocale.MenuFiltersSetMultiName, TgLocale.MenuFiltersSetMultiExtension,
                TgLocale.MenuFiltersSetMinSize, TgLocale.MenuFiltersSetMaxSize));
        if (Equals(type, TgLocale.MenuReturn))
            return;

        filter.IsEnabled = true;
        filter.Name = AnsiConsole.Ask<string>(TgLog.GetMarkupString($"{TgLocale.MenuFiltersSetName}:"));
        switch (type)
        {
            case "Single name":
                filter.FilterType = TgEnumFilterType.SingleName;
                break;
            case "Single extension":
                filter.FilterType = TgEnumFilterType.SingleExtension;
                break;
            case "Multi name":
                filter.FilterType = TgEnumFilterType.MultiName;
                break;
            case "Multi extension":
                filter.FilterType = TgEnumFilterType.MultiExtension;
                break;
            case "File minimum size":
                filter.FilterType = TgEnumFilterType.MinSize;
                break;
            case "File maximum size":
                filter.FilterType = TgEnumFilterType.MaxSize;
                break;
        }
        switch (filter.FilterType)
        {
            case TgEnumFilterType.SingleName:
            case TgEnumFilterType.SingleExtension:
            case TgEnumFilterType.MultiName:
            case TgEnumFilterType.MultiExtension:
                filter.Mask = AnsiConsole.Ask<string>(TgLog.GetMarkupString($"{TgLocale.MenuFiltersSetMask}:"));
                break;
            case TgEnumFilterType.MinSize:
                SetFilterSize(filter, TgLocale.MenuFiltersSetMinSize);
                break;
            case TgEnumFilterType.MaxSize:
                SetFilterSize(filter, TgLocale.MenuFiltersSetMaxSize);
                break;
        }

        await BusinessLogicManager.StorageManager.FilterRepository.SaveAsync(filter);
        await FiltersViewAsync();
    }

    /// <summary> Edit filter </summary>
    private async Task SetFiltersEditAsync()
    {
        var storageResult = await BusinessLogicManager.StorageManager.FilterRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
        var filter = await GetFilterFromEnumerableAsync(TgLocale.MenuStorageViewFilters, storageResult.Items);
        filter.IsEnabled = AskQuestionTrueFalseReturnPositive(TgLocale.MenuFiltersSetIsEnabled, true);
        await BusinessLogicManager.StorageManager.FilterRepository.SaveAsync(filter);
        await FiltersViewAsync();
    }

    /// <summary> Set filter size </summary>
    private void SetFilterSize(TgEfFilterEntity filter, string question)
    {
        filter.SizeType = AnsiConsole.Prompt(new SelectionPrompt<TgEnumFileSizeType>()
            .Title($"  {TgLocale.MenuFiltersSetSizeType}")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(TgEnumFileSizeType.Bytes, TgEnumFileSizeType.KBytes, TgEnumFileSizeType.MBytes, TgEnumFileSizeType.GBytes, TgEnumFileSizeType.TBytes));
        filter.Size = AnsiConsole.Ask<uint>(TgLog.GetMarkupString($"{question}:"));
    }

    /// <summary> Remove filter </summary>
    private async Task SetFiltersRemoveAsync()
    {
        var storageResult = await BusinessLogicManager.StorageManager.FilterRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
        var filter = await GetFilterFromEnumerableAsync(TgLocale.MenuStorageViewFilters, storageResult.Items);
        await BusinessLogicManager.StorageManager.FilterRepository.DeleteAsync(filter);
        await FiltersViewAsync();
    }

    /// <summary> Clear filters </summary>
    private async Task ClearFiltersAsync()
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuFiltersClear)) return;

        await BusinessLogicManager.StorageManager.FilterRepository.DeleteAllAsync();
        await FiltersViewAsync();
    }

    #endregion
}