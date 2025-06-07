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
		AnsiConsole.Progress()
			.AutoRefresh(false)
			.AutoClear(true)
			.HideCompleted(true)
			.Columns(GetProgressColumns())
			.Start(progressContext =>
			{
				var task = RunTaskProgressCoreAsync(progressContext, tgDownloadSettings, func, isScanCount);
				task.Wait();
			});
		TgLog.MarkupLine(TgLocale.WaitDownloadCompleteWithQuit);
		while (!tgDownloadSettings.SourceVm.Dto.IsComplete)
		{
			var key = Console.ReadKey();
			if (key.KeyChar == 'q' || key.KeyChar == 'Q')
				break;
			TgLog.MarkupLine(TgLocale.WaitDownloadCompleteWithQuit);
		}
	}

	private async Task RunTaskProgressCoreAsync(ProgressContext progressContext, TgDownloadSettingsViewModel tgDownloadSettings,
		Func<TgDownloadSettingsViewModel, Task> func, bool isScanCount)
	{
		var swChat = Stopwatch.StartNew();
		//var swMessage = Stopwatch.StartNew();
		var stringLimit = Console.WindowWidth / 2 - 10;
		//var progressTaskDefaultName = TgDataFormatUtils.GetFormatStringWithStrongLength("Starting reading the message", stringLimit).TrimEnd();
		var progressSourceDefaultName = TgDataFormatUtils.GetFormatStringWithStrongLength("Starting reading the source", stringLimit).TrimEnd();
		// progressMessages
		var progressMessages = new List<ProgressTask>();
		// progressSource
		var progressSource = progressContext.AddTask(progressSourceDefaultName, autoStart: true, maxValue: tgDownloadSettings.SourceVm.Dto.Count);
		progressSource.Value = 0;
		// Setup
		BusinessLogicManager.ConnectClient.SetupUpdateTitle(UpdateConsoleTitleAsync);
		BusinessLogicManager.ConnectClient.SetupUpdateStateSource(UpdateStateSourceAsync);
		BusinessLogicManager.ConnectClient.SetupUpdateStateFile(UpdateStateFileAsync);
		BusinessLogicManager.ConnectClient.SetupUpdateStateMessageThread(UpdateStateMessageThreadAsync);
		// Task
		await func(tgDownloadSettings);
		// Finish
		swChat.Stop();
		//swMessage.Stop();
		var progressMessagesStarted = progressMessages.Where(x => x.IsStarted).ToList();
		foreach (var progress in progressMessagesStarted)
			progress.StopTask();
		if (progressSource.IsStarted)
			progressSource.StopTask();
		var messageFinally = isScanCount
			? $"{GetStatus(swChat, tgDownloadSettings.SourceScanCount, tgDownloadSettings.SourceScanCurrent)}"
			: $"{GetStatus(swChat, tgDownloadSettings.SourceVm.Dto.FirstId, tgDownloadSettings.SourceVm.Dto.Count)}";
		await UpdateStateSourceAsync(0, 0, 0, messageFinally);
		return;

		// Update console title
		async Task UpdateConsoleTitleAsync(string title)
		{
			Console.Title = string.IsNullOrEmpty(title) ? $"{TgConstants.OTR}" : $"{TgConstants.OTR} {title}";
			await Task.CompletedTask;
		}
		// Update source
		async Task UpdateStateSourceAsync(long sourceId, int messageId, int count, string message)
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
				// State
				//if (!swMessage.IsRunning)
				//	swMessage.Start();
				//else if (swMessage.Elapsed > TimeSpan.FromMilliseconds(1_000))
				//{
				//	swMessage.Reset();
				//	progressContext.Refresh();
				//}
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
				// State
				//if (!swMessage.IsRunning)
				//	swMessage.Start();
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
		bool isSkipCheckTgSettings, bool isScanCount, bool isWaitComplete)
	{
		if (!isSkipCheckTgSettings && !await CheckTgSettingsWithWarningAsync(tgDownloadSettings))
			return;
		AnsiConsole.Status()
			.AutoRefresh(false)
			.Spinner(Spinner.Known.Star)
			.SpinnerStyle(Style.Parse("green"))
			.Start("Thinking...", statusContext =>
			{
				var task = RunTaskStatusCoreAsync(statusContext, tgDownloadSettings, func, isScanCount);
				task.Wait();
			});
		TgLog.MarkupLine(TgLocale.WaitDownloadComplete);
		while (isWaitComplete && !tgDownloadSettings.SourceVm.Dto.IsComplete)
		{
			Console.ReadKey();
			TgLog.MarkupLine(TgLocale.WaitDownloadComplete);
		}
	}

	private async Task RunTaskStatusCoreAsync(StatusContext statusContext, TgDownloadSettingsViewModel tgDownloadSettings,
		Func<TgDownloadSettingsViewModel, Task> func, bool isScanCount)
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
		BusinessLogicManager.ConnectClient.SetupUpdateStateSource(UpdateStateSourceAsync);
		BusinessLogicManager.ConnectClient.SetupUpdateStateContact(UpdateStateContactAsync);
		BusinessLogicManager.ConnectClient.SetupUpdateStateStory(UpdateStateStoryAsync);
		BusinessLogicManager.ConnectClient.SetupUpdateStateFile(UpdateStateFileAsync);
		BusinessLogicManager.ConnectClient.SetupUpdateStateMessage(UpdateStateMessageAsync);
		// Task
		var sw = Stopwatch.StartNew();
		await func(tgDownloadSettings);
		sw.Stop();
		// Update console title
		async Task UpdateConsoleTitleAsync(string title)
		{
			Console.Title = string.IsNullOrEmpty(title) ? $"{TgConstants.OTR}" : $"{TgConstants.OTR} {title}";
			await Task.CompletedTask;
		}
		// Update source
		async Task UpdateStateSourceAsync(long sourceId, int messageId, int count, string message)
		{
			if (string.IsNullOrEmpty(message))
				return;
			statusContext.Status(TgLog.GetMarkupString(isScanCount
				? $"{GetStatus(tgDownloadSettings.SourceScanCount, messageId),15} | {message,40}"
				: GetFileStatus(message)));
			statusContext.Refresh();
			await Task.CompletedTask;
		}
		// Update state source
		await UpdateStateSourceAsync(0, 0, 0, isScanCount
			? $"{GetStatus(sw, tgDownloadSettings.SourceScanCount, tgDownloadSettings.SourceScanCurrent)}"
			: $"{GetStatus(sw, tgDownloadSettings.SourceVm.Dto.FirstId, tgDownloadSettings.SourceVm.Dto.Count)}");
		return;

		// Update contact
		async Task UpdateStateContactAsync(long id, string firstName, string lastName, string userName)
		{
			statusContext.Status(TgLog.GetMarkupString(
				$"{GetStatus(tgDownloadSettings.SourceScanCount, id),15} | {firstName,20} | {lastName,20} | {userName,20}"));
			statusContext.Refresh();
			await Task.CompletedTask;
		}
		// Update story
		async Task UpdateStateStoryAsync(long id, int messageId, int count, string caption)
		{
			statusContext.Status(TgLog.GetMarkupString(
				$"{GetStatus(tgDownloadSettings.SourceScanCount, id),15} | {caption,30}"));
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
	}


	private static string GetStatus(Stopwatch sw, long count, long current) =>
		count is 0 && current is 0
			? $"{TgLog.GetDtShortStamp()} | {sw.Elapsed} | "
			: $"{TgLog.GetDtShortStamp()} | {sw.Elapsed} | {TgDataUtils.CalcSourceProgress(count, current):#00.00} % | {TgDataUtils.GetLongString(current)} / {TgDataUtils.GetLongString(count)}";

	private static string GetStatus(long count, long current) =>
		count is 0 && current is 0
			? TgLog.GetDtShortStamp()
			: $"{TgLog.GetDtShortStamp()} | {TgDataUtils.CalcSourceProgress(count, current):#00.00} % | {TgDataUtils.GetLongString(current)} / {TgDataUtils.GetLongString(count)}";

#endregion
}