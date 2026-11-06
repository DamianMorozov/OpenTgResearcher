namespace OpenTgResearcherDesktop.ChatSelfTests;

internal static partial class Program
{
    // Проверяем доступность реестра во время сетевой операции и реальные строки локального хранилища
    private static IEnumerable<(string Name, Func<Task> Run)> GetRegistryTests() =>
    [
        ("RegistryRemainsVisibleWhileParsing", RegistryRemainsVisibleWhileParsingAsync),
        ("RegistryHasIdentityAndSelection", RegistryHasIdentityAndSelectionAsync),
        ("RegistryCountsStoredMessagesAndFiles", RegistryCountsStoredMessagesAndFilesAsync),
        ("RegistryReadsWhileParserUsesRepository", RegistryReadsWhileParserUsesRepositoryAsync),
        ("RegistryOpensChatWhileParserReads", RegistryOpensChatWhileParserReadsAsync),
        ("RegistryBrowsingPreservesParsingTarget", RegistryBrowsingPreservesParsingTargetAsync),
        ("RegistryFiltersAndControlsSelectedChat", RegistryFiltersAndControlsSelectedChatAsync),
        ("RegistryContentWhileParserReads", RegistryContentWhileParserReadsAsync),
        ("RegistryParticipantsWhileParserReads", RegistryParticipantsWhileParserReadsAsync),
        ("RegistryProfileWhileParserReads", RegistryProfileWhileParserReadsAsync),
        ("RegistryStatisticsWhileParserReads", RegistryStatisticsWhileParserReadsAsync),
        ("RegistryOverviewWhileParserReads", RegistryOverviewWhileParserReadsAsync),
        ("RegistryMissingMetadata", RegistryMissingMetadataAsync),
        ("RegistryOpensWithoutConfirmation", RegistryOpensWithoutConfirmationAsync),
        ("RegistryParsingControlsMarkup", RegistryParsingControlsMarkupAsync),
        ("RegistryStartsSelectedAndStopsWhileRunning", RegistryStartsSelectedAndStopsWhileRunningAsync),
        ("RegistryStopsOnlySelectedChat", RegistryStopsOnlySelectedChatAsync),
        ("RegistryParsingErrors", RegistryParsingErrorsAsync),
        ("RegistryStartsTwoChats", RegistryStartsTwoChatsAsync),
    ];

    // Одна команда реестра запускает второй чат до завершения первого и останавливает только выбранный
    private static async Task RegistryStartsTwoChatsAsync()
    {
        var other = new TgEfSourceDto { Id = 200, Uid = Guid.NewGuid(), Title = "Other", Directory = "second-media" };
        await App.BusinessLogicManager.StorageManager.SourceRepository.SaveAsync(other);
        _state.IsOnlineReady = true; _state.IsOnlineProcessing = true;
        App.BusinessLogicManager.ConnectClient.Ready = true;
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!);
        await vm.OnNavigatedToAsync(null);
        var entered = new Dictionary<long, TaskCompletionSource> { [100] = new(), [200] = new() };
        var settingsByChat = new Dictionary<long, TgDownloadSettingsViewModel>();
        using var firstStop = new CancellationTokenSource(); using var secondStop = new CancellationTokenSource();
        App.BusinessLogicManager.ConnectClient.Parse = async settings =>
        {
            var id = settings.SourceVm.Dto.Id; var operation = Guid.NewGuid();
            var stop = id == 100 ? firstStop : secondStop;
            settingsByChat[id] = settings;
            _state.NetworkActivity.Start(operation, TgNetworkOperationKind.Parsing);
            _state.NetworkActivity.SetChat(operation, id, settings.SourceVm.Dto.Title);
            _state.NetworkActivity.RegisterCancellation(operation, stop.Cancel);
            entered[id].TrySetResult();
            try { await Task.Delay(Timeout.Infinite, stop.Token); }
            finally { _state.NetworkActivity.Finish(operation, TgNetworkOperationState.Canceled); }
        };
        vm.SelectedRow = vm.RegistryRows.Single(x => x.Source.Id == 100);
        var first = vm.ToggleSelectedParsingCommand.ExecuteAsync(null);
        Task second = Task.CompletedTask;
        try
        {
            await entered[100].Task.WaitAsync(TimeSpan.FromSeconds(3));
            vm.SelectedRow = vm.RegistryRows.Single(x => x.Source.Id == 200);
            Assert(vm.ToggleSelectedParsingCommand.CanExecute(null), "Второй чат заблокирован выполняющимся первым");
            second = vm.ToggleSelectedParsingCommand.ExecuteAsync(null);
            await entered[200].Task.WaitAsync(TimeSpan.FromSeconds(3));
            Assert(!first.IsCompleted && !second.IsCompleted && settingsByChat[100].SourceVm.Dto.Id == 100
                && settingsByChat[200].SourceVm.Dto.Directory == "second-media", "Один запуск подменил настройки или завершил другой");
            await vm.ToggleSelectedParsingCommand.ExecuteAsync(null);
            await second.WaitAsync(TimeSpan.FromSeconds(3));
            Assert(!first.IsCompleted && !firstStop.IsCancellationRequested && secondStop.IsCancellationRequested,
                "Прерывание второго чата остановило первый");
        }
        finally { firstStop.Cancel(); secondStop.Cancel(); await Task.WhenAll(first, second); }
        Assert(vm.Errors.Count == 0, "Штатная отмена параллельного запуска отображается ошибкой");
    }

    // Кнопка открытия сразу переходит к выбранному чату без диалога
    private static async Task RegistryOpensWithoutConfirmationAsync()
    {
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!);
        await vm.OnNavigatedToAsync(null);
        var command = GetRegistryCommand<IRelayCommand>(vm, "OpenSelectedChatCommand");
        command.Execute(null);
        Assert(_navigation.Parameter is Guid uid && uid == _source.Uid && vm.ConfirmationCount == 0, "Открытие не выполнило прямой переход");
        vm.SelectedRow = null;
        Assert(!command.CanExecute(null), "Открытие доступно без выбранного чата");
    }

    // Обе команды привязаны к кнопкам, запуск расположен непосредственно перед паузой
    private static Task RegistryParsingControlsMarkupAsync()
    {
        var page = ReadMarkup("TgChatsPage.xaml");
        var open = page.Descendants().Single(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "ChatOpenChat");
        Assert(((string?)open.Attribute("Command"))?.Contains("OpenSelectedChatCommand") == true, "Кнопка открытия использует прежний диалог");
        var parsing = page.Descendants().SingleOrDefault(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "ChatRegistryParsing");
        Assert(parsing is not null, "Нет кнопки парсинга выбранного чата");
        var next = parsing!.ElementsAfterSelf().First();
        Assert(((string?)next.Attribute("Command"))?.Contains("PauseSelectedCommand") == true, "Парсинг расположен не слева от паузы");
        return Task.CompletedTask;
    }

    // Длительная команда остаётся доступной для прерывания и использует сохранённые настройки выбранной строки
    private static async Task RegistryStartsSelectedAndStopsWhileRunningAsync()
    {
        _state.IsOnlineReady = true;
        App.BusinessLogicManager.ConnectClient.Ready = true;
        _source.Directory = "saved-media";
        _source.IsSaveMessages = true;
        _source.FirstId = 37;
        await App.BusinessLogicManager.StorageManager.SourceRepository.SaveAsync(_source);
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!);
        await vm.OnNavigatedToAsync(null);
        var entered = new TaskCompletionSource<TgDownloadSettingsViewModel>();
        using var canceled = new CancellationTokenSource();
        var operationUid = Guid.NewGuid();
        App.BusinessLogicManager.ConnectClient.Parse = async settings =>
        {
            _state.NetworkActivity.Start(operationUid, TgNetworkOperationKind.Parsing);
            _state.NetworkActivity.SetChat(operationUid, settings.SourceVm.Dto.Id, settings.SourceVm.Dto.Title);
            _state.NetworkActivity.RegisterCancellation(operationUid, canceled.Cancel);
            entered.SetResult(settings);
            try { await Task.Delay(Timeout.Infinite, canceled.Token); }
            finally { _state.NetworkActivity.Finish(operationUid, TgNetworkOperationState.Canceled); }
        };
        var command = GetRegistryCommand<IAsyncRelayCommand>(vm, "ToggleSelectedParsingCommand");
        Assert(command.CanExecute(null), "Запуск выбранного чата недоступен");
        var parsing = command.ExecuteAsync(null);
        try
        {
            var settings = await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
            Assert(settings.SourceVm.Dto.Id == _source.Id && settings.SourceVm.Dto.Directory == "saved-media"
                && settings.SourceVm.Dto.IsSaveMessages && settings.SourceVm.Dto.FirstId == 37, "Запуск потерял сохранённые настройки");
            vm.SelectedRow = new(new() { Uid = Guid.NewGuid(), Id = 200 }, "other", 0, 0, default);
            Assert(command.CanExecute(null) && settings.SourceVm.Dto.Id == _source.Id, "Другой чат недоступен для параллельного запуска или изменил первый");
            vm.SelectedRow = vm.RegistryRows.Single();
            vm.RefreshActivity();
            Assert(command.CanExecute(null), "Выполняющаяся команда заблокировала собственное прерывание");
            vm.PauseSelectedCommand.Execute(null);
            await command.ExecuteAsync(null);
            await parsing.WaitAsync(TimeSpan.FromSeconds(3));
            Assert(canceled.IsCancellationRequested && vm.Errors.Count == 0 && !vm.RegistryRows.Single().Status.Contains("Running"),
                "Прерывание из паузы не завершило парсинг или показало исключение");
        }
        finally { canceled.Cancel(); await parsing; }
    }

    // Прерывание относится только к выбранному чату, повторное нажатие во время остановки недоступно
    private static async Task RegistryStopsOnlySelectedChatAsync()
    {
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!);
        await vm.OnNavigatedToAsync(null);
        var stopped = new List<long>();
        foreach (var id in new long[] { _source.Id, 200 })
        {
            var uid = Guid.NewGuid();
            _state.NetworkActivity.Start(uid, TgNetworkOperationKind.Parsing);
            _state.NetworkActivity.SetChat(uid, id, "chat");
            _state.NetworkActivity.RegisterCancellation(uid, () => stopped.Add(id));
        }
        vm.RefreshActivity();
        var command = GetRegistryCommand<IAsyncRelayCommand>(vm, "ToggleSelectedParsingCommand");
        Assert(command.CanExecute(null), "Прерывание выбранного активного чата недоступно");
        await command.ExecuteAsync(null);
        Assert(stopped.SequenceEqual(new[] { _source.Id }) && !command.CanExecute(null) && vm.ConfirmationCount == 1,
            "Прерывание затронуло другой чат, не подтвердилось или осталось доступным во время остановки");
    }

    // Недоступное подключение блокирует запуск, настоящая ошибка остаётся в журнале модели
    private static async Task RegistryParsingErrorsAsync()
    {
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!);
        await vm.OnNavigatedToAsync(null);
        var command = GetRegistryCommand<IAsyncRelayCommand>(vm, "ToggleSelectedParsingCommand");
        Assert(!command.CanExecute(null), "Запуск доступен без подключения");
        _state.IsOnlineReady = true;
        App.BusinessLogicManager.ConnectClient.Ready = true;
        App.BusinessLogicManager.ConnectClient.Parse = _ => throw new InvalidOperationException("Ошибка тестового парсинга");
        vm.RefreshActivity();
        await command.ExecuteAsync(null);
        Assert(vm.Errors.Single().Message == "Ошибка тестового парсинга" && command.CanExecute(null), "Ошибка скрыта или заблокировала повторный запуск");
    }

    // Получаем команду через отражение, чтобы проверить её отсутствие до реализации
    private static T GetRegistryCommand<T>(TgChatsViewModel vm, string name) where T : class =>
        vm.GetType().GetProperty(name)?.GetValue(vm) as T ?? throw new InvalidOperationException($"Отсутствует команда {name}");

    // Отсутствие истории парсинга и времени изменения не должно выглядеть завершённой загрузкой
    private static Task RegistryMissingMetadataAsync()
    {
        var row = new OpenTgResearcherDesktop.Models.TgChatRegistryRow(new(), "", 0, 0, default);
        row.Refresh(null);
        Assert(row.Progress == 0 && row.Updated == "—", "Нет данных, но показаны 100 % и дата первого года");
        return Task.CompletedTask;
    }

    // Автоматический расчёт статистики сохраняет доступ к данным при занятых репозиториях парсера
    private static async Task RegistryStatisticsWhileParserReadsAsync()
    {
        await SeedRegistryMemberAsync();
        App.BusinessLogicManager.ConnectClient.Ready = true;
        _state.IsOnlineReady = true;
        var operationUid = Guid.NewGuid();
        _state.NetworkActivity.Start(operationUid, TgNetworkOperationKind.Parsing);
        App.BusinessLogicManager.ConnectClient.ReadParticipants = () => throw new InvalidOperationException("Сетевой просмотр использует репозитории парсера");
        var vm = new TgChatStatisticsViewModel(_state, _settings, _navigation, null!, null!);
        await ReadWhileParserBusyAsync(async () =>
        {
            await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
            Assert(vm.TotalMessages == 1 && vm.IsCalculated && vm.Errors.Count == 0, "Статистика недоступна во время парсинга");
        });
    }

    // Прямое открытие обзора по идентификатору использует независимый запрос
    private static async Task RegistryOverviewWhileParserReadsAsync()
    {
        await ReadWhileParserBusyAsync(async () =>
        {
            await Download.OnNavigatedToAsync(new() { Parameter = _source.Uid });
            Assert(Download.DownloadDto.Id == _source.Id && Download.Errors.Count == 0, "Обзор недоступен во время парсинга");
        });
    }

    // Читаем настоящие сообщения, пока репозитории парсера заняты запросами
    private static async Task RegistryContentWhileParserReadsAsync()
    {
        await SeedRegistryMemberAsync();
        var vm = new TgChatContentViewModel(_state, _settings, _navigation, null!, null!);
        await ReadWhileParserBusyAsync(async () =>
        {
            await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
            Assert(vm.MessageDtos.SingleOrDefault()?.Id == 1 && vm.Errors.Count == 0, "Лента недоступна во время парсинга");
        });
    }

    // Участники выбранного чата доступны независимо от занятости контекста парсера
    private static async Task RegistryParticipantsWhileParserReadsAsync()
    {
        await SeedRegistryMemberAsync();
        var vm = new TgChatParticipantsViewModel(_state, _settings, _navigation, null!, null!);
        await ReadWhileParserBusyAsync(async () =>
        {
            await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
            Assert(vm.UserDtos.SingleOrDefault()?.Id == 10 && vm.Errors.Count == 0, "Каталог участников недоступен во время парсинга");
        });
    }

    // Локальная карточка профиля не должна обращаться к занятым репозиториям
    private static async Task RegistryProfileWhileParserReadsAsync()
    {
        await SeedRegistryMemberAsync();
        var vm = new TgChatMyMessagesViewModel(_state, _settings, _navigation, null!, null!);
        await ReadWhileParserBusyAsync(async () =>
        {
            await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
            Assert(vm.UserDtos.SingleOrDefault()?.Id == 10 && vm.Errors.Count == 0, "Профиль недоступен во время парсинга");
        });
    }

    // Создаём связанного с чатом автора и сообщение для проверки наблюдаемого результата
    private static async Task SeedRegistryMemberAsync()
    {
        using var users = new TgEfUserRepository();
        using var messages = new TgEfMessageRepository();
        using var links = new TgEfChatUserRepository();
        await users.SaveAsync(new TgEfUserDto { Uid = Guid.NewGuid(), Id = 10, FirstName = "Member" });
        await messages.SaveAsync(new TgEfMessageDto { Uid = Guid.NewGuid(), SourceId = _source.Id, UserId = 10, Id = 1,
            Message = "test", DtCreated = DateTime.UtcNow });
        await links.SaveAsync(new TgStorage.Domain.ChatUsers.TgEfChatUserDto { Uid = Guid.NewGuid(), ChatId = _source.Id, UserId = 10 });
    }

    // Удерживаем первые реальные запросы парсера до завершения проверки просмотра
    private static async Task ReadWhileParserBusyAsync(Func<Task> read)
    {
        _state.IsOnlineProcessing = true;
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        DelayedQueryInterceptor.HoldNext = 3;
        DelayedQueryInterceptor.Release = release.Task;
        var storage = App.BusinessLogicManager.StorageManager;
        var reads = Task.WhenAll(storage.SourceRepository.GetCountAsync(), storage.MessageRepository.GetCountAsync(), storage.UserRepository.GetCountAsync());
        try { await read(); }
        finally { release.TrySetResult(); await reads; DelayedQueryInterceptor.Release = null; DelayedQueryInterceptor.HoldNext = 0; }
    }

    // Работа Telegram не должна скрывать или блокировать таблицу и локальный поиск
    private static Task RegistryRemainsVisibleWhileParsingAsync()
    {
        var page = ReadMarkup("TgChatsPage.xaml");
        var table = page.Descendants().Single(x => x.Name.LocalName == "DataGrid");
        Assert(!table.AncestorsAndSelf().Attributes().Any(x => x.Name.LocalName is "Visibility" or "IsEnabled"
            && (x.Value.Contains("IsOnlineProcessing") || x.Value.Contains("IsStorageProcessing"))),
            "Глобальная операция скрывает или блокирует список чатов");
        return Task.CompletedTask;
    }

    // Открытие пульта не должно читать занятый парсером контекст настроек
    private static async Task RegistryOpensChatWhileParserReadsAsync()
    {
        _state.IsOnlineProcessing = true;
        DelayedQueryInterceptor.Enabled = true;
        var parserRead = App.BusinessLogicManager.StorageManager.SourceRepository.GetCountAsync();
        try
        {
            await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
            Assert(Chat.Dto.Id == _source.Id && Chat.Errors.Count == 0, "Пульт чата недоступен во время чтения настроек парсером");
        }
        finally { await parserRead; DelayedQueryInterceptor.Enabled = false; }
    }

    // Просмотр другого чата сохраняет источник и незавершённую задачу парсинга
    private static async Task RegistryBrowsingPreservesParsingTargetAsync()
    {
        using var sources = new TgEfSourceRepository();
        var other = new TgEfSourceDto { Uid = Guid.NewGuid(), Id = 200, Title = "Other chat" };
        await sources.SaveAsync(other);
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        var entered = new TaskCompletionSource<TgDownloadSettingsViewModel>();
        var release = new TaskCompletionSource();
        App.BusinessLogicManager.ConnectClient.Ready = true;
        App.BusinessLogicManager.ConnectClient.Parse = async settings => { entered.SetResult(settings); await release.Task; };
        var parsing = Chat.StartUpdateOnlineCommand.ExecuteAsync(null);
        var snapshot = await entered.Task;
        _state.IsOnlineProcessing = true;
        try
        {
            await Chat.OnNavigatedToAsync(new() { Parameter = other.Uid });
            Assert(Chat.Dto.Id == other.Id && snapshot.SourceVm.Dto.Id == _source.Id && !parsing.IsCompleted,
                "Просмотр другого чата изменил цель или завершил парсинг");
        }
        finally { release.TrySetResult(); await parsing; }
    }

    // Фильтр работающих чатов применяется до порции, пауза относится только к выбранной строке
    private static async Task RegistryFiltersAndControlsSelectedChatAsync()
    {
        using var sources = new TgEfSourceRepository();
        var other = new TgEfSourceDto { Uid = Guid.NewGuid(), Id = 200, Title = "Other chat" };
        await sources.SaveAsync(other);
        var uid = Guid.NewGuid();
        _state.NetworkActivity.Start(uid, TgNetworkOperationKind.Parsing);
        _state.NetworkActivity.SetChat(uid, other.Id, other.Title);
        _state.NetworkActivity.RegisterCancellation(uid, () => { });
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!) { RegistryFilter = 1 };
        await vm.OnNavigatedToAsync(null);
        Assert(vm.RegistryRows.Count == 1 && vm.Dtos.Single().Id == other.Id, "Фильтр потерял работающий чат");
        vm.PauseSelectedCommand.Execute(null);
        Assert(_state.NetworkActivity.Operations.Single().IsPaused, "Пауза не дошла до выбранного чата");
    }

    // Длительный запрос парсера не должен занимать контекст чтения списка чатов
    private static async Task RegistryReadsWhileParserUsesRepositoryAsync()
    {
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!) { IsFilterBySubscribe = false };
        DelayedQueryInterceptor.Enabled = true;
        var parserRead = App.BusinessLogicManager.StorageManager.SourceRepository.GetCountAsync();
        try
        {
            var rows = await vm.GetListDtosAsync();
            Assert(rows.Single().Id == _source.Id, "Просмотр не получил сохранённый чат во время запроса парсера");
        }
        finally { await parserRead; DelayedQueryInterceptor.Enabled = false; }
    }

    // Выбранный вариант объединяет идентификацию чата и сохраняет доступные действия выбранной строки
    private static Task RegistryHasIdentityAndSelectionAsync()
    {
        var page = ReadMarkup("TgChatsPage.xaml");
        Assert(page.Descendants().Any(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "ChatsRegistry"),
            "Нет рабочего реестра чатов");
        Assert(page.Descendants().Any(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "ChatSelectionActions"),
            "Нет панели действий выбранного чата");
        Assert(page.Descendants().Any(x => x.Name.LocalName == "DataTemplate" && x.Descendants().Any(n =>
            ((string?)n.Attribute("Text"))?.Contains("Identity") == true)), "Название и ID не собраны в читаемую ячейку чата");
        return Task.CompletedTask;
    }

    // Последний ID Telegram не равен числу сохранённых сообщений; файлы считаются по сообщениям с именем файла
    private static async Task RegistryCountsStoredMessagesAndFilesAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        context.Messages.AddRange(
            new TgEfMessageEntity { Uid = Guid.NewGuid(), SourceId = _source.Id, Id = 1, Message = "text" },
            new TgEfMessageEntity { Uid = Guid.NewGuid(), SourceId = _source.Id, Id = 9, Type = TgEnumMessageType.Document, Message = "sample.pdf" });
        await context.SaveChangesAsync();
        _state.IsOnlineProcessing = true;
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!) { IsFilterBySubscribe = false };
        var rows = await vm.GetListDtosAsync();
        Assert(rows.Single().Id == _source.Id, "Сетевой парсинг мешает читать сохранённый чат");
        await vm.OnNavigatedToAsync(null);
        var registry = vm.RegistryRows.Single();
        Assert(registry.MessagesCount == 2 && registry.FilesCount == 1, "Показаны выдуманные количества сообщений или файлов");
        Assert(_state.IsOnlineProcessing && vm.Errors.Count == 0, "Чтение реестра остановило парсинг или скрыло ошибку");
    }
}
