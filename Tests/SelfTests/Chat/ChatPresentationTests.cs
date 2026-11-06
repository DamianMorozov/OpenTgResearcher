namespace OpenTgResearcherDesktop.ChatSelfTests;

internal static partial class Program
{
    // Проверяем выбор участника, очистку досье и реальные результаты представления статистики
    private static IEnumerable<(string Name, Func<Task> Run)> GetPresentationTests() =>
    [
        ("ContentReopeningClosesPreview", ContentReopeningClosesPreviewAsync),
        ("MessageWithoutAuthorUsesChat", MessageWithoutAuthorUsesChatAsync),
        ("ImageBackdropClosesPreview", ImageBackdropClosesPreviewAsync),
        ("ParticipantsLoadOnEmptyOpening", ParticipantsLoadOnEmptyOpeningAsync),
        ("StatisticsCalculateOnEmptyOpening", StatisticsCalculateOnEmptyOpeningAsync),
        ("ParticipantsDossierSelection", ParticipantsDossierSelectionAsync),
        ("StatisticsSummaryAndShares", StatisticsSummaryAndSharesAsync),
        ("StatisticsRejectsReversedDates", StatisticsRejectsReversedDatesAsync),
        ("StatisticsUnexpectedCancellationShowsError", StatisticsUnexpectedCancellationShowsErrorAsync),
        ("StatisticsEmptyPeriodAndDateChange", StatisticsEmptyPeriodAndDateChangeAsync),
        ("ParticipantsSearchClearsDossier", ParticipantsSearchClearsDossierAsync),
        ("ModernChatPagesOwnScrolling", ModernChatPagesOwnScrollingAsync),
        ("ContentFeedAndProfileCards", ContentFeedAndProfileCardsAsync),
        ("ChatPrivacyCoversEmptyStates", ChatPrivacyCoversEmptyStatesAsync),
    ];

    // Новая страница не должна наследовать пустой непрозрачный просмотр изображения прежней страницы
    private static async Task ContentReopeningClosesPreviewAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        context.Messages.Add(new() { Uid = Guid.NewGuid(), SourceId = _source.Id, Id = 1, Message = "Visible message" });
        await context.SaveChangesAsync();
        var vm = new TgChatContentViewModel(_state, _settings, _navigation, null!, null!) { IsImageViewerVisible = true };
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(vm.MessageDtos.Count == 1 && !vm.IsImageViewerVisible, "Непустая лента закрыта пустым просмотром изображения");
    }

    // Нулевой ID означает сообщение от имени чата, остальные отправители сохраняют свой ID
    private static async Task MessageWithoutAuthorUsesChatAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        context.Messages.AddRange(new TgEfMessageEntity { Uid = Guid.NewGuid(), SourceId = _source.Id, Id = 1, UserId = 0, Message = "Channel post" },
            new TgEfMessageEntity { Uid = Guid.NewGuid(), SourceId = _source.Id, Id = 2, UserId = 20, Message = "User post" });
        await context.SaveChangesAsync();
        var vm = new TgChatContentViewModel(_state, _settings, _navigation, null!, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        var post = vm.MessageDtos.Single(x => x.UserId == 0);
        Assert(post.UserContact == _source.Title, "В заголовке сообщения нет названия чата вместо отсутствующего автора");
        var display = typeof(TgEfMessageDto).GetProperty("SenderDisplay");
        Assert(display?.GetValue(post) as string == _source.Title, "Сведения о сообщении показывают нулевой ID вместо чата");
        Assert(display?.GetValue(vm.MessageDtos.Single(x => x.UserId == 20)) as string == "20", "Изменён ID обычного отправителя");
    }

    // Вся подложка увеличенного изображения принимает нажатие для закрытия
    private static Task ImageBackdropClosesPreviewAsync()
    {
        var page = ReadMarkup("TgChatContentPage.xaml");
        Assert(page.Descendants().Any(x => x.Name.LocalName == "Border" && x.Attribute("Background") is not null
            && (string?)x.Attribute("Tapped") == "OnFullScreenImageTapped" && x.Descendants().Any(e => (string?)e.Attribute(
                XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml")) == "FullScreenImage")),
            "Нажатие по подложке не закрывает просмотр изображения");
        return Task.CompletedTask;
    }

    // Пустой каталог заполняется при открытии, повторное открытие сохраняет выбор и порцию
    private static async Task ParticipantsLoadOnEmptyOpeningAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        context.Users.Add(new() { Uid = Guid.NewGuid(), Id = 20, FirstName = "Member" });
        context.ChatUsers.Add(new() { Uid = Guid.NewGuid(), ChatId = _source.Id, UserId = 20 });
        await context.SaveChangesAsync();
        var vm = new TgChatParticipantsViewModel(_state, _settings, _navigation, null!, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(vm.UserDtos.Count == 1 && vm.SelectedUser?.Id == 20 && !vm.IsEmptyData, "Открытие оставило каталог пустым");
        var first = vm.SelectedUser;
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(ReferenceEquals(first, vm.SelectedUser) && vm.PageSkip == 1, "Повторное открытие перечитало непустой каталог");
        await vm.ClearParticipantsCommand.ExecuteAsync(null);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(vm.UserDtos.Count == 1, "Открытие очищенного каталога не выполнило загрузку");
    }

    // Открытие пустой статистики подключённого чата выполняет расчёт без повторения для готового результата
    private static async Task StatisticsCalculateOnEmptyOpeningAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        context.Users.Add(new() { Uid = Guid.NewGuid(), Id = 20, FirstName = "Author" });
        context.Messages.Add(new() { Uid = Guid.NewGuid(), SourceId = _source.Id, Id = 1, UserId = 20, Message = "Post", DtCreated = DateTime.UtcNow });
        await context.SaveChangesAsync();
        _state.IsOnlineReady = true;
        App.BusinessLogicManager.ConnectClient.Ready = true;
        var checks = 0;
        App.BusinessLogicManager.ConnectClient.CheckConnection = () => { checks++; return Task.FromResult(true); };
        var vm = new TgChatStatisticsViewModel(_state, _settings, _navigation, null!, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(vm.IsCalculated && vm.TotalMessages == 1 && vm.ActivityRows.Count == 1 && checks == 1, "Открытие не рассчитало статистику");
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(vm.TotalMessages == 1 && checks == 1, "Готовая статистика запрашивается повторно при каждом открытии");
        _state.IsOnlineReady = false;
        await vm.OnNavigatedToAsync(new() { Parameter = Guid.NewGuid() });
        Assert(!vm.IsCalculated && vm.ActivityRows.Count == 0 && checks == 1, "Неподключённый другой чат получил прежний результат или сетевой запрос");
    }

    // Первая загрузка выбирает участника, следующая порция сохраняет выбор, очистка удаляет досье
    private static async Task ParticipantsDossierSelectionAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        foreach (var id in new long[] { 20, 21, 22 })
        {
            context.Users.Add(new() { Uid = Guid.NewGuid(), Id = id, FirstName = "User" + id });
            context.ChatUsers.Add(new() { Uid = Guid.NewGuid(), ChatId = _source.Id, UserId = id });
        }
        await context.SaveChangesAsync();
        var vm = new TgChatParticipantsViewModel(_state, _settings, _navigation, null!, null!);
        var selected = vm.GetType().GetProperty("SelectedUser");
        Assert(selected is not null, "Участники не предоставляют выбранное досье");
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        await vm.LoadParticipantsCommand.ExecuteAsync(null);
        Assert(selected!.GetValue(vm) is TgEfUserDto first && first.Id == vm.UserDtos[0].Id, "Первая страница не выбрала участника");
        selected.SetValue(vm, vm.UserDtos[1]); var expected = vm.UserDtos[1].Id;
        await vm.LoadParticipantsCommand.ExecuteAsync(null);
        Assert(selected.GetValue(vm) is TgEfUserDto current && current.Id == expected && vm.UserDtos.Count == 3,
            "Догрузка потеряла выбранного участника или строки каталога");
        await vm.ClearParticipantsCommand.ExecuteAsync(null);
        Assert(selected.GetValue(vm) is null && vm.UserDtos.Count == 0 && vm.IsEmptyData, "Очистка оставила досье прежнего участника");
        await vm.OnNavigatedToAsync(new() { Parameter = Guid.NewGuid() });
        Assert(selected.GetValue(vm) is null, "Другой чат показывает прежнее досье");
    }

    // Итоги и доли вычисляются из отфильтрованных сообщений, включая автора без сообщений
    private static async Task StatisticsSummaryAndSharesAsync()
    {
        var context = TgGlobalTools.Container.Resolve<ITgEfContext>();
        foreach (var id in new long[] { 20, 21, 22 }) context.Users.Add(new() { Uid = Guid.NewGuid(), Id = id, FirstName = "User" + id });
        context.Messages.AddRange(
            new TgEfMessageEntity { Uid = Guid.NewGuid(), SourceId = _source.Id, Id = 1, UserId = 20, DtCreated = new(2026, 10, 5, 12, 0, 0) },
            new TgEfMessageEntity { Uid = Guid.NewGuid(), SourceId = _source.Id, Id = 2, UserId = 20, DtCreated = new(2026, 10, 5, 13, 0, 0) },
            new TgEfMessageEntity { Uid = Guid.NewGuid(), SourceId = _source.Id, Id = 3, UserId = 21, DtCreated = new(2026, 10, 5, 14, 0, 0) });
        await context.SaveChangesAsync();
        App.BusinessLogicManager.ConnectClient.Ready = true;
        App.BusinessLogicManager.ConnectClient.Participants.Add(new(new TL.User { id = 22, first_name = "User22" }));
        var vm = new TgChatStatisticsViewModel(_state, _settings, _navigation, null!, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        vm.ChatStatisticsDto.DtStart = new(2026, 10, 5, 0, 0, 0, TimeSpan.Zero);
        vm.ChatStatisticsDto.DtEnd = new(2026, 10, 5, 0, 0, 0, TimeSpan.Zero);
        await vm.CalcChatStatisticsCommand.ExecuteAsync(null);
        Assert(vm.GetType().GetProperty("TotalMessages")?.GetValue(vm) is int total && total == 3, "Нет итогового числа учтённых сообщений");
        Assert(vm.GetType().GetProperty("ActiveAuthorsCount")?.GetValue(vm) is int active && active == 2, "Неверно посчитаны писавшие авторы");
        var rows = ((System.Collections.IEnumerable)vm.GetType().GetProperty("ActivityRows")!.GetValue(vm)!).Cast<object>().ToArray();
        var shares = rows.Select(x => (double)x.GetType().GetProperty("Share")!.GetValue(x)!).ToArray();
        Assert(rows.Length == 3 && Math.Abs(shares.Sum() - 100) < .001 && shares[0] > shares[1] && shares[2] == 0,
            "Доли сообщений не соответствуют ранжированию или включают выдуманную активность");
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        Assert(vm.TotalMessages == 3 && vm.ActivityRows.Count == 3 && ReferenceEquals(vm.ActivityRows[0], rows[0]),
            "Повторное открытие потеряло готовую статистику или перестроило результат без запроса");
    }

    // Незатребованная отмена запроса остаётся видимой ошибкой, штатная отмена не создаёт результат
    private static async Task StatisticsUnexpectedCancellationShowsErrorAsync()
    {
        var vm = new TgChatStatisticsViewModel(_state, _settings, _navigation, null!, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        App.BusinessLogicManager.ConnectClient.CheckConnection = () => throw new OperationCanceledException("Test transport cancellation");
        await vm.CalcChatStatisticsCommand.ExecuteAsync(null);
        Assert(vm.HasStatisticsError && !vm.IsCalculated, "Незатребованная отмена статистики скрыла сбой запроса");
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        App.BusinessLogicManager.ConnectClient.CheckConnection = () => throw new OperationCanceledException(cancelled.Token);
        await vm.CalcChatStatisticsCommand.ExecuteAsync(null);
        Assert(!vm.HasStatisticsError && !vm.IsCalculated && vm.ActivityRows.Count == 0, "Штатная отмена сохранила ошибку или ложный результат");
    }

    // Пустой период даёт нулевые доли без NaN, смена даты убирает устаревший результат
    private static async Task StatisticsEmptyPeriodAndDateChangeAsync()
    {
        App.BusinessLogicManager.ConnectClient.Ready = true;
        App.BusinessLogicManager.ConnectClient.Participants.Add(new(new TL.User { id = 22, first_name = "Silent" }));
        var vm = new TgChatStatisticsViewModel(_state, _settings, _navigation, null!, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        vm.ChatStatisticsDto.DtStart = new(2026, 10, 5, 0, 0, 0, TimeSpan.Zero);
        vm.ChatStatisticsDto.DtEnd = vm.ChatStatisticsDto.DtStart;
        await vm.CalcChatStatisticsCommand.ExecuteAsync(null);
        Assert(vm.IsCalculated && vm.TotalMessages == 0 && vm.ActivityRows.Count == 1
            && vm.ActivityRows[0].Share == 0 && vm.ActivityRows[0].Relative == 0, "Пустой период содержит выдуманные доли или NaN");
        Assert(vm.GetType().GetProperty("HasNoMessages")?.GetValue(vm) is true, "Нет объяснения успешно рассчитанного пустого периода");
        Assert(vm.GetType().GetProperty("IsPeriodExpanded")?.GetValue(vm) is false, "Период не освободил место для результатов расчёта");
        vm.ChatStatisticsDto.DtEnd = vm.ChatStatisticsDto.DtEnd!.Value.AddDays(1);
        Assert(!vm.IsCalculated && vm.ActivityRows.Count == 0, "Новый период показывает итоги предыдущего расчёта");
    }

    // Настоящие обработчики поиска очищают выбор и корректно завершают обновление списка
    private static async Task ParticipantsSearchClearsDossierAsync()
    {
        var vm = new TgChatParticipantsViewModel(_state, _settings, _navigation, null!, null!);
        vm.UserDtos.Add(new() { Id = 20 });
        vm.SelectedUser = vm.UserDtos[0];
        typeof(TgChatParticipantsViewModel).GetMethod("ItemsClearCore", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(vm, null);
        Assert(vm.UserDtos.Count == 0 && vm.SelectedUser is null && vm.IsEmptyData, "Поиск сохранил прежнее досье");
        vm.UserDtos.Add(new() { Id = 21 });
        await (Task)typeof(TgChatParticipantsViewModel).GetMethod("AfterDataUpdateCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(vm, null)!;
        Assert(!vm.IsEmptyData, "Результаты поиска остались скрыты пустым состоянием");
    }

    // Перевёрнутый период объясняется на странице до сетевых запросов и расчёта
    private static async Task StatisticsRejectsReversedDatesAsync()
    {
        var vm = new TgChatStatisticsViewModel(_state, _settings, _navigation, null!, null!);
        await vm.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        var checkedConnection = false;
        App.BusinessLogicManager.ConnectClient.CheckConnection = () => { checkedConnection = true; return Task.FromResult(true); };
        vm.ChatStatisticsDto.DtStart = new(2026, 10, 6, 0, 0, 0, TimeSpan.Zero);
        vm.ChatStatisticsDto.DtEnd = new(2026, 10, 5, 0, 0, 0, TimeSpan.Zero);
        await vm.CalcChatStatisticsCommand.ExecuteAsync(null);
        Assert(!checkedConnection && vm.GetType().GetProperty("StatisticsError")?.GetValue(vm) is string { Length: > 0 },
            "Неверный период не объяснён пользователю или начал сетевой запрос");
    }

    // Каталог и ранжирование измеряются в конечной области без внешней прокрутки списков
    private static Task ModernChatPagesOwnScrollingAsync()
    {
        foreach (var file in new[] { "TgChatContentPage.xaml", "TgChatParticipantsPage.xaml", "TgChatStatisticsPage.xaml" })
        {
            var page = ReadMarkup(file);
            var lists = page.Descendants().Where(x => x.Name.LocalName == "ListView").ToArray();
            Assert(lists.Length > 0 && lists.All(x => !x.Ancestors().Any(p => p.Name.LocalName is "ScrollView" or "ScrollViewer")),
                "Список вложен во внешнюю прокрутку: " + file);
            Assert(!page.Descendants().Attributes("Margin").Any(a => a.Value.Contains('-')), "Остались отрицательные отступы: " + file);
        }
        return Task.CompletedTask;
    }

    // Пустое состояние скрывается вместе с данными и не накладывается на сообщение конфиденциальности
    private static Task ChatPrivacyCoversEmptyStatesAsync()
    {
        foreach (var file in new[] { "TgChatContentPage.xaml", "TgChatMyMessagesPage.xaml", "TgChatParticipantsPage.xaml", "TgChatStatisticsPage.xaml" })
        {
            var page = ReadMarkup(file);
            var labels = page.Descendants().Where(x => x.Attributes().Any(a => a.Name.LocalName == "Uids.Uid"
                && a.Value is "FieldEmptyData" or "ChatParticipantsEmpty" or "ChatStatisticsEmpty"));
            Assert(labels.Any() && labels.All(x => x.Ancestors().Any(p => ((string?)p.Attribute("Visibility"))
                == "{x:Bind ViewModel.IsDisplaySensitiveData, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}")),
                "Пустое состояние перекрывает сообщение конфиденциальности: " + file);
        }
        return Task.CompletedTask;
    }

    // Лента сохраняет изображения и обложки, профиль использует общий шаблон карточки
    private static Task ContentFeedAndProfileCardsAsync()
    {
        var page = ReadMarkup("TgChatContentPage.xaml");
        Assert(page.Descendants().Any(x => x.Name.LocalName == "Expander"), "Метаданные ленты не вынесены в раскрываемые сведения");
        foreach (var id in new[] { "MessageImage", "MessageThumbnail", "ClearChatContent" })
            Assert(page.Descendants().Any(x => (string?)x.Attribute("AutomationProperties.AutomationId") == id), "Лента потеряла действие или медиа: " + id);
        var profile = ReadMarkup("TgChatMyMessagesPage.xaml");
        Assert(profile.Descendants().Attributes().Any(x => x.Value == "{StaticResource ChatUserDossierTemplate}"), "Профиль не использует карточку пользователя");
        return Task.CompletedTask;
    }
    // Проверяем исходную модель ленты на полной пользовательской БД, открытой только для чтения
    private static async Task<int> InspectContentAsync(string database, long chatId)
    {
        try
        {
            TgGlobalTools.SetAppType(TgEnumAppType.Test);
            var connection = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
                { DataSource = database, Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly };
            await using var context = new TestContext(new DbContextOptionsBuilder().UseSqlite(connection.ToString()).Options);
            var builder = new ContainerBuilder();
            builder.RegisterInstance(context).As<ITgEfContext>().ExternallyOwned();
            builder.RegisterAssemblyTypes(typeof(TgEfSourceRepository).Assembly).Where(x => x.Name.EndsWith("Repository")).AsImplementedInterfaces();
            using var container = builder.Build();
            TgGlobalTools.Container = container;
            using var sources = new TgEfSourceRepository();
            using var messages = new TgEfMessageRepository();
            using var users = new TgEfUserRepository();
            App.BusinessLogicManager.StorageManager.SourceRepository = sources;
            App.BusinessLogicManager.StorageManager.MessageRepository = messages;
            App.BusinessLogicManager.StorageManager.UserRepository = users;
            var source = await sources.GetDtoAsync(x => x.Id == chatId);
            var vm = new TgChatContentViewModel(_state, _settings, _navigation, null!, null!);
            await vm.OnNavigatedToAsync(new() { Parameter = source.Uid });
            if (vm.Errors.Count > 0) throw new AggregateException(vm.Errors);
            var expected = await messages.GetListDtosAsync(take: 100, skip: 0, x => x.SourceId == chatId, x => x.Id, isOrderDesc: true);
            Assert(expected.Count > 0 && vm.MessageDtos.Select(x => x.Id).SequenceEqual(expected.Select(x => x.Id).Reverse()),
                "Лента не показала последнюю порцию сообщений выбранного чата");
            Console.WriteLine($"PASS: read-only content; chat={chatId}; loaded={vm.MessageDtos.Count}; total={await messages.GetCountAsync(x => x.SourceId == chatId)}");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

}
