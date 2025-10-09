namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgStoriesViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

	[ObservableProperty]
	public partial ObservableCollection<TgEfStoryDto> Dtos { get; set; } = [];
	public IAsyncRelayCommand LoadDataStorageCommand { get; }
	public IAsyncRelayCommand ClearViewCommand { get; }
	public IAsyncRelayCommand DefaultSortCommand { get; }
	public IAsyncRelayCommand StartUpdateOnlineCommand { get; }

	public TgStoriesViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgStoriesViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgStoriesViewModel))
	{
		// Commands
		ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
		DefaultSortCommand = new AsyncRelayCommand(DefaultSortAsync);
		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
		StartUpdateOnlineCommand = new AsyncRelayCommand(StartUpdateOnlineAsync);
	}

	#endregion

	#region Methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
		{
			await LoadDataStorageCoreAsync();
			await ReloadUiAsync();
		});

	/// <summary> Sort data </summary>
	private void SetOrderData(ObservableCollection<TgEfStoryDto> dtos)
	{
		if (!dtos.Any()) return;
		Dtos = [.. dtos.OrderBy(x => x.DtChanged).ThenBy(x => x.FromName)];
	}

	private async Task ClearViewAsync() => 
        await ContentDialogAsync(ClearDataStorageCore, TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

    private void ClearDataStorageCore() => Dtos.Clear();

    private async Task LoadDataStorageAsync() => 
        await ContentDialogAsync(LoadDataStorageCoreAsync, TgResourceExtensions.AskLoading(), TgEnumLoadDesktopType.Storage);

	private async Task LoadDataStorageCoreAsync()
	{
		if (!SettingsService.IsExistsAppStorage) return;
		SetOrderData([.. await App.BusinessLogicManager.StorageManager.StoryRepository.GetListDtosAsync()]);
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

    #endregion
}
