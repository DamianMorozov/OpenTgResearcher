namespace OpenTgResearcherDesktop.ChatSelfTests;

// Проверяем дозагрузку реальных страниц SQLite без кнопки и дублирования строк
internal static partial class Program
{
    private static IEnumerable<(string Name, Func<Task> Run)> GetRegistryPagingTests() =>
    [
        ("RegistryScrollPagination", RegistryScrollPaginationAsync),
        ("RegistryScrollPaginationConcurrent", RegistryScrollPaginationConcurrentAsync),
        ("RegistryPaginationDuringSizeUpdate", RegistryPaginationDuringSizeUpdateAsync),
        ("RegistryPaginationPreservesRows", RegistryPaginationPreservesRowsAsync),
        ("RegistrySizesAfterUiContextClosed", RegistrySizesAfterUiContextClosedAsync),
        ("RegistryScrollPaginationMarkup", RegistryScrollPaginationMarkupAsync),
    ];

    // Завершение фоновой очереди не должно обращаться к уже остановленному контексту окна
    private static async Task RegistrySizesAfterUiContextClosedAsync()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"registry-context-{Guid.NewGuid():N}");
        System.IO.Directory.CreateDirectory(directory);
        await File.WriteAllBytesAsync(Path.Combine(directory, "size.bin"), new byte[33]);
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var context = new RegistryClosingContext();
        var previousContext = SynchronizationContext.Current;
        Task pending;
        OpenTgResearcherDesktop.Models.TgChatRegistryRow[] rows = [new(new(), directory, 0, 0, default)];
        try
        {
            SynchronizationContext.SetSynchronizationContext(context);
            pending = (Task)typeof(TgChatsViewModel).GetMethod("UpdateDirectorySizesAsync", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(vm, [release.Task, rows, CancellationToken.None])!;
        }
        finally { SynchronizationContext.SetSynchronizationContext(previousContext); }
        release.SetResult();
        try
        {
            await Task.WhenAny(pending, context.Posted.Task).WaitAsync(TimeSpan.FromSeconds(5));
            await pending.WaitAsync(TimeSpan.FromSeconds(5));
            Assert(!context.Posted.Task.IsCompleted, "Фоновая очередь обращается к закрытому контексту окна");
            Assert(rows.Single().DirectorySizeBytes == 33, "Фоновая очередь не выполнила фактический подсчёт размера");
        }
        finally { System.IO.Directory.Delete(directory, recursive: true); }
    }

    // Фиксируем попытку отправки в закрытый контекст без падения всего процесса селфтестов
    private sealed class RegistryClosingContext : SynchronizationContext
    {
        public TaskCompletionSource Posted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override void Post(SendOrPostCallback callback, object? state)
        {
            Posted.TrySetResult();
            ThreadPool.QueueUserWorkItem(_ => callback(state));
        }
    }

    // Порция дополняет привязанную коллекцию, чтобы DataGrid не сбрасывал позицию прокрутки
    private static async Task RegistryPaginationPreservesRowsAsync()
    {
        using var sources = new TgEfSourceRepository();
        for (var id = 200; id < 204; id++)
            await sources.SaveAsync(new TgEfSourceDto { Id = id, Uid = Guid.NewGuid(), Title = $"Next {id}", UserName = $"chat_{id}" });
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!);
        await vm.OnNavigatedToAsync(null);
        var rows = vm.RegistryRows;
        var appended = 0;
        rows.CollectionChanged += (_, change) =>
        {
            Assert(change.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add, "Дозагрузка сбрасывает содержимое таблицы");
            appended += change.NewItems?.Count ?? 0;
        };
        await vm.LoadNextRegistryPageAsync();
        Assert(ReferenceEquals(rows, vm.RegistryRows) && rows.Count == 4 && appended == 2,
            "Дозагрузка заменяет коллекцию таблицы вместо добавления строк и сбрасывает прокрутку");
        await WaitRegistrySizesAsync(vm);
    }

    // Удерживаем реальное обновление размера первой строки и проверяем независимость загрузки страниц
    private static async Task RegistryPaginationDuringSizeUpdateAsync()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"registry-background-{Guid.NewGuid():N}");
        System.IO.Directory.CreateDirectory(directory);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task refresh = Task.CompletedTask;
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(directory, "size.bin"), new byte[4096]);
            using var sources = new TgEfSourceRepository();
            _source.Directory = directory;
            await sources.SaveAsync(_source);
            for (var id = 200; id < 204; id++)
                await sources.SaveAsync(new TgEfSourceDto { Id = id, Uid = Guid.NewGuid(), Title = $"Next {id}", UserName = $"chat_{id}", Directory = directory });
            var vm = new TgChatsViewModel(_state, _settings, _navigation, null!);
            OpenTgResearcherDesktop.Models.TgChatRegistryRow? heldRow = null;
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName != nameof(vm.RegistryRows) || heldRow is not null) return;
                heldRow = vm.RegistryRows.First(x => x.Source.Id == _source.Id);
                heldRow.PropertyChanged += (_, change) =>
                {
                    if (change.PropertyName != nameof(heldRow.DirectorySizeBytes)) return;
                    started.TrySetResult();
                    release.Task.GetAwaiter().GetResult();
                };
            };
            refresh = vm.OnNavigatedToAsync(null);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            try { await refresh.WaitAsync(TimeSpan.FromSeconds(2)); }
            catch (TimeoutException) { throw new InvalidOperationException("Подсчёт размера первой порции удерживает команду загрузки чатов"); }
            Assert(!vm.IsLoading && !vm.RefreshRegistryCommand.IsRunning && vm.RegistryRows.Count == 2,
                "Загрузка первой порции остаётся занятой подсчётом размера");
            await vm.LoadNextRegistryPageAsync().WaitAsync(TimeSpan.FromSeconds(2));
            await vm.LoadNextRegistryPageAsync().WaitAsync(TimeSpan.FromSeconds(2));
            Assert(vm.RegistryRows.Count == 5 && !vm.HasMoreItems, "Подсчёт размера блокирует следующую порцию");
            var obsoleteRows = vm.RegistryRows.Skip(1).ToArray();
            vm.FilterText = "Next 203";
            await vm.RefreshRegistryCommand.ExecuteAsync(null).WaitAsync(TimeSpan.FromSeconds(2));
            Assert(vm.RegistryRows.Single().Source.Id == 203, "Поиск заблокирован предыдущим подсчётом размеров");
            release.TrySetResult();
            await WaitRegistrySizesAsync(vm);
            Assert(heldRow?.DirectorySizeBytes == 4096 && vm.RegistryRows.Single().DirectorySizeBytes == 4096 && vm.Errors.Count == 0,
                "Фоновый подсчёт не завершился фактическим размером");
            Assert(obsoleteRows.All(x => x.DirectorySizeBytes is null), "После поиска продолжается подсчёт удалённых из реестра строк");
        }
        finally
        {
            release.TrySetResult();
            await refresh;
            System.IO.Directory.Delete(directory, recursive: true);
        }
    }

    // Ожидаем отдельное завершение подсчёта без привязки к команде загрузки строк
    private static Task WaitRegistrySizesAsync(TgChatsViewModel vm) => vm.DirectorySizesTask.WaitAsync(TimeSpan.FromSeconds(10));

    // Подготавливаем больше двух страниц только в отдельной синтетической БД окна
    private static async Task<int> SeedRegistryPagesAsync(string path)
    {
        try
        {
            await using var context = new TgEfDesktopContext();
            context.Database.SetConnectionString($"Data Source={path};Mode=ReadWrite");
            var sizeDirectory = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!, "PagingSize");
            System.IO.Directory.CreateDirectory(sizeDirectory);
            for (var index = 0; index < 6000; index++) await File.WriteAllBytesAsync(Path.Combine(sizeDirectory, $"{index:00000}.bin"), []);
            for (var index = 0; index < 942; index++)
                context.Sources.Add(new() { Uid = Guid.NewGuid(), Id = 1000 + index, Title = $"ZZ Page {index:000}", UserName = $"page_{index}",
                    Directory = index < 199 ? sizeDirectory : string.Empty });
            await context.SaveChangesAsync();
            return 0;
        }
        catch (Exception ex) { TgLogUtils.WriteException(ex); return 1; }
    }

    // Следующая порция дополняет список, сохраняет выбор и завершает пагинацию
    private static async Task RegistryScrollPaginationAsync()
    {
        for (var id = 200; id < 205; id++)
        {
            using var sources = new TgEfSourceRepository();
            await sources.SaveAsync(new TgEfSourceDto { Id = id, Uid = Guid.NewGuid(), Title = $"Next {id}", UserName = $"chat_{id}" });
        }
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!);
        await vm.OnNavigatedToAsync(null);
        var selected = vm.SelectedRow!.Source.Id;
        Assert(vm.RegistryRows.Count == 2 && vm.HasMoreItems, "Первая порция не ограничена размером страницы");
        await LoadNextRegistryPageAsync(vm);
        Assert(vm.RegistryRows.Count == 4 && vm.SelectedRow?.Source.Id == selected, "Порция заменила список или сбросила выбор");
        await LoadNextRegistryPageAsync(vm);
        await LoadNextRegistryPageAsync(vm);
        Assert(vm.RegistryRows.Count == 6 && !vm.HasMoreItems && vm.RegistryRows.Select(x => x.Source.Id).Distinct().Count() == 6,
            "Пагинация потеряла или повторила строки либо не завершилась");
        await LoadNextRegistryPageAsync(vm);
        Assert(vm.RegistryRows.Count == 6 && vm.Errors.Count == 0, "Конец списка вызвал лишнюю загрузку или ошибку");
        vm.FilterText = "Next 204";
        await vm.RefreshRegistryCommand.ExecuteAsync(null);
        Assert(vm.RegistryRows.Single().Source.Id == 204 && !vm.HasMoreItems, "Поиск не сбросил пагинацию");
    }

    // Несколько событий прокрутки во время запроса не должны загружать одну страницу дважды
    private static async Task RegistryScrollPaginationConcurrentAsync()
    {
        for (var id = 200; id < 204; id++)
        {
            using var sources = new TgEfSourceRepository();
            await sources.SaveAsync(new TgEfSourceDto { Id = id, Uid = Guid.NewGuid(), Title = $"Next {id}", UserName = $"chat_{id}" });
        }
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!);
        await vm.OnNavigatedToAsync(null);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        DelayedQueryInterceptor.HoldNext = 1; DelayedQueryInterceptor.Release = release.Task;
        var first = LoadNextRegistryPageAsync(vm);
        try
        {
            await LoadNextRegistryPageAsync(vm).WaitAsync(TimeSpan.FromSeconds(2));
            Assert(!first.IsCompleted && vm.RegistryRows.Count == 2, "Повторное событие не дождалось первой загрузки или изменило список");
        }
        finally { release.TrySetResult(); await first; DelayedQueryInterceptor.Release = null; DelayedQueryInterceptor.HoldNext = 0; }
        Assert(vm.RegistryRows.Count == 4 && vm.RegistryRows.Select(x => x.Source.Id).Distinct().Count() == 4,
            "Одна порция загружена дважды");
    }

    // Таблица запрашивает дозагрузку при подготовке отображаемых строк, ручной кнопки больше нет
    private static Task RegistryScrollPaginationMarkupAsync()
    {
        var page = ReadMarkup("TgChatsPage.xaml");
        var grid = page.Descendants().Single(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "ChatsRegistry");
        Assert((string?)grid.Attribute("LoadingRow") == "OnRegistryRowLoading", "Таблица не подключает дозагрузку по отображаемым строкам");
        Assert(!page.Descendants().Any(x => ((string?)x.Attribute("Command"))?.Contains("LazyLoadCommand") == true),
            "Осталась кнопка ручной дозагрузки");
        return Task.CompletedTask;
    }

    // Вызываем новый обработчик модели, чтобы исходная версия падала до исправления
    private static Task LoadNextRegistryPageAsync(TgChatsViewModel vm)
    {
        var method = vm.GetType().GetMethod("LoadNextRegistryPageAsync");
        Assert(method is not null, "Отсутствует автоматическая дозагрузка порции");
        return (Task)method!.Invoke(vm, null)!;
    }
}
