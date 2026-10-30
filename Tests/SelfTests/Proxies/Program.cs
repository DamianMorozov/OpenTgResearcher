namespace OpenTgResearcherDesktop.ProxySelfTests;

internal static partial class Program
{
    // Проверяем исходную VM, настоящий репозиторий и клиент без подключения к Telegram
    private static async Task<int> Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--live") return await RunLiveConnectionAsync(args);
        TgGlobalTools.SetAppType(TgEnumAppType.Test);
        typeof(TgInfrastructure.Helpers.TgLogUtils).GetField("_startupLog", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null,
            Path.Combine(AppContext.BaseDirectory, "proxy-selftest.log"));
        await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "proxy-selftest.log"), "");
        var tests = new (string Name, Func<Task> Run)[]
        {
            ("FormInitialized", FormInitializedAsync),
            ("SaveReturnsGeneratedUid", SaveReturnsGeneratedUidAsync),
            ("SavedFormRemainsLoaded", SavedFormRemainsLoadedAsync),
            ("SaveStartsAfterDialogClosed", SaveStartsAfterDialogClosedAsync),
            ("SaveIsLocal", SaveIsLocalAsync),
            ("InvalidEndpointRejected", InvalidEndpointRejectedAsync),
            ("ProxyConfiguredBeforeLogin", ProxyConfiguredBeforeLoginAsync),
            ("CancelledSaveDoesNotWrite", CancelledSaveDoesNotWriteAsync),
            ("InvalidPortDoesNotSaveOldValue", InvalidPortDoesNotSaveOldValueAsync),
            ("InvalidSaveIsShownAndLogged", InvalidSaveIsShownAndLoggedAsync),
            ("ReloadRestoresStoredFields", ReloadRestoresStoredFieldsAsync),
            ("ClearKeepsStoredRecordUntilSave", ClearKeepsStoredRecordUntilSaveAsync),
            ("DefaultProxyIsIdempotent", DefaultProxyIsIdempotentAsync),
            ("DuplicateDoesNotOverwriteCredentials", DuplicateDoesNotOverwriteCredentialsAsync),
            ("Port65535Accepted", Port65535AcceptedAsync),
            ("MtProtoRequiresSecret", MtProtoRequiresSecretAsync),
            ("AddAndEditNavigate", AddAndEditNavigateAsync),
            ("SelectedDeletePreservesOtherReferences", SelectedDeletePreservesOtherReferencesAsync),
            ("DeleteAllClearsReferences", DeleteAllClearsReferencesAsync),
            ("CancelledDeletionPreservesRecords", CancelledDeletionPreservesRecordsAsync),
            ("DeletionUsesCapturedSelection", DeletionUsesCapturedSelectionAsync),
            ("DetailDeletionReturnsToList", DetailDeletionReturnsToListAsync),
            ("DeleteRollbackRestoresReferences", DeleteRollbackRestoresReferencesAsync),
            ("HttpUsesConnectProtocol", HttpUsesConnectProtocolAsync),
            ("SocksUsesSocks5Protocol", SocksUsesSocks5ProtocolAsync),
            ("MtProtoConfiguredBeforeLogin", MtProtoConfiguredBeforeLoginAsync),
            ("NoneClearsPreviousTransport", NoneClearsPreviousTransportAsync),
            ("ConfiguredProxyDoesNotPreventReadiness", ConfiguredProxyDoesNotPreventReadinessAsync),
            ("InvalidProxyFailsBeforeConnection", InvalidProxyFailsBeforeConnectionAsync),
            ("ConfirmedSaveWaitsForStorage", ConfirmedSaveWaitsForStorageAsync),
            ("RejectedSaveCanBeCorrected", RejectedSaveCanBeCorrectedAsync),
            ("CancelledRepositorySaveDoesNotWrite", CancelledRepositorySaveDoesNotWriteAsync),
            ("DefaultProxyRecognizedByConnectionForm", DefaultProxyRecognizedByConnectionFormAsync),
            ("HttpPassesCredentials", HttpPassesCredentialsAsync),
            ("SocksPassesCredentials", SocksPassesCredentialsAsync),
            ("DirectTransportConnectsAfterReset", DirectTransportConnectsAfterResetAsync),
            ("DisposeWaitsForTelegramDisconnect", DisposeWaitsForTelegramDisconnectAsync),
            ("DisposeDoesNotCallClosedUi", DisposeDoesNotCallClosedUiAsync),
        };
        var failures = 0;
        foreach (var test in tests)
        {
            await using var context = new TestContext(new DbContextOptionsBuilder().UseSqlite("Data Source=:memory:").Options);
            await context.Database.OpenConnectionAsync();
            await context.Database.EnsureCreatedAsync();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(context).As<ITgEfContext>().ExternallyOwned();
            builder.Register(_ => App.BusinessLogicManager.StorageManager.ProxyRepository).As<ITgEfProxyRepository>().ExternallyOwned();
            using var container = builder.Build();
            TgGlobalTools.Container = container;
            using var repository = new TgEfProxyRepository();
            App.BusinessLogicManager.StorageManager.ProxyRepository = repository;
            ContentDialog.Confirm = true;
            ContentDialog.OnShow = null;
            try { await test.Run(); Console.WriteLine($"PASS: {test.Name}"); }
            catch (Exception ex) { failures++; Console.WriteLine($"FAIL: {test.Name}: {ex.GetBaseException().Message}"); }
        }
        Console.WriteLine($"Результат: {tests.Length - failures}/{tests.Length}; SQLite в памяти, Telegram не используется");
        return failures == 0 ? 0 : 1;
    }

    // Проверяем готовность модели до создания привязок XAML
    private static Task FormInitializedAsync()
    {
        Assert(CreateViewModel().Dto is not null, "DTO формы не создан");
        return Task.CompletedTask;
    }

    // Проверяем возврат идентификатора созданной записи вызывающему коду
    private static async Task SaveReturnsGeneratedUidAsync()
    {
        var dto = CreateProxy();
        await Repository.SaveAsync(dto);
        var saved = (await Repository.GetListDtosAsync()).Single();
        Assert(dto.Uid != Guid.Empty && dto.Uid == saved.Uid, "UID сохранённой записи не возвращён в DTO");
    }

    // Проверяем сохранение новой формы и повторное обновление той же записи
    private static async Task SavedFormRemainsLoadedAsync()
    {
        var vm = CreateViewModel();
        await vm.OnNavigatedToAsync(null);
        vm.Dto = CreateProxy();
        await vm.SaveProxySettingsCommand.ExecuteAsync(null);
        Assert(vm.Uid != Guid.Empty && vm.Dto.HostName == "localhost", "После сохранения новая запись исчезла из формы");
        vm.Dto.HostName = "updated.local";
        await vm.SaveProxySettingsCommand.ExecuteAsync(null);
        Assert((await Repository.GetListDtosAsync()).Single().HostName == "updated.local", "Редактирование создало дубль вместо обновления");
    }

    // Проверяем закрытие подтверждения до обращения к хранилищу
    private static async Task SaveStartsAfterDialogClosedAsync()
    {
        var vm = CreateViewModel();
        vm.Dto = CreateProxy();
        await vm.SaveProxySettingsCommand.ExecuteAsync(null);
        Assert(!vm.StorageStartedWhileDialogOpen, "Запись началась при открытом диалоге");
    }

    // Проверяем отсутствие запуска онлайн-операции при сохранении настроек
    private static async Task SaveIsLocalAsync()
    {
        var vm = CreateViewModel();
        vm.Dto = CreateProxy();
        await vm.SaveProxySettingsCommand.ExecuteAsync(null);
        Assert(vm.OnlineCalls == 0, "Локальная операция запущена как сетевая");
    }

    // Проверяем валидацию до записи, а не молчаливое сохранение нерабочего адреса
    private static async Task InvalidEndpointRejectedAsync()
    {
        var dto = CreateProxy();
        dto.HostName = "";
        var failed = false;
        try { await Repository.SaveAsync(dto); }
        catch (FluentValidation.ValidationException) { failed = true; }
        Assert(failed && (await Repository.GetCountAsync()) == 0, "Прокси без адреса сохранён");
    }

    // Проверяем установку транспорта на настоящем неавторизованном клиенте
    private static async Task ProxyConfiguredBeforeLoginAsync()
    {
        using var fixture = new ClientFixture();
        var original = fixture.Client.TcpHandler;
        await fixture.Connection.ConnectThroughProxyAsync(CreateProxy(), isDesktop: true);
        Assert(fixture.Connection.IsProxyUsage && fixture.Client.TcpHandler != original, "Прокси не настроен до авторизации");
    }

    // Создаём новую запись без реального адреса внешнего сервера
    private static TgEfProxyDto CreateProxy() => new() { Type = TgEnumProxyType.Socks, HostName = "localhost", Port = 1080 };
    // Создаём исходную VM без Windows
    private static TgProxyViewModel CreateViewModel() => new(null!, new TestSettings(), new INavigationService(), null!);
    private static ITgEfProxyRepository Repository => App.BusinessLogicManager.StorageManager.ProxyRepository;
    // Проверяем наблюдаемый результат
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
