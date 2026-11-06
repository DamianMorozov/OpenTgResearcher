namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgProxiesViewModel : TgProxyPageViewModelBase
{
    public string ProxyTypeHeader => TgResourceExtensions.GetProxyColumnHeader("Type");
    public string ProxyHostHeader => TgResourceExtensions.GetProxyColumnHeader("Host");
    public string ProxyPortHeader => TgResourceExtensions.GetProxyColumnHeader("Port");
    public string ProxyUserHeader => TgResourceExtensions.GetProxyColumnHeader("User");
    public string ProxyPasswordHeader => TgResourceExtensions.GetProxyColumnHeader("Password");
    public string ProxyAvailabilityHeader => TgResourceExtensions.GetProxyColumnHeader("Availability");
    [ObservableProperty]
    public partial ObservableCollection<TgEfProxyDto> Dtos { get; set; } = [];
    [ObservableProperty]
    public partial TgEfProxyDto? SelectedProxy { get; set; }
    [ObservableProperty]
    public partial string TelegramDataPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Telegram Desktop", "tdata");
    [ObservableProperty]
    public partial string ImportResult { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsCheckingProxies { get; set; }
    public IAsyncRelayCommand LoadDataStorageCommand { get; }
    public IRelayCommand DefaultSortCommand { get; }
    public IRelayCommand AddProxyCommand { get; }
    public IAsyncRelayCommand CreateDefaultProxyCommand { get; }
    public IAsyncRelayCommand DeleteAllProxiesCommand { get; }
    public IAsyncRelayCommand DeleteProxyCommand { get; }
    public IRelayCommand OpenOrEditProxyCommand { get; }
    public IRelayCommand ClearViewCommand { get; }
    public IAsyncRelayCommand ImportTelegramProxiesCommand { get; }
    public IAsyncRelayCommand CheckAllProxiesCommand { get; }
    public IRelayCommand ApplyProxyCommand { get; }

    public TgProxiesViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService,
        ILogger<TgProxiesViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgProxiesViewModel))
    {
        DefaultSortCommand = new RelayCommand(SetOrderData);
        LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
        AddProxyCommand = new RelayCommand(AddProxy);
        CreateDefaultProxyCommand = new AsyncRelayCommand(CreateDefaultProxyAsync);
        DeleteAllProxiesCommand = new AsyncRelayCommand(DeleteAllProxiesAsync);
        DeleteProxyCommand = new AsyncRelayCommand(DeleteProxyAsync, () => SelectedProxy is not null);
        OpenOrEditProxyCommand = new RelayCommand(OpenOrEditProxy, () => SelectedProxy is not null);
        ClearViewCommand = new RelayCommand(ClearView);
        ImportTelegramProxiesCommand = new AsyncRelayCommand(ImportTelegramProxiesAsync);
        CheckAllProxiesCommand = new AsyncRelayCommand(CheckAllProxiesAsync, () => !IsCheckingProxies && Dtos.Any(x => x.Type != TgEnumProxyType.None));
        ApplyProxyCommand = new RelayCommand(ApplyProxy, () => SelectedProxy is not null && !IsCheckingProxies);
    }

    /// <summary> Загружает список при открытии и после возврата из карточки </summary>
    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await RunStorageActionAsync(LoadDataStorageCoreAsync);

    // Обновляем команды при выборе строки
    partial void OnSelectedProxyChanged(TgEfProxyDto? value)
    {
        DeleteProxyCommand.NotifyCanExecuteChanged();
        OpenOrEditProxyCommand.NotifyCanExecuteChanged();
        ApplyProxyCommand.NotifyCanExecuteChanged();
    }

    // Обновляем доступность проверки после загрузки или очистки списка
    partial void OnDtosChanged(ObservableCollection<TgEfProxyDto> value) => CheckAllProxiesCommand?.NotifyCanExecuteChanged();

    // На время проверки исключаем повторный запуск и применение незавершённой строки
    partial void OnIsCheckingProxiesChanged(bool value)
    {
        CheckAllProxiesCommand.NotifyCanExecuteChanged();
        ApplyProxyCommand.NotifyCanExecuteChanged();
    }

    // Передаём выбранную запись форме подключения, которая сохраняет выбор в существующем профиле
    private void ApplyProxy()
    {
        if (SelectedProxy is { } proxy) NavigationService.NavigateTo(typeof(TgClientConnectionViewModel).FullName!, proxy.Uid);
    }

    // Проверяем все серверы списка без изменения сессии и транспорта Telegram
    private async Task CheckAllProxiesAsync(CancellationToken ct = default)
    {
        TgEfProxyDto? current = null;
        var networkUid = Guid.NewGuid();
        try
        {
            IsCheckingProxies = true;
            await LoadStateService.PrepareDownloadTokenAsync(networkUid);
            LoadStateService.NetworkActivity.SetKind(networkUid, TgNetworkOperationKind.Proxies);
            Exception.Default();
            var proxies = Dtos.ToArray();
            foreach (var proxy in proxies)
                proxy.AvailabilityStatus = TgResourceExtensions.GetProxyAvailabilityStatus(proxy.Type == TgEnumProxyType.None ? "Direct" : "NotChecked");
            foreach (var proxy in proxies.Where(x => x.Type != TgEnumProxyType.None))
            {
                ct.ThrowIfCancellationRequested();
                current = proxy;
                proxy.AvailabilityStatus = TgResourceExtensions.GetProxyAvailabilityStatus("Checking");
                var result = await TgProxyAvailabilityChecker.CheckAsync(proxy, ct: ct);
                proxy.AvailabilityStatus = TgResourceExtensions.GetProxyAvailabilityStatus(result.IsAvailable ? "Available" : "Unavailable", result.ElapsedMilliseconds);
                current = null;
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            LoadStateService.NetworkActivity.Finish(networkUid, TgNetworkOperationState.Canceled);
            if (current is not null) current.AvailabilityStatus = TgResourceExtensions.GetProxyAvailabilityStatus("NotChecked");
        }
        catch (Exception ex)
        {
            LoadStateService.NetworkActivity.Finish(networkUid, TgNetworkOperationState.Failed);
            LogError(ex); Exception.Set(ex);
        }
        finally
        {
            LoadStateService.StopSoftDownloadProcessing(networkUid);
            IsCheckingProxies = false;
        }
    }

    // Сортируем список, сохраняя выделение
    private void SetOrderData()
    {
        var uid = SelectedProxy?.Uid;
        Dtos = [.. Dtos.OrderBy(x => x.HostName).ThenBy(x => x.Type).ThenBy(x => x.Port)];
        SelectedProxy = Dtos.FirstOrDefault(x => x.Uid == uid);
    }

    // Очищаем только представление списка
    private void ClearView() { SelectedProxy = null; Dtos = []; }

    // Перечитываем сохранённые записи
    private async Task LoadDataStorageAsync() => await RunStorageActionAsync(LoadDataStorageCoreAsync);

    // Загружаем данные без запуска сетевых служб
    private async Task LoadDataStorageCoreAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;
        var uid = SelectedProxy?.Uid;
        Dtos = [.. await App.BusinessLogicManager.StorageManager.ProxyRepository.GetListDtosAsync()];
        foreach (var proxy in Dtos)
            proxy.AvailabilityStatus = TgResourceExtensions.GetProxyAvailabilityStatus(proxy.Type == TgEnumProxyType.None ? "Direct" : "NotChecked");
        SelectedProxy = Dtos.FirstOrDefault(x => x.Uid == uid);
        SetOrderData();
    }

    // Открываем новую форму без создания пустой записи в БД
    private void AddProxy() => NavigationService.NavigateTo(typeof(TgProxyViewModel).FullName!, Guid.Empty);

    // Открываем выбранную запись
    private void OpenOrEditProxy()
    {
        if (SelectedProxy is not null) NavigationService.NavigateTo(typeof(TgProxyViewModel).FullName!, SelectedProxy.Uid);
    }

    // Подтверждаем создание записи без прокси
    private async Task CreateDefaultProxyAsync() => await ConfirmStorageActionAsync(CreateDefaultProxyCoreAsync, TgResourceExtensions.AskCreateDefault());

    // Создаём стандартную запись и обновляем список
    private async Task CreateDefaultProxyCoreAsync()
    {
        await App.BusinessLogicManager.StorageManager.ProxyRepository.CreateDefaultAsync();
        await LoadDataStorageCoreAsync();
    }

    // Фиксируем выбранный каталог до подтверждения чтения и сохранения
    private async Task ImportTelegramProxiesAsync()
    {
        ImportResult = string.Empty;
        var directory = TelegramDataPath;
        await ConfirmStorageActionAsync(() => ImportTelegramProxiesCoreAsync(directory), TgResourceExtensions.AskImportTelegramProxies());
    }

    // Читаем только общие настройки Telegram и сохраняем новые записи без изменения текущего подключения
    private async Task ImportTelegramProxiesCoreAsync(string directory, CancellationToken ct = default)
    {
        var result = await Task.Run(() => TgTelegramDesktopProxyImporter.ReadAsync(directory, ct), ct);
        var added = await App.BusinessLogicManager.StorageManager.ProxyRepository.ImportAsync(result.Proxies, ct);
        await LoadDataStorageCoreAsync();
        ImportResult = TgResourceExtensions.GetTelegramProxyImportResult(added, result.Proxies.Count - added, result.SkippedWebCount);
    }

    // Фиксируем выбранную запись до показа подтверждения
    private async Task DeleteProxyAsync()
    {
        if (SelectedProxy is not { } selected) return;
        await ConfirmStorageActionAsync(() => DeleteProxyCoreAsync(selected.Uid), TgResourceExtensions.AskDeleteRecord());
    }

    // Удаляем выбранный прокси с обновлением списка
    private async Task DeleteProxyCoreAsync(Guid uid)
    {
        await App.BusinessLogicManager.StorageManager.ProxyRepository.DeleteAsync(x => x.Uid == uid);
        await LoadDataStorageCoreAsync();
    }

    // Подтверждаем очистку всех записей
    private async Task DeleteAllProxiesAsync() => await ConfirmStorageActionAsync(DeleteAllProxiesCoreAsync, TgResourceExtensions.AskDeleteAllRecords());

    // Удаляем записи и их ссылки одной операцией репозитория
    private async Task DeleteAllProxiesCoreAsync()
    {
        await App.BusinessLogicManager.StorageManager.ProxyRepository.DeleteAllAsync();
        await LoadDataStorageCoreAsync();
    }

    /// <summary> Открывает карточку выбранного прокси двойным нажатием </summary>
    public void DataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is not DataGrid { SelectedItem: TgEfProxyDto dto }) return;
        SelectedProxy = dto;
        OpenOrEditProxy();
    }
}
