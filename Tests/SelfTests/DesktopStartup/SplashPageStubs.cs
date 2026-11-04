namespace OpenTgResearcherDesktop.StartupSelfTests;

public enum Visibility { Visible, Collapsed }
public sealed class RoutedEventArgs : EventArgs;
public sealed class UISettings { public bool AnimationsEnabled => true; }

// Подменяем загрузку ресурсов и WinUI, сохраняя исходную последовательность этапов страницы
public class SplashPageBase
{
    public virtual TgSplashScreenViewModel ViewModel => null!;
    public FrameworkElement? GridContent { get; set; }
    public object? DataContext { get; set; }
    public event Action<object, RoutedEventArgs>? Loaded;
    public event Action<object, RoutedEventArgs>? Unloaded;
    /// <summary> Заменяет уведомление модели о появлении XAML </summary>
    protected void PageLoaded(object sender, RoutedEventArgs e) { }
    /// <summary> Вызывает настоящий обработчик загрузки страницы </summary>
    public void RaiseLoaded() => Loaded?.Invoke(this, new());
    /// <summary> Вызывает настоящий обработчик ухода со страницы </summary>
    public void RaiseUnloaded() => Unloaded?.Invoke(this, new());
}
public static class SplashPageState { public static TgSplashScreenViewModel ViewModel { get; set; } = new(); }
public sealed class TgSplashScreenViewModel
{
    public bool IsLoadComplete { get; set; }
    public Action BackToMainWindow { get; set; } = () => { };
    public TaskCompletionSource StorageGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource HardwareStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    /// <summary> Заменяет журналирование без задержки теста </summary>
    public Task LoadingLoggingAsync() => Task.CompletedTask;
    /// <summary> Заменяет проверку установщика </summary>
    public Task LoadingVelopackInstallerAsync() => Task.CompletedTask;
    /// <summary> Заменяет чтение настроек </summary>
    public Task LoadingSettingsAsync() => Task.CompletedTask;
    /// <summary> Удерживает фактическую последовательность страницы на этапе хранилища </summary>
    public Task LoadingStorageAsync() => StorageGate.Task;
    /// <summary> Заменяет чтение лицензии </summary>
    public Task LoadingLicenseAsync() => Task.CompletedTask;
    /// <summary> Заменяет подготовку уведомлений </summary>
    public Task LoadingNotificationsAsync() => Task.CompletedTask;
    /// <summary> Заменяет запуск оборудования </summary>
    public void LoadingHardwareControl() => HardwareStarted.TrySetResult();
    /// <summary> Завершает загрузку только при вызове исходной страницей </summary>
    public Task LoadingCompleteAsync() { IsLoadComplete = true; return Task.CompletedTask; }
}
public sealed class SplashBindings
{
    /// <summary> Заменяет сформированное XAML-компилятором обновление привязок </summary>
    public void Update() { }
}
