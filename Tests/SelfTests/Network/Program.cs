namespace OpenTgResearcherDesktop.NetworkSelfTests;

internal static class Program
{
    private static readonly string _root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../.."));

    // Проверяем исходный учёт операций, а не имитацию сетевой загрузки
    private static async Task<int> Main()
    {
        var tests = new (string Name, Func<Task> Run)[]
        {
            ("ParallelOperations", ParallelOperationsAsync), ("ErrorAndCancellation", ErrorAndCancellationAsync),
            ("ClearKeepsActive", ClearKeepsActiveAsync), ("HistoryLimit", HistoryLimitAsync),
            ("DownloadStateIntegration", DownloadStateIntegrationAsync), ("ToolbarPlacement", ToolbarPlacementAsync),
            ("NetworkPage", NetworkPageAsync), ("Localization", LocalizationAsync),
            ("TransferProgress", TransferProgressAsync), ("ParsingCancellation", ParsingCancellationAsync),
            ("ParsingPause", ParsingPauseAsync), ("CompactRows", CompactRowsAsync)
        };
        var failures = 0;
        foreach (var test in tests)
        {
            try { await test.Run(); Console.WriteLine($"PASS: {test.Name}"); }
            catch (Exception ex) { failures++; Console.WriteLine($"FAIL: {test.Name}: {ex.GetBaseException().Message}"); }
        }
        return failures == 0 ? 0 : 1;
    }

    // Получаем реальный экземпляр без зависимости компиляции от ещё отсутствующего API
    private static object Tracker(LoadStateService state) => state.GetType().GetProperty("NetworkActivity")?.GetValue(state)
        ?? throw new InvalidOperationException("Нет общего учёта сетевых операций");

    // Пауза удерживает реальную работу, продолжение освобождает её, а отмена прерывает ожидание
    private static async Task ParsingPauseAsync()
    {
        using var state = new LoadStateService(); using var cancel = new CancellationTokenSource();
        var tracker = state.NetworkActivity; var uid = Guid.NewGuid();
        tracker.Start(uid, TgBusinessLogic.Models.TgNetworkOperationKind.Parsing);
        tracker.RegisterCancellation(uid, cancel.Cancel);
        var toggle = tracker.GetType().GetMethod("ToggleParsingPause") ?? throw new InvalidOperationException("Нет паузы парсинга");
        var wait = tracker.GetType().GetMethod("WaitIfPausedAsync") ?? throw new InvalidOperationException("Нет ожидания продолжения парсинга");
        toggle.Invoke(tracker, null);
        var blocked = (Task)wait.Invoke(tracker, [uid, cancel.Token])!;
        Assert(!blocked.IsCompleted, "Пауза не приостановила работу");
        toggle.Invoke(tracker, null); await blocked.WaitAsync(TimeSpan.FromSeconds(2));
        toggle.Invoke(tracker, null);
        var stopped = (Task)wait.Invoke(tracker, [uid, cancel.Token])!;
        tracker.CancelParsing();
        try { await stopped.WaitAsync(TimeSpan.FromSeconds(2)); throw new InvalidOperationException("Остановка паузы не отменила ожидание"); }
        catch (OperationCanceledException) { }
    }

    // Строка операции остаётся одной строкой даже для длинного имени файла
    private static Task CompactRowsAsync()
    {
        var page = Load("Views/TgNetworkActivityPage.xaml");
        var template = page.Descendants().Single(x => (string?)x.Attribute("{http://schemas.microsoft.com/winfx/2006/xaml}Key") == "OperationRow");
        Assert(!template.Descendants().Any(x => x.Name.LocalName == "StackPanel" && (string?)x.Attribute("Orientation") != "Horizontal"),
            "Строка содержит вертикальную стопку подписей");
        Assert(!template.Descendants().Any(x => (string?)x.Attribute("TextWrapping") == "Wrap"), "Подписи строки переносятся на несколько строк");
        Assert(page.Descendants().Any(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "NetworkPauseParsing"), "Нет кнопки паузы");
        return Task.CompletedTask;
    }

    // Снимок должен содержать настоящие активные и завершённые операции
    private static object[] Items(object tracker) => ((IEnumerable)tracker.GetType().GetProperty("Operations")!.GetValue(tracker)!).Cast<object>().ToArray();

    // Читаем наблюдаемое состояние операции
    private static string State(object item) => item.GetType().GetProperty("State")!.GetValue(item)!.ToString()!;

    // Запускаем реальное действие через проверяемый учёт
    private static Task Run(object tracker, string kind, Func<Task> action, CancellationToken ct = default)
    {
        var method = tracker.GetType().GetMethods().Single(x => x.Name == "RunAsync" && x.GetParameters()[1].ParameterType == typeof(Func<Task>));
        return (Task)method.Invoke(tracker, [Enum.Parse(method.GetParameters()[0].ParameterType, kind), action, ct])!;
    }

    // Параллельные действия остаются независимыми после завершения одного из них
    private static async Task ParallelOperationsAsync()
    {
        using var state = new LoadStateService(); var tracker = Tracker(state);
        var gate = new TaskCompletionSource();
        var first = Run(tracker, "Connection", () => gate.Task);
        await Run(tracker, "Request", () => Task.CompletedTask);
        Assert(Items(tracker).Count(x => State(x) == "Running") == 1 && Items(tracker).Count(x => State(x) == "Completed") == 1,
            "Завершение запроса потеряло активное подключение");
        gate.SetResult(); await first;
        Assert(Items(tracker).Length == 2 && Items(tracker).All(x => State(x) == "Completed"), "Не сохранён результат операций");
    }

    // Ошибки и отмена сохраняют исходное поведение и отличаются в истории
    private static async Task ErrorAndCancellationAsync()
    {
        using var state = new LoadStateService(); var tracker = Tracker(state);
        try { await Run(tracker, "Download", () => throw new IOException("Синтетическая ошибка")); throw new Exception("Ошибка скрыта"); }
        catch (IOException) { }
        using var cts = new CancellationTokenSource(); cts.Cancel();
        try { await Run(tracker, "Parsing", () => Task.Delay(20, cts.Token), cts.Token); throw new Exception("Отмена скрыта"); }
        catch (OperationCanceledException) { }
        Assert(Items(tracker).Any(x => State(x) == "Failed") && Items(tracker).Any(x => State(x) == "Canceled"), "Состояния ошибки и отмены потеряны");
    }

    // Очистка истории не останавливает и не скрывает активную загрузку
    private static async Task ClearKeepsActiveAsync()
    {
        using var state = new LoadStateService(); var tracker = Tracker(state); var gate = new TaskCompletionSource();
        var running = Run(tracker, "Download", () => gate.Task);
        await Run(tracker, "Request", () => Task.CompletedTask);
        tracker.GetType().GetMethod("ClearCompleted")!.Invoke(tracker, null);
        Assert(Items(tracker).Length == 1 && State(Items(tracker)[0]) == "Running", "Очистка потеряла активную операцию");
        gate.SetResult(); await running;
    }

    // Длительное использование не накапливает неограниченную историю
    private static async Task HistoryLimitAsync()
    {
        using var state = new LoadStateService(); var tracker = Tracker(state);
        for (var i = 0; i < 140; i++) await Run(tracker, "Request", () => Task.CompletedTask);
        Assert(Items(tracker).Length == 100, "История не ограничена последними ста операциями");
    }

    // Сетевые загрузки из общего сервиса видны без открытия новой страницы
    private static async Task DownloadStateIntegrationAsync()
    {
        using var state = new LoadStateService(); var tracker = Tracker(state); var uid = Guid.NewGuid();
        await state.PrepareDownloadTokenAsync(uid);
        Assert(Items(tracker).Any(x => State(x) == "Running"), "Настоящая загрузка не появилась в учёте");
        state.StopHardDownloadProcessing();
        Assert(Items(tracker).All(x => State(x) == "Canceled"), "Остановка загрузки не отразилась в истории");
    }

    // Обновление страницы находится непосредственно перед выбором языка вне области перетаскивания
    private static Task ToolbarPlacementAsync()
    {
        var root = Load("Views/ShellPage.xaml");
        var refresh = root.Descendants().SingleOrDefault(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "ToolbarRefresh");
        Assert(refresh is not null, "Обновление страницы отсутствует в верхней панели");
        Assert(!refresh!.Ancestors().Any(x => x.Name.LocalName == "DataTemplate"), "Кнопка осталась в шаблоне заголовка");
        Assert(refresh.Attribute("Command")!.Value.Contains("UpdatePageCommand"), "Кнопка не вызывает обновление текущей страницы");
        var language = root.Descendants().Single(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "ToolbarLanguage");
        Assert(language.ElementsBeforeSelf().LastOrDefault() == refresh, "Обновление страницы не находится непосредственно перед выбором языка");
        Assert(!refresh.Ancestors().Any(x => (string?)x.Attribute("{http://schemas.microsoft.com/winfx/2006/xaml}Name") == "TitleBarDragRegion"),
            "Кнопка обновления попала в область перетаскивания");
        Assert(!root.Descendants().Where(x => x.Name.LocalName == "DataTemplate").Descendants().Any(x => x.Name.LocalName == "Button"),
            "В заголовке осталась дублирующая кнопка");
        return Task.CompletedTask;
    }

    // У страницы есть навигация, независимое открытие и отдельные активные и завершённые списки
    private static Task NetworkPageAsync()
    {
        var page = Load("Views/TgNetworkActivityPage.xaml");
        foreach (var id in new[] { "NetworkActive", "NetworkHistory", "NetworkClear", "NetworkEmpty", "NetworkStopParsing",
            "NetworkOperationDetail", "NetworkOperationProgress" })
            Assert(page.Descendants().Any(x => (string?)x.Attribute("AutomationProperties.AutomationId") == id), $"Нет области {id}");
        Assert(Load("Views/ShellPage.xaml").ToString().Contains("TgNetworkActivityViewModel"), "Нет пункта меню сети");
        return Task.CompletedTask;
    }

    // Размер и имя файла сохраняются с настоящим процентом, поздние события не меняют завершение
    private static Task TransferProgressAsync()
    {
        using var state = new LoadStateService();
        var tracker = state.NetworkActivity; var uid = Guid.NewGuid();
        tracker.Start(uid, TgBusinessLogic.Models.TgNetworkOperationKind.Download);
        var report = tracker.GetType().GetMethod("ReportTransfer") ?? throw new InvalidOperationException("Нет учёта прогресса файла");
        report.Invoke(tracker, [uid, "clip.mp4", 25L, 100L]);
        var item = tracker.Operations.Single();
        Assert((string?)item.GetType().GetProperty("Detail")?.GetValue(item) == "clip.mp4", "Потеряно имя скачиваемого файла");
        Assert((double?)item.GetType().GetProperty("Progress")?.GetValue(item) == 25, "Не рассчитан процент скачивания");
        report.Invoke(tracker, [uid, "clip.mp4", 250L, 100L]);
        Assert((double?)tracker.Operations.Single().GetType().GetProperty("Progress")?.GetValue(tracker.Operations.Single()) == 100,
            "Процент вышел за пределы 100");
        tracker.Finish(uid, TgBusinessLogic.Models.TgNetworkOperationState.Canceled);
        report.Invoke(tracker, [uid, "late.mp4", 1L, 100L]);
        Assert((string?)tracker.Operations.Single().GetType().GetProperty("Detail")?.GetValue(tracker.Operations.Single()) == "clip.mp4",
            "Поздний прогресс изменил завершённую операцию");
        return Task.CompletedTask;
    }

    // Прерывание отменяет токен парсинга, сохраняя независимую проверку обновлений
    private static async Task ParsingCancellationAsync()
    {
        using var state = new LoadStateService(); using var parsing = new CancellationTokenSource();
        var tracker = state.NetworkActivity; var uid = Guid.NewGuid(); var update = Guid.NewGuid();
        tracker.Start(uid, TgBusinessLogic.Models.TgNetworkOperationKind.Parsing);
        tracker.Start(update, TgBusinessLogic.Models.TgNetworkOperationKind.Updates);
        var register = tracker.GetType().GetMethod("RegisterCancellation") ?? throw new InvalidOperationException("Нет отмены отдельного парсинга");
        register.Invoke(tracker, [uid, (Action)parsing.Cancel]);
        var cancel = tracker.GetType().GetMethod("CancelParsing") ?? throw new InvalidOperationException("Нет команды отмены парсинга");
        var running = Task.Delay(Timeout.Infinite, parsing.Token);
        cancel.Invoke(tracker, null);
        try { await running.WaitAsync(TimeSpan.FromSeconds(2)); throw new InvalidOperationException("Парсинг не остановлен"); }
        catch (OperationCanceledException) { }
        Assert(tracker.Operations.Single(x => x.Id == update).State == TgBusinessLogic.Models.TgNetworkOperationState.Running,
            "Прерывание парсинга остановило обновление программы");
        tracker.Finish(uid, TgBusinessLogic.Models.TgNetworkOperationState.Canceled);
        cancel.Invoke(tracker, null);
        Assert(tracker.Operations.Single(x => x.Id == uid).State == TgBusinessLogic.Models.TgNetworkOperationState.Canceled, "Отмена потеряла состояние");
    }

    // Подписи команд различимы на обоих языках
    private static Task LocalizationAsync()
    {
        foreach (var language in new[] { "ru-RU", "en-us" })
        {
            var root = Load($"Strings/{language}/Resources.resw");
            foreach (var name in new[] { "ToolbarRefresh", "ToolbarUpdateApplication", "NetworkTitle.Text", "NetworkEmpty.Text", "NetworkScope.Text" })
                Assert(root.Descendants("data").Any(x => (string?)x.Attribute("name") == name), $"Нет перевода {language}/{name}");
        }
        return Task.CompletedTask;
    }

    // Читаем исходники решения с относительной привязкой тестового проекта
    private static XDocument Load(string path) => XDocument.Load(Path.Combine(_root, "Presentation/OpenTgResearcherDesktop", path));

    // Любое отсутствие результата завершает проверку ошибкой
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
