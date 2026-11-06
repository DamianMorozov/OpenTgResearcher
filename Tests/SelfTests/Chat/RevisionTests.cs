namespace OpenTgResearcherDesktop.ChatSelfTests;

internal static partial class Program
{
    // Проверяем новые сценарии на исходных моделях и настоящих репозиториях
    private static IEnumerable<(string Name, Func<Task> Run)> GetRevisionTests() =>
    [
        ("AutoUpdateToggleWritesDraft", AutoUpdateToggleWritesDraftAsync),
        ("AutoUpdatePersistsBothValues", AutoUpdatePersistsBothValuesAsync),
        ("DownloadPercentMatchesLabel", DownloadPercentMatchesLabelAsync),
        ("InfoLoadsAutomaticallyOnline", InfoLoadsAutomaticallyOnlineAsync),
        ("FirstSettingsPageWaitsForSource", FirstSettingsPageWaitsForSourceAsync),
        ("ParticipantsIncludeSilentMembers", ParticipantsIncludeSilentMembersAsync),
        ("ParticipantsClearAllowsReload", ParticipantsClearAllowsReloadAsync),
        ("ParticipantsSearchFiltersUsers", ParticipantsSearchFiltersUsersAsync),
        ("ParticipantsDossierBindings", ParticipantsDossierBindingsAsync),
        ("OnlineParticipantsPersistMembership", OnlineParticipantsPersistMembershipAsync),
        ("DisconnectedTransportDoesNotCancelWork", DisconnectedTransportDoesNotCancelWorkAsync),
        ("TelegramRequestRetriesTransportFailure", TelegramRequestRetriesTransportFailureAsync),
        ("TelegramRequestDoesNotRetryRpcError", TelegramRequestDoesNotRetryRpcErrorAsync),
        ("MissingClientIsNotFalseCancellation", MissingClientIsNotFalseCancellationAsync),
        ("TransportCancellationRetriesWithoutUserCancellation", TransportCancellationRetriesWithoutUserCancellationAsync),
        ("ConcurrentParsingLogsKeepAllEntries", ConcurrentParsingLogsKeepAllEntriesAsync),
        ("TextParsingDoesNotQueryAuthorWithZeroHash", TextParsingDoesNotQueryAuthorWithZeroHashAsync),
        ("PartialMessageBufferDoesNotBlock", PartialMessageBufferDoesNotBlockAsync),
        ("OnlineParticipantsReloadCountsAndOrder", OnlineParticipantsReloadCountsAndOrderAsync),
        ("ParticipantsActionsWrapCaptions", ParticipantsActionsWrapCaptionsAsync),
        ("DownloadShowsSavedPercentOnOpening", DownloadShowsSavedPercentOnOpeningAsync),
    ];

    // Переключатель обязан менять общий черновик, иначе команда сохранения получает старое значение
    private static Task AutoUpdateToggleWritesDraftAsync()
    {
        var toggle = ReadMarkup("TgChatDownloadPage.xaml").Descendants().Single(x => x.Name.LocalName == "ToggleSwitch"
            && ((string?)x.Attribute("IsOn"))?.Contains("IsAutoUpdate") == true);
        Assert(((string)toggle.Attribute("IsOn")!).Contains("Mode=TwoWay"), "Автообновление не записывается из переключателя в модель");
        return Task.CompletedTask;
    }

    // После включения и выключения значение должно пережить сохранение и повторное чтение SQLite
    private static async Task AutoUpdatePersistsBothValuesAsync()
    {
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        foreach (var value in new[] { true, false })
        {
            Chat.Dto.IsAutoUpdate = value;
            await Chat.SaveChatSettingsCommand.ExecuteAsync(null);
            var saved = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == _source.Uid);
            Assert(saved.IsAutoUpdate == value && Chat.Errors.Count == 0, "Значение автообновления не сохранилось");
        }
    }

    // Подпись процента должна относиться к проценту, а поле оставаться доступным для чтения и копирования
    private static Task DownloadPercentMatchesLabelAsync()
    {
        var page = ReadMarkup("TgChatDownloadPage.xaml");
        var field = page.Descendants().Single(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "ChatDownloadPercent");
        Assert(((string?)field.Attribute("Text"))?.Contains("ChatProgressMessage") == true,
            "Под подписью процента показывается счётчик сообщений");
        Assert((string?)field.Attribute("IsReadOnly") == "True" && (string?)field.Attribute("IsEnabled") != "False",
            "Поле процента заблокировано вместо режима только чтения");
        return Task.CompletedTask;
    }

    // Показываем процент сохранённой позиции уже при открытии вкладки загрузки
    private static async Task DownloadShowsSavedPercentOnOpeningAsync()
    {
        var dto = new TgEfSourceDto { Uid = _source.Uid, Id = _source.Id, FirstId = 25, Count = 100 };
        await Download.OnNavigatedToAsync(new() { Parameter = dto });
        Assert(Download.ChatProgressMessage == $"{25.0:00.00} %", "При открытии вкладки процент сохранённого прогресса пустой");
    }

    // Открытие сведений при активном подключении должно получить данные без дополнительного подтверждения
    private static async Task InfoLoadsAutomaticallyOnlineAsync()
    {
        App.BusinessLogicManager.ConnectClient.Ready = true;
        var vm = new TgChatInfoViewModel(_state, _settings, _navigation, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(vm.ChatDetailsDto.Title == "Online" && vm.Errors.Count == 0, "При подключении сведения чата остались пустыми");
        Assert(vm.OnlineLoads == 1, "Автоматический запрос сведений не сообщает о сетевой загрузке верхней панели");
    }

    // Первая вкладка не должна получить пустой черновик, пока родитель ещё читает SQLite
    private static async Task FirstSettingsPageWaitsForSourceAsync()
    {
        DelayedQueryInterceptor.Enabled = true;
        var loading = Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        var page = new TgChatPage();
        page.ChatDetailsSelectorBar.SelectedItem = page.ChatDetailsSelectorBar.Items[0];
        var method = typeof(TgChatPage).GetMethod("LoadDetailsAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert(method is not null, "Первая вкладка открывается без ожидания загрузки родительского чата");
        await (Task)method!.Invoke(page, null)!;
        await loading;
        Assert(page.ChatDetailsFrame.Parameter is TgEfSourceDto dto && dto.Id == _source.Id && ReferenceEquals(dto, Chat.Dto),
            "Первая вкладка получила пустой или отдельный черновик");
    }

    // Участник без сообщений должен присутствовать в списке участников
    private static async Task ParticipantsIncludeSilentMembersAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        context.Users.Add(new() { Uid = Guid.NewGuid(), Id = 20, FirstName = "Silent" });
        context.ChatUsers.Add(new() { Uid = Guid.NewGuid(), ChatId = _source.Id, UserId = 20 });
        await context.SaveChangesAsync();
        var vm = new TgChatParticipantsViewModel(_state, _settings, _navigation, null!, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(vm.UserDtos.Count == 1 && vm.UserDtos[0].Id == 20 && vm.UserDtos[0].MessagesCount == 0,
            "Участник без сообщений отфильтрован при открытии вкладки");
    }

    // Очистка должна вернуть пустое состояние и разрешить повторное чтение с первой страницы
    private static async Task ParticipantsClearAllowsReloadAsync()
    {
        var vm = new TgChatParticipantsViewModel(_state, _settings, _navigation, null!, null!) { PageSkip = 8, HasMoreItems = false };
        vm.UserDtos.Add(new() { Id = 20 });
        await vm.ClearParticipantsCommand.ExecuteAsync(null);
        Assert(vm.UserDtos.Count == 0 && vm.IsEmptyData && vm.PageSkip == 0 && vm.HasMoreItems,
            "Очистка оставила старую пагинацию или непустое состояние");
    }

    // Поиск должен применяться до пагинации и учитывать переключатели полей
    private static async Task ParticipantsSearchFiltersUsersAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        for (var id = 20; id < 23; id++)
        {
            context.Users.Add(new() { Uid = Guid.NewGuid(), Id = id, UserName = id == 22 ? "target" : "other" + id });
            context.ChatUsers.Add(new() { Uid = Guid.NewGuid(), ChatId = _source.Id, UserId = id });
            context.Messages.Add(new() { Uid = Guid.NewGuid(), SourceId = _source.Id, UserId = id, Id = id, Message = "test" });
        }
        await context.SaveChangesAsync();
        var vm = new TgChatParticipantsViewModel(_state, _settings, _navigation, null!, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        vm.FilterText = "target";
        vm.PageSkip = 0;
        var users = await vm.GetListDtosAsync();
        Assert(users.Count == 1 && users[0].Id == 22, "Поиск участников игнорируется или применяется после пагинации");
    }

    // Выбор строки каталога показывает общее локализованное досье пользователя
    private static Task ParticipantsDossierBindingsAsync()
    {
        var page = ReadMarkup("TgChatParticipantsPage.xaml");
        Assert(page.Descendants().Any(x => x.Name.LocalName == "ListView"
            && (string?)x.Attribute("SelectedItem") == "{x:Bind ViewModel.SelectedUser, Mode=TwoWay}"), "Каталог не передаёт выбранного участника");
        Assert(page.Descendants().Any(x => x.Name.LocalName == "ContentControl"
            && (string?)x.Attribute("ContentTemplate") == "{StaticResource ChatUserDossierTemplate}"), "Выбранному участнику не назначено досье");
        var dossier = ReadMarkup("TgChatResources.xaml");
        foreach (var field in new[] { "FieldId", "FieldDisplayName", "FieldUserName", "FieldPhoneNumber", "FieldMessagesCount" })
            Assert(dossier.Descendants().Attributes().Any(x => x.Name.LocalName == "Uids.Uid" && x.Value == field), "Нет локализованного поля досье: " + field);
        return Task.CompletedTask;
    }

    // Временная потеря транспорта не является запросом отмены парсинга
    private static async Task DisconnectedTransportDoesNotCancelWorkAsync()
    {
        using var files = new MediaFixture();
        var method = typeof(TgConnectClientBase).GetMethod("CheckShouldStop", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var client = files.Connection.Client!;
        var transportField = typeof(WTelegram.Client).GetField("_tcpClient", BindingFlags.Instance | BindingFlags.NonPublic)!;
        transportField.SetValue(client, new System.Net.Sockets.TcpClient());
        Assert(client.Disconnected, "Стенд не воспроизвёл отключение транспорта");
        Assert(!(bool)method.Invoke(files.Connection, [CancellationToken.None])!, "Разрыв транспорта ошибочно отменяет обработку");
        await Task.CompletedTask;
    }

    // Основные кнопки переносят подписи, все действия участников сохранены в меню
    private static Task ParticipantsActionsWrapCaptionsAsync()
    {
        var page = ReadMarkup("TgChatParticipantsPage.xaml");
        var buttons = page.Descendants().Where(x => x.Name.LocalName == "Button"
            && ((string?)x.Attribute("Command"))?.Contains("ParticipantsCommand") == true).ToArray();
        Assert(buttons.Length == 2 && buttons.All(x => x.Descendants().Any(t => t.Name.LocalName == "TextBlock"
            && (string?)t.Attribute("TextWrapping") == "Wrap")), "Основные кнопки участников обрезают подписи");
        foreach (var command in new[] { "GetParticipantsCommand", "GetParticipantsFromMessagesCommand", "ClearParticipantsCommand" })
            Assert(page.Descendants().Any(x => x.Name.LocalName == "MenuFlyoutItem"
                && ((string?)x.Attribute("Command"))?.Contains(command) == true), "В меню потеряно действие: " + command);
        return Task.CompletedTask;
    }

    // Читаем разметку, встроенную из исходного файла страницы
    private static XDocument ReadMarkup(string name)
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream(name)!;
        return XDocument.Load(stream);
    }

    // Ошибки параллельных задач должны полностью записываться без отказа самого журнала
    private static async Task ConcurrentParsingLogsKeepAllEntriesAsync()
    {
        var marker = Guid.NewGuid().ToString("N");
        var field = typeof(TgLogUtils).GetField("_startupLog", BindingFlags.Static | BindingFlags.NonPublic)!;
        await Task.WhenAll(Enumerable.Range(0, 200).Select(i => Task.Run(() => TgLogUtils.WriteLog($"{marker}:{i}"))));
        var lines = await File.ReadAllLinesAsync((string)field.GetValue(null)!);
        Assert(Enumerable.Range(0, 200).All(i => lines.Count(x => x.EndsWith($"{marker}:{i}")) == 1), "Журнал потерял сообщения параллельных задач");
    }

    // Сохранение текста и автора не требует отдельного запроса участника с заведомо пустым access hash
    private static async Task TextParsingDoesNotQueryAuthorWithZeroHashAsync()
    {
        using var files = new MediaFixture();
        files.Connection.DicChats[100] = new TL.Channel { id = 100, access_hash = 123 };
        files.Settings.SourceVm.Dto.IsSaveMessages = true;
        var message = new TL.Message { id = 42, peer_id = new TL.PeerChannel { channel_id = 100 }, from_id = new TL.PeerUser { user_id = 20 },
            flags = TL.Message.Flags.has_from_id, message = "Text", date = DateTime.UtcNow };
        await files.CallAsync("HandleMessageContentAsync", files.Settings, new TgParseHelper(), message, new TgChatCache(),
            new TgForumTopicSettings(), message, new TgMessageSettings { CurrentChatId = 100, CurrentMessageId = 42 }, CancellationToken.None);
        var buffer = (TgBusinessLogic.Helpers.TgBufferCacheHelper<TgEfMessageDto>)typeof(TgConnectClientBase)
            .GetField("_messageBuffer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(files.Connection)!;
        Assert(files.NetworkCalls == 0 && buffer.Count == 1 && buffer.FirstOrDefault(x => x.Id == 42) is { UserId: 20, Message: "Text" },
            "Обычный текст вызвал ненужный запрос участника или потерял автора");
    }

    // Неполный буфер откладывает запись до следующего пакета, не блокируя обработку сообщения
    private static async Task PartialMessageBufferDoesNotBlockAsync()
    {
        using var files = new MediaFixture();
        await files.CallAsync("FlushMessageBufferAsync", new TgParseHelper(), true, false, false, CancellationToken.None);
        Assert(!files.State.IsDownloadProcessing, "Проверка небольшого буфера запустила загрузку");
    }

    // Временная ошибка передачи должна повторить чтение и вернуть реальный результат
    private static async Task TelegramRequestRetriesTransportFailureAsync()
    {
        using var files = new MediaFixture();
        var calls = 0;
        Func<CancellationToken, Task<int>> action = _ => ++calls == 1
            ? Task.FromException<int>(new IOException("Тестовый обрыв соединения")) : Task.FromResult(42);
        var result = await InvokeTelegramRequestAsync(files.Connection, action);
        Assert(result == 42 && calls == 2, "Чтение не восстановилось после временного обрыва");
    }

    // Отмена внутреннего транспорта WTelegram отличается от отмены операции пользователем
    private static async Task TransportCancellationRetriesWithoutUserCancellationAsync()
    {
        using var files = new MediaFixture();
        var calls = 0;
        Func<CancellationToken, Task<int>> action = _ => ++calls == 1
            ? Task.FromException<int>(new TaskCanceledException("Транспорт прерван")) : Task.FromResult(42);
        var result = await InvokeTelegramRequestAsync(files.Connection, action);
        Assert(result == 42 && calls == 2, "Внутренняя отмена транспорта принята за остановку пользователем");
    }

    // Ошибки прав доступа не должны повторяться как сетевые сбои
    private static async Task TelegramRequestDoesNotRetryRpcErrorAsync()
    {
        using var files = new MediaFixture();
        var calls = 0;
        Func<CancellationToken, Task<int>> action = _ => { calls++; return Task.FromException<int>(new TL.RpcException(400, "CHANNEL_PRIVATE")); };
        try { await InvokeTelegramRequestAsync(files.Connection, action); }
        catch (TL.RpcException) { Assert(calls == 1, "Ошибка RPC повторилась как сетевой сбой"); return; }
        throw new InvalidOperationException("Ошибка доступа скрыта");
    }

    // Отсутствие клиента не должно создавать отменённую задачу с неотменённым токеном
    private static async Task MissingClientIsNotFalseCancellationAsync()
    {
        using var files = new MediaFixture();
        files.Connection.Client = null;
        try { await InvokeTelegramRequestAsync(files.Connection, _ => Task.FromResult(42)); }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Telegram")) { return; }
        throw new InvalidOperationException("Нет понятной ошибки отсутствующего подключения");
    }

    // Полученные онлайн участники должны остаться доступными после перечитывания страницы из БД
    private static async Task OnlineParticipantsPersistMembershipAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        context.Users.Add(new() { Uid = Guid.NewGuid(), Id = 20, UserName = "member" });
        await context.SaveChangesAsync();
        var client = App.BusinessLogicManager.ConnectClient;
        client.Ready = true;
        client.Participants.Add(new(new TL.User { id = 20, username = "member" }));
        var vm = new TgChatParticipantsViewModel(_state, _settings, _navigation, null!, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        await vm.GetParticipantsCommand.ExecuteAsync(null);
        var linked = await context.ChatUsers.AsNoTracking().AnyAsync(x => x.ChatId == _source.Id && x.UserId == 20 && !x.IsDeleted);
        Assert(linked && vm.Errors.Count == 0 && !vm.IsEmptyData, "Онлайн-участники не связаны с чатом в БД или остались скрытыми");
    }

    // Получение участников должно показывать тот же порядок и счётчики, что и следующая загрузка из БД
    private static async Task OnlineParticipantsReloadCountsAndOrderAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        context.Users.AddRange(new TgEfUserEntity { Uid = Guid.NewGuid(), Id = 20, UserName = "a" },
            new TgEfUserEntity { Uid = Guid.NewGuid(), Id = 21, UserName = "b" });
        context.Messages.Add(new() { Uid = Guid.NewGuid(), SourceId = _source.Id, UserId = 21, Id = 1 });
        await context.SaveChangesAsync();
        var client = App.BusinessLogicManager.ConnectClient;
        client.Ready = true;
        client.Participants.Add(new(new TL.User { id = 20, username = "a" }));
        client.Participants.Add(new(new TL.User { id = 21, username = "b" }));
        var vm = new TgChatParticipantsViewModel(_state, _settings, _navigation, null!, null!) { PageTake = 3 };
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        await vm.GetParticipantsCommand.ExecuteAsync(null);
        Assert(vm.Errors.Count == 0 && vm.UserDtos.First().Id == 21 && vm.UserDtos.First().MessagesCount == 1
            && vm.PageSkip == 2 && !vm.HasMoreItems,
            $"Онлайн-список потерял счётчики, порядок или границу пагинации: {string.Join(',', vm.UserDtos.Select(x => $"{x.Id}:{x.MessagesCount}"))}; "
                + $"skip={vm.PageSkip}; take={vm.PageTake}; more={vm.HasMoreItems}; errors={string.Join(',', vm.Errors)}");
    }

    // Вызываем настоящий общий обработчик запросов без сетевой подмены его логики
    private static async Task<int> InvokeTelegramRequestAsync(TgConnectClientBase connection, Func<CancellationToken, Task<int>> action)
    {
        var method = typeof(TgConnectClientBase).GetMethod("TelegramCallAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.MakeGenericMethod(typeof(int));
        return await (Task<int>)method.Invoke(connection, [action, true, CancellationToken.None])!;
    }

    // Читаем настройки выбранного чата без изменений исходной БД
    private static async Task<int> InspectChatAsync(string database, long chatId)
    {
        try
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
                { DataSource = database, Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly };
            await using var context = new TestContext(new DbContextOptionsBuilder().UseSqlite(connection.ToString()).Options);
            var chat = await context.Sources.AsNoTracking().SingleAsync(x => x.Id == chatId);
            var messages = await context.Messages.AsNoTracking().Where(x => x.SourceId == chatId).CountAsync();
            Console.WriteLine($"ID={chat.Id}; Title={chat.Title}; Directory={chat.Directory}; FirstId={chat.FirstId}; Count={chat.Count}");
            Console.WriteLine($"Messages={messages}; AutoUpdate={chat.IsAutoUpdate}; SaveFiles={chat.IsSaveFiles}; JoinId={chat.IsJoinFileNameWithMessageId}");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }
}
