namespace OpenTgResearcherDesktop;

internal static class Program
{
    internal static bool IsFirstRun { get; private set; }

    // Обрабатываем служебные запуски установщика до создания WinUI и сервисов приложения
    [STAThread]
    private static void Main()
    {
        VelopackApp.Build().OnFirstRun(_ => IsFirstRun = true).Run();
        WinRT.ComWrappersSupport.InitializeComWrappers();
        Application.Start(InitializeApplication);
    }

    // Сохраняем инициализацию потока интерфейса из стандартной точки входа WinUI
    private static void InitializeApplication(ApplicationInitializationCallbackParams args)
    {
        var context = new Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
        SynchronizationContext.SetSynchronizationContext(context);
        _ = new App();
    }
}
