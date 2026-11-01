namespace OpenTgResearcherDesktop.ProxySelfTests;

internal static partial class Program
{
    // Web-записи в начале, середине и конце списка не должны отменять перенос поддерживаемых прокси
    private static async Task TelegramImportMixedWebProxiesAsync()
    {
        using var fixture = new TelegramSettingsFixture("mixed-web.tdf");
        var vm = CreateListViewModel();
        SetTelegramDirectory(vm, fixture.DirectoryPath);
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        var stored = await Repository.GetListDtosAsync();
        Assert(!vm.Exception.IsExist && stored.Count == 3 && vm.Dtos.Count == 3, "Web-запись отменила импорт поддерживаемых прокси");
        Assert(stored.Any(x => x.Type == TgEnumProxyType.Socks && x.Password == "pass:123")
            && stored.Any(x => x.Type == TgEnumProxyType.Http && x.Port == 65535)
            && stored.Any(x => x.Type == TgEnumProxyType.MtProto && x.Secret == "dd" + string.Concat(Enumerable.Repeat("ab", 16))),
            "При пропуске Web потеряны параметры поддерживаемых прокси");
        Assert(ReadImportResult(vm).Contains("Web: 3"), "Пропуск Web не показан пользователю отдельным счётчиком");
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(await Repository.GetCountAsync() == 3 && ReadImportResult(vm).Contains("Добавлено: 0")
            && ReadImportResult(vm).Contains("совпадения: 3") && ReadImportResult(vm).Contains("Web: 3"),
            "Повторный импорт создал дубликаты или смешал счётчики совпадений и Web");
        fixture.AssertUnchanged();
    }

    // Файл только с Web должен объяснять отсутствие добавленных записей
    private static async Task TelegramImportOnlyWebProxiesAsync()
    {
        using var fixture = new TelegramSettingsFixture("only-web.tdf");
        var vm = CreateListViewModel();
        SetTelegramDirectory(vm, fixture.DirectoryPath);
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(!vm.Exception.IsExist && await Repository.GetCountAsync() == 0 && vm.Dtos.Count == 0
            && ReadImportResult(vm).Contains("Добавлено: 0") && ReadImportResult(vm).Contains("Web: 1"),
            "Файл только с Web выдан за ошибку чтения или причина пустого результата скрыта");
        fixture.AssertUnchanged();
    }

    // Пропуск известного Web не должен скрывать ошибку следующей поддерживаемой записи
    private static async Task TelegramImportInvalidAfterWebAsync()
    {
        using var fixture = new TelegramSettingsFixture("invalid-after-web.tdf");
        var vm = CreateListViewModel();
        SetTelegramDirectory(vm, fixture.DirectoryPath);
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(vm.Exception.IsExist && vm.Exception.Message.Contains("недопустимый порт") && await Repository.GetCountAsync() == 0
            && ReadImportResult(vm).Length == 0, "Ошибка порта после Web скрыта или сохранена часть списка");
        fixture.AssertUnchanged();
    }

    // Проверяем путь, скопированный из проводника вместе с кавычками
    private static async Task TelegramImportQuotedPathAsync()
    {
        using var fixture = new TelegramSettingsFixture("modern.tdf");
        var vm = CreateListViewModel();
        SetTelegramDirectory(vm, $"  \"{fixture.DirectoryPath}\"  ");
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(!vm.Exception.IsExist && vm.Dtos.Count == 3 && await Repository.GetCountAsync() == 3,
            "Путь в кавычках не принят командой импорта");
        fixture.AssertUnchanged();
    }

    // Проверяем переменные окружения в пути из инструкции
    private static async Task TelegramImportEnvironmentPathAsync()
    {
        using var fixture = new TelegramSettingsFixture("modern.tdf");
        var variable = "OTR_PROXY_TEST_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(variable, fixture.DirectoryPath);
        try
        {
            var proxies = await ReadTelegramSettingsAsync($"%{variable}%");
            Assert(proxies.Count == 3 && proxies[0].HostName == "socks.example.test", "Переменная окружения в пути не раскрыта");
        }
        finally { Environment.SetEnvironmentVariable(variable, null); }
        fixture.AssertUnchanged();
    }

    // Проверяем приоритет выбранного каталога и чтение tdata из родительского каталога
    private static async Task TelegramImportDirectoryPriorityAsync()
    {
        using var fixture = new TelegramSettingsFixture("modern.tdf");
        var nested = Path.Combine(fixture.DirectoryPath, "tdata");
        Directory.CreateDirectory(nested);
        try
        {
            Assert((await ReadTelegramSettingsAsync(fixture.DirectoryPath)).Count == 3, "Выбранный settingss скрыт вложенным каталогом tdata");
            var nestedSettings = Path.Combine(nested, "settingss");
            File.Move(fixture.SettingsPath, nestedSettings);
            try { Assert((await ReadTelegramSettingsAsync(fixture.DirectoryPath)).Count == 3, "Настройки в дочернем tdata не найдены"); }
            finally { File.Move(nestedSettings, fixture.SettingsPath); }
        }
        finally { Directory.Delete(nested); }
        fixture.AssertUnchanged();
    }

    // Показываем фактически проверенный каталог при отсутствии файла
    private static async Task TelegramImportMissingPathDiagnosticAsync()
    {
        using var fixture = new TelegramSettingsFixture("modern.tdf");
        File.Delete(fixture.SettingsPath);
        var vm = CreateListViewModel();
        SetTelegramDirectory(vm, fixture.DirectoryPath);
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(vm.Exception.IsExist && vm.Exception.Message.Contains(fixture.DirectoryPath) && await Repository.GetCountAsync() == 0,
            "Сообщение об отсутствующем settingss не указывает проверенный каталог");
    }

    // Отличаем занятый файл от повреждённого контейнера
    private static async Task TelegramImportLockedFileDiagnosticAsync()
    {
        using var fixture = new TelegramSettingsFixture("modern.tdf");
        var vm = CreateListViewModel();
        SetTelegramDirectory(vm, fixture.DirectoryPath);
        using (File.Open(fixture.SettingsPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(vm.Exception.IsExist && vm.Exception.Message.Contains(fixture.SettingsPath)
            && !vm.Exception.Message.Contains("повреждён") && await Repository.GetCountAsync() == 0,
            "Блокировка файла выдана за повреждённые настройки или не показан путь");
        fixture.AssertUnchanged();
    }

    // Не скрываем причину отказа формата за общим сообщением о повреждении
    private static async Task TelegramImportUnsupportedTypeDiagnosticAsync()
    {
        using var fixture = new TelegramSettingsFixture("unknown-proxy.tdf");
        var vm = CreateListViewModel();
        SetTelegramDirectory(vm, fixture.DirectoryPath);
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(vm.Exception.IsExist && vm.Exception.Message.Contains("тип прокси: 5") && !vm.Exception.Message.Contains("повреждён")
            && await Repository.GetCountAsync() == 0,
            "Пользователь не видит причину отказа неподдерживаемого типа прокси");
        fixture.AssertUnchanged();
    }

    // Проверяем расшифровку контейнера и отображение всех полей на независимо созданном файле Telegram Desktop
    private static async Task ReadTelegramDesktopSettingsAsync()
    {
        using var fixture = new TelegramSettingsFixture("modern.tdf");
        IReadOnlyList<TgEfProxyDto> dtos;
        using (File.Open(Path.Combine(fixture.DirectoryPath, "key_data"), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            dtos = await ReadTelegramSettingsAsync(fixture.DirectoryPath);
        Assert(dtos.Count == 3, "Список локальных прокси Telegram не прочитан полностью");
        Assert(dtos[0].Type == TgEnumProxyType.Socks && dtos[0].HostName == "socks.example.test" && dtos[0].Port == 1080
            && dtos[0].UserName == "Тест" && dtos[0].Password == "pass:123", "Поля SOCKS5 или строки Qt прочитаны неверно");
        Assert(dtos[1].Type == TgEnumProxyType.Http && dtos[1].HostName == "2001:db8::1" && dtos[1].Port == 65535,
            "Тип HTTP, IPv6 или граничный порт прочитаны неверно");
        Assert(dtos[2].Type == TgEnumProxyType.MtProto && dtos[2].Secret == "dd" + string.Concat(Enumerable.Repeat("ab", 16))
            && dtos[2].Password.Length == 0, "Секрет MTProto не перенесён из поля Telegram password");
        fixture.AssertUnchanged();
    }

    // Проверяем файлы резервного формата без удаления источника или авторизационных данных
    private static async Task ReadTelegramDesktopBackupAsync()
    {
        using var fixture = new TelegramSettingsFixture("modern.tdf");
        File.Move(fixture.SettingsPath, Path.Combine(fixture.DirectoryPath, "settings1"));
        await File.WriteAllBytesAsync(Path.Combine(fixture.DirectoryPath, "settings0"), [0, 1, 2, 3]);
        Assert((await ReadTelegramSettingsAsync(fixture.DirectoryPath)).Count == 3, "Рабочая резервная копия настроек не прочитана");
        Assert(File.Exists(Path.Combine(fixture.DirectoryPath, "settings0")) && File.Exists(Path.Combine(fixture.DirectoryPath, "settings1")),
            "Импорт удалил один из файлов Telegram");
    }

    // Проверяем отсутствие сохранений при повреждённом контейнере, шифротексте или неизвестном формате
    private static async Task RejectInvalidTelegramSettingsAsync()
    {
        foreach (var name in new[] { "modern.tdf", "bad-encryption.tdf", "unsupported.tdf", "invalid-proxy.tdf", "truncated.tdf", "unknown-proxy.tdf" })
        {
            using var fixture = new TelegramSettingsFixture(name);
            if (name == "modern.tdf")
            {
                var bytes = await File.ReadAllBytesAsync(fixture.SettingsPath);
                bytes[^1] ^= 1;
                await File.WriteAllBytesAsync(fixture.SettingsPath, bytes);
            }
            var failed = false;
            try { await ReadTelegramSettingsAsync(fixture.DirectoryPath); }
            catch (System.IO.InvalidDataException ex)
            {
                failed = true;
                Assert(!ex.ToString().Contains("pass:123") && !ex.ToString().Contains("socks.example.test"), "Ошибка раскрывает реквизиты прокси");
            }
            Assert(failed, "Повреждённые или неподдерживаемые настройки молча приняты");
        }
    }

    // Проверяем пустой список и отмену чтения до открытия файлов
    private static async Task EmptyAndCancelledTelegramSettingsAsync()
    {
        using var fixture = new TelegramSettingsFixture("empty.tdf");
        Assert((await ReadTelegramSettingsAsync(fixture.DirectoryPath)).Count == 0, "Пустой список создаёт стандартный прокси");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = false;
        try { await ReadTelegramSettingsAsync(fixture.DirectoryPath, cancellation.Token); }
        catch (OperationCanceledException) { cancelled = true; }
        Assert(cancelled, "Отмена импорта проигнорирована");
    }

    // Проверяем сохранение списка, обновление страницы и отсутствие сетевых операций
    private static async Task ImportTelegramProxiesFromPageAsync()
    {
        using var fixture = new TelegramSettingsFixture("modern.tdf");
        var vm = CreateListViewModel();
        SetTelegramDirectory(vm, fixture.DirectoryPath);
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(await Repository.GetCountAsync() == 3 && vm.Dtos.Count == 3 && vm.OnlineCalls == 0,
            "Команда не сохранила прокси, не обновила список или использовала сетевую операцию");
        Assert(!vm.StorageStartedWhileDialogOpen && !vm.Exception.IsExist && ReadImportResult(vm).Contains('3'),
            "Импорт начался до закрытия подтверждения или не показал результат");
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(await Repository.GetCountAsync() == 3 && ReadImportResult(vm).Contains('0'), "Повторный импорт создал дубликаты");
        fixture.AssertUnchanged();
    }

    // Отмена диалога не должна читать файлы даже при ошибочном пути
    private static async Task CancelTelegramImportAsync()
    {
        var vm = CreateListViewModel();
        SetTelegramDirectory(vm, Path.Combine(AppContext.BaseDirectory, Guid.NewGuid().ToString("N")));
        ContentDialog.Confirm = false;
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(await Repository.GetCountAsync() == 0 && !vm.Exception.IsExist && vm.Errors.Count == 0, "Отмена импорта проигнорирована");
    }

    // Проверяем фиксацию пути до подтверждения и видимую ошибку без частичного импорта
    private static async Task TelegramImportCapturesPathAndShowsErrorsAsync()
    {
        using var valid = new TelegramSettingsFixture("modern.tdf");
        using var invalid = new TelegramSettingsFixture("invalid-proxy.tdf");
        var vm = CreateListViewModel();
        SetTelegramDirectory(vm, valid.DirectoryPath);
        ContentDialog.OnShow = () => SetTelegramDirectory(vm, invalid.DirectoryPath);
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(await Repository.GetCountAsync() == 3, "После подтверждения импортирован другой каталог");
        ContentDialog.OnShow = null;
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(vm.Exception.IsExist && vm.Errors.Count == 1 && await Repository.GetCountAsync() == 3 && ReadImportResult(vm).Length == 0,
            "Ошибка чтения скрыта, остался прежний успех или произошла частичная запись");
    }

    // Дубликаты не должны изменять сохранённые реквизиты и ссылки профилей
    private static async Task TelegramImportPreservesExistingProxiesAsync()
    {
        using var fixture = new TelegramSettingsFixture("modern.tdf");
        var proxies = await ReadTelegramSettingsAsync(fixture.DirectoryPath);
        var existing = new TgEfProxyDto { Type = TgEnumProxyType.Socks, HostName = "SOCKS.EXAMPLE.TEST", Port = 1080, Password = "original" };
        await Repository.SaveAsync(existing);
        var profile = new TgEfAppEntity { Uid = Guid.NewGuid(), ApiHash = Guid.NewGuid(), ApiId = 1, ProxyUid = existing.Uid };
        Context.Apps.Add(profile);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        var count = await ImportTelegramListAsync([.. proxies, proxies[1]]);
        Assert(count == 2 && await Repository.GetCountAsync() == 3, "Дубликат адреса не пропущен или повтор внутри файла сохранён дважды");
        var storedProfile = await Context.Apps.AsNoTracking().SingleAsync(x => x.Uid == profile.Uid);
        Assert((await Repository.GetDtoAsync(existing.Uid)).Password == "original" && storedProfile.ProxyUid == existing.Uid,
            "Импорт перезаписал реквизиты существующего прокси или ссылку профиля");
    }

    // Ошибка записи должна откатывать весь импорт, сохраняя предшествующие записи
    private static async Task TelegramImportRollbackAsync()
    {
        var existing = CreateProxy();
        await Repository.SaveAsync(existing);
        var valid = CreateProxy(); valid.HostName = "new.example.test";
        var invalid = CreateProxy(); invalid.HostName = "";
        var failed = false;
        try { await ImportTelegramListAsync([valid, invalid]); }
        catch (FluentValidation.ValidationException) { failed = true; }
        Assert(failed && await Repository.GetCountAsync() == 1 && (await Repository.GetDtoAsync(existing.Uid)).HostName == existing.HostName,
            "Ошибка в середине импорта не откатила уже записанные строки");
    }

    // Отменённая операция репозитория не должна записывать даже первую строку
    private static async Task CancelledTelegramRepositoryImportAsync()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = false;
        try { await ImportTelegramListAsync([CreateProxy()], cancellation.Token); }
        catch (OperationCanceledException) { cancelled = true; }
        Assert(cancelled && await Repository.GetCountAsync() == 0, "Отменённый импорт записал данные");
    }

    // Импорт должен создавать новые идентификаторы даже при передаче DTO с существующим UID
    private static async Task TelegramImportDoesNotOverwriteByUidAsync()
    {
        var existing = CreateProxy();
        await Repository.SaveAsync(existing);
        var incoming = CreateProxy(); incoming.Uid = existing.Uid; incoming.HostName = "other.example.test";
        var added = await ImportTelegramListAsync([incoming]);
        Assert(added == 1 && await Repository.GetCountAsync() == 2 && (await Repository.GetDtoAsync(existing.Uid)).HostName == existing.HostName,
            "Импорт изменил существующий прокси вместо добавления новой записи");
    }

    // Проверяем откат первой строки и видимую ошибку при фактическом отказе SQLite во время импорта
    private static async Task TelegramImportDatabaseFailureAsync()
    {
        using var fixture = new TelegramSettingsFixture("modern.tdf");
        await Context.Database.ExecuteSqlRawAsync("CREATE TRIGGER DenyImport BEFORE INSERT ON PROXIES WHEN NEW.PORT = 65535 " +
            "BEGIN SELECT RAISE(ABORT, 'Test import failure'); END;");
        var vm = CreateListViewModel();
        SetTelegramDirectory(vm, fixture.DirectoryPath);
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(vm.Exception.IsExist && vm.Errors.Count == 1 && await Repository.GetCountAsync() == 0 && ReadImportResult(vm).Length == 0,
            "Отказ БД оставил часть импортированных строк или был скрыт сообщением об успехе");
        fixture.AssertUnchanged();
    }

    // При ошибочном пути показываем ошибку, а пустой список завершаем без создания записи без прокси
    private static async Task MissingAndEmptyTelegramImportAsync()
    {
        var vm = CreateListViewModel();
        SetTelegramDirectory(vm, Path.Combine(AppContext.BaseDirectory, Guid.NewGuid().ToString("N")));
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(vm.Exception.IsExist && vm.Errors.Count == 1 && await Repository.GetCountAsync() == 0,
            "Отсутствующий каталог не объяснён пользователю");
        using var fixture = new TelegramSettingsFixture("empty.tdf");
        SetTelegramDirectory(vm, fixture.DirectoryPath);
        await GetTelegramImportCommand(vm).ExecuteAsync(null);
        Assert(!vm.Exception.IsExist && await Repository.GetCountAsync() == 0 && ReadImportResult(vm).Contains('0'),
            "Пустой список Telegram создаёт запись без прокси или сохраняет прежнюю ошибку");
    }

    // Проверяем доступность импорта в XAML и полноту пользовательских ресурсов на обоих языках
    private static Task TelegramImportIsAvailableOnPageAsync()
    {
        var assembly = typeof(Program).Assembly;
        using var stream = assembly.GetManifestResourceStream("ProxiesPage.xaml")!;
        var page = System.Xml.Linq.XDocument.Load(stream);
        var attributes = page.Descendants().Attributes().Select(x => x.Value).ToArray();
        Assert(attributes.Contains("{x:Bind ViewModel.ImportTelegramProxiesCommand}"), "На странице нет кнопки импорта");
        Assert(attributes.Any(x => x.Contains("ViewModel.TelegramDataPath") && x.Contains("TwoWay")), "Поле выбора каталога не связано с моделью");
        Assert(attributes.Any(x => x.Contains("ViewModel.ImportResult")), "На странице не отображается результат импорта");
        foreach (var language in new[] { "ru", "en" })
        {
            using var resource = assembly.GetManifestResourceStream($"ProxyResources.{language}.resw")!;
            var values = System.Xml.Linq.XDocument.Load(resource).Root!.Elements("data")
                .Where(x => ((string)x.Attribute("name")!).StartsWith("Telegram", StringComparison.Ordinal)
                    || (string?)x.Attribute("name") == "AskImportTelegramProxies")
                .ToDictionary(x => (string)x.Attribute("name")!, x => x.Element("value")!.Value);
            foreach (var key in new[] { "TelegramProxyImportAction.Text", "TelegramDataDirectory.Header", "TelegramProxyImportHelp.Text",
                "AskImportTelegramProxies", "TelegramProxyImportResult" })
                Assert(values.TryGetValue(key, out var value) && value.Length > 0, "Отсутствует подпись импорта в ресурсах страницы");
            Assert(values["TelegramProxyImportResult"].Contains("{0}") && values["TelegramProxyImportResult"].Contains("{1}")
                && values["TelegramProxyImportResult"].Contains("{2}") && values["TelegramProxyImportResult"].Contains("Web"),
                "Результат импорта не содержит отдельных счётчиков новых записей, совпадений и Web");
        }
        return Task.CompletedTask;
    }

    // Выполняем ту же команду, что и кнопка страницы
    private static IAsyncRelayCommand GetTelegramImportCommand(TgProxiesViewModel vm) => vm.ImportTelegramProxiesCommand;

    // Задаём путь тем же свойством, которое будет связано с полем страницы
    private static void SetTelegramDirectory(TgProxiesViewModel vm, string directory) => vm.TelegramDataPath = directory;

    // Проверяем пользовательский результат импорта
    private static string ReadImportResult(TgProxiesViewModel vm) => vm.ImportResult;

    // Проверяем настоящий репозиторий SQLite
    private static Task<int> ImportTelegramListAsync(IReadOnlyList<TgEfProxyDto> proxies, CancellationToken ct = default) => Repository.ImportAsync(proxies, ct);

    // Вызываем фактический импортёр без сети и файлов установленного Telegram
    private static async Task<IReadOnlyList<TgEfProxyDto>> ReadTelegramSettingsAsync(string directory, CancellationToken ct = default) =>
        (await TgTelegramDesktopProxyImporter.ReadAsync(directory, ct)).Proxies;
}

// Используем только синтетические файлы, не связанные с установленным Telegram
internal sealed class TelegramSettingsFixture : IDisposable
{
    private readonly byte[] _original;
    public string DirectoryPath { get; } = Path.Combine(AppContext.BaseDirectory, "telegram-settings", Guid.NewGuid().ToString("N"));
    public string SettingsPath => Path.Combine(DirectoryPath, "settingss");

    public TelegramSettingsFixture(string resource)
    {
        Directory.CreateDirectory(DirectoryPath);
        var assembly = typeof(Program).Assembly;
        using var stream = assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().Single(x => x.EndsWith(resource, StringComparison.Ordinal)))!;
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        _original = buffer.ToArray();
        File.WriteAllBytes(SettingsPath, _original);
        File.WriteAllText(Path.Combine(DirectoryPath, "key_data"), "Не читать и не изменять");
    }

    /// <summary> Проверяет неизменность настроек и файла авторизации </summary>
    public void AssertUnchanged()
    {
        if (!_original.SequenceEqual(File.ReadAllBytes(SettingsPath)) || File.ReadAllText(Path.Combine(DirectoryPath, "key_data")) != "Не читать и не изменять")
            throw new InvalidOperationException("Импорт изменил локальные данные Telegram");
    }

    /// <summary> Удаляет только файлы собственного временного каталога </summary>
    public void Dispose()
    {
        foreach (var path in Directory.EnumerateFiles(DirectoryPath)) File.Delete(path);
        Directory.Delete(DirectoryPath);
    }
}
