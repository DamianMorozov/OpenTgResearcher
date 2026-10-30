namespace OpenTgResearcherDesktop;

public partial class App
{
    public static bool IsClosing { get; set; }
    public static TestWindow MainWindow { get; set; } = new();
    public static ITgSettingsService? Settings { get; set; }
    public static TestBusinessLogicManager BusinessLogicManager { get; set; } = new();
    /// <summary> Возвращает исходную службу настроек </summary>
    public static T GetService<T>() where T : class => (T)Settings!;
}

public sealed class TestBusinessLogicManager { public IDisposable ConnectClient { get; set; } = null!; }
