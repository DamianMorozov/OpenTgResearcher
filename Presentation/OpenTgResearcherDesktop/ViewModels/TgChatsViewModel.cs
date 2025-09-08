// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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

    public TgChatsViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgChatsViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgChatsViewModel))
    {
        //
    }

    #endregion

    #region Methods

    protected override async Task SetDisplaySensitiveAsync()
    {
        foreach (var userDto in Dtos)
        {
            userDto.IsDisplaySensitiveData = IsDisplaySensitiveData;
        }

        await Task.CompletedTask;
    }

    private Expression<Func<TgEfSourceEntity, TgEfSourceLiteDto>> SelectDto() => item => new TgEfSourceLiteDto().GetNewDto(item);

    public async Task<List<TgEfSourceLiteDto>> GetListDtosAsync()
    {
        var query = App.BusinessLogicManager.StorageManager.SourceRepository.GetQuery(isReadOnly: true);

        // Apply subscription filter
        if (IsFilterBySubscribe)
            query = query.Where(x => x.IsSubscribe);

        // Apply text search
        if (!string.IsNullOrWhiteSpace(FilterText))
        {
            var trimmed = FilterText.Trim();
            var searchText = trimmed;
            var searchTextWithoutAt = trimmed.StartsWith('@') ? trimmed[1..] : trimmed;

            // Build predicate
            var predicates = new List<Expression<Func<TgEfSourceEntity, bool>>>();

            if (IsFilterById)
                predicates.Add(x => EF.Functions.Like(EF.Property<string>(x, nameof(TgEfSourceEntity.Id)), $"%{searchText}%")
                    || EF.Functions.Like(EF.Property<string>(x, nameof(TgEfSourceEntity.Id)), $"%{searchTextWithoutAt}%"));
            if (IsFilterByUserName)
                predicates.Add(x => EF.Functions.Like(x.UserName, $"%{searchText}%") || EF.Functions.Like(x.UserName, $"%{searchTextWithoutAt}%"));
            if (IsFilterByTitle)
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

        if (PageSkip > 0) query = query.Skip(PageSkip);
        if (PageTake > 0) query = query.Take(PageTake);

        // Projection
        return await query.Select(SelectDto()).ToListAsync();
    }

    public void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not DataGrid dataGrid) return;
        if (dataGrid.SelectedItem is not TgEfSourceLiteDto dto) return;

        NavigationService.NavigateTo(typeof(TgChatViewModel).FullName!, dto.Uid);
    }

    #endregion

    #region Override methods

    protected override void ItemsClearCore() => Dtos.Clear();

    protected override async Task LazyLoadCoreAsync()
    {
        if (!HasMoreItems) return;

        var newItems = await GetListDtosAsync();
        if (newItems.Count < PageTake)
            HasMoreItems = false;

        foreach (var item in newItems)
        {
            item.IsDisplaySensitiveData = IsDisplaySensitiveData;
            Dtos.Add(item);
        }

        PageSkip += newItems.Count;
    }

    protected override async Task AfterDataUpdateCoreAsync()
    {
        // Update loaded data statistics
        var countAll = await App.BusinessLogicManager.StorageManager.SourceRepository.GetCountAsync();
        var countSubscribed = await App.BusinessLogicManager.StorageManager.SourceRepository.GetCountAsync(x => x.IsSubscribe);
        LoadedDataStatistics =
            $"{TgResourceExtensions.GetTextBlockFiltered()} {Dtos.Count} | " +
            $"{TgResourceExtensions.GetTextBlockSubscribed()} {countSubscribed} | " +
            $"{TgResourceExtensions.GetTextBlockTotalAmount()} {countAll}";
    }

    protected override async Task UpdateOnlineCoreAsync() => await LoadDataAsync(async () =>
    {
        var listIds = !string.IsNullOrEmpty(FilterText) ? Dtos.Select(x => x.Id).ToList() : null;
        await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.Chat, listIds);
        
        await LazyLoadAsync();
    });

    #endregion
}
