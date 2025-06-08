// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming


namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
    #region Public and private methods

    private TgEnumMenuClientConAdvanced SetMenuClientConAdvanced()
    {
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(
            TgLocale.MenuReturn,
            TgLocale.MenuClientAdvancedStartAutoDownload,
            TgLocale.MenuClientAdvancedAutoViewEvents,
            TgLocale.MenuClientAdvancedSearchContacts,
            TgLocale.MenuClientAdvancedSearchChats,
            TgLocale.MenuClientAdvancedSearchDialogs,
            TgLocale.MenuClientAdvancedSearchStories,
            TgLocale.MenuClientAdvancedMarkAllMessagesAsRead,
            TgLocale.MenuStorageViewChats
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuClientAdvancedStartAutoDownload))
            return TgEnumMenuClientConAdvanced.AdvancedStartAutoDownload;
        if (prompt.Equals(TgLocale.MenuClientAdvancedAutoViewEvents))
            return TgEnumMenuClientConAdvanced.AdvancedAutoViewEvents;
        if (prompt.Equals(TgLocale.MenuClientAdvancedSearchChats))
            return TgEnumMenuClientConAdvanced.AdvancedSearchChats;
        if (prompt.Equals(TgLocale.MenuClientAdvancedSearchDialogs))
            return TgEnumMenuClientConAdvanced.AdvancedSearchDialogs;
        if (prompt.Equals(TgLocale.MenuClientAdvancedSearchContacts))
            return TgEnumMenuClientConAdvanced.AdvancedSearchContacts;
        if (prompt.Equals(TgLocale.MenuClientAdvancedSearchStories))
            return TgEnumMenuClientConAdvanced.AdvancedSearchStories;
        if (prompt.Equals(TgLocale.MenuClientAdvancedMarkAllMessagesAsRead))
            return TgEnumMenuClientConAdvanced.AdvancedMarkAllMessagesAsRead;
        if (prompt.Equals(TgLocale.MenuStorageViewChats))
            return TgEnumMenuClientConAdvanced.AdvancedViewChats;

        return TgEnumMenuClientConAdvanced.Return;
    }

    public async Task SetupClientConAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        TgEnumMenuClientConAdvanced menu;
        do
        {
            await ShowTableClientConAdvancedAsync(tgDownloadSettings);

            menu = SetMenuClientConAdvanced();
            switch (menu)
            {
                case TgEnumMenuClientConAdvanced.AdvancedStartAutoDownload:
                    await RunTaskStatusAsync(tgDownloadSettings, ClientAdvancedStartAutoDownloadAsync,
                        isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
                    break;
                case TgEnumMenuClientConAdvanced.AdvancedAutoViewEvents:
                    await RunTaskStatusAsync(tgDownloadSettings, ClientAdvancedAutoViewEventsAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
                    break;
                case TgEnumMenuClientConAdvanced.AdvancedSearchChats:
                    await ClientAdvancedSearchAsync(tgDownloadSettings, TgEnumSourceType.Chat);
                    break;
                case TgEnumMenuClientConAdvanced.AdvancedSearchDialogs:
                    await ClientAdvancedSearchAsync(tgDownloadSettings, TgEnumSourceType.Dialog);
                    break;
                case TgEnumMenuClientConAdvanced.AdvancedSearchContacts:
                    await ClientAdvancedSearchAsync(tgDownloadSettings, TgEnumSourceType.Contact);
                    break;
                case TgEnumMenuClientConAdvanced.AdvancedSearchStories:
                    await ClientAdvancedSearchAsync(tgDownloadSettings, TgEnumSourceType.Story);
                    break;
                case TgEnumMenuClientConAdvanced.AdvancedMarkAllMessagesAsRead:
                    await ClientAdvancedMarkAllMessagesAsReadAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConAdvanced.AdvancedViewChats:
                    await StorageViewChatsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConAdvanced.Return:
                    break;
            }
        } while (menu is not TgEnumMenuClientConAdvanced.Return);
    }

    private async Task ClientAdvancedStartAutoDownloadAsync(TgDownloadSettingsViewModel _)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuClientAdvancedStartAutoDownload)) return;

        var chats = (await BusinessLogicManager.StorageManager.SourceRepository.GetListAsync(TgEnumTableTopRecords.All, 0)).Items;
        chats = [.. chats.Where(sourceSetting => sourceSetting.IsAutoUpdate)];
        foreach (var chat in chats)
        {
            var tgDownloadSettings = await SetupDownloadSourceByIdAsync(chat.Id);
            var chatId = string.IsNullOrEmpty(chat.UserName) ? $"{chat.Id}" : $"{chat.Id} | @{chat.UserName}";
            // StatusContext
            await BusinessLogicManager.ConnectClient.UpdateStateSourceAsync(chat.Id, chat.FirstId, chat.Count,
                chat.Count <= 0
                    ? $"The source {chatId} hasn't any messages!"
                    : $"The source {chatId} has {chat.Count} messages.");
            // ManualDownload
            if (chat.Count > 0)
                await DownloadManualAsync(tgDownloadSettings);
        }
    }

    private async Task ClientAdvancedAutoViewEventsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        BusinessLogicManager.ConnectClient.IsClientUpdateStatus = true;
        await BusinessLogicManager.ConnectClient.UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceVm.Dto.FirstId,
            tgDownloadSettings.SourceVm.Dto.Count, "Client auto view updates is started");
        TgLog.TypeAnyKeyForReturn();
        BusinessLogicManager.ConnectClient.IsClientUpdateStatus = false;
        await Task.CompletedTask;
    }

    private async Task ClientAdvancedSearchAsync(TgDownloadSettingsViewModel tgDownloadSettings, TgEnumSourceType sourceType)
    {
        await ShowTableClientConAdvancedAsync(tgDownloadSettings);
        if (!BusinessLogicManager.ConnectClient.IsReady)
        {
            TgLog.MarkupWarning(TgLocale.TgMustClientConnect);
            Console.ReadKey();
            return;
        }

        await RunTaskStatusAsync(tgDownloadSettings, async _ => { await BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(tgDownloadSettings, sourceType); },
            isSkipCheckTgSettings: true, isScanCount: true, isWaitComplete: true);
    }

    private async Task ClientAdvancedMarkAllMessagesAsReadAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuClientAdvancedMarkAllMessagesAsRead)) return;
        await RunTaskStatusAsync(tgDownloadSettings, MarkHistoryReadCoreAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
    }

    #endregion
}
