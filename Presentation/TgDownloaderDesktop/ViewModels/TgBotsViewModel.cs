//// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
//// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

//namespace TgDownloaderDesktop.ViewModels;

//[DebuggerDisplay("{ToDebugString()}")]
//public sealed partial class TgBotsViewModel : TgPageViewModelBase
//{
//	#region Public and private fields, properties, constructor

//	[ObservableProperty]
//	public partial string FilterText { get; set; } = string.Empty;
//	public IRelayCommand LoadDataStorageCommand { get; }
//	public IRelayCommand ClearDataStorageCommand { get; }
//	public IRelayCommand DefaultSortCommand { get; }
//	public IRelayCommand ClientConnectCommand { get; }
//	public IRelayCommand AddRecordStorageCommand { get; }

//	public TgBotsViewModel(ITgSettingsService settingsService, INavigationService navigationService) : base(settingsService, navigationService)
//	{
//		// Commands
//		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
//		ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
//		DefaultSortCommand = new AsyncRelayCommand(DefaultSortAsync);
//		ClientConnectCommand = new AsyncRelayCommand(ClientConnectAsync);
//		AddRecordStorageCommand = new AsyncRelayCommand(AddRecordStorageAsync);
//		// Updates
//		//TgGlobalTools.ConnectClient.SetupUpdateStateConnect(UpdateStateConnectAsync);
//		//TgGlobalTools.ConnectClient.SetupUpdateStateProxy(UpdateStateProxyAsync);
//		//TgGlobalTools.ConnectClient.SetupUpdateStateSource(UpdateStateSourceAsync);
//		//TgGlobalTools.ConnectClient.SetupUpdateStateMessage(UpdateStateMessageAsync);
//		//TgGlobalTools.ConnectClient.SetupUpdateException(UpdateExceptionAsync);
//		//TgGlobalTools.ConnectClient.SetupUpdateStateExceptionShort(UpdateStateExceptionShortAsync);
//		//TgGlobalTools.ConnectClient.SetupAfterClientConnect(AfterClientConnectAsync);
//		//TgGlobalTools.ConnectClient.SetupGetClientDesktopConfig(ConfigClientDesktop);
//	}

//	#endregion

//	#region Public and private methods

//	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await LoadDataAsync(async () =>
//		{
//			await LoadDataStorageCoreAsync();
//			await ReloadUiAsync();
//		});

//	/// <summary> Sort data </summary>
//	private void SetOrderData(ObservableCollection<TgEfBotDto> dtos)
//	{
//		if (!dtos.Any())
//			return;
//		Dtos = [.. dtos.OrderBy(x => x.BotToken)];
//		ApplyFilter();
//	}

//	public void ApplyFilter()
//	{
//		if (string.IsNullOrWhiteSpace(FilterText))
//		{
//			FilteredDtos = [.. Dtos];
//		}
//		else
//		{
//			var filtered = Dtos.Where(dto =>
//				dto.BotToken.ToString().Contains(FilterText, StringComparison.InvariantCultureIgnoreCase)
//				).ToList();
//			FilteredDtos = new ObservableCollection<TgEfBotDto>(filtered);
//		}
//	}

//	//private async Task UpdateFromTelegramAsync()
//	//{
//	//	if (!TgGlobalTools.ConnectClient.CheckClientIsReady()) return;
//	//	foreach (TgEfSourceViewModel sourceVm in Dtos)
//	//		await UpdateDtoFromTelegramAsync(sourceVm);
//	//}

//	//private async Task GetSourcesFromTelegramAsync()
//	//{
//	//	if (!TgGlobalTools.ConnectClient.CheckClientIsReady()) return;
//	//	await TgGlobalTools.ConnectClient.ScanSourcesTgDesktopAsync(TgEnumSourceType.Chat, LoadFromTelegramAsync);
//	//	await TgGlobalTools.ConnectClient.ScanSourcesTgDesktopAsync(TgEnumSourceType.Dialog, LoadFromTelegramAsync);
//	//}

//	///// <summary> Load sources from Telegram </summary>
//	//private async Task LoadFromTelegramAsync(TgEfSourceViewModel sourceVm)
//	//{
//	//	var storageResult = await SourceRepository.GetAsync(new TgEfSourceEntity { Id = sourceVm.Item.Id }, isReadOnly: false);
//	//	if (storageResult.IsExists)
//	//		sourceVm = new(storageResult.Item);
//	//	if (!Dtos.Select(x => x.SourceId).Contains(sourceVm.SourceId))
//	//		Dtos.Add(sourceVm);
//	//	await SaveSourceAsync(sourceVm);
//	//}

//	//private async Task MarkAllMessagesAsReadAsync()
//	//{
//	//	if (!TgGlobalTools.ConnectClient.CheckClientIsReady()) return;
//	//	await TgGlobalTools.ConnectClient.MarkHistoryReadAsync();
//	//}

//	private async Task LoadDataStorageAsync() => await ContentDialogAsync(LoadDataStorageCoreAsync, TgResourceExtensions.AskDataLoad(), useLoadData: true);

//	private async Task LoadDataStorageCoreAsync()
//	{
//		if (!SettingsService.IsExistsAppStorage)
//			return;
//		SetOrderData([.. await Repository.GetListDtosAsync(take: 0, skip: 0)]);
//	}

//	private async Task ClearDataStorageAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

//	private async Task ClearDataStorageCoreAsync()
//	{
//		Dtos.Clear();
//		FilteredDtos.Clear();
//		await Task.CompletedTask;
//	}

//	private async Task DefaultSortAsync()
//	{
//		SetOrderData(Dtos);
//		await Task.CompletedTask;
//	}

//	private async Task AddRecordStorageAsync() => await ContentDialogAsync(AddRecordStorageCoreAsync, TgResourceExtensions.AskAddRecord(), useLoadData: true);

//	private async Task AddRecordStorageCoreAsync()
//	{
//		if (!SettingsService.IsExistsAppStorage)
//			return;
//		var dto = new TgEfBotDto();
//		NavigationService.NavigateTo(typeof(TgBotDetailsViewModel).FullName!, dto.Uid);
//		await Task.CompletedTask;
//	}

//	private async Task ClientConnectAsync() => await ContentDialogAsync(ClientConnectCoreAsync, TgResourceExtensions.AskUpdateOnline());

//	private async Task ClientConnectCoreAsync()
//	{
//		await LoadDataAsync(async () =>
//		{
//			if (!await TgGlobalTools.ConnectClient.CheckClientIsReadyAsync())
//				return;
//			await TgGlobalTools.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.Chat);
//			//await TgGlobalTools.ConnectClient.SearchSourcesTgAsync(tgDownloadSettings, TgEnumSourceType.Dialog);
//			await LoadDataStorageCoreAsync();
//		});
//	}

//	public void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
//	{
//		if (sender is not DataGrid dataGrid)
//			return;
//		if (dataGrid.SelectedItem is not TgEfBotDto dto)
//			return;

//		NavigationService.NavigateTo(typeof(TgChatDetailsViewModel).FullName!, dto.Uid);
//	}

//	public void OnFilterTextChanged(object sender, TextChangedEventArgs e)
//	{
//		if (sender is not TextBox textBox)
//			return;
//		FilterText = textBox.Text;
//		ApplyFilter();
//	}

//	#endregion
//}