// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgStorageViewModel : TgPageViewModelBase
{
    #region Public and private fields, properties, constructor

    [ObservableProperty]
    public partial ObservableCollection<TgStorageTableDto> Dtos { get; set; } = [];
    [ObservableProperty]
    public partial bool IsStorageLoadDataShow { get; set; }
    [ObservableProperty]
    public partial bool IsStorageSetupShow { get; set; }
    [ObservableProperty]
    public partial bool IsStorageAdvancedSettingShow { get; set; }
    [ObservableProperty]
    public partial bool IsStorageClearingShow { get; set; }

    public IRelayCommand StorageLoadDataCommand { get; }
    public IRelayCommand StorageSetupCommand { get; }
    public IRelayCommand StorageAdvancedSettingCommand { get; }
    public IRelayCommand StorageClearingCommand { get; }
    public IRelayCommand StorageChatsCommand { get; }
    public IRelayCommand StorageFiltersCommand { get; }

    public TgStorageViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILogger<TgStorageViewModel> logger)
        : base(settingsService, navigationService, logger, nameof(TgStorageViewModel))
    {
        // Commands
        StorageLoadDataCommand = new AsyncRelayCommand(LoadDataStorageAsync);
        StorageSetupCommand = new AsyncRelayCommand(StorageSetupAsync);
        StorageAdvancedSettingCommand = new AsyncRelayCommand(StorageAdvancedSettingsAsync);
        StorageClearingCommand = new AsyncRelayCommand(StorageClearingAsync);
        StorageChatsCommand = new AsyncRelayCommand(StorageChatsAsync);
        StorageFiltersCommand = new AsyncRelayCommand(StorageFiltersAsync);
    }

    #endregion

    #region Public and private methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadDataAsync(async () =>
        {
            await LoadDataStorageAsync();
            await ReloadUiAsync();
        });

    /// <summary> Sort data </summary>
    private void SetOrderData(ObservableCollection<TgStorageTableDto> dtos)
    {
        if (!dtos.Any()) return;
        Dtos = [.. dtos.OrderBy(x => x.Name)];
    }

    private async Task LoadDataStorageAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        try
        {
            var appDtos = new TgStorageTableDto(TgResourceExtensions.GetTableNameApps(),
            await App.BusinessLogicManager.StorageManager.AppRepository.GetListCountAsync());
            var chatsDtos = new TgStorageTableDto(TgResourceExtensions.GetTableNameChats(),
                await App.BusinessLogicManager.StorageManager.SourceRepository.GetListCountAsync());
            var contactsDtos = new TgStorageTableDto(TgResourceExtensions.GetTableNameContacts(),
                await App.BusinessLogicManager.StorageManager.ContactRepository.GetListCountAsync());
            var documentsDtos = new TgStorageTableDto(TgResourceExtensions.GetTableNameDocuments(),
                await App.BusinessLogicManager.StorageManager.DocumentRepository.GetListCountAsync());
            var filtersDtos = new TgStorageTableDto(TgResourceExtensions.GetTableNameFilters(),
                await App.BusinessLogicManager.StorageManager.FilterRepository.GetListCountAsync());
            var messagesDtos = new TgStorageTableDto(TgResourceExtensions.GetTableNameMessages(),
                await App.BusinessLogicManager.StorageManager.MessageRepository.GetListCountAsync());
            var proxiesDtos = new TgStorageTableDto(TgResourceExtensions.GetTableNameProxies(),
                await App.BusinessLogicManager.StorageManager.ProxyRepository.GetListCountAsync());
            var storiesDtos = new TgStorageTableDto(TgResourceExtensions.GetTableNameStories(),
                await App.BusinessLogicManager.StorageManager.StoryRepository.GetListCountAsync());
            var versionsDtos = new TgStorageTableDto(TgResourceExtensions.GetTableNameVersions(),
                await App.BusinessLogicManager.StorageManager.VersionRepository.GetListCountAsync());
            SetOrderData([appDtos, chatsDtos, contactsDtos, documentsDtos, filtersDtos, messagesDtos, proxiesDtos, storiesDtos, versionsDtos]);
        }
        finally
        {
            IsStorageLoadDataShow = true;
            IsStorageSetupShow = false;
            IsStorageAdvancedSettingShow = false;
            IsStorageClearingShow = false;
        }
    }

    private async Task StorageSetupAsync()
    {
        try
        {
            await Task.Delay(250);
        }
        finally
        {
            IsStorageLoadDataShow = false;
            IsStorageSetupShow = true;
            IsStorageAdvancedSettingShow = false;
            IsStorageClearingShow = false;
        }
    }

    private async Task StorageAdvancedSettingsAsync()
    {
        try
        {
            await Task.CompletedTask;
        }
        finally
        {
            IsStorageLoadDataShow = false;
            IsStorageSetupShow = false;
            IsStorageAdvancedSettingShow = true;
            IsStorageClearingShow = false;
        }
    }

    private async Task StorageClearingAsync()
    {
        try
        {
            await Task.CompletedTask;
        }
        finally
        {
            IsStorageLoadDataShow = false;
            IsStorageSetupShow = false;
            IsStorageAdvancedSettingShow = false;
            IsStorageClearingShow = true;
        }
    }

    private async Task StorageChatsAsync()
    {
        await Task.CompletedTask;
    }

    private async Task StorageFiltersAsync()
    {
        await Task.CompletedTask;
    }

    #endregion
}