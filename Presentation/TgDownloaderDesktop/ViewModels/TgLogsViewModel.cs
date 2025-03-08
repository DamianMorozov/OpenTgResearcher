// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using Telegram.Bot.Types;
using Windows.Storage;

namespace TgDownloaderDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgLogsViewModel : TgPageViewModelBase, ITgLogsViewModel
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial ObservableCollection<TgLogFile> LogFiles { get; private set; } = new();
	public IRelayCommand LoadLogsCommand { get; }
	public IRelayCommand DeleteLogFileCommand { get; }

	public TgLogsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgLogsViewModel> logger)
		: base(settingsService, navigationService, logger)
	{
		// Commands
		LoadLogsCommand = new AsyncRelayCommand(LoadLogsAsync);
		DeleteLogFileCommand = new AsyncRelayCommand<TgLogFile>(DeleteLogFileAsync);
	}

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await LoadDataAsync(async () =>
	{
		await LoadLogsCoreAsync();
		await ReloadUiAsync();
	});

	private async Task LoadLogsAsync() => await LoadLogsCoreAsync();

	private async Task LoadLogsCoreAsync()
	{
		try
		{
			LogFiles.Clear();
			var appFolder = SettingsService.AppFolder;
			var storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.Combine(appFolder, TgFileUtils.LogsDirectory));
			var storageFiles = await storageFolder.GetFilesAsync();
			foreach (var logFile in storageFiles)
			{
				if (logFile is null) continue;
				try
				{
					var content = await File.ReadAllTextAsync(logFile.Path);
					LogFiles.Add(new(this, Path.GetFileName(logFile.Path), content));
				}
				catch (Exception ex)
				{
					TgLogUtils.LogFatal(ex, TgResourceExtensions.GetErrorOccurredWhileLoadingLogs());
					LogFiles.Add(new(this, Path.GetFileName(logFile.Path), TgResourceExtensions.GetErrorOccurredWhileLoadingLogs()));
				}
			}
		}
		catch (Exception ex)
		{
			TgLogUtils.LogFatal(ex, TgResourceExtensions.GetErrorOccurredWhileLoadingLogs());
		}
	}

	private async Task DeleteLogFileAsync(TgLogFile? logFile)
	{
		if (XamlRootVm is null)
			return;
		ContentDialog dialog = new()
		{
			XamlRoot = XamlRootVm,
			Title = TgResourceExtensions.AskDeleteFile(),
			PrimaryButtonText = TgResourceExtensions.GetYesButton(),
			CloseButtonText = TgResourceExtensions.GetCancelButton(),
			DefaultButton = ContentDialogButton.Close,
		};
		dialog.PrimaryButtonClick += async (sender, args) =>
		{
			var deferral = args.GetDeferral();
			await DeleteLogFileCoreAsync(logFile);
			deferral.Complete();
		};
		_ = await dialog.ShowAsync();
	}

	private async Task DeleteLogFileCoreAsync(TgLogFile? logFile)
	{
		if (logFile is null) return;
		var appFolder = SettingsService.AppFolder;
		var storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.Combine(appFolder, TgFileUtils.LogsDirectory));
		var storageFile = await storageFolder.GetFileAsync(logFile.FileName);
		if (storageFile.IsAvailable)
		{
			await storageFile.DeleteAsync();
			LogFiles.Remove(logFile);
		}

		await LoadLogsCoreAsync();
	}

	#endregion
}