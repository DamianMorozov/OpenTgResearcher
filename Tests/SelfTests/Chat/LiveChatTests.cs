namespace OpenTgResearcherDesktop.ChatSelfTests;

internal static class LiveChatTests
{
    // Проверяем последние сообщения, переподключение и настоящую передачу файлов в каталоге из БД
    internal static async Task<int> RunAsync(string database, string sessionPath, long chatId)
    {
        TgGlobalTools.SetAppType(TgEnumAppType.Test);
        var caseDirectory = Path.Combine(AppContext.BaseDirectory, "live", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(caseDirectory);
        var copiedSession = Path.Combine(caseDirectory, "Telegram.session");
        string? downloadDirectory = null;
        string? sourceDirectory = null;
        TgConnectClientDesktop? connection = null;
        try
        {
            typeof(TgLogUtils).GetField("_startupLog", BindingFlags.Static | BindingFlags.NonPublic)!
                .SetValue(null, Path.Combine(caseDirectory, "live.log"));
            var readOnly = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
                { DataSource = database, Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly };
            await using var original = new TestContext(new DbContextOptionsBuilder().UseSqlite(readOnly.ToString()).Options);
            var source = await original.Sources.AsNoTracking().SingleAsync(x => x.Id == chatId);
            var profileEntity = await original.Apps.AsNoTracking().FirstAsync(x => x.Uid != Guid.Empty);
            var profile = TgEfDomainUtils.CreateNewDto(profileEntity, isUidCopy: true);
            var proxyEntity = await original.Proxies.AsNoTracking().FirstOrDefaultAsync(x => x.Uid == profile.ProxyUid);
            var proxy = proxyEntity is null ? new TgStorage.Domain.Proxies.TgEfProxyDto() : TgEfDomainUtils.CreateNewDto(proxyEntity, isUidCopy: true);
            if (!Directory.Exists(source.Directory)) throw new DirectoryNotFoundException("Каталог чата из БД не существует");
            sourceDirectory = Path.GetFullPath(source.Directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            downloadDirectory = Path.Combine(sourceDirectory, ".OpenTgResearcher-SelfTest-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(downloadDirectory);
            await using var context = new TestContext(new DbContextOptionsBuilder().UseSqlite("Data Source=:memory:").Options);
            await context.Database.OpenConnectionAsync();
            await context.Database.EnsureCreatedAsync();
            if (proxyEntity is not null) context.Proxies.Add(proxyEntity);
            context.Apps.Add(profileEntity);
            context.Sources.Add(source);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(context).As<ITgEfContext>().ExternallyOwned();
            builder.RegisterAssemblyTypes(typeof(TgEfSourceRepository).Assembly).Where(x => x.Name.EndsWith("Repository")).AsImplementedInterfaces();
            using var container = builder.Build();
            TgGlobalTools.Container = container;
            using var storage = new TgStorageService();
            using var state = new LoadStateService();
            using var cache = new FusionCache(new FusionCacheOptions());
            connection = new(storage, new TgFloodControlService(), cache, state);
            GC.SuppressFinalize(connection);
            WTelegram.Helpers.Log = (_, _) => { };
            await using (var originalSession = new FileStream(sessionPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            await using (var copy = File.Create(copiedSession))
                await originalSession.CopyToAsync(copy);
            await connection.ConnectSessionAsync(name => GetConfiguration(name, profile, copiedSession), proxy, isDesktop: true)
                .WaitAsync(TimeSpan.FromSeconds(60));
            if (!await connection.CheckClientConnectionReadyAsync()) throw new InvalidOperationException("Не восстановлена авторизация Telegram");
            var client = connection.Client!;
            var chats = await TL.SchemaExtensions.Channels_GetChannels(client, [new TL.InputChannel(source.Id, source.AccessHash)])
                .WaitAsync(TimeSpan.FromSeconds(30));
            var channel = chats.chats.Values.OfType<TL.Channel>().Single(x => x.ID == source.Id);
            connection.DicChats[source.Id] = channel;
            var history = await TL.SchemaExtensions.Messages_GetHistory(client, new TL.InputPeerChannel(source.Id, source.AccessHash), limit: 100)
                .WaitAsync(TimeSpan.FromSeconds(30));
            var messages = history.Messages.OfType<TL.Message>().ToArray();
            var latest = messages.Max(x => x.ID);
            var settings = new TgDownloadSettingsViewModel();
            settings.SourceVm.Dto = TgEfDomainUtils.CreateNewDto(source, isUidCopy: true);
            settings.SourceVm.Dto.Directory = downloadDirectory;
            settings.SourceVm.Dto.FirstId = Math.Max(1, latest - 5);
            settings.SourceVm.Dto.IsSaveMessages = true;
            settings.SourceVm.Dto.IsSaveFiles = false;
            settings.SourceVm.Dto.IsParsingComments = false;
            settings.SourceVm.Dto.IsCreatingSubdirectories = false;
            settings.SourceVm.Dto.IsRewriteMessages = false;
            settings.SourceVm.Dto.IsAutoUpdate = false;
            settings.SourceVm.Dto.CountThreads = 1;
            await storage.SourceRepository.SaveAsync(settings.SourceVm.Dto);
            await connection.ParseChatAsync(settings).WaitAsync(TimeSpan.FromSeconds(90));
            var count = await context.Messages.CountAsync(x => x.SourceId == source.Id);
            if (count == 0 || count > 10) throw new InvalidOperationException("Ограниченный парсинг не сохранил ожидаемую выборку сообщений");
            Console.WriteLine($"PASS: настоящий ParseChatAsync сохранил {count} сообщений в изолированной БД");
            // Закрываем только транспорт тестового клиента, не меняя сетевые настройки компьютера
            var transport = (System.Net.Sockets.TcpClient?)typeof(WTelegram.Client)
                .GetField("_tcpClient", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(client);
            if (transport is null) throw new InvalidOperationException("Не найден транспорт тестового клиента");
            transport.Close();
            await Task.Delay(300);
            var request = typeof(TgConnectClientBase).GetMethod("TelegramCallAsync", BindingFlags.Instance | BindingFlags.NonPublic)!
                .MakeGenericMethod(typeof(TL.UserBase[]));
            Func<CancellationToken, Task<TL.UserBase[]>> getUser = _ => TL.SchemaExtensions.Users_GetUsers(client, [new TL.InputUserSelf()]);
            var userTask = (Task<TL.UserBase[]>)request.Invoke(connection, [getUser, true, CancellationToken.None])!;
            var users = await userTask.WaitAsync(TimeSpan.FromSeconds(60));
            if (users.Single().ID != connection.Me!.ID || !await connection.CheckClientConnectionReadyAsync())
                throw new InvalidOperationException("После обрыва не восстановился авторизованный запрос");
            Console.WriteLine("PASS: после закрытия TCP-соединения запрос Telegram восстановился с прежней авторизацией");
            Console.WriteLine($"Повторный проход: FirstId={settings.SourceVm.Dto.FirstId}; Count={settings.SourceVm.Dto.Count}; latest={latest}");
            await connection.ParseChatAsync(settings).WaitAsync(TimeSpan.FromSeconds(90));
            var repeated = await context.Messages.CountAsync(x => x.SourceId == source.Id);
            if (repeated != count) throw new InvalidOperationException("Повторный разбор создал дубликаты сообщений");
            Console.WriteLine($"PASS: повторный парсинг сохранил {repeated} сообщений без дубликатов");
            settings.SourceVm.Dto.IsSaveMessages = false;
            settings.SourceVm.Dto.IsSaveFiles = true;
            settings.SourceVm.Dto.IsDownloadThumbnail = false;
            settings.SourceVm.Dto.IsRewriteFiles = false;
            var documents = messages.Where(x => x.media is TL.MessageMediaDocument { document: TL.Document d } && d.size is > 0 and <= 1048576)
                .Take(2).ToArray();
            if (documents.Length == 0) throw new InvalidOperationException("Среди последних ста сообщений нет небольших документов для проверки");
            foreach (var message in documents)
            {
                var document = (TL.Document)((TL.MessageMediaDocument)message.media).document;
                var media = new TgMediaInfoModel(document.Filename, document.size, message.Date)
                    { LocalPathOnly = downloadDirectory, Number = message.ID.ToString() };
                media.Normalize(settings.SourceVm.Dto.IsJoinFileNameWithMessageId);
                var method = typeof(TgConnectClientBase).GetMethod("DownloadMediaFileFromMessageAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
                await ((Task)method.Invoke(connection, [settings, new TgParseHelper(), message, message.media,
                    new TgMessageSettings(), media, CancellationToken.None])!).WaitAsync(TimeSpan.FromSeconds(60));
                var file = new FileInfo(media.LocalPathWithNumber);
                if (!file.Exists || file.Length != document.size) throw new InvalidOperationException("Размер скачанного документа не совпадает с Telegram");
            }
            Console.WriteLine($"PASS: скачано настоящих документов {documents.Length}; размеры совпали; временный подкаталог каталога из БД");
            await connection.DisconnectClientAsync(isAfterClientConnect: false);
            connection = null;
            return 0;
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            Console.Error.WriteLine($"FAIL: {ex.GetBaseException().GetType().Name}; {ex.GetBaseException().Message}; {ex.GetBaseException().StackTrace}");
            return 1;
        }
        finally
        {
            if (connection is not null) await connection.DisconnectClientAsync(isAfterClientConnect: false);
            if (File.Exists(copiedSession)) File.Delete(copiedSession);
            if (downloadDirectory is not null && sourceDirectory is not null && Directory.Exists(downloadDirectory))
            {
                var fullPath = Path.GetFullPath(downloadDirectory);
                if (!fullPath.StartsWith(sourceDirectory, StringComparison.OrdinalIgnoreCase)
                    || !Path.GetFileName(fullPath).StartsWith(".OpenTgResearcher-SelfTest-", StringComparison.Ordinal))
                    throw new InvalidOperationException("Тестовый каталог вышел за пределы каталога чата");
                Directory.Delete(fullPath, recursive: true);
            }
            Console.WriteLine($"Журнал проверки: {caseDirectory}; исходные БД, сессия и файлы чата не изменялись");
        }
    }

    // Восстанавливаем существующую авторизацию без запроса новых кодов и паролей
    private static string? GetConfiguration(string name, TgStorage.Domain.Apps.TgEfAppDto profile, string session) => name switch
    {
        "api_id" => profile.ApiIdString,
        "api_hash" => profile.ApiHashString,
        "phone_number" => profile.PhoneNumber,
        "session_pathname" => session,
        "verification_code" or "password" or "email_verification_code" => throw new InvalidOperationException("Требуется действующая сохранённая авторизация"),
        _ => null,
    };
}
