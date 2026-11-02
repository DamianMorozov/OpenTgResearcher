namespace OpenTgResearcherDesktop.ChatSelfTests;

internal static class LiveMediaTests
{
    // Проверяем готовые файлы по настоящим метаданным Telegram, не запуская загрузку архива
    internal static async Task<int> RunAsync(string database, string sessionPath, long chatId, string directory)
    {
        TgGlobalTools.SetAppType(TgEnumAppType.Test);
        var copiedSession = Path.Combine(Path.GetTempPath(), "OpenTgResearcher-LiveMedia-" + Guid.NewGuid().ToString("N") + ".session");
        try
        {
            var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
                { DataSource = database, Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly }.ToString();
            await using var context = new TestContext(new DbContextOptionsBuilder().UseSqlite(connectionString).Options);
            var builder = new ContainerBuilder();
            builder.RegisterInstance(context).As<ITgEfContext>().ExternallyOwned();
            builder.RegisterType<TgEfUserRepository>().As<ITgEfUserRepository>();
            builder.RegisterType<TgEfSourceRepository>().As<ITgEfSourceRepository>();
            builder.RegisterType<TgEfStoryRepository>().As<ITgEfStoryRepository>();
            builder.RegisterType<TgEfVersionRepository>().As<ITgEfVersionRepository>();
            using var container = builder.Build();
            TgGlobalTools.Container = container;
            using var profiles = new TgEfAppRepository();
            using var proxies = new TgEfProxyRepository();
            var profile = await profiles.GetCurrentDtoAsync();
            var proxy = await proxies.GetDtoAsync(x => x.Uid == profile.ProxyUid);
            var source = await context.Sources.AsNoTracking().SingleAsync(x => x.Id == chatId);
            var ids = await context.Messages.AsNoTracking().Where(x => x.SourceId == chatId && x.Type == TgEnumMessageType.Document)
                .OrderByDescending(x => x.Id).Take(20).Select(x => x.Id).ToArrayAsync();
            if (ids.Length == 0) throw new InvalidOperationException("В выбранном чате нет документов для проверки");
            using var session = new MemoryStream();
            await using (var original = new FileStream(sessionPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                await original.CopyToAsync(session);
            await File.WriteAllBytesAsync(copiedSession, session.ToArray());
            WTelegram.Helpers.Log = (_, _) => { };
            using var client = new WTelegram.Client(name => name switch
            {
                "api_id" => profile.ApiIdString,
                "api_hash" => profile.ApiHashString,
                "phone_number" => profile.PhoneNumber,
                "session_pathname" => copiedSession,
                "verification_code" or "password" or "email_verification_code" =>
                    throw new InvalidOperationException("Для проверки требуется существующая авторизация без нового кода"),
                _ => null,
            });
            using var state = new LoadStateService();
            using var cache = new FusionCache(new FusionCacheOptions());
            var connection = new LiveMediaConnection(cache, state) { Client = client };
            GC.SuppressFinalize(connection);
            WTelegram.Helpers.Log = (_, _) => { };
            await connection.ConnectThroughProxyAsync(proxy ?? new(), isDesktop: true);
            await client.LoginUserIfNeeded().WaitAsync(TimeSpan.FromSeconds(40));
            var messages = await TL.SchemaExtensions.Channels_GetMessages(client, new TL.InputChannel(source.Id, source.AccessHash),
                ids.Select(x => (TL.InputMessage)x).ToArray()).WaitAsync(TimeSpan.FromSeconds(30));
            var settings = new TgDownloadSettingsViewModel();
            settings.SourceVm.Dto.IsSaveMessages = false;
            settings.SourceVm.Dto.IsRewriteFiles = false;
            var checkedFiles = 0;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            foreach (var message in messages.Messages.OfType<TL.Message>())
            {
                if (message.media is not TL.MessageMediaDocument { document: TL.Document document }) continue;
                var media = new TgMediaInfoModel(document.Filename, document.size, message.Date) { LocalPathOnly = directory, Number = message.ID.ToString() };
                media.Normalize(isJoinFileNameWithMessageId: true);
                var file = new DirectoryInfo(directory).EnumerateFiles("* " + document.Filename)
                    .FirstOrDefault(x => long.TryParse(x.Name[..x.Name.IndexOf(' ')], out var id) && id == message.ID && x.Length == document.size);
                if (file is null) continue;
                // Блокируем запись и удаление оригинала даже при регрессии проверяемого обработчика
                using var readLock = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                media.Number = file.Name[..file.Name.IndexOf(' ')];
                media.Normalize(isJoinFileNameWithMessageId: true);
                var method = typeof(TgConnectClientBase).GetMethod("DownloadMediaFileFromMessageAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
                for (var pass = 0; pass < 2; pass++)
                    await ((Task)method.Invoke(connection, [settings, new TgParseHelper(), message, message.media,
                        new TgMessageSettings(), media, CancellationToken.None])!).WaitAsync(TimeSpan.FromSeconds(3));
                checkedFiles++;
            }
            if (checkedFiles == 0) throw new InvalidOperationException("Среди двадцати документов не найдено готовых файлов с ID и совпадающим размером");
            Console.WriteLine($"PASS: Telegram подтвердил сессию; получено сообщений {messages.Messages.Length}; готовых файлов {checkedFiles}; " +
                $"два прохода {timer.ElapsedMilliseconds} мс; запросов загрузки {connection.Downloads}; БД и файлы открыты только для чтения");
            return connection.Downloads == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FAIL: проверка Telegram: {ex.GetBaseException().GetType().Name}; {ex.GetBaseException().Message}");
            return 1;
        }
        finally
        {
            try { if (File.Exists(copiedSession)) File.Delete(copiedSession); }
            catch (Exception ex) { TgLogUtils.WriteException(ex); }
        }
    }
}

internal sealed class LiveMediaConnection(FusionCache cache, LoadStateService state)
    : TgConnectClientBase(null!, new TgFloodControlService(), cache, state)
{
    public int Downloads { get; private set; }

    /// <summary> Запрещает скачивание: проверяем только пропуск уже готовых файлов </summary>
    protected override Task DownloadDocumentMediaAsync(TL.Document document, Stream output, TL.PhotoSizeBase? thumbSize,
        WTelegram.Client.ProgressCallback? progress, CancellationToken ct = default)
    {
        Downloads++;
        throw new InvalidOperationException("Готовый файл ошибочно отправлен на повторную загрузку");
    }
}
