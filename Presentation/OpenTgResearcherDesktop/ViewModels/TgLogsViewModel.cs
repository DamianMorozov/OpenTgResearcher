namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgLogsViewModel : TgPageViewModelBase, ITgLogsViewModel
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial ObservableCollection<TgLogFile> LogFiles { get; private set; } = [];
    public IAsyncRelayCommand DeleteLogFileCommand { get; }

	public TgLogsViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgLogsViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgLogsViewModel))
	{
		// Commands
		DeleteLogFileCommand = new AsyncRelayCommand<TgLogFile>(DeleteLogFileAsync);
	}

	#endregion

	#region Methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
	{
		await LoadLogsCoreAsync();
		await ReloadUiAsync();
	});

	private async Task LoadLogsCoreAsync()
	{
		try
		{
            LogFiles.Clear();
            if (!Directory.Exists(TgLogUtils.GetLogsDirectory(TgEnumAppType.Desktop))) return;

            var files = Directory.GetFiles(TgLogUtils.GetLogsDirectory(TgEnumAppType.Desktop))
                .Where(x =>
                {
                    var fileName = Path.GetFileName(x);
                    return fileName.StartsWith("Log-", StringComparison.OrdinalIgnoreCase) && fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
                });
            foreach (var logFile in files)
            {
                if (!string.IsNullOrEmpty(logFile))
                    await TryLoadLogAsync(logFile);
            }
		}
		catch (Exception ex)
		{
            LogError(ex, TgResourceExtensions.GetErrorOccurredWhileLoadingLogs());
		}
	}

	private async Task TryLoadLogAsync(string logFile, bool isRetry = false, int attempt = 0)
	{
		const int maxAttempts = 5;
		try
		{
			if (!isRetry)
			{
				var content = await File.ReadAllTextAsync(logFile);
				LogFiles.Add(new(this, Path.GetFileName(logFile), content));
			}
			else
			{
				await using var fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var reader = new StreamReader(fileStream);
				var content = await reader.ReadToEndAsync();
				LogFiles.Add(new(this, Path.GetFileName(logFile), content));
			}
		}
		catch (Exception ex)
		{
			if (attempt < maxAttempts)
			{
				int delay = 100 * (attempt + 1);
				await Task.Delay(delay);
				await TryLoadLogAsync(logFile, isRetry: true, attempt + 1);
			}
			else
			{
                LogError(ex, TgResourceExtensions.GetErrorOccurredWhileLoadingLogs());
				LogFiles.Add(new(this, Path.GetFileName(logFile), TgResourceExtensions.GetErrorOccurredWhileLoadingLogs()));
			}
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
            // Wait async operations
			var deferral = args.GetDeferral();
			await DeleteLogFileCoreAsync(logFile);
            await LoadLogsCoreAsync();
            // Complete async operations
            deferral.Complete();
		};
		_ = await dialog.ShowAsync();
	}

	private async Task DeleteLogFileCoreAsync(TgLogFile? logFile)
	{
		if (logFile is null) return;
		// Close logger
		await TgLogUtils.CloseAndFlushAsync();

        if (!Directory.Exists(TgLogUtils.GetLogsDirectory(TgEnumAppType.Desktop))) return;
        var files = Directory.GetFiles(TgLogUtils.GetLogsDirectory(TgEnumAppType.Desktop));
        var file = files.FirstOrDefault(x => x.EndsWith(logFile.FileName));
        if (file is not null)
        {
		    if (File.Exists(file))
		    {
			    File.Delete(file);
			    LogFiles.Remove(logFile);
		    }
        }
	}

	#endregion
}
