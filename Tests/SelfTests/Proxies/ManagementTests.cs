namespace OpenTgResearcherDesktop.ProxySelfTests;

internal static partial class Program
{
    private static TestContext Context => (TestContext)TgGlobalTools.Container.Resolve<ITgEfContext>();

    // Проверяем отмену сохранения без изменения SQLite
    private static async Task CancelledSaveDoesNotWriteAsync()
    {
        var vm = CreateViewModel();
        vm.Dto = CreateProxy();
        ContentDialog.Confirm = false;
        await vm.SaveProxySettingsCommand.ExecuteAsync(null);
        Assert(await Repository.GetCountAsync() == 0, "Отмена сохранила запись");
    }

    // Проверяем, что ошибочный текст не подменяется старым допустимым портом DTO
    private static async Task InvalidPortDoesNotSaveOldValueAsync()
    {
        var vm = CreateViewModel();
        vm.Dto = CreateProxy();
        vm.PortText = "65536";
        await vm.SaveProxySettingsCommand.ExecuteAsync(null);
        Assert(await Repository.GetCountAsync() == 0 && vm.Exception.IsExist, "Недопустимый порт сохранён или ошибка не показана");
        vm.PortText = "abc";
        await vm.SaveProxySettingsCommand.ExecuteAsync(null);
        Assert(await Repository.GetCountAsync() == 0 && vm.Errors.Count == 2, "Ошибка текстового порта не журналируется");
    }

    // Проверяем видимую ошибку валидации и запись в журнале
    private static async Task InvalidSaveIsShownAndLoggedAsync()
    {
        var vm = CreateViewModel();
        vm.Dto = CreateProxy();
        vm.Dto.HostName = "https://invalid.example/path";
        await vm.SaveProxySettingsCommand.ExecuteAsync(null);
        Assert(vm.Exception.IsExist && vm.Errors.Count == 1 && await Repository.GetCountAsync() == 0, "Ошибка валидации скрыта");
        var log = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "proxy-selftest.log"));
        Assert(log.Contains("Ошибка сохранения прокси") && log.Contains("Введите имя сервера"), "Причина отказа не попала в журнал");
    }

    // Проверяем повторное чтение всех реквизитов вместо сохранения несохранённых правок
    private static async Task ReloadRestoresStoredFieldsAsync()
    {
        var dto = CreateProxy();
        dto.UserName = "user"; dto.Password = "password"; dto.Secret = "secret";
        await Repository.SaveAsync(dto);
        var vm = CreateViewModel();
        await vm.OnNavigatedToAsync(new() { Parameter = dto.Uid });
        vm.Dto.HostName = "edited.local";
        vm.PortText = "1234";
        await vm.LoadDataStorageCommand.ExecuteAsync(null);
        Assert(vm.Dto.HostName == "localhost" && vm.PortText == "1080" && vm.Dto.UserName == "user" &&
            vm.Dto.Password == "password" && vm.Dto.Secret == "secret", "Перезагрузка не восстановила сохранённые поля");
    }

    // Проверяем очистку полей без удаления записи и потери её UID
    private static async Task ClearKeepsStoredRecordUntilSaveAsync()
    {
        var dto = CreateProxy();
        await Repository.SaveAsync(dto);
        var vm = CreateViewModel();
        await vm.OnNavigatedToAsync(new() { Parameter = dto.Uid });
        await vm.ClearViewCommand.ExecuteAsync(null);
        Assert(vm.Dto.Uid == dto.Uid && vm.Dto.HostName == "" && vm.PortText == "0", "Очистка не сбросила поля или потеряла UID");
        Assert((await Repository.GetDtoAsync(dto.Uid)).HostName == "localhost", "Очистка формы изменила хранилище");
    }

    // Проверяем повторное создание единственной записи без прокси
    private static async Task DefaultProxyIsIdempotentAsync()
    {
        await Repository.CreateDefaultAsync();
        await Repository.CreateDefaultAsync();
        var saved = (await Repository.GetListDtosAsync()).Single();
        Assert(saved.Type == TgEnumProxyType.None && saved.Uid != Guid.Empty, "Стандартная запись не создана или продублирована");
    }

    // Проверяем запрет незаметного изменения существующей записи при добавлении совпадающего адреса
    private static async Task DuplicateDoesNotOverwriteCredentialsAsync()
    {
        var first = CreateProxy(); first.Password = "first";
        await Repository.SaveAsync(first);
        var second = CreateProxy(); second.Password = "second";
        var failed = false;
        try { await Repository.SaveAsync(second); }
        catch (InvalidOperationException) { failed = true; }
        Assert(failed && (await Repository.GetListDtosAsync()).Single().Password == "first", "Добавление дубля изменило чужие реквизиты");
    }

    // Проверяем верхнюю допустимую границу порта
    private static async Task Port65535AcceptedAsync()
    {
        var dto = CreateProxy(); dto.Port = ushort.MaxValue;
        await Repository.SaveAsync(dto);
        Assert((await Repository.GetDtoAsync(dto.Uid)).Port == ushort.MaxValue, "Допустимый порт 65535 отклонён или изменён");
        Assert(TgGlobalTools.GetValidDto<TgEfProxyEntity, TgEfProxyDto>(dto).IsValid, "Проверки DTO и хранилища расходятся");
    }

    // Проверяем обязательный секрет MTProto до записи
    private static async Task MtProtoRequiresSecretAsync()
    {
        var dto = CreateProxy(); dto.Type = TgEnumProxyType.MtProto;
        var failed = false;
        try { await Repository.SaveAsync(dto); }
        catch (FluentValidation.ValidationException) { failed = true; }
        Assert(failed && await Repository.GetCountAsync() == 0, "MTProto без секрета сохранён");
    }

    // Проверяем создание формы без записи и открытие точной выделенной строки
    private static async Task AddAndEditNavigateAsync()
    {
        var vm = CreateListViewModel();
        Assert(!vm.OpenOrEditProxyCommand.CanExecute(null) && !vm.DeleteProxyCommand.CanExecute(null), "Команды доступны без выбора");
        vm.AddProxyCommand.Execute(null);
        Assert(vm.NavigationService.Parameter is Guid uid && uid == Guid.Empty && await Repository.GetCountAsync() == 0, "Добавление создало запись до сохранения");
        var dto = CreateProxy(); await Repository.SaveAsync(dto);
        await vm.OnNavigatedToAsync(null);
        vm.SelectedProxy = vm.Dtos.Single();
        vm.OpenOrEditProxyCommand.Execute(null);
        Assert(vm.NavigationService.Parameter is Guid selected && selected == dto.Uid, "Открыта другая запись");
    }

    // Проверяем удаление выбранного прокси и сброс только связанных с ним профилей
    private static async Task SelectedDeletePreservesOtherReferencesAsync()
    {
        var (first, second) = await SeedReferencesAsync();
        var vm = CreateListViewModel();
        await vm.OnNavigatedToAsync(null);
        vm.SelectedProxy = vm.Dtos.Single(x => x.Uid == first.Uid);
        await vm.DeleteProxyCommand.ExecuteAsync(null);
        Assert(vm.Dtos.Single().Uid == second.Uid && vm.SelectedProxy is null, "Список после удаления неверен");
        var apps = await Context.Apps.AsNoTracking().OrderBy(x => x.ApiId).ToListAsync();
        Assert(apps.Count == 2 && apps[0].ProxyUid is null && apps[1].ProxyUid == second.Uid, "Изменены ссылки другого профиля");
    }

    // Проверяем удаление всех прокси с сохранением профилей
    private static async Task DeleteAllClearsReferencesAsync()
    {
        await SeedReferencesAsync();
        var vm = CreateListViewModel();
        await vm.OnNavigatedToAsync(null);
        await vm.DeleteAllProxiesCommand.ExecuteAsync(null);
        Assert(vm.Dtos.Count == 0 && await Repository.GetCountAsync() == 0, "Прокси остались после удаления всех записей");
        Assert(await Context.Apps.CountAsync() == 2 && await Context.Apps.AllAsync(x => x.ProxyUid == null), "Профили удалены или сохранили ссылки");
    }

    // Проверяем отмену удаления одной записи и очистки списка
    private static async Task CancelledDeletionPreservesRecordsAsync()
    {
        var (first, _) = await SeedReferencesAsync();
        var vm = CreateListViewModel();
        await vm.OnNavigatedToAsync(null);
        vm.SelectedProxy = vm.Dtos.First(x => x.Uid == first.Uid);
        ContentDialog.Confirm = false;
        await vm.DeleteProxyCommand.ExecuteAsync(null);
        await vm.DeleteAllProxiesCommand.ExecuteAsync(null);
        Assert(await Repository.GetCountAsync() == 2 && await Context.Apps.CountAsync(x => x.ProxyUid != null) == 2, "Отмена изменила записи или ссылки");
    }

    // Проверяем неизменность цели удаления при смене выделения во время диалога
    private static async Task DeletionUsesCapturedSelectionAsync()
    {
        var (first, second) = await SeedReferencesAsync();
        var vm = CreateListViewModel();
        await vm.OnNavigatedToAsync(null);
        vm.SelectedProxy = vm.Dtos.First(x => x.Uid == first.Uid);
        ContentDialog.OnShow = () => vm.SelectedProxy = vm.Dtos.First(x => x.Uid == second.Uid);
        await vm.DeleteProxyCommand.ExecuteAsync(null);
        Assert((await Repository.GetListDtosAsync()).Single().Uid == second.Uid, "Удалён новый выбор вместо подтверждённой записи");
    }

    // Проверяем удаление из карточки и переход к обновляемому списку
    private static async Task DetailDeletionReturnsToListAsync()
    {
        var (first, second) = await SeedReferencesAsync();
        var vm = CreateViewModel();
        await vm.OnNavigatedToAsync(new() { Parameter = first.Uid });
        await vm.DeleteProxyCommand.ExecuteAsync(null);
        Assert(vm.Uid == Guid.Empty && !vm.DeleteProxyCommand.CanExecute(null), "Удалённая карточка осталась доступна");
        Assert((await Repository.GetListDtosAsync()).Single().Uid == second.Uid && vm.NavigationService.LastKey == typeof(TgProxiesViewModel).FullName,
            "Удаление не вернуло к списку или изменило другую запись");
    }

    // Проверяем откат изменения ссылок при настоящей ошибке SQLite
    private static async Task DeleteRollbackRestoresReferencesAsync()
    {
        var (first, _) = await SeedReferencesAsync();
        await Context.Database.ExecuteSqlRawAsync("CREATE TRIGGER DenyProxyDelete BEFORE DELETE ON PROXIES BEGIN SELECT RAISE(ABORT, 'Test delete failure'); END;");
        var failed = false;
        try { await Repository.DeleteAsync(x => x.Uid == first.Uid); }
        catch (Microsoft.Data.Sqlite.SqliteException) { failed = true; }
        Assert(failed && await Repository.GetCountAsync() == 2 && await Context.Apps.CountAsync(x => x.ProxyUid != null) == 2,
            "Ошибка удаления оставила частично сброшенные ссылки");
    }

    // Создаём два прокси и два профиля с разными внешними ключами
    private static async Task<(TgEfProxyDto First, TgEfProxyDto Second)> SeedReferencesAsync()
    {
        var first = CreateProxy(); await Repository.SaveAsync(first);
        var second = CreateProxy(); second.HostName = "second.local"; await Repository.SaveAsync(second);
        Context.Apps.AddRange(new TgEfAppEntity { Uid = Guid.NewGuid(), ApiHash = Guid.NewGuid(), ApiId = 1, ProxyUid = first.Uid },
            new TgEfAppEntity { Uid = Guid.NewGuid(), ApiHash = Guid.NewGuid(), ApiId = 2, ProxyUid = second.Uid });
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return (first, second);
    }

    // Создаём исходную модель списка без Windows
    private static TgProxiesViewModel CreateListViewModel() => new(null!, new TestSettings(), new INavigationService(), null!);

    // Проверяем, что команда и индикатор выполнения ожидают завершения записи
    private static async Task ConfirmedSaveWaitsForStorageAsync()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Context.SaveGate = gate;
        var vm = CreateViewModel(); vm.Dto = CreateProxy();
        var operation = vm.SaveProxySettingsCommand.ExecuteAsync(null);
        try
        {
            Assert(!operation.IsCompleted && vm.SaveProxySettingsCommand.IsRunning, "Команда завершилась до записи в SQLite");
        }
        finally { gate.TrySetResult(); await operation; Context.SaveGate = null; }
        Assert(vm.Uid != Guid.Empty && await Repository.GetCountAsync() == 1, "Подтверждение не сохранило запись после ожидания");
    }

    // Проверяем повторное сохранение после исправления ошибочного ввода
    private static async Task RejectedSaveCanBeCorrectedAsync()
    {
        var vm = CreateViewModel(); vm.Dto = CreateProxy(); vm.Dto.HostName = "";
        await vm.SaveProxySettingsCommand.ExecuteAsync(null);
        Assert(vm.Exception.IsExist, "Ошибка ввода не показана");
        vm.Dto.HostName = "fixed.local";
        await vm.SaveProxySettingsCommand.ExecuteAsync(null);
        Assert(!vm.Exception.IsExist && (await Repository.GetListDtosAsync()).Single().HostName == "fixed.local",
            "Исправленная форма не сохраняется или осталась ошибка");
    }

    // Проверяем передачу отмены в настоящий репозиторий
    private static async Task CancelledRepositorySaveDoesNotWriteAsync()
    {
        using var cts = new CancellationTokenSource(); cts.Cancel();
        var cancelled = false;
        try { await Repository.SaveAsync(CreateProxy(), cts.Token); }
        catch (OperationCanceledException) { cancelled = true; }
        Assert(cancelled && await Repository.GetCountAsync() == 0, "Отменённая операция сохранила запись");
    }

    // Проверяем распознавание стандартной записи настоящей моделью списка подключения
    private static async Task DefaultProxyRecognizedByConnectionFormAsync()
    {
        await Repository.CreateDefaultAsync();
        var entity = await Repository.GetQuery().SingleAsync();
        var vm = new TgEfProxyViewModel(TgGlobalTools.Container, entity);
        Assert(vm.IsEmptyProxy, "Запись без прокси не распознана из-за отсутствия текстовых заглушек логина и пароля");
    }
}
