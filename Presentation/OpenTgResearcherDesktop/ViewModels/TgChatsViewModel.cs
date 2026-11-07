namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgChatsViewModel : TgSectionViewModel
{
    private List<OpenTgResearcherDesktop.Models.TgChatRegistryRow> _pageRows = [];
    private readonly HashSet<long> _parsingChatIds = [];
    private bool _isLoadingNextPage;
    private CancellationTokenSource _directorySizesCancellation = new();
    internal Task DirectorySizesTask { get; private set; } = Task.CompletedTask;
    public string ChatTitleHeader => "RegistryChat".GetLocalizedResource();
    public string ChatCountHeader => "RegistryMessages".GetLocalizedResource();
    public string ChatSizeHeader => "RegistryDirectorySize".GetLocalizedResource();
    public string ChatProgressHeader => "RegistryParsing".GetLocalizedResource();
    public string ChatUpdatedHeader => "RegistryUpdated".GetLocalizedResource();
    [ObservableProperty]
    public partial ObservableCollection<TgEfSourceLiteDto> Dtos { get; set; } = [];
    [ObservableProperty]
    public partial ObservableCollection<OpenTgResearcherDesktop.Models.TgChatRegistryRow> RegistryRows { get; private set; } = [];
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedChat))]
    public partial OpenTgResearcherDesktop.Models.TgChatRegistryRow? SelectedRow { get; set; }
    public bool HasSelectedChat => SelectedRow is not null;
    [ObservableProperty]
    public partial bool IsFilterBySubscribe { get; set; }
    [ObservableProperty]
    public partial bool IsFilterByTitle { get; set; } = true;
    [ObservableProperty]
    public partial int RegistryFilter { get; set; }
    [ObservableProperty]
    public partial int RegistrySort { get; set; } = 1;
    [ObservableProperty]
    public partial bool CanPauseSelected { get; private set; }
    [ObservableProperty]
    public partial string PauseCaption { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string SelectionDetails { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial bool CanToggleSelectedParsing { get; private set; }
    [ObservableProperty]
    public partial string ParsingCaption { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string ParsingGlyph { get; private set; } = "\uE768";
    public IAsyncRelayCommand RefreshRegistryCommand { get; }
    public IRelayCommand PauseSelectedCommand { get; }
    public IRelayCommand OpenSelectedChatCommand { get; }
    public IAsyncRelayCommand ToggleSelectedParsingCommand { get; }
    [ObservableProperty]
    public partial string PublicChatError { get; private set; } = string.Empty;
    public IAsyncRelayCommand AddPublicChatCommand { get; }

    public TgChatsViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService,
        ILogger<TgChatsViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgChatsViewModel))
    {
        RefreshRegistryCommand = new AsyncRelayCommand(() => LazyLoadAsync(isNewQuery: true, isSearch: true));
        AddPublicChatCommand = new AsyncRelayCommand(AddPublicChatAsync, () => HasSelectedPublicChat);
        SearchChatsCommand = new AsyncRelayCommand(SearchChatsAsync);
        PauseSelectedCommand = new RelayCommand(PauseSelected, () => CanPauseSelected);
        OpenSelectedChatCommand = new RelayCommand(OpenSelectedChat, () => HasSelectedChat);
        // Повторный вызов нужен для прерывания ещё выполняющейся команды запуска
        ToggleSelectedParsingCommand = new AsyncRelayCommand(ToggleSelectedParsingAsync, () => CanToggleSelectedParsing,
            AsyncRelayCommandOptions.AllowConcurrentExecutions);
    }

    /// <summary> Открывает реестр без остановки выполняющегося парсинга </summary>
    public override Task OnNavigatedToAsync(NavigationEventArgs? e) => RefreshRegistryCommand.ExecuteAsync(null);

    /// <summary> Читает независимый снимок чатов и фактические количества из локального хранилища </summary>
    public async Task<List<TgEfSourceLiteDto>> GetListDtosAsync()
    {
        try
        {
            // Отдельные контексты чтения не занимают репозитории выполняющегося парсера
            using var sources = new TgEfSourceRepository();
            using var messages = new TgEfMessageRepository();
            using var chatUsers = new TgEfChatUserRepository();
            var query = sources.GetQuery(isReadOnly: true);
            var operations = LoadStateService.NetworkActivity.Operations;
            var runningIds = operations.Where(x => x.Kind == TgNetworkOperationKind.Parsing && x.State == TgNetworkOperationState.Running)
                .Select(x => x.ChatId ?? 0).ToArray();
            // Отбираем подписки и работающие чаты до пагинации
            if (IsFilterBySubscribe || RegistryFilter == 2) query = query.Where(x => x.IsSubscribe);
            if (RegistryFilter == 1) query = query.Where(x => runningIds.Contains(x.Id));
            var raw = FilterText.Trim().TrimStart('@');
            // Поиск по ID, имени и заголовку не зависит от регистра кириллицы и латиницы
            var entities = await query.ToListAsync();
            if (!string.IsNullOrWhiteSpace(raw))
                entities = [.. entities.Where(x => (IsFilterById && x.Id.ToString().Contains(raw, StringComparison.OrdinalIgnoreCase))
                    || (IsFilterByName && (x.UserName?.Contains(raw, StringComparison.OrdinalIgnoreCase) ?? false))
                    || (IsFilterByTitle && (x.Title?.Contains(raw, StringComparison.OrdinalIgnoreCase) ?? false)))];
            // Считаем сообщения и медиа по отобранным чатам, не подменяя количество последним ID Telegram
            var allIds = entities.Select(x => x.Id).ToArray();
            var totals = await messages.GetQuery(isReadOnly: true).Where(x => allIds.Contains(x.SourceId))
                .GroupBy(x => x.SourceId).Select(g => new { Id = g.Key, Messages = g.Count(),
                    Files = g.Count(x => x.Type == TgEnumMessageType.Document || x.Type == TgEnumMessageType.Photo) }).ToListAsync();
            var counts = totals.ToDictionary(x => x.Id);
            var ordered = RegistrySort switch
            {
                1 => entities.OrderBy(x => x.Title ?? x.UserName, StringComparer.CurrentCultureIgnoreCase),
                2 => entities.OrderByDescending(x => counts.TryGetValue(x.Id, out var count) ? count.Messages : 0),
                _ => entities.OrderByDescending(x => runningIds.Contains(x.Id)).ThenByDescending(x => x.DtChanged),
            };
            var page = ordered.ThenBy(x => x.Id).Skip(Math.Max(0, PageSkip)).Take(PageTake).ToArray();
            var pageIds = page.Select(x => x.Id).ToArray();
            var participants = await chatUsers.GetQuery(isReadOnly: true).Where(x => pageIds.Contains(x.ChatId) && !x.IsDeleted)
                .GroupBy(x => x.ChatId).Select(g => new { Id = g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Id, x => x.Count);
            _pageRows = [];
            foreach (var entity in page)
            {
                var dto = TgEfDomainUtils.CreateNewLiteDto(entity, isUidCopy: true);
                dto.ParticipantsCount = participants.GetValueOrDefault(dto.Id);
                counts.TryGetValue(dto.Id, out var count);
                var row = new OpenTgResearcherDesktop.Models.TgChatRegistryRow(dto, entity.Directory ?? string.Empty,
                    count?.Messages ?? 0, count?.Files ?? 0, entity.DtChanged);
                row.Refresh(operations.FirstOrDefault(x => x.Kind == TgNetworkOperationKind.Parsing && x.ChatId == dto.Id));
                _pageRows.Add(row);
            }
            LoadedDataStatistics = string.Format("RegistryLoaded".GetLocalizedResource(), PageSkip + page.Length, entities.Count);
            return [.. _pageRows.Select(x => x.Source)];
        }
        catch (Exception ex) { LogError(ex); throw; }
    }

    /// <summary> Обновляет живые состояния без обращения к БД и сетевым токенам </summary>
    public void RefreshActivity()
    {
        var operations = LoadStateService.NetworkActivity.Operations;
        foreach (var row in RegistryRows)
            row.Refresh(operations.FirstOrDefault(x => x.Kind == TgNetworkOperationKind.Parsing && x.ChatId == row.Source.Id));
        var selected = operations.FirstOrDefault(x => x.Kind == TgNetworkOperationKind.Parsing && x.ChatId == SelectedRow?.Source.Id);
        CanPauseSelected = selected?.State == TgNetworkOperationState.Running && selected.CanCancel && !selected.IsCancellationRequested;
        PauseSelectedCommand.NotifyCanExecuteChanged();
        var isRunning = selected?.State == TgNetworkOperationState.Running;
        CanToggleSelectedParsing = isRunning ? CanPauseSelected : SelectedRow?.Source.Id > 0 && IsOnlineReady
            && !_parsingChatIds.Contains(SelectedRow.Source.Id);
        ParsingCaption = (isRunning ? selected!.IsCancellationRequested ? "NetworkStateStopping" : "RegistryStop" : "RegistryStart").GetLocalizedResource();
        ParsingGlyph = isRunning ? "\uE71A" : "\uE768";
        ToggleSelectedParsingCommand.NotifyCanExecuteChanged();
        PauseCaption = (selected?.IsPaused == true ? "NetworkResume" : "NetworkPause").GetLocalizedResource();
        SelectionDetails = SelectedRow is null ? "RegistrySelectChat".GetLocalizedResource()
            : string.Format("RegistrySelectionDetails".GetLocalizedResource(), SelectedRow.Source.ParticipantsCount,
                SelectedRow.Source.IsSubscribe ? "RegistrySubscribed".GetLocalizedResource() : "RegistryNotSubscribed".GetLocalizedResource(),
                SelectedRow.Source.IsUserAccess ? "RegistryHasAccess".GetLocalizedResource() : "RegistryNoAccess".GetLocalizedResource());
        RefreshPublicSearchLanguage();
        OnPropertyChanged(nameof(ChatTitleHeader));
        OnPropertyChanged(nameof(ChatCountHeader));
        OnPropertyChanged(nameof(ChatSizeHeader));
        OnPropertyChanged(nameof(ChatProgressHeader));
        OnPropertyChanged(nameof(ChatUpdatedHeader));
    }

    // Обновляем действия выбранного чата
    partial void OnSelectedRowChanged(OpenTgResearcherDesktop.Models.TgChatRegistryRow? value)
    {
        OpenSelectedChatCommand.NotifyCanExecuteChanged();
        RefreshActivity();
    }

    // Открываем выбранный чат напрямую, без диалога подтверждения
    private void OpenSelectedChat()
    {
        if (SelectedRow is { } row) NavigationService.NavigateTo(typeof(TgChatViewModel).FullName!, row.Source.Uid);
    }

    // Фиксируем выбранный чат до ожидания и подтверждаем только его прерывание
    private async Task ToggleSelectedParsingAsync()
    {
        RefreshActivity();
        if (!CanToggleSelectedParsing || SelectedRow is not { } row) return;
        var chatId = row.Source.Id;
        try
        {
            if (CanPauseSelected)
            {
                await ContentDialogAsync(() =>
                {
                    LoadStateService.NetworkActivity.CancelParsing(chatId);
                    RefreshActivity();
                    return Task.CompletedTask;
                }, "AskStopChatParsing".GetLocalizedResource(), TgEnumLoadDesktopType.Storage);
            }
            else
                await StartSelectedParsingAsync(row.Source.Uid, chatId);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
        {
            // Запрошенное прерывание не является ошибкой подключения
        }
        catch (Exception ex) { LogError(ex); }
        finally { RefreshActivity(); }
    }

    // Загружаем сохранённые настройки в отдельный снимок, не меняя черновик открытого чата
    private async Task StartSelectedParsingAsync(Guid uid, long chatId)
    {
        if (LoadStateService.NetworkActivity.Operations
            .Any(x => x.Kind == TgNetworkOperationKind.Parsing && x.ChatId == chatId && x.State == TgNetworkOperationState.Running)
            || !_parsingChatIds.Add(chatId)) return;
        RefreshActivity();
        try
        {
            using var sources = new TgEfSourceRepository();
            var source = await sources.GetDtoAsync(x => x.Uid == uid);
            if (source.Id <= 0) throw new InvalidOperationException("Выбранный чат не найден в хранилище");
            var settings = new TgDownloadSettingsViewModel();
            settings.SourceVm.Dto = source;
            await LoadOnlineDataAsync(async () =>
            {
                if (!await App.BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync()) return;
                await App.BusinessLogicManager.ConnectClient.ParseChatAsync(settings);
            });
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
        {
            // Обрабатываем штатную отмену до общего обработчика диалога
        }
        catch (Exception ex) { LogError(ex); }
        finally
        {
            _parsingChatIds.Remove(chatId);
            await RefreshRegistryCommand.ExecuteAsync(null);
            RefreshActivity();
        }
    }

    // Приостанавливаем только выбранный чат, не затрагивая другие сетевые операции
    private void PauseSelected()
    {
        if (!CanPauseSelected || SelectedRow is null) return;
        LoadStateService.NetworkActivity.ToggleParsingPause(SelectedRow.Source.Id);
        RefreshActivity();
    }

    /// <summary> Открывает пульт выбранного чата двойным нажатием </summary>
    public void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not DataGrid dataGrid) return;
        if (dataGrid.SelectedItem is OpenTgResearcherDesktop.Models.TgChatRegistryRow row)
            NavigationService.NavigateTo(typeof(TgChatViewModel).FullName!, row.Source.Uid);
    }

    // Очищаем только локальный просмотр, сохраняя выбранный чат для восстановления после поиска
    protected override void ItemsClearCore()
    {
        _directorySizesCancellation.Cancel();
        _directorySizesCancellation.Dispose();
        _directorySizesCancellation = new();
        Dtos.Clear();
        RegistryRows.Clear();
    }

    // Добавляем следующую порцию и сохраняем выбор при повторном чтении
    protected override async Task LazyLoadCoreAsync(bool isSearch)
    {
        if (!HasMoreItems) return;
        var selectedId = SelectedRow?.Source.Id;
        var newItems = await GetListDtosAsync();
        HasMoreItems = newItems.Count == PageTake;
        if (isSearch)
        {
            Dtos = [.. newItems];
            RegistryRows = [.. _pageRows];
        }
        else
        {
            // Сохраняем коллекцию таблицы и её позицию прокрутки при добавлении порции
            foreach (var item in newItems) Dtos.Add(item);
            foreach (var row in _pageRows) RegistryRows.Add(row);
        }
        SelectedRow = RegistryRows.FirstOrDefault(x => x.Source.Id == selectedId) ?? RegistryRows.FirstOrDefault();
        IsEmptyData = RegistryRows.Count == 0;
        PageSkip += newItems.Count;
        RefreshActivity();
        // Подсчёт размеров не удерживает команду загрузки и не блокирует следующую порцию
        DirectorySizesTask = UpdateDirectorySizesAsync(DirectorySizesTask, _pageRows.ToArray(), _directorySizesCancellation.Token);
    }

    // Обрабатываем порции последовательно, отменяя устаревшие строки после поиска и обновления
    private async Task UpdateDirectorySizesAsync(Task previous, OpenTgResearcherDesktop.Models.TgChatRegistryRow[] rows, CancellationToken ct)
    {
        try
        {
            await previous.ConfigureAwait(false);
            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                await row.UpdateDirectorySizeAsync(ct).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex) { TgLogUtils.WriteException(ex); }
    }

    /// <summary> Отменяет подсчёт размеров при уходе со страницы или закрытии окна </summary>
    public void CancelDirectorySizeUpdates() => _directorySizesCancellation.Cancel();

    /// <summary> Догружает следующую порцию по запросу прокрутки без повторных параллельных запросов </summary>
    public async Task LoadNextRegistryPageAsync()
    {
        if (_isLoadingNextPage || IsLoading || !HasMoreItems || RefreshRegistryCommand.IsRunning) return;
        _isLoadingNextPage = true;
        try { await LazyLoadAsync(isNewQuery: false, isSearch: false); }
        catch (Exception ex) { LogError(ex); }
        finally { _isLoadingNextPage = false; }
    }

    // Статистика снимка уже рассчитана тем же запросом и не занимает контекст парсера
    protected override Task AfterDataUpdateCoreAsync() => Task.CompletedTask;

    // Обновляем список из Telegram отдельной командой, локальный реестр остаётся доступным
    protected override async Task StartUpdateOnlineCoreAsync() => await LoadOnlineDataAsync(async () =>
    {
        // Поиск и пагинация относятся к локальному просмотру, обновляем весь список аккаунта
        var settings = new TgDownloadSettingsViewModel();
        await App.BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(settings, TgEnumSourceType.Chat);
        await RefreshRegistryCommand.ExecuteAsync(null);
    });
}
