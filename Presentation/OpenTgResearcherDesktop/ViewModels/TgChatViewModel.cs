namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgChatViewModel : TgPageViewModelBase
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Guid Uid { get; set; } = Guid.Empty!;
    [ObservableProperty]
    public partial TgEfSourceDto Dto { get; set; } = new();
    [ObservableProperty]
    public partial TgEfSourceDto DiscussionDto { get; set; } = new();
    [ObservableProperty]
    public partial Frame ChatDetailsFrame { get; set; } = default!;

    public IAsyncRelayCommand LoadDataStorageCommand { get; }
    public IAsyncRelayCommand StartUpdateOnlineCommand { get; }
    public IAsyncRelayCommand ClearViewCommand { get; }
    public IAsyncRelayCommand SaveChatSettingsCommand { get; }
    public IAsyncRelayCommand StopUpdateOnlineCommand { get; }
    public Task InitializationTask { get; private set; } = Task.CompletedTask;

    public TgChatViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService, 
        ILogger<TgChatViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatViewModel))
    {
        // Команды страницы чата
        LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageCoreAsync);
        StartUpdateOnlineCommand = new AsyncRelayCommand(StartUpdateOnlineAsync);
        ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
        SaveChatSettingsCommand = new AsyncRelayCommand(SaveChatSettingsAsync);
        StopUpdateOnlineCommand = new AsyncRelayCommand(StopUpdateOnlineAsync);
    }

    #endregion

    #region Methods

    /// <summary> Загружает общий черновик до открытия первой вкладки </summary>
    public override Task OnNavigatedToAsync(NavigationEventArgs? e) => InitializationTask = LoadStorageDataAsync(async () =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        await LoadDataStorageCoreAsync();
    });

    private async Task ClearViewAsync() => 
        await ContentDialogAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear(), TgEnumLoadDesktopType.Storage);

    // Очищаем форму и общий черновик открытой вкладки
    private async Task ClearDataStorageCoreAsync()
    {
        Dto = new();
        DiscussionDto = new();
        IsEmptyData = true;
        await UpdateDetailsAsync();
    }

    // Читаем настройки чата и обновляем модель открытой вкладки
    private async Task LoadDataStorageCoreAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;

        await TgDesktopUtils.InvokeOnUIThreadAsync(async () => {
            try
            {
                Dto = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == Uid);
                DiscussionDto = await App.BusinessLogicManager.StorageManager.SourceRepository.FindCommentDtoSourceAsync(Dto.Id);
            
                await UpdateDetailsAsync();
                IsEmptyData = Dto.Id <= 0;
            }
            finally
            {
                await ReloadUiAsync();
            }
        });
    }

    // Обновляем общий черновик открытой вкладки без повторной навигации
    private async Task UpdateDetailsAsync()
    {
        if (ChatDetailsFrame is null) return;
        var pageViewModel = await ChatDetailsFrame.GetPageViewModelAsync();
        if (pageViewModel is TgChatSettingsViewModel settings)
        {
            settings.Uid = Uid;
            settings.Dto = Dto;
            settings.DiscussionDto = DiscussionDto;
            settings.IsDiscussionDtoExists = DiscussionDto.Id > 0;
        }
        else if (pageViewModel is TgChatDownloadViewModel download)
        {
            download.Uid = Uid;
            download.DownloadDto = Dto;
        }
    }

    private async Task StartUpdateOnlineAsync() => 
        await ContentDialogAsync(StartUpdateOnlineCoreAsync, TgResourceExtensions.AskStartParseTelegram(), TgEnumLoadDesktopType.Online);

    private async Task StartUpdateOnlineCoreAsync() => 
        await LoadOnlineDataAsync(async () =>
        {
            try
            {
                if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

                await SaveChatSettingsCoreAsync(isLoadDataStorage: false);

                await App.BusinessLogicManager.ConnectClient.ParseChatAsync(DownloadSettings);
            }
            finally
            {
                await LoadDataStorageCoreAsync();
            }
        });

    public async Task SaveChatSettingsAsync() => 
        await ContentDialogAsync(() => SaveChatSettingsCoreAsync(isLoadDataStorage: true), TgResourceExtensions.AskSettingsSave(), TgEnumLoadDesktopType.Storage);

    // Проверяем выбранный чат и сохраняем его настройки
    private async Task SaveChatSettingsCoreAsync(bool isLoadDataStorage)
    {
        try
        {
            if (Dto.Uid == Guid.Empty || Dto.Id <= 0)
                throw new InvalidOperationException("Выберите чат в списке «Чаты» перед сохранением настроек");
            // Сохраняем общий черновик настроек текущего чата
            Dto.DtChanged = DateTime.UtcNow;
            DownloadSettings.SourceVm.Dto = Dto;
            await App.BusinessLogicManager.StorageManager.SourceRepository.SaveAsync(Dto);
        }
        finally
        {
            if (isLoadDataStorage)
                await LoadDataStorageCoreAsync();
        }
    }

    private async Task StopUpdateOnlineAsync() => 
        await ContentDialogAsync(StopUpdateOnlineCoreAsync, TgResourceExtensions.AskStopParseTelegram(), TgEnumLoadDesktopType.Online);

    private async Task StopUpdateOnlineCoreAsync()
    {
        LoadStateService.StopHardOnlineProcessing();
        LoadStateService.StopHardDownloadProcessing();
        await Task.CompletedTask;
    }

    #endregion
}
