namespace OpenTgResearcherDesktop.ChatSelfTests;

internal static partial class Program
{
    // Проверяем поиск публичного источника без изменения подписки и локального реестра
    private static IEnumerable<(string Name, Func<Task> Run)> GetGlobalSearchTests() =>
    [
        ("GlobalSearchDoesNotSaveBeforeConfirmation", GlobalSearchDoesNotSaveBeforeConfirmationAsync),
        ("GlobalSearchFiltersPeersAndFindsSavedSource", GlobalSearchFiltersPeersAndFindsSavedSourceAsync),
        ("GlobalSearchScopesAndKinds", GlobalSearchScopesAndKindsAsync),
        ("GlobalSearchCancellationDiscardsResponse", GlobalSearchCancellationDiscardsResponseAsync),
        ("GlobalSearchFailureClearsOldResults", GlobalSearchFailureClearsOldResultsAsync),
        ("GlobalSearchSavedSourceOpensOffline", GlobalSearchSavedSourceOpensOfflineAsync),
        ("GlobalSearchMarkupAndTranslations", GlobalSearchMarkupAndTranslationsAsync),
        ("GlobalSearchProfileHasPhotoAndRightPane", GlobalSearchProfileHasPhotoAndRightPaneAsync),
        ("GlobalProfileReadsInformationPhotoAndCounters", GlobalProfileReadsInformationPhotoAndCountersAsync),
        ("GlobalProfileLoadsOnlySelectedAndMediaOnDemand", GlobalProfileLoadsOnlySelectedAndMediaOnDemandAsync),
        ("GlobalProfileDiscardsPreviousSelection", GlobalProfileDiscardsPreviousSelectionAsync),
        ("GlobalProfileFailureCanRetry", GlobalProfileFailureCanRetryAsync),
        ("GlobalPhotoFailurePreservesOtherResults", GlobalPhotoFailurePreservesOtherResultsAsync),
    ];

    // Получение профиля и фотографии не должно читать историю или создавать записи чатов
    private static async Task GlobalProfileReadsInformationPhotoAndCountersAsync()
    {
        using var fixture = PublicChatFixture();
        var client = (PublicChatConnection)fixture.Connection;
        client.Channel.photo = new TL.ChatPhoto { photo_id = 9, dc_id = 2 };
        client.Channel.usernames = [new() { username = "durov", flags = TL.Username.Flags.active },
            new() { username = "public_alias", flags = TL.Username.Flags.active }, new() { username = "inactive_alias" }];
        client.ProfileReply = new()
        {
            full_chat = new TL.ChannelFull { id = client.Channel.id, about = "Описание\nВторая строка", participants_count = 359775,
                linked_chat_id = 200, flags = TL.ChannelFull.Flags.has_participants_count | TL.ChannelFull.Flags.has_linked_chat_id },
            chats = new() { [client.Channel.id] = client.Channel, [200] = new TL.Channel { id = 200, username = "public_discussion" } }
        };
        using var photo = typeof(Program).Assembly.GetManifestResourceStream("cover.jpg")!;
        using var buffer = new MemoryStream();
        await photo.CopyToAsync(buffer);
        client.PhotoReply = buffer.ToArray();
        client.MediaReply = [new() { filter = new TL.InputMessagesFilterPhotos(), count = 661 },
            new() { filter = new TL.InputMessagesFilterVideo(), count = 45, flags = TL.Messages_SearchCounter.Flags.inexact },
            new() { filter = new TL.InputMessagesFilterVoice(), count = 0 }];
        await client.SearchPublicChatsAsync("@durov");
        var profile = await client.GetPublicChatProfileAsync(client.Channel.id);
        var bytes = await client.GetPublicChatPhotoAsync(client.Channel.id);
        var counts = await client.GetPublicChatMediaCountsAsync(client.Channel.id);
        Assert(profile.About == "Описание\nВторая строка" && profile.ParticipantsCount == 359775
            && profile.UserNames.SequenceEqual(new[] { "durov", "public_alias" }) && profile.DiscussionLink == "https://t.me/public_discussion",
            "Потеряны описание, число подписчиков или активные ссылки источника");
        Assert(client.ProfileChannel?.access_hash == client.Channel.access_hash && bytes.SequenceEqual(client.PhotoReply) && client.PhotoCalls == 1,
            "Профиль использует чужой адрес или фотография не загружена");
        Assert(counts.Count == 3 && counts.Single(x => x.Kind == "Photos").Count == 661
            && counts.Single(x => x.Kind == "Videos").IsApproximate && counts.Single(x => x.Kind == "Voice").Count == 0,
            "Счётчики потеряли тип, значение или отметку приблизительного количества");
        using var repository = new TgEfSourceRepository();
        Assert((await repository.GetDtoAsync(x => x.Id == client.Channel.id)).Id == 0 && client.HistoryCalls == 0,
            "Просмотр информации сохранил чат или начал чтение истории");
        client.Channel.photo = null!;
        Assert((await client.GetPublicChatPhotoAsync(client.Channel.id)).Length == 0 && client.PhotoCalls == 1,
            "Отсутствующая фотография вызвала загрузку или вернула старое изображение");
    }

    // Список виден до загрузки фото; описание относится к выбранной строке, медиа запрашиваются при раскрытии
    private static async Task GlobalProfileLoadsOnlySelectedAndMediaOnDemandAsync()
    {
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!) { SearchScope = 1, FilterText = "telegram" };
        var client = App.BusinessLogicManager.ConnectClient;
        client.Ready = true;
        var photos = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        var profiles = new List<long>(); var mediaCalls = 0;
        client.SearchPublicChats = (_, _) => Task.FromResult(new List<TgPublicChatSearchResult>
            { new(1, "Channel", "durov", false, null), new(2, "Group", "public_group", true, null) });
        client.PublicProfile = (id, _) =>
        {
            profiles.Add(id);
            return Task.FromResult(new TgPublicChatProfile("Description " + id, 123, ["durov"], null));
        };
        client.PublicPhoto = (_, _) => photos.Task;
        client.PublicMedia = (_, _) => { mediaCalls++; return Task.FromResult(new List<TgPublicChatMediaCount> { new("Photos", 12, false) }); };
        await vm.SearchChatsCommand.ExecuteAsync(null);
        await vm.PublicProfileTask;
        Assert(vm.PublicSearchRows.Count == 2 && !vm.IsPublicSearchBusy && !vm.PublicPhotosTask.IsCompleted
            && profiles.SequenceEqual(new long[] { 1 }) && mediaCalls == 0 && vm.SelectedPublicChat!.About == "Description 1",
            "Фото блокирует выдачу, описание запрошено для всех строк или медиа загружены заранее");
        photos.SetResult([1, 2, 3]);
        await vm.PublicPhotosTask;
        Assert(vm.PublicSearchRows.All(x => x.Photo?.SequenceEqual(new byte[] { 1, 2, 3 }) == true), "Фото не переданы строкам списка");
        await vm.LoadPublicMediaCommand.ExecuteAsync(null);
        await vm.LoadPublicMediaCommand.ExecuteAsync(null);
        Assert(mediaCalls == 1 && vm.SelectedPublicChat!.Media.Single().Count == "12", "Счётчики не отображены или загружаются повторно при раскрытии");
        vm.ClosePublicProfileCommand.Execute(null);
        Assert(!vm.IsPublicProfileOpen, "Карточка не закрывается");
        vm.OpenPublicProfile(vm.SelectedPublicChat!);
        Assert(vm.IsPublicProfileOpen && profiles.Count == 1, "Повторное нажатие не открывает карточку или повторно загружает готовый профиль");
    }

    // Поздние сведения предыдущего источника не должны заменять открытую карточку
    private static async Task GlobalProfileDiscardsPreviousSelectionAsync()
    {
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!) { SearchScope = 1 };
        var first = new TaskCompletionSource<TgPublicChatProfile>(TaskCreationOptions.RunContinuationsAsynchronously);
        App.BusinessLogicManager.ConnectClient.PublicProfile = (id, _) => id == 1 ? first.Task
            : Task.FromResult(new TgPublicChatProfile("Second description", 55, ["second"], null));
        var row = new OpenTgResearcherDesktop.Models.TgPublicChatSearchRow(new(1, "First", "durov", false, null));
        vm.SelectedPublicChat = row;
        var pending = vm.PublicProfileTask;
        vm.SelectedPublicChat = new(new(2, "Second", "second", false, null));
        await vm.PublicProfileTask;
        first.SetResult(new("Old description", 20, ["old"], null));
        await pending;
        Assert(vm.SelectedPublicChat.About == "Second description" && row.About.Length == 0 && vm.Errors.Count == 0,
            "Устаревшее описание попало в карточку или отмена показана ошибкой");
    }

    // Ошибка подробностей оставляет доступным источник и позволяет повторить запрос
    private static async Task GlobalProfileFailureCanRetryAsync()
    {
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!) { SearchScope = 1 };
        var client = App.BusinessLogicManager.ConnectClient;
        client.PublicProfile = (_, _) => throw new InvalidOperationException("Сведения недоступны");
        vm.SelectedPublicChat = new(new(1, "Channel", "durov", false, null));
        await vm.PublicProfileTask;
        Assert(vm.SelectedPublicChat.ProfileError == "Сведения недоступны" && vm.AddPublicChatCommand.CanExecute(null),
            "Ошибка профиля скрыта или блокирует добавление источника");
        client.PublicProfile = (_, _) => Task.FromResult(new TgPublicChatProfile("Restored", null, [], null));
        await vm.ReloadPublicProfileCommand.ExecuteAsync(null);
        Assert(vm.SelectedPublicChat.About == "Restored" && vm.SelectedPublicChat.ProfileError.Length == 0
            && vm.SelectedPublicChat.ParticipantsCount is null, "Повторный запрос не восстановил сведения или неизвестное число подменено нулём");
    }

    // Недоступное фото одной строки не прерывает загрузку остальных фотографий
    private static async Task GlobalPhotoFailurePreservesOtherResultsAsync()
    {
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!) { SearchScope = 1, FilterText = "telegram" };
        var client = App.BusinessLogicManager.ConnectClient;
        client.Ready = true;
        client.SearchPublicChats = (_, _) => Task.FromResult(new List<TgPublicChatSearchResult>
            { new(1, "First", "durov", false, null), new(2, "Second", "second", false, null) });
        client.PublicPhoto = (id, _) => id == 1 ? throw new InvalidOperationException("Фото недоступно") : Task.FromResult(new byte[] { 4 });
        await vm.SearchChatsCommand.ExecuteAsync(null);
        await vm.PublicPhotosTask;
        Assert(vm.PublicSearchRows[0].Photo is null && vm.PublicSearchRows[0].PhotoError.Length > 0
            && vm.PublicSearchRows[1].Photo?.Single() == 4 && vm.PublicChatError.Length == 0, "Ошибка фото уничтожила выдачу или остановила другие фото");
    }

    // Карточка справа должна заменять нижний предпросмотр и получать фото и описание
    private static Task GlobalSearchProfileHasPhotoAndRightPaneAsync()
    {
        var page = ReadMarkup("TgChatsPage.xaml");
        var pane = page.Descendants().Single(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "PublicSearchPreview");
        Assert((string?)pane.Attribute("Grid.Column") == "1", "Карточка результата остаётся под списком вместо правой панели");
        Assert(pane.Descendants().Any(x => (string?)x.Attribute("Text") == "{x:Bind ViewModel.SelectedPublicChat.About, Mode=OneWay}"),
            "В карточке нет описания Telegram");
        Assert(page.Descendants().Where(x => x.Name.LocalName == "PersonPicture").Count(x => x.Attribute("ProfilePicture") is not null) >= 2,
            "В списке и карточке не подключены фотографии Telegram");
        Assert(pane.Descendants().Any(x => x.Name.LocalName == "ScrollViewer"), "Большое описание не имеет независимой прокрутки");
        foreach (var language in new[] { "ru-RU", "en-US" })
        {
            using var stream = typeof(Program).Assembly.GetManifestResourceStream($"ChatResources.{language}.resw")!;
            var data = XDocument.Load(stream).Descendants("data").Where(x => ((string?)x.Attribute("name"))?.StartsWith("GlobalProfile") == true)
                .ToDictionary(x => (string)x.Attribute("name")!, x => x.Element("value")!.Value);
            foreach (var key in new[] { "GlobalProfileTitle.Text", "GlobalProfileInformation.Header", "GlobalProfileMembers", "GlobalProfileSubscribers",
                "GlobalProfileCountUnknown", "GlobalProfileOpenTelegram.Content", "GlobalProfileDiscussion.Content", "GlobalProfileMedia.Header",
                "GlobalProfilePhotos", "GlobalProfileVideos", "GlobalProfileDocuments", "GlobalProfileLinks", "GlobalProfileMusic",
                "GlobalProfileVoice", "GlobalProfileGifs", "GlobalProfilePolls" })
                Assert(data.TryGetValue(key, out var text) && text.Length > 0, $"Нет перевода {key}: {language}");
        }
        return Task.CompletedTask;
    }

    // Поиск по ссылке должен возвращать источник, оставляя сохранение отдельным действием
    private static async Task GlobalSearchDoesNotSaveBeforeConfirmationAsync()
    {
        using var fixture = PublicChatFixture();
        var results = await fixture.Connection.SearchPublicChatsAsync("https://t.me/durov");
        Assert(results.Count == 1 && results[0].UserName == "durov", "Поиск по ссылке не вернул нужный источник");
        using var repository = new TgEfSourceRepository();
        Assert((await repository.GetDtoAsync(x => x.Id == 1002003004)).Id == 0, "Поиск сохранил источник без команды добавления");
        Assert(((PublicChatConnection)fixture.Connection).HistoryCalls == 0, "Поиск начал чтение истории");
    }

    // Пользователи, приватные каналы и посторонние записи справочника не попадают в выдачу
    private static async Task GlobalSearchFiltersPeersAndFindsSavedSourceAsync()
    {
        using var fixture = PublicChatFixture();
        var client = (PublicChatConnection)fixture.Connection;
        var channel = client.Channel;
        using var repository = new TgEfSourceRepository();
        await repository.SaveAsync(new TgEfSourceDto { Uid = Guid.NewGuid(), Id = channel.id, Title = "Saved", Directory = "keep-settings" });
        var saved = await repository.GetDtoAsync(x => x.Id == channel.id);
        var group = new TL.Channel { id = 200, username = "public_group", title = "Group", flags = TL.Channel.Flags.megagroup };
        client.SearchReply = new()
        {
            my_results = [new TL.PeerChannel { channel_id = channel.id }],
            results = [new TL.PeerChannel { channel_id = channel.id }, new TL.PeerChannel { channel_id = group.id },
                new TL.PeerChannel { channel_id = 300 }, new TL.PeerUser { user_id = 10 }],
            chats = new() { [channel.id] = channel, [group.id] = group, [300] = new TL.Channel { id = 300, title = "Private" },
                [400] = new TL.Channel { id = 400, username = "not_a_result", title = "Not found" } },
            users = new()
        };
        var results = await client.SearchPublicChatsAsync("public");
        Assert(client.SearchQuery == "public" && results.Count == 2 && results.Select(x => x.Id).Distinct().Count() == 2,
            "Выдача содержит личные контакты, дубли или посторонние записи");
        Assert(results.Single(x => x.Id == channel.id).SourceUid == saved.Uid && results.Single(x => x.Id == group.id).IsGroup,
            "Не определены сохранённый источник или тип группы");
        Assert((await repository.GetDtoAsync(x => x.Id == channel.id)).Directory == "keep-settings", "Поиск изменил настройки");
        Assert((await repository.GetDtoAsync(x => x.Id == group.id)).Id == 0 && client.HistoryCalls == 0, "Поиск изменил хранилище или читал историю");
        await ExpectPublicChatErrorAsync(() => client.SearchPublicChatsAsync("ab"), "трёх символов");
        await ExpectPublicChatErrorAsync(() => client.SearchPublicChatsAsync("https://t.me/+invite"), "Укажите", ignoreCase: true);
    }

    // Области используют разные источники; фильтры Telegram не запускают повторный сетевой поиск
    private static async Task GlobalSearchScopesAndKindsAsync()
    {
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!) { FilterText = "First" };
        var calls = 0;
        App.BusinessLogicManager.ConnectClient.SearchPublicChats = (query, ct) =>
        {
            Assert(query == "telegram", "В сетевой поиск передан старый ввод");
            calls++;
            return Task.FromResult(new List<TgPublicChatSearchResult>
            {
                new(1, "Channel", "durov", false, null), new(2, "Group", "public_group", true, null)
            });
        };
        await vm.SearchChatsCommand.ExecuteAsync(null);
        Assert(calls == 0 && vm.RegistryRows.Single().Source.Id == _source.Id, "Локальный поиск обратился к Telegram или потерял строки БД");
        _state.IsOnlineProcessing = true;
        App.BusinessLogicManager.ConnectClient.Ready = true;
        vm.SearchScope = 1; vm.FilterText = "telegram";
        await vm.SearchChatsCommand.ExecuteAsync(null);
        Assert(calls == 1 && vm.PublicSearchRows.Count == 2 && vm.RegistryRows.Count == 1, "Глобальный поиск заблокирован парсингом или очистил реестр");
        vm.PublicSearchKind = 2;
        Assert(vm.PublicSearchRows.Single().IsGroup && calls == 1, "Фильтр групп неверен или повторно обращается к Telegram");
        vm.PublicSearchKind = 1;
        Assert(!vm.PublicSearchRows.Single().IsGroup && calls == 1, "Фильтр каналов неверен");
        vm.SearchScope = 0; vm.FilterText = "First";
        await vm.SearchChatsCommand.ExecuteAsync(null);
        Assert(vm.RegistryRows.Single().Source.Id == _source.Id && calls == 1, "Возврат к локальному поиску вызывает Telegram");
    }

    // Поздний ответ после ухода из глобального поиска не обновляет выбранный источник
    private static async Task GlobalSearchCancellationDiscardsResponseAsync()
    {
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!) { SearchScope = 1, FilterText = "telegram" };
        App.BusinessLogicManager.ConnectClient.Ready = true;
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var reply = new TaskCompletionSource<List<TgPublicChatSearchResult>>(TaskCreationOptions.RunContinuationsAsynchronously);
        App.BusinessLogicManager.ConnectClient.SearchPublicChats = (query, ct) => { started.SetResult(); return reply.Task; };
        var search = vm.SearchChatsCommand.ExecuteAsync(null);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert(vm.IsPublicSearchBusy, "Не отображается ожидание поиска");
        vm.SearchScope = 0;
        reply.SetResult([new(1, "Late response", "durov", false, null)]);
        await search;
        Assert(vm.PublicSearchRows.Count == 0 && vm.SelectedPublicChat is null && !vm.IsPublicSearchBusy && vm.Errors.Count == 0,
            "Отменённая выдача попала на страницу или отмена показана ошибкой");
    }

    // Неудачный новый поиск не оставляет доступным добавление результата прежнего запроса
    private static async Task GlobalSearchFailureClearsOldResultsAsync()
    {
        var vm = new TgChatsViewModel(_state, _settings, _navigation, null!) { SearchScope = 1, FilterText = "telegram" };
        App.BusinessLogicManager.ConnectClient.Ready = true;
        App.BusinessLogicManager.ConnectClient.SearchPublicChats = (_, _) =>
            Task.FromResult(new List<TgPublicChatSearchResult> { new(1, "Found", "durov", false, null) });
        await vm.SearchChatsCommand.ExecuteAsync(null);
        Assert(vm.HasSelectedPublicChat, "Успешная выдача не выбирает источник");
        App.BusinessLogicManager.ConnectClient.SearchPublicChats = (_, _) => throw new InvalidOperationException("Сеть недоступна");
        await vm.SearchChatsCommand.ExecuteAsync(null);
        Assert(vm.PublicChatError == "Сеть недоступна" && vm.PublicSearchRows.Count == 0 && !vm.AddPublicChatCommand.CanExecute(null),
            "Ошибка скрыта или доступен прежний результат");
    }

    // Уже сохранённый источник открывается без повторного сохранения и без подключения
    private static async Task GlobalSearchSavedSourceOpensOfflineAsync()
    {
        var navigation = new INavigationService();
        var vm = new TgChatsViewModel(_state, _settings, navigation, null!)
        {
            SelectedPublicChat = new(new TgPublicChatSearchResult(_source.Id, _source.Title, _source.UserName, false, _source.Uid))
        };
        App.BusinessLogicManager.ConnectClient.AddPublicChat = _ => throw new InvalidOperationException("Повторное добавление");
        await vm.AddPublicChatCommand.ExecuteAsync(null);
        Assert(navigation.Parameter is Guid uid && uid == _source.Uid && vm.Errors.Count == 0, "Сохранённый источник требует сети");
    }

    // Проверяем единственное поле ввода и перевод всех новых подписей, включая пустую выдачу
    private static Task GlobalSearchMarkupAndTranslationsAsync()
    {
        var page = ReadMarkup("TgChatsPage.xaml");
        var fields = page.Descendants().Where(x => x.Name.LocalName == "TextBox").ToArray();
        Assert(fields.Length == 1 && (string?)fields[0].Attribute("TextChanged") == "OnSearchTextChanged"
            && (string?)fields[0].Attribute("KeyDown") == "FieldFilter_KeyDown", "Страница не использует единое поле поиска");
        Assert(page.Descendants().Any(x => (string?)x.Attribute("Command") == "{x:Bind ViewModel.SearchChatsCommand}"), "Кнопка не выбирает область поиска");
        var results = page.Descendants().Single(x => (string?)x.Attribute("AutomationProperties.AutomationId") == "PublicSearchResults");
        Assert(results.Ancestors().Any(x => ((string?)x.Attribute("Visibility"))?.Contains("IsDisplaySensitiveData") == true),
            "Результаты обходят переключатель конфиденциальности");
        foreach (var language in new[] { "ru-RU", "en-US" })
        {
            using var stream = typeof(Program).Assembly.GetManifestResourceStream($"ChatResources.{language}.resw")!;
            var data = XDocument.Load(stream).Descendants("data").Where(x => ((string?)x.Attribute("name"))?.StartsWith("GlobalSearch") == true)
                .ToDictionary(x => (string)x.Attribute("name")!, x => x.Element("value")!.Value);
            foreach (var key in new[] { "GlobalSearchLocal.Text", "GlobalSearchTelegram.Text", "GlobalSearchField.PlaceholderText",
                "GlobalSearchChannels.Text", "GlobalSearchGroups.Text", "GlobalSearchHintLabel.Text", "GlobalSearchHint", "GlobalSearchCount",
                "GlobalSearchEmpty", "GlobalSearchChannel", "GlobalSearchGroup", "GlobalSearchSaved.Text", "GlobalSearchAdd", "GlobalSearchPreviewHint.Text" })
                Assert(data.TryGetValue(key, out var text) && text.Length > 0, $"Нет перевода {key}: {language}");
        }
        return Task.CompletedTask;
    }
}
