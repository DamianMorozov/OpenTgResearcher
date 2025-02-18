// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace TgDownloaderConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	private TgEnumMenuDownload SetMenuAdvanced()
	{
		var prompt = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
				.Title($"  {TgLocale.MenuSwitchNumber}")
				.PageSize(Console.WindowHeight - 17)
				.MoreChoicesText(TgLocale.MoveUpDown)
				.AddChoices(
					TgLocale.MenuMainReturn,
					TgLocale.MenuAutoDownload,
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
		if (prompt.Equals(TgLocale.MenuAutoDownload))
			return TgEnumMenuDownload.AutoDownload;
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
			return TgEnumMenuDownload.ViewSources;
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
				case TgEnumMenuDownload.AutoDownload:
					await RunTaskStatusAsync(tgDownloadSettings, AutoDownloadAsync, 
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
					ViewVersions(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.ViewContacts:
					await ViewContactsAsync(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.ViewStories:
                    await ViewStoriesAsync(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.ViewSources:
					await ViewSourcesAsync(tgDownloadSettings);
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

	private async Task ViewSourcesAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await ShowTableViewSourcesAsync(tgDownloadSettings);
		var storageResult = await SourceRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
		var source = await GetSourceFromEnumerableAsync(TgLocale.MenuViewChats, storageResult.Items);
		if (source.Uid != Guid.Empty)
		{
			Value = TgEnumMenuMain.Download;
            tgDownloadSettings = await SetupDownloadSourceByIdAsync(source.Id);
			await SetupDownloadAsync(tgDownloadSettings);
		}
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

	private async void ViewVersions(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await ShowTableViewVersionsAsync(tgDownloadSettings);
		GetVersionFromEnumerable(TgLocale.MenuViewChats,
			(await VersionRepository.GetListAsync(TgEnumTableTopRecords.All, 0)).Items);
	}

	private async Task MarkHistoryReadAsync(TgDownloadSettingsViewModel tgDownloadSettings) => 
		await RunTaskStatusAsync(tgDownloadSettings, MarkHistoryReadCoreAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);

	private async Task AutoDownloadAsync(TgDownloadSettingsViewModel _)
	{
		var sources = (await SourceRepository.GetListAsync(TgEnumTableTopRecords.All, 0)).Items;
		foreach (var source in sources.Where(sourceSetting => sourceSetting.IsAutoUpdate))
		{
            var tgDownloadSettings = await SetupDownloadSourceByIdAsync(source.Id);
			var sourceId = string.IsNullOrEmpty(source.UserName) ? $"{source.Id}" : $"{source.Id} | @{source.UserName}";
            // StatusContext.
            await TgGlobalTools.ConnectClient.UpdateStateSourceAsync(source.Id, source.FirstId, source.Count, 
				source.Count <= 0
					? $"The source {sourceId} hasn't any messages!"
					: $"The source {sourceId} has {source.Count} messages.");
			// ManualDownload.
			if (source.Count > 0)
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