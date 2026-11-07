namespace OpenTgResearcherDesktop.ViewModels;

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

    // Читаем таблицы отдельной службой, не занимая контексты парсера
    private async Task LoadStorageTableDtosAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        using var reader = new TgStorageService();
        StorageTableDtos = await reader.LoadStorageTableDtosAsync(
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
