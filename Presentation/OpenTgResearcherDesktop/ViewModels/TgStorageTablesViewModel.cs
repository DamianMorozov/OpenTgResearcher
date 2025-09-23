namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgStorageTablesViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial ObservableCollection<TgStorageTableDto> StorageTableDtos { get; set; } = [];

    public TgStorageTablesViewModel(ITgSettingsService settingsService, INavigationService navigationService, ILoadStateService loadStateService, 
        ILogger<TgStorageTablesViewModel> logger) : base(settingsService, navigationService, loadStateService, logger, nameof(TgStorageTablesViewModel))
    {
        //
    }

    #endregion

    #region Methods

    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await LoadStorageDataAsync(async () =>
    {
        await LoadStorageTableDtosAsync();
        await ReloadUiAsync();
    });

    /// <summary> Load storage table dtos </summary>
    private async Task LoadStorageTableDtosAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        StorageTableDtos = await App.BusinessLogicManager.StorageManager.LoadStorageTableDtosAsync(
            TgResourceExtensions.GetTableNameApps(),
            TgResourceExtensions.GetTableNameChats(),
            TgResourceExtensions.GetTableNameUsers(),
            TgResourceExtensions.GetTableNameChatUsers(),
            TgResourceExtensions.GetTableNameDocuments(),
            TgResourceExtensions.GetTableNameFilters(),
            TgResourceExtensions.GetTableNameMessages(),
            TgResourceExtensions.GetTableNameProxies(),
            TgResourceExtensions.GetTableNameStories(),
            TgResourceExtensions.GetTableNameVersions());
    }

    #endregion
}
