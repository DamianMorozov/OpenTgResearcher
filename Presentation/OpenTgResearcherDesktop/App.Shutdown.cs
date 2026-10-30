namespace OpenTgResearcherDesktop;

public partial class App
{
    private bool _canClose;

    // Задерживаем уничтожение окна до завершения отключения Telegram
    private async void MainWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (_canClose) return;
        args.Cancel = true;
        try { await CloseMainWindowAsync(); }
        catch (Exception ex) { TgLogUtils.WriteException(ex); }
    }

    // Ожидаем освобождения клиента при работающем диспетчере интерфейса
    private async Task CloseMainWindowAsync()
    {
        if (IsClosing) return;
        IsClosing = true;
        try
        {
            TgLogUtils.WriteLog("Завершение приложения: отключение Telegram");
            await Task.Run(() => BusinessLogicManager.ConnectClient.Dispose());
            TgLogUtils.WriteLog("Завершение приложения: Telegram отключён, сессия освобождена");
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
