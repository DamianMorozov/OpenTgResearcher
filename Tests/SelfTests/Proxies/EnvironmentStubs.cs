namespace OpenTgResearcherDesktop.ProxySelfTests;

public interface ILoadStateService;
public interface ITgSettingsService { bool IsExistsAppStorage { get; } }
public sealed class TestSettings : ITgSettingsService { public bool IsExistsAppStorage => true; }
public interface ILogger<out T>;
public sealed class DoubleTappedRoutedEventArgs;
public sealed class DataGrid { public object? SelectedItem { get; set; } }
public sealed class INavigationService
{
    public object? Parameter { get; private set; }
    public string? LastKey { get; private set; }
    /// <summary> Запоминает переход без окна Windows </summary>
    public bool NavigateTo(string key, object? parameter = null) { LastKey = key; Parameter = parameter; return true; }
}
public sealed class NavigationEventArgs { public object? Parameter { get; set; } }
public enum ContentDialogResult { None, Primary }
public enum ContentDialogButton { Close }
public sealed class ContentDialog
{
    public static bool Confirm { get; set; }
    public static bool IsOpen { get; private set; }
    public static Action? OnShow { get; set; }
    public object? XamlRoot { get; set; }
    public string Title { get; set; } = "";
    public string PrimaryButtonText { get; set; } = "";
    public string CloseButtonText { get; set; } = "";
    public ContentDialogButton DefaultButton { get; set; }
    public IAsyncRelayCommand? PrimaryButtonCommand { get; set; }
    /// <summary> Повторяет жизненный цикл подтверждения, включая старую команду кнопки </summary>
    public Task<ContentDialogResult> ShowAsync()
    {
        IsOpen = true;
        OnShow?.Invoke();
        if (Confirm) PrimaryButtonCommand?.Execute(null);
        IsOpen = false;
        return Task.FromResult(Confirm ? ContentDialogResult.Primary : ContentDialogResult.None);
    }
}
public abstract class TgPageViewModelBase : ObservableObject
{
    protected ITgSettingsService SettingsService { get; }
    public INavigationService NavigationService { get; }
    public object? XamlRootVm { get; set; } = new();
    public TgExceptionViewModel Exception { get; } = new();
    public List<Exception> Errors { get; } = [];
    public int OnlineCalls { get; private set; }
    public bool StorageStartedWhileDialogOpen { get; private set; }
    protected TgPageViewModelBase(ILoadStateService state, ITgSettingsService settings, INavigationService navigation, object logger, string name)
    { SettingsService = settings; NavigationService = navigation; }
    /// <summary> Заменяет навигацию без платформы Windows </summary>
    public virtual Task OnNavigatedToAsync(NavigationEventArgs? e) => Task.CompletedTask;
    /// <summary> Выполняет настоящие операции с изолированной SQLite </summary>
    public async Task LoadStorageDataAsync(Func<Task> action)
    {
        StorageStartedWhileDialogOpen |= ContentDialog.IsOpen;
        await action();
    }
    // Повторяем старый диалог, чтобы исходная VM обнаружила запуск действия до закрытия
    protected async Task ContentDialogAsync(Func<Task> action, string title, TgEnumLoadDesktopType loadType)
    {
        if (loadType == TgEnumLoadDesktopType.Online) OnlineCalls++;
        await new ContentDialog { PrimaryButtonCommand = new AsyncRelayCommand(action) }.ShowAsync();
    }
    /// <summary> Сохраняет ошибку для проверки журналирования </summary>
    public void LogError(Exception ex) => Errors.Add(ex);
}
public static class TgResourceExtensions
{
    /// <summary> Возвращает тестовую подпись </summary>
    public static string GetYesButton() => "Да";
    /// <summary> Возвращает тестовую подпись </summary>
    public static string GetCancelButton() => "Отмена";
    /// <summary> Возвращает тестовую подпись </summary>
    public static string AskDataClear() => "Очистить?";
    /// <summary> Возвращает тестовую подпись </summary>
    public static string AskLoading() => "Перезагрузить?";
    /// <summary> Возвращает тестовую подпись </summary>
    public static string AskSettingsSave() => "Сохранить?";
    /// <summary> Возвращает тестовую подпись </summary>
    public static string AskDeleteRecord() => "Удалить?";
    /// <summary> Возвращает тестовую подпись </summary>
    public static string AskDeleteAllRecords() => "Удалить всё?";
    /// <summary> Возвращает тестовую подпись </summary>
    public static string AskCreateDefault() => "Создать стандартную запись?";
}
public static class App { public static TestManager BusinessLogicManager { get; } = new(); }
public sealed class TestManager { public TestStorage StorageManager { get; } = new(); }
public sealed class TestStorage { public ITgEfProxyRepository ProxyRepository { get; set; } = null!; }
public sealed class TestContext(DbContextOptions options) : TgEfContextBase(options)
{
    public TaskCompletionSource? SaveGate { get; set; }
    /// <summary> Позволяет проверить ожидание реальной записи SQLite командой страницы </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        if (SaveGate is not null) await SaveGate.Task.WaitAsync(ct);
        return await base.SaveChangesAsync(ct);
    }
    // Используем только подключение SQLite в памяти, без настроек пользовательской БД
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
}
