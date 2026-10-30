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

public sealed class TestWindow
{
    public object? Content { get; set; }
    public int CloseCalls { get; private set; }
    /// <summary> Запоминает закрытие без платформы Windows </summary>
    public void Close() => CloseCalls++;
}
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
    public StorageFolder LocalFolder { get; set; } = new(AppContext.BaseDirectory);
}
public sealed class StorageFolder(string path)
{
    public string Path { get; } = path;
    /// <summary> Открывает временный каталог вместо платформенного хранилища </summary>
    public static Task<StorageFolder> GetFolderFromPathAsync(string path) => Task.FromResult(new StorageFolder(path));
    /// <summary> Создаёт настоящий каталог внутри селфтеста </summary>
    public Task<StorageFolder> CreateFolderAsync(string name, CreationCollisionOption option)
    {
        var directory = Directory.CreateDirectory(System.IO.Path.Combine(Path, name));
        return Task.FromResult(new StorageFolder(directory.FullName));
    }
    /// <summary> Проверяет наличие настоящего файла в каталоге селфтеста </summary>
    public Task<object?> TryGetItemAsync(string name)
    {
        var filePath = System.IO.Path.Combine(Path, name);
        return Task.FromResult<object?>(File.Exists(filePath) ? new StorageFile(filePath) : null);
    }
}
public enum NameCollisionOption { ReplaceExisting }
public sealed class StorageFile(string path)
{
    public static string PackageDirectory { get; set; } = AppContext.BaseDirectory;
    public string Path { get; } = path;
    /// <summary> Открывает настоящий файл из временного аналога пакета </summary>
    public static Task<StorageFile> GetFileFromApplicationUriAsync(Uri uri)
    {
        var filePath = System.IO.Path.Combine(PackageDirectory, uri.AbsolutePath.TrimStart('/'));
        if (!File.Exists(filePath)) throw new FileNotFoundException("Не найден ресурс тестового пакета", filePath);
        return Task.FromResult(new StorageFile(filePath));
    }
    /// <summary> Копирует ресурс без перезаписи существующего файла </summary>
    public Task<StorageFile> CopyAsync(StorageFolder folder) => CopyCoreAsync(folder, System.IO.Path.GetFileName(Path), overwrite: false);
    /// <summary> Копирует ресурс с заменой существующего файла </summary>
    public Task<StorageFile> CopyAsync(StorageFolder folder, string fileName, NameCollisionOption option) =>
        CopyCoreAsync(folder, fileName, overwrite: option == NameCollisionOption.ReplaceExisting);

    // Выполняем настоящую файловую операцию для проверки содержимого и ошибок копирования
    private Task<StorageFile> CopyCoreAsync(StorageFolder folder, string fileName, bool overwrite)
    {
        var destination = System.IO.Path.Combine(folder.Path, fileName);
        File.Copy(Path, destination, overwrite);
        return Task.FromResult(new StorageFile(destination));
    }
}
public sealed class LocalizerOptions { public string DefaultLanguage { get; set; } = string.Empty; }
public sealed class LocalizerBuilder
{
    private readonly LocalizerOptions _options = new();
    private string _resourcesPath = string.Empty;
    /// <summary> Запоминает каталог ресурсов, переданный исходной службой </summary>
    public LocalizerBuilder AddStringResourcesFolderForLanguageDictionaries(string path) { _resourcesPath = path; return this; }
    /// <summary> Выполняет исходную настройку языка локализатора </summary>
    public LocalizerBuilder SetOptions(Action<LocalizerOptions> action) { action(_options); return this; }
    /// <summary> Фиксирует язык, выбранный службой до создания интерфейса </summary>
    public Task<Localizer> Build()
    {
        Localizer.Instance.Language = _options.DefaultLanguage;
        Localizer.Instance.ResourcesAtBuild.Clear();
        foreach (var language in Localizer.Instance.GetAvailableLanguages())
        {
            var resourcePath = Path.Combine(_resourcesPath, language, "Resources.resw");
            if (File.Exists(resourcePath)) Localizer.Instance.ResourcesAtBuild[language] = File.ReadAllText(resourcePath);
        }
        return Task.FromResult(Localizer.Instance);
    }
}
public sealed class Localizer
{
    public static Localizer Instance { get; } = new();
    public string Language { get; set; } = string.Empty;
    public TaskCompletionSource? LanguageGate { get; set; }
    public int SetLanguageCalls { get; set; }
    public Dictionary<string, string> ResourcesAtBuild { get; } = [];
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
