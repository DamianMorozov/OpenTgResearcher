namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgChatDownloadViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto DownloadDto { get; set; } = new();
    [ObservableProperty]
    public partial string ChatProgressMessage { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string ChatProgressInfo { get; set; } = string.Empty;

    public IRelayCommand OpenNetworkActivityCommand { get; }

    public TgChatDownloadViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgChatDownloadViewModel> logger)
        : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatDownloadViewModel))
    {
        // Команда перехода к общей активности остаётся доступной во время парсинга
        OpenNetworkActivityCommand = new RelayCommand(OpenNetworkActivity);
    }

    #endregion

    #region Methods

    /// <summary> Подготавливает настройки загрузки до обращения к прогрессу чата </summary>
    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => 
        await LoadStorageDataAsync(async () =>
        {
            DownloadDto = e?.Parameter as TgEfSourceDto ?? new();
            Uid = e?.Parameter is Guid uid ? uid : DownloadDto.Uid;
            if (SettingsService.IsExistsAppStorage && e?.Parameter is not TgEfSourceDto)
                DownloadDto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid);
            App.VmLocator.ClearChatViewModel();
            App.VmLocator.ClearStateDownloadingChat();
            ChatProgressMessage = $"{TgDataUtils.CalcSourceProgress(DownloadDto.Count, DownloadDto.FirstId):00.00} %";
        });

    // Открываем общую страницу сетевых операций без остановки текущего парсинга
    private void OpenNetworkActivity() => NavigationService.NavigateTo(typeof(TgNetworkActivityViewModel).FullName!);

    #endregion
}
