namespace OpenTgResearcherDesktop.ViewModels;

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
    public IAsyncRelayCommand StartUpdateOnlineCommand { get; }
    public IAsyncRelayCommand StopUpdateOnlineCommand { get; }
    public IAsyncRelayCommand<TgEfSourceLiteDto> OpenOrEditCommand { get; }
    public IAsyncRelayCommand<TgEfSourceLiteDto> DeleteCommand { get; }

    public TgSectionViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgSectionViewModel> logger, string name) : base(loadStateService, settingsService, navigationService, logger, name)
    {
        // Commands
        ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
        LazyLoadCommand = new AsyncRelayCommand<bool>(async (isNewQuery) => await LazyLoadAsync(isNewQuery, isSearch: false), canExecute: _ => HasMoreItems && !IsLoading);
        OpenOrEditCommand = new AsyncRelayCommand<TgEfSourceLiteDto>(OpenOrEditAsync);
        DeleteCommand = new AsyncRelayCommand<TgEfSourceLiteDto>(DeleteAsync);
        SearchCommand = new AsyncRelayCommand(async () => await LazyLoadAsync(isNewQuery: false, isSearch: true));
        StartUpdateOnlineCommand = new AsyncRelayCommand(StartUpdateOnlineAsync);
        StopUpdateOnlineCommand = new AsyncRelayCommand(StopUpdateOnlineAsync);
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

    private async Task ClearViewAsync() => 
        await ContentDialogAsync(() => ClearViewCoreAsync(isFinally: true), TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

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

    private async Task StartUpdateOnlineAsync() => 
        await ContentDialogAsync(() => LoadOnlineDataAsync(async () =>
        {
            if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

            await StartUpdateOnlineCoreAsync();
        }), TgResourceExtensions.AskStartParseTelegram(), TgEnumLoadDesktopType.Online);

    private async Task StopUpdateOnlineAsync() => 
        await ContentDialogAsync(() => LoadOnlineDataAsync(async () =>
        {
            await StopUpdateOnlineCoreAsync();
        }), TgResourceExtensions.AskStopParseTelegram(), TgEnumLoadDesktopType.Online);

    private async Task OpenOrEditAsync(TgEfSourceLiteDto? sourceLiteDto)
    {
        if (!SettingsService.IsExistsAppStorage) return;
        if (sourceLiteDto is null)
        {
            await ContentDialogAsync(TgResourceExtensions.Warning(), TgResourceExtensions.RecordNotSelected());
            return;
        }

        await ContentDialogAsync(() => OpenOrEditCoreAsync(sourceLiteDto), TgResourceExtensions.AskOpenOrEdit(), TgEnumLoadDesktopType.Storage);
    }

    protected async Task OpenOrEditCoreAsync(TgEfSourceLiteDto sourceLiteDto)
    {
        NavigationService.NavigateTo(typeof(TgChatViewModel).FullName!, sourceLiteDto.Uid);
        await Task.CompletedTask;
    }

    private async Task DeleteAsync(TgEfSourceLiteDto? sourceLiteDto)
    {
        if (!SettingsService.IsExistsAppStorage) return;
        if (sourceLiteDto is null)
        {
            await ContentDialogAsync(TgResourceExtensions.Warning(), TgResourceExtensions.RecordNotSelected());
            return;
        }

        await ContentDialogAsync(() => DeleteCoreAsync(sourceLiteDto), TgResourceExtensions.AskDeleteRecord(), TgEnumLoadDesktopType.Storage);
    }

    protected async Task DeleteCoreAsync(TgEfSourceLiteDto sourceLiteDto)
    {
        await App.BusinessLogicManager.StorageManager.SourceRepository.DeleteAsync(where: x => x.Uid == sourceLiteDto.Uid);
        await OnNavigatedToAsync(null);
        await Task.CompletedTask;
    }

    #endregion

    #region Virtual methods

    protected virtual void ItemsClearCore() => throw new NotImplementedException();

    protected virtual Task LazyLoadCoreAsync(bool isSearch) => throw new NotImplementedException();

    protected virtual Task AfterDataUpdateCoreAsync() => throw new NotImplementedException();

    protected virtual Task StartUpdateOnlineCoreAsync() => throw new NotImplementedException();

    protected virtual async Task StopUpdateOnlineCoreAsync()
    {
        LoadStateService.StopHardOnlineProcessing();
        LoadStateService.StopHardDownloadProcessing();
        await Task.CompletedTask;
    }

    #endregion
}
