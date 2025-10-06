namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgChatsViewModel : TgSectionViewModel
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial ObservableCollection<TgEfSourceLiteDto> Dtos { get; set; } = [];
    [ObservableProperty]
    public partial bool IsFilterBySubscribe { get; set; } = true;
    [ObservableProperty]
    public partial bool IsFilterByTitle { get; set; } = true;

    public TgChatsViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgChatsViewModel> logger)
        : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatsViewModel))
    {
        //
    }

    #endregion

    #region Methods

    private Expression<Func<TgEfSourceEntity, TgEfSourceLiteDto>> SelectDto() => item => TgEfDomainUtils.CreateNewLiteDto(item, isUidCopy: true);

    public async Task<List<TgEfSourceLiteDto>> GetListDtosAsync()
    {
        // Build base query
        var query = App.BusinessLogicManager.StorageManager.SourceRepository.GetQuery(isReadOnly: true);

        // Prepare search strings
        var raw = FilterText.Trim();
        if (raw.StartsWith('@')) raw = raw[1..];

        // Apply subscribe filter first (to reduce data set)
        if (IsFilterBySubscribe)
            query = query.Where(x => x.IsSubscribe);

        // Apply database-side filtering by Id only (to reduce data set)
        var idParsed = long.TryParse(raw, out var id) ? id : (long?)null;
        if (IsFilterById)
        {
            if (idParsed.HasValue)
                query = query.Where(x => x.Id == idParsed.Value);
        }

        // Projection to DTOs first
        var projected = query.Select(SelectDto());
        var list = await projected.ToListAsync();

        // Apply case-insensitive text search in memory (for Cyrillic and Latin)
        if (!idParsed.HasValue && !string.IsNullOrWhiteSpace(FilterText) && (IsFilterByName || IsFilterByTitle))
        {
            // Prepare search strings
            var lower = raw.ToLower(CultureInfo.InvariantCulture);
            list = [.. list
                .Where(x =>
                    (IsFilterByName && !string.IsNullOrEmpty(x.UserName) && x.UserName.Contains(lower, StringComparison.InvariantCultureIgnoreCase)) ||
                    (IsFilterByTitle && !string.IsNullOrEmpty(x.Title) && x.Title.Contains(lower, StringComparison.InvariantCultureIgnoreCase))
                )];
        }

        // Order & pagination in memory
        list = [.. list
            .OrderByDescending(x => x.IsSubscribe)
            .ThenBy(x => x.UserName)
            .ThenBy(x => x.Title)
            .Skip(PageSkip > 0 ? PageSkip : 0)
            .Take(PageTake > 0 ? PageTake : int.MaxValue)];

        // If no items, return early
        if (list.Count == 0) return list;

        // Collect ids for counting participants
        var ids = list.Select(d => d.Id).ToList();

        // Query chat users grouped by ChatId to get counts; exclude soft-deleted rows if applicable
        var counts = await App.BusinessLogicManager
            .StorageManager
            .ChatUserRepository
            .GetQuery(isReadOnly: true)
            .Where(cu => ids.Contains(cu.ChatId) && !cu.IsDeleted)
            .GroupBy(cu => cu.ChatId)
            .Select(g => new { ChatId = g.Key, Count = g.Count() })
            .ToListAsync();

        // Merge counts into DTOs; default zero when no group found
        var countsDict = counts.ToDictionary(x => x.ChatId, x => x.Count);
        foreach (var dto in list)
        {
            dto.ParticipantsCount = countsDict.TryGetValue(dto.Id, out var c) ? c : 0;
        }

        return list;
    }

    public void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not DataGrid dataGrid) return;
        if (dataGrid.SelectedItem is not TgEfSourceLiteDto sourceLiteDto) return;

        NavigationService.NavigateTo(typeof(TgChatViewModel).FullName!, sourceLiteDto.Uid);
    }

    #endregion

    #region Override methods

    protected override void ItemsClearCore() => Dtos.Clear();

    protected override async Task LazyLoadCoreAsync(bool isSearch)
    {
        if (!HasMoreItems) return;

        var newItems = await GetListDtosAsync();
        if (newItems.Count < PageTake)
            HasMoreItems = false;

        Dtos = isSearch ? [.. newItems] : [.. Dtos, .. newItems];

        PageSkip += newItems.Count;
    }

    protected override async Task AfterDataUpdateCoreAsync()
    {
        // Update loaded data statistics
        var countAll = await App.BusinessLogicManager.StorageManager.SourceRepository.GetCountAsync();
        var countFiltered = IsFilterBySubscribe 
            ? await App.BusinessLogicManager.StorageManager.SourceRepository.GetCountAsync(x => x.IsSubscribe)
            : await App.BusinessLogicManager.StorageManager.SourceRepository.GetCountAsync();
        LoadedDataStatistics =
            $"{TgResourceExtensions.GetTextBlockLoaded()} {Dtos.Count} | " +
            $"{TgResourceExtensions.GetTextBlockFiltered()} {countFiltered} | " +
            $"{TgResourceExtensions.GetTextBlockTotalAmount()} {countAll}";
    }

    protected override async Task UpdateOnlineCoreAsync() => await LoadStorageDataAsync(async () =>
    {
        var listIds = !string.IsNullOrEmpty(FilterText) ? Dtos.Select(x => x.Id).ToList() : null;
        await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.Chat, listIds);
        
        await LazyLoadAsync(isNewQuery: true, isSearch: false);
    });

    #endregion
}
