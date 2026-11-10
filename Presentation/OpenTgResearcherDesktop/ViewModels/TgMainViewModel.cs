namespace OpenTgResearcherDesktop.ViewModels;

/// <summary> Центр запуска с локальной историей и переходами к основным сценариям </summary>
public sealed partial class TgMainViewModel : TgPageViewModelBase
{
    private readonly INavigationService _navigation;
    private bool _isActive;
    public ObservableCollection<TgHomeRecentChat> RecentChats { get; } = [];
    public IRelayCommand PrimaryActionCommand { get; }
    public IRelayCommand OpenChatsCommand { get; }
    public IRelayCommand OpenConnectionCommand { get; }
    public IRelayCommand OpenNetworkCommand { get; }
    public IRelayCommand OpenSettingsCommand { get; }
    public IRelayCommand<TgHomeRecentChat> OpenRecentChatCommand { get; }
    public string VersionText => TgProgramIntegration.CurrentVersion;

    [ObservableProperty]
    public partial string ProfileName { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string ConnectionCaption { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string PrimaryActionCaption { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsRecentEmpty { get; private set; } = true;

    /// <summary> Связывает главную с действующими страницами без запросов к Telegram </summary>
    public TgMainViewModel(ILoadStateService loadStateService, ITgSettingsService settingsService, INavigationService navigationService,
        ILogger<TgMainViewModel> logger) : base(loadStateService, settingsService, navigationService, logger, nameof(TgMainViewModel))
    {
        _navigation = navigationService;
        PrimaryActionCommand = new RelayCommand(() => Navigate(LoadStateService.IsOnlineReady ? typeof(TgChatsViewModel) : typeof(TgClientConnectionViewModel)));
        OpenChatsCommand = new RelayCommand(() => Navigate(typeof(TgChatsViewModel)));
        OpenConnectionCommand = new RelayCommand(() => Navigate(typeof(TgClientConnectionViewModel)));
        OpenNetworkCommand = new RelayCommand(() => Navigate(typeof(TgNetworkActivityViewModel)));
        OpenSettingsCommand = new RelayCommand(() => Navigate(typeof(TgSettingsViewModel)));
        OpenRecentChatCommand = new RelayCommand<TgHomeRecentChat>(OpenRecentChat);
    }

    /// <summary> Загружает главную без сети и без блокировки хранилища выполняющегося парсинга </summary>
    public override Task OnNavigatedToAsync(NavigationEventArgs? e) => ReloadUiAsync();

    /// <summary> Обновляет локальное представление вместо проверки соединения через API Telegram </summary>
    public override Task ReloadUiAsync()
    {
        Refresh();
        return Task.CompletedTask;
    }

    /// <summary> Подписывается на изменения только на время показа страницы </summary>
    public void Activate()
    {
        if (_isActive) return;
        _isActive = true;
        LoadStateService.PropertyChanged += OnStateChanged;
        Localizer.Get().LanguageChanged += OnLanguageChanged;
        Refresh();
    }

    /// <summary> Убирает подписки при переходе на другую страницу </summary>
    public void Deactivate()
    {
        if (!_isActive) return;
        _isActive = false;
        LoadStateService.PropertyChanged -= OnStateChanged;
        Localizer.Get().LanguageChanged -= OnLanguageChanged;
    }

    /// <summary> Освобождает подписки главной страницы вместе с базовой моделью </summary>
    public override void ReleaseManagedResources()
    {
        Deactivate();
        base.ReleaseManagedResources();
    }

    // Читаем только общие настройки, скрывая названия чатов вместе с остальными конфиденциальными данными
    private void Refresh()
    {
        try
        {
            ProfileName = TgAccountProfiles.ReadActive(SettingsService)?.Name ?? "ProfileDefaultName".GetLocalizedResource();
            ConnectionCaption = (LoadStateService.IsOnlineReady ? "HomeConnected" : "HomeDisconnected").GetLocalizedResource();
            PrimaryActionCaption = (LoadStateService.IsOnlineReady ? "HomeOpenChats" : "HomeConnectTelegram").GetLocalizedResource();
            RecentChats.Clear();
            foreach (var entry in TgRecentChats.Read(SettingsService))
            {
                var title = !LoadStateService.IsDisplaySensitiveData ? LoadStateService.SensitiveData
                    : string.IsNullOrWhiteSpace(entry.Title)
                        ? string.Format(CultureInfo.CurrentCulture, "HomeChatFallback".GetLocalizedResource(), entry.Id) : entry.Title;
                var detail = (RecentChats.Count == 0 ? "HomeLastOpened" : "HomeLocalArchive").GetLocalizedResource();
                RecentChats.Add(new(entry.Uid, title, detail, OpenRecentChatCommand));
            }
            IsRecentEmpty = RecentChats.Count == 0;
        }
        catch (Exception ex) { LogError(ex); }
    }

    // Отражаем смену готовности соединения и общего режима конфиденциальности
    private void OnStateChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(ILoadStateService.IsOnlineReady) or nameof(ILoadStateService.IsDisplaySensitiveData))) return;
        TgDesktopUtils.InvokeOnUIThread(Refresh);
    }

    // Переводим вычисляемые подписи при переключении языка открытой страницы
    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e) => TgDesktopUtils.InvokeOnUIThread(Refresh);

    // Открываем локальный чат по идентификатору, не запуская парсинг
    private void OpenRecentChat(TgHomeRecentChat? chat)
    {
        if (chat is not null && chat.Uid != Guid.Empty) Navigate(typeof(TgChatViewModel), chat.Uid);
    }

    // Все переходы используют навигацию приложения и оставляют диагностику при ошибке
    private void Navigate(Type page, object? parameter = null)
    {
        try { _navigation.NavigateTo(page.FullName!, parameter); }
        catch (Exception ex) { LogError(ex); }
    }
}
