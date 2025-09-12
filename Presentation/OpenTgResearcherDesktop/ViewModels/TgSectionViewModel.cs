namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public abstract partial class TgSectionViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial string LoadedDataStatistics { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string ItemsProgressMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int ItemsProgressCounter { get; set; }
    [ObservableProperty]
    public partial string ItemsProgressString { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int ItemsProgressCountAll { get; set; }
    [ObservableProperty]
    public partial string FilterText { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int PageSkip { get; set; } = 0;
    [ObservableProperty]
    public partial int PageTake { get; set; } = 100;
    [ObservableProperty]
    public partial bool HasMoreItems { get; set; } = true;
    [ObservableProperty]
    public partial bool IsLoading { get; set; }
    [ObservableProperty]
    public partial bool IsFilterById { get; set; } = true;
    [ObservableProperty]
    public partial bool IsFilterByUserName { get; set; } = true;
    [ObservableProperty]
    public partial string ChatsProgressMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int ChatsProgressCounter { get; set; }
    [ObservableProperty]
    public partial string ChatsProgressString { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int ChatsProgressCountAll { get; set; }

    public IRelayCommand ClearViewCommand { get; }
    public IRelayCommand UpdateOnlineCommand { get; }
    public IRelayCommand SearchCommand { get; }
    public IRelayCommand LazyLoadCommand { get; }

    public TgSectionViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgSectionViewModel> logger, string name)
        : base(settingsService, navigationService, logger, name)
    {
        // Commands
        ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
        LazyLoadCommand = new AsyncRelayCommand(LazyLoadAsync, () => HasMoreItems && !IsLoading);
        SearchCommand = new AsyncRelayCommand(LazyLoadWrapperAsync);
        UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        await LoadDataStorageAsync();
        await ReloadUiAsync();
    });

    protected async Task LoadDataStorageAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        PageSkip = 0;
        HasMoreItems = true;
        ItemsClearCore();

        await LazyLoadAsync();
    }

    private async Task LazyLoadWrapperAsync()
    {
        PageSkip = 0;
        HasMoreItems = true;
        ItemsClearCore();

        await LazyLoadAsync();
    }

    protected async Task LazyLoadAsync() => await LoadStorageDataAsync(async () =>
    {
        if (IsLoading || !HasMoreItems) return;

        try
        {
            IsLoading = true;
            await LazyLoadCoreAsync();
        }
        finally
        {
            await AfterDataUpdateAsync();
        }
    });

    private async Task ClearViewAsync() => await ContentDialogAsync(ClearViewCoreAsync, TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

    private async Task ClearViewCoreAsync()
    {
        try
        {
            PageSkip = 0;
            HasMoreItems = true;
            FilterText = string.Empty;
            ItemsClearCore();
        }
        finally
        {
            await AfterDataUpdateAsync();
        }
    }

    private async Task AfterDataUpdateAsync()
    {
        IsLoading = false;
        (LazyLoadCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();

        await AfterDataUpdateCoreAsync();

        await Task.CompletedTask;
    }

    private async Task UpdateOnlineAsync() => await ContentDialogAsync(() => LoadStorageDataAsync(async () =>
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

        await UpdateOnlineCoreAsync();
    }), TgResourceExtensions.AskUpdateOnline(), TgEnumLoadDesktopType.Online);

    #endregion

    #region Virtual methods

    protected virtual void ItemsClearCore() => throw new NotImplementedException();

    protected virtual Task LazyLoadCoreAsync() => throw new NotImplementedException();

    protected virtual Task AfterDataUpdateCoreAsync() => throw new NotImplementedException();

    protected virtual Task UpdateOnlineCoreAsync() => throw new NotImplementedException();

    #endregion
}
