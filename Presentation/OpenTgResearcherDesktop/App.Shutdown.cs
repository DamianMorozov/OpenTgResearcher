namespace OpenTgResearcherDesktop;

public partial class App
{
    private bool _canClose;

    /// <summary> Ожидает остановки парсинга и освобождения сессии перед применением обновления </summary>
    internal static Task RestartForUpdateAsync(UpdateManager manager, VelopackAsset target) =>
        ((App)Current).CloseForUpdateAsync(() => manager.WaitExitThenApplyUpdates(target, silent: true, restart: true));

    // Используем штатное завершение с отложенным запуском установщика
    internal Task CloseForUpdateAsync(Action scheduleUpdate) => CloseCoreAsync(scheduleUpdate);

    // Задерживаем уничтожение окна до завершения отключения Telegram
    private async void MainWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (_canClose) return;
        args.Cancel = true;
        try { await CloseMainWindowAsync(); }
        catch (Exception ex) { TgLogUtils.WriteException(ex); }
    }

    // Ожидаем освобождения клиента при работающем диспетчере интерфейса
    private Task CloseMainWindowAsync() => CloseCoreAsync(null);

    // Запускаем обновление после отключения Telegram, непосредственно перед закрытием окна
    private async Task CloseCoreAsync(Action? scheduleUpdate)
    {
        if (IsClosing)
        {
            if (scheduleUpdate is not null) throw new InvalidOperationException("Приложение уже завершает работу. Обновление применится при следующем запуске");
            return;
        }
        IsClosing = true;
        try
        {
            TgLogUtils.WriteLog("Завершение приложения: остановка парсинга всех чатов");
            await BusinessLogicManager.ConnectClient.StopParsingAsync();
            TgLogUtils.WriteLog("Завершение приложения: отключение Telegram");
            await Task.Run(() => BusinessLogicManager.ConnectClient.Dispose());
            TgLogUtils.WriteLog("Завершение приложения: Telegram отключён, сессия освобождена");
            scheduleUpdate?.Invoke();
            _canClose = true;
            MainWindow.Close();
        }
        catch
        {
            IsClosing = false;
            throw;
        }
    }
}
