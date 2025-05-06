// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace TgDownloaderConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	private static TgEnumMenuDownload SetMenuAdvanced()
	{
		var prompt = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
				.Title($"  {TgLocale.MenuSwitchNumber}")
				.PageSize(Console.WindowHeight - 17)
				.MoreChoicesText(TgLocale.MoveUpDown)
				.AddChoices(
					TgLocale.MenuMainReturn,
					TgLocale.MenuClearChats,
					TgLocale.MenuClearAutoDownload,
					TgLocale.MenuStartAutoDownload,
					TgLocale.MenuAutoViewEvents,
					TgLocale.MenuSearchContacts,
					TgLocale.MenuSearchChats,
					TgLocale.MenuSearchDialogs,
					TgLocale.MenuSearchStories,
					TgLocale.MenuMarkAllMessagesAsRead,
					TgLocale.MenuViewVersions,
					TgLocale.MenuViewContacts,
					TgLocale.MenuViewStories,
					TgLocale.MenuViewChats
				));
		if (prompt.Equals(TgLocale.MenuClearChats))
			return TgEnumMenuDownload.ClearChats;
		if (prompt.Equals(TgLocale.MenuClearAutoDownload))
			return TgEnumMenuDownload.ClearAutoDownload;
		if (prompt.Equals(TgLocale.MenuStartAutoDownload))
			return TgEnumMenuDownload.StartAutoDownload;
		if (prompt.Equals(TgLocale.MenuAutoViewEvents))
			return TgEnumMenuDownload.AutoViewEvents;
		if (prompt.Equals(TgLocale.MenuSearchChats))
			return TgEnumMenuDownload.SearchChats;
		if (prompt.Equals(TgLocale.MenuSearchDialogs))
			return TgEnumMenuDownload.SearchDialogs;
		if (prompt.Equals(TgLocale.MenuSearchContacts))
			return TgEnumMenuDownload.SearchContacts;
		if (prompt.Equals(TgLocale.MenuSearchStories))
			return TgEnumMenuDownload.SearchStories;
		if (prompt.Equals(TgLocale.MenuMarkAllMessagesAsRead))
			return TgEnumMenuDownload.MarkHistoryRead;
		if (prompt.Equals(TgLocale.MenuViewVersions))
			return TgEnumMenuDownload.ViewVersions;
		if (prompt.Equals(TgLocale.MenuViewContacts))
			return TgEnumMenuDownload.ViewContacts;
		if (prompt.Equals(TgLocale.MenuViewStories))
			return TgEnumMenuDownload.ViewStories;
		if (prompt.Equals(TgLocale.MenuViewChats))
			return TgEnumMenuDownload.ViewChats;
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
				case TgEnumMenuDownload.ClearChats:
					await ClearChatsAsync(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.ClearAutoDownload:
					await RunTaskStatusAsync(tgDownloadSettings, ClearAutoDownloadAsync, 
						isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
					break;
				case TgEnumMenuDownload.StartAutoDownload:
					await RunTaskStatusAsync(tgDownloadSettings, StartAutoDownloadAsync, 
						isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
					break;
				case TgEnumMenuDownload.AutoViewEvents:
					await RunTaskStatusAsync(tgDownloadSettings, AutoViewEventsAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
					break;
				case TgEnumMenuDownload.SearchChats:
					await SearchSourcesAsync(tgDownloadSettings, TgEnumSourceType.Chat);
					break;
				case TgEnumMenuDownload.SearchDialogs:
					await SearchSourcesAsync(tgDownloadSettings, TgEnumSourceType.Dialog);
					break;
				case TgEnumMenuDownload.SearchContacts:
					await SearchSourcesAsync(tgDownloadSettings, TgEnumSourceType.Contact);
					break;
				case TgEnumMenuDownload.SearchStories:
					await SearchSourcesAsync(tgDownloadSettings, TgEnumSourceType.Story);
					break;
				case TgEnumMenuDownload.MarkHistoryRead:
					await MarkHistoryReadAsync(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.ViewVersions:
					await ViewVersionsAsync(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.ViewContacts:
					await ViewContactsAsync(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.ViewStories:
                    await ViewStoriesAsync(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.ViewChats:
					await ViewChatsAsync(tgDownloadSettings);
					break;
			}
		} while (menu is not TgEnumMenuDownload.Return);
	}

	private async Task SearchSourcesAsync(TgDownloadSettingsViewModel tgDownloadSettings, TgEnumSourceType sourceType)
	{
		await ShowTableAdvancedAsync(tgDownloadSettings);
		if (!TgGlobalTools.ConnectClient.IsReady)
		{
			TgLog.MarkupWarning(TgLocale.TgMustClientConnect);
			Console.ReadKey();
			return;
		}

		await RunTaskStatusAsync(tgDownloadSettings, async _ => { await TgGlobalTools.ConnectClient.SearchSourcesTgAsync(tgDownloadSettings, sourceType); }, 
			isSkipCheckTgSettings: true, isScanCount: true, isWaitComplete: true);
	}

	private async Task ViewContactsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await ShowTableViewContactsAsync(tgDownloadSettings);
		var storageResult = await ContactRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
		var contact = await GetContactFromEnumerableAsync(TgLocale.MenuViewContacts, storageResult.Items);
		if (contact.Uid != Guid.Empty)
		{
			Value = TgEnumMenuMain.Download;
			tgDownloadSettings = await SetupDownloadSourceByIdAsync(contact.Id);
			await SetupDownloadAsync(tgDownloadSettings);
		}
	}

	private async Task ViewChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await ShowTableViewChatsAsync(tgDownloadSettings);
		var storageResult = await SourceRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
		var chat = await GetChatFromEnumerableAsync(TgLocale.MenuViewChats, storageResult.Items);
		if (chat.Uid != Guid.Empty)
		{
			Value = TgEnumMenuMain.Download;
			tgDownloadSettings = await SetupDownloadSourceByIdAsync(chat.Id);
			await SetupDownloadAsync(tgDownloadSettings);
		}
	}

	private async Task ClearChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuClearChats)) return;

		await ShowTableViewChatsAsync(tgDownloadSettings);
		await SourceRepository.DeleteAllAsync();
	}

	private async Task ViewStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await ShowTableViewStoriesAsync(tgDownloadSettings);
		var storageResult = await StoryRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
		var story = await GetStoryFromEnumerableAsync(TgLocale.MenuViewChats, storageResult.Items);
		if (story.Uid != Guid.Empty)
		{
			Value = TgEnumMenuMain.Download;
			tgDownloadSettings = await SetupDownloadSourceByIdAsync(story.Id);
			await SetupDownloadAsync(tgDownloadSettings);
		}
	}

	private async Task ViewVersionsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await ShowTableViewVersionsAsync(tgDownloadSettings);
		GetVersionFromEnumerable(TgLocale.MenuViewChats, (await VersionRepository.GetListAsync(TgEnumTableTopRecords.All, 0)).Items);
	}

	private async Task MarkHistoryReadAsync(TgDownloadSettingsViewModel tgDownloadSettings) => 
		await RunTaskStatusAsync(tgDownloadSettings, MarkHistoryReadCoreAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);

	private async Task ClearAutoDownloadAsync(TgDownloadSettingsViewModel _)
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuClearAutoDownload)) return;

		var chats = (await SourceRepository.GetListAsync(TgEnumTableTopRecords.All, 0)).Items;
		chats = [.. chats.Where(sourceSetting => sourceSetting.IsAutoUpdate)];
		foreach (var chat in chats)
		{
			chat.IsAutoUpdate = false;
			await SourceRepository.SaveAsync(chat);
		}
	}

	private async Task StartAutoDownloadAsync(TgDownloadSettingsViewModel _)
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuStartAutoDownload)) return;

		var chats = (await SourceRepository.GetListAsync(TgEnumTableTopRecords.All, 0)).Items;
		chats = [.. chats.Where(sourceSetting => sourceSetting.IsAutoUpdate)];
		foreach (var chat in chats)
		{
			var tgDownloadSettings = await SetupDownloadSourceByIdAsync(chat.Id);
			var chatId = string.IsNullOrEmpty(chat.UserName) ? $"{chat.Id}" : $"{chat.Id} | @{chat.UserName}";
			// StatusContext
			await TgGlobalTools.ConnectClient.UpdateStateSourceAsync(chat.Id, chat.FirstId, chat.Count,
				chat.Count <= 0
					? $"The source {chatId} hasn't any messages!"
					: $"The source {chatId} has {chat.Count} messages.");
			// ManualDownload
			if (chat.Count > 0)
				await ManualDownloadAsync(tgDownloadSettings);
		}
	}

	private async Task AutoViewEventsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgGlobalTools.ConnectClient.IsUpdateStatus = true;
		await TgGlobalTools.ConnectClient.UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceVm.Dto.FirstId,
			tgDownloadSettings.SourceVm.Dto.Count, "Auto view updates is started");
		TgLog.MarkupLine(TgLocale.TypeAnyKeyForReturn);
		Console.ReadKey();
		TgGlobalTools.ConnectClient.IsUpdateStatus = false;
		await Task.CompletedTask;
	}

	#endregion
}