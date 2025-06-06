// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	private TgEnumMenuBotCon SetMenuBotCon()
	{
		var selectionPrompt = new SelectionPrompt<string>()
			.Title($"  {TgLocale.MenuSwitchNumber}")
			.PageSize(Console.WindowHeight - 17)
			.MoreChoicesText(TgLocale.MoveUpDown);
		selectionPrompt.AddChoices(
			TgLocale.MenuReturn,
            TgLocale.MenuBotSetup,
            TgLocale.MenuBotAdvanced,
            TgLocale.MenuBotDownload
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuBotSetup))
			return TgEnumMenuBotCon.BotSetup;
        if (prompt.Equals(TgLocale.MenuBotAdvanced))
            return TgEnumMenuBotCon.BotAdvanced;
        if (prompt.Equals(TgLocale.MenuBotDownload))
            return TgEnumMenuBotCon.BotDownload;

        return TgEnumMenuBotCon.Return;
    }

    public async Task SetupBotConAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuBotCon menu;
		do
		{
			await ShowTableBotConAsync(tgDownloadSettings);

			menu = SetMenuBotCon();
            switch (menu)
            {
                case TgEnumMenuBotCon.BotSetup:
                    await SetupBotConSetupAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuBotCon.BotAdvanced:
                    await SetupBotConAdvancedAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuBotCon.BotDownload:
                    await SetupBotConDownloadAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuBotCon.Return:
                    break;
            }
        } while (menu is not TgEnumMenuBotCon.Return);
    }

    #endregion
}