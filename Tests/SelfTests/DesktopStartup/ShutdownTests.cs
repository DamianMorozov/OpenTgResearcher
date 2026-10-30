namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static partial class Program
{
    // Проверяем порядок на исходном обработчике завершения приложения
    private static async Task CloseWaitsForTelegramAsync()
    {
        App.MainWindow = new();
        var client = new BlockingConnection();
        App.BusinessLogicManager = new() { ConnectClient = client };
        var app = new App();
        var close = CloseMainWindowAsync(app);
        try
        {
            await client.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert(!close.IsCompleted && App.MainWindow.CloseCalls == 0 && App.IsClosing, "Окно закрывается до завершения отключения Telegram");
            await CloseMainWindowAsync(app);
            Assert(client.DisposeCalls == 1, "Повторное закрытие повторно освобождает клиента");
        }
        finally { client.Completion.Set(); }
        await close.WaitAsync(TimeSpan.FromSeconds(5));
        Assert(client.IsDisposed && App.MainWindow.CloseCalls == 1, "Клиент не отключён или окно не закрыто после отключения");
    }

    // Ошибка завершения должна оставаться видимой и не приводить к уничтожению окна
    private static async Task FailedDisconnectKeepsWindowOpenAsync()
    {
        App.MainWindow = new();
        App.BusinessLogicManager = new() { ConnectClient = new FailingConnection() };
        var app = new App();
        var failed = false;
        try { await CloseMainWindowAsync(app); }
        catch (InvalidOperationException ex) when (ex.Message == "Тестовая ошибка отключения") { failed = true; }
        Assert(failed && !App.IsClosing && App.MainWindow.CloseCalls == 0, "Ошибка отключения скрыта или окно закрыто до освобождения клиента");
    }

    // Вызываем настоящий метод приложения без создания WinUI
    private static Task CloseMainWindowAsync(App app)
    {
        var method = typeof(App).GetMethod("CloseMainWindowAsync", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Приложение не ожидает отключения Telegram до закрытия окна");
        return (Task)method.Invoke(app, null)!;
    }
}

internal sealed class BlockingConnection : IDisposable
{
    public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public ManualResetEventSlim Completion { get; } = new(initialState: false);
    public int DisposeCalls { get; private set; }
    public bool IsDisposed { get; private set; }
    /// <summary> Управляем временем освобождения клиента без сети </summary>
    public void Dispose()
    {
        if (IsDisposed) return;
        DisposeCalls++;
        Started.TrySetResult();
        if (!Completion.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("Селфтест не разрешил завершение отключения");
        IsDisposed = true;
    }
}

internal sealed class FailingConnection : IDisposable
{
    /// <summary> Воспроизводит ошибку завершения клиента </summary>
    public void Dispose() => throw new InvalidOperationException("Тестовая ошибка отключения");
}
