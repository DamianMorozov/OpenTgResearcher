// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	public async Task<bool> CheckTgSettingsWithWarningAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		var result = BusinessLogicManager.ConnectClient is { IsReady: true } && tgDownloadSettings.SourceVm.Dto.IsReady;
		if (!result)
		{
			await ClientConnectAsync(tgDownloadSettings, isSilent: true);
			result = BusinessLogicManager.ConnectClient is { IsReady: true } && tgDownloadSettings.SourceVm.Dto.IsReady;
			if (!result)
			{
				TgLog.MarkupWarning(TgLocale.TgMustSetSettings);
				Console.ReadKey();
			}
		}
		return result;
	}

	private static ProgressColumn[] GetProgressColumns() =>
	[
		new TaskDescriptionColumn { Alignment = Justify.Left },
		new ProgressBarColumn { Width = 25 },
		new PercentageColumn(),
		new SpinnerColumn(),
		new DownloadedColumn { Culture = CultureInfo.InvariantCulture },
		new TransferSpeedColumn { Culture = CultureInfo.InvariantCulture }
	];

	public async Task RunTaskProgressAsync(TgDownloadSettingsViewModel tgDownloadSettings, Func<TgDownloadSettingsViewModel, Task> func,
		bool isSkipCheckTgSettings, bool isScanCount)
	{
		if (!isSkipCheckTgSettings && !await CheckTgSettingsWithWarningAsync(tgDownloadSettings))
			return;
		await AnsiConsole.Progress()
			.AutoRefresh(false)
			.AutoClear(true)
			.HideCompleted(true)
			.Columns(GetProgressColumns())
			.StartAsync(async progressContext => await RunTaskProgressCoreAsync(progressContext, tgDownloadSettings, func, isScanCount));
		TgLog.MarkupLine(TgLocale.WaitDownloadCompleteWithQuit);
		while (!tgDownloadSettings.SourceVm.Dto.IsComplete)
		{
			var key = Console.ReadKey();
			if (key.KeyChar == 'q' || key.KeyChar == 'Q') break;
			TgLog.MarkupLine(TgLocale.WaitDownloadCompleteWithQuit);
		}
	}

	private async Task RunTaskProgressCoreAsync(ProgressContext progressContext, TgDownloadSettingsViewModel tgDownloadSettings,
		Func<TgDownloadSettingsViewModel, Task> func, bool isScanCount)
	{
		var stringLimit = Console.WindowWidth / 2 - 10;
		var progressSourceDefaultName = TgDataFormatUtils.GetFormatStringWithStrongLength("Starting reading the source", stringLimit).TrimEnd();
		// progressMessages
		var progressMessages = new List<ProgressTask>();
		// progressSource
		var progressSource = progressContext.AddTask(progressSourceDefaultName, autoStart: true, maxValue: tgDownloadSettings.SourceVm.Dto.Count);
		progressSource.Value = 0;
		// Setup
		BusinessLogicManager.ConnectClient.SetupUpdateTitle(UpdateConsoleTitleAsync);
        BusinessLogicManager.ConnectClient.SetupUpdateChatViewModel(UpdateChatViewModelAsync);
        BusinessLogicManager.ConnectClient.SetupUpdateStateFile(UpdateStateFileAsync);
		BusinessLogicManager.ConnectClient.SetupUpdateStateMessageThread(UpdateStateMessageThreadAsync);
		// Task
		await func(tgDownloadSettings);
		//swMessage.Stop();
		var progressMessagesStarted = progressMessages.Where(x => x.IsStarted).ToList();
		foreach (var progress in progressMessagesStarted)
			progress.StopTask();
		if (progressSource.IsStarted)
			progressSource.StopTask();
		var messageFinally = isScanCount
			? $"{GetStatus(tgDownloadSettings.SourceScanCount, tgDownloadSettings.SourceScanCurrent)}"
			: $"{GetStatus(tgDownloadSettings.SourceVm.Dto.FirstId, tgDownloadSettings.SourceVm.Dto.Count)}";
        await UpdateChatViewModelAsync(0, 0, 0, messageFinally);
        return;

		// Update console title
		async Task UpdateConsoleTitleAsync(string title)
		{
			Console.Title = string.IsNullOrEmpty(title) ? $"{TgConstants.OTR}" : $"{TgConstants.OTR} {title}";
			await Task.CompletedTask;
		}
        // Update source
        async Task UpdateChatViewModelAsync(long sourceId, int messageId, int count, string message)
        {
            progressSource.Description = TgDataFormatUtils.GetFormatStringWithStrongLength(message, stringLimit).TrimEnd();
            if (tgDownloadSettings.SourceVm.Dto.Id.Equals(sourceId))
            {
                if (messageId > 0)
                    progressSource.Value = messageId;
            }
            progressContext.Refresh();
            await Task.CompletedTask;
        }
        // Update file
        async Task UpdateStateFileAsync(long sourceId, int messageId, string fileName, long fileSize, long transmitted, long fileSpeed,
			bool isStartTask, int threadNumber)
		{
			if (!tgDownloadSettings.SourceVm.Dto.Id.Equals(sourceId)) return;
			try
			{
				//var progressDescription = $"Thread {(threadNumber + 1):00}: Message {messageId} {TgDataFormatUtils.GetFormatStringWithStrongLength(fileName, stringLimit).TrimEnd()}";
				var progressDescription = $"Message {messageId}: {TgDataFormatUtils.GetFormatStringWithStrongLength(fileName, stringLimit).TrimEnd()}";
				var progress = progressMessages.FirstOrDefault(x => x.Description == progressDescription);
				if (progress is null)
				{
					progress = progressContext.AddTask(progressDescription, autoStart: true, maxValue: 100);
					progressMessages.Add(progress);
					progress.Value = 0;
				}
				// Start
				if (isStartTask)
				{
					progress.Value = transmitted;
					if (!progress.MaxValue.Equals(fileSize))
						progress.MaxValue = fileSize;
					tgDownloadSettings.SourceVm.Dto.FirstId = messageId;
					tgDownloadSettings.SourceVm.Dto.CurrentFileName = fileName;
					tgDownloadSettings.SourceVm.Dto.CurrentFileSize = fileSize;
					tgDownloadSettings.SourceVm.Dto.CurrentFileTransmitted = transmitted;
					tgDownloadSettings.SourceVm.Dto.CurrentFileSpeed = fileSpeed;
				}
				// Stop
				else
				{
					progress.Value = fileSize;
					progress.StopTask();
					tgDownloadSettings.SourceVm.Dto.CurrentFileName = string.Empty;
					tgDownloadSettings.SourceVm.Dto.CurrentFileSize = 0;
					tgDownloadSettings.SourceVm.Dto.CurrentFileTransmitted = 0;
					tgDownloadSettings.SourceVm.Dto.CurrentFileSpeed = 0;
				}
				progressContext.Refresh();
				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
                TgDebugUtils.WriteExceptionToDebug(ex);
            }
		}
		// Update message
		async Task UpdateStateMessageThreadAsync(long sourceId, int messageId, string message, bool isStartTask, int threadNumber)
		{
			if (!tgDownloadSettings.SourceVm.Dto.Id.Equals(sourceId)) return;
			try
			{
				//var progressDescription = $"Thread {(threadNumber + 1):00}: Message {messageId} {TgDataFormatUtils.GetFormatStringWithStrongLength(message, stringLimit).TrimEnd()}";
				var progressDescription = $"Message {messageId}: {TgDataFormatUtils.GetFormatStringWithStrongLength(message, stringLimit).TrimEnd()}";
				var progress = progressMessages.FirstOrDefault(x => x.Description == progressDescription);
				if (progress is null)
				{
					progress = progressContext.AddTask(progressDescription, autoStart: true, maxValue: 100);
					progressMessages.Add(progress);
					progress.Value = 0;
				}
				// Start
				if (isStartTask)
				{
					tgDownloadSettings.SourceVm.Dto.FirstId = messageId;
				}
				// Stop
				else
				{
					progress.Value = 100;
					progress.StopTask();
				}
				progressContext.Refresh();
				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
                TgDebugUtils.WriteExceptionToDebug(ex);
            }
		}
	}

	public async Task RunTaskStatusAsync(TgDownloadSettingsViewModel tgDownloadSettings, Func<TgDownloadSettingsViewModel, Task> func,
		bool isSkipCheckTgSettings, bool isScanCount, bool isWaitComplete, bool isUpdateChatViewModel = true)
	{
		if (!isSkipCheckTgSettings && !await CheckTgSettingsWithWarningAsync(tgDownloadSettings))
			return;
		await AnsiConsole.Status()
			.AutoRefresh(false)
			.Spinner(Spinner.Known.Star)
			.SpinnerStyle(Style.Parse("green"))
			.StartAsync("Thinking...", statusContext => RunTaskStatusCoreAsync(statusContext, tgDownloadSettings, func, isScanCount, isUpdateChatViewModel));
		while (isWaitComplete && !tgDownloadSettings.SourceVm.Dto.IsComplete)
		{
			Console.ReadKey();
			TgLog.WriteLine($"  {TgLocale.WaitDownloadComplete}");
		}
	}

	private async Task RunTaskStatusCoreAsync(StatusContext statusContext, TgDownloadSettingsViewModel tgDownloadSettings,
		Func<TgDownloadSettingsViewModel, Task> func, bool isScanCount, bool isUpdateChatViewModel)
	{
		statusContext.Spinner(Spinner.Known.Star);
		statusContext.SpinnerStyle(Style.Parse("green"));
		string GetFileStatus(string message = "") =>
			string.IsNullOrEmpty(message)
				? $"{GetStatus(tgDownloadSettings.SourceVm.Dto.Count, tgDownloadSettings.SourceVm.Dto.FirstId)} | " +
				  $"Progress {tgDownloadSettings.SourceVm.Dto.ProgressPercentString}"
				: $"{GetStatus(tgDownloadSettings.SourceVm.Dto.Count, tgDownloadSettings.SourceVm.Dto.FirstId)} | {message} | " +
				  $"Progress {tgDownloadSettings.SourceVm.Dto.ProgressPercentString}";
		// Setup
		BusinessLogicManager.ConnectClient.SetupUpdateTitle(UpdateConsoleTitleAsync);
		BusinessLogicManager.ConnectClient.SetupUpdateStateContact(UpdateStateContactAsync);
		BusinessLogicManager.ConnectClient.SetupUpdateStateFile(UpdateStateFileAsync);
		BusinessLogicManager.ConnectClient.SetupUpdateStateMessage(UpdateStateMessageAsync);
        if (isUpdateChatViewModel)
            BusinessLogicManager.ConnectClient.SetupUpdateChatViewModel(UpdateChatViewModelAsync);
		BusinessLogicManager.ConnectClient.SetupUpdateChatsViewModel(UpdateChatsViewModelAsync);
		BusinessLogicManager.ConnectClient.SetupUpdateShellViewModel(UpdateShellViewModelAsync);
        // Task
    	await func(tgDownloadSettings);
        return;

        // Update console title
        async Task UpdateConsoleTitleAsync(string title)
		{
			Console.Title = string.IsNullOrEmpty(title) ? $"{TgConstants.OTR}" : $"{TgConstants.OTR} {title}";
			await Task.CompletedTask;
		}
        // Update source
        async Task UpdateChatViewModelAsync(long sourceId, int messageId, int count, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            statusContext.Status(TgLog.GetMarkupString(isScanCount
                ? $"{GetStatus(tgDownloadSettings.SourceScanCount, messageId),15} | {message,40}"
                : GetFileStatus(message)));
            statusContext.Refresh();
            await Task.CompletedTask;
        }
        // Update chat ViewModel
        //await UpdateChatViewModelAsync(0, 0, 0, isScanCount
        //    ? $"{GetStatus(tgDownloadSettings.SourceScanCount, tgDownloadSettings.SourceScanCurrent)}"
        //    : $"{GetStatus(tgDownloadSettings.SourceVm.Dto.FirstId, tgDownloadSettings.SourceVm.Dto.Count)}");

		// Update contact
		async Task UpdateStateContactAsync(long id, string firstName, string lastName, string userName)
		{
			statusContext.Status(TgLog.GetMarkupString(
				$"{GetStatus(tgDownloadSettings.SourceScanCount, id),15} | {firstName,20} | {lastName,20} | {userName,20}"));
			statusContext.Refresh();
			await Task.CompletedTask;
		}
		// Update message
		async Task UpdateStateMessageAsync(string message)
		{
			if (string.IsNullOrEmpty(message))
				return;
			statusContext.Status(TgLog.GetMarkupString(message));
			statusContext.Refresh();
			await Task.CompletedTask;
		}
		// Update download file state
		async Task UpdateStateFileAsync(long sourceId, int messageId, string fileName, long fileSize, long transmitted, long fileSpeed,
			bool isFileNewDownload, int threadNumber)
		{
			// Download job
			if (!string.IsNullOrEmpty(fileName) && !isFileNewDownload && tgDownloadSettings.SourceVm.Dto.Id.Equals(sourceId))
			{
				// Download status job
				tgDownloadSettings.SourceVm.Dto.FirstId = messageId;
				tgDownloadSettings.SourceVm.Dto.CurrentFileName = fileName;
				tgDownloadSettings.SourceVm.Dto.CurrentFileSize = fileSize;
				tgDownloadSettings.SourceVm.Dto.CurrentFileTransmitted = transmitted;
				tgDownloadSettings.SourceVm.Dto.CurrentFileSpeed = fileSpeed;
			}
			// Download reset
			else
			{
				// Download status reset
				tgDownloadSettings.SourceVm.Dto.FirstId = messageId;
				tgDownloadSettings.SourceVm.Dto.CurrentFileName = string.Empty;
				tgDownloadSettings.SourceVm.Dto.CurrentFileSize = 0;
				tgDownloadSettings.SourceVm.Dto.CurrentFileTransmitted = 0;
				tgDownloadSettings.SourceVm.Dto.CurrentFileSpeed = 0;
			}
			// State
			statusContext.Status(TgLog.GetMarkupString($"{GetFileStatus()} | " +
				$"File {fileName} | " +
				$"Transmitted {tgDownloadSettings.SourceVm.Dto.CurrentFileProgressPercentString} | Speed " +
				$"{tgDownloadSettings.SourceVm.Dto.CurrentFileSpeedKBString}"));
			statusContext.Refresh();
			await Task.CompletedTask;
		}
        // Update chats ViewModel
        async Task UpdateChatsViewModelAsync(int counter, int countAll, TgEnumChatsMessageType chatsMessageType)
        {
            switch (chatsMessageType)
            {
                case TgEnumChatsMessageType.StartScan:
                    statusContext.Status(TgLog.GetMarkupString($" Start parsing"));
                    break;
                case TgEnumChatsMessageType.ProcessingChats:
                    statusContext.Status(TgLog.GetMarkupString($" Process parsing chats: {counter} from {countAll}"));
                    break;
                case TgEnumChatsMessageType.ProcessingDialogs:
                    statusContext.Status(TgLog.GetMarkupString($" Process parsing dialogs: {counter} from {countAll}"));
                    break;
                case TgEnumChatsMessageType.ProcessingGroups:
                    statusContext.Status(TgLog.GetMarkupString($" Process parsing groups: {counter} from {countAll}"));
                    break;
                case TgEnumChatsMessageType.ProcessingStories:
                    statusContext.Status(TgLog.GetMarkupString($" Process parsing stories: {counter} from {countAll}"));
                    break;
                case TgEnumChatsMessageType.ProcessingUsers:
                    statusContext.Status(TgLog.GetMarkupString($" Process parsing users: {counter} from {countAll}"));
                    break;
                case TgEnumChatsMessageType.StopScan:
                    statusContext.Status(TgLog.GetMarkupString($" Stop parsing"));
                    break;
                default:
                    break;
            }
            statusContext.Refresh();
            await Task.CompletedTask;
        }
        // Update shell ViewModel
        async Task UpdateShellViewModelAsync(bool isFloodVisible, int seconds, string message)
        {
            if (isFloodVisible)
            {
                if (!string.IsNullOrEmpty(message))
                    statusContext.Status(TgLog.GetMarkupString($" Flood control: wait {seconds} seconds | {message}"));
                else
                    statusContext.Status(TgLog.GetMarkupString($" Flood control: wait {seconds} seconds"));
            }
            else
            {
                if (!string.IsNullOrEmpty(message))
                    statusContext.Status(TgLog.GetMarkupString(message));
            }
            statusContext.Refresh();
            await Task.CompletedTask;
        }
	}

	private static string GetStatus(long count, long current) =>
		count is 0 && current is 0
			? TgLog.GetDtShortStamp()
			: $"{TgLog.GetDtShortStamp()} | {TgDataUtils.CalcSourceProgress(count, current):#00.00} % | {TgDataUtils.GetLongString(current)} / {TgDataUtils.GetLongString(count)}";

#endregion
}