namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgChatViewModel : TgPageViewModelBase
{
    private readonly HashSet<long> _startingChatIds = [];
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
        StartUpdateOnlineCommand = new AsyncRelayCommand(StartUpdateOnlineAsync, AsyncRelayCommandOptions.AllowConcurrentExecutions);
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

    // Запускаем чат напрямую, сохраняя обработку ошибок без диалога подтверждения
    private async Task StartUpdateOnlineAsync()
    {
        try { await StartUpdateOnlineCoreAsync(); }
        catch (OperationCanceledException ex) when (ex.CancellationToken.IsCancellationRequested) { }
        catch (Exception ex) { LogError(ex); }
    }

    private async Task StartUpdateOnlineCoreAsync()
    {
        var source = Dto;
        if (LoadStateService.NetworkActivity.Operations.Any(x => x.Kind == TgNetworkOperationKind.Parsing
            && x.ChatId == source.Id && x.State == TgNetworkOperationState.Running) || !_startingChatIds.Add(source.Id)) return;
        try
        {
            // Фиксируем выбранный чат до первого ожидания, чтобы навигация не перенаправила запуск
            var settings = new TgDownloadSettingsViewModel();
            settings.SourceVm.Dto = TgEfDomainUtils.CreateNewDto(source, isUidCopy: true);
            await LoadOnlineDataAsync(async () =>
            {
                // Сохраняем снимок до обращения к Telegram без повторного диалога подтверждения
                if (source.Uid == Guid.Empty || source.Id <= 0)
                    throw new InvalidOperationException("Выберите чат в списке «Чаты» перед сохранением настроек");
                settings.SourceVm.Dto.DtChanged = DateTime.UtcNow;
                using var writer = new TgEfSourceRepository();
                await writer.SaveAsync(settings.SourceVm.Dto);
                try
                {
                    if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
                    await App.BusinessLogicManager.ConnectClient.ParseChatAsync(settings);
                }
                catch (OperationCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
                {
                    // Прерывание пользователем завершает команду без сообщения об ошибке
                }
            });
        }
        finally
        {
            _startingChatIds.Remove(source.Id);
            if (Uid == source.Uid) await LoadDataStorageCoreAsync();
        }
    }

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
            // Сохраняем настройки независимо от контекстов выполняющихся парсингов и сетевых операций
            using var writer = new TgEfSourceRepository();
            await writer.SaveAsync(Dto);
        }
        finally
        {
            if (isLoadDataStorage)
                await LoadDataStorageCoreAsync();
        }
    }

    private async Task StopUpdateOnlineAsync() => 
        await ContentDialogAsync(StopUpdateOnlineCoreAsync, "AskStopChatParsing".GetLocalizedResource(), TgEnumLoadDesktopType.Online);

    private async Task StopUpdateOnlineCoreAsync()
    {
        LoadStateService.NetworkActivity.CancelParsing(Dto.Id);
        await Task.CompletedTask;
    }

    #endregion
}
