namespace OpenTgResearcherDesktop.ChatSelfTests;

internal static partial class Program
{
    // Проверяем разрешение источника без подмены подготовки чата
    private static IEnumerable<(string Name, Func<Task> Run)> GetPublicChatTests() =>
    [
        ("PublicChatMatchesUsernameInsteadOfFirstDialog", PublicChatMatchesUsernameInsteadOfFirstDialogAsync),
        ("PublicChatResolvesOwnAccessHash", PublicChatResolvesOwnAccessHashAsync),
        ("PublicChatRejectsMismatchedId", PublicChatRejectsMismatchedIdAsync),
        ("PublicChatUnknownIdExplainsUsername", PublicChatUnknownIdExplainsUsernameAsync),
        ("PublicChatPersistsWithoutSubscription", PublicChatPersistsWithoutSubscriptionAsync),
        ("PublicChatPreservesExistingSettings", PublicChatPreservesExistingSettingsAsync),
        ("PublicChatHistorySurvivesDialogRefresh", PublicChatHistorySurvivesDialogRefreshAsync),
        ("PublicChatPrivateHistoryExplained", PublicChatPrivateHistoryExplainedAsync),
        ("PublicChatMissingUsernameExplained", PublicChatMissingUsernameExplainedAsync),
        ("PublicChatRejectsUserAndInvitation", PublicChatRejectsUserAndInvitationAsync),
        ("PublicChatSubscribedAndSmallGroup", PublicChatSubscribedAndSmallGroupAsync),
        ("PublicChatRegistryOpensResolvedSource", PublicChatRegistryOpensResolvedSourceAsync),
        ("PublicChatRegistryDisplaysFailure", PublicChatRegistryDisplaysFailureAsync),
        ("PublicChatRegistryLocalizedEntry", PublicChatRegistryLocalizedEntryAsync),
        ("PublicChatParsingSavesMessagesAndFiles", PublicChatParsingSavesMessagesAndFilesAsync),
        ("PublicChatParsingDenialIsFailed", PublicChatParsingDenialIsFailedAsync),
        ("PublicChatRefreshPreservesExternalAccess", PublicChatRefreshPreservesExternalAccessAsync),
        ("PublicChatParsingReservesResolvedId", PublicChatParsingReservesResolvedIdAsync),
    ];

    // Имя с символом @ должно выбирать нужный источник, а не первый диалог аккаунта
    private static async Task PublicChatMatchesUsernameInsteadOfFirstDialogAsync()
    {
        using var fixture = new MediaFixture();
        fixture.Connection.DicChats[10] = new TL.Channel { id = 10, username = "other_chat", access_hash = 100 };
        fixture.Connection.DicChats[1002003004] = new TL.Channel
        {
            id = 1002003004, username = "durov", access_hash = 900,
            flags = TL.Channel.Flags.has_access_hash | TL.Channel.Flags.left
        };
        fixture.Settings.SourceVm.Dto.Id = 0;
        fixture.Settings.SourceVm.Dto.UserName = "@durov";
        var chat = await fixture.Connection.GetChatBaseAsync(fixture.Settings, default);
        Assert(chat?.ID == 1002003004, "Вместо указанного публичного источника выбран посторонний диалог");
        Assert(fixture.Settings.SourceVm.Dto.Id == 1002003004, "Не сохранён разрешённый ID источника");
        Assert(fixture.NetworkCalls == 0, "Для известного источника выполнен лишний сетевой запрос");
    }

    // Чужой публичный источник должен получить адрес канала, а не access_hash пользователя
    private static async Task PublicChatResolvesOwnAccessHashAsync()
    {
        using var fixture = PublicChatFixture();
        var client = (PublicChatConnection)fixture.Connection;
        fixture.Settings.SourceVm.Dto.Id = client.Channel.id;
        fixture.Settings.SourceVm.Dto.UserName = "@durov";
        await client.CreateChatBaseCoreAsync(fixture.Settings);
        Assert(ReferenceEquals(fixture.Settings.Chat.Base, client.Channel), "Потерян объект разрешённого канала");
        Assert(fixture.Settings.SourceVm.Dto.AccessHash == 900 && client.ResolveCalls == 1, "Не сохранён собственный access_hash канала");
        Assert(await client.GetChatLastMessageIdAsync(client.Channel.id, default) == 2, "Внешний источник не доступен дальнейшему чтению истории");
        Assert(client.HistoryPeer is TL.InputPeerChannel { channel_id: 1002003004, access_hash: 900 }, "История запрашивается с неверным peer");
        Assert(fixture.NetworkCalls == 0, "Вместо подменённого транспорта выполнен посторонний запрос или вступление");
    }

    // Пара ID и имени разных источников не должна незаметно перенаправлять парсинг
    private static async Task PublicChatRejectsMismatchedIdAsync()
    {
        using var fixture = PublicChatFixture();
        fixture.Settings.SourceVm.Dto.Id = 10;
        fixture.Settings.SourceVm.Dto.UserName = "durov";
        await ExpectPublicChatErrorAsync(() => fixture.Connection.CreateChatBaseCoreAsync(fixture.Settings), "разным источникам");
        Assert(fixture.Settings.Chat.Base is null && fixture.Settings.SourceVm.Dto.Id == 10, "Несовпадение ID изменило выбранный источник");
    }

    // По неизвестному числовому ID Telegram не предоставляет access_hash
    private static async Task PublicChatUnknownIdExplainsUsernameAsync()
    {
        using var fixture = PublicChatFixture();
        fixture.Settings.SourceVm.Dto.Id = 1002003004;
        await ExpectPublicChatErrorAsync(() => fixture.Connection.CreateChatBaseCoreAsync(fixture.Settings), "@durov");
        Assert(((PublicChatConnection)fixture.Connection).ResolveCalls == 0, "Числовой ID отправлен как публичное имя");
    }

    // Добавление проверяет настоящую историю и сохраняет доступ отдельно от участия
    private static async Task PublicChatPersistsWithoutSubscriptionAsync()
    {
        using var fixture = PublicChatFixture();
        var client = (PublicChatConnection)fixture.Connection;
        var dto = await client.AddPublicChatAsync("https://t.me/durov/");
        using var sources = new TgEfSourceRepository();
        var saved = await sources.GetDtoAsync(x => x.Id == dto.Id);
        Assert(saved.Id == 1002003004 && saved.Title == "Public channel" && saved.AccessHash == 900 && saved.Count == 2,
            "Разрешённый источник не сохранён в SQLite с доступной границей истории");
        Assert(saved.IsUserAccess && !saved.IsSubscribe && dto.Uid == saved.Uid, "Доступ к публичной истории ошибочно оформил подписку");
        Assert(saved.IsSaveMessages && saved.IsSaveFiles && saved.FirstId == 1, "Новый источник не подготовлен к сохранению сообщений и файлов");
        Assert(client.HistoryCalls == 1 && fixture.NetworkCalls == 0, "История не проверена или выполнено вступление");
    }

    // Повторное добавление обновляет адрес, сохраняя каталог, позицию и опции существующего источника
    private static async Task PublicChatPreservesExistingSettingsAsync()
    {
        using var fixture = PublicChatFixture();
        using var sources = new TgEfSourceRepository();
        var existing = new TgEfSourceDto { Id = 1002003004, UserName = "durov", Directory = fixture.Directory,
            FirstId = 12, IsSaveMessages = true, IsSaveFiles = true, IsAutoUpdate = true, CountThreads = 3 };
        await sources.SaveAsync(existing);
        var dto = await fixture.Connection.AddPublicChatAsync("@durov");
        var saved = await sources.GetDtoAsync(x => x.Id == existing.Id);
        Assert(dto.Uid == saved.Uid && saved.Directory == fixture.Directory && saved.FirstId == 12 && saved.CountThreads == 3,
            "Обновление адреса потеряло идентичность или настройки источника");
        Assert(saved.IsSaveMessages && saved.IsSaveFiles && saved.IsAutoUpdate && saved.AccessHash == 900,
            "Повторное добавление сбросило параметры парсинга");
    }

    // Обновление подписок не должно удалять адрес внешнего публичного источника
    private static async Task PublicChatHistorySurvivesDialogRefreshAsync()
    {
        using var fixture = PublicChatFixture();
        var dto = await fixture.Connection.AddPublicChatAsync("t.me/durov");
        typeof(TgConnectClientBase).GetMethod("FillEnumerableChats", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(fixture.Connection, [new Dictionary<long, TL.ChatBase> { [10] = new TL.Chat { id = 10 } }]);
        Assert(await fixture.Connection.GetChatLastMessageIdAsync(dto.Id, default) == 2, "Обновление подписок потеряло публичный источник");
        Assert(fixture.Connection.GetChatUserName(dto.Id) == "durov", "Для ссылок на внешний источник потеряно публичное имя");
    }

    // Отказ истории не должен создавать в БД доступный источник
    private static async Task PublicChatPrivateHistoryExplainedAsync()
    {
        using var fixture = PublicChatFixture();
        ((PublicChatConnection)fixture.Connection).HistoryError = new TL.RpcException(400, "CHANNEL_PRIVATE");
        await ExpectPublicChatErrorAsync(() => fixture.Connection.AddPublicChatAsync("@durov"), "не разрешает читать историю");
        using var sources = new TgEfSourceRepository();
        Assert(await sources.GetCountAsync(x => x.Id == 1002003004) == 0, "Отказ истории сохранён как доступный источник");
    }

    // Удалённое или ошибочное имя должно иметь объяснение на языке пользователя
    private static async Task PublicChatMissingUsernameExplainedAsync()
    {
        using var fixture = PublicChatFixture();
        ((PublicChatConnection)fixture.Connection).ResolveError = new TL.RpcException(400, "USERNAME_NOT_OCCUPIED");
        await ExpectPublicChatErrorAsync(() => fixture.Connection.AddPublicChatAsync("@missing_name"), "Публичный чат не найден");
    }

    // Пользователь и приглашение не являются публичным каналом для парсинга
    private static async Task PublicChatRejectsUserAndInvitationAsync()
    {
        using var fixture = PublicChatFixture();
        var client = (PublicChatConnection)fixture.Connection;
        foreach (var reference in new[] { "1002003004", "https://t.me/+private", "https://t.me/c/123/1", "https://example.com/channel" })
            await ExpectPublicChatErrorAsync(() => client.AddPublicChatAsync(reference), "Укажите", ignoreCase: true);
        Assert(client.ResolveCalls == 0, "Приглашение или некорректная ссылка отправлены в Telegram");
        client.ResolveUser = true;
        await ExpectPublicChatErrorAsync(() => client.AddPublicChatAsync("@some_user"), "не является публичным");
    }

    // Известный канал и обычная группа по ID продолжают разрешаться без обращения к сети
    private static async Task PublicChatSubscribedAndSmallGroupAsync()
    {
        using var fixture = PublicChatFixture();
        var client = (PublicChatConnection)fixture.Connection;
        client.Channel.flags &= ~TL.Channel.Flags.left;
        client.DicChats[client.Channel.id] = client.Channel;
        var dto = await client.AddPublicChatAsync("durov");
        Assert(dto.IsSubscribe && dto.IsUserAccess && client.ResolveCalls == 0, "У подписанного канала потерян признак участия");
        fixture.Settings.SourceVm.Dto.Id = 10;
        fixture.Settings.SourceVm.Dto.UserName = string.Empty;
        var group = await client.GetChatBaseAsync(fixture.Settings, default);
        Assert(group is TL.Chat { id: 10 }, "Обычная группа не разрешается по известному ID");
    }

    // Команда страницы должна передать имя и открыть настройки именно разрешённого источника
    private static async Task PublicChatRegistryOpensResolvedSourceAsync()
    {
        var navigation = new INavigationService();
        var vm = new TgChatsViewModel(_state, _settings, navigation, null!)
        {
            SelectedPublicChat = new(new TgPublicChatSearchResult(1002003004, "Public channel", "durov", false, null))
        };
        var calls = 0;
        App.BusinessLogicManager.ConnectClient.Ready = true;
        App.BusinessLogicManager.ConnectClient.AddPublicChat = reference =>
        {
            Assert(reference == "@durov", "Потерян выбранный источник");
            calls++;
            return Task.FromResult(_source);
        };
        await vm.AddPublicChatCommand.ExecuteAsync(null);
        Assert(navigation.PageKey is null && vm.SelectedPublicChat.IsSaved, "Добавление не сохранило результат или сразу открыло пульт");
        await vm.AddPublicChatCommand.ExecuteAsync(null);
        Assert(calls == 1 && navigation.Parameter is Guid uid && uid == _source.Uid
            && navigation.PageKey == typeof(TgChatViewModel).FullName && vm.PublicChatError.Length == 0,
            "Команда не открыла настройки разрешённого источника");
    }

    // Отказ Telegram остаётся виден на странице и не вызывает переход к пустому чату
    private static async Task PublicChatRegistryDisplaysFailureAsync()
    {
        var navigation = new INavigationService();
        var vm = new TgChatsViewModel(_state, _settings, navigation, null!)
        {
            SelectedPublicChat = new(new TgPublicChatSearchResult(1002003004, "Public channel", "durov", false, null))
        };
        App.BusinessLogicManager.ConnectClient.Ready = true;
        App.BusinessLogicManager.ConnectClient.AddPublicChat = _ => throw new InvalidOperationException("История недоступна");
        await vm.AddPublicChatCommand.ExecuteAsync(null);
        Assert(vm.PublicChatError == "История недоступна" && vm.Errors.Count == 1 && navigation.PageKey is null,
            "Ошибка добавления скрыта или открыта пустая страница");
    }

    // Точка добавления и пояснения присутствуют на странице на обоих языках
    private static Task PublicChatRegistryLocalizedEntryAsync()
    {
        var page = ReadMarkup("TgChatsPage.xaml");
        Assert(page.Descendants().Any(x => (string?)x.Attribute("Command") == "{x:Bind ViewModel.AddPublicChatCommand}"), "Нет команды добавления");
        foreach (var language in new[] { "ru-RU", "en-US" })
        {
            using var stream = typeof(Program).Assembly.GetManifestResourceStream($"ChatResources.{language}.resw")!;
            var data = XDocument.Load(stream).Descendants("data");
            foreach (var key in new[] { "GlobalSearchLocal.Text", "GlobalSearchTelegram.Text", "GlobalSearchAdd", "GlobalSearchPreviewHint.Text" })
                Assert(data.Any(x => (string?)x.Attribute("name") == key && x.Element("value")!.Value.Length > 0), $"Нет перевода {key}: {language}");
        }
        return Task.CompletedTask;
    }

    // Выполняем настоящую подготовку, проверку истории, парсинг и запись файла без участия в канале
    private static async Task PublicChatParsingSavesMessagesAndFilesAsync()
    {
        using var fixture = PublicChatFixture();
        var client = (PublicChatConnection)fixture.Connection;
        client.SeedParsingData = true;
        using var apps = new TgEfAppRepository();
        await apps.SetUseClientAsync(useClient: true);
        var settings = CreateParallelSettings(client.Channel.id, fixture.Directory);
        settings.SourceVm.Dto.UserName = "@durov";
        settings.Chat.Base = null!;
        await client.ParseChatAsync(settings).WaitAsync(TimeSpan.FromSeconds(8));
        using var sources = new TgEfSourceRepository();
        using var messages = new TgEfMessageRepository();
        var source = await sources.GetDtoAsync(x => x.Id == client.Channel.id);
        Assert(await messages.GetCountAsync(x => x.SourceId == client.Channel.id && x.Id == 1) == 1,
            "Полный парсинг не сохранил сообщение внешнего канала");
        var files = Directory.GetFiles(settings.SourceVm.Dto.Directory, "sample.bin", SearchOption.AllDirectories);
        Assert(files.Length == 1 && new FileInfo(files[0]).Length == 4, "Полный парсинг не сохранил файл внешнего канала");
        Assert(source.IsUserAccess && !source.IsSubscribe && source.AccessHash == 900, "Парсинг потерял доступ или оформил подписку");
        Assert(fixture.State.NetworkActivity.Operations.Single(x => x.Kind == TgNetworkOperationKind.Parsing).State
            == TgNetworkOperationState.Completed && fixture.NetworkCalls == 0, "Парсинг завершён с ошибкой или обращался к постороннему API");
    }

    // Отказ чтения истории завершает настоящий парсинг ошибкой и не сохраняет ложный успех
    private static async Task PublicChatParsingDenialIsFailedAsync()
    {
        using var fixture = PublicChatFixture();
        var client = (PublicChatConnection)fixture.Connection;
        client.HistoryError = new TL.RpcException(400, "CHANNEL_PRIVATE");
        var settings = CreateParallelSettings(client.Channel.id, fixture.Directory);
        settings.SourceVm.Dto.UserName = "durov";
        await ExpectPublicChatErrorAsync(() => client.ParseChatAsync(settings), "не разрешает читать историю");
        Assert(fixture.State.NetworkActivity.Operations.Single(x => x.Kind == TgNetworkOperationKind.Parsing).State
            == TgNetworkOperationState.Failed, "Отказ Telegram отмечен как успешный пустой парсинг");
    }

    // Отсутствие источника среди подписок не означает потерю уже проверенного доступа к публичной истории
    private static async Task PublicChatRefreshPreservesExternalAccessAsync()
    {
        using var storage = new TgStorageService();
        using var fixture = new MediaFixture((cache, state) => new RegistryScanConnection(storage, cache, state));
        using var sources = new TgEfSourceRepository();
        await sources.SaveAsync(new TgEfSourceDto { Id = 1002003004, UserName = "durov", IsUserAccess = true, IsSubscribe = false });
        await sources.SaveAsync(new TgEfSourceDto { Id = 20, UserName = string.Empty, IsUserAccess = true, IsSubscribe = true });
        await fixture.CallAsync("SetUserAccessForAllChatsAsync", null!, false);
        var external = await sources.GetDtoAsync(x => x.Id == 1002003004);
        var privateChat = await sources.GetDtoAsync(x => x.Id == 20);
        Assert(external.IsUserAccess && !external.IsSubscribe, "Обновление подписок ошибочно отозвало доступ к внешней публичной истории");
        Assert(!privateChat.IsUserAccess, "Проверка перестала обновлять доступ к отсутствующему приватному чату");
    }

    // Запуск по одному имени резервирует настоящий ID, иначе пауза и запрет дублей относятся к нулевому чату
    private static async Task PublicChatParsingReservesResolvedIdAsync()
    {
        using var fixture = PublicChatFixture();
        var client = (PublicChatConnection)fixture.Connection;
        client.SeedParsingData = true;
        client.HoldHistory = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using var apps = new TgEfAppRepository();
        await apps.SetUseClientAsync(useClient: true);
        var settings = CreateParallelSettings(client.Channel.id, fixture.Directory);
        settings.SourceVm.Dto.Id = 0;
        settings.SourceVm.Dto.UserName = "durov";
        settings.Chat.Base = null!;
        var parsing = client.ParseChatAsync(settings);
        try
        {
            await client.HistoryStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var running = (System.Collections.Concurrent.ConcurrentDictionary<long, TaskCompletionSource>)typeof(TgConnectClientBase)
                .GetField("_parsingChats", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(client)!;
            Assert(running.ContainsKey(client.Channel.id) && !running.ContainsKey(0), "Парсинг по имени зарезервировал нулевой ID");
        }
        finally { client.HoldHistory.TrySetResult(); await parsing.WaitAsync(TimeSpan.FromSeconds(8)); }
    }

    // Создаём сетевой клиент только с изолированным транспортом и посторонней известной группой
    private static MediaFixture PublicChatFixture()
    {
        var fixture = new MediaFixture((cache, state) => new PublicChatConnection(cache, state));
        fixture.Connection.DicChats[10] = new TL.Chat { id = 10, title = "Other group" };
        return fixture;
    }

    // Требуем понятное исключение, чтобы отсутствие действий не считалось успешным результатом
    private static async Task ExpectPublicChatErrorAsync(Func<Task> action, string expected, bool ignoreCase = false)
    {
        try { await action(); }
        catch (InvalidOperationException ex)
        {
            Assert(ex.Message.Contains(expected, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal),
                $"Непонятное объяснение: {ex.Message}");
            return;
        }
        throw new InvalidOperationException($"Не получен ожидаемый отказ: {expected}");
    }
}

// Подменяем ответы Telegram, оставляя настоящие разрешение, проверку истории и сохранение в SQLite
internal sealed class PublicChatConnection(FusionCache cache, LoadStateService state) : MediaTestConnection(cache, state)
{
    public TL.Channel Channel { get; } = new()
    {
        id = 1002003004, username = "durov", title = "Public channel", access_hash = 900,
        flags = TL.Channel.Flags.has_access_hash | TL.Channel.Flags.left
    };
    public int ResolveCalls { get; private set; }
    public int HistoryCalls { get; private set; }
    public TL.InputPeer? HistoryPeer { get; private set; }
    public Exception? ResolveError { get; set; }
    public Exception? HistoryError { get; set; }
    public bool ResolveUser { get; set; }
    public bool SeedParsingData { get; set; }
    public TL.Contacts_Found? SearchReply { get; set; }
    public string? SearchQuery { get; private set; }
    public TL.Messages_ChatFull? ProfileReply { get; set; }
    public TL.Channel? ProfileChannel { get; private set; }
    public byte[] PhotoReply { get; set; } = [];
    public int PhotoCalls { get; private set; }
    public TL.Messages_SearchCounter[] MediaReply { get; set; } = [];
    public int MediaCalls { get; private set; }
    public TaskCompletionSource HistoryStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource? HoldHistory { get; set; }

    /// <summary> Сохраняет изолированный клиент без авторизации и чтения пользовательской сессии </summary>
    public override Task LoginUserAsync(bool isProxyUpdate) => Task.CompletedTask;

    /// <summary> Возвращает заданную выдачу поиска без вызова Telegram </summary>
    protected override Task<TL.Contacts_Found> SearchPublicChatPeersAsync(string query, CancellationToken ct = default)
    {
        SearchQuery = query;
        return Task.FromResult(SearchReply ?? throw new InvalidOperationException("Не задана выдача поиска"));
    }

    /// <summary> Подменяет только сетевое получение информации канала </summary>
    protected override Task<TL.Messages_ChatFull> ReadPublicChatProfileAsync(TL.Channel channel, CancellationToken ct = default)
    {
        ProfileChannel = channel;
        return Task.FromResult(ProfileReply ?? throw new InvalidOperationException("Не заданы сведения канала"));
    }

    /// <summary> Записывает содержимое фотографии в настоящий поток загрузки </summary>
    protected override async Task DownloadPublicChatPhotoAsync(TL.Channel channel, Stream output, CancellationToken ct = default)
    {
        PhotoCalls++;
        await output.WriteAsync(PhotoReply, ct);
    }

    /// <summary> Подменяет только сетевые счётчики медиа </summary>
    protected override Task<TL.Messages_SearchCounter[]> ReadPublicChatMediaCountsAsync(TL.Channel channel, CancellationToken ct = default)
    {
        MediaCalls++;
        return Task.FromResult(MediaReply);
    }

    /// <summary> Возвращает полный объект источника с его собственным адресом </summary>
    protected override Task<TL.Contacts_ResolvedPeer> ResolveChatUsernameAsync(string username, CancellationToken ct = default)
    {
        ResolveCalls++;
        if (ResolveError is not null) throw ResolveError;
        return Task.FromResult(new TL.Contacts_ResolvedPeer
        {
            peer = ResolveUser ? new TL.PeerUser { user_id = 50 } : new TL.PeerChannel { channel_id = Channel.id },
            chats = new() { [Channel.id] = Channel }
        });
    }

    // Заполняем кеш текущего запуска ответами сервера, не подменяя парсер
    private async Task SeedParsingCacheAsync(CancellationToken ct)
    {
        var parsingCache = (IFusionCache)typeof(TgConnectClientBase).GetProperty("Cache", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(this)!;
        await parsingCache.SetAsync<TL.Messages_ChatFull>(TgBusinessLogic.Helpers.TgCacheUtils.GetCacheKeyFullChat(Channel.id),
            new() { full_chat = new TL.ChannelFull { id = Channel.id, about = "Public history" } }, token: ct);
        var message = new TL.Message { id = 1, peer_id = new TL.PeerChannel { channel_id = Channel.id }, date = DateTime.UtcNow,
            message = "Public message", flags = TL.Message.Flags.has_media,
            media = new TL.MessageMediaDocument { flags = TL.MessageMediaDocument.Flags.has_document,
                document = new TL.Document { id = 99, size = 4, date = DateTime.UtcNow, mime_type = "application/octet-stream",
                    attributes = [new TL.DocumentAttributeFilename { file_name = "sample.bin" }] } } };
        await parsingCache.SetAsync<TL.Messages_MessagesBase>(TgBusinessLogic.Helpers.TgCacheUtils.GetCacheKeyMessages(Channel.id, 1, 2),
            new TL.Messages_Messages { messages = [message] }, token: ct);
    }

    /// <summary> Фиксирует адрес чтения истории без любых действий с подпиской </summary>
    protected override async Task<TL.Messages_MessagesBase> ReadChatHistoryAsync(TL.InputPeer peer, CancellationToken ct = default)
    {
        HistoryCalls++;
        HistoryPeer = peer;
        if (SeedParsingData) await SeedParsingCacheAsync(ct);
        HistoryStarted.TrySetResult();
        if (HoldHistory is not null) await HoldHistory.Task.WaitAsync(ct);
        if (HistoryError is not null) throw HistoryError;
        return new TL.Messages_Messages { messages = [new TL.Message { id = 2 }] };
    }
}
