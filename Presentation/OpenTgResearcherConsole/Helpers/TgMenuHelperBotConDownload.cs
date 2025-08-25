// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
    #region Methods

    private TgEnumMenuBotConDownload SetMenuBotConDownload()
    {
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(
            TgLocale.MenuReturn
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);

        return TgEnumMenuBotConDownload.Return;
    }

    public async Task SetupBotConDownloadAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        TgEnumMenuBotConDownload menu;
        do
        {
            await ShowTableBotConDownloadAsync(tgDownloadSettings);

            menu = SetMenuBotConDownload();
            switch (menu)
            {
                case TgEnumMenuBotConDownload.Return:
                    break;
                default:
                    break;
            }
        } while (menu is not TgEnumMenuBotConDownload.Return);
    }

    #endregion
}
