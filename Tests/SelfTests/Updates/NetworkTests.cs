namespace OpenTgResearcherDesktop.UpdateSelfTests;

internal static partial class Program
{
    // Текущий запрос виден до получения ответа, переводится и сохраняется при отмене без раскрытия названия чата
    private static async Task NetworkParsingMessageIdsAsync()
    {
        using var state = new LoadStateService();
        var vm = new TgNetworkActivityViewModel(state, new(), new(), null!);
        var uid = Guid.NewGuid();
        state.NetworkActivity.Start(uid, TgNetworkOperationKind.Parsing);
        state.NetworkActivity.ReportParsing(uid, "Тестовый чат", 100, 1000);
        await vm.OnNavigatedToAsync(null);
        Assert(!vm.Active.Single().Caption.Contains("Чтение ID"), "До первого запроса показан выдуманный ID");
        var report = typeof(TgNetworkActivityService).GetMethod("ReportParsingRequest");
        Assert(report is not null, "Сетевая активность не принимает ID текущего запроса сообщений");
        report!.Invoke(state.NetworkActivity, [uid, 101L, 1000L]); vm.Refresh();
        Assert(vm.Active.Single().Caption.Contains("Чтение ID 101 из 1000"), "Нет текущего ID и последнего ID чата");
        Assert(vm.Active.Single().Progress == 10, "Начало запроса изменило процент уже прочитанных сообщений");
        state.NetworkActivity.ReportParsing(uid, "Тестовый чат", 150, 1000);
        vm.Refresh();
        Assert(vm.Active.Single().Caption.Contains("Чтение ID 101 из 1000") && vm.Active.Single().Progress == 15,
            "Ответ на порцию подменил запрашиваемый ID процентом или потерял продвижение");
        report.Invoke(state.NetworkActivity, [uid, 151L, 1200L]);
        state.SetIsDisplaySensitiveData(isDisplaySensitiveData: false); vm.Refresh();
        Assert(vm.Active.Single().Caption.Contains("Чтение ID 151 из 1200") && !vm.Active.Single().Caption.Contains("Тестовый чат"),
            "Обновление ID потерялось или раскрыло название чата");
        Localizer.Language = "en-US"; vm.Refresh();
        Assert(vm.Active.Single().Caption.Contains("Reading ID 151 of 1200"), "Текущий ID не переведён на английский");
        state.NetworkActivity.Finish(uid, TgNetworkOperationState.Canceled);
        report.Invoke(state.NetworkActivity, [uid, 999L, 1200L]); vm.Refresh();
        Assert(vm.History.Single().Caption.Contains("Reading ID 151 of 1200"), "Поздний отчёт изменил отменённую операцию");
        var unknown = Guid.NewGuid(); state.NetworkActivity.Start(unknown, TgNetworkOperationKind.Parsing);
        report.Invoke(state.NetworkActivity, [unknown, 15L, 0L]); vm.Refresh();
        Assert(vm.Active.Single().Caption.Contains("Reading ID 15") && !vm.Active.Single().Caption.Contains(" of 0"),
            "Неизвестная граница чата показана как ноль");
        var download = Guid.NewGuid(); state.NetworkActivity.Start(download, TgNetworkOperationKind.Download);
        state.NetworkActivity.ReportTransfer(download, "clip.mp4", 25, 100);
        report.Invoke(state.NetworkActivity, [download, 2L, 10L]); vm.Refresh();
        var file = vm.Active.Single(x => x.Title == "File download");
        Assert(!file.Caption.Contains("Reading ID") && file.Progress == 25, "ID сообщений попал в строку скачивания файла");
    }

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
        var canceled = Guid.NewGuid(); state.NetworkActivity.Start(canceled, TgNetworkOperationKind.Request);
        state.NetworkActivity.Finish(canceled, TgNetworkOperationState.Canceled);
        await vm.OnNavigatedToAsync(null);
        Assert(vm.Active.Count == 1 && vm.Active[0].Title == "Парсинг чата", "Активный парсинг потерялся во VM");
        Assert(GroupRows(ChatGroups(vm).Single()).Any(x => x.State.Contains("Ошибка")), "Ошибка потерялась из группы");
        Assert(vm.History.Count == 3 && vm.History.Any(x => x.State == "Отменено"), "Завершения и отмена потерялись в истории");
        Assert(!vm.IsActiveEmpty && !vm.IsActivityEmpty, "Списки скрыты пустым состоянием");
        var historyRow = vm.History[0];
        vm.Refresh();
        Assert(ReferenceEquals(historyRow, vm.History[0]), "Опрос таймера пересоздаёт неизменную историю и сбрасывает элементы интерфейса");
        state.SetIsOnlineReady(isReady: true);
        Localizer.Language = "en-US"; vm.Refresh();
        Assert(vm.Active[0].Title == "Chat parsing" && vm.ConnectionStatus == "Telegram connected", "Динамические строки не переведены");
        vm.ClearHistoryCommand.Execute(null);
        Assert(vm.Active.Count == 1 && vm.History.Count == 0 && !vm.IsActivityEmpty, "Очистка потеряла парсинг или сохранила историю");
        state.NetworkActivity.Finish(parsing, TgNetworkOperationState.Canceled); vm.Refresh();
        Assert(vm.IsActiveEmpty && vm.History.Single().State == "Canceled", "Отмена не появилась на странице");
    }
}
