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
    public static int PageTake => 100;
    [ObservableProperty]
    public partial bool HasMoreItems { get; set; } = true;
    [ObservableProperty]
    public partial bool IsLoading { get; set; }
    [ObservableProperty]
    public partial bool IsFilterById { get; set; } = true;
    [ObservableProperty]
    public partial bool IsFilterByName { get; set; } = true;
    [ObservableProperty]
    public partial string ChatsProgressMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int ChatsProgressCounter { get; set; }
    [ObservableProperty]
    public partial string ChatsProgressString { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int ChatsProgressCountAll { get; set; }

    public IAsyncRelayCommand ClearViewCommand { get; }
    public IAsyncRelayCommand LazyLoadCommand { get; }
    public IAsyncRelayCommand SearchCommand { get; }
    public IAsyncRelayCommand UpdateOnlineCommand { get; }
    public IAsyncRelayCommand<TgEfSourceLiteDto> OpenCommand { get; }

    public TgSectionViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgSectionViewModel> logger, string name) : base(loadStateService, settingsService, navigationService, logger, name)
    {
        // Commands
        ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
        LazyLoadCommand = new AsyncRelayCommand<bool>(async (isNewQuery) => await LazyLoadAsync(isNewQuery, isSearch: false), canExecute: _ => HasMoreItems && !IsLoading);
        OpenCommand = new AsyncRelayCommand<TgEfSourceLiteDto>(OpenAsync);
        SearchCommand = new AsyncRelayCommand(async () => await LazyLoadAsync(isNewQuery: false, isSearch: true));
        UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        await ClearViewCoreAsync(isFinally: false);
        await LazyLoadAsync(isNewQuery: false, isSearch: false);
        await ReloadUiAsync();
    });

    protected async Task LazyLoadAsync(bool isNewQuery, bool isSearch) => await LoadStorageDataAsync(async () =>
    {
        if (!SettingsService.IsExistsAppStorage) return;
        if (isNewQuery || isSearch)
            await ClearViewCoreAsync(isFinally: false);
        if (IsLoading || !HasMoreItems) return;

        try
        {
            IsLoading = true;
            await LazyLoadCoreAsync(isSearch);
        }
        finally
        {
            await AfterDataUpdateAsync();
        }
    });

    private async Task ClearViewAsync() => await ContentDialogAsync(async () => 
        await ClearViewCoreAsync(isFinally: true), TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

    protected async Task ClearViewCoreAsync(bool isFinally)
    {
        try
        {
            PageSkip = 0;
            HasMoreItems = true;
            ItemsClearCore();
        }
        finally
        {
            if (isFinally)
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

    private async Task OpenAsync(TgEfSourceLiteDto? sourceLiteDto) => await ContentDialogAsync(
        async () => await OpenCoreAsync(sourceLiteDto), TgResourceExtensions.AskOpen(), TgEnumLoadDesktopType.Storage);

    protected async Task OpenCoreAsync(TgEfSourceLiteDto? sourceLiteDto)
    {
        if (!SettingsService.IsExistsAppStorage) return;
        if (sourceLiteDto is null) return;

        NavigationService.NavigateTo(typeof(TgChatViewModel).FullName!, sourceLiteDto.Uid);
        await Task.CompletedTask;
    }

    #endregion

    #region Virtual methods

    protected virtual void ItemsClearCore() => throw new NotImplementedException();

    protected virtual Task LazyLoadCoreAsync(bool isSearch) => throw new NotImplementedException();

    protected virtual Task AfterDataUpdateCoreAsync() => throw new NotImplementedException();

    protected virtual Task UpdateOnlineCoreAsync() => throw new NotImplementedException();

    #endregion
}
