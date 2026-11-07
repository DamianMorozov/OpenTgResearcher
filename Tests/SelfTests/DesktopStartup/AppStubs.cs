namespace OpenTgResearcherDesktop;

public partial class App
{
    public static bool IsClosing { get; set; }
    public static TestWindow MainWindow { get; set; } = new();
    public static ITgSettingsService? Settings { get; set; }
    public static TestBusinessLogicManager BusinessLogicManager { get; set; } = new();
    /// <summary> Возвращает исходную службу настроек </summary>
    public static T GetService<T>() where T : class => typeof(T) == typeof(TgSplashScreenViewModel)
        ? (T)(object)SplashPageState.ViewModel : (T)Settings!;
}

public sealed class TestBusinessLogicManager { public IDisposable ConnectClient { get; set; } = null!; }

// Подменяем только контракт ожидания для прежних тестовых IDisposable и вызываем настоящий метод клиента
internal static class ShutdownClientExtensions
{
    public static Task StopParsingAsync(this IDisposable client, CancellationToken ct = default)
    {
        if (client is ParsingStopConnection controlled)
        {
            controlled.Started.TrySetResult();
            return controlled.Finished.Task.WaitAsync(ct);
        }
        var method = client.GetType().GetMethod("StopParsingAsync");
        return method is null ? Task.CompletedTask : (Task)method.Invoke(client, [ct])!;
    }
}
