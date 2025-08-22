// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatsViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial ObservableCollection<TgEfSourceLiteDto> Dtos { get; set; } = [];
    [ObservableProperty]
    public partial string LoadedDataStatistics { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string ChatsProgressMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int ChatsProgressCounter { get; set; }
    [ObservableProperty]
    public partial string ChatsProgressString { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int ChatsProgressCountAll { get; set; }
    [ObservableProperty]
    public partial string FilterText { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int PageSize { get; set; } = 100;
    [ObservableProperty]
    public partial int CurrentSkip { get; set; }
    [ObservableProperty]
    public partial bool HasMoreItems { get; set; } = true;
    [ObservableProperty]
    public partial bool IsLoading { get; set; }
    [ObservableProperty]
    public partial bool IsFilterBySubscribe { get; set; } = true;
    [ObservableProperty]
    public partial bool IsFilterById { get; set; } = true;
    [ObservableProperty]
    public partial bool IsFilterByUserName { get; set; } = true;
    [ObservableProperty]
    public partial bool IsFilterByTitle { get; set; } = true;

    public IRelayCommand ClearDataStorageCommand { get; }
    public IRelayCommand UpdateOnlineCommand { get; }
    public IRelayCommand SearchCommand { get; }
    public IRelayCommand LazyLoadCommand { get; }

    public TgChatsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgChatsViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgChatsViewModel))
    {
        // Commands
        ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
        UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
        LazyLoadCommand = new AsyncRelayCommand(LazyLoadAsync, CanLoadMore);
        SearchCommand = new AsyncRelayCommand(async () =>
        {
            CurrentSkip = 0;
            HasMoreItems = true;
            Dtos.Clear();
            await LazyLoadAsync();
        });
        SetDisplaySensitiveCommand = new AsyncRelayCommand(SetDisplaySensitiveAsync);
    }

    #endregion

    #region Public and private methods

    private bool CanLoadMore() => HasMoreItems && !IsLoading;

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
    {
        await LoadDataStorageCoreAsync();
        await ReloadUiAsync();
    });

    private async Task SetDisplaySensitiveAsync()
    {
        foreach (var userDto in Dtos)
        {
            userDto.IsDisplaySensitiveData = IsDisplaySensitiveData;
        }
        await Task.CompletedTask;
    }

    private Expression<Func<TgEfSourceEntity, TgEfSourceLiteDto>> SelectLiteDto() => item => new TgEfSourceLiteDto().GetNewDto(item);

    public async Task<List<TgEfSourceLiteDto>> GetListLiteDtosAsync(int take, int skip, string filterText = "", 
        bool isFilterBySubscribe = true, bool isFilterById = true, bool isFilterByUserName = true, bool isFilterByTitle = true)
    {
        var query = App.BusinessLogicManager.StorageManager.SourceRepository.GetQuery(isReadOnly: true);

        // Apply subscription filter
        if (isFilterBySubscribe)
            query = query.Where(x => x.IsSubscribe);

        // Apply text search
        if (!string.IsNullOrWhiteSpace(filterText))
        {
            var trimmed = filterText.Trim();
            var searchText = trimmed;
            var searchTextWithoutAt = trimmed.StartsWith("@") ? trimmed[1..] : trimmed;

            // Build predicate
            var predicates = new List<Expression<Func<TgEfSourceEntity, bool>>>();

            if (isFilterById)
                predicates.Add(x => EF.Functions.Like(EF.Property<string>(x, nameof(TgEfSourceEntity.Id)), $"%{searchText}%")
                    || EF.Functions.Like(EF.Property<string>(x, nameof(TgEfSourceEntity.Id)), $"%{searchTextWithoutAt}%"));
            if (isFilterByUserName)
                predicates.Add(x => EF.Functions.Like(x.UserName, $"%{searchText}%") || EF.Functions.Like(x.UserName, $"%{searchTextWithoutAt}%"));
            if (isFilterByTitle)
                predicates.Add(x => EF.Functions.Like(x.Title, $"%{searchText}%") || EF.Functions.Like(x.Title, $"%{searchTextWithoutAt}%"));
            
            if (predicates.Count > 0)
            {
                var param = Expression.Parameter(typeof(TgEfSourceEntity), "x");
                Expression? body = null;
                foreach (var p in predicates)
                {
                    var invoked = Expression.Invoke(p, param);
                    body = body == null ? invoked : Expression.OrElse(body, invoked);
                }
                if (body is not null)
                {
                    var combined = Expression.Lambda<Func<TgEfSourceEntity, bool>>(body, param);
                    query = query.Where(combined);
                }
            }
        }

        // Order & pagination
        query = query.OrderByDescending(x => x.IsSubscribe).ThenBy(x => x.UserName).ThenBy(x => x.Title);

        if (skip > 0) query = query.Skip(skip);
        if (take > 0) query = query.Take(take);

        // Projection
        return await query.Select(SelectLiteDto()).ToListAsync();
    }

    private async Task LoadDataStorageCoreAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        CurrentSkip = 0;
        HasMoreItems = true;
        Dtos.Clear();

        await LazyLoadAsync();
    }

    public async Task LazyLoadAsync() => await LoadDataAsync(async () =>
    {
        if (IsLoading || !HasMoreItems) return;
        IsLoading = true;

        var newItems = await GetListLiteDtosAsync(PageSize, CurrentSkip, FilterText, IsFilterBySubscribe, IsFilterById, IsFilterByUserName, IsFilterByTitle);
        if (newItems.Count < PageSize)
            HasMoreItems = false;

        foreach (var item in newItems)
        {
            item.IsDisplaySensitiveData = IsDisplaySensitiveData;
            Dtos.Add(item);
        }

        CurrentSkip += newItems.Count;

        IsLoading = false;
        (LazyLoadCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();

        // Update loaded data statistics
        var countAll = await App.BusinessLogicManager.StorageManager.SourceRepository.GetCountAsync();
        var countSubscribed = await App.BusinessLogicManager.StorageManager.SourceRepository.GetCountAsync(x => x.IsSubscribe);
        LoadedDataStatistics = 
            $"{TgResourceExtensions.GetTextBlockFiltered()} {Dtos.Count} | " +
            $"{TgResourceExtensions.GetTextBlockSubscribed()} {countSubscribed} | " +
            $"{TgResourceExtensions.GetTextBlockTotalAmount()} {countAll}";
    });

    private async Task ClearDataStorageAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

    private async Task ClearDataStorageCoreAsync()
    {
        CurrentSkip = 0;
        HasMoreItems = true;
        Dtos.Clear();
        FilterText = string.Empty;

        await Task.CompletedTask;
    }

    private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline());

    private async Task UpdateOnlineCoreAsync() => await LoadDataAsync(async () =>
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

        var chatIds = !string.IsNullOrEmpty(FilterText) ? Dtos.Select(x => x.Id).ToList() : null;
        await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.Chat, chatIds);
        await LazyLoadAsync();
    });

    public void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not DataGrid dataGrid) return;
        if (dataGrid.SelectedItem is not TgEfSourceLiteDto dto) return;

        NavigationService.NavigateTo(typeof(TgChatViewModel).FullName!, dto.Uid);
    }

    #endregion
}