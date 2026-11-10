namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static partial class Program
{
    // Обновлятор получает управление после остановки парсинга и освобождения сессии, пока окно ещё существует
    private static async Task UpdateWaitsForSessionReleaseAsync()
    {
        var client = new ParsingStopConnection();
        App.BusinessLogicManager = new() { ConnectClient = client };
        App.MainWindow = new();
        var schedules = 0;
        var close = new App().CloseForUpdateAsync(() =>
        {
            Assert(client.DisposeCalls == 1 && App.MainWindow.CloseCalls == 0, "Обновлятор запущен до освобождения сессии или после закрытия окна");
            schedules++;
        });
        await client.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert(schedules == 0 && !close.IsCompleted, "Перезапуск не ожидает остановки парсинга");
        client.Finished.TrySetResult();
        await close.WaitAsync(TimeSpan.FromSeconds(5));
        Assert(schedules == 1 && App.MainWindow.CloseCalls == 1, "Перезапуск не запланирован или окно не закрыто");
    }

    // Ошибка запуска обновлятора оставляет окно открытым и возвращается вызывающей команде
    private static async Task UpdateFailureKeepsWindowOpenAsync()
    {
        var client = new ParsingStopConnection();
        client.Finished.TrySetResult();
        App.BusinessLogicManager = new() { ConnectClient = client };
        var failed = false;
        try { await new App().CloseForUpdateAsync(() => throw new IOException("Нет доступа к обновлению")); }
        catch (IOException) { failed = true; }
        Assert(failed && App.MainWindow.CloseCalls == 0 && !App.IsClosing, "Ошибка запуска обновлятора скрыта или окно закрыто");
    }

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

    // Исходный обработчик обязан ждать парсеры до вызова Dispose и закрытия окна
    private static async Task CloseWaitsForParsingAsync()
    {
        var client = new ParsingStopConnection();
        App.BusinessLogicManager = new() { ConnectClient = client };
        App.MainWindow = new();
        var close = CloseMainWindowAsync(new App());
        try
        {
            await client.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));
            Assert(!close.IsCompleted && client.DisposeCalls == 0 && App.MainWindow.CloseCalls == 0,
                "Отключение клиента началось до завершения парсинга");
        }
        finally { client.Finished.TrySetResult(); await close; }
        Assert(client.DisposeCalls == 1 && App.MainWindow.CloseCalls == 1, "После завершения парсинга окно не закрылось");
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

internal sealed class ParsingStopConnection : IDisposable
{
    public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Finished { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public int DisposeCalls { get; private set; }
    /// <summary> Регистрирует вызов отключения транспорта </summary>
    public void Dispose() => DisposeCalls++;
}
