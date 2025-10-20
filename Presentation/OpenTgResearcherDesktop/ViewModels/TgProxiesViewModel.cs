namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgProxiesViewModel : TgSectionViewModel
{
    #region Fields, properties, constructor

	[ObservableProperty]
	public partial ObservableCollection<TgEfProxyDto> Dtos { get; set; } = [];
	public IAsyncRelayCommand LoadDataStorageCommand { get; }
	public IAsyncRelayCommand DefaultSortCommand { get; }
	public IAsyncRelayCommand CreateDefaultProxyCommand { get; }
	public IAsyncRelayCommand DeleteAllProxiesCommand { get; }

    public TgProxiesViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgProxiesViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgProxiesViewModel))
	{
		// Commands
		DefaultSortCommand = new AsyncRelayCommand(DefaultSortAsync);
		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
        CreateDefaultProxyCommand = new AsyncRelayCommand(CreateDefaultProxyAsync);
        DeleteAllProxiesCommand = new AsyncRelayCommand(DeleteAllProxiesAsync);
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

    protected override async Task ClearViewCoreAsync(bool isFinally)
    {
        await base.ClearViewCoreAsync(isFinally);

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

    protected override async Task StartUpdateOnlineCoreAsync()
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
        await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.Story);
        await LoadDataStorageCoreAsync();
    }

    protected override async Task StopUpdateOnlineCoreAsync()
    {
        await Task.CompletedTask;
    }

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

    public void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not DataGrid dataGrid) return;
        if (dataGrid.SelectedItem is not TgEfProxyDto dto) return;

        NavigationService.NavigateTo(typeof(TgProxyViewModel).FullName!, dto.Uid);
    }

    #endregion
}
