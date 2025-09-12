namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgProxiesViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

	[ObservableProperty]
	public partial ObservableCollection<TgEfProxyDto> Dtos { get; set; } = [];
	public IRelayCommand LoadDataStorageCommand { get; }
	public IRelayCommand ClearViewCommand { get; }
	public IRelayCommand DefaultSortCommand { get; }
	public IRelayCommand UpdateOnlineCommand { get; }

	public TgProxiesViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgProxiesViewModel> logger) 
		: base(settingsService, navigationService, logger, nameof(TgProxiesViewModel))
	{
		// Commands
		ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
		DefaultSortCommand = new AsyncRelayCommand(DefaultSortAsync);
		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
		UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
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
		if (!dtos.Any()) return;
		Dtos = [.. dtos.OrderBy(x => x.HostName).ThenBy(x => x.Type)];
	}

	private async Task ClearViewAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

	private async Task ClearDataStorageCoreAsync()
	{
		Dtos.Clear();
		await Task.CompletedTask;
	}

	private async Task LoadDataStorageAsync() => await ContentDialogAsync(LoadDataStorageCoreAsync, TgResourceExtensions.AskDataLoad(), TgEnumLoadDesktopType.Storage);

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

	private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline(), TgEnumLoadDesktopType.Online);

    private async Task UpdateOnlineCoreAsync() => await LoadStorageDataAsync(async () =>
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
        await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.Story);
        await LoadDataStorageCoreAsync();
    });

    #endregion
}
