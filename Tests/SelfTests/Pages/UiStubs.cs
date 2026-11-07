namespace OpenTgResearcherDesktop.PageSelfTests;

public static class TgDesktopUtils
{
    /// <summary> Выполняет работу без диспетчера, сохраняя отмену и исключение </summary>
    public static Task InvokeOnUIThreadAsync(Func<Task> action, CancellationToken ct = default) => action();
    /// <summary> Выполняет синхронное изменение модели страницы </summary>
    public static void InvokeOnUIThread(Action action) => action();
}
