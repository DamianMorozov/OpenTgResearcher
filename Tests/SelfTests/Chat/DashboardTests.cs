namespace OpenTgResearcherDesktop.ChatSelfTests;

internal static partial class Program
{
    // Прерывание парсинга завершает команду без ошибки и перечитывает сохранённую позицию чата
    private static async Task DashboardStopIsNotFailureAsync()
    {
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        App.BusinessLogicManager.ConnectClient.Ready = true;
        using var stop = new CancellationTokenSource(); var calls = 0;
        App.BusinessLogicManager.ConnectClient.Parse = async settings =>
        {
            calls++;
            var saved = await App.BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == settings.SourceVm.Dto.Uid);
            saved.FirstId = 42;
            await App.BusinessLogicManager.StorageManager.SourceRepository.SaveAsync(saved);
            stop.Cancel();
            await Task.FromCanceled(stop.Token);
        };
        await Chat.StartUpdateOnlineCommand.ExecuteAsync(null);
        Assert(calls == 1 && Chat.Errors.Count == 0 && Chat.Dto.FirstId == 42,
            "Прерывание показано как исключение или потеряло сохранённую позицию чата");
    }

    // Отмена без запроса пользователя остаётся диагностируемой ошибкой, а не успешной остановкой
    private static async Task DashboardUnexpectedCancellationIsFailureAsync()
    {
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        App.BusinessLogicManager.ConnectClient.Ready = true;
        App.BusinessLogicManager.ConnectClient.Parse = _ => Task.FromException(new OperationCanceledException("Тестовый сбой транспорта"));
        await Chat.StartUpdateOnlineCommand.ExecuteAsync(null);
        Assert(Chat.Errors.Count == 1 && Chat.Errors[0] is OperationCanceledException, "Незапрошенная отмена скрыта как штатное прерывание");
    }

    // Ошибка ручного обновления должна попасть в журнал и интерфейс без аварийного завершения команды
    private static async Task DashboardDetailsFailureAsync()
    {
        await Download.OnNavigatedToAsync(new() { Parameter = _source });
        App.BusinessLogicManager.ConnectClient.Ready = true;
        App.BusinessLogicManager.ConnectClient.ReadDetails = () => Task.FromException<TgChatDetailsDto>(new IOException("Тестовый обрыв сети"));
        await Download.LoadDetailsCommand.ExecuteAsync(null);
        Assert(Download.Errors.Count == 1 && Download.Errors[0] is IOException && !Download.HasOnlineDetails,
            "Ошибка обновления скрыта или ошибочно показаны загруженные сведения");
        Assert(typeof(TgChatDownloadViewModel).GetProperty("DetailsError")?.GetValue(Download) is string { Length: > 0 },
            "В пульте не показана причина недоступных сведений");
    }

    // Пульт разрешает запуск другого чата и блокирует повторный запуск текущего
    private static async Task DashboardStartAvailabilityAsync()
    {
        await Download.OnNavigatedToAsync(new() { Parameter = _source });
        var available = typeof(TgChatDownloadViewModel).GetProperty("CanStartParsing");
        Assert(available is not null, "Кнопка запуска не учитывает общую сетевую загрузку");
        _state.IsOnlineReady = true;
        Download.RefreshActivity();
        Assert((bool)available!.GetValue(Download)!, "Подключённый чат не разрешает запуск");
        _state.IsOnlineProcessing = true;
        Download.RefreshActivity();
        Assert((bool)available.GetValue(Download)!, "Сетевая операция другого чата заблокировала запуск");
        var uid = Guid.NewGuid();
        _state.NetworkActivity.Start(uid, TgNetworkOperationKind.Parsing);
        _state.NetworkActivity.SetChat(uid, _source.Id, _source.Title);
        Download.RefreshActivity();
        Assert(!(bool)available.GetValue(Download)!, "Пульт разрешил повторный парсинг текущего чата");
        _state.IsOnlineProcessing = false; _state.IsOnlineReady = false;
        Download.RefreshActivity();
        Assert(!(bool)available.GetValue(Download)!, "Отключённый Telegram разрешил запуск");
    }

    // Обзор не обращается к Telegram до запроса сведений и очищает их при выборе другого чата
    private static async Task DashboardDetailsAndDiscussionAsync()
    {
        await Download.OnNavigatedToAsync(new() { Parameter = _source });
        Assert(Download.OnlineLoads == 0 && !Download.HasOnlineDetails, "Открытие обзора неожиданно запросило Telegram");
        App.BusinessLogicManager.ConnectClient.Ready = true;
        await Download.LoadDetailsCommand.ExecuteAsync(null);
        Assert(Download.HasOnlineDetails && Download.ChatDetailsDto.Title == "Online", "Запрос не отобразил онлайн-сведения чата");
        var discussion = new TgEfSourceDto { Id = 200, Uid = Guid.NewGuid() };
        Chat.DiscussionDto = discussion;
        Download.RefreshActivity();
        Download.OpenDiscussionCommand.Execute(null);
        Assert(Download.HasDiscussion && Equals(_navigation.Parameter, discussion.Uid), "Недоступен переход к обсуждению выбранного чата");
        await Download.OnNavigatedToAsync(new() { Parameter = new TgEfSourceDto { Id = 300, Uid = Guid.NewGuid() } });
        Assert(!Download.HasOnlineDetails && Download.ChatDetailsDto.Title == string.Empty, "Сведения прежнего чата остались в новом");
    }

    // Локальная навигация содержимого сохраняет доступ к профилю текущего пользователя
    private static async Task DashboardContentNavigationAsync()
    {
        await Chat.OnNavigatedToAsync(new() { Parameter = _source.Uid });
        var page = new TgChatPage();
        page.Select(new() { Tag = nameof(TgChatContentPage) });
        Assert(page.ChatDetailsFrame.Navigations == 1, "Первая вкладка содержимого открылась дважды и повторно запустила чтение SQLite");
        var method = typeof(TgChatPage).GetMethod("ContentViewSelector_SelectionChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert(method is not null, "Из содержимого недоступны прежние «Мои сообщения»");
        var selector = new SelectorBar { SelectedItem = new() { Tag = nameof(TgChatMyMessagesPage) } };
        method!.Invoke(page, [selector, null]);
        Assert(page.ChatDetailsFrame.PageType == typeof(TgChatMyMessagesPage) && Equals(page.ChatDetailsFrame.Parameter, _source.Uid),
            "Локальная навигация открыла другой чат или другую страницу");
        selector.SelectedItem = new() { Tag = nameof(TgChatContentPage) };
        method.Invoke(page, [selector, null]);
        Assert(page.ChatDetailsFrame.PageType == typeof(TgChatContentPage), "Не удалось вернуться к содержимому чата");
    }

    // Пауза и прерывание из пульта относятся к выбранному чату
    private static async Task DashboardControlsSelectedChatAsync()
    {
        await Download.OnNavigatedToAsync(new() { Parameter = _source });
        var current = Guid.NewGuid(); var other = Guid.NewGuid();
        var activity = _state.NetworkActivity;
        activity.Start(current, TgNetworkOperationKind.Parsing); activity.SetChat(current, _source.Id, "First");
        activity.Start(other, TgNetworkOperationKind.Parsing); activity.SetChat(other, _source.Id + 1, "Other");
        var canceled = 0; var otherCanceled = 0;
        activity.RegisterCancellation(current, () => canceled++);
        activity.RegisterCancellation(other, () => otherCanceled++);
        var pause = typeof(TgChatDownloadViewModel).GetProperty("PauseParsingCommand")?.GetValue(Download) as IRelayCommand;
        Assert(pause is not null, "В пульте отсутствует команда паузы");
        typeof(TgChatDownloadViewModel).GetMethod("RefreshActivity")!.Invoke(Download, null);
        pause!.Execute(null);
        Assert(activity.Operations.Single(x => x.Id == current).IsPaused && !activity.Operations.Single(x => x.Id == other).IsPaused,
            "Пауза затронула чужой чат или не остановила выбранный");
        pause.Execute(null);
        Assert(!activity.Operations.Single(x => x.Id == current).IsPaused, "Продолжение не сняло паузу");
        var stop = typeof(TgChatDownloadViewModel).GetProperty("StopParsingCommand")?.GetValue(Download) as IAsyncRelayCommand;
        Assert(stop is not null, "В пульте отсутствует команда прерывания");
        await stop!.ExecuteAsync(null);
        Assert(canceled == 1 && otherCanceled == 0, "Прерывание отменило чужой чат или не отменило выбранный");
    }

    // Основная навигация пульта объединяет прежние настройки, скачивание и сведения в обзор
    private static Task DashboardTabsAsync()
    {
        var tabs = ReadMarkup("TgChatPage.xaml").Descendants().Single(x => (string?)x.Attribute(XName.Get("Name",
            "http://schemas.microsoft.com/winfx/2006/xaml")) == "ChatDetailsSelectorBar").Elements().Select(x => (string?)x.Attribute("Tag")).ToArray();
        Assert(tabs.SequenceEqual(new[] { "TgChatDownloadPage", "TgChatContentPage", "TgChatParticipantsPage", "TgChatStatisticsPage" }),
            "Пульт должен содержать четыре вкладки: обзор, содержимое, участники и статистика");
        return Task.CompletedTask;
    }

    // Семь основных опций, дополнительные параметры и действия доступны внутри единого обзора
    private static Task DashboardOverviewAsync()
    {
        var page = ReadMarkup("TgChatDownloadPage.xaml");
        foreach (var option in _resetOptions.Concat(_otherOptions))
            Assert(page.Descendants().Any(x => x.Name.LocalName == "ToggleSwitch" && ((string?)x.Attribute("IsOn"))?.Contains(option) == true),
                $"Обзор потерял настройку {option}");
        Assert(page.Descendants().Any(x => x.Name.LocalName == "Expander" && (string?)x.Attribute("AutomationProperties.AutomationId") == "ChatDetails"),
            "В обзоре нет раскрываемых сведений о чате");
        foreach (var command in new[] { "StartUpdateOnlineCommand", "ResetChatSettingsCommand", "SaveChatSettingsCommand", "OpenNetworkActivityCommand" })
            Assert(page.Descendants().Any(x => ((string?)x.Attribute("Command"))?.Contains(command) == true), $"Из обзора недоступно действие {command}");
        return Task.CompletedTask;
    }

    // При возврате к обзору читаем живой снимок только выбранного чата, сохраняя общий черновик
    private static async Task DashboardRestoresActivityAsync()
    {
        var parsing = Guid.NewGuid(); var download = Guid.NewGuid(); var other = Guid.NewGuid();
        var activity = _state.NetworkActivity;
        activity.Start(parsing, TgNetworkOperationKind.Parsing); activity.SetChat(parsing, _source.Id, "First");
        activity.ReportParsing(parsing, "First", 40, 100); activity.ReportParsingRequest(parsing, 42, 100);
        activity.RegisterCancellation(parsing, () => { });
        activity.Start(download, TgNetworkOperationKind.Download); activity.SetChat(download, _source.Id, "First");
        activity.ReportTransfer(download, "first.zip", 50, 200);
        activity.Start(other, TgNetworkOperationKind.Download); activity.SetChat(other, _source.Id + 1, "Other");
        activity.ReportTransfer(other, "other.zip", 90, 100);
        await Download.OnNavigatedToAsync(new() { Parameter = _source });
        var refresh = typeof(TgChatDownloadViewModel).GetMethod("RefreshActivity");
        Assert(refresh is not null, "Обзор не восстанавливает снимок работающего парсинга");
        refresh!.Invoke(Download, null);
        Assert(Download.ChatProgressMessage == $"{40d:00.00} %", "Процент парсинга потерян при открытии обзора");
        Assert((string?)typeof(TgChatDownloadViewModel).GetProperty("CurrentFile")?.GetValue(Download) == "first.zip",
            "Обзор потерял текущий файл или показывает скачивание другого чата");
        Assert(Download.FileProgress == 25 && Download.CurrentTransfer.Contains("25") && Download.ChatProgressInfo == "Чтение ID 42 из 100",
            "Обзор перепутал процент файла, объём загрузки и текущий запрашиваемый ID");
        _state.IsDisplaySensitiveData = false;
        Download.RefreshActivity();
        Assert(Download.CurrentFile == Download.SensitiveData && !Download.CurrentFile.Contains("first.zip"),
            "Название текущего файла осталось открытым после скрытия конфиденциальных данных");
        _state.IsDisplaySensitiveData = true;
        Download.RefreshActivity();
        Assert(ReferenceEquals(Download.DownloadDto, _source), "Обзор заменил общий черновик");
        _source.Directory = "edited/downloads";
        await Download.OnNavigatedToAsync(new() { Parameter = _source });
        Assert(Download.DownloadDto.Directory == "edited/downloads" && Download.ChatProgressMessage == $"{40d:00.00} %",
            "Возврат к обзору потерял правки или прогресс");
    }
}
