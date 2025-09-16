namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Methods

	private static TgEnumMenuStorageChats SetMenuStorageChats()
	{
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(
            TgLocale.MenuReturn,
            TgLocale.MenuStorageChatsClear,
            TgLocale.MenuStorageViewChats
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuStorageChatsClear))
            return TgEnumMenuStorageChats.ChatsClear;
        if (prompt.Equals(TgLocale.MenuStorageViewChats))
            return TgEnumMenuStorageChats.ChatsView;

        return TgEnumMenuStorageChats.Return;
	}

	public async Task SetupStorageChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuStorageChats menu;
		do
		{
			await ShowTableStorageChatsAsync(tgDownloadSettings);
			menu = SetMenuStorageChats();
            switch (menu)
            {
                case TgEnumMenuStorageChats.ChatsView:
                    await StorageViewChatsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorageChats.ChatsClear:
                    await StorageClearChatsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuStorageChats.Return:
                    break;
            }
        } while (menu is not TgEnumMenuStorageChats.Return);
    }

    #endregion
}
