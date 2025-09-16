namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Methods

	private static TgEnumMenuStorageClear SetMenuStorageClear()
	{
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(
            TgLocale.MenuReturn,
            TgLocale.MenuStorageChatsClear,
            TgLocale.MenuStorageFiltersClear
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuStorageChatsClear))
            return TgEnumMenuStorageClear.ClearChats;
        if (prompt.Equals(TgLocale.MenuStorageFiltersClear))
            return TgEnumMenuStorageClear.ClearFilters;

        return TgEnumMenuStorageClear.Return;
	}

	public async Task SetupStorageClearAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuStorageClear menu;
		do
		{
			await ShowTableStorageClearAsync(tgDownloadSettings);
			menu = SetMenuStorageClear();
			switch (menu)
            {
                case TgEnumMenuStorageClear.ClearChats:
                    await StorageClearChatsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorageClear.ClearFilters:
                    await FiltersClearAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorageClear.Return:
					break;
			}
		} while (menu is not TgEnumMenuStorageClear.Return);
	}

    /// <summary> Clear chats from storage </summary>
    private async Task StorageClearChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuStorageChatsClear)) return;

        await ShowTableViewChatsAsync(tgDownloadSettings);
        await BusinessLogicManager.StorageManager.SourceRepository.DeleteAllAsync();
    }

    /// <summary> Clear filters from storage </summary>
    private async Task FiltersClearAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuStorageFiltersClear)) return;

        await BusinessLogicManager.StorageManager.FilterRepository.DeleteAllAsync();
        await FiltersViewAsync(tgDownloadSettings);
    }

    #endregion
}
