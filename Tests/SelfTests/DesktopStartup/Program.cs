namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static class Program
{
    // Проверяем исходную службу настроек и фактическое завершение фоновой задачи мониторинга
    private static async Task<int> Main()
    {
        var tests = new (string Name, Func<Task> Run)[]
        {
            ("MsixCreatesMissingResources", MsixCreatesMissingResourcesAsync),
            ("MsixUpdatesChangedResourceSize", MsixUpdatesChangedResourceSizeAsync),
            ("MsixPreservesUnchangedResources", MsixPreservesUnchangedResourcesAsync),
            ("MsixLogsMissingPackageResource", MsixLogsMissingPackageResourceAsync),
            ("MsixLogsLockedResource", MsixLogsLockedResourceAsync),
            ("SavedRussianPackaged", SavedRussianPackagedAsync),
            ("SavedRussianUnpackaged", SavedRussianUnpackagedAsync),
            ("DefaultEnglish", DefaultEnglishAsync),
            ("LanguageContinuationAfterClose", LanguageContinuationAfterCloseAsync),
            ("LanguageChangeAlreadyClosed", LanguageChangeAlreadyClosedAsync),
            ("UnchangedLanguageDoesNotRefreshUi", UnchangedLanguageDoesNotRefreshUiAsync),
            ("HardwareDisposalWaitsForWorker", HardwareDisposalWaitsForWorkerAsync),
        };
        var outputDirectory = Path.Combine(AppContext.BaseDirectory, "cases", Guid.NewGuid().ToString("N"));
        var failures = 0;
        foreach (var test in tests)
        {
            App.IsClosing = false;
            App.MainWindow = new();
            TestLogUtils.Errors.Clear();
            Localizer.Instance.LanguageGate = null;
            Localizer.Instance.SetLanguageCalls = 0;
            Localizer.Instance.Language = string.Empty;
            Localizer.Instance.ResourcesAtBuild.Clear();
            try
            {
                PrepareResourceFiles(Path.Combine(outputDirectory, test.Name));
                await test.Run();
                Console.WriteLine($"PASS: {test.Name}");
            }
            catch (Exception ex) { failures++; Console.WriteLine($"FAIL: {test.Name}: {ex.GetBaseException().Message}"); }
        }
        Console.WriteLine($"Результат: {tests.Length - failures}/{tests.Length}");
        return failures == 0 ? 0 : 1;
    }

    // Подготавливаем изолированные каталоги пакета и LocalState без доступа к профилю пользователя
    private static void PrepareResourceFiles(string directory)
    {
        StorageFile.PackageDirectory = Path.Combine(directory, "package");
        ApplicationData.Current.LocalFolder = new StorageFolder(Path.Combine(directory, "LocalState"));
        foreach (var language in Localizer.Instance.GetAvailableLanguages())
        {
            var folder = Path.Combine(StorageFile.PackageDirectory, "Strings", language);
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "Resources.resw"), $"<root><data name=\"Greeting\"><value>{language}</value></data></root>");
        }
    }

    // Проверяем создание обоих ресурсов до построения локализатора при первом запуске Package
    private static async Task MsixCreatesMissingResourcesAsync()
    {
        TgRuntimeHelper.IsMSIX = true;
        await CreateSettings("Russian").InitializeLocalizationAsync();
        AssertResourceContents();
        Assert(TestLogUtils.Errors.Count == 0, "Создание отсутствующих ресурсов зарегистрировало ошибку");
    }

    // Проверяем обновление существующих ресурсов, отличающихся размером от файлов пакета
    private static async Task MsixUpdatesChangedResourceSizeAsync()
    {
        foreach (var language in Localizer.Instance.GetAvailableLanguages())
        {
            var destination = GetLocalResourcePath(language);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.WriteAllText(destination, "Old resource");
        }
        TgRuntimeHelper.IsMSIX = true;
        await CreateSettings("Russian").InitializeLocalizationAsync();
        AssertResourceContents();
        Assert(TestLogUtils.Errors.Count == 0, "Обновление ресурсов зарегистрировало ошибку");
    }

    // Проверяем сохранение содержимого и времени записи файлов того же размера при повторном запуске
    private static async Task MsixPreservesUnchangedResourcesAsync()
    {
        var writeTimes = new Dictionary<string, DateTime>();
        foreach (var language in Localizer.Instance.GetAvailableLanguages())
        {
            var destination = GetLocalResourcePath(language);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(Path.Combine(StorageFile.PackageDirectory, "Strings", language, "Resources.resw"), destination);
            File.SetLastWriteTimeUtc(destination, DateTime.UnixEpoch.AddDays(1));
            writeTimes[language] = File.GetLastWriteTimeUtc(destination);
        }
        TgRuntimeHelper.IsMSIX = true;
        await CreateSettings("Russian").InitializeLocalizationAsync();
        AssertResourceContents();
        foreach (var item in writeTimes)
            Assert(File.GetLastWriteTimeUtc(GetLocalResourcePath(item.Key)) == item.Value, $"Неизменившийся ресурс {item.Key} перезаписан");
        Assert(TestLogUtils.Errors.Count == 0, "Проверка неизменившихся ресурсов зарегистрировала ошибку");
    }

    // Проверяем регистрацию ошибки отсутствующего ресурса в пакете без фиктивного успешного копирования
    private static async Task MsixLogsMissingPackageResourceAsync()
    {
        File.Delete(Path.Combine(StorageFile.PackageDirectory, "Strings", "ru-RU", "Resources.resw"));
        TgRuntimeHelper.IsMSIX = true;
        await CreateSettings("Russian").InitializeLocalizationAsync();
        Assert(TestLogUtils.Errors.Any(x => x is FileNotFoundException), "Отсутствие ресурса в пакете не зарегистрировано в журнале");
        Assert(!File.Exists(GetLocalResourcePath("ru-RU")), "Создан фиктивный ресурс вместо отсутствующего файла пакета");
        Assert(File.Exists(GetLocalResourcePath("en-US")), "Не скопирован доступный ресурс перед ошибкой второго языка");
    }

    // Проверяем журналирование ошибки записи без потери содержимого заблокированного файла
    private static async Task MsixLogsLockedResourceAsync()
    {
        var destination = GetLocalResourcePath("ru-RU");
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.WriteAllText(destination, "Locked resource");
        TgRuntimeHelper.IsMSIX = true;
        using (var lockedFile = new FileStream(destination, FileMode.Open, FileAccess.Read, FileShare.None))
            await CreateSettings("Russian").InitializeLocalizationAsync();
        Assert(TestLogUtils.Errors.Any(x => x is IOException), "Ошибка записи заблокированного ресурса не зарегистрирована");
        Assert(File.ReadAllText(destination) == "Locked resource", "Ошибка копирования повредила существующий ресурс");
    }

    // Проверяем настоящий результат копирования для обоих языков и порядок подготовки локализатора
    private static void AssertResourceContents()
    {
        foreach (var language in Localizer.Instance.GetAvailableLanguages())
        {
            var destination = GetLocalResourcePath(language);
            Assert(File.Exists(destination), $"Не создан отсутствующий ресурс {language}/Resources.resw");
            var expected = File.ReadAllText(Path.Combine(StorageFile.PackageDirectory, "Strings", language, "Resources.resw"));
            Assert(File.ReadAllText(destination) == expected, $"Содержимое ресурса {language} отличается от пакета");
            Assert(Localizer.Instance.ResourcesAtBuild.TryGetValue(language, out var content) && content == expected,
                $"Ресурс {language} не подготовлен до построения локализатора");
        }
    }

    // Возвращаем путь к ресурсу во временном аналоге LocalState
    private static string GetLocalResourcePath(string language) => Path.Combine(ApplicationData.Current.LocalFolder.Path, "Strings", language, "Resources.resw");

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
