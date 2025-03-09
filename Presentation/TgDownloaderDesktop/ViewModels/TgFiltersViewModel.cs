// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgFiltersViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    private TgEfFilterRepository Repository { get; } = new();
	[ObservableProperty]
	public partial ObservableCollection<TgEfFilterDto> Dtos { get; set; } = [];
	public IRelayCommand LoadDataStorageCommand { get; }
	public IRelayCommand ClearDataStorageCommand { get; }
	public IRelayCommand DefaultSortCommand { get; }

	public TgFiltersViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgFiltersViewModel> logger) 
		: base(settingsService, navigationService, logger, nameof(TgFiltersViewModel))
	{
		// Commands
		ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
		DefaultSortCommand = new AsyncRelayCommand(DefaultSortAsync);
		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
	}

	#endregion

	#region Public and private methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs e) => await LoadDataAsync(async () =>
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
		var dtos = await Repository.GetListDtosAsync(take: 0, skip: 0);
		SetOrderData([.. dtos]);
	}

	private async Task DefaultSortAsync()
	{
		SetOrderData(Dtos);
		await Task.CompletedTask;
	}

	#endregion
}