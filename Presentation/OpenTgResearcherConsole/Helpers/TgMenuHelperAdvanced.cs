// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	private TgEnumMenuDownload SetMenuAdvanced()
	{
        var selectionPrompt = new SelectionPrompt<string>()
                .Title($"  {TgLocale.MenuSwitchNumber}")
                .PageSize(Console.WindowHeight - 17)
                .MoreChoicesText(TgLocale.MoveUpDown)
                .AddChoices(
                    TgLocale.MenuReturn,
                    TgLocale.MenuClientStartAutoDownload,
                    TgLocale.MenuClientAutoViewEvents,
                    TgLocale.MenuClientSearchContacts,
                    TgLocale.MenuClientSearchChats,
                    TgLocale.MenuClientSearchDialogs,
                    TgLocale.MenuClientSearchStories,
                    TgLocale.MenuClientMarkAllMessagesAsRead);
        // Check paid license
        if (BusinessLogicManager.LicenseService.CurrentLicense.CheckPaidLicense())
        {
            selectionPrompt.AddChoice(TgLocale.MenuBotAutoViewEvents);
        }

        var prompt = AnsiConsole.Prompt(selectionPrompt);

        if (prompt.Equals(TgLocale.MenuClientStartAutoDownload))
			return TgEnumMenuDownload.ClientStartAutoDownload;
		if (prompt.Equals(TgLocale.MenuClientAutoViewEvents))
			return TgEnumMenuDownload.ClientAutoViewEvents;
		if (prompt.Equals(TgLocale.MenuClientSearchChats))
			return TgEnumMenuDownload.ClientSearchChats;
		if (prompt.Equals(TgLocale.MenuClientSearchDialogs))
			return TgEnumMenuDownload.ClientSearchDialogs;
		if (prompt.Equals(TgLocale.MenuClientSearchContacts))
			return TgEnumMenuDownload.ClientSearchContacts;
		if (prompt.Equals(TgLocale.MenuClientSearchStories))
			return TgEnumMenuDownload.ClientSearchStories;
		if (prompt.Equals(TgLocale.MenuClientMarkAllMessagesAsRead))
			return TgEnumMenuDownload.ClientMarkAllMessagesAsRead;
        if (prompt.Equals(TgLocale.MenuBotAutoViewEvents))
            return TgEnumMenuDownload.BotAutoViewEvents;
        return TgEnumMenuDownload.Return;
	}

	public async Task SetupAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuDownload menu;
		do
		{
			await ShowTableAdvancedAsync(tgDownloadSettings);
			menu = SetMenuAdvanced();
			switch (menu)
			{
                case TgEnumMenuDownload.ClientStartAutoDownload:
					await RunTaskStatusAsync(tgDownloadSettings, ClientStartAutoDownloadAsync, 
						isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
					break;
				case TgEnumMenuDownload.ClientAutoViewEvents:
					await RunTaskStatusAsync(tgDownloadSettings, ClientAutoViewEventsAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
					break;
				case TgEnumMenuDownload.ClientSearchChats:
					await ClientSearchAsync(tgDownloadSettings, TgEnumSourceType.Chat);
					break;
				case TgEnumMenuDownload.ClientSearchDialogs:
					await ClientSearchAsync(tgDownloadSettings, TgEnumSourceType.Dialog);
					break;
				case TgEnumMenuDownload.ClientSearchContacts:
					await ClientSearchAsync(tgDownloadSettings, TgEnumSourceType.Contact);
					break;
				case TgEnumMenuDownload.ClientSearchStories:
					await ClientSearchAsync(tgDownloadSettings, TgEnumSourceType.Story);
					break;
				case TgEnumMenuDownload.ClientMarkAllMessagesAsRead:
					await ClientMarkAllMessagesAsReadAsync(tgDownloadSettings);
					break;
                case TgEnumMenuDownload.BotAutoViewEvents:
                    await RunTaskStatusAsync(tgDownloadSettings, BotAutoViewEventsAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
                    break;
            }
        } while (menu is not TgEnumMenuDownload.Return);
	}

    private async Task ClientSearchAsync(TgDownloadSettingsViewModel tgDownloadSettings, TgEnumSourceType sourceType)
	{
		await ShowTableAdvancedAsync(tgDownloadSettings);
		if (!BusinessLogicManager.ConnectClient.IsReady)
		{
			TgLog.MarkupWarning(TgLocale.TgMustClientConnect);
			Console.ReadKey();
			return;
		}

		await RunTaskStatusAsync(tgDownloadSettings, async _ => { await BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(tgDownloadSettings, sourceType); }, 
			isSkipCheckTgSettings: true, isScanCount: true, isWaitComplete: true);
	}

	private async Task ClientMarkAllMessagesAsReadAsync(TgDownloadSettingsViewModel tgDownloadSettings) => 
		await RunTaskStatusAsync(tgDownloadSettings, MarkHistoryReadCoreAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);

	private async Task ClientStartAutoDownloadAsync(TgDownloadSettingsViewModel _)
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuClientStartAutoDownload)) return;

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
				await ManualDownloadAsync(tgDownloadSettings);
		}
	}

	private async Task ClientAutoViewEventsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		BusinessLogicManager.ConnectClient.IsClientUpdateStatus = true;
		await BusinessLogicManager.ConnectClient.UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceVm.Dto.FirstId,
			tgDownloadSettings.SourceVm.Dto.Count, "Client auto view updates is started");
		TgLog.TypeAnyKeyForReturn();
		BusinessLogicManager.ConnectClient.IsClientUpdateStatus = false;
		await Task.CompletedTask;
	}

	private async Task BotAutoViewEventsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		BusinessLogicManager.ConnectClient.IsBotUpdateStatus = true;
		await BusinessLogicManager.ConnectClient.UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceVm.Dto.FirstId,
			tgDownloadSettings.SourceVm.Dto.Count, "Bot auto view updates is started");
		TgLog.TypeAnyKeyForReturn();
		BusinessLogicManager.ConnectClient.IsBotUpdateStatus = false;
		await Task.CompletedTask;
	}

	#endregion
}