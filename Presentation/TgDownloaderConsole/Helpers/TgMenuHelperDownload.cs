﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace TgDownloaderConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	private TgEnumMenuDownload SetMenuDownload()
	{
		var prompt = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
				.Title($"  {TgLocale.MenuSwitchNumber}")
				.PageSize(Console.WindowHeight - 17)
				.MoreChoicesText(TgLocale.MoveUpDown)
				.AddChoices(
					TgLocale.MenuMainReturn,
					TgLocale.MenuDownloadSetSource,
					TgLocale.MenuDownloadSetFolder,
					TgLocale.MenuDownloadSetSourceFirstIdAuto,
					TgLocale.MenuDownloadSetSourceFirstIdManual,
					TgLocale.MenuDownloadSetIsRewriteFiles,
					TgLocale.MenuDownloadSetIsRewriteMessages,
					TgLocale.MenuDownloadSetIsAddMessageId,
					TgLocale.MenuDownloadSetIsAutoUpdate,
					TgLocale.MenuDownloadSetCountThreads,
					TgLocale.MenuSaveSettings,
					TgLocale.MenuManualDownload
				));
		if (prompt.Equals(TgLocale.MenuDownloadSetSource))
			return TgEnumMenuDownload.SetSource;
		if (prompt.Equals(TgLocale.MenuDownloadSetFolder))
			return TgEnumMenuDownload.SetDestDirectory;
		if (prompt.Equals(TgLocale.MenuDownloadSetSourceFirstIdAuto))
			return TgEnumMenuDownload.SetSourceFirstIdAuto;
		if (prompt.Equals(TgLocale.MenuDownloadSetSourceFirstIdManual))
			return TgEnumMenuDownload.SetSourceFirstIdManual;
		if (prompt.Equals(TgLocale.MenuDownloadSetIsRewriteFiles))
			return TgEnumMenuDownload.SetIsRewriteFiles;
		if (prompt.Equals(TgLocale.MenuDownloadSetIsRewriteMessages))
			return TgEnumMenuDownload.SetIsRewriteMessages;
		if (prompt.Equals(TgLocale.MenuDownloadSetIsAddMessageId))
			return TgEnumMenuDownload.SetIsAddMessageId;
		if (prompt.Equals(TgLocale.MenuDownloadSetIsAutoUpdate))
			return TgEnumMenuDownload.SetIsAutoUpdate;
		if (prompt.Equals(TgLocale.MenuDownloadSetCountThreads))
			return TgEnumMenuDownload.SetCountThreads;
		if (prompt.Equals(TgLocale.MenuSaveSettings))
			return TgEnumMenuDownload.SettingsSave;
		if (prompt.Equals(TgLocale.MenuManualDownload))
			return TgEnumMenuDownload.ManualDownload;
		return TgEnumMenuDownload.Return;
	}

	public async Task SetupDownloadAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuDownload menu;
		do
		{
			await ShowTableDownloadAsync(tgDownloadSettings);
			menu = SetMenuDownload();
			switch (menu)
			{
				case TgEnumMenuDownload.SetSource:
					tgDownloadSettings = await SetupDownloadSourceAsync();
					break;
				case TgEnumMenuDownload.SetSourceFirstIdAuto:
					await RunTaskStatusAsync(tgDownloadSettings, SetupDownloadSourceFirstIdAutoAsync, isSkipCheckTgSettings: true, 
						isScanCount: false, isWaitComplete: true);
					break;
				case TgEnumMenuDownload.SetSourceFirstIdManual:
					await SetupDownloadSourceFirstIdManualAsync(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.SetDestDirectory:
					SetupDownloadDestDirectory(tgDownloadSettings);
					if (!tgDownloadSettings.SourceVm.Dto.IsAutoUpdate)
						SetTgDownloadIsAutoUpdate(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.SetIsRewriteFiles:
					SetTgDownloadIsRewriteFiles(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.SetIsRewriteMessages:
					SetTgDownloadIsRewriteMessages(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.SetIsAddMessageId:
					SetTgDownloadIsJoinFileNameWithMessageId(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.SetIsAutoUpdate:
					SetTgDownloadIsAutoUpdate(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.SetCountThreads:
					await SetTgDownloadCountThreadsAsync(tgDownloadSettings);
					break;
				case TgEnumMenuDownload.SettingsSave:
					await RunTaskStatusAsync(tgDownloadSettings, UpdateSourceWithSettingsAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: false);
					break;
				case TgEnumMenuDownload.ManualDownload:
					await RunTaskProgressAsync(tgDownloadSettings, ManualDownloadAsync, isSkipCheckTgSettings: false, isScanCount: false);
					break;
			}
		} while (menu is not TgEnumMenuDownload.Return);
	}

	private async Task<TgDownloadSettingsViewModel> SetupDownloadSourceByIdAsync(long id)
	{
		var tgDownloadSettings = SetupDownloadSourceByIdCore(id);
		await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
		await TgGlobalTools.ConnectClient.CreateChatAsync(tgDownloadSettings, isSilent: true);
		return tgDownloadSettings;
	}

	private async Task<TgDownloadSettingsViewModel> SetupDownloadSourceAsync()
	{
		var tgDownloadSettings = SetupDownloadSourceCore();
		if (string.IsNullOrEmpty(tgDownloadSettings.SourceVm.Dto.UserName))
			await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
		else
			await LoadTgClientSettingsByNameAsync(tgDownloadSettings);
		await TgGlobalTools.ConnectClient.CreateChatAsync(tgDownloadSettings, isSilent: true);
		return tgDownloadSettings;
	}

	private TgDownloadSettingsViewModel SetupDownloadSourceByIdCore(long id)
	{
		TgDownloadSettingsViewModel tgDownloadSettings = new();
		tgDownloadSettings.SourceVm.Dto.Id = id;
		return tgDownloadSettings;
	}

	private TgDownloadSettingsViewModel SetupDownloadSourceCore()
	{
		TgDownloadSettingsViewModel tgDownloadSettings = new();
		var input = AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.MenuDownloadSetSource}:"));
		if (!string.IsNullOrEmpty(input))
		{
			if (long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sourceId))
				return SetupDownloadSourceByIdCore(sourceId);
			input = TgCommonUtils.ClearTgPeer(input);
			tgDownloadSettings.SourceVm.Dto.UserName = input;
			if (!string.IsNullOrEmpty(tgDownloadSettings.SourceVm.Dto.UserName))
				return tgDownloadSettings;
		}
		return tgDownloadSettings;
	}

	private async Task SetupDownloadSourceFirstIdAutoAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
		await TgGlobalTools.ConnectClient.CreateChatAsync(tgDownloadSettings, isSilent: true);
		await TgGlobalTools.ConnectClient.SetChannelMessageIdFirstAsync(tgDownloadSettings);
	}

	private async Task SetupDownloadSourceFirstIdManualAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
		do
		{
			tgDownloadSettings.SourceVm.Dto.FirstId = AnsiConsole.Ask<int>(TgLog.GetLineStampInfo($"{TgLocale.TypeTgSourceFirstId}:"));
		} while (!tgDownloadSettings.SourceVm.Dto.IsReadySourceFirstId);
	}

	private void SetupDownloadDestDirectory(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		do
		{
			tgDownloadSettings.SourceVm.Dto.Directory = AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.DirectoryDestType}:"));
			if (!Directory.Exists(tgDownloadSettings.SourceVm.Dto.Directory))
			{
				TgLog.MarkupInfo(TgLocale.DirectoryIsNotExists(tgDownloadSettings.SourceVm.Dto.Directory));
				if (AskQuestionReturnPositive(TgLocale.DirectoryCreate, true))
				{
					try
					{
						Directory.CreateDirectory(tgDownloadSettings.SourceVm.Dto.Directory);
					}
					catch (Exception ex)
					{
						TgLog.MarkupWarning(TgLocale.DirectoryCreateIsException(ex));
					}
				}
			}
		} while (!Directory.Exists(tgDownloadSettings.SourceVm.Dto.Directory));
	}

	private void SetTgDownloadIsRewriteFiles(TgDownloadSettingsViewModel tgDownloadSettings) =>
		tgDownloadSettings.IsRewriteFiles = AskQuestionReturnPositive(TgLocale.TgSettingsIsRewriteFiles, true);

	private void SetTgDownloadIsRewriteMessages(TgDownloadSettingsViewModel tgDownloadSettings) =>
		tgDownloadSettings.IsRewriteMessages = AskQuestionReturnPositive(TgLocale.TgSettingsIsRewriteMessages, true);

	private void SetTgDownloadIsJoinFileNameWithMessageId(TgDownloadSettingsViewModel tgDownloadSettings) =>
		tgDownloadSettings.IsJoinFileNameWithMessageId = AskQuestionReturnPositive(TgLocale.TgSettingsIsJoinFileNameWithMessageId, true);

	private void SetTgDownloadIsAutoUpdate(TgDownloadSettingsViewModel tgDownloadSettings) =>
		tgDownloadSettings.SourceVm.Dto.IsAutoUpdate = AskQuestionReturnPositive(TgLocale.MenuDownloadSetIsAutoUpdate, true);

	private async Task SetTgDownloadCountThreadsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
		tgDownloadSettings.CountThreads = AnsiConsole.Ask<int>(TgLog.GetLineStampInfo($"{TgLocale.MenuDownloadSetCountThreads}:"));
		if (tgDownloadSettings.CountThreads < 1)
			tgDownloadSettings.CountThreads = 1;
		else if (tgDownloadSettings.CountThreads > 100)
			tgDownloadSettings.CountThreads = 100;
	}

	private async Task UpdateSourceWithSettingsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await tgDownloadSettings.UpdateSourceWithSettingsAsync();
		await TgGlobalTools.ConnectClient.UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceVm.Dto.FirstId, tgDownloadSettings.SourceVm.Dto.Count, 
			TgLocale.SettingsChat);
	}

	private async Task LoadTgClientSettingsByIdAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		var directory = tgDownloadSettings.SourceVm.Dto.Directory;
		// Find by ID
		var storageResult = await SourceRepository.GetAsync(new() { Id = tgDownloadSettings.SourceVm.Dto.Id });
		if (storageResult.IsExists)
		{
			if (string.IsNullOrEmpty(directory))
				directory = storageResult.Item.Directory;
			tgDownloadSettings.SourceVm.Dto = new TgEfSourceDto().Copy(storageResult.Item, isUidCopy: true);
		}
		// Restore directory
		if (!string.IsNullOrEmpty(directory) && string.IsNullOrEmpty(tgDownloadSettings.SourceVm.Dto.Directory))
			tgDownloadSettings.SourceVm.Dto.Directory = directory;
	}

	private async Task LoadTgClientSettingsByNameAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		var directory = tgDownloadSettings.SourceVm.Dto.Directory;
		// Find by UserName
		var storageResult = await SourceRepository.GetAsync(new() { UserName = tgDownloadSettings.SourceVm.Dto.UserName });
		if (storageResult.IsExists)
		{
			if (string.IsNullOrEmpty(directory))
				directory = storageResult.Item.Directory;
			tgDownloadSettings.SourceVm.Dto = new TgEfSourceDto().Copy(storageResult.Item, isUidCopy: true);
		}
		// Restore directory
		if (!string.IsNullOrEmpty(directory) && string.IsNullOrEmpty(tgDownloadSettings.SourceVm.Dto.Directory))
			tgDownloadSettings.SourceVm.Dto.Directory = directory;
	}

	private async Task ManualDownloadAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await ShowTableDownloadAsync(tgDownloadSettings);
		await UpdateSourceWithSettingsAsync(tgDownloadSettings);
		try
		{
			await TgGlobalTools.ConnectClient.DownloadAllDataAsync(tgDownloadSettings);
		}
		catch (Exception ex)
		{
			TgLog.MarkupWarning(ex.Message);
			var floodWait = TgGlobalTools.ConnectClient.Client?.FloodRetryThreshold ?? 60;
			TgLog.MarkupWarning($"Flood control: waiting {floodWait} seconds");
			await Task.Delay(floodWait * 1_000);
			// Repeat request after waiting
			await ManualDownloadAsync(tgDownloadSettings);
		}
		await UpdateSourceWithSettingsAsync(tgDownloadSettings);
	}

	private async Task MarkHistoryReadCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await ShowTableMarkHistoryReadProgressAsync(tgDownloadSettings);
		await TgGlobalTools.ConnectClient.MarkHistoryReadAsync();
		await ShowTableMarkHistoryReadCompleteAsync(tgDownloadSettings);
	}

	#endregion
}