namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Methods

	private static TgEnumMenuStorage SetMenuStorage()
	{
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(
            TgLocale.MenuReturn,
            TgLocale.MenuStorageSetup,
            TgLocale.MenuStorageAdvanced,
            TgLocale.MenuStorageClearing,
            TgLocale.MenuStorageChats,
            TgLocale.MenuStorageFilters
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuStorageSetup))
            return TgEnumMenuStorage.Setup;
        if (prompt.Equals(TgLocale.MenuStorageAdvanced))
            return TgEnumMenuStorage.Advanced;
        if (prompt.Equals(TgLocale.MenuStorageClearing))
            return TgEnumMenuStorage.Clear;
        if (prompt.Equals(TgLocale.MenuStorageChats))
            return TgEnumMenuStorage.Chats;
        if (prompt.Equals(TgLocale.MenuStorageFilters))
            return TgEnumMenuStorage.Filters;

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
                case TgEnumMenuStorage.Setup:
                    await SetupStorageSetupAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorage.Advanced:
                    await SetupStorageAdvancedAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorage.Clear:
                    await SetupStorageClearAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorage.Chats:
                    await SetupStorageChatsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorage.Filters:
                    await SetupStorageFilterAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorage.Return:
                    break;
            }
        } while (menu is not TgEnumMenuStorage.Return);
    }

    #endregion
}
