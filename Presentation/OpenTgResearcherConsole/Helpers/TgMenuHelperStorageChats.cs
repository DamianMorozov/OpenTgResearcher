// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

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
			}
		} while (menu is not TgEnumMenuStorageChats.Return);
	}

    #endregion
}