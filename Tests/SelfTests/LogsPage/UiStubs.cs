namespace OpenTgResearcherDesktop.SelfTests;

// Подменяем только окружение WinUI и службы страницы; операции с файлами выполняет исходная модель представления
public interface ILoadStateService;
public interface ITgSettingsService;
public interface INavigationService;
public interface ITgPageViewModel;
public interface ILogger<T>;
public sealed class NavigationEventArgs;
public enum ContentDialogButton { Close }
public enum ContentDialogResult { None, Primary }

public abstract class TgPageViewModelBase : ObservableObject
{
    public object? XamlRootVm { get; set; } = new();
    public List<Exception> Errors { get; } = [];

    protected TgPageViewModelBase(object? loadStateService, object? settingsService, object? navigationService, object? logger, string name) { }

    /// <summary> Вызывает загрузку исходной модели без окружения WinUI </summary>
    public virtual Task OnNavigatedToAsync(NavigationEventArgs? e) => Task.CompletedTask;

    /// <summary> Выполняет загрузку в потоке селфтеста </summary>
    public Task LoadStorageDataAsync(Func<Task> action) => action();

    /// <summary> Заменяет обновление несвязанных с журналами элементов интерфейса </summary>
    public Task ReloadUiAsync() => Task.CompletedTask;

    /// <summary> Сохраняет ошибки исходной модели для проверки </summary>
    public void LogError(Exception exception, string message) => Errors.Add(exception);
}

public sealed class ContentDialog
{
    private readonly TaskCompletionSource<ContentDialogResult> _result = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public static ContentDialog? LastShown { get; private set; }
    public bool IsOpen { get; private set; }
    public object? XamlRoot { get; set; }
    public object? Tag { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PrimaryButtonText { get; set; } = string.Empty;
    public string CloseButtonText { get; set; } = string.Empty;
    public ContentDialogButton DefaultButton { get; set; }
    public event Action<ContentDialog, ContentDialogButtonClickEventArgs>? PrimaryButtonClick;

    /// <summary> Ожидает закрытия диалога перед продолжением команды </summary>
    public Task<ContentDialogResult> ShowAsync() { LastShown = this; IsOpen = true; return _result.Task; }

    /// <summary> Подтверждает удаление и проверяет завершение асинхронного обработчика </summary>
    public async Task ConfirmAsync()
    {
        var args = new ContentDialogButtonClickEventArgs();
        PrimaryButtonClick?.Invoke(this, args);
        if (args.IsDeferred) await args.Completion.Task.WaitAsync(TimeSpan.FromSeconds(10));
        IsOpen = false;
        _result.SetResult(ContentDialogResult.Primary);
    }

    /// <summary> Закрывает диалог без подтверждения удаления </summary>
    public void Cancel() { IsOpen = false; _result.SetResult(ContentDialogResult.None); }
}

public sealed class ContentDialogButtonClickEventArgs
{
    internal TaskCompletionSource Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal bool IsDeferred { get; private set; }

    /// <summary> Возвращает отсрочку закрытия диалога </summary>
    public DialogDeferral GetDeferral() { IsDeferred = true; return new(Completion); }
}

public sealed class DialogDeferral(TaskCompletionSource completion)
{
    /// <summary> Завершает обработку подтверждения удаления </summary>
    public void Complete() => completion.SetResult();
}

public static class TestLogUtils
{
    public static string DirectoryPath { get; set; } = string.Empty;
    public static int CloseCalls { get; set; }

    /// <summary> Направляет исходную модель в отдельный каталог селфтеста </summary>
    public static string GetLogsDirectory(TgEnumAppType appType) => DirectoryPath;

    /// <summary> Регистрирует нежелательное завершение журнала при удалении файла </summary>
    public static Task CloseAndFlushAsync() { CloseCalls++; return Task.CompletedTask; }
}

public static class TgResourceExtensions
{
    /// <summary> Возвращает текст ошибки чтения </summary>
    public static string GetErrorOccurredWhileLoadingLogs() => "Не удалось загрузить журнал";
    /// <summary> Возвращает заголовок подтверждения </summary>
    public static string AskDeleteFile() => "Удалить файл?";
    /// <summary> Возвращает подпись подтверждения </summary>
    public static string GetYesButton() => "Да";
    /// <summary> Возвращает подпись отмены </summary>
    public static string GetCancelButton() => "Отмена";
}
