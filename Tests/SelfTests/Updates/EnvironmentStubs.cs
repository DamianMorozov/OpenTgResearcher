namespace OpenTgResearcherDesktop.UpdateSelfTests;

// Подменяем только среду WinUI и источник пакетов, исходные VM, службу навигации и чтение строк компилируем без изменений
public sealed class ITgSettingsService
{
    public bool IsExistsAppStorage { get; set; } = true;
    public TgEnumLanguage AppLanguage { get; set; }
    public TgEnumTheme AppTheme { get; set; }
    public int LanguageWrites { get; private set; }
    public int ThemeWrites { get; private set; }
    /// <summary> Учитывает сохранение выбранного языка </summary>
    public void SaveLanguage(TgEnumLanguage language) { AppLanguage = language; LanguageWrites++; }
    /// <summary> Учитывает сохранение выбранной темы </summary>
    public void SaveTheme(TgEnumTheme theme) { AppTheme = theme; ThemeWrites++; }
}
public interface ILogger<out T>;
public sealed class INavigationService
{
    public string? LastPage { get; private set; }
    public int Navigations { get; private set; }
    /// <summary> Запоминает переход для проверки команды настроек </summary>
    public bool NavigateTo(string pageKey) { LastPage = pageKey; Navigations++; return true; }
    /// <summary> Заменяет возврат на предыдущую страницу </summary>
    public bool GoBack() => true;
}
public interface INavigationViewService;
public sealed class IPageService
{
    /// <summary> Возвращает тип тестовой страницы </summary>
    public Type GetPageType(string pageKey) => typeof(TgSettingsViewModel);
}
public sealed class TgSettingsViewModel;
public sealed class NavigationEventArgs;
public sealed class RoutedEventArgs;
public sealed class NavigationViewBackRequestedEventArgs;
public sealed class NavigationViewItemInvokedEventArgs
{
    public bool IsSettingsInvoked { get; set; }
    public object? InvokedItemContainer { get; set; }
}
public sealed class NavigationViewItem
{
    public string Uid { get; set; } = string.Empty;
    public string Content { get; set; } = "Settings";
    public string AutomationName { get; private set; } = "Settings";
    public List<object> MenuItems { get; } = [];
    /// <summary> Заменяет чтение свойства навигации </summary>
    public object? GetValue(object property) => null;
    /// <summary> Заменяет установку имени элемента для средств доступности </summary>
    public void SetValue(object property, object value) => AutomationName = (string)value;
}
public sealed class NavigationView
{
    public List<object> MenuItems { get; } = [];
    public List<object> FooterMenuItems { get; } = [];
    public object SettingsItem { get; } = new NavigationViewItem();
    public event Action<object, RoutedEventArgs>? Loaded;
    public event Action<NavigationView, NavigationViewBackRequestedEventArgs>? BackRequested;
    public event Action<NavigationView, NavigationViewItemInvokedEventArgs>? ItemInvoked;
    /// <summary> Вызывает загрузку после создания встроенного пункта настроек </summary>
    public void RaiseLoaded() => Loaded?.Invoke(this, new());
    /// <summary> Повторяет выбор встроенного пункта настроек </summary>
    public void InvokeSettings() => ItemInvoked?.Invoke(this, new() { IsSettingsInvoked = true });
    /// <summary> Вызывает обработчик возврата </summary>
    public void GoBack() => BackRequested?.Invoke(this, new());
}
public static class Uids
{
    /// <summary> Регистрирует UID после Loaded без повторного вызова загрузки элемента </summary>
    public static void SetUid(NavigationViewItem item, string uid) => item.Uid = uid;
}
public static class TgNavigationHelper { public static object NavigateToProperty { get; } = new(); }
public enum ContentDialogResult { None, Primary }
public enum ContentDialogButton { Close }
public sealed class ContentDialog
{
    public static bool Confirm { get; set; } = true;
    public static bool IsOpen { get; private set; }
    public static Task? LastAction { get; private set; }
    public static TaskCompletionSource? Gate { get; set; }
    public object? XamlRoot { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PrimaryButtonText { get; set; } = string.Empty;
    public string CloseButtonText { get; set; } = string.Empty;
    public ContentDialogButton DefaultButton { get; set; }
    public IAsyncRelayCommand? PrimaryButtonCommand { get; set; }
    /// <summary> Повторяет WinUI: команда кнопки запускается до закрытия диалога и не ожидается им </summary>
    public async Task<ContentDialogResult> ShowAsync()
    {
        IsOpen = true;
        LastAction = null;
        if (Gate is not null) await Gate.Task;
        if (Confirm && PrimaryButtonCommand is not null)
        {
            PrimaryButtonCommand.Execute(null);
            LastAction = PrimaryButtonCommand.ExecutionTask;
        }
        IsOpen = false;
        return Confirm ? ContentDialogResult.Primary : ContentDialogResult.None;
    }
}
public abstract class TgPageViewModelBase : ObservableObject
{
    public ILoadStateService LoadStateService { get; }
    public object? XamlRootVm { get; set; } = new();
    public List<Exception> Errors { get; } = [];
    public int StorageCalls { get; private set; }
    public int TelegramChecks { get; private set; }
    protected TgPageViewModelBase(ILoadStateService state, ITgSettingsService settings, INavigationService navigation, object logger, string name)
        => LoadStateService = state;
    /// <summary> Заменяет навигацию без сети </summary>
    public virtual Task OnNavigatedToAsync(NavigationEventArgs? e) => Task.CompletedTask;
    /// <summary> Учитывает ненужную проверку Telegram из базовой перезагрузки страницы </summary>
    public virtual Task ReloadUiAsync() { TelegramChecks++; return Task.CompletedTask; }
    /// <summary> Учитывает ненужную операцию с хранилищем </summary>
    public Task LoadStorageDataAsync(Func<Task> action) { StorageCalls++; return action(); }
    // Повторяем исходный базовый диалог, чтобы обнаружить потерю ожидания операции
    protected async Task ContentDialogAsync(Func<Task> action, string title, TgEnumLoadDesktopType loadType)
    {
        if (XamlRootVm is null) return;
        var dialog = new ContentDialog { PrimaryButtonCommand = new AsyncRelayCommand(action) };
        _ = await dialog.ShowAsync();
    }
    /// <summary> Сохраняет оригинальное исключение для проверки журнала </summary>
    public void LogError(Exception ex) => Errors.Add(ex);
}
public static class App { public static bool IsClosing { get; set; } }
public static class TgDesktopUtils
{
    /// <summary> Выполняет действие без диспетчера WinUI </summary>
    public static Task InvokeOnUIThreadAsync(Func<Task> action, CancellationToken ct = default) => action();
    /// <summary> Выполняет изменение прогресса без диспетчера WinUI </summary>
    public static void InvokeOnUIThread(Action action) => action();
}
public sealed class LanguageChangedEventArgs : EventArgs;
public interface ILocalizer
{
    event EventHandler<LanguageChangedEventArgs>? LanguageChanged;
    string GetLocalizedString(string key);
}
public sealed class Localizer : ILocalizer
{
    private static string _language = "ru-RU";
    public static string Language
    {
        get => _language;
        set { _language = value; _instance.LanguageChanged?.Invoke(_instance, new()); }
    }
    private static readonly Localizer _instance = new();
    public event EventHandler<LanguageChangedEventArgs>? LanguageChanged;
    /// <summary> Возвращает локализатор настоящих ресурсов </summary>
    public static ILocalizer Get() => _instance;
    /// <summary> Читает ресурс, отсутствие которого считается ошибкой теста </summary>
    public string GetLocalizedString(string key)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Strings", Language, "Resources.resw");
        // WinUI3Localizer объединяет свойства Content и Text по UID и возвращает последнюю строку этой группы
        var entry = XDocument.Load(path).Root!.Elements("data").LastOrDefault(item => ((string?)item.Attribute("name"))?.Split('.')[0] == key);
        return entry?.Element("value")?.Value ?? string.Empty;
    }
}
public static class TestConstants
{
    public const string LinkGitHub = "https://github.com/test/application";
}
public sealed class GithubSource(string url, string token, bool prerelease)
{
    public bool Prerelease { get; } = prerelease;
    public string Url { get; } = url;
    public string Token { get; } = token;
}
public sealed class UpdateInfo
{
    public TestAsset TargetFullRelease { get; } = new();
}
public sealed class TestAsset { public Version Version { get; } = new(0, 8, 81); }
public sealed class TestUpdateState
{
    public bool Installed { get; set; } = true;
    public UpdateInfo? Update { get; set; }
    public int Created { get; set; }
    public int Checks { get; set; }
    public int Downloads { get; set; }
    public bool Preview { get; set; }
    public bool StartedInsideDialog { get; set; }
    public bool DownloadCancelled { get; set; }
    public Exception? CheckError { get; set; }
    public Exception? DownloadError { get; set; }
    public TaskCompletionSource? CheckGate { get; set; }
    public TaskCompletionSource? DownloadGate { get; set; }
    public Action<int>? Progress { get; set; }
}
public sealed class UpdateManager
{
    public static TestUpdateState State { get; set; } = new();
    public bool IsInstalled => State.Installed;
    public Version CurrentVersion { get; } = new(0, 8, 80);
    public UpdateManager(GithubSource source) { State.Created++; State.Preview = source.Prerelease; }
    /// <summary> Имитирует границу сети и удерживает задачу для проверки ожидания и одновременных команд </summary>
    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        State.Checks++;
        State.StartedInsideDialog |= ContentDialog.IsOpen;
        if (State.CheckGate is not null) await State.CheckGate.Task;
        if (State.CheckError is not null) throw State.CheckError;
        return State.Update;
    }
    /// <summary> Передаёт прогресс, отмену и ошибки загрузки пакета в исходную VM </summary>
    public async Task DownloadUpdatesAsync(UpdateInfo update, Action<int>? progress = null, CancellationToken cancelToken = default)
    {
        State.Downloads++;
        State.Progress = progress;
        progress?.Invoke(37);
        try
        {
            if (State.DownloadGate is not null) await State.DownloadGate.Task.WaitAsync(cancelToken);
            if (State.DownloadError is not null) throw State.DownloadError;
            cancelToken.ThrowIfCancellationRequested();
            progress?.Invoke(100);
        }
        catch (OperationCanceledException) { State.DownloadCancelled = true; throw; }
    }
}
