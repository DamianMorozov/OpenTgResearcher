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
    [ObservableProperty]
    public partial bool IsContentTab { get; set; }

    public IAsyncRelayCommand LoadDataStorageCommand { get; }
    public IAsyncRelayCommand StartUpdateOnlineCommand { get; }
    public IAsyncRelayCommand ClearViewCommand { get; }
    public IAsyncRelayCommand SaveChatSettingsCommand { get; }
    public IAsyncRelayCommand StopUpdateOnlineCommand { get; }
    public IRelayCommand ResetChatSettingsCommand { get; }
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
        ResetChatSettingsCommand = new RelayCommand(ResetChatSettings);
    }

    #endregion

    #region Methods

    /// <summary> Загружает общий черновик до открытия первой вкладки </summary>
    public override Task OnNavigatedToAsync(NavigationEventArgs? e) => InitializationTask = LoadStorageDataAsync(async () =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        await LoadDataStorageCoreAsync();
        if (Dto.Id > 0 && !HasSelectedChatOptions()) ResetChatSettingsCommand.Execute(null);
    });

    // Проверяем все переключатели загрузки, сохраняя любую выбранную пользователем комбинацию
    private bool HasSelectedChatOptions() => Dto.IsSaveMessages || Dto.IsSaveFiles || Dto.IsParsingComments || Dto.IsJoinFileNameWithMessageId
        || Dto.IsDownloadThumbnail || Dto.IsCreatingSubdirectories || Dto.IsAutoUpdate || Dto.IsRewriteMessages || Dto.IsRewriteFiles || Dto.IsFileNamingByMessage;

    // Включаем основные опции в общем черновике без изменения каталога, позиции, потоков и остальных переключателей
    private void ResetChatSettings()
    {
        if (Dto.Id <= 0 || Dto.Uid == Guid.Empty) return;
        Dto.IsSaveMessages = true;
        Dto.IsSaveFiles = true;
        Dto.IsParsingComments = true;
        Dto.IsJoinFileNameWithMessageId = true;
        Dto.IsDownloadThumbnail = true;
        Dto.IsCreatingSubdirectories = true;
        Dto.IsAutoUpdate = true;
    }

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
                using var reader = IsOnlineProcessing ? new TgEfSourceRepository() : null;
                var sources = reader ?? App.BusinessLogicManager.StorageManager.SourceRepository;
                Dto = await sources.GetDtoAsync(x => x.Uid == Uid);
                DiscussionDto = await sources.FindCommentDtoSourceAsync(Dto.Id);
            
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
            // Сохраняем общий черновик до обращения к Telegram без повторного диалога подтверждения
            await SaveChatSettingsCoreAsync(isLoadDataStorage: false);
            try
            {
                if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;

                await App.BusinessLogicManager.ConnectClient.ParseChatAsync(DownloadSettings);
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
            {
                // Прерывание пользователем завершает команду без сообщения об ошибке
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
