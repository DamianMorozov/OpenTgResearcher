// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	private static TgEnumMenuStorageAdvanced SetMenuStorageAdvanced()
	{
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(
            TgLocale.MenuReturn,
            TgLocale.MenuStorageResetAutoUpdate,
            TgLocale.MenuStorageViewChats,
            TgLocale.MenuStorageViewContacts,
            TgLocale.MenuStorageViewStories,
            TgLocale.MenuStorageViewVersions
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuStorageResetAutoUpdate))
            return TgEnumMenuStorageAdvanced.ResetAutoUpdate;
        if (prompt.Equals(TgLocale.MenuStorageViewChats))
            return TgEnumMenuStorageAdvanced.ViewChats;
        if (prompt.Equals(TgLocale.MenuStorageViewContacts))
            return TgEnumMenuStorageAdvanced.ViewContacts;
        if (prompt.Equals(TgLocale.MenuFiltersView))
            return TgEnumMenuStorageAdvanced.ViewFilters;
        if (prompt.Equals(TgLocale.MenuStorageViewStories))
            return TgEnumMenuStorageAdvanced.ViewStories;
        if (prompt.Equals(TgLocale.MenuStorageViewVersions))
            return TgEnumMenuStorageAdvanced.ViewVersions;

        return TgEnumMenuStorageAdvanced.Return;
	}

	public async Task SetupStorageAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuStorageAdvanced menu;
		do
		{
			await ShowTableStorageAdvancedAsync(tgDownloadSettings);
			menu = SetMenuStorageAdvanced();
			switch (menu)
            {
                case TgEnumMenuStorageAdvanced.ResetAutoUpdate:
                    await RunTaskStatusAsync(tgDownloadSettings, StorageResetAutoDownloadAsync,
                        isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
                    break;
                case TgEnumMenuStorageAdvanced.ViewChats:
                    await StorageViewChatsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorageAdvanced.ViewContacts:
                    await StorageViewContactsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorageAdvanced.ViewFilters:
                    await FiltersViewAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorageAdvanced.ViewStories:
                    await StorageViewStoriesAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorageAdvanced.ViewVersions:
                    await StorageViewVersionsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorageAdvanced.Return:
					break;
			}
		} while (menu is not TgEnumMenuStorageAdvanced.Return);
	}

    private async Task StorageResetAutoDownloadAsync(TgDownloadSettingsViewModel _)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuStorageResetAutoUpdate)) return;

        await BusinessLogicManager.StorageManager.SourceRepository.ResetAutoUpdateAsync();
    }

    /// <summary> View chats </summary>
    private async Task StorageViewChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ShowTableViewChatsAsync(tgDownloadSettings);
        
        var dtos = await BusinessLogicManager.StorageManager.SourceRepository.GetListDtosAsync();
        dtos = [.. dtos.OrderBy(x => x.UserName).ThenBy(x => x.Title)];
        var dto = await GetDtoFromEnumerableAsync(TgLocale.MenuStorageViewChats, dtos, BusinessLogicManager.StorageManager.SourceRepository);
        if (dto.Uid != Guid.Empty)
        {
            Value = TgEnumMenuMain.ClientConnection;
            tgDownloadSettings = await SetupDownloadSourceByIdAsync(dto.Id);
            await SetupClientConAsync(tgDownloadSettings);
        }
    }

    /// <summary> View contacts </summary>
    private async Task StorageViewContactsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ShowTableViewContactsAsync(tgDownloadSettings);

        var dtos = await BusinessLogicManager.StorageManager.ContactRepository.GetListDtosAsync();
        dtos = [.. dtos.OrderBy(x => x.UserName).ThenBy(x => x.LastName).ThenBy(x => x.FirstName)];
        var dto = await GetDtoFromEnumerableAsync(TgLocale.MenuStorageViewContacts, dtos, BusinessLogicManager.StorageManager.ContactRepository);
        //if (dto.Uid != Guid.Empty)
        //{
        //    Value = TgEnumMenuMain.ClientConnection;
        //    tgDownloadSettings = await SetupDownloadSourceByIdAsync(dto.Id);
        //    await SetupClientConnectionAsync(tgDownloadSettings);
        //}
    }

    /// <summary> View filters </summary>
    private async Task FiltersViewAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ShowTableViewFiltersAsync(tgDownloadSettings);

        var dtos = await BusinessLogicManager.StorageManager.FilterRepository.GetListDtosAsync();
        dtos = [.. dtos.OrderBy(x => x.IsEnabled).ThenBy(x => x.Name)];
        var dto = await GetDtoFromEnumerableAsync(TgLocale.MenuStorageViewFilters, dtos, BusinessLogicManager.StorageManager.FilterRepository);
        //if (dto.Uid != Guid.Empty)
        //{
        //    Value = TgEnumMenuMain.ClientConnection;
        //    tgDownloadSettings = await SetupDownloadSourceByIdAsync(dto.Id);
        //    await SetupClientConnectionAsync(tgDownloadSettings);
        //}
    }

    /// <summary> View stories </summary>
    private async Task StorageViewStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ShowTableViewStoriesAsync(tgDownloadSettings);

        var dtos = await BusinessLogicManager.StorageManager.StoryRepository.GetListDtosAsync();
        dtos = [.. dtos.OrderBy(x => x.Id).ThenBy(x => x.Date)];
        var dto = await GetDtoFromEnumerableAsync(TgLocale.MenuStorageViewFilters, dtos, BusinessLogicManager.StorageManager.StoryRepository);
        //if (dto.Uid != Guid.Empty)
        //{
        //    Value = TgEnumMenuMain.ClientConnection;
        //    tgDownloadSettings = await SetupDownloadSourceByIdAsync(dto.Id);
        //    await SetupClientConnectionAsync(tgDownloadSettings);
        //}
    }

    /// <summary> View versions </summary>
    private async Task StorageViewVersionsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ShowTableViewVersionsAsync(tgDownloadSettings);

        var dtos = await BusinessLogicManager.StorageManager.VersionRepository.GetListDtosAsync();
        dtos = [.. dtos.OrderBy(x => x.Version)];
        var dto = await GetDtoFromEnumerableAsync(TgLocale.MenuStorageViewFilters, dtos, BusinessLogicManager.StorageManager.VersionRepository);
        //if (dto.Uid != Guid.Empty)
        //{
        //    Value = TgEnumMenuMain.ClientConnection;
        //    tgDownloadSettings = await SetupDownloadSourceByIdAsync(dto.Id);
        //    await SetupClientConnectionAsync(tgDownloadSettings);
        //}
    }

    #endregion
}