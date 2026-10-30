namespace OpenTgResearcherDesktop.ViewModels;

public sealed partial class TgProxyViewModel : TgProxyPageViewModelBase
{
    [ObservableProperty]
    public partial Guid Uid { get; set; }
    [ObservableProperty]
    public partial TgEfProxyDto Dto { get; set; } = new();
    [ObservableProperty]
    public partial string PortText { get; set; } = "0";
    public ObservableCollection<TgEnumProxyType> Types { get; } = [.. Enum.GetValues<TgEnumProxyType>()];
    public IAsyncRelayCommand LoadDataStorageCommand { get; }
    public IAsyncRelayCommand ClearViewCommand { get; }
    public IAsyncRelayCommand SaveProxySettingsCommand { get; }
    public IAsyncRelayCommand DeleteProxyCommand { get; }
    public IRelayCommand BackCommand { get; }

    public TgProxyViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService,
        ILogger<TgProxyViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgProxyViewModel))
    {
        ClearViewCommand = new AsyncRelayCommand(ClearViewAsync);
        LoadDataStorageCommand = new AsyncRelayCommand(LoadDataStorageAsync);
        SaveProxySettingsCommand = new AsyncRelayCommand(SaveProxySettingsAsync);
        DeleteProxyCommand = new AsyncRelayCommand(DeleteProxyAsync, () => Uid != Guid.Empty);
        BackCommand = new RelayCommand(Back);
    }

    /// <summary> Загружает выбранный прокси или создаёт новую форму без записи в БД </summary>
    public override async Task OnNavigatedToAsync(NavigationEventArgs? e) => await RunStorageActionAsync(async () =>
    {
        Uid = e?.Parameter is Guid uid ? uid : Guid.Empty;
        await LoadDataStorageCoreAsync();
    });

    // Обновляем доступность удаления после создания или удаления записи
    partial void OnUidChanged(Guid value) => DeleteProxyCommand?.NotifyCanExecuteChanged();

    // Обновляем текст порта при смене модели формы
    partial void OnDtoChanged(TgEfProxyDto value) => PortText = value.Port.ToString();

    // Очищаем редактируемые поля, сохраняя идентификатор записи
    private async Task ClearViewAsync() => await ConfirmStorageActionAsync(ClearDataStorageCoreAsync, TgResourceExtensions.AskDataClear());

    // Заменяем модель, чтобы все привязки получили уведомления об очистке
    private Task ClearDataStorageCoreAsync()
    {
        Dto = new() { Uid = Uid };
        return Task.CompletedTask;
    }

    // Подтверждаем отказ от несохранённых изменений
    private async Task LoadDataStorageAsync() => await ConfirmStorageActionAsync(LoadDataStorageCoreAsync, TgResourceExtensions.AskLoading());

    // Читаем запись по точному идентификатору
    private async Task LoadDataStorageCoreAsync()
    {
        if (!SettingsService.IsExistsAppStorage) return;
        Dto = Uid == Guid.Empty ? new() : await App.BusinessLogicManager.StorageManager.ProxyRepository.GetDtoAsync(Uid);
        if (Uid != Guid.Empty && Dto.Uid != Uid) throw new InvalidOperationException("Запись прокси больше не существует");
    }

    // Сохраняем настройки после закрытия диалога
    private async Task SaveProxySettingsAsync() => await ConfirmStorageActionAsync(SaveProxySettingsCoreAsync, TgResourceExtensions.AskSettingsSave());

    // Проверяем текст порта до преобразования, чтобы ошибочный ввод не сохранил старое значение
    private async Task SaveProxySettingsCoreAsync()
    {
        if (!ushort.TryParse(PortText, out var port)) throw new ArgumentException("Порт должен быть целым числом от 0 до 65535");
        Dto.Port = port;
        await App.BusinessLogicManager.StorageManager.ProxyRepository.SaveAsync(Dto);
        Uid = Dto.Uid;
        await LoadDataStorageCoreAsync();
    }

    // Подтверждаем удаление сохранённой записи
    private async Task DeleteProxyAsync() => await ConfirmStorageActionAsync(DeleteProxyCoreAsync, TgResourceExtensions.AskDeleteRecord());

    // Удаляем только выбранный идентификатор и возвращаемся к списку
    private async Task DeleteProxyCoreAsync()
    {
        if (Uid == Guid.Empty) return;
        await App.BusinessLogicManager.StorageManager.ProxyRepository.DeleteAsync(x => x.Uid == Uid);
        Uid = Guid.Empty;
        Dto = new();
        Back();
    }

    // Возвращаемся к списку прокси
    private void Back() => NavigationService.NavigateTo(typeof(TgProxiesViewModel).FullName!);
}
