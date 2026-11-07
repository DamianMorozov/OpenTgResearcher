namespace OpenTgResearcherDesktop.ChatSelfTests;

public sealed class ILoadStateService
{
    public TgNetworkActivityService NetworkActivity { get; set; } = new();
    public bool IsOnlineReady { get; set; }
    public bool IsOnlineProcessing { get; set; }
    public bool IsDisplaySensitiveData { get; set; } = true;
    public string SensitiveData => "**********";
    public CancellationToken OnlineToken => default;
    public CancellationToken MemoryToken => default;
    public CancellationToken StorageToken => default;
    /// <summary> Подготавливает токен тестовой операции </summary>
    public Task PrepareOnlineTokenAsync(Guid uid) => Task.CompletedTask;
    /// <summary> Подготавливает токен тестовой операции </summary>
    public Task PrepareMemoryTokenAsync(Guid uid) => Task.CompletedTask;
    /// <summary> Подготавливает токен тестовой операции </summary>
    public Task PrepareStorageTokenAsync(Guid uid) => Task.CompletedTask;
    /// <summary> Завершает тестовую операцию без обращения к клиенту </summary>
    public void StopSoftOnlineProcessing(Guid uid) { }
    /// <summary> Завершает тестовую операцию без обращения к клиенту </summary>
    public void StopSoftMemoryProcessing(Guid uid) { }
    /// <summary> Завершает тестовую операцию без обращения к клиенту </summary>
    public void StopSoftStorageProcessing(Guid uid) { }
    /// <summary> Завершает тестовую операцию без обращения к клиенту </summary>
    public void StopHardOnlineProcessing() { }
    /// <summary> Завершает тестовую операцию без обращения к клиенту </summary>
    public void StopHardDownloadProcessing() { }
    /// <summary> Завершает тестовую операцию без обращения к клиенту </summary>
    public void StopHardAllProcessing() { }
}
public sealed class ITgSettingsService { public bool IsExistsAppStorage { get; set; } = true; }
public interface ILogger<out T>;
public interface IAppNotificationService;
public sealed class INavigationService
{
    public object? Parameter { get; private set; }
    public string? PageKey { get; private set; }
    /// <summary> Запоминает параметр перехода для проверки </summary>
    public bool NavigateTo(string key, object? parameter = null) { PageKey = key; Parameter = parameter; return true; }
}
public sealed class NavigationEventArgs { public object? Parameter { get; set; } }
public sealed class RoutedEventArgs;
public sealed class DoubleTappedRoutedEventArgs;
public sealed class Button { public object Tag { get; set; } = null!; }
public sealed class DataGrid { public object? SelectedItem { get; set; } }
public sealed class SelectorBarItem { public object? Tag { get; set; } }
public sealed class SelectorBar
{
    private SelectorBarItem _selectedItem = null!;
    public Action? SelectionChanged { get; set; }
    public List<SelectorBarItem> Items { get; } = [new() { Tag = nameof(TgChatDownloadPage) }];
    public SelectorBarItem SelectedItem
    {
        get => _selectedItem;
        set { if (ReferenceEquals(_selectedItem, value)) return; _selectedItem = value; SelectionChanged?.Invoke(); }
    }
}
public sealed class SelectorBarSelectionChangedEventArgs;
public sealed class Frame
{
    public object? Model { get; set; }
    public Type? PageType { get; private set; }
    public object? Parameter { get; private set; }
    public int Navigations { get; private set; }
    /// <summary> Запоминает тип страницы, параметр и число переходов </summary>
    public bool Navigate(Type type, object? parameter)
    { PageType = type; Parameter = parameter; Navigations++; return true; }
    /// <summary> Возвращает модель открытой тестовой страницы </summary>
    public Task<object?> GetPageViewModelAsync() => Task.FromResult(Model);
}
public abstract class TgPageBase
{
    public virtual TgPageViewModelBase ViewModel => null!;
    public object? DataContext { get; set; }
    public event Action<object, RoutedEventArgs>? Loaded;
    // Заменяет обработку загрузки настоящей страницы
    protected void PageLoaded(object sender, RoutedEventArgs e) { }
    // Вызывает событие загрузки тестовой страницы
    protected void RaiseLoaded() => Loaded?.Invoke(this, new());
}
public abstract class TgPageViewModelBase : ObservableObject, IDisposable
{
    public bool IsOnlineReady => LoadStateService.IsOnlineReady;
    public bool IsOnlineProcessing => LoadStateService.IsOnlineProcessing;
    public bool IsDisplaySensitiveData => LoadStateService.IsDisplaySensitiveData;
    public string SensitiveData => LoadStateService.SensitiveData;
    protected ILoadStateService LoadStateService { get; }
    protected ITgSettingsService SettingsService { get; }
    public INavigationService NavigationService { get; }
    public TgDownloadSettingsViewModel DownloadSettings { get; } = new();
    public bool IsEmptyData { get; set; } = true;
    public List<Exception> Errors { get; } = [];
    public int OnlineLoads { get; private set; }
    public int ConfirmationCount { get; private set; }
    protected TgPageViewModelBase(ILoadStateService state, ITgSettingsService settings, INavigationService navigation, object logger, string name)
    { LoadStateService = state; SettingsService = settings; NavigationService = navigation; }
    /// <summary> Заменяет навигацию базовой модели </summary>
    public virtual Task OnNavigatedToAsync(NavigationEventArgs? e) => Task.CompletedTask;
    /// <summary> Заменяет обновление интерфейса </summary>
    public virtual Task ReloadUiAsync() => Task.CompletedTask;
    /// <summary> Выполняет действие и сохраняет исключение для проверки </summary>
    public async Task LoadStorageDataAsync(Func<Task> action)
    { try { await action(); } catch (Exception ex) { LogError(ex); } }
    /// <summary> Выполняет действие и сохраняет исключение для проверки </summary>
    public Task LoadStorageDataAsync(Action action) => LoadStorageDataAsync(() => { action(); return Task.CompletedTask; });
    // Выполняет действие с подменённым сетевым клиентом
    protected Task LoadOnlineDataAsync(Func<Task> action) { OnlineLoads++; return action(); }
    // Заменяет подтверждение действия без настоящего диалога
    protected Task ContentDialogAsync(Func<Task> action, string title, TgEnumLoadDesktopType type)
    { ConfirmationCount++; return LoadStorageDataAsync(action); }
    // Заменяет подтверждение действия без настоящего диалога
    protected Task ContentDialogAsync(string title, string content) => Task.CompletedTask;
    protected Task ContentDialogAsync(Action action, string title, TgEnumLoadDesktopType type)
    { ConfirmationCount++; return LoadStorageDataAsync(action); }
    /// <summary> Сохраняет исключение для проверки результата </summary>
    public void LogError(Exception ex) => Errors.Add(ex);
    /// <summary> Завершает работу тестовой модели </summary>
    public void Dispose() { }
}
public abstract class TgSectionViewModel : TgPageViewModelBase
{
    public string LoadedDataStatistics { get; set; } = string.Empty;
    public string FilterText { get; set; } = string.Empty;
    public bool IsFilterById { get; set; } = true;
    public bool IsFilterByName { get; set; } = true;
    public int PageSkip { get; set; }
    public int PageTake { get; set; } = 2;
    public bool HasMoreItems { get; set; } = true;
    protected TgSectionViewModel(ILoadStateService state, ITgSettingsService settings, INavigationService navigation, object logger, string name)
        : base(state, settings, navigation, logger, name) { }
    /// <summary> Повторяет локальную загрузку раздела при открытии </summary>
    public override Task OnNavigatedToAsync(NavigationEventArgs? e) => LoadStorageDataAsync(async () =>
    {
        ItemsClearCore(); PageSkip = 0; HasMoreItems = true;
        await LazyLoadAsync(isNewQuery: false, isSearch: false);
        await ReloadUiAsync();
    });
    // Заменяет базовую постраничную загрузку
    protected virtual Task LazyLoadCoreAsync(bool isSearch) => Task.CompletedTask;
    // Сохраняем переопределяемые точки очистки и обновления списка
    protected virtual void ItemsClearCore() { }
    protected virtual Task AfterDataUpdateCoreAsync() => Task.CompletedTask;
    protected virtual Task StartUpdateOnlineCoreAsync() => Task.CompletedTask;
    protected virtual Task StopUpdateOnlineCoreAsync() => Task.CompletedTask;
    // Выполняем последовательную загрузку настоящего запроса модели реестра
    protected async Task LazyLoadAsync(bool isNewQuery, bool isSearch)
    {
        if (isNewQuery || isSearch) { ItemsClearCore(); PageSkip = 0; HasMoreItems = true; }
        await LazyLoadCoreAsync(isSearch);
        await AfterDataUpdateCoreAsync();
    }
}
public static class TgDesktopUtils
{
    /// <summary> Выполняет действие без диспетчера WinUI </summary>
    public static Task InvokeOnUIThreadAsync(Func<Task> action, CancellationToken ct = default) => action();
}
public static class App
{
    public static TestManager BusinessLogicManager { get; } = new();
    public static TestLocator VmLocator { get; } = new();
    /// <summary> Возвращает модель чата текущего сценария </summary>
    public static T GetService<T>() => (T)(object)Program.Chat;
}
public sealed class TestLocator
{
    /// <summary> Проверяет доступность данных формы при сбросе прогресса </summary>
    public void ClearChatViewModel()
    {
        // Как настоящий локатор, обращаемся к данным формы загрузки
        _ = Program.Download.DownloadDto.FirstId;
    }
    /// <summary> Заменяет сброс состояния загрузки </summary>
    public void ClearStateDownloadingChat() { }
}
public sealed class TestManager
{
    public TestStorage StorageManager { get; } = new();
    public TestClient ConnectClient { get; } = new();
}
public sealed class TestStorage
{
    /// <summary> Повторяет чтение таблиц через исходный занятый репозиторий </summary>
    public async Task<ObservableCollection<TgStorageTableDto>> LoadStorageTableDtosAsync(params string[] names) =>
        [new(names[1], await SourceRepository.GetListCountAsync())];
    public ITgEfSourceRepository SourceRepository { get; set; } = null!;
    public ITgEfMessageRepository MessageRepository { get; set; } = null!;
    public ITgEfUserRepository UserRepository { get; set; } = null!;
    public ITgEfLicenseRepository LicenseRepository { get; set; } = null!;
    public ITgEfChatUserRepository ChatUserRepository { get; set; } = null!;
    public ITgEfFilterRepository FilterRepository { get; set; } = null!;
    public ITgEfStoryRepository StoryRepository { get; set; } = null!;
}
public sealed class TestClient
{
    /// <summary> Заменяет обновление списка чатов без сети </summary>
    public Task SearchSourcesTgAsync(TgDownloadSettingsViewModel settings, TgEnumSourceType type, List<long>? ids = null) =>
        SearchSources?.Invoke(settings, type, ids) ?? Task.CompletedTask;
    public Func<TgDownloadSettingsViewModel, TgEnumSourceType, List<long>?, Task>? SearchSources { get; set; }
    public TestClient? Client => this;
    public long UserId { get; set; } = 10;
    public bool Ready { get; set; }
    public List<TgParticipantDto> Participants { get; } = [];
    public Func<Task<bool>>? CheckConnection { get; set; }
    public Func<TgDownloadSettingsViewModel, Task>? Parse { get; set; }
    public Func<Task<TgChatDetailsDto>>? ReadDetails { get; set; }
    public Func<Task<List<TgParticipantDto>>>? ReadParticipants { get; set; }
    /// <summary> Возвращает заданную готовность тестового клиента </summary>
    public Task<bool> CheckClientConnectionReadyAsync() => CheckConnection?.Invoke() ?? Task.FromResult(Ready);
    /// <summary> Заменяет сетевую загрузку без обращения к Telegram </summary>
    public Task ParseChatAsync(TgDownloadSettingsViewModel settings) => Parse?.Invoke(settings) ?? Task.CompletedTask;
    /// <summary> Возвращает тестовые сведения о чате </summary>
    public Task<TgChatDetailsDto> GetChatDetailsByClientAsync(Guid uid, CancellationToken ct = default) =>
        ReadDetails?.Invoke() ?? Task.FromResult(new TgChatDetailsDto { Title = "Online" });
    /// <summary> Возвращает заданный список участников </summary>
    public Task<List<TgParticipantDto>> GetParticipantsAsync(long id) => ReadParticipants?.Invoke() ?? Task.FromResult(Participants);
    /// <summary> Заменяет обновление пользователей в Telegram </summary>
    public Task UpdateUsersAsync(List<TgEfUserDto> users) => Task.CompletedTask;
}
public sealed class TestContext(DbContextOptions options) : TgEfContextBase(options)
{
    // Использует только параметры изолированной тестовой БД
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
}
public sealed class TgUserDetailsViewModel;
public static class TgResourceExtensions
{
    /// <summary> Возвращает имя таблицы тестового хранилища </summary>
    public static string GetTableNameApps() => "Apps";
    /// <summary> Возвращает имя таблицы тестового хранилища </summary>
    public static string GetTableNameChats() => "Chats";
    /// <summary> Возвращает имя таблицы тестового хранилища </summary>
    public static string GetTableNameUsers() => "Users";
    /// <summary> Возвращает имя таблицы тестового хранилища </summary>
    public static string GetTableNameChatUsers() => "ChatUsers";
    /// <summary> Возвращает имя таблицы тестового хранилища </summary>
    public static string GetTableNameDocuments() => "Documents";
    /// <summary> Возвращает имя таблицы тестового хранилища </summary>
    public static string GetTableNameFilters() => "Filters";
    /// <summary> Возвращает имя таблицы тестового хранилища </summary>
    public static string GetTableNameMessages() => "Messages";
    /// <summary> Возвращает имя таблицы тестового хранилища </summary>
    public static string GetTableNameProxies() => "Proxies";
    /// <summary> Возвращает имя таблицы тестового хранилища </summary>
    public static string GetTableNameStories() => "Stories";
    /// <summary> Возвращает имя таблицы тестового хранилища </summary>
    public static string GetTableNameVersions() => "Versions";
    public static string AskUpdateOnline() => "Обновить из Telegram?";
    /// <summary> Возвращает заголовок реестра чатов </summary>
    public static string GetChatColumnHeader(string column) => column;
    /// <summary> Возвращает подпись числа загруженных строк </summary>
    public static string GetTextBlockLoaded() => "Загружено";
    /// <summary> Возвращает формат локализованной строки для проверки живого снимка </summary>
    public static string GetLocalizedResource(this string key) => key switch
    {
        "RegistryBytes" => "Б",
        "RegistryKilobytes" => "КБ",
        "RegistryMegabytes" => "МБ",
        "RegistryGigabytes" => "ГБ",
        "NetworkParsingMessageIds" => "Чтение ID {0} из {1}",
        "NetworkParsingMessageId" => "Чтение ID {0}",
        "ChatDetailsFailed" => "Не удалось обновить сведения о чате: {0}",
        _ => key,
    };
    /// <summary> Возвращает ключ заголовка участника для проверки модели </summary>
    public static string GetUserColumnHeader(string column) => column;
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string AskDataClear() => "Очистить";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string AskStartParseTelegram() => "Начать";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string AskStopParseTelegram() => "Остановить";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string AskSettingsSave() => "Сохранить";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string AskDefaultSettings() => "По умолчанию";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string Navigation() => "Переход";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string DiscussionChatNotFound() => "Обсуждение отсутствует";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string AskUpdateChatDetails() => "Обновить";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string GetTextBlockFiltered() => "Загружено";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string GetTextBlockTotalAmount() => "Всего";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string AskCalcContentStatistics() => "Статистика файлов";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string AskCalcChatStatistics() => "Статистика чата";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string AskLoading() => "Загрузить";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string AskStopLoading() => "Остановить";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string AskGetParticipants() => "Участники";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string AskGetParticipantsFromMessages() => "Авторы";
    /// <summary> Возвращает подпись тестового интерфейса </summary>
    public static string AskClearParticipants() => "Очистить";
}
