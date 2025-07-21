// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgUsersViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial TgEnumSourceType SourceType { get; set; } = TgEnumSourceType.Default;
    [ObservableProperty]
    public partial ObservableCollection<long> ChatIds { get; set; } = [];
    [ObservableProperty]
    public partial ObservableCollection<TgEfUserDto> Dtos { get; set; } = [];
    public IRelayCommand ClearDataStorageCommand { get; }
    public IRelayCommand DefaultSortCommand { get; }
    public IRelayCommand UpdateOnlineCommand { get; }

    public TgUsersViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgUsersViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgUsersViewModel))
    {
        // Commands
        ClearDataStorageCommand = new AsyncRelayCommand(ClearDataStorageAsync);
        DefaultSortCommand = new AsyncRelayCommand(DefaultSortAsync);
        UpdateOnlineCommand = new AsyncRelayCommand(UpdateOnlineAsync);
        SetDisplaySensitiveCommand = new AsyncRelayCommand(SetDisplaySensitiveAsync);
    }

    #endregion

    #region Public and private methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
        {
            if (e?.Parameter is string paramString)
            {
                if (Enum.TryParse<TgEnumSourceType>(paramString, out var sourceType))
                    SourceType = sourceType;
            }
            await LoadDataStorageCoreAsync();
            await ReloadUiAsync();
        });

    private async Task SetDisplaySensitiveAsync()
    {
        foreach (var dto in Dtos)
        {
            dto.IsDisplaySensitiveData = IsDisplaySensitiveData;
        }
        await Task.CompletedTask;
    }

    /// <summary> Sort data </summary>
    private void SetOrderData(ObservableCollection<TgEfUserDto> dtos)
    {
        if (!dtos.Any()) return;

        // Order: 1 - local user, 2 - name without username, 3 - username
        Dtos = [.. dtos
            .OrderByDescending(x => x.Id == App.BusinessLogicManager.ConnectClient.Client?.UserId)
            .ThenBy(x => x.UserName)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
        ];
    }

    private async Task ClearDataStorageAsync() => await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

    private async Task ClearDataStorageCoreAsync()
    {
        Dtos.Clear();
        await Task.CompletedTask;
    }

    public async Task LoadDataStorageCoreAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;
        
        List<TgEfUserDto> users = [];
        if (SourceType == TgEnumSourceType.Contact)
            users = await App.BusinessLogicManager.StorageManager.UserRepository.GetListDtosAsync(0, 0, x => x.IsContact);
        if (SourceType == TgEnumSourceType.User)
            users = await App.BusinessLogicManager.StorageManager.UserRepository.GetListDtosAsync(0, 0, x => !x.IsContact);
        SetOrderData([.. users]);
    }

    private async Task DefaultSortAsync()
    {
        SetOrderData(Dtos);
        await Task.CompletedTask;
    }

    private async Task UpdateOnlineAsync() => await ContentDialogAsync(UpdateOnlineCoreAsync, TgResourceExtensions.AskUpdateOnline());

    private async Task UpdateOnlineCoreAsync()
    {
        await LoadDataAsync(async () =>
        {
            if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
            await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(DownloadSettings, SourceType, [.. ChatIds]);
            await LoadDataStorageCoreAsync();
        });
    }

    public void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not DataGrid dataGrid) return;
        if (dataGrid.SelectedItem is not TgEfUserDto dto) return;

        NavigationService.NavigateTo(typeof(TgUserDetailsViewModel).FullName!, dto.Uid);
    }

    #endregion
}