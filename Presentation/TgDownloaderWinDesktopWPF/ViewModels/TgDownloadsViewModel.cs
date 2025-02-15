// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderWinDesktopWPF.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgDownloadsViewModel : TgPageViewModelBase, INavigationAware
{
    #region Public and private fields, properties, constructor

    public ObservableCollection<TgEfDownloadViewModel> DownloadVms { get; set; } = [];

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
        await base.InitializeViewModelAsync();

        //await OnLoadSourcesFromStorageAsync();
    }

    /// <summary>
    /// Sort.
    /// </summary>
    private void SetOrderJobs(IEnumerable<TgEfDownloadViewModel> downloadVms)
    {
	    List<TgEfDownloadViewModel> list = downloadVms.ToList();
	    if (!list.Any()) return;
	    DownloadVms = [];

	    downloadVms = [.. list.OrderBy(x => x.DownloadSetting.SourceVm.Dto.UserName).ThenBy(x => x.DownloadSetting.SourceVm.Dto.Title)];
	    if (downloadVms.Any())
		    foreach (TgEfDownloadViewModel downloadVm in downloadVms)
			    DownloadVms.Add(downloadVm);
    }

	/// <summary>
	/// Create new download settings.
	/// </summary>
	/// <param name="sourceVm"></param>
	/// <returns></returns>
	public TgDownloadSettingsViewModel CreateDownloadSettings(TgEfSourceViewModel sourceVm)
	{
		TgDownloadSettingsViewModel downloadSettings = new() { SourceVm = new() };
        downloadSettings.SourceVm.Dto.Id= sourceVm.Dto.Id;
		downloadSettings.SourceVm.Dto.FirstId = sourceVm.Dto.FirstId;
		downloadSettings.SourceVm.Dto.Directory = sourceVm.Dto.Directory ?? string.Empty;
        if (!DownloadVms.Any())
        {
            //DownloadVms.Add(downloadSettings);
        }
        return downloadSettings;
	}
    
	#endregion

	#region Public and private methods - RelayCommand

	// SortViewCommand
	[RelayCommand]
    public async Task OnSortViewAsync()
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            SetOrderJobs(DownloadVms.ToList());
        }, false);
    }

    // StartCommand
    [RelayCommand]
    public async Task OnStartAsync(TgEfDownloadViewModel jobVm)
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            TgEfDownloadViewModel? findJobVm = DownloadVms
				.Where(x => x.DownloadSetting.SourceVm.Dto.Id.Equals(jobVm.DownloadSetting.SourceVm.Dto.Id))
				.SingleOrDefault();
			if (findJobVm is not null)
            {
	            // 
            }
		}, false);
    }

    // StopCommand
    [RelayCommand]
    public async Task OnStopAsync(TgEfDownloadViewModel jobVm)
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            TgEfDownloadViewModel? findJobVm = DownloadVms
				.Where(x => x.DownloadSetting.SourceVm.Dto.Id.Equals(jobVm.DownloadSetting.SourceVm.Dto.Id))
				.SingleOrDefault();
			if (findJobVm is not null)
            {
	            // 
            }
        }, false);
    }

    #endregion
}