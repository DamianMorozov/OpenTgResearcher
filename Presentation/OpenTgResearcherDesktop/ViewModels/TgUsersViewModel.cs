// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgUsersViewModel : TgSectionViewModel
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial TgEnumSourceType SourceType { get; set; } = TgEnumSourceType.Default;
    [ObservableProperty]
    public partial ObservableCollection<long> ListIds { get; set; } = [];
    [ObservableProperty]
    public partial ObservableCollection<TgEfUserDto> Dtos { get; set; } = [];
    [ObservableProperty]
    public partial bool IsFilterByFirstName { get; set; } = true;
    [ObservableProperty]
    public partial bool IsFilterByLastName { get; set; } = true;

    public TgUsersViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgUsersViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgUsersViewModel))
    {
        //
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        if (e?.Parameter is string paramString)
        {
            if (Enum.TryParse<TgEnumSourceType>(paramString, out var sourceType))
                SourceType = sourceType;
        }

        await base.OnNavigatedToAsync(e);
    });

    protected override async Task SetDisplaySensitiveAsync()
    {
        foreach (var dto in Dtos)
        {
            dto.IsDisplaySensitiveData = IsDisplaySensitiveData;
        }

        await Task.CompletedTask;
    }

    private Expression<Func<TgEfUserEntity, TgEfUserDto>> SelectDto() => item => new TgEfUserDto().GetNewDto(item);

    public async Task<List<TgEfUserDto>> GetListDtosAsync()
    {
        var query = App.BusinessLogicManager.StorageManager.UserRepository.GetQuery(isReadOnly: true);

        // Apply subscription filter
        if (SourceType == TgEnumSourceType.Contact)
            query = query.Where(x => x.IsContact);
        else if (SourceType == TgEnumSourceType.User)
            query = query.Where(x => !x.IsContact);

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
            if (IsFilterByUserName)
                predicates.Add(x => EF.Functions.Like(x.UserName, $"%{searchText}%") || EF.Functions.Like(x.UserName, $"%{searchTextWithoutAt}%"));
            if (IsFilterByFirstName)
                predicates.Add(x => EF.Functions.Like(x.FirstName, $"%{searchText}%") || EF.Functions.Like(x.FirstName, $"%{searchTextWithoutAt}%"));
            if (IsFilterByLastName)
                predicates.Add(x => EF.Functions.Like(x.LastName, $"%{searchText}%") || EF.Functions.Like(x.LastName, $"%{searchTextWithoutAt}%"));

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

    protected override async Task UpdateOnlineCoreAsync() => await LoadStorageDataAsync(async () =>
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

        var listIds = !string.IsNullOrEmpty(FilterText) ? Dtos.Select(x => x.Id).ToList() : null;
        if (SourceType == TgEnumSourceType.Contact)
            await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.Contact, listIds);
        else if (SourceType == TgEnumSourceType.User)
            await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.User, listIds);
        await LazyLoadAsync();
    });

    #endregion
}
