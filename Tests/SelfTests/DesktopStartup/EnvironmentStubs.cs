namespace OpenTgResearcherDesktop.StartupSelfTests;

// Подменяем платформенное окружение, сохраняя исходный код службы настроек
public interface IFileService
{
    T Read<T>(string folderPath, string fileName);
    void Save<T>(string folderPath, string fileName, T content);
}

public sealed class MemoryFileService(string? language) : IFileService
{
    /// <summary> Возвращает сохранённый язык без доступа к настройкам пользователя </summary>
    public T Read<T>(string folderPath, string fileName) => (T)(object)new Dictionary<string, object>
        { [nameof(LocalSettingsOptions.AppLanguage)] = language ?? "Default" };

    /// <summary> Не записывает настройки пользователя </summary>
    public void Save<T>(string folderPath, string fileName, T content) { }
}

public static class App
{
    public static bool IsClosing { get; set; }
    public static TestWindow MainWindow { get; set; } = new();
    public static ITgSettingsService? Settings { get; set; }
    /// <summary> Возвращает исходную службу настроек </summary>
    public static T GetService<T>() where T : class => (T)Settings!;
}

public sealed class TestWindow { public object? Content { get; set; } }
public class FrameworkElement
{
    public object? RequestedTheme { get; set; }
    public bool IsClosed { get; set; }
    public int LayoutCalls { get; private set; }
    /// <summary> Обнаруживает обращение к уже закрытому интерфейсу </summary>
    public void UpdateLayout()
    {
        LayoutCalls++;
        if (IsClosed) throw new InvalidOperationException("Обращение к закрытому XAML");
    }
}
public sealed class Page : FrameworkElement { public Frame? Frame { get; set; } }
public sealed class Frame : FrameworkElement
{
    public Type CurrentSourcePageType { get; } = typeof(Page);
    public object? Tag { get; set; }
    public List<object> BackStack { get; } = [];
    /// <summary> Проверяет доступ к интерфейсу при повторной навигации </summary>
    public void Navigate(Type pageType, object? parameter) => UpdateLayout();
}

public static class TgRuntimeHelper { public static bool IsMSIX { get; set; } }
public static class TgThemeUtils
{
    /// <summary> Возвращает тестовое значение темы </summary>
    public static object GetElementTheme(TgEnumTheme theme) => theme;
}
public static class TgTitleBarHelper
{
    /// <summary> Заменяет оформление заголовка окна </summary>
    public static void UpdateTitleBar(object theme) { }
}
public static class TgDesktopUtils
{
    /// <summary> Выполняет действие без реального диспетчера WinUI </summary>
    public static void InvokeOnUIThread(Action action) => action();
}
public static class TestGlobalTools { public static string AppStorage { get; set; } = string.Empty; }
public static class TestLogUtils
{
    public static List<Exception> Errors { get; } = [];
    /// <summary> Возвращает каталог селфтеста </summary>
    public static string GetAppDirectory(TgEnumAppType appType) => AppContext.BaseDirectory;
    /// <summary> Сохраняет ошибку исходной службы </summary>
    public static void WriteException(Exception exception) => Errors.Add(exception);
    /// <summary> Сохраняет ошибку исходной службы </summary>
    public static void WriteExceptionWithMessage(Exception exception, string message) => Errors.Add(exception);
    /// <summary> Заменяет запись журнала без доступа к файлам пользователя </summary>
    public static void WriteLog(string message) { }
}

public static class JsonUtils
{
    /// <summary> Сериализует значение настройки в памяти </summary>
    public static string Stringify<T>(T value) => JsonSerializer.Serialize(value);
}
public enum CreationCollisionOption { OpenIfExists }
public sealed class ApplicationData
{
    public static ApplicationData Current { get; } = new();
    public StorageFolder LocalFolder { get; } = new();
}
public sealed class StorageFolder
{
    public string Path { get; } = AppContext.BaseDirectory;
    /// <summary> Заменяет открытие каталога ресурсов </summary>
    public static Task<StorageFolder> GetFolderFromPathAsync(string path) => Task.FromResult(new StorageFolder());
    /// <summary> Заменяет создание каталога ресурсов </summary>
    public Task<StorageFolder> CreateFolderAsync(string name, CreationCollisionOption option) => Task.FromResult(new StorageFolder());
    /// <summary> Возвращает отсутствие файла без обращения к профилю пользователя </summary>
    public Task<object?> TryGetItemAsync(string name) => Task.FromResult<object?>(null);
}
public sealed class StorageFile
{
    public string Path { get; } = string.Empty;
    /// <summary> Заменяет получение ресурса из пакета </summary>
    public static Task<StorageFile> GetFileFromApplicationUriAsync(Uri uri) => Task.FromResult(new StorageFile());
    /// <summary> Заменяет копирование ресурса </summary>
    public Task<StorageFile> CopyAsync(StorageFolder folder) => Task.FromResult(new StorageFile());
}
public sealed class LocalizerOptions { public string DefaultLanguage { get; set; } = string.Empty; }
public sealed class LocalizerBuilder
{
    private readonly LocalizerOptions _options = new();
    /// <summary> Заменяет регистрацию каталога ресурсов </summary>
    public LocalizerBuilder AddStringResourcesFolderForLanguageDictionaries(string path) => this;
    /// <summary> Выполняет исходную настройку языка локализатора </summary>
    public LocalizerBuilder SetOptions(Action<LocalizerOptions> action) { action(_options); return this; }
    /// <summary> Фиксирует язык, выбранный службой до создания интерфейса </summary>
    public Task<Localizer> Build() { Localizer.Instance.Language = _options.DefaultLanguage; return Task.FromResult(Localizer.Instance); }
}
public sealed class Localizer
{
    public static Localizer Instance { get; } = new();
    public string Language { get; set; } = string.Empty;
    public TaskCompletionSource? LanguageGate { get; set; }
    public int SetLanguageCalls { get; set; }
    /// <summary> Возвращает тестовый локализатор </summary>
    public static Localizer Get() => Instance;
    /// <summary> Возвращает уже применённый язык </summary>
    public string GetCurrentLanguage() => Language;
    /// <summary> Возвращает доступные языки </summary>
    public string[] GetAvailableLanguages() => ["en-US", "ru-RU"];
    /// <summary> Позволяет закрыть окно во время асинхронного изменения языка </summary>
    public async Task SetLanguage(string language)
    {
        SetLanguageCalls++;
        if (LanguageGate is not null) await LanguageGate.Task;
        Language = language;
    }
}
