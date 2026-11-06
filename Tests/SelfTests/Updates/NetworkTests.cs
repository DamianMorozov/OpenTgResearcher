namespace OpenTgResearcherDesktop.UpdateSelfTests;

internal static partial class Program
{
    // Процент файла, название и скрытие конфиденциальных данных должны работать на исходной модели
    private static async Task NetworkFileDetailsAsync()
    {
        using var state = new LoadStateService();
        var vm = new TgNetworkActivityViewModel(state, new(), new(), null!); var uid = Guid.NewGuid();
        state.NetworkActivity.Start(uid, TgNetworkOperationKind.Download);
        state.NetworkActivity.ReportTransfer(uid, "clip.mp4", 25, 100);
        await vm.OnNavigatedToAsync(null);
        var row = vm.Active.Single();
        Assert((string?)row.GetType().GetProperty("Detail")?.GetValue(row) == "clip.mp4", "Модель страницы не показывает скачиваемый файл");
        Assert((double?)row.GetType().GetProperty("Progress")?.GetValue(row) == 25, "Модель страницы потеряла процент файла");
        Assert(((string?)row.GetType().GetProperty("ProgressText")?.GetValue(row))?.Contains("25") == true, "Нет подписи процента");
        state.SetIsDisplaySensitiveData(isDisplaySensitiveData: false); vm.Refresh();
        Assert((string?)vm.Active.Single().GetType().GetProperty("Detail")?.GetValue(vm.Active.Single()) == state.SensitiveData,
            "Имя файла раскрыто при скрытых конфиденциальных данных");
    }

    // Команда страницы отменяет реальный токен парсинга и становится недоступной после запроса
    private static async Task NetworkStopParsingAsync()
    {
        using var state = new LoadStateService(); using var parsing = new CancellationTokenSource();
        var vm = new TgNetworkActivityViewModel(state, new(), new(), null!);
        var command = (IRelayCommand?)vm.GetType().GetProperty("StopParsingCommand")?.GetValue(vm);
        Assert(command is not null, "Нет команды прерывания парсинга");
        Assert(!command!.CanExecute(null), "Прерывание доступно без парсинга");
        var uid = Guid.NewGuid();
        state.NetworkActivity.Start(uid, TgNetworkOperationKind.Parsing);
        state.NetworkActivity.RegisterCancellation(uid, parsing.Cancel); vm.Refresh();
        Assert(command.CanExecute(null), "Прерывание недоступно при парсинге");
        if (command is IAsyncRelayCommand asyncCommand) await asyncCommand.ExecuteAsync(null); else command.Execute(null);
        Assert(parsing.IsCancellationRequested && !command.CanExecute(null), "Команда не запросила отмену или доступна повторно");
        Assert(vm.Active.Single().State.Contains("Остановка"), "Не показано ожидание завершения парсинга");
        state.NetworkActivity.Finish(uid, TgNetworkOperationState.Canceled); await vm.OnNavigatedToAsync(null);
        Assert(vm.IsActiveEmpty && vm.History.Single().State == "Отменено", "Остановленный парсинг остался активным");
    }

    // Отказ и незакрытый диалог не должны прерывать парсинг
    private static async Task NetworkStopConfirmationAsync()
    {
        using var state = new LoadStateService(); using var parsing = new CancellationTokenSource();
        var vm = new TgNetworkActivityViewModel(state, new(), new(), null!); var uid = Guid.NewGuid();
        state.NetworkActivity.Start(uid, TgNetworkOperationKind.Parsing);
        state.NetworkActivity.RegisterCancellation(uid, parsing.Cancel); vm.Refresh();
        var command = vm.StopParsingCommand as IAsyncRelayCommand;
        Assert(command is not null, "Прерывание не ожидает подтверждения");
        ContentDialog.Confirm = false; await command!.ExecuteAsync(null);
        Assert(!parsing.IsCancellationRequested, "Отказ в диалоге остановил парсинг");
        ContentDialog.Confirm = true; ContentDialog.Gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var pending = command.ExecuteAsync(null);
        Assert(!parsing.IsCancellationRequested && !pending.IsCompleted, "Парсинг остановлен до ответа пользователя");
        ContentDialog.Gate.SetResult(); await pending;
        Assert(parsing.IsCancellationRequested, "Подтверждение не остановило парсинг");
    }

    // Кнопка паузы меняет подпись и не отменяет токен
    private static async Task NetworkPauseCommandAsync()
    {
        using var state = new LoadStateService(); using var parsing = new CancellationTokenSource();
        var vm = new TgNetworkActivityViewModel(state, new(), new(), null!); var uid = Guid.NewGuid();
        state.NetworkActivity.Start(uid, TgNetworkOperationKind.Parsing);
        state.NetworkActivity.RegisterCancellation(uid, parsing.Cancel); await vm.OnNavigatedToAsync(null);
        var command = vm.GetType().GetProperty("PauseParsingCommand")?.GetValue(vm) as IRelayCommand;
        Assert(command is not null && command.CanExecute(null), "Нет доступной команды паузы");
        command!.Execute(null);
        Assert(!parsing.IsCancellationRequested && vm.Active.Single().State.Contains("паузе"), "Пауза не отражена или отменила парсинг");
        Assert((string?)vm.GetType().GetProperty("PauseButtonText")?.GetValue(vm) == "Продолжить", "Не показано продолжение");
        Localizer.Language = "en-US"; vm.Refresh();
        Assert((string?)vm.GetType().GetProperty("PauseButtonText")?.GetValue(vm) == "Resume", "Продолжение не переведено");
        command.Execute(null); Assert(vm.Active.Single().State == "Running", "Продолжение не сняло паузу");
    }

    // Проверяем настоящую VM страницы с общим учётом операций и обоими языками
    private static async Task NetworkPageViewModelAsync()
    {
        using var state = new LoadStateService();
        var vm = new TgNetworkActivityViewModel(state, new(), new(), null!);
        var connection = Guid.NewGuid(); var parsing = Guid.NewGuid(); var download = Guid.NewGuid();
        state.NetworkActivity.Start(connection, TgNetworkOperationKind.Connection);
        state.NetworkActivity.Start(parsing, TgNetworkOperationKind.Parsing);
        state.NetworkActivity.Start(download, TgNetworkOperationKind.Download);
        state.NetworkActivity.Finish(connection, TgNetworkOperationState.Completed);
        state.NetworkActivity.Finish(download, TgNetworkOperationState.Failed);
        await vm.OnNavigatedToAsync(null);
        Assert(vm.Active.Count == 1 && vm.Active[0].Title == "Парсинг чата", "Активный парсинг потерялся во VM");
        Assert(vm.History.Count == 2 && vm.History.Any(x => x.State.Contains("Ошибка")), "Завершение и ошибка не отражены в истории");
        Assert(!vm.IsActiveEmpty && !vm.IsHistoryEmpty, "Списки скрыты пустым состоянием");
        var historyRow = vm.History[0];
        vm.Refresh();
        Assert(ReferenceEquals(historyRow, vm.History[0]), "Опрос таймера пересоздаёт неизменную историю и сбрасывает элементы интерфейса");
        state.SetIsOnlineReady(isReady: true);
        Localizer.Language = "en-US"; vm.Refresh();
        Assert(vm.Active[0].Title == "Chat parsing" && vm.ConnectionStatus == "Telegram connected", "Динамические строки не переведены");
        vm.ClearHistoryCommand.Execute(null);
        Assert(vm.Active.Count == 1 && vm.History.Count == 0 && vm.IsHistoryEmpty, "Очистка потеряла парсинг или сохранила историю");
        state.NetworkActivity.Finish(parsing, TgNetworkOperationState.Canceled); vm.Refresh();
        Assert(vm.IsActiveEmpty && vm.History.Single().State == "Canceled", "Отмена не появилась на странице");
    }
}
