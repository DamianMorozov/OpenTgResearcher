namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static class Program
{
    // Проверяем исходную службу настроек и фактическое завершение фоновой задачи мониторинга
    private static async Task<int> Main()
    {
        var tests = new (string Name, Func<Task> Run)[]
        {
            ("SavedRussianPackaged", SavedRussianPackagedAsync),
            ("SavedRussianUnpackaged", SavedRussianUnpackagedAsync),
            ("DefaultEnglish", DefaultEnglishAsync),
            ("LanguageContinuationAfterClose", LanguageContinuationAfterCloseAsync),
            ("LanguageChangeAlreadyClosed", LanguageChangeAlreadyClosedAsync),
            ("UnchangedLanguageDoesNotRefreshUi", UnchangedLanguageDoesNotRefreshUiAsync),
            ("HardwareDisposalWaitsForWorker", HardwareDisposalWaitsForWorkerAsync),
        };
        var failures = 0;
        foreach (var test in tests)
        {
            App.IsClosing = false;
            App.MainWindow = new();
            TestLogUtils.Errors.Clear();
            Localizer.Instance.LanguageGate = null;
            Localizer.Instance.SetLanguageCalls = 0;
            Localizer.Instance.Language = string.Empty;
            try { await test.Run(); Console.WriteLine($"PASS: {test.Name}"); }
            catch (Exception ex) { failures++; Console.WriteLine($"FAIL: {test.Name}: {ex.GetBaseException().Message}"); }
        }
        Console.WriteLine($"Результат: {tests.Length - failures}/{tests.Length}");
        return failures == 0 ? 0 : 1;
    }

    // Проверяем сохранённый русский язык до создания окна Package
    private static Task SavedRussianPackagedAsync() => CheckInitialLanguageAsync("Russian", "ru-RU", isMsix: true);
    // Проверяем сохранённый русский язык до создания окна Unpackaged
    private static Task SavedRussianUnpackagedAsync() => CheckInitialLanguageAsync("Russian", "ru-RU", isMsix: false);
    // Проверяем язык по умолчанию при отсутствии настройки
    private static Task DefaultEnglishAsync() => CheckInitialLanguageAsync(null, "en-US", isMsix: false);

    // Проверяем результат исходного метода и отсутствие зарегистрированных ошибок
    private static async Task CheckInitialLanguageAsync(string? savedLanguage, string expected, bool isMsix)
    {
        TgRuntimeHelper.IsMSIX = isMsix;
        var settings = CreateSettings(savedLanguage);
        await settings.InitializeLocalizationAsync();
        Assert(Localizer.Instance.Language == expected, $"Первый язык: {Localizer.Instance.Language}, ожидался {expected}");
        Assert(TestLogUtils.Errors.Count == 0, "Инициализация языка зарегистрировала ошибку");
    }

    // Закрываем окно, пока исходный метод ожидает изменения языка
    private static async Task LanguageContinuationAfterCloseAsync()
    {
        var settings = CreateSettings("Russian");
        var root = new FrameworkElement();
        App.MainWindow.Content = root;
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Localizer.Instance.LanguageGate = gate;
        var method = typeof(TgSettingsService).GetMethod("SetLanguageFromLocalizerAsync", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Не найден метод применения языка");
        var pending = (Task)method.Invoke(settings, ["ru-RU", root])!;
        Assert(!pending.IsCompleted, "Изменение языка не ожидает асинхронную операцию");
        App.IsClosing = true;
        root.IsClosed = true;
        gate.SetResult();
        await pending.WaitAsync(TimeSpan.FromSeconds(5));
        Assert(root.LayoutCalls == 0, "Продолжение изменения языка обратилось к закрытому XAML");
        Assert(TestLogUtils.Errors.Count == 0, "Продолжение изменения языка зарегистрировало ошибку после закрытия");
    }

    // Проверяем отсутствие нового изменения языка после закрытия окна
    private static async Task LanguageChangeAlreadyClosedAsync()
    {
        var settings = CreateSettings("Russian");
        var root = new FrameworkElement { IsClosed = true };
        App.MainWindow.Content = root;
        App.IsClosing = true;
        var method = typeof(TgSettingsService).GetMethod("SetLanguageFromLocalizerAsync", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Не найден метод применения языка");
        await (Task)method.Invoke(settings, ["ru-RU", root])!;
        Assert(Localizer.Instance.SetLanguageCalls == 0 && root.LayoutCalls == 0, "После закрытия началось обновление интерфейса");
        Assert(TestLogUtils.Errors.Count == 0, "Смена языка после закрытия зарегистрировала ошибку");
    }

    // Сохранение настроек при запуске не должно повторно обновлять уже локализованную заставку
    private static Task UnchangedLanguageDoesNotRefreshUiAsync()
    {
        var settings = CreateSettings("Russian");
        var root = new FrameworkElement();
        App.MainWindow.Content = root;
        Localizer.Instance.Language = "ru-RU";
        var method = typeof(TgSettingsService).GetMethod("TryRefreshUiResources", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Не найден метод обновления интерфейса");
        method.Invoke(settings, ["ru-RU"]);
        Assert(root.LayoutCalls == 0 && Localizer.Instance.SetLanguageCalls == 0, "Неизменившийся язык вызвал повторное обновление XAML");
        return Task.CompletedTask;
    }

    // Проверяем освобождение ресурсов на управляемой фоновой задаче без доступа к оборудованию
    private static async Task HardwareDisposalWaitsForWorkerAsync()
    {
        using var container = new ContainerBuilder().Build();
        using var cache = new FusionCache(new FusionCacheOptions());
        var scope = container.BeginLifetimeScope();
        var service = (TgHardwareResourceMonitoringService)RuntimeHelpers.GetUninitializedObject(typeof(TgHardwareResourceMonitoringService));
        var cancellation = new CancellationTokenSource();
        var worker = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        SetField(service, "_computer", new LibreHardwareMonitor.Hardware.Computer());
        SetField(service, "_locker", new SemaphoreSlim(initialCount: 1, maxCount: 1));
        SetField(service, "_scope", scope);
        SetField(service, "_cache", cache);
        SetField(service, "_cts", cancellation);
        SetField(service, "_worker", worker.Task);
        var canceled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = cancellation.Token.Register(() => canceled.TrySetResult());
        var disposal = Task.Run(service.Dispose);
        bool returnedEarly;
        try
        {
            await canceled.Task.WaitAsync(TimeSpan.FromSeconds(5));
            returnedEarly = await Task.WhenAny(disposal, Task.Delay(100)) == disposal;
        }
        finally
        {
            worker.TrySetResult();
            await disposal.WaitAsync(TimeSpan.FromSeconds(5));
            scope.Dispose();
        }
        Assert(!returnedEarly, "Dispose завершился до остановки фоновой задачи и освободил используемые ею ресурсы");
        Assert(worker.Task.IsCompletedSuccessfully, "Фоновая задача не завершена");
    }

    // Создаём исходную службу с настройками в памяти
    private static TgSettingsService CreateSettings(string? language)
    {
        var settings = new TgSettingsService(new MemoryFileService(language), Options.Create(new LocalSettingsOptions()));
        App.Settings = settings;
        return settings;
    }

    // Подготавливаем внутреннее состояние настоящей службы для воспроизводимого теста завершения
    private static void SetField(object instance, string name, object value) =>
        (instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Не найдено поле {name}")).SetValue(instance, value);

    // Проверяем наблюдаемый результат
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
