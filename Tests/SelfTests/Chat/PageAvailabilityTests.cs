namespace OpenTgResearcherDesktop.ChatSelfTests;

// Проверяем просмотр локальных разделов при активном парсинге и занятых репозиториях
internal static partial class Program
{
    private static IEnumerable<(string Name, Func<Task> Run)> GetPageAvailabilityTests() =>
    [
        ("PagesRemainVisibleWhileParsing", PagesRemainVisibleWhileParsingAsync),
        ("StorageTablesReadWhileParserBusy", StorageTablesReadWhileParserBusyAsync),
        ("FiltersReadWhileParserBusy", FiltersReadWhileParserBusyAsync),
        ("StoriesReadWhileParserBusy", StoriesReadWhileParserBusyAsync),
        ("UsersReadWhileParserBusy", UsersReadWhileParserBusyAsync),
    ];

    // Общая операция не должна скрывать содержимое и команды другого раздела
    private static Task PagesRemainVisibleWhileParsingAsync()
    {
        foreach (var name in new[] { "TgStoragePage", "TgStorageTablesPage", "TgStorageConfigurationPage", "TgStorageAdvancedPage",
            "TgClientConnectionPage", "TgFiltersPage", "TgStoriesPage", "TgUsersPage" })
        {
            var page = ReadMarkup($"{name}.xaml");
            Assert(!page.Descendants().Attributes().Any(a => a.Name.LocalName is "IsEnabled" or "Visibility"
                && (a.Value.Contains("IsOnlineProcessing") || a.Value.Contains("IsStorageProcessing"))),
                $"{name}: содержимое скрывается общей операцией другого раздела");
        }
        return Task.CompletedTask;
    }

    // Таблицы и число записей доступны до освобождения контекста фоновой операции
    private static async Task StorageTablesReadWhileParserBusyAsync()
    {
        var vm = new TgStorageTablesViewModel(_state, _settings, _navigation, null!);
        await OpenWhileParsingAsync(ct => App.BusinessLogicManager.StorageManager.SourceRepository.GetCountAsync(ct: ct),
            () => vm.OnNavigatedToAsync(null), () => CheckPageRows(vm.Errors,
                vm.StorageTableDtos.SingleOrDefault(x => x.Name == "Chats")?.RecordsCount == 1));
    }

    // Читаем реальные фильтры отдельным запросом, пока исходный репозиторий занят
    private static async Task FiltersReadWhileParserBusyAsync()
    {
        using var filters = new TgEfFilterRepository();
        await filters.SaveAsync(new TgStorage.Domain.Filters.TgEfFilterDto
        { Uid = Guid.NewGuid(), Name = "filter-for-reading", FilterType = Enum.GetValues<TgEnumFilterType>().First(x => (int)x != 0) });
        var vm = new TgFiltersViewModel(_state, _settings, _navigation, null!);
        await OpenWhileParsingAsync(ct => App.BusinessLogicManager.StorageManager.FilterRepository.GetCountAsync(ct: ct),
            () => vm.OnNavigatedToAsync(null), () => CheckPageRows(vm.Errors, vm.Dtos.Any(x => x.Name == "filter-for-reading")));
    }

    // Сторис должны оставаться доступными без ожидания окончания парсинга
    private static async Task StoriesReadWhileParserBusyAsync()
    {
        using var stories = new TgEfStoryRepository();
        await stories.SaveAsync(new TgStorage.Domain.Stories.TgEfStoryDto { Uid = Guid.NewGuid(), Id = 77, FromId = 100, FromName = "story-for-reading" });
        var vm = new TgStoriesViewModel(_state, _settings, _navigation, null!);
        await OpenWhileParsingAsync(ct => App.BusinessLogicManager.StorageManager.StoryRepository.GetCountAsync(ct: ct),
            () => vm.OnNavigatedToAsync(null), () => CheckPageRows(vm.Errors, vm.Dtos.Any(x => x.FromName == "story-for-reading")));
    }

    // Первая порция контактов и их счётчики читаются без использования занятого контекста
    private static async Task UsersReadWhileParserBusyAsync()
    {
        using var users = new TgEfUserRepository();
        await users.SaveAsync(new TgEfUserDto { Uid = Guid.NewGuid(), Id = 123, FirstName = "contact-for-reading", IsContact = true });
        var vm = new TgUsersViewModel(_state, _settings, _navigation, null!);
        await OpenWhileParsingAsync(ct => App.BusinessLogicManager.StorageManager.UserRepository.GetCountAsync(ct: ct),
            () => vm.OnNavigatedToAsync(null), () => CheckPageRows(vm.Errors, vm.Dtos.Any(x => x.Id == 123)));
    }

    // Удерживаем реальный SQL-запрос, проверяя результат открытия до его освобождения
    private static async Task OpenWhileParsingAsync(Func<CancellationToken, Task<int>> occupy, Func<Task> open, Func<bool> result)
    {
        _state.IsOnlineProcessing = true; _state.IsOnlineReady = true;
        var uid = Guid.NewGuid(); _state.NetworkActivity.Start(uid, TgNetworkOperationKind.Parsing);
        _state.NetworkActivity.SetChat(uid, _source.Id, _source.Title);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        DelayedQueryInterceptor.HoldNext = 1; DelayedQueryInterceptor.Release = release.Task;
        var pending = occupy(default);
        try
        {
            await open().WaitAsync(TimeSpan.FromSeconds(3));
            Assert(result(), "Раздел не загрузил данные до завершения парсинга или зарегистрировал ошибку EF");
            Assert(_state.NetworkActivity.Operations.Single().State == TgNetworkOperationState.Running,
                "Просмотр раздела завершил выполняющийся парсинг");
        }
        finally { release.TrySetResult(); await pending; DelayedQueryInterceptor.HoldNext = 0; DelayedQueryInterceptor.Release = null; }
    }

    // Сохраняем первичную причину отказа загрузки вместо одной проверки пустой коллекции
    private static bool CheckPageRows(IReadOnlyList<Exception> errors, bool hasRows)
    {
        Assert(errors.Count == 0, string.Join(Environment.NewLine, errors.Select(x => x.GetBaseException().Message)));
        return hasRows;
    }
}
