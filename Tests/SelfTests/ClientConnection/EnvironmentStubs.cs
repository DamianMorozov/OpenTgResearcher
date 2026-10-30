namespace OpenTgResearcherDesktop.ConnectionSelfTests;

// Подменяем службы страницы без подключения к Telegram и без доступа к пользовательскому хранилищу
public interface ILoadStateService
{
    CancellationToken OnlineToken { get; }
    Task PrepareOnlineTokenAsync(Guid uid);
    void StopSoftOnlineProcessing(Guid uid);
}
public sealed class TestLoadStateService : ILoadStateService
{
    public CancellationToken OnlineToken => CancellationToken.None;
    /// <summary> Заменяет подготовку операции без запуска фоновых служб </summary>
    public Task PrepareOnlineTokenAsync(Guid uid) => Task.CompletedTask;
    /// <summary> Заменяет завершение учёта операции </summary>
    public void StopSoftOnlineProcessing(Guid uid) { }
}
public interface ITgSettingsService { bool IsExistsAppStorage { get; } string AppSession { get; } }
public sealed class TestSettingsService : ITgSettingsService
{
    public bool IsExistsAppStorage => true;
    public string AppSession => "test.session";
}
public interface INavigationService;
public interface ILogger<T>;
public sealed class NavigationEventArgs;
public sealed class TextChangedEventArgs;
public sealed class TextBox { public string Text { get; set; } = string.Empty; }
public static class TestGlobalTools { public static object Container { get; } = new(); }
public enum ContentDialogResult { None, Primary }
public enum ContentDialogButton { Close }
public sealed class ContentDialog
{
    public static bool Confirm { get; set; }
    public static bool IsOpen { get; private set; }
    public static Task? LastAction { get; private set; }
    public object? XamlRoot { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PrimaryButtonText { get; set; } = string.Empty;
    public string CloseButtonText { get; set; } = string.Empty;
    public ContentDialogButton DefaultButton { get; set; }
    public IAsyncRelayCommand? PrimaryButtonCommand { get; set; }
    /// <summary> Повторяет закрытие диалога без ожидания асинхронной команды кнопки </summary>
    public Task<ContentDialogResult> ShowAsync()
    {
        IsOpen = true;
        LastAction = null;
        if (Confirm && PrimaryButtonCommand is not null)
        {
            PrimaryButtonCommand.Execute(null);
            LastAction = PrimaryButtonCommand.ExecutionTask;
        }
        IsOpen = false;
        return Task.FromResult(Confirm ? ContentDialogResult.Primary : ContentDialogResult.None);
    }
}

public abstract class TgPageViewModelBase : ObservableObject
{
    protected ILoadStateService LoadStateService { get; }
    protected ITgSettingsService SettingsService { get; }
    public TgExceptionViewModel Exception { get; set; } = new();
    public string ConnectionDt { get; set; } = string.Empty;
    public string ConnectionMsg { get; set; } = string.Empty;
    public bool IsOnlineReady => App.BusinessLogicManager.ConnectClient.Ready;
    public List<Exception> Errors { get; } = [];
    public int DialogRequests { get; private set; }
    public object? XamlRootVm { get; set; } = new();
    public bool ConfirmDialog { get => ContentDialog.Confirm; set => ContentDialog.Confirm = value; }

    protected TgPageViewModelBase(ILoadStateService state, ITgSettingsService settings, INavigationService navigation, object logger, string name)
    { LoadStateService = state; SettingsService = settings; }
    /// <summary> Заменяет переход на страницу </summary>
    public virtual Task OnNavigatedToAsync(NavigationEventArgs? e) => Task.CompletedTask;
    /// <summary> Выполняет исходную загрузку через подменённые репозитории </summary>
    public Task LoadStorageDataAsync(Func<Task> action) => action();
    // Выполняем подтверждённое действие без настоящей сети
    protected Task LoadOnlineDataAsync(Func<Task> action) => action();
    /// <summary> Повторяет очистку состояния в базовой модели страницы </summary>
    public virtual Task ReloadUiAsync() { ConnectionDt = string.Empty; ConnectionMsg = string.Empty; Exception.Default(); return Task.CompletedTask; }
    /// <summary> Сохраняет ошибку для проверки результата </summary>
    public void LogError(Exception exception) => Errors.Add(exception);
    /// <summary> Повторяет обновление ошибки базовой модели </summary>
    public void UpdateException(Exception exception) => Exception = new(exception);
    // Повторяем существующий базовый диалог: команда кнопки запускается, но её завершение не ожидается
    protected async Task ContentDialogAsync(Func<Task> action, string title, TgEnumLoadDesktopType loadType)
    {
        DialogRequests++;
        if (XamlRootVm is null) return;
        var dialog = new ContentDialog { PrimaryButtonCommand = new AsyncRelayCommand(action) };
        _ = await dialog.ShowAsync();
    }
}
public static class App
{
    public static TestManager BusinessLogicManager { get; set; } = new();
    public static TestLocator VmLocator { get; set; } = new();
}
public sealed class TestManager
{
    public TestStorage StorageManager { get; } = new();
    public TestConnection ConnectClient { get; } = new();
}
public sealed class TestStorage
{
    public TestAppRepository AppRepository { get; } = new();
    public TestProxyRepository ProxyRepository { get; } = new();
}
public sealed class TestAppRepository
{
    public TgEfAppDto Current { get; set; } = new();
    public Guid? SavedProxyUid { get; private set; }
    public int SaveCalls { get; private set; }
    public int DeleteCalls { get; private set; }
    public bool FailSave { get; set; }
    /// <summary> Возвращает настройки из памяти </summary>
    public Task<TgEfAppDto> GetCurrentDtoAsync(CancellationToken ct = default) => Task.FromResult(Current);
    /// <summary> Проверяет сохранение изменённых настроек </summary>
    public Task SaveAsync(TgEfAppDto dto)
    {
        SaveCalls++;
        if (FailSave) throw new InvalidOperationException("Тестовая ошибка сохранения");
        SavedProxyUid = dto.ProxyUid;
        Current = dto;
        return Task.CompletedTask;
    }
    /// <summary> Удаляет только тестовые настройки в памяти </summary>
    public Task DeleteAllAsync() { DeleteCalls++; Current = new(); return Task.CompletedTask; }
}
public sealed class TestProxyRepository
{
    public List<TgEfProxyEntity> Items { get; } = [];
    /// <summary> Возвращает список прокси без обращения к сети и SQLite </summary>
    public Task<TestProxyResult> GetListAsync(TgEnumTableTopRecords take, int skip, bool isReadOnly) => Task.FromResult(new TestProxyResult(Items));
}
public sealed class TestProxyResult(List<TgEfProxyEntity> items)
{
    public bool IsExists => Items.Count > 0;
    public List<TgEfProxyEntity> Items { get; } = items;
}
public sealed class TestProxyViewModel
{
    public TgEfProxyDto Dto { get; set; }
    public bool IsEmptyProxy => Dto.Type == TgEnumProxyType.None && (Dto.UserName == "No user" || Dto.Password == "No password");
    public TestProxyViewModel(object container) : this(container, new TgEfProxyEntity()) { }
    public TestProxyViewModel(object container, TgEfProxyEntity entity) => Dto = new()
        { Uid = entity.Uid, Type = entity.Type, UserName = entity.UserName, Password = entity.Password };
}
public sealed class TestClient
{
    public bool Disconnected { get; set; }
    public TL.User? User { get; set; }
    public string MTProxyUrl { get; set; } = string.Empty;
    public int MaxAutoReconnects => 1;
    public int FloodRetryThreshold => 1;
    public int PingInterval => 1;
    public int MaxCodePwdAttempts => 1;
}
public sealed class TestConnection
{
    private Func<Task> _afterConnect = () => Task.CompletedTask;
    public TestClient? Client { get; set; }
    public TL.User? Me { get; set; }
    public bool Ready { get; set; }
    public int ConnectCalls { get; private set; }
    public int SavedMessagesCalls { get; private set; }
    public Guid SavedMessagesUid { get; } = Guid.NewGuid();
    public TaskCompletionSource? ConnectGate { get; set; }
    public bool StartedWhileDialogOpen { get; private set; }
    public Dictionary<string, string?> Configuration { get; } = [];
    /// <summary> Заменяет регистрацию обработчика ошибки </summary>
    public void SetupUpdateException(Action<Exception> action) { }
    /// <summary> Регистрирует исходный обработчик завершения подключения </summary>
    public void SetupAfterClientConnect(Func<Task> action) => _afterConnect = action;
    /// <summary> Выполняет исходный обратный вызов без сети </summary>
    public Task NotifyAsync() => _afterConnect();
    /// <summary> Возвращает заданный результат готовности без Telegram </summary>
    public Task<bool> CheckClientConnectionReadyAsync() => Task.FromResult(Ready);
    /// <summary> Имитирует возврат управления из подключения, включая отказ авторизации </summary>
    public async Task ConnectSessionAsync(Func<string, string?> config, TgEfProxyDto proxy, bool isDesktop)
    {
        ConnectCalls++;
        StartedWhileDialogOpen = ContentDialog.IsOpen;
        foreach (var key in new[] { "api_id", "api_hash", "phone_number", "password", "verification_code", "session_pathname" })
            Configuration[key] = config(key);
        if (ConnectGate is not null) await ConnectGate.Task;
    }
    /// <summary> Имитирует отключение без обращения к сессии </summary>
    public Task DisconnectClientAsync() { Ready = false; Client = null; Me = null; return Task.CompletedTask; }
    /// <summary> Регистрирует попытку открыть Избранное </summary>
    public Task<Guid> OpenOrCreateSavedMessagesAsync() { SavedMessagesCalls++; return Task.FromResult(SavedMessagesUid); }
}
public sealed class ShellViewModel { public Guid UidSavedMessages { get; set; } }
public sealed class TestLocator
{
    public ShellViewModel Shell { get; } = new();
    /// <summary> Возвращает тестовую оболочку </summary>
    public T Get<T>() => (T)(object)Shell;
}
public static class TgDesktopUtils
{
    /// <summary> Выполняет действие без диспетчера Windows </summary>
    public static void InvokeOnUIThread(Action action) => action();
    /// <summary> Выполняет асинхронное действие без диспетчера Windows </summary>
    public static Task InvokeOnUIThreadAsync(Func<Task> action, CancellationToken ct = default) => action();
}
public static class TgResourceExtensions
{
    /// <summary> Возвращает тестовую подпись подтверждения </summary>
    public static string GetYesButton() => "Да";
    /// <summary> Возвращает тестовую подпись отмены </summary>
    public static string GetCancelButton() => "Отмена";
    /// <summary> Возвращает тестовое сообщение </summary>
    public static string GetRpcErrorPhoneCodeInvalid() => "Неверный код";
    /// <summary> Возвращает тестовое сообщение </summary>
    public static string GetRpcErrorPasswordHashInvalid() => "Неверный пароль";
    /// <summary> Возвращает тестовое сообщение </summary>
    public static string GetRpcErrorFloodWait() => "Ожидание";
    /// <summary> Возвращает тестовое сообщение </summary>
    public static string GetRpcErrorPhonePasswordFlood() => "Лимит попыток";
    /// <summary> Возвращает тестовое состояние </summary>
    public static string GetClientIsDisconnected() => "Отключён";
    /// <summary> Возвращает тестовое состояние </summary>
    public static string GetClientIsConnected() => "Подключён";
    /// <summary> Возвращает тестовую подпись </summary>
    public static string GetClientDataRequestEmptyResponse() => "Нет ответа";
    /// <summary> Возвращает тестовый вопрос </summary>
    public static string AskClientConnect() => "Подключить?";
    /// <summary> Возвращает тестовый вопрос </summary>
    public static string AskClientDisconnect() => "Отключить?";
    /// <summary> Возвращает тестовый вопрос </summary>
    public static string AskSettingsSave() => "Сохранить?";
    /// <summary> Возвращает тестовый вопрос </summary>
    public static string AskSettingsClear() => "Очистить?";
    /// <summary> Возвращает тестовый вопрос </summary>
    public static string AskSettingsDelete() => "Удалить?";
}
