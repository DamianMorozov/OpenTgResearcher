// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgContactsViewModel : TgPageViewModelBase
{
	#region Public and private fields, properties, constructor

	private TgEfContactRepository Repository { get; } = new();
	[ObservableProperty]
	public partial ObservableCollection<TgEfContactDto> Dtos { get; set; } = [];
	public IRelayCommand LoadDataStorageCommand { get; }
	public IRelayCommand ClearDataStorageCommand { get; }
	public IRelayCommand DefaultSortCommand { get; }
	public IRelayCommand UpdateOnlineCommand { get; }

	public TgContactsViewModel(ITgSettingsService settingsService, INavigationService navigationService) : base(settingsService, navigationService)
    {
		// Commands
		ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
		DefaultSortCommand = new AsyncRelayCommand(DefaultSortAsync);
		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
		UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
	}

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await LoadDataAsync(async () =>
		{
			await LoadDataStorageCoreAsync();
			await ReloadUiAsync();
		});

	/// <summary> Sort data </summary>
	private void SetOrderData(ObservableCollection<TgEfContactDto> dtos)
	{
		if (!dtos.Any()) return;
		Dtos = [.. dtos.OrderBy(x => x.UserName).ThenBy(x => x.FirstName).ThenBy(x => x.LastName)];
	}

	private async Task ClearDataStorageAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

	private async Task ClearDataStorageCoreAsync()
	{
		Dtos.Clear();
		await Task.CompletedTask;
	}

	private async Task LoadDataStorageAsync() => await ContentDialogAsync(LoadDataStorageCoreAsync, TgResourceExtensions.AskDataLoad(), useLoadData: true);

	private async Task LoadDataStorageCoreAsync()
	{
		if (!SettingsService.IsExistsAppStorage) return;
		SetOrderData([.. await Repository.GetListDtosAsync(take: 0, skip: 0)]);
	}

	private async Task DefaultSortAsync()
	{
		SetOrderData(Dtos);
		await Task.CompletedTask;
	}

	private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline());

	private async Task UpdateOnlineCoreAsync()
	{
		await LoadDataAsync(async () => {
			if (!await TgDesktopUtils.TgClient.CheckClientIsReadyAsync()) return;
			await TgDesktopUtils.TgClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.Contact);
			await LoadDataStorageCoreAsync();
		});
	}

	public void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
	{
		if (sender is not DataGrid dataGrid) return;
		if (dataGrid.SelectedItem is not TgEfContactDto dto) return;

		NavigationService.NavigateTo(typeof(TgContactDetailsViewModel).FullName!, dto.Uid);
	}

	#endregion
}