namespace OpenTgResearcherDesktop.SelfTests;

internal static class Program
{
    // Запускаем проверки на исходной модели представления с реальными временными файлами
    private static async Task<int> Main()
    {
        var outputDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "cases", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(outputDirectory);
        var tests = new (string Name, Func<Task> Run)[]
        {
            ("ReadActiveLog", ReadActiveLogAsync),
            ("DeleteSelectedFile", DeleteSelectedFileAsync),
            ("DeleteLockedFile", DeleteLockedFileAsync),
            ("DeleteWithoutClosingLogger", DeleteWithoutClosingLoggerAsync),
            ("LoadOnlyLogFiles", LoadOnlyLogFilesAsync),
            ("ConfirmDeletion", ConfirmDeletionAsync),
            ("CancelDeletion", CancelDeletionAsync),
            ("ConfirmLockedDeletion", ConfirmLockedDeletionAsync),
            ("ReadLockedLog", ReadLockedLogAsync),
            ("LargeLogRendering", LargeLogRenderingAsync),
        };
        var failures = 0;
        foreach (var test in tests)
        {
            TestLogUtils.DirectoryPath = Path.Combine(outputDirectory, test.Name);
            Directory.CreateDirectory(TestLogUtils.DirectoryPath);
            TestLogUtils.CloseCalls = 0;
            try { await test.Run(); Console.WriteLine($"PASS: {test.Name}"); }
            catch (Exception ex) { failures++; Console.WriteLine($"FAIL: {test.Name}: {ex.GetBaseException().Message}"); }
        }
        Console.WriteLine($"Результат: {tests.Length - failures}/{tests.Length}. Файлы проверок: {outputDirectory}");
        return failures == 0 ? 0 : 1;
    }

    // Проверяем чтение журнала, который в этот момент открыт на запись
    private static async Task ReadActiveLogAsync()
    {
        var filePath = CreateLog("Log-current.txt", "Первая запись\n");
        await using var writer = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        var viewModel = CreateViewModel();
        await viewModel.OnNavigatedToAsync(null);
        Assert(viewModel.LogFiles.Count == 1 && viewModel.LogFiles[0].Content == "Первая запись\n", "Не прочитано содержимое активного журнала");
        Assert(viewModel.Errors.Count == 0, "Чтение активного журнала зарегистрировало ошибку");
        await writer.WriteAsync(Encoding.UTF8.GetBytes("Вторая запись\n"));
        await writer.FlushAsync();
        await viewModel.OnNavigatedToAsync(null);
        Assert(viewModel.LogFiles.Count == 1 && viewModel.LogFiles[0].Content.Contains("Вторая запись"), "Повторный вход не обновил журнал");
    }

    // Проверяем точное имя выбранного файла вместо совпадения суффикса
    private static async Task DeleteSelectedFileAsync()
    {
        var otherPath = CreateLog("Log-Log-target.txt", "Другой журнал");
        var selectedPath = CreateLog("Log-target.txt", "Выбранный журнал");
        var viewModel = CreateViewModel();
        var selected = AddLog(viewModel, selectedPath);
        await DeleteCoreAsync(viewModel, selected);
        Assert(!File.Exists(selectedPath), "Выбранный файл не удалён");
        Assert(File.Exists(otherPath), "Удалён другой файл с совпадающим суффиксом");
        Assert(viewModel.LogFiles.Count == 0, "Удалённый файл остался в списке");
    }

    // Проверяем обработку реальной блокировки удаления другим процессом
    private static async Task DeleteLockedFileAsync()
    {
        var filePath = CreateLog("Log-locked.txt", "Заблокированный журнал");
        await using var fileLock = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        var viewModel = CreateViewModel();
        var selected = AddLog(viewModel, filePath);
        await DeleteCoreAsync(viewModel, selected);
        Assert(File.Exists(filePath), "Заблокированный файл неожиданно удалён");
        Assert(viewModel.LogFiles.Contains(selected), "Неудалённый файл исчез из списка");
        Assert(viewModel.Errors.Count == 1 && viewModel.Errors[0] is IOException, "Ошибка удаления не зарегистрирована");
    }

    // Удаление архивного файла не должно записывать фиктивное завершение приложения
    private static async Task DeleteWithoutClosingLoggerAsync()
    {
        var filePath = CreateLog("Log-archive.txt", "Архивный журнал");
        var viewModel = CreateViewModel();
        await DeleteCoreAsync(viewModel, AddLog(viewModel, filePath));
        Assert(!File.Exists(filePath), "Архивный журнал не удалён");
        Assert(TestLogUtils.CloseCalls == 0, "Удаление файла вызвало завершение журнала приложения");
    }

    // Проверяем фильтрацию файлов и очистку списка при повторном входе
    private static async Task LoadOnlyLogFilesAsync()
    {
        CreateLog("Log-test.txt", "Содержимое журнала");
        CreateLog("settings.txt", "Настройки");
        CreateLog("Log-test.db", "База данных");
        var viewModel = CreateViewModel();
        await viewModel.OnNavigatedToAsync(null);
        await viewModel.OnNavigatedToAsync(null);
        Assert(viewModel.LogFiles.Count == 1 && viewModel.LogFiles[0].FileName == "Log-test.txt", "Неверный список файлов журнала");
        Assert(viewModel.LogFiles[0].Content == "Содержимое журнала" && viewModel.LogFiles[0].FileSize != "0 B", "Не загружены данные файла");
        Assert(viewModel.LogDirectory == TestLogUtils.DirectoryPath, "Не показан каталог журналов");
    }

    // Проверяем выполнение команды, подтверждение и обновление списка
    private static async Task ConfirmDeletionAsync()
    {
        var filePath = CreateLog("Log-confirm.txt", "Удаляемый журнал");
        var viewModel = CreateViewModel();
        var selected = AddLog(viewModel, filePath);
        var deletion = viewModel.DeleteLogFileCommand.ExecuteAsync(selected);
        Assert(File.Exists(filePath), "Файл удалён до подтверждения");
        var dialog = ContentDialog.LastShown ?? throw new InvalidOperationException("Не показан диалог подтверждения");
        Assert(!deletion.IsCompleted && viewModel.DeleteLogFileCommand.IsRunning, "Команда не ожидает решения пользователя");
        var changedWhileOpen = false;
        viewModel.LogFiles.CollectionChanged += (_, _) => changedWhileOpen |= dialog.IsOpen;
        await dialog.ConfirmAsync();
        await deletion.WaitAsync(TimeSpan.FromSeconds(10));
        Assert(!File.Exists(filePath) && viewModel.LogFiles.Count == 0, "Подтверждённое удаление не завершено");
        Assert(!changedWhileOpen, "Список журналов изменён до закрытия модального диалога");
        Assert(!dialog.IsOpen && !viewModel.DeleteLogFileCommand.IsRunning, "Диалог или команда удаления остались активными");
    }

    // Проверяем отмену удаления раскрытого элемента без изменения его данных
    private static async Task CancelDeletionAsync()
    {
        var filePath = CreateLog("Log-cancel.txt", "Сохранённый журнал");
        var viewModel = CreateViewModel();
        var selected = AddLog(viewModel, filePath);
        var changed = false;
        viewModel.LogFiles.CollectionChanged += (_, _) => changed = true;
        var deletion = viewModel.DeleteLogFileCommand.ExecuteAsync(selected);
        var dialog = ContentDialog.LastShown ?? throw new InvalidOperationException("Не показан диалог подтверждения");
        Assert(!deletion.IsCompleted && dialog.IsOpen, "Диалог не ожидает решения пользователя");
        dialog.Cancel();
        await deletion.WaitAsync(TimeSpan.FromSeconds(10));
        Assert(File.Exists(filePath) && File.ReadAllText(filePath) == "Сохранённый журнал", "Отмена изменила файл");
        Assert(!changed && viewModel.LogFiles.Single() == selected, "Отмена изменила список журналов");
        Assert(!dialog.IsOpen && !viewModel.DeleteLogFileCommand.IsRunning, "После отмены диалог или команда остались активными");
        Assert(viewModel.Errors.Count == 0, "Отмена зарегистрировала ошибку");
    }

    // Проверяем завершение диалога при отказе файловой системы удалить журнал
    private static async Task ConfirmLockedDeletionAsync()
    {
        var filePath = CreateLog("Log-confirm-locked.txt", "Заблокированный журнал");
        await using var fileLock = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        var viewModel = CreateViewModel();
        var deletion = viewModel.DeleteLogFileCommand.ExecuteAsync(AddLog(viewModel, filePath));
        var dialog = ContentDialog.LastShown ?? throw new InvalidOperationException("Не показан диалог подтверждения");
        var changedWhileOpen = false;
        viewModel.LogFiles.CollectionChanged += (_, _) => changedWhileOpen |= dialog.IsOpen;
        await dialog.ConfirmAsync();
        await deletion.WaitAsync(TimeSpan.FromSeconds(10));
        Assert(File.Exists(filePath) && viewModel.LogFiles.Count == 1, "Заблокированный журнал исчез из списка после подтверждения");
        Assert(viewModel.Errors.Count == 1 && viewModel.Errors[0] is IOException, "Ошибка удаления через диалог не зарегистрирована");
        Assert(viewModel.LogFiles[0].Content == "Заблокированный журнал", "Список не обновлён после отказа удаления");
        Assert(!changedWhileOpen, "Список журналов обновлён до закрытия диалога при ошибке удаления");
    }

    // Проверяем сообщение об ошибке чтения и продолжение загрузки остальных файлов
    private static async Task ReadLockedLogAsync()
    {
        var filePath = CreateLog("Log-unreadable.txt", "Недоступный журнал");
        CreateLog("Log-readable.txt", "Доступный журнал");
        await using var fileLock = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var viewModel = CreateViewModel();
        await viewModel.OnNavigatedToAsync(null);
        Assert(viewModel.LogFiles.Count == 2, "Недоступный журнал прервал загрузку списка");
        Assert(viewModel.LogFiles.Single(x => x.FileName == "Log-unreadable.txt").Content == TgResourceExtensions.GetErrorOccurredWhileLoadingLogs(),
            "Не показана ошибка чтения журнала");
        Assert(viewModel.LogFiles.Single(x => x.FileName == "Log-readable.txt").Content == "Доступный журнал", "Не прочитан доступный журнал");
        Assert(viewModel.Errors.Count == 1 && viewModel.Errors[0] is IOException, "Ошибка чтения не зарегистрирована");
    }

    // Проверяем данные большого журнала и ограниченный виртуализируемый контейнер в исходном XAML
    private static async Task LargeLogRenderingAsync()
    {
        var content = string.Concat(Enumerable.Repeat("[INFO] Log entry for the rendering regression test\r\n", 65_536));
        var filePath = CreateLog("Log-large.txt", content);
        var viewModel = CreateViewModel();
        await viewModel.OnNavigatedToAsync(null);
        var logFile = viewModel.LogFiles.Single();
        Assert(logFile.Content == content && new FileInfo(filePath).Length > 3_000_000, "Не загружен полный журнал размером более 3 МБ");
        var linesProperty = typeof(TgLogFile).GetProperty("ContentLines")
            ?? throw new InvalidOperationException("Большой журнал отображается целиком, отсутствует представление по строкам");
        var lines = (IEnumerable<string>)(linesProperty.GetValue(logFile) ?? throw new InvalidOperationException("Отсутствуют строки журнала"));
        Assert(string.Join('\n', lines) == content.Replace("\r\n", "\n"), "Отображение по строкам потеряло часть журнала");
        var sourcePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../../Presentation/OpenTgResearcherDesktop/Views/TgLogsPage.xaml"));
        var xaml = System.Xml.Linq.XDocument.Load(sourcePath);
        var viewer = xaml.Descendants().SingleOrDefault(x => x.Name.LocalName == "ListView" &&
            (string?)x.Attribute("ItemsSource") == "{x:Bind ContentLines, Mode=OneTime}");
        Assert(viewer is not null && double.TryParse((string?)viewer.Attribute("Height"), out var height) && height > 0 && height <= 600,
            "Отсутствует ограниченный по высоте список строк журнала с виртуализацией");
        Assert(!xaml.Descendants().Any(x => x.Name.LocalName == "TextBlock" && (string?)x.Attribute("Text") == "{x:Bind Content, Mode=OneTime}"),
            "Полный журнал по-прежнему передаётся в один TextBlock");
        Assert(xaml.Descendants().Any(x => (string?)x.Attribute("Tag") == "{x:Bind Content}"), "Копирование больше не использует полный журнал");
    }

    // Создаём исходную модель без служб, не участвующих в операциях с журналами
    private static TgLogsViewModel CreateViewModel() => new(null!, null!, null!, null!);

    // Создаём файл в отдельном каталоге текущей проверки
    private static string CreateLog(string fileName, string content)
    {
        var filePath = Path.Combine(TestLogUtils.DirectoryPath, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    // Добавляем выбранный файл в коллекцию исходной модели
    private static TgLogFile AddLog(TgLogsViewModel viewModel, string filePath)
    {
        var logFile = new TgLogFile(viewModel, Path.GetFileName(filePath), "1 B", "Журнал");
        viewModel.LogFiles.Add(logFile);
        return logFile;
    }

    // Вызываем исходную операцию удаления без зависимости от окна подтверждения
    private static Task DeleteCoreAsync(TgLogsViewModel viewModel, TgLogFile logFile)
    {
        var method = typeof(TgLogsViewModel).GetMethod("DeleteLogFileCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Не найден метод удаления журнала");
        var arguments = method.GetParameters().Length == 1 ? new object[] { logFile } : [logFile, CancellationToken.None];
        return (Task)(method.Invoke(viewModel, arguments) ?? throw new InvalidOperationException("Операция удаления не возвращает задачу"));
    }

    // Останавливаем проверку при отсутствии ожидаемого наблюдаемого результата
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
