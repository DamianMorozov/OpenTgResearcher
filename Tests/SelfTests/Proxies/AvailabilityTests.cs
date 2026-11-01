namespace OpenTgResearcherDesktop.ProxySelfTests;

internal static partial class Program
{
    // Отменённая проверка не должна открывать сокет и превращаться в недоступность сервера
    private static async Task CancelProxyAvailabilityBeforeConnectAsync()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var proxy = CreateProxy(); proxy.HostName = "127.0.0.1"; proxy.Port = (ushort)((IPEndPoint)listener.LocalEndpoint).Port;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = false;
        try { await TgProxyAvailabilityChecker.CheckAsync(proxy, ct: cancellation.Token); }
        catch (OperationCanceledException) { cancelled = true; }
        Assert(cancelled && !listener.Pending(), "Отмена проигнорирована или уже открыла соединение");
    }

    // Проверяем реальные открытый и закрытый TCP-порты, пропуск прямого соединения и обновление строк без записи результата в БД
    private static async Task CheckAllProxyAvailabilityAsync()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var open = CreateProxy(); open.HostName = "127.0.0.1"; open.Port = (ushort)((IPEndPoint)listener.LocalEndpoint).Port;
        await Repository.SaveAsync(open);
        using var closedListener = new TcpListener(IPAddress.Loopback, 0);
        closedListener.Start();
        var closed = CreateProxy(); closed.HostName = "127.0.0.1"; closed.Port = (ushort)((IPEndPoint)closedListener.LocalEndpoint).Port;
        closedListener.Stop();
        await Repository.SaveAsync(closed);
        await Repository.CreateDefaultAsync();
        var vm = CreateListViewModel();
        await vm.OnNavigatedToAsync(null);
        await GetCheckAllCommand(vm).ExecuteAsync(null);
        Assert(GetAvailability(vm.Dtos.Single(x => x.Uid == open.Uid)).Contains("Доступен")
            && GetAvailability(vm.Dtos.Single(x => x.Uid == closed.Uid)).Contains("Недоступен")
            && GetAvailability(vm.Dtos.Single(x => x.Type == TgEnumProxyType.None)) == "Без прокси", "Результаты TCP-проверки не соответствуют портам");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var accepted = await listener.AcceptTcpClientAsync(timeout.Token);
        Assert(accepted.Connected && !vm.Exception.IsExist && !GetCheckAllCommand(vm).IsRunning, "Проверка не открыла соединение или не завершилась");
        Assert((await Repository.GetListDtosAsync()).All(x => GetAvailability(x).Length == 0), "Результат проверки записан в БД");
        await vm.OnNavigatedToAsync(null);
        Assert(GetAvailability(vm.Dtos.Single(x => x.Uid == open.Uid)) == "Не проверен", "После перезагрузки показан устаревший результат");
    }

    // Команда применения должна использовать выделенную запись и передавать её UID форме подключения
    private static Task ProxyTableApplyNavigationAsync()
    {
        var vm = CreateListViewModel();
        var command = vm.ApplyProxyCommand;
        Assert(!command.CanExecute(null), "Можно применить отсутствующую строку");
        var dto = CreateProxy(); dto.Uid = Guid.NewGuid(); vm.SelectedProxy = dto;
        Assert(command.CanExecute(null), "Выбранная строка недоступна для применения");
        command.Execute(null);
        Assert(vm.NavigationService.LastKey?.EndsWith("TgClientConnectionViewModel") == true && Equals(vm.NavigationService.Parameter, dto.Uid),
            "Команда не передала выбранный прокси форме подключения");
        return Task.CompletedTask;
    }

    // Вызываем ту же команду, что и кнопка проверки таблицы
    private static IAsyncRelayCommand GetCheckAllCommand(TgProxiesViewModel vm) => vm.CheckAllProxiesCommand;

    // Читаем наблюдаемый статус строки таблицы
    private static string GetAvailability(TgEfProxyDto dto) => dto.AvailabilityStatus;

    // Строковые заголовки совместимы с DataGridColumnHeader и не требуют собственного шаблона
    private static Task ProxyHeadersUseStringContentAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("ProxiesPage.xaml")!;
        var page = XDocument.Load(stream);
        var columns = page.Descendants().Single(x => x.Name.LocalName == "DataGrid.Columns").Elements().ToArray();
        foreach (var column in columns)
        {
            Assert(!column.Elements().Any(x => x.Name.LocalName.EndsWith(".Header")),
                "DataGridColumnHeader не поддерживает UIElement в Header: визуальный элемент должен находиться в ContentTemplate");
            Assert(((string?)column.Attribute("Header"))?.StartsWith("{x:Bind ViewModel.Proxy") == true, "У колонки нет переведённого строкового Header");
            Assert(column.Attribute("HeaderStyle") is null, "Строковый заголовок не должен подменяться шаблоном с унаследованной моделью страницы");
        }
        return Task.CompletedTask;
    }

    // Проверяем привязки и настоящие переводы строк Header для обеих локализаций
    private static Task ProxyTableHeadersAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("ProxiesPage.xaml")!;
        var page = XDocument.Load(stream);
        var columns = page.Descendants().Single(x => x.Name.LocalName == "DataGrid.Columns").Elements().ToArray();
        Assert(columns.Length == 6, "В таблице нет отдельной колонки доступности");
        var keys = new[] { "Type", "Host", "Port", "User", "Password", "Availability" };
        Assert(columns.Select(x => (string?)x.Attribute("Header")).SequenceEqual(keys.Select(x => $"{{x:Bind ViewModel.Proxy{x}Header}}")),
            "Заголовки колонок не связаны с нужными строковыми свойствами модели");
        foreach (var language in new[] { "ru", "en" })
        {
            using var resource = typeof(Program).Assembly.GetManifestResourceStream($"ProxyResources.{language}.resw")!;
            var values = XDocument.Load(resource).Root!.Elements("data");
            TgResourceExtensions.ProxyHeaderLanguage = language;
            var vm = CreateListViewModel();
            var headers = new[] { vm.ProxyTypeHeader, vm.ProxyHostHeader, vm.ProxyPortHeader, vm.ProxyUserHeader, vm.ProxyPasswordHeader, vm.ProxyAvailabilityHeader };
            TgResourceExtensions.ProxyHeaderLanguage = "ru";
            for (var i = 0; i < keys.Length; i++)
                Assert(!string.IsNullOrWhiteSpace(headers[i]) && headers[i] == values.Single(x => (string?)x.Attribute("name")
                    == $"ProxyColumn{keys[i]}").Element("value")!.Value, "Header не содержит перевода соответствующей колонки");
            foreach (var key in new[] { "ProxyCheckAllAction.Text", "ProxyApplyAction.Text", "ProxyCheckHelp.Text", "ProxyAvailabilityNotChecked",
                "ProxyAvailabilityChecking", "ProxyAvailabilityDirect", "ProxyAvailabilityAvailable", "ProxyAvailabilityUnavailable",
                "ProxyApplied", "ProxySelectedNewProfile" })
                Assert(values.Any(x => (string?)x.Attribute("name") == key && !string.IsNullOrWhiteSpace(x.Element("value")?.Value)),
                    "Отсутствует перевод действия или результата проверки прокси");
        }
        return Task.CompletedTask;
    }

    // Проверяем наличие кнопки проверки и применение записи через контекстное меню таблицы
    private static Task ProxyTableActionsAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("ProxiesPage.xaml")!;
        var page = XDocument.Load(stream);
        var checkAllButton = page.Descendants().Single(x => x.Name.LocalName == "Button"
            && (string?)x.Attribute("Command") == "{x:Bind ViewModel.CheckAllProxiesCommand}");
        var checkAllText = checkAllButton.Elements().Single();
        Assert(checkAllText.Name.LocalName == "TextBlock"
            && checkAllText.Attributes().Any(x => x.Name.LocalName == "Uids.Uid" && x.Value == "ProxyCheckAllAction")
            && checkAllText.Attribute("Text") is null, "Подпись кнопки должна браться из ресурсов без переопределения фиксированным текстом");
        var grid = page.Descendants().Single(x => x.Name.LocalName == "DataGrid");
        Assert(grid.Descendants().Any(x => x.Name.LocalName == "MenuFlyoutItem"
            && (string?)x.Attribute("Command") == "{x:Bind ViewModel.ApplyProxyCommand}"), "В таблице нет команды применения прокси");
        Assert(grid.Attribute("RightTapped") is not null, "Правая кнопка мыши не выбирает строку контекстного меню");
        return Task.CompletedTask;
    }
}
