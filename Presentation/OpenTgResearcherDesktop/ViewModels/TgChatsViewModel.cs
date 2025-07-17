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
	public partial ObservableCollection<TgEfSourceLiteDto> FilteredDtos { get; set; } = [];
	[ObservableProperty]
	public partial string LoadedDataStatistics { get; set; } = string.Empty;
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
    public partial int CountAll { get; set; }

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
		SearchCommand = new AsyncRelayCommand(SearchAsync);
        LazyLoadCommand = new AsyncRelayCommand(LazyLoadAsync, CanLoadMore);
        IsDisplaySensitiveCommand = new AsyncRelayCommand(IsDisplaySensitiveAsync);
    }

    #endregion

    #region Public and private methods

    private bool CanLoadMore() => HasMoreItems && !IsLoading;

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
	{
		await LoadDataStorageCoreAsync();
		await ReloadUiAsync();
	});

    private async Task IsDisplaySensitiveAsync()
    {
        foreach (var userDto in Dtos)
        {
            userDto.IsDisplaySensitiveData = IsDisplaySensitiveData;
        }
        await Task.CompletedTask;
    }

    private Expression<Func<TgEfSourceEntity, TgEfSourceLiteDto>> SelectLiteDto() => item => new TgEfSourceLiteDto().GetNewDto(item);

    public async Task<List<TgEfSourceLiteDto>> GetListLiteDtosAsync(int take, int skip, bool isReadOnly = true) => 
        take > 0
            ? await App.BusinessLogicManager.StorageManager.SourceRepository
                .GetQuery(isReadOnly)
                .OrderBy(x => x.Title)
                .OrderBy(x => x.UserName)
                .Skip(skip).Take(take)
                .Select(SelectLiteDto()).ToListAsync()
            : await App.BusinessLogicManager.StorageManager.SourceRepository
                .GetQuery(isReadOnly)
                .OrderBy(x => x.Title)
                .OrderBy(x => x.UserName)
                .Select(SelectLiteDto()).ToListAsync();

    private async Task LoadDataStorageCoreAsync()
	{
		if (!SettingsService.IsExistsAppStorage) return;

        CurrentSkip = 0;
        HasMoreItems = true;
        CountAll = 0;
        Dtos.Clear();
        FilteredDtos.Clear();
        
        await LazyLoadAsync();
	}

    public async Task LazyLoadAsync() => await LoadDataAsync(async () =>
    {
        if (IsLoading || !HasMoreItems) return;
        IsLoading = true;

        var newItems = await GetListLiteDtosAsync(PageSize, CurrentSkip);
        if (newItems.Count < PageSize)
            HasMoreItems = false;

        foreach (var item in newItems)
        {
            item.IsDisplaySensitiveData = IsDisplaySensitiveData;
            Dtos.Add(item);
        }

        CurrentSkip += newItems.Count;
        ApplyFilter();

        IsLoading = false;
        (LazyLoadCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();

        // Update loaded data statistics
        CountAll = await App.BusinessLogicManager.StorageManager.SourceRepository.GetCountAsync();
        LoadedDataStatistics = $"{TgResourceExtensions.GetTextBlockFiltered()} {FilteredDtos.Count} | {TgResourceExtensions.GetTextBlockLoaded()} {Dtos.Count} | {TgResourceExtensions.GetTextBlockTotalAmount()} {CountAll}";
    });

    public void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(FilterText))
        {
            FilteredDtos = new ObservableCollection<TgEfSourceLiteDto>(Dtos);
        }
        else
        {
            FilterText = FilterText.Trim();
            var filtered = Dtos.Where(dto =>
                dto.Id.ToString().Contains(FilterText, StringComparison.InvariantCultureIgnoreCase) ||
                dto.UserName.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase) ||
                dto.Title.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase)
                ).ToList();
            FilteredDtos = new ObservableCollection<TgEfSourceLiteDto>(filtered);
        }
    }

    private async Task ClearDataStorageAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

	private async Task ClearDataStorageCoreAsync()
	{
		Dtos.Clear();
		FilteredDtos.Clear();
        FilterText = string.Empty;
		await Task.CompletedTask;
	}

	private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline());

    private async Task UpdateOnlineCoreAsync() => await LoadDataAsync(async () =>
    {
        if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

        var chatIds = !string.IsNullOrEmpty(FilterText) ? FilteredDtos.Select(x => x.Id).ToList() : null;
        await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, TgEnumSourceType.Chat, chatIds);
        await SearchAsync();
    });

    public void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
	{
		if (sender is not DataGrid dataGrid) return;
		if (dataGrid.SelectedItem is not TgEfSourceLiteDto dto) return;

		NavigationService.NavigateTo(typeof(TgChatDetailsViewModel).FullName!, dto.Uid);
	}

	private async Task SearchAsync()
	{
		ApplyFilter();
		await Task.CompletedTask;
	}

	#endregion
}