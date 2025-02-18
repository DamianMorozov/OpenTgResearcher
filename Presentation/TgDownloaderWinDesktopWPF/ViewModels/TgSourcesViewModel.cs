// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TgInfrastructure.Enums;

namespace TgDownloaderWinDesktopWPF.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgSourcesViewModel : TgPageViewModelBase, INavigationAware
{
    #region Public and private fields, properties, constructor

    public ObservableCollection<TgEfSourceViewModel> SourcesVms { get; set; } = [];
    private TgEfSourceRepository SourceRepository { get; } = new();

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

        await OnLoadSourcesFromStorageAsync();
    }

    /// <summary> Sort sources </summary>
    private void SetOrderSources(IEnumerable<TgEfSourceDto> dtos)
    {
        var list = dtos.ToList();
        if (!list.Any()) return;
        SourcesVms = [];

        dtos = list.OrderBy(x => x.UserName).ThenBy(x => x.Title).ToList();
        if (dtos.Any())
            foreach (var dto in dtos)
                SourcesVms.Add(new() { Dto = dto });
    }

    /// <summary> Load sources from Telegram </summary>
    public async Task LoadFromTelegramAsync(ITgEfSourceViewModel sourceVm)
    {
        if (sourceVm is not TgEfSourceViewModel sourceVm2) return;
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
			var storageResult = await SourceRepository.GetAsync(new TgEfSourceEntity { Id = sourceVm2.Dto.Id });
            if (storageResult.IsExists)
                sourceVm2 = new (storageResult.Item);
            if (!SourcesVms.Select(x => x.Dto.Id).Contains(sourceVm2.Dto.Id))
                SourcesVms.Add(sourceVm2);
            await SaveSourceAsync(sourceVm2);
        }, false);
    }

    /// <summary> Update state </summary>
    public override async Task UpdateStateSourceAsync(long sourceId, int messageId, int count, string message)
    {
        await base.UpdateStateSourceAsync(sourceId, messageId, count, message);
        for (int i = 0; i < SourcesVms.Count; i++)
        {
            if (SourcesVms[i].Dto.Id.Equals(sourceId))
            {
                var entity = await SourceRepository.GetItemAsync(new TgEfSourceEntity { Id = sourceId });
				SourcesVms[i].Dto = new TgEfSourceDto().Fill(entity, isUidCopy: true);
                break;
            }
        }
    }

	#endregion

	#region Public and private methods - RelayCommand - All

	// LoadSourcesFromStorageCommand
	[RelayCommand]
    public async Task OnLoadSourcesFromStorageAsync()
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            TgEfStorageResult<TgEfSourceEntity> storageResult = await SourceRepository.GetListAsync(TgEnumTableTopRecords.All, 0);
			var sources = storageResult.IsExists ? storageResult.Items.ToList() : [];
            //if (!sources.Any())
            //{
            //    TgSqlTableSourceModel sourceDefault = ContextManager.SourceRepository.CreateNew(true);
            //    sourceDefault.Id = 1710176740;
            //    sourceDefault.UserName = "TgDownloader";
            //    sourceDefault.Directory = Path.Combine(Environment.CurrentDirectory, sourceDefault.UserName);
            //    sourceDefault.Title = "TgDownloader News";
            //    sourceDefault.About = "Telegram Files Downloader";
            //    await ContextManager.SourceRepository.SaveAsync(sourceDefault);
            //    sources.Add(sourceDefault);
            //}
            SetOrderSources(sources.Select(x => new TgEfSourceDto().Fill(x, isUidCopy: true)));
        }, false);
    }

    // UpdateSourcesFromTelegram
    [RelayCommand]
    public async Task OnUpdateSourcesFromTelegramAsync()
    {
        if (!await CheckClientReadyAsync())
            return;

        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            foreach (TgEfSourceViewModel sourceVm in SourcesVms)
                await OnUpdateSourceFromTelegramAsync(sourceVm);
        }, false);
    }

    // GetSourcesFromTelegramCommand
    [RelayCommand]
    public async Task OnGetSourcesFromTelegramAsync()
    {
        if (!await CheckClientReadyAsync())
            return;

        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            await TgGlobalTools.ConnectClient.ScanSourcesTgDesktopAsync(TgEnumSourceType.Chat, LoadFromTelegramAsync);
            await TgGlobalTools.ConnectClient.ScanSourcesTgDesktopAsync(TgEnumSourceType.Dialog, LoadFromTelegramAsync);
        }, false);
    }

	// MarkAllMessagesAsReadCommand
	[RelayCommand]
    public async Task OnMarkAllMessagesAsReadAsync()
    {
        if (!await CheckClientReadyAsync())
            return;

        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            await TgGlobalTools.ConnectClient.MarkHistoryReadAsync();
        }, false);
    }

    // ClearViewCommand
    [RelayCommand]
    public async Task OnClearViewAsync()
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            SourcesVms = [];
        }, false);
    }

    // SortViewCommand
    [RelayCommand]
    public async Task OnSortViewAsync()
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            SetOrderSources(SourcesVms.Select(x => x.Dto).ToList());
        }, false);
    }

    /// <summary>
    /// Save sources into the Storage.
    /// </summary>
    // SaveSourcesCommand
    [RelayCommand]
    public async Task OnSaveSourcesAsync()
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            // Checks.
            if (!SourcesVms.Any())
            {
                await TgGlobalTools.ConnectClient.UpdateStateSourceAsync(0, 0, 0, "Empty sources list!");
                return;
            }
            foreach (TgEfSourceViewModel sourceVm in SourcesVms)
            {
                await SaveSourceAsync(sourceVm);
            }
        }, false);
    }

    public async Task SaveSourceAsync(TgEfSourceViewModel sourceVm)
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
			TgEfStorageResult<TgEfSourceEntity> storageResult = await SourceRepository.GetAsync(new TgEfSourceEntity { Id = sourceVm.Dto.Id }, isReadOnly: false);
            if (!storageResult.IsExists)
            {
                var entity = sourceVm.Dto.GetEntity();
                await SourceRepository.SaveAsync(entity);
                await TgGlobalTools.ConnectClient.UpdateStateSourceAsync(sourceVm.Dto.Id, sourceVm.Dto.FirstId, sourceVm.Dto.Count, $"Saved source | {sourceVm.Dto}");
            }
        }, false);
    }

    #endregion

    #region Public and private methods - RelayCommand - One

    // GetSourceFromStorageCommand
    [RelayCommand]
    public async Task OnGetSourceFromStorageAsync(TgEfSourceViewModel sourceVm)
    {
        TgDesktopUtils.TgItemSourceVm.SetItemSourceVm(sourceVm);
        await TgDesktopUtils.TgItemSourceVm.OnGetSourceFromStorageAsync();

        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            for (int i = 0; i < SourcesVms.Count; i++)
            {
                if (SourcesVms[i].Dto.Id.Equals(sourceVm.Dto.Id))
                {
                    SourcesVms[i].Dto.Fill(TgDesktopUtils.TgItemSourceVm.SourceVm.Dto, isUidCopy: false);
                    break;
                }
            }
        }, false);
    }

    // UpdateSourceFromTelegram
    [RelayCommand]
    public async Task OnUpdateSourceFromTelegramAsync(TgEfSourceViewModel sourceVm)
    {
        TgDesktopUtils.TgItemSourceVm.SetItemSourceVm(sourceVm);
        await TgDesktopUtils.TgItemSourceVm.OnUpdateSourceFromTelegramAsync();

        await OnGetSourceFromStorageAsync(sourceVm);
    }

    // DownloadCommand
    [RelayCommand]
    public async Task OnDownloadAsync(TgEfSourceViewModel sourceVm)
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            TgDesktopUtils.TgItemSourceVm.SetItemSourceVm(sourceVm);
            TgDesktopUtils.TgItemSourceVm.ViewModel = this;
            if (await TgDesktopUtils.TgItemSourceVm.OnDownloadSourceAsync())
                await TgDesktopUtils.TgItemSourceVm.OnUpdateSourceFromTelegramAsync();
        }, false);
    }

    // EditSourceCommand
    [RelayCommand]
    public async Task OnEditSourceAsync(TgEfSourceViewModel sourceVm)
    {
        await TgDesktopUtils.RunFuncAsync(this, async () =>
        {
            await Task.Delay(1);
            if (Application.Current.MainWindow is MainWindow navigationWindow)
            {
                TgDesktopUtils.TgItemSourceVm.SetItemSourceVm(sourceVm);
                navigationWindow.ShowWindow();
                navigationWindow.Navigate(typeof(TgItemSourcePage));
            }
        }, false);
    }

    #endregion
}