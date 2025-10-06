namespace OpenTgResearcherDesktop.ViewModels;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgStorageTablesViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial ObservableCollection<TgStorageTableDto> StorageTableDtos { get; set; } = [];

    public TgStorageTablesViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgStorageTablesViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgStorageTablesViewModel))
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
