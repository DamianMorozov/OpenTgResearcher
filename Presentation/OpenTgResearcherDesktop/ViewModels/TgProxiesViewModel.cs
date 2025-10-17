namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgProxiesViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

	[ObservableProperty]
	public partial ObservableCollection<TgEfProxyDto> Dtos { get; set; } = [];
	public IAsyncRelayCommand LoadDataStorageCommand { get; }
	public IAsyncRelayCommand ClearViewCommand { get; }
	public IAsyncRelayCommand DefaultSortCommand { get; }
	public IAsyncRelayCommand StartUpdateOnlineCommand { get; }
	public IAsyncRelayCommand CreateDefaultProxyCommand { get; }
	public IAsyncRelayCommand DeleteAllProxiesCommand { get; }
    public IAsyncRelayCommand<TgEfProxyDto> OpenOrEditCommand { get; }

    public TgProxiesViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgProxiesViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgProxiesViewModel))
	{
		// Commands
		ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
		DefaultSortCommand = new AsyncRelayCommand(DefaultSortAsync);
		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
		StartUpdateOnlineCommand = new AsyncRelayCommand(StartUpdateOnlineAsync);
        CreateDefaultProxyCommand = new AsyncRelayCommand(CreateDefaultProxyAsync);
        DeleteAllProxiesCommand = new AsyncRelayCommand(DeleteAllProxiesAsync);
        OpenOrEditCommand = new AsyncRelayCommand<TgEfProxyDto>(OpenOrEditAsync);
    }

	#endregion

	#region Methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
	{
		await LoadDataStorageCoreAsync();
		await ReloadUiAsync();
	});

	/// <summary> Sort data </summary>
	private void SetOrderData(ObservableCollection<TgEfProxyDto> dtos)
	{
		if (!dtos.Any())
        {
            Dtos = [];
            return;
        }

		Dtos = [.. dtos.OrderBy(x => x.HostName).ThenBy(x => x.Type)];
	}

	private async Task ClearViewAsync() => 
        await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

	private async Task ClearDataStorageCoreAsync()
	{
		Dtos.Clear();
		await Task.CompletedTask;
	}

	private async Task LoadDataStorageAsync() => 
        await ContentDialogAsync(LoadDataStorageCoreAsync, TgResourceExtensions.AskLoading(), TgEnumLoadDesktopType.Storage);

	private async Task LoadDataStorageCoreAsync()
	{
		if (!SettingsService.IsExistsAppStorage) return;
		SetOrderData([.. await App.BusinessLogicManager.StorageManager.ProxyRepository.GetListDtosAsync()]);
	}

	private async Task DefaultSortAsync()
	{
		SetOrderData(Dtos);
		await Task.CompletedTask;
	}

	private async Task StartUpdateOnlineAsync() => 
        await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline(), TgEnumLoadDesktopType.Online);

    private async Task UpdateOnlineCoreAsync() => await LoadStorageDataAsync(async () =>
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
        await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.Story);
        await LoadDataStorageCoreAsync();
    });

	private async Task CreateDefaultProxyAsync() => 
        await ContentDialogAsync(CreateDefaultProxyCoreAsync, TgResourceExtensions.AskCreateDefault(), TgEnumLoadDesktopType.Online);

    private async Task CreateDefaultProxyCoreAsync() => await LoadStorageDataAsync(async () =>
    {
        await App.BusinessLogicManager.StorageManager.ProxyRepository.CreateDefaultAsync();
        await LoadDataStorageCoreAsync();
    });

	private async Task DeleteAllProxiesAsync() => 
        await ContentDialogAsync(DeleteAllProxiesCoreAsync, TgResourceExtensions.AskDeleteAllRecords(), TgEnumLoadDesktopType.Online);

    private async Task DeleteAllProxiesCoreAsync() => await LoadStorageDataAsync(async () =>
    {
        await App.BusinessLogicManager.StorageManager.AppRepository.ClearProxyAsync();
        await App.BusinessLogicManager.StorageManager.ProxyRepository.DeleteAllAsync();
        await LoadDataStorageCoreAsync();
    });

    private async Task OpenOrEditAsync(TgEfProxyDto? proxyDto) =>
        await ContentDialogAsync(() => OpenOrEditCoreAsync(proxyDto), TgResourceExtensions.AskOpenOrEdit(), TgEnumLoadDesktopType.Storage);

    private async Task OpenOrEditCoreAsync(TgEfProxyDto? proxyDto)
    {
        if (!SettingsService.IsExistsAppStorage) return;
        if (proxyDto is null) return;

        NavigationService.NavigateTo(typeof(TgProxyViewModel).FullName!, proxyDto.Uid);
        await Task.CompletedTask;
    }

    public void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not DataGrid dataGrid) return;
        if (dataGrid.SelectedItem is not TgEfProxyDto dto) return;

        NavigationService.NavigateTo(typeof(TgProxyViewModel).FullName!, dto.Uid);
    }

    #endregion
}
