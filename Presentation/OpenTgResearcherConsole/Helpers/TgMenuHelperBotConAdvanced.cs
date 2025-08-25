// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
    #region Methods

    private TgEnumMenuBotConAdvanced SetMenuBotConAdvanced()
    {
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(
            TgLocale.MenuReturn,
            TgLocale.MenuBotClearConnectionData,
            TgLocale.MenuBotAutoViewEvents
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuBotClearConnectionData))
            return TgEnumMenuBotConAdvanced.ClearBotConnectionData;
        if (prompt.Equals(TgLocale.MenuBotAutoViewEvents))
            return TgEnumMenuBotConAdvanced.BotAutoViewEvents;

        return TgEnumMenuBotConAdvanced.Return;
    }

    public async Task SetupBotConAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        TgEnumMenuBotConAdvanced menu;
        do
        {
            await ShowTableBotConAdvancedAsync(tgDownloadSettings);

            menu = SetMenuBotConAdvanced();
            switch (menu)
            {
                case TgEnumMenuBotConAdvanced.ClearBotConnectionData:
                    await ClearBotConnectionDataAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuBotConAdvanced.BotAutoViewEvents:
                    await RunTaskStatusAsync(tgDownloadSettings, BotAutoViewEventsAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
                    break;
                case TgEnumMenuBotConAdvanced.Return:
                    break;
            }
        } while (menu is not TgEnumMenuBotConAdvanced.Return);
    }

    public async Task ClearBotConnectionDataAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuBotClearConnectionData)) return;

        await ShowTableBotConAdvancedAsync(tgDownloadSettings);
        await BusinessLogicManager.StorageManager.AppRepository.DeleteAllAsync();
        await BusinessLogicManager.ConnectClient.DisconnectBotAsync();
    }

    private async Task BotAutoViewEventsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        BusinessLogicManager.ConnectClient.IsBotUpdateStatus = true;
        await BusinessLogicManager.ConnectClient.UpdateChatViewModelAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceVm.Dto.FirstId,
            tgDownloadSettings.SourceVm.Dto.Count, "Bot auto view updates is started");
        TgLog.TypeAnyKeyForReturn();
        BusinessLogicManager.ConnectClient.IsBotUpdateStatus = false;
        await Task.CompletedTask;
    }

    #endregion
}
