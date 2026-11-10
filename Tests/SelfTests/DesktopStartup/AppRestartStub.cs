namespace OpenTgResearcherDesktop;

public static class AppInstance
{
    public static Func<AppRestartFailureReason>? RestartHandler { get; set; }
    /// <summary> Проверяет момент запроса перезапуска без запуска второго экземпляра Desktop </summary>
    public static AppRestartFailureReason Restart(string arguments) => RestartHandler?.Invoke() ?? AppRestartFailureReason.RestartPending;
}
