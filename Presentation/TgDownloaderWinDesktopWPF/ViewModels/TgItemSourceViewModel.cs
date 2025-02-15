// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderWinDesktopWPF.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgItemSourceViewModel : TgPageViewModelBase, INavigationAware
{
    #region Public and private fields, properties, constructor

    private TgEfSourceRepository SourceRepository { get; } = new();
    public TgEfSourceViewModel SourceVm { get; } = new();
    public Visibility ChartVisibility { get; private set; } = Visibility.Hidden;
    private LineSeries LineSerie { get; set; } = new()
    {
	    Title = string.Empty,
	    Values = new ChartValues<long> { 1, 2 },
	    StrokeThickness = 0.5,
	    PointGeometry = null,
    };
    private Stopwatch SwDownloadChart { get; set; } = Stopwatch.StartNew();
	public SeriesCollection FileSpeedSeries { get; } = [];
	public List<string> FileSpeedFormatterX { get; set; } = ["0", "1"];
	public Func<long, string> FileSpeedFormatterY { get; set; } = value => value.ToString();
	public TgPageViewModelBase? ViewModel { get; set; }
    private Guid SourceUid { get; set; }
    private long FileSizeMinSizeMonitoring => 52_428_800; // 50 * 1024 * 1024;

    #endregion

	#region Public and private methods

	public void OnNavigatedTo()
	{
        var task = InitializeViewModelAsync();
		task.Wait();
	}

	public void OnNavigatedFrom() { }

    protected override async Task InitializeViewModelAsync()
    {
        TgDesktopUtils.TgClient.SetupUpdateStateItemSource(UpdateStateItemSourceAsync);
        TgDesktopUtils.TgClient.SetupUpdateStateFile(UpdateStateFileAsync);
        await base.InitializeViewModelAsync();
        await OnGetSourceFromStorageAsync();
    }

    private async Task UpdateStateItemSourceAsync(long sourceId)
    {
	    if (SourceVm.Dto.Id.Equals(sourceId))
	    {
		    await OnGetSourceFromStorageAsync();
	    }
        //else
        //{
        //    await TgDesktopUtils.RunFuncAsync(ViewModel ?? this, async () =>
        //    {
        //        await Task.Delay(1);
        //        SourceUid = Guid.Empty;
        //        TgSqlTableSourceModel source = await ContextManager.SourceRepository.GetNewAsync();
        //        SetItemSourceVm(source, source.Uid);
        //    }, true);
        //}
    }

    public void SetItemSourceVm(TgEfSourceViewModel itemSourceVm) => SetItemSourceVm(itemSourceVm.Dto);

    public void SetItemSourceVm(TgEfSourceDto dto)
    {
        SourceVm.Dto.Fill(dto, isUidCopy: false);
    }

    // GetSourceFromStorageCommand
    [RelayCommand]
    public async Task OnGetSourceFromStorageAsync()
    {
        await TgDesktopUtils.RunFuncAsync(ViewModel ?? this, async () =>
        {
            await Task.Delay(1);
            if (SourceVm.Dto.Uid != SourceUid)
                SourceUid = SourceVm.Dto.Uid;
            TgEfSourceEntity source = await SourceRepository.GetItemAsync(new TgEfSourceEntity() { Uid = SourceUid });
            var dto = new TgEfSourceDto().Fill(source, isUidCopy: true);
			SetItemSourceVm(dto);
        }, true);
    }

    /// <summary> Update download file state </summary>
    private async Task UpdateStateFileAsync(long sourceId, int messageId, string fileName, long fileSize, long transmitted, long fileSpeed, bool isFileNewDownload,
	    int threadNumber)
    {
	    await TgDesktopUtils.RunFuncAsync(ViewModel ?? this, async () =>
	    {
		    await Task.Delay(1);
			// Download job.
			if (!string.IsNullOrEmpty(fileName) && !isFileNewDownload && SourceVm.Dto.Id.Equals(sourceId))
            {
				// Download chart job.
				if (fileSize > FileSizeMinSizeMonitoring && SwDownloadChart.Elapsed.Seconds > 5)
                {
	                ChartVisibility = Visibility.Visible;
                    LineSerie.Values.Add(fileSpeed / 1024 / 1024);
                    FileSpeedFormatterX.Add($"{SwDownloadChart.Elapsed.Seconds}");
                }
				else
				{
					ChartVisibility = Visibility.Hidden;
				}
				// Download status job.
				SourceVm.Dto.FirstId = messageId;
				SourceVm.Dto.CurrentFileName = fileName;
                SourceVm.Dto.CurrentFileSize = fileSize;
                SourceVm.Dto.CurrentFileTransmitted = transmitted;
                SourceVm.Dto.CurrentFileSpeed = fileSpeed;
			}
			// Download reset.
			else
			{
				ChartVisibility = Visibility.Hidden;
				// Download chart reset.
				SwDownloadChart = Stopwatch.StartNew();
				SourceVm.Dto.CurrentFileName = fileName;
				LineSerie = new()
				{
					Title = fileName,
					Values = new ChartValues<long>(),
					StrokeThickness = 0.5,
					PointGeometry = null,
				};
				FileSpeedFormatterX = [];
				FileSpeedSeries.Clear();
				FileSpeedSeries.Add(LineSerie);
				// Download status reset.
				SourceVm.Dto.FirstId = messageId;
				SourceVm.Dto.CurrentFileName = string.Empty;
				SourceVm.Dto.CurrentFileSize = 0;
				SourceVm.Dto.CurrentFileTransmitted = 0;
				SourceVm.Dto.CurrentFileSpeed = 0;
			}
		}, true);
    }

	// UpdateSourceFromTelegramCommand
	[RelayCommand]
    public async Task OnUpdateSourceFromTelegramAsync()
    {
        if (!await CheckClientReadyAsync())
            return;

        await TgDesktopUtils.RunFuncAsync(ViewModel ?? this, async () =>
        {
            await Task.Delay(1);
            if (SourceVm.Dto.Uid != SourceUid)
				SourceUid = SourceVm.Dto.Uid;
            // Collect chats from Telegram
            if (!TgDesktopUtils.TgClient.DicChatsAll.Any())
                await TgDesktopUtils.TgClient.CollectAllChatsAsync();
			// Download settings
            TgDownloadSettingsViewModel tgDownloadSettings = TgDesktopUtils.TgDownloadsVm.CreateDownloadSettings(SourceVm);
			// Update source from Telegram
			await TgDesktopUtils.TgClient.UpdateSourceDbAsync(SourceVm, tgDownloadSettings);
            var entity = SourceVm.Dto.GetEntity();
            await SourceRepository.SaveAsync(entity);
            // Message
            await TgDesktopUtils.TgClient.UpdateStateSourceAsync(SourceVm.Dto.Id, SourceVm.Dto.FirstId, SourceVm.Dto.Count, TgDesktopUtils.TgLocale.SettingsChat);
        }, false);

        await OnGetSourceFromStorageAsync();
    }

    // DownloadSourceCommand
    [RelayCommand]
    public async Task<bool> OnDownloadSourceAsync()
    {
        if (!await CheckClientReadyAsync())
            return false;

        await OnUpdateSourceFromTelegramAsync();

        bool result = true;
        await TgDesktopUtils.RunFuncAsync(ViewModel ?? this, async () =>
        {
            await Task.Delay(1);
            // Check directory.
            if (!Directory.Exists(SourceVm.Dto.Directory))
            {
                await TgDesktopUtils.TgClient.UpdateStateSourceAsync(SourceVm.Dto.Id, SourceVm.Dto.FirstId, SourceVm.Dto.Count,
                    $"Directory is not exists! {SourceVm.Dto.Directory}");
                result = false;
                return;
            }

            // Download settings.
            TgDownloadSettingsViewModel tgDownloadSettings = TgDesktopUtils.TgDownloadsVm.CreateDownloadSettings(SourceVm);
            SwDownloadChart = Stopwatch.StartNew();
			// Job.
			await TgDesktopUtils.TgClient.DownloadAllDataAsync(tgDownloadSettings);
			await TgDesktopUtils.TgClient.UpdateStateSourceAsync(SourceVm.Dto.Id, SourceVm.Dto.FirstId, SourceVm.Dto.Count, TgDesktopUtils.TgLocale.SettingsChat);
        }, true);

        await OnGetSourceFromStorageAsync();
        return result;
    }

	// StopSourceCommand
	[RelayCommand]
	public async Task<bool> OnStopSourceAsync()
	{
        if (!await CheckClientReadyAsync()) return false;

        bool result = true;
        await TgDesktopUtils.RunFuncAsync(ViewModel ?? this, async () =>
        {
	        await Task.Delay(1).ConfigureAwait(false);
	        SourceVm.Dto.IsDownload = false;
        }, true);

        await OnGetSourceFromStorageAsync();
        return result;
	}

	// ClearViewCommand
	[RelayCommand]
    public async Task OnClearViewAsync()
    {
        await TgDesktopUtils.RunFuncAsync(ViewModel ?? this, async () =>
        {
            await Task.Delay(1);
            //if (ItemSourceVm.SourceUid != SourceUid)
            //    SourceUid = ItemSourceVm.SourceUid;
            var entity = (await SourceRepository.GetNewAsync()).Item;
            SourceVm.Dto = new TgEfSourceDto().Fill(entity, isUidCopy: true);
        }, false);
    }

    // SaveSourceCommand
    [RelayCommand]
    public async Task OnSaveSourceAsync()
    {
        await TgDesktopUtils.RunFuncAsync(ViewModel ?? this, async () =>
        {
            await Task.Delay(1);
            var entity = SourceVm.Dto.GetEntity();
            await SourceRepository.SaveAsync(entity);
        }, false);

        await OnGetSourceFromStorageAsync();
    }

    // ReturnToSectionSourcesCommand
    [RelayCommand]
    public async Task OnReturnToSectionSourcesAsync()
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            if (Application.Current.MainWindow is MainWindow navigationWindow)
            {
                navigationWindow.ShowWindow();
                navigationWindow.Navigate(typeof(TgSourcesPage));
            }
        }, false);
    }

    // CopyCommand
    [RelayCommand]
    public async Task OnCopyAsync(string fieldName)
    {
	    await TgDesktopUtils.RunFuncAsync(this, async () =>
	    {
		    await Task.Delay(1);
		    string value = fieldName switch
		    {
			    nameof(SourceVm.Dto.Id) => SourceVm.Dto.Id.ToString(),
			    nameof(SourceVm.Dto.UserName) => SourceVm.Dto.UserName,
			    nameof(SourceVm.Dto.Title) => SourceVm.Dto.Title,
			    nameof(SourceVm.Dto.About) => SourceVm.Dto.About,
			    nameof(SourceVm.Dto.FirstId) => SourceVm.Dto.FirstId.ToString(),
			    nameof(SourceVm.Dto.Count) => SourceVm.Dto.Count.ToString(),
			    nameof(SourceVm.Dto.Directory) => SourceVm.Dto.Directory,
			    nameof(SourceVm.Dto.DtChangedString) => SourceVm.Dto.DtChangedString,
			    _ => string.Empty
		    };
		    Clipboard.SetText(value);
		    await UpdateStateSourceAsync(0, 0, 0, $"{fieldName} is copied");
	    }, false);
    }

	#endregion
}