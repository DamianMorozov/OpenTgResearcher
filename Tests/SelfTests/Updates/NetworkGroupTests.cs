namespace OpenTgResearcherDesktop.UpdateSelfTests;

internal static partial class Program
{
    // Все состояния остаются в одной группе, отмена не теряет прогресс и не сбрасывает раскрытие
    private static async Task NetworkUnifiedActivityAsync()
    {
        using var state = new LoadStateService(); using var cancel = new CancellationTokenSource();
        var vm = new TgNetworkActivityViewModel(state, new(), new(), null!);
        var parsing = AddChatOperation(state, TgNetworkOperationKind.Parsing, 101, "Первый чат");
        state.NetworkActivity.ReportParsing(parsing, "Первый чат", 30, 100);
        state.NetworkActivity.RegisterCancellation(parsing, cancel.Cancel);
        var file = AddChatOperation(state, TgNetworkOperationKind.Download, 101, "Первый чат");
        state.NetworkActivity.ReportTransfer(file, "clip.mp4", 25, 100);
        var completed = AddChatOperation(state, TgNetworkOperationKind.Download, 101, "Первый чат");
        state.NetworkActivity.Finish(completed, TgNetworkOperationState.Completed);
        var failed = AddChatOperation(state, TgNetworkOperationKind.Download, 101, "Первый чат");
        state.NetworkActivity.Finish(failed, TgNetworkOperationState.Failed);
        var other = AddChatOperation(state, TgNetworkOperationKind.Download, 202, "Второй чат");
        await vm.OnNavigatedToAsync(null);
        var group = ChatGroups(vm).Single(x => ChatId(x) == 101);
        group.GetType().GetProperty("IsExpanded")!.SetValue(group, true);
        state.NetworkActivity.CancelParsing(); vm.Refresh();
        Assert(GroupRows(group).Any(x => x.State.Contains("Остановка")), "Запрос отмены пропал из активности");
        state.NetworkActivity.Finish(parsing, TgNetworkOperationState.Canceled); vm.Refresh();
        Assert(ReferenceEquals(group, ChatGroups(vm).Single(x => ChatId(x) == 101)) && GroupRows(group).Length == 4,
            "Отменённая операция исчезла из единого блока активности");
        Assert(GroupRows(group).Any(x => x.State == "Отменено" && x.Progress == 30)
            && GroupRows(group).Any(x => x.State == "Завершено") && GroupRows(group).Any(x => x.State.Contains("Ошибка")),
            "Группа потеряла отмену, завершение, ошибку или прогресс");
        state.NetworkActivity.Finish(file, TgNetworkOperationState.Canceled); vm.Refresh();
        Assert(ChatGroups(vm).Length == 2 && (bool)group.GetType().GetProperty("IsExpanded")!.GetValue(group)!,
            "Завершение отмены удалило или свернуло группу");
        state.NetworkActivity.Finish(other, TgNetworkOperationState.Completed); vm.Refresh();
        Assert(vm.IsActiveEmpty && !vm.IsActivityEmpty && ChatGroups(vm).Length == 2, "Отсутствие активных действий скрыло историю");
        var next = AddChatOperation(state, TgNetworkOperationKind.Download, 202, "Второй чат"); vm.Refresh();
        var canceledFile = GroupRows(group).Single(x => x.Detail == "clip.mp4");
        Assert(canceledFile.Progress == 25, "Отменённый файл потерял процент");
        vm.Refresh(); Assert(ReferenceEquals(canceledFile, GroupRows(group).Single(x => x.Detail == "clip.mp4")), "Таймер пересоздал неизменную строку");
        state.SetIsDisplaySensitiveData(isDisplaySensitiveData: false); vm.Refresh();
        Assert(GroupTitle(group) == state.SensitiveData && GroupRows(group).All(x => !x.Caption.Contains("clip.mp4")), "Отмена раскрыла личные данные");
        vm.ClearHistoryCommand.Execute(null);
        Assert(ChatGroups(vm).Length == 1 && vm.Active.Count == 1 && state.NetworkActivity.Operations.Single().Id == next,
            "Очистка потеряла активный чат или оставила завершённые операции");
    }

    // Сопоставляем операции по ID чата, включая скачивания, завершения и одинаковые названия разных чатов
    private static async Task NetworkChatGroupsAsync()
    {
        using var state = new LoadStateService();
        var vm = new TgNetworkActivityViewModel(state, new(), new(), null!);
        var parsing = AddChatOperation(state, TgNetworkOperationKind.Parsing, 101, "Общий заголовок");
        var file = AddChatOperation(state, TgNetworkOperationKind.Download, 101, "Общий заголовок");
        var other = AddChatOperation(state, TgNetworkOperationKind.Download, 202, "Общий заголовок");
        state.NetworkActivity.ReportTransfer(file, "first.mp4", 25, 100);
        state.NetworkActivity.ReportTransfer(other, "second.pdf", 10, 100);
        state.NetworkActivity.Finish(file, TgNetworkOperationState.Completed);
        var connection = Guid.NewGuid(); state.NetworkActivity.Start(connection, TgNetworkOperationKind.Connection);
        await vm.OnNavigatedToAsync(null);
        var groups = ChatGroups(vm);
        Assert(groups.Length == 3, "Чаты с одинаковыми названиями или общие операции объединены неправильно");
        var chat = groups.Single(x => ChatId(x) == 101);
        var rows = GroupRows(chat);
        Assert(rows.Length == 2 && rows.Any(x => x.Detail == "first.mp4" && x.State == "Завершено") && rows.Any(x => x.Title == "Парсинг чата"),
            "Группа не содержит всю активность чата, включая завершённый файл");
        Assert(GroupRows(groups.Single(x => ChatId(x) == 202)).Single().Detail == "second.pdf", "Файл попал в чужой чат");
        Assert(GroupRows(groups.Single(x => ChatId(x) is null)).Single().Title == "Подключение к Telegram", "Потеряны общие операции");
        Assert((bool)chat.GetType().GetProperty("IsExpanded")!.GetValue(chat)! == false, "Чат должен раскрываться по нажатию");
        Assert(parsing != file, "Проверка требует независимых операций");
    }

    // Прогресс, новые файлы, завершение и очистка истории не сбрасывают раскрытую группу
    private static async Task NetworkGroupRefreshAsync()
    {
        using var state = new LoadStateService();
        var vm = new TgNetworkActivityViewModel(state, new(), new(), null!);
        var parsing = AddChatOperation(state, TgNetworkOperationKind.Parsing, 101, "Первый чат");
        await vm.OnNavigatedToAsync(null);
        var group = ChatGroups(vm).Single();
        var expanded = group.GetType().GetProperty("IsExpanded")!; expanded.SetValue(group, true);
        var file = AddChatOperation(state, TgNetworkOperationKind.Download, 101, "Первый чат");
        state.NetworkActivity.ReportTransfer(file, "clip.mp4", 75, 100); vm.Refresh();
        Assert(ReferenceEquals(group, ChatGroups(vm).Single()) && (bool)expanded.GetValue(group)!, "Таймер сбросил раскрытие чата");
        Assert(GroupRows(group).Any(x => x.Detail == "clip.mp4" && x.Progress == 75), "Прогресс файла не обновился внутри группы");
        state.NetworkActivity.Finish(file, TgNetworkOperationState.Completed);
        state.NetworkActivity.Finish(parsing, TgNetworkOperationState.Completed); vm.Refresh();
        Assert(ReferenceEquals(group, ChatGroups(vm).Single()) && (bool)expanded.GetValue(group)!, "Завершение свернуло чат");
        Assert(GroupRows(group).Length == 2 && GroupRows(group).All(x => x.State == "Завершено"), "Завершённые действия исчезли из чата");
        var next = AddChatOperation(state, TgNetworkOperationKind.Parsing, 101, "Первый чат"); vm.Refresh();
        vm.ClearHistoryCommand.Execute(null);
        Assert(ReferenceEquals(group, ChatGroups(vm).Single()) && GroupRows(group).Single().Title == "Парсинг чата", "Очистка потеряла активный чат");
        state.NetworkActivity.Finish(next, TgNetworkOperationState.Completed); vm.Refresh(); vm.ClearHistoryCommand.Execute(null);
        Assert(ChatGroups(vm).Length == 0, "Пустой чат остался после очистки");
    }

    // Скрываем названия и переводим заголовки, сохраняя связь операции с чатом
    private static async Task NetworkGroupPrivacyAsync()
    {
        using var state = new LoadStateService();
        var vm = new TgNetworkActivityViewModel(state, new(), new(), null!);
        var uid = AddChatOperation(state, TgNetworkOperationKind.Download, 101, "Личный чат");
        state.NetworkActivity.ReportTransfer(uid, "private.mp4", 50, 100);
        var connection = Guid.NewGuid(); state.NetworkActivity.Start(connection, TgNetworkOperationKind.Connection);
        await vm.OnNavigatedToAsync(null);
        var group = ChatGroups(vm).Single(x => ChatId(x) == 101);
        Assert(GroupTitle(group) == "Личный чат", "Нет заголовка чата");
        state.SetIsDisplaySensitiveData(isDisplaySensitiveData: false); vm.Refresh();
        Assert(GroupTitle(group) == state.SensitiveData && GroupRows(group).Single().Detail == state.SensitiveData, "Группа раскрывает личные данные");
        Localizer.Language = "en-US"; vm.Refresh();
        Assert(GroupTitle(ChatGroups(vm).Single(x => ChatId(x) is null)) == "General operations", "Общие операции не переведены");
        Assert(((string)group.GetType().GetProperty("Summary")!.GetValue(group)!).Contains("Running: 1"), "Сводка чата не переведена");
        state.SetIsDisplaySensitiveData(isDisplaySensitiveData: true); vm.Refresh();
        Assert(ReferenceEquals(group, ChatGroups(vm).Single(x => ChatId(x) == 101)) && GroupTitle(group) == "Личный чат", "Переключение данных потеряло чат");
    }

    // Через настоящий сервис создаём операцию и задаём принадлежность чату до первого прогресса
    private static Guid AddChatOperation(LoadStateService state, TgNetworkOperationKind kind, long chatId, string title)
    {
        var uid = Guid.NewGuid(); state.NetworkActivity.Start(uid, kind);
        var method = state.NetworkActivity.GetType().GetMethod("SetChat") ?? throw new InvalidOperationException("Нет связи сетевой операции с чатом");
        method.Invoke(state.NetworkActivity, [uid, chatId, title]);
        return uid;
    }

    // Читаем фактические группы исходной модели, отсутствие групп считается ошибкой
    private static object[] ChatGroups(TgNetworkActivityViewModel vm) =>
        ((System.Collections.IEnumerable)(vm.GetType().GetProperty("ChatGroups")?.GetValue(vm)
            ?? throw new InvalidOperationException("Нет раскрывающихся групп чатов"))).Cast<object>().ToArray();

    // Получаем идентификатор, не зависящий от названия и скрытия данных
    private static long? ChatId(object group) => (long?)group.GetType().GetProperty("ChatId")!.GetValue(group);

    // Получаем фактические строки, которые отображаются при раскрытии группы
    private static TgNetworkOperationModel[] GroupRows(object group) =>
        ((System.Collections.IEnumerable)group.GetType().GetProperty("Operations")!.GetValue(group)!).Cast<TgNetworkOperationModel>().ToArray();

    // Читаем отображаемый заголовок группы
    private static string GroupTitle(object group) => (string)group.GetType().GetProperty("Title")!.GetValue(group)!;
}
