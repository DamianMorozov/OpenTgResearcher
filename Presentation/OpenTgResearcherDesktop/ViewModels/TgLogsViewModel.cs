namespace OpenTgResearcherDesktop.ViewModels;

public partial class TgLogsViewModel : TgPageViewModelBase, ITgLogsViewModel
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial ObservableCollection<TgLogFile> LogFiles { get; private set; } = [];
    [ObservableProperty]
    public partial string LogDirectory { get; private set; } = string.Empty;
    
    public IAsyncRelayCommand DeleteLogFileCommand { get; }

	public TgLogsViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgLogsViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgLogsViewModel))
	{
		// Команды
		DeleteLogFileCommand = new AsyncRelayCommand<TgLogFile>(DeleteLogFileAsync);
	}

	#endregion

	#region Methods

	/// <summary> Загружает журналы при переходе на страницу </summary>
	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
	{
		await LoadLogsCoreAsync();
		await ReloadUiAsync();
	});

	// Загружаем файлы журнала из каталога приложения
	private async Task LoadLogsCoreAsync(CancellationToken ct = default)
	{
		try
		{
            LogFiles.Clear();
            LogDirectory = TgLogUtils.GetLogsDirectory(TgEnumAppType.Desktop);
            if (!Directory.Exists(LogDirectory)) return;

            var files = Directory.GetFiles(LogDirectory)
                .Where(x =>
                {
                    var fileName = Path.GetFileName(x);
                    return fileName.StartsWith("Log-", StringComparison.OrdinalIgnoreCase) && fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
                });
            foreach (var logFile in files)
            {
                if (!string.IsNullOrEmpty(logFile))
                    await TryLoadLogAsync(logFile, ct: ct);
            }
		}
		catch (Exception ex)
		{
            LogError(ex, TgResourceExtensions.GetErrorOccurredWhileLoadingLogs());
		}
	}

	// Читаем журнал, разрешая приложению продолжать запись в открытый файл
	private async Task TryLoadLogAsync(string logFile, int attempt = 0, CancellationToken ct = default)
	{
		const int maxAttempts = 5;
		try
		{
			await using var fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			using var reader = new StreamReader(fileStream);
			var content = await reader.ReadToEndAsync(ct);
            var fileSize = TgFileUtils.GetFileSizeAsString(fileStream.Length);
            LogFiles.Add(new(this, Path.GetFileName(logFile), fileSize, content));
		}
		catch (Exception ex)
		{
			if (ex is IOException && attempt < maxAttempts)
			{
				int delay = 100 * (attempt + 1);
				await Task.Delay(delay, ct);
				await TryLoadLogAsync(logFile, attempt + 1, ct);
			}
			else
			{
                LogError(ex, TgResourceExtensions.GetErrorOccurredWhileLoadingLogs());
                LogFiles.Add(new(this, Path.GetFileName(logFile), "0 B", TgResourceExtensions.GetErrorOccurredWhileLoadingLogs()));
			}
		}
	}

	// Запрашиваем подтверждение удаления выбранного журнала
	private async Task DeleteLogFileAsync(TgLogFile? logFile, CancellationToken ct = default)
	{
		if (XamlRootVm is null || logFile is null)
			return;
		try
		{
			ContentDialog dialog = new()
			{
				XamlRoot = XamlRootVm,
				Title = TgResourceExtensions.AskDeleteFile(),
				PrimaryButtonText = TgResourceExtensions.GetYesButton(),
				CloseButtonText = TgResourceExtensions.GetCancelButton(),
				DefaultButton = ContentDialogButton.Close,
			};
			var result = await dialog.ShowAsync();
			if (result != ContentDialogResult.Primary) return;

			// Изменяем элементы страницы после закрытия модального диалога
			await DeleteLogFileCoreAsync(logFile, ct);
			await LoadLogsCoreAsync(ct);
		}
		catch (Exception ex)
		{
			LogError(ex, "Не удалось выполнить удаление журнала");
		}
	}

	// Удаляем только выбранный файл, сохраняя его в списке при ошибке
	private Task DeleteLogFileCoreAsync(TgLogFile? logFile, CancellationToken ct = default)
	{
		if (logFile is null) return Task.CompletedTask;
		try
		{
			var file = Path.Combine(TgLogUtils.GetLogsDirectory(TgEnumAppType.Desktop), logFile.FileName);
			File.Delete(file);
			LogFiles.Remove(logFile);
		}
		catch (Exception ex)
		{
			LogError(ex, "Не удалось удалить файл журнала");
		}
		return Task.CompletedTask;
	}

	#endregion
}
