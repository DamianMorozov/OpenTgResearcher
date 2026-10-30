namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgProxiesViewModel : TgProxyPageViewModelBase
{
    [ObservableProperty]
    public partial ObservableCollection<TgEfProxyDto> Dtos { get; set; } = [];
    [ObservableProperty]
    public partial TgEfProxyDto? SelectedProxy { get; set; }
    public IAsyncRelayCommand LoadDataStorageCommand { get; }
    public IRelayCommand DefaultSortCommand { get; }
    public IRelayCommand AddProxyCommand { get; }
    public IAsyncRelayCommand CreateDefaultProxyCommand { get; }
    public IAsyncRelayCommand DeleteAllProxiesCommand { get; }
    public IAsyncRelayCommand DeleteProxyCommand { get; }
    public IRelayCommand OpenOrEditProxyCommand { get; }
    public IRelayCommand ClearViewCommand { get; }

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
    }

    /// <summary> Загружает список при открытии и после возврата из карточки </summary>
    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await RunStorageActionAsync(LoadDataStorageCoreAsync);

    // Обновляем команды при выборе строки
    partial void OnSelectedProxyChanged(TgEfProxyDto? value)
    {
        DeleteProxyCommand.NotifyCanExecuteChanged();
        OpenOrEditProxyCommand.NotifyCanExecuteChanged();
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
