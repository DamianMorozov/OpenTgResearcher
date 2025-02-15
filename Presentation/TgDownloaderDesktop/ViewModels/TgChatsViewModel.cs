// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatsViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    private TgEfSourceRepository Repository { get; } = new();
	[ObservableProperty]
	public partial ObservableCollection<TgEfSourceLiteDto> Dtos { get; set; } = [];
	[ObservableProperty]
	public partial ObservableCollection<TgEfSourceLiteDto> FilteredDtos { get; set; } = [];
	[ObservableProperty]
	public partial string FilterText { get; set; } = string.Empty;
	public IRelayCommand LoadDataStorageCommand { get; }
	public IRelayCommand ClearDataStorageCommand { get; }
	public IRelayCommand DefaultSortCommand { get; }
	public IRelayCommand UpdateOnlineCommand { get; }

	public TgChatsViewModel(ITgSettingsService settingsService, INavigationService navigationService) : base(settingsService, navigationService)
	{
		// Commands
		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
		ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
		DefaultSortCommand = new AsyncRelayCommand(DefaultSortAsync);
		UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
		// Updates
		//TgDesktopUtils.TgClient.SetupUpdateStateConnect(UpdateStateConnectAsync);
		//TgDesktopUtils.TgClient.SetupUpdateStateProxy(UpdateStateProxyAsync);
		//TgDesktopUtils.TgClient.SetupUpdateStateSource(UpdateStateSourceAsync);
		//TgDesktopUtils.TgClient.SetupUpdateStateMessage(UpdateStateMessageAsync);
		//TgDesktopUtils.TgClient.SetupUpdateException(UpdateExceptionAsync);
		//TgDesktopUtils.TgClient.SetupUpdateStateExceptionShort(UpdateStateExceptionShortAsync);
		//TgDesktopUtils.TgClient.SetupAfterClientConnect(AfterClientConnectAsync);
		//TgDesktopUtils.TgClient.SetupGetClientDesktopConfig(ConfigClientDesktop);
	}

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await LoadDataAsync(async () =>
		{
			await LoadDataStorageCoreAsync();
			await ReloadUiAsync();
		});

	/// <summary> Sort data </summary>
	private void SetOrderData(ObservableCollection<TgEfSourceLiteDto> dtos)
	{
		if (!dtos.Any()) return;
		Dtos = [.. dtos.OrderBy(x => x.UserName).ThenBy(x => x.Title)];
		ApplyFilter();
	}

	public void ApplyFilter()
	{
		if (string.IsNullOrWhiteSpace(FilterText))
		{
			FilteredDtos = [.. Dtos];
		}
		else
		{
			var filtered = Dtos.Where(dto =>
				dto.Id.ToString().Contains(FilterText, StringComparison.InvariantCultureIgnoreCase) ||
				dto.UserName.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase) ||
				dto.Title.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase)
				).ToList();
			FilteredDtos = new ObservableCollection<TgEfSourceLiteDto>(filtered);
		}
	}

	//private async Task UpdateFromTelegramAsync()
	//{
	//	if (!TgDesktopUtils.TgClient.CheckClientIsReady()) return;
	//	foreach (TgEfSourceViewModel sourceVm in Dtos)
	//		await UpdateDtoFromTelegramAsync(sourceVm);
	//}

	//private async Task GetSourcesFromTelegramAsync()
	//{
	//	if (!TgDesktopUtils.TgClient.CheckClientIsReady()) return;
	//	await TgDesktopUtils.TgClient.ScanSourcesTgDesktopAsync(TgEnumSourceType.Chat, LoadFromTelegramAsync);
	//	await TgDesktopUtils.TgClient.ScanSourcesTgDesktopAsync(TgEnumSourceType.Dialog, LoadFromTelegramAsync);
	//}

	///// <summary> Load sources from Telegram </summary>
	//private async Task LoadFromTelegramAsync(TgEfSourceViewModel sourceVm)
	//{
	//	var storageResult = await SourceRepository.GetAsync(new TgEfSourceEntity { Id = sourceVm.Item.Id }, isReadOnly: false);
	//	if (storageResult.IsExists)
	//		sourceVm = new(storageResult.Item);
	//	if (!Dtos.Select(x => x.SourceId).Contains(sourceVm.SourceId))
	//		Dtos.Add(sourceVm);
	//	await SaveSourceAsync(sourceVm);
	//}

	//private async Task MarkAllMessagesAsReadAsync()
	//{
	//	if (!TgDesktopUtils.TgClient.CheckClientIsReady()) return;
	//	await TgDesktopUtils.TgClient.MarkHistoryReadAsync();
	//}

	private async Task LoadDataStorageAsync() => await ContentDialogAsync(LoadDataStorageCoreAsync, TgResourceExtensions.AskDataLoad(), useLoadData: true);

	private async Task LoadDataStorageCoreAsync()
	{
		if (!SettingsService.IsExistsAppStorage) return;
		SetOrderData([.. await Repository.GetListLiteDtosAsync(take: 0, skip: 0)]);
	}

	private async Task ClearDataStorageAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

	private async Task ClearDataStorageCoreAsync()
	{
		Dtos.Clear();
		FilteredDtos.Clear();
		await Task.CompletedTask;
	}

	private async Task DefaultSortAsync()
	{
		SetOrderData(Dtos);
		await Task.CompletedTask;
	}

	//private async Task SaveSourceAsync(TgEfSourceViewModel sourceVm)
	//{
	//	if (sourceVm is null) return;
	//	var storageResult = await SourceRepository.GetAsync(new TgEfSourceEntity { Id = sourceVm.Item.Id }, isReadOnly: false);
	//	if (!storageResult.IsExists)
	//	{
	//		await SourceRepository.SaveAsync(sourceVm.Item);
	//		await TgDesktopUtils.TgClient.UpdateStateSourceAsync(sourceVm.Item.Id, 0, $"Saved source | {sourceVm.Item}");
	//	}
	//}

	//private async Task GetSourceFromStorageAsync(TgEfSourceViewModel? sourceVm)
	//{
	//	if (sourceVm is null) return;
	//	//TgDesktopUtils.TgItemSourceVm.SetItemSourceVm(sourceVm);
	//	//await TgDesktopUtils.TgItemSourceVm.OnGetSourceFromStorageAsync();

	//	//for (int i = 0; i < Dtos.Count; i++)
	//	//{
	//	//	if (Dtos[i].SourceId.Equals(sourceVm.SourceId))
	//	//	{
	//	//		Dtos[i].Item.Fill(TgDesktopUtils.TgItemSourceVm.ItemSourceVm.Item, isUidCopy: false);
	//	//		break;
	//	//	}
	//	//}
	//	await Task.CompletedTask;
	//}

	//private async Task UpdateDtoFromTelegramAsync(TgEfSourceViewModel? sourceVm)
	//{
	//	if (sourceVm is null) return;
	//	//TgDesktopUtils.TgItemSourceVm.SetItemSourceVm(sourceVm);
	//	//await TgDesktopUtils.TgItemSourceVm.OnUpdateSourceFromTelegramAsync();
	//	await GetSourceFromStorageAsync(sourceVm);
	//}

	//private async Task DownloadAsync(TgEfSourceViewModel? sourceVm)
	//{
	//	if (sourceVm is null) return;
	//	//TgDesktopUtils.TgItemSourceVm.SetItemSourceVm(sourceVm);
	//	//TgDesktopUtils.TgItemSourceVm.ViewModel = this;
	//	//if (await TgDesktopUtils.TgItemSourceVm.OnDownloadSourceAsync())
	//	//	await TgDesktopUtils.TgItemSourceVm.OnUpdateSourceFromTelegramAsync();
	//	await Task.CompletedTask;
	//}

	//private async Task EditSourceAsync(TgEfSourceViewModel? sourceVm)
	//{
	//	if (sourceVm is null) return;
	//	//if (Application.Current.MainWindow is MainWindow navigationWindow)
	//	//{
	//	//	TgDesktopUtils.TgItemSourceVm.SetItemSourceVm(sourceVm);
	//	//	navigationWindow.ShowWindow();
	//	//	navigationWindow.Navigate(typeof(TgItemSourcePage));
	//	//}
	//	await Task.CompletedTask;
	//}

	private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline());

	private async Task UpdateOnlineCoreAsync()
	{
		await LoadDataAsync(async () => {
			if (!await TgDesktopUtils.TgClient.CheckClientIsReadyAsync()) return;
			await TgDesktopUtils.TgClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.Chat);
			//await TgDesktopUtils.TgClient.SearchSourcesTgAsync(tgDownloadSettings, TgEnumSourceType.Dialog);
			await LoadDataStorageCoreAsync();
		});
	}

	public void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
	{
		if (sender is not DataGrid dataGrid)
			return;
		if (dataGrid.SelectedItem is not TgEfSourceLiteDto dto)
			return;

		NavigationService.NavigateTo(typeof(TgChatDetailsViewModel).FullName!, dto.Uid);
	}

	public void OnFilterTextChanged(object sender, TextChangedEventArgs e)
	{
		var textBox = sender as TextBox;
		if (textBox is null) return;
		FilterText = textBox.Text;
		ApplyFilter();
	}

	#endregion
}