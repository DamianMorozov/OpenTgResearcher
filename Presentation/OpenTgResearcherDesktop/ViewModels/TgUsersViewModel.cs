namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgUsersViewModel : TgSectionViewModel
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial ObservableCollection<long> ListIds { get; set; } = [];
    [ObservableProperty]
    public partial ObservableCollection<TgEfUserDto> Dtos { get; set; } = [];
    [ObservableProperty]
    public partial bool IsFilterByContacts { get; set; } = true;
    [ObservableProperty]
    public partial bool IsFilterByPhone { get; set; } = true;

    public TgUsersViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILoadStateService loadStateService, ILogger<TgUsersViewModel> logger)
        : base(settingsService, navigationService, loadStateService, logger, nameof(TgUsersViewModel))
    {
        //
    }

    #endregion

    #region Methods

    private Expression<Func<TgEfUserEntity, TgEfUserDto>> SelectDto() => item => new TgEfUserDto().Copy(item, isUidCopy: true);

    private async Task<List<TgEfUserDto>> GetListDtosAsync()
    {
        var query = App.BusinessLogicManager.StorageManager.UserRepository.GetQuery(isReadOnly: true);

        // Apply filter
        if (IsFilterByContacts)
            query = query.Where(x => x.IsContact);

        // Apply text search
        if (!string.IsNullOrWhiteSpace(FilterText))
        {
            var trimmed = FilterText.Trim();
            var searchText = trimmed;
            var searchTextWithoutAt = trimmed.StartsWith('@') ? trimmed[1..] : trimmed;

            // Build predicate
            var predicates = new List<Expression<Func<TgEfUserEntity, bool>>>();

            if (IsFilterById)
                predicates.Add(x => EF.Functions.Like(EF.Property<string>(x, nameof(TgEfUserEntity.Id)), $"%{searchText}%")
                    || EF.Functions.Like(EF.Property<string>(x, nameof(TgEfUserEntity.Id)), $"%{searchTextWithoutAt}%"));
            if (IsFilterByName)
            {
                predicates.Add(x => EF.Functions.Like(x.UserName, $"%{searchText}%") || EF.Functions.Like(x.UserName, $"%{searchTextWithoutAt}%"));
                predicates.Add(x => EF.Functions.Like(x.FirstName, $"%{searchText}%") || EF.Functions.Like(x.FirstName, $"%{searchTextWithoutAt}%"));
                predicates.Add(x => EF.Functions.Like(x.LastName, $"%{searchText}%") || EF.Functions.Like(x.LastName, $"%{searchTextWithoutAt}%"));
            }
            if (IsFilterByPhone)
            {
                predicates.Add(x => EF.Functions.Like(x.LastName, $"%{searchText}%") || EF.Functions.Like(x.PhoneNumber, $"%{searchTextWithoutAt}%"));
            }

            if (predicates.Count > 0)
            {
                var param = Expression.Parameter(typeof(TgEfUserEntity), "x");
                Expression? body = null;
                foreach (var p in predicates)
                {
                    var invoked = Expression.Invoke(p, param);
                    body = body == null ? invoked : Expression.OrElse(body, invoked);
                }
                if (body is not null)
                {
                    var combined = Expression.Lambda<Func<TgEfUserEntity, bool>>(body, param);
                    query = query.Where(combined);
                }
            }
        }

        // Order & pagination
        query = query.OrderByDescending(x => x.IsContact).ThenBy(x => x.FirstName).ThenBy(x => x.LastName).ThenBy(x => x.UserName);

        if (PageSkip > 0) query = query.Skip(PageSkip);
        if (PageTake > 0) query = query.Take(PageTake);

        // Projection
        return await query.Select(SelectDto()).ToListAsync();
    }

    public void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not DataGrid dataGrid) return;
        if (dataGrid.SelectedItem is not TgEfUserDto dto) return;

        NavigationService.NavigateTo(typeof(TgUserDetailsViewModel).FullName!, dto.Uid);
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
        var countAll = await App.BusinessLogicManager.StorageManager.UserRepository.GetCountAsync();
        var countFiltered = IsFilterByContacts
            ? await App.BusinessLogicManager.StorageManager.UserRepository.GetCountAsync(x => x.IsContact)
            : await App.BusinessLogicManager.StorageManager.UserRepository.GetCountAsync();
        LoadedDataStatistics =
            $"{TgResourceExtensions.GetTextBlockLoaded()} {Dtos.Count} | " +
            $"{TgResourceExtensions.GetTextBlockFiltered()} {countFiltered} | " +
            $"{TgResourceExtensions.GetTextBlockTotalAmount()} {countAll}";
    }

    protected override async Task UpdateOnlineCoreAsync() => await LoadStorageDataAsync(async () =>
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

        var listIds = !string.IsNullOrEmpty(FilterText) ? Dtos.Select(x => x.Id).ToList() : null;
        if (IsFilterByContacts)
        {
            await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.UserContact, listIds);
        }
        else
        {
            await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.UserContact, listIds);
            await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.UserOnly, listIds);
        }
        
        await LazyLoadAsync(isNewQuery: true, isSearch: false);
    });

    #endregion
}
