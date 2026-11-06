namespace OpenTgResearcherDesktop.ChatSelfTests;

internal static partial class Program
{
    internal static TgChatViewModel Chat { get; private set; } = null!;
    internal static TgChatDownloadViewModel Download { get; private set; } = null!;
    private static readonly ITgSettingsService _settings = new();
    private static readonly ILoadStateService _state = new();
    private static readonly INavigationService _navigation = new();
    private static TgEfSourceDto _source = null!;

    // Проверяем исходные модели с настоящими репозиториями и отдельной SQLite в памяти
    private static async Task<int> Main(string[] args)
    {
        if (args is ["--inspect-chat", var chatDatabase, var selectedChat])
            return await InspectChatAsync(chatDatabase, long.Parse(selectedChat));
        if (args is ["--live-chat", var sourceDatabase, var savedSession, var targetChat])
            return await LiveChatTests.RunAsync(sourceDatabase, savedSession, long.Parse(targetChat));
        if (args is ["--inspect-media", var database, var chatId, var mediaDirectory])
            return await MediaTests.InspectAsync(database, long.Parse(chatId), mediaDirectory);
        if (args is ["--live-media", var liveDatabase, var session, var liveChat, var liveDirectory])
            return await LiveMediaTests.RunAsync(liveDatabase, session, long.Parse(liveChat), liveDirectory);
        if (args is ["--seed", var path]) return await SeedDatabaseAsync(path);
        if (args is ["--seed", var target, "--source", var sourcePath, "--chat", var id, "--directory", var directory])
            return await SeedDatabaseAsync(target, sourcePath, long.Parse(id), directory);
        TgGlobalTools.SetAppType(TgEnumAppType.Test);
        typeof(TgLogUtils).GetField("_startupLog", BindingFlags.Static | BindingFlags.NonPublic)!
            .SetValue(null, Path.Combine(AppContext.BaseDirectory, "Chat.SelfTest.log"));
        var tests = new (string Name, Func<Task> Run)[]
        {
            ("DashboardTabs", DashboardTabsAsync),
            ("DashboardStartAvailability", DashboardStartAvailabilityAsync),
            ("DashboardDetailsAndDiscussion", DashboardDetailsAndDiscussionAsync),
            ("DashboardDetailsFailure", DashboardDetailsFailureAsync),
            ("DashboardContentNavigation", DashboardContentNavigationAsync),
            ("DashboardControlsSelectedChat", DashboardControlsSelectedChatAsync),
            ("DashboardOverview", DashboardOverviewAsync),
            ("DashboardRestoresActivity", DashboardRestoresActivityAsync),
            ("ChatFormsInitialized", ChatFormsInitializedAsync),
            ("ChatResetOptions", ChatResetOptionsAsync),
            ("EmptyChatOptionsResetOnOpen", EmptyChatOptionsResetOnOpenAsync),
            ("SelectedChatOptionsPreserved", SelectedChatOptionsPreservedAsync),
            ("StartSavesBeforeConnectionCheck", StartSavesBeforeConnectionCheckAsync),
            ("StartDoesNotParseAfterSaveFailure", StartDoesNotParseAfterSaveFailureAsync),
            ("DownloadNetworkNavigation", DownloadNetworkNavigationAsync),
            ("ChatActionsMarkup", ChatActionsMarkupAsync),
            ("SettingsLoadsSource", SettingsLoadsSourceAsync),
            ("DownloadLoadsBeforeProgress", DownloadLoadsBeforeProgressAsync),
            ("SettingsSharesUnsavedChanges", SettingsSharesUnsavedChangesAsync),
            ("DownloadSharesUnsavedChanges", DownloadSharesUnsavedChangesAsync),
            ("SwitchTabPreservesEdits", SwitchTabPreservesEditsAsync),
            ("ReloadKeepsDetailsPage", ReloadKeepsDetailsPageAsync),
            ("SaveWritesEditedSource", SaveWritesEditedSourceAsync),
            ("SaveWithoutChatDoesNotWrite", SaveWithoutChatDoesNotWriteAsync),
            ("EmptySelectionDoesNotCrash", EmptySelectionDoesNotCrashAsync),
            ("ParticipantsResetForOtherChat", ParticipantsResetForOtherChatAsync),
            ("ParticipantsFirstPageVisible", ParticipantsFirstPageVisibleAsync),
            ("ContentPaginationAndClear", ContentPaginationAndClearAsync),
            ("ContentEmptyStateAfterClear", ContentEmptyStateAfterClearAsync),
            ("StatisticsCountsDistinctUsers", StatisticsCountsDistinctUsersAsync),
            ("StatisticsUsesOneContextSequentially", StatisticsUsesOneContextSequentiallyAsync),
            ("StatisticsResetForOtherChat", StatisticsResetForOtherChatAsync),
            ("InfoResetForOtherChat", InfoResetForOtherChatAsync),
            ("MyMessagesEmptyWithoutUser", MyMessagesEmptyWithoutUserAsync),
            ("ChatTabsMarkup", ChatTabsMarkupAsync),
            ("ContentEmptyStateReachable", ContentEmptyStateReachableAsync),
            ("ChatsGridOwnsScrolling", ChatsGridOwnsScrollingAsync),
            ("ContentTemplateUsesPageContext", ContentTemplateUsesPageContextAsync),
            ("ChatsHeadersHaveBindings", ChatsHeadersHaveBindingsAsync),
            ("ChatsFilterHasOwnRow", ChatsFilterHasOwnRowAsync),
        }.Concat(MediaTests.GetTests()).Concat(GetRevisionTests()).ToArray();
        if (args is ["--test", var testName]) tests = [tests.Single(x => x.Name == testName)];
        var failures = 0;
        foreach (var test in tests)
        {
            await using var context = new TestContext(new DbContextOptionsBuilder().UseSqlite("Data Source=:memory:")
                .AddInterceptors(new DelayedQueryInterceptor()).Options);
            await context.Database.OpenConnectionAsync();
            await context.Database.EnsureCreatedAsync();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(context).As<ITgEfContext>().ExternallyOwned();
            builder.RegisterType<TgEfSourceRepository>().As<ITgEfSourceRepository>();
            builder.RegisterType<TgEfUserRepository>().As<ITgEfUserRepository>();
            builder.RegisterType<TgEfStoryRepository>().As<ITgEfStoryRepository>();
            builder.RegisterType<TgEfVersionRepository>().As<ITgEfVersionRepository>();
            using var container = builder.Build();
            TgGlobalTools.Container = container;
            using var sources = new TgEfSourceRepository();
            using var messages = new TgEfMessageRepository();
            using var users = new TgEfUserRepository();
            using var licenses = new TgEfLicenseRepository();
            using var chatUsers = new TgEfChatUserRepository();
            var storage = App.BusinessLogicManager.StorageManager;
            storage.SourceRepository = sources; storage.MessageRepository = messages; storage.UserRepository = users;
            storage.LicenseRepository = licenses; storage.ChatUserRepository = chatUsers;
            Chat = new(_state, _settings, _navigation, null!);
            Download = new(_state, _settings, _navigation, null!);
            _state.NetworkActivity = new();
            _state.IsOnlineReady = false;
            _state.IsOnlineProcessing = false;
            _state.IsDisplaySensitiveData = true;
            _source = new() { Uid = Guid.NewGuid(), Id = 100, UserName = "chat_one", Title = "First", FirstId = 15, Directory = "data" };
            App.BusinessLogicManager.ConnectClient.Ready = false;
            App.BusinessLogicManager.ConnectClient.UserId = 10;
            App.BusinessLogicManager.ConnectClient.Participants.Clear();
            App.BusinessLogicManager.ConnectClient.CheckConnection = null;
            App.BusinessLogicManager.ConnectClient.Parse = null;
            App.BusinessLogicManager.ConnectClient.ReadDetails = null;
            DelayedQueryInterceptor.Enabled = false;
            try
            {
                await sources.SaveAsync(_source);
                _source = await sources.GetDtoAsync(x => x.Id == 100);
                await test.Run();
                Console.WriteLine($"PASS: {test.Name}");
            }
            catch (Exception ex) { failures++; Console.WriteLine($"FAIL: {test.Name}: {ex.GetBaseException()}"); }
        }
        Console.WriteLine($"Результат: {tests.Length - failures}/{tests.Length}; SQLite в памяти, Telegram не используется");
        return failures == 0 ? 0 : 1;
    }

    // Создаём только новую изолированную БД для проверки настоящего окна
    private static async Task<int> SeedDatabaseAsync(string path, string? sourcePath = null, long chatId = 100, string directory = "")
    {
        try
        {
            if (File.Exists(path)) throw new InvalidOperationException("Тестовая БД уже существует");
            await using var context = new TgEfDesktopContext();
            context.Database.SetConnectionString($"Data Source={path}");
            await context.Database.MigrateAsync();
            if (sourcePath is null)
            {
                var mediaDirectory = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!, "Media");
                Directory.CreateDirectory(mediaDirectory);
                await File.WriteAllBytesAsync(Path.Combine(mediaDirectory, "0001 sample.mp4"), [1, 2, 3, 4]);
                await using var cover = typeof(Program).Assembly.GetManifestResourceStream("cover.jpg")!;
                await using var targetCover = File.Create(Path.Combine(mediaDirectory, "0001 sample.thumb.jpg"));
                await cover.CopyToAsync(targetCover);
                context.Sources.Add(new() { Uid = Guid.NewGuid(), Id = 100, UserName = "selftest_chat", Title = "SelfTest Chat",
                    IsSubscribe = true, Directory = mediaDirectory });
                context.Messages.Add(new() { Uid = Guid.NewGuid(), SourceId = 100, Id = 1, Message = "sample.mp4", Size = 4,
                    Type = TgEnumMessageType.Document, DtCreated = DateTime.UtcNow });
            }
            else
            {
                var connection = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
                    { DataSource = sourcePath, Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly };
                await using var original = new TestContext(new DbContextOptionsBuilder().UseSqlite(connection.ToString()).Options);
                var source = await original.Sources.AsNoTracking().SingleAsync(x => x.Id == chatId);
                var messages = await original.Messages.AsNoTracking().Where(x => x.SourceId == chatId).OrderByDescending(x => x.Id).Take(100).ToListAsync();
                var userIds = messages.Select(x => x.UserId).Distinct().ToList();
                var users = await original.Users.AsNoTracking().Where(x => userIds.Contains(x.Id)).ToListAsync();
                var links = await original.ChatUsers.AsNoTracking().Where(x => x.ChatId == chatId && userIds.Contains(x.UserId)).ToListAsync();
                source.IsSubscribe = true;
                if (!string.IsNullOrWhiteSpace(directory)) source.Directory = directory;
                context.Sources.Add(source);
                context.Users.AddRange(users);
                context.Messages.AddRange(messages);
                context.ChatUsers.AddRange(links);
                Console.WriteLine($"БД открыта только для чтения; чатов: {await original.Sources.CountAsync()}; выбран ID {chatId}; сообщений в выборке: {messages.Count}");
            }
            await context.SaveChangesAsync();
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    // Модели должны быть готовы до создания XAML-привязок
    private static Task ChatFormsInitializedAsync()
    {
        var settings = new TgChatSettingsViewModel(_state, _settings, _navigation, null!);
        Assert(Chat.Dto is not null && Chat.DiscussionDto is not null && settings.Dto is not null
            && settings.DiscussionDto is not null && Download.DownloadDto is not null, "DTO чата не инициализированы");
        Assert(new TgChatContentViewModel(_state, _settings, _navigation, null!, null!).Dto is not null
            && new TgChatParticipantsViewModel(_state, _settings, _navigation, null!, null!).Dto is not null
            && new TgChatStatisticsViewModel(_state, _settings, _navigation, null!, null!).Dto is not null
            && new TgChatMyMessagesViewModel(_state, _settings, _navigation, null!, null!).Dto is not null, "DTO вложенных страниц не инициализированы");
        return Task.CompletedTask;
    }

    // Вкладка настроек должна загружать выбранный источник при отдельной навигации
    private static async Task SettingsLoadsSourceAsync()
    {
        var vm = new TgChatSettingsViewModel(_state, _settings, _navigation, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(vm.Dto?.Id == _source.Id && vm.Dto.Title == _source.Title && vm.Errors.Count == 0, "Вкладка настроек не загрузила чат");
    }

    // Настройки загрузки должны читаться до вычисления прогресса локатором
    private static async Task DownloadLoadsBeforeProgressAsync()
    {
        await Download.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(Download.DownloadDto?.FirstId == 15 && Download.Errors.Count == 0, "DTO загрузки не прочитан до обращения локатора");
    }

    // Обе редактируемые вкладки должны работать с одним черновиком родительской страницы
    private static async Task SettingsSharesUnsavedChangesAsync()
    {
        var vm = new TgChatSettingsViewModel(_state, _settings, _navigation, null!);
        _source.Directory = "unsaved";
        await vm.OnNavigatedToAsync(new() { Parameter = _source });
        Assert(ReferenceEquals(vm.Dto, _source) && vm.Uid == _source.Uid, "Вкладка настроек потеряла общий черновик");
    }

    // Переключение вкладки загрузки не должно возвращать сохранённые значения поверх черновика
    private static async Task DownloadSharesUnsavedChangesAsync()
    {
        await Download.OnNavigatedToAsync(new() { Parameter = _source });
        Download.DownloadDto.FirstId = 77;
        Assert(ReferenceEquals(Download.DownloadDto, _source) && _source.FirstId == 77 && Download.Errors.Count == 0, "Вкладка загрузки не меняет черновик");
    }

    // Навигация страницы не должна перечитывать БД и терять правки
    private static async Task SwitchTabPreservesEditsAsync()
    {
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Chat.Dto.Directory = "unsaved";
        var page = new TgChatPage();
        page.Select(new() { Tag = nameof(TgChatDownloadPage) });
        if (Chat.LoadDataStorageCommand.ExecutionTask is { } task) await task;
        Assert(Chat.Dto.Directory == "unsaved" && ReferenceEquals(page.ChatDetailsFrame.Parameter, Chat.Dto), "Переключение вкладки стёрло правки");
    }

    // Перечитывание данных должно обновить модель открытой вкладки без создания другой страницы
    private static async Task ReloadKeepsDetailsPageAsync()
    {
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Chat.ChatDetailsFrame = new() { Model = Download };
        await Chat.LoadDataStorageCommand.ExecuteAsync(null);
        Assert(Chat.ChatDetailsFrame.Navigations == 0 && ReferenceEquals(Download.DownloadDto, Chat.Dto), "Обновление повторно создало вкладку");
    }

    // Команда сохранения должна записать редактируемый объект в настоящую БД
    private static async Task SaveWritesEditedSourceAsync()
    {
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Chat.Dto.Directory = "saved-new";
        await Chat.SaveChatSettingsCommand.ExecuteAsync(null);
        var saved = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == _source.Uid);
        Assert(saved.Directory == "saved-new" && Chat.Errors.Count == 0, "Изменения чата не сохранены");
    }

    // Сохранение без выбранного чата должно объяснять причину и не создавать запись
    private static async Task SaveWithoutChatDoesNotWriteAsync()
    {
        Chat.Dto = new();
        await Chat.SaveChatSettingsCommand.ExecuteAsync(null);
        Assert(await App.BusinessLogicManager.StorageManager.SourceRepository.GetCountAsync() == 1
            && Chat.Errors.Count == 1 && Chat.Errors[0].Message.Contains("Выберите чат"), "Пустая форма сохранилась или ошибка скрыта");
    }

    // Сброс выделения SelectorBar не должен разыменовывать null
    private static Task EmptySelectionDoesNotCrashAsync()
    {
        var page = new TgChatPage();
        page.Select(null);
        Assert(page.ChatDetailsFrame.Navigations == 0, "Пустое выделение выполнило переход");
        return Task.CompletedTask;
    }

    // Singleton-модель участников не должна переносить строки и пагинацию другого чата
    private static async Task ParticipantsResetForOtherChatAsync()
    {
        var vm = new TgChatParticipantsViewModel(_state, _settings, _navigation, null!, null!);
        vm.UserDtos.Add(new() { Id = 999 }); vm.PageSkip = 100; vm.HasMoreItems = false;
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(vm.UserDtos.Count == 0 && vm.PageSkip == 0 && vm.HasMoreItems, "Сохранились участники или пагинация другого чата");
    }

    // Первая непустая выборка должна убрать надпись об отсутствии участников
    private static async Task ParticipantsFirstPageVisibleAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        context.Users.Add(new() { Uid = Guid.NewGuid(), Id = 20, FirstName = "Member" });
        context.ChatUsers.Add(new() { Uid = Guid.NewGuid(), ChatId = _source.Id, UserId = 20 });
        context.Messages.Add(new() { Uid = Guid.NewGuid(), SourceId = _source.Id, UserId = 20, Id = 1, Message = "test" });
        await context.SaveChangesAsync();
        var vm = new TgChatParticipantsViewModel(_state, _settings, _navigation, null!, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        var users = await vm.GetListDtosAsync();
        Assert(users.Single().Id == 20 && !vm.IsEmptyData, "Непустая первая страница помечена как пустая");
    }

    // Постраничная загрузка должна показывать сообщения именно выбранного чата без пропусков и дублей
    private static async Task ContentPaginationAndClearAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        for (var i = 1; i <= 5; i++) context.Messages.Add(new() { Uid = Guid.NewGuid(), SourceId = _source.Id, Id = i, Message = $"m{i}" });
        await context.SaveChangesAsync();
        var vm = new TgChatContentViewModel(_state, _settings, _navigation, null!, null!) { PageSize = 2 };
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        await vm.LazyLoadMessagesCommand.ExecuteAsync(null);
        await vm.LazyLoadMessagesCommand.ExecuteAsync(null);
        Assert(vm.MessageDtos.Select(x => x.Id).SequenceEqual(new[] { 1, 2, 3, 4, 5 })
            && vm.PageSkip == 5 && !vm.HasMoreMessages && vm.Errors.Count == 0, "Пагинация пропустила сообщения или скрыла ошибку");
    }

    // Очистка формы должна обновлять состояние пустого списка и сохранять сообщения в БД
    private static async Task ContentEmptyStateAfterClearAsync()
    {
        var vm = new TgChatContentViewModel(_state, _settings, _navigation, null!, null!) { Dto = _source, IsEmptyData = false };
        vm.MessageDtos.Add(new() { Id = 1 });
        await vm.ClearViewCommand.ExecuteAsync(null);
        Assert(vm.MessageDtos.Count == 0 && vm.IsEmptyData && vm.Errors.Count == 0, "Очистка оставила непустое состояние списка");
    }

    // Пользователь из Telegram и БД должен учитываться в статистике один раз
    private static async Task StatisticsCountsDistinctUsersAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        context.Users.Add(new() { Uid = Guid.NewGuid(), Id = 10, FirstName = "User" });
        context.Messages.Add(new() { Uid = Guid.NewGuid(), SourceId = _source.Id, Id = 1, UserId = 10, Message = "test", DtCreated = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var client = App.BusinessLogicManager.ConnectClient;
        client.Ready = true;
        client.Participants.Add(new(new TL.User { id = 10, first_name = "User" }));
        var vm = new TgChatStatisticsViewModel(_state, _settings, _navigation, null!, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        await vm.CalcChatStatisticsCommand.ExecuteAsync(null);
        Assert(vm.ChatStatisticsDto.UsersCount == 1 && vm.ChatStatisticsDto.UserWithCountDtos.Single().Count == 1 && vm.Errors.Count == 0,
            "Один пользователь посчитан дважды или статистика завершилась ошибкой");
    }

    // Задержка настоящего запроса воспроизводит конфликт параллельных операций одного DbContext
    private static async Task StatisticsUsesOneContextSequentiallyAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        foreach (var id in new long[] { 10, 20 })
        {
            context.Users.Add(new() { Uid = Guid.NewGuid(), Id = id, FirstName = $"User{id}" });
            context.Messages.Add(new() { Uid = Guid.NewGuid(), SourceId = _source.Id, Id = (int)id, UserId = id,
                Message = "test", DtCreated = DateTime.UtcNow });
        }
        await context.SaveChangesAsync();
        App.BusinessLogicManager.ConnectClient.Ready = true;
        var vm = new TgChatStatisticsViewModel(_state, _settings, _navigation, null!, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        DelayedQueryInterceptor.Enabled = true;
        await vm.CalcChatStatisticsCommand.ExecuteAsync(null);
        Assert(vm.Errors.Count == 0 && vm.ChatStatisticsDto.UserWithCountDtos.Count == 2
            && vm.ChatStatisticsDto.UserWithCountDtos.All(x => x.Count == 1), "Запросы статистики конфликтуют или подсчёт не выполнен");
    }

    // При переходе к другому чату нельзя показывать рассчитанную статистику предыдущего
    private static async Task StatisticsResetForOtherChatAsync()
    {
        var vm = new TgChatStatisticsViewModel(_state, _settings, _navigation, null!, null!);
        vm.UserDtos.Add(new() { Id = 999 });
        vm.ChatStatisticsDto.UsersCount = 42;
        vm.ChatStatisticsDto.UserWithCountDtos.Add(new() { UserDto = new() { Id = 999 }, Count = 10 });
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(vm.UserDtos.Count == 0 && vm.ChatStatisticsDto.UsersCount == 0 && vm.ChatStatisticsDto.UserWithCountDtos.Count == 0,
            "Показывается статистика предыдущего чата");
    }

    // Онлайн-сведения предыдущего чата не должны показываться для другого UID
    private static async Task InfoResetForOtherChatAsync()
    {
        var vm = new TgChatInfoViewModel(_state, _settings, _navigation, null!);
        vm.ChatDetailsDto.Title = "Previous";
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(vm.ChatDetailsDto.Title != "Previous", "Показываются сведения другого чата");
    }

    // При отсутствии пользователя и лицензии нельзя показывать фиктивную строку пользователя
    private static async Task MyMessagesEmptyWithoutUserAsync()
    {
        App.BusinessLogicManager.ConnectClient.UserId = 0;
        var vm = new TgChatMyMessagesViewModel(_state, _settings, _navigation, null!, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(vm.UserDtos.Count == 0 && vm.IsEmptyData && vm.Errors.Count == 0, "Добавлен пустой пользователь вместо пустого списка");
    }

    // Проверяем наличие всех вкладок и отсутствие прямых UIElement-заголовков во вложенных таблицах
    private static Task ChatTabsMarkupAsync()
    {
        var names = typeof(Program).Assembly.GetManifestResourceNames().Where(x => x.EndsWith("Page.xaml")).ToArray();
        Assert(names.Length == 9, "Не включены все страницы чата и список чатов");
        foreach (var name in names)
        {
            using var stream = typeof(Program).Assembly.GetManifestResourceStream(name)!;
            var page = XDocument.Load(stream);
            Assert(!page.Descendants().Any(x => x.Name.LocalName.Contains("DataGrid") && x.Name.LocalName.EndsWith(".Header") && x.Elements().Any()),
                "UIElement в заголовке таблицы");
        }
        return Task.CompletedTask;
    }

    // Сообщение о пустом списке не должно находиться внутри скрытого контейнера
    private static Task ContentEmptyStateReachableAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgChatContentPage.xaml")!;
        var page = XDocument.Load(stream);
        var empty = page.Descendants().Single(x => x.Attributes().Any(a => a.Value == "FieldEmptyData"));
        Assert(!empty.Ancestors().Any(x => (string?)x.Attribute("Visibility") is string value &&
            value.Contains("IsEmptyData") && value.Contains("InverseBoolToVisibilityConverter")), "Подсказка пустого списка скрыта вместе с родителем");
        return Task.CompletedTask;
    }

    // Таблица должна измеряться в конечной области и использовать собственную прокрутку
    private static Task ChatsGridOwnsScrollingAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgChatsPage.xaml")!;
        var page = XDocument.Load(stream);
        var grid = page.Descendants().Single(x => x.Name.LocalName == "DataGrid");
        Assert(!grid.Ancestors().Any(x => x.Name.LocalName is "ScrollView" or "ScrollViewer"), "Таблица чатов вложена во внешнюю прокрутку");
        return Task.CompletedTask;
    }

    // DTO сообщения не содержит модели страницы, поэтому для таких привязок нужен явный источник
    private static Task ContentTemplateUsesPageContextAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgChatContentPage.xaml")!;
        var template = XDocument.Load(stream).Descendants().Single(x => x.Name.LocalName == "DataTemplate");
        Assert(!template.Descendants().Attributes().Any(a => a.Value.StartsWith("{Binding ViewModel.") && !a.Value.Contains("ElementName=")),
            "Привязки шаблона ищут ViewModel в DTO сообщения");
        Assert(!template.Descendants().Attributes().Any(a => a.Value.StartsWith("{Binding LoadStateService.")),
            "Привязки шаблона ищут службу в DTO сообщения");
        return Task.CompletedTask;
    }

    // Локализатор не обходит колонки DataGrid как элементы визуального дерева
    private static Task ChatsHeadersHaveBindingsAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgChatsPage.xaml")!;
        var columns = XDocument.Load(stream).Descendants().Where(x => x.Name.LocalName == "TgBindableDataGridTemplateColumn").ToArray();
        Assert(columns.Length == 9 && columns.All(x => ((string?)x.Attribute("Header"))?.StartsWith("{x:Bind ViewModel.") == true),
            "Заголовки колонок чатов не привязаны к строкам модели");
        return Task.CompletedTask;
    }

    // Поле фильтра не должно делить строку с четырьмя подписями переключателей
    private static Task ChatsFilterHasOwnRowAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgChatsPage.xaml")!;
        var page = XDocument.Load(stream);
        var filter = page.Descendants().Single(x => x.Name.LocalName == "TextBox");
        Assert((string?)filter.Attribute("Grid.Row") == "1" && (string?)filter.Attribute("Grid.Column") == "0",
            "Поле фильтра сжимается подписями переключателей и кнопками");
        return Task.CompletedTask;
    }

    // Каждый тест должен проверять наблюдаемый результат исходной модели
    private static void Assert(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}
