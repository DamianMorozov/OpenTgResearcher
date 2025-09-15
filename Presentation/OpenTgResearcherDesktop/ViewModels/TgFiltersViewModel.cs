namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgFiltersViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

	[ObservableProperty]
	public partial ObservableCollection<TgEfFilterDto> Dtos { get; set; } = [];
	public IAsyncRelayCommand LoadDataStorageCommand { get; }
	public IAsyncRelayCommand ClearViewCommand { get; }
	public IAsyncRelayCommand DefaultSortCommand { get; }

	public TgFiltersViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgFiltersViewModel> logger) 
		: base(settingsService, navigationService, logger, nameof(TgFiltersViewModel))
	{
		// Commands
		ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
		DefaultSortCommand = new AsyncRelayCommand(DefaultSortAsync);
		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
	}

	#endregion

	#region Methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
	{
		await LoadDataStorageCoreAsync();
		await ReloadUiAsync();
	});

	/// <summary> Sort data </summary>
	private void SetOrderData(ObservableCollection<TgEfFilterDto> dtos)
	{
		if (!dtos.Any()) return;
		Dtos = [.. dtos.OrderBy(x => x.Name)];
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
		var dtos = await App.BusinessLogicManager.StorageManager.FilterRepository.GetListDtosAsync();
		SetOrderData([.. dtos]);
	}

	private async Task DefaultSortAsync()
	{
		SetOrderData(Dtos);
		await Task.CompletedTask;
	}

	#endregion
}
