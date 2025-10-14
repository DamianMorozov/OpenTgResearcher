namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgProxyViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
	public partial TgEfProxyDto Dto { get; set; } = default!;
    [ObservableProperty]
	public partial ObservableCollection<TgEnumProxyType> Types { get; set; } = default!;
	
    public IAsyncRelayCommand LoadDataStorageCommand { get; }
	public IAsyncRelayCommand ClearViewCommand { get; }
    public IAsyncRelayCommand SaveProxySettingsCommand { get; }

    public TgProxyViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgProxyViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgProxyViewModel))
	{
        Types = new ObservableCollection<TgEnumProxyType>(Enum.GetValues<TgEnumProxyType>());
        // Commands
        ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
		LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
        SaveProxySettingsCommand = new AsyncRelayCommand(SaveProxySettingsAsync);
	}

	#endregion

	#region Methods

	public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
	{
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        await LoadDataStorageCoreAsync();
	});

	private async Task ClearViewAsync() => 
        await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

	private async Task ClearDataStorageCoreAsync()
	{
		Dto.HostName = string.Empty;
		Dto.Port = 0;
		Dto.UserName = string.Empty;
		Dto.Password = string.Empty;
		Dto.Secret = string.Empty;
		Dto.Type = TgEnumProxyType.None;
        await Task.CompletedTask;
	}

	private async Task LoadDataStorageAsync() => 
        await ContentDialogAsync(LoadDataStorageCoreAsync, TgResourceExtensions.AskLoading(), TgEnumLoadDesktopType.Storage);

	private async Task LoadDataStorageCoreAsync()
	{
		if (!SettingsService.IsExistsAppStorage) return;
		Dto = await App.BusinessLogicManager.StorageManager.ProxyRepository.GetDtoAsync(Uid);
	}

	private async Task SaveProxySettingsAsync() => 
        await ContentDialogAsync(SaveProxySettingsCoreAsync, TgResourceExtensions.AskSettingsSave(), TgEnumLoadDesktopType.Online);

    private async Task SaveProxySettingsCoreAsync() => await LoadStorageDataAsync(async () =>
    {
        await App.BusinessLogicManager.StorageManager.ProxyRepository.SaveAsync(Dto);
        await LoadDataStorageCoreAsync();
    });

    #endregion
}
