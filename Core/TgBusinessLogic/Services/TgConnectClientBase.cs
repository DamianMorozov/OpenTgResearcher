using TL;
using WTelegram;

namespace TgBusinessLogic.Services;

/// <summary> Base connection client </summary>
public abstract partial class TgConnectClientBase : TgWebDisposable, ITgConnectClient
{
    #region Fields, properties, constructor

    private static TgAppSettingsHelper TgAppSettings => TgAppSettingsHelper.Instance;
    private static TgLocaleHelper TgLocale => TgLocaleHelper.Instance;
    private static TgLogHelper TgLog => TgLogHelper.Instance;
    public Client? Client { get; set; } = default!;
    public Bot? Bot { get; private set; } = default!;
    public TgBotInfoDto? BotInfoDto { get; private set; } = default!;
    public StreamWriter? BotStreamWriterLogs { get; private set; } = default!;
    public TgExceptionViewModel ClientException { get; set; } = default!;
    public TgExceptionViewModel ProxyException { get; set; } = default!;
    public bool IsReady { get; private set; } = default!;
    public bool IsNotReady => !IsReady;
    public bool IsProxyUsage { get; private set; } = default!;
    public TL.User? Me { get; protected set; } = default!;
    public SqliteConnection? BotSqlConnection { get; private set; } = default!;
    protected ITgStorageService StorageManager { get; set; } = default!;
    protected ITgFloodControlService FloodControlService { get; set; } = default!;

    public ConcurrentDictionary<long, TL.ChatBase> ChatCache { get; private set; } = [];
    public ConcurrentDictionary<long, TL.ChatBase> DicChats { get; private set; } = [];
    public ConcurrentDictionary<long, TL.ChatBase> DicChatsUpdated { get; private set; } = [];
    public ConcurrentDictionary<long, TL.InputChannel> InputChannelCache { get; private set; } = [];
    public ConcurrentDictionary<long, TL.StoryItem> DicStories { get; private set; } = [];
    public ConcurrentDictionary<long, TL.User> DicUsers { get; private set; } = [];
    public ConcurrentDictionary<long, TL.User> DicUsersUpdated { get; private set; } = [];
    public ConcurrentQueue<TL.Channel> EnumerableChannels { get; private set; } = [];
    public ConcurrentQueue<TL.Channel> EnumerableGroups { get; private set; } = [];
    public ConcurrentQueue<TL.ChatBase> EnumerableChats { get; private set; } = [];
    public ConcurrentQueue<TL.ChatBase> EnumerableSmallGroups { get; private set; } = [];
    public ConcurrentQueue<TL.DialogBase> EnumerableDialogs { get; private set; } = [];
    public ConcurrentQueue<TL.User> EnumerableUsers { get; private set; } = [];

    public bool IsClientUpdateStatus { get; set; }
    public bool IsBotUpdateStatus { get; set; }

    /// <summary> Cancellation token for download session </summary>
    private CancellationTokenSource? _downloadCts;
    /// <summary> Public token to query cancellation state </summary>
    public CancellationToken DownloadToken { get; private set; } = CancellationToken.None;
    /// <summary> Cancellation token for log session </summary>
    private CancellationTokenSource? _logCts;
    public CancellationToken LogToken { get; private set; } = CancellationToken.None;
    /// <summary> Checks if the operation should be stopped based on the provided cancellation token </summary>
    private bool CheckShouldStop(CancellationToken ct) => ct.IsCancellationRequested || DownloadToken.IsCancellationRequested || Client is null || Client.Disconnected;

    private IEnumerable<TgEfFilterDto> Filters { get; set; } = [];
    public Func<string, Task> UpdateTitleAsync { get; private set; } = default!;
    public Func<string, Task> UpdateStateProxyAsync { get; private set; } = default!;
    public Func<long, int, int, string, Task> UpdateChatViewModelAsync { get; private set; } = default!;
    public Func<int, int, TgEnumChatsMessageType, Task> UpdateChatsViewModelAsync { get; private set; } = default!;
    public Func<bool, int, string, Task> UpdateShellViewModelAsync { get; private set; } = default!;
    public Func<long, string, string, string, Task> UpdateStateContactAsync { get; private set; } = default!;
    public Func<string, int, string, string, Task> UpdateStateExceptionAsync { get; private set; } = default!;
    public Func<Exception, Task> UpdateExceptionAsync { get; private set; } = default!;
    public Func<string, Task> UpdateStateExceptionShortAsync { get; private set; } = default!;
    public Func<Task> AfterClientConnectAsync { get; private set; } = default!;
    public Func<long, int, string, long, long, long, bool, int, Task> UpdateStateFileAsync { get; private set; } = default!;
    public Func<string, Task> UpdateStateMessageAsync { get; private set; } = default!;
    public Func<long, int, string, bool, int, Task> UpdateStateMessageThreadAsync { get; private set; } = default!;

    private TgDownloadSettingsViewModel? _downloadSettings = null;

    protected TgConnectClientBase(ITgStorageService storageManager, ITgFloodControlService floodControlService, IFusionCache cache) : base()
    {
        InitializeClient();
        // Services
        StorageManager = storageManager;
        FloodControlService = floodControlService;
        // FusionCache
        Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _chatBuffer = new(Cache, TgCacheUtils.GetCacheKeyChatPrefix(), entity => $"{entity.Id}");
        _messageBuffer = new(Cache, TgCacheUtils.GetCacheKeyMessagePrefix(), entity => $"{entity.SourceId}:{entity.Id}");
        _messageRelationBuffer = new(Cache, TgCacheUtils.GetCacheKeyMessageRelationPrefix(), entity => $"{entity.ParentSourceId}:{entity.ParentMessageId}:{entity.ChildSourceId}:{entity.ChildMessageId}");
        _storyBuffer = new(Cache, TgCacheUtils.GetCacheKeyStoryPrefix(), entity => $"{entity.Id}");
        _userBuffer = new(Cache, TgCacheUtils.GetCacheKeyUserPrefix(), entity => $"{entity.Id}");
        _tlUserBuffer = new(Cache, TgCacheUtils.GetCacheKeyUserPrefix(), entity => $"{entity.id}");
    }

    protected TgConnectClientBase(IWebHostEnvironment webHostEnvironment, ITgStorageService storageManager, ITgFloodControlService floodControlService, IFusionCache cache) :
        base(webHostEnvironment)
    {
        InitializeClient();
        // Services
        StorageManager = storageManager;
        FloodControlService = floodControlService;
        // FusionCache
        Cache = cache;
        _chatBuffer = new(Cache, TgCacheUtils.GetCacheKeyChatPrefix(), entity => $"{entity.Id}");
        _messageBuffer = new(Cache, TgCacheUtils.GetCacheKeyMessagePrefix(), entity => $"{entity.SourceId}:{entity.Id}");
        _messageRelationBuffer = new(Cache, TgCacheUtils.GetCacheKeyMessageRelationPrefix(), entity => $"{entity.ParentSourceId}:{entity.ParentMessageId}:{entity.ChildSourceId}:{entity.ChildMessageId}");
        _storyBuffer = new(Cache, TgCacheUtils.GetCacheKeyStoryPrefix(), entity => $"{entity.Id}");
        _userBuffer = new(Cache, TgCacheUtils.GetCacheKeyUserPrefix(), entity => $"{entity.Id}");
        _tlUserBuffer = new(Cache, TgCacheUtils.GetCacheKeyUserPrefix(), entity => $"{entity.id}");
    }

    #endregion

    #region IDisposable

    /// <summary> Release managed resources </summary>
    public override async void ReleaseManagedResources()
    {
        var tasks = new List<Task>
        {
            DisconnectClientAsync(),
            DisconnectBotAsync(),
            ReleaseBuffersAsync(),
        };
        await Task.WhenAll(tasks);
    }

    /// <summary> Release unmanaged resources </summary>
    public override void ReleaseUnmanagedResources()
    {
        //
    }

    #endregion

    #region Methods - Flood control

    /// <summary> Creates an action for logging Telegram messages with flood control </summary>
    private Action<int, string> BuildTelegramLogAction() => async (level, message) => { await ProcessLogAndCheckFloodAsync(level, message, LogToken); };

    /// <summary> Processes a log message, checking for flood control and handling it if necessary </summary>
    private async Task ProcessLogAndCheckFloodAsync(int level, string message, CancellationToken ct)
    {
        try
        {
            // Check if flood control is triggered
            if (!FloodControlService.IsFlood(message)) return;

            await UpdateShellViewModelAsync(true, FloodControlService.TryExtractFloodWaitSeconds(message), message);
            if (_downloadSettings is not null)
                await FlushChatBufferAsync(_downloadSettings.IsSaveMessages, _downloadSettings.IsRewriteMessages, isForce: true, ct);
            await FloodControlService.WaitIfFloodAsync(message, ct);
            await UpdateShellViewModelAsync(false, 0, string.Empty);
        }
        catch (OperationCanceledException)
        {
            // Return default immediately when cancellation requested
            return;
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
        finally
        {
#if DEBUG
            Debug.WriteLine($"{level} | {message}", TgConstants.LogTypeNetwork);
#endif
        }
    }

    /// <summary> Executes a Telegram API call with flood control and cancellation support, handling exceptions if needed </summary>
    private async Task<T> TelegramCallAsync<T>(Func<CancellationToken, Task<T>> telegramCall, bool isThrow, CancellationToken ct = default)
    {
        // Check if cancellation was requested before starting
        if (CheckShouldStop(ct))
            return await Task.FromCanceled<T>(ct);

        try
        {
            var result = await FloodControlService.ExecuteWithFloodControlAsync<T>(innerCt => telegramCall(innerCt), isThrow, ct);

            // Check cancellation after execution
            if (CheckShouldStop(ct))
                return await Task.FromCanceled<T>(ct);

            return result;
        }
        catch (OperationCanceledException)
        {
            // Return canceled task if operation was cancelled
            return await Task.FromCanceled<T>(ct);
        }
    }

    #endregion

    #region Methods

    public string ToDebugString() => $"{TgDataUtils.GetIsReady(IsReady)} | {Me}";

    private void InitializeClient()
    {
        ClientException = new();
        ProxyException = new();

        UpdateTitleAsync = _ => Task.CompletedTask;
        UpdateStateProxyAsync = _ => Task.CompletedTask;
        UpdateStateExceptionAsync = (_, _, _, _) => Task.CompletedTask;
        UpdateExceptionAsync = _ => Task.CompletedTask;
        UpdateStateExceptionShortAsync = _ => Task.CompletedTask;
        UpdateShellViewModelAsync = (_, _, _) => Task.CompletedTask;
        UpdateChatViewModelAsync = (_, _, _, _) => Task.CompletedTask;
        UpdateChatsViewModelAsync = (_, _, _) => Task.CompletedTask;
        UpdateStateContactAsync = (_, _, _, _) => Task.CompletedTask;
        AfterClientConnectAsync = () => Task.CompletedTask;
        UpdateStateFileAsync = (_, _, _, _, _, _, _, _) => Task.CompletedTask;
        UpdateStateMessageAsync = _ => Task.CompletedTask;
        UpdateStateMessageThreadAsync = (_, _, _, _, _) => Task.CompletedTask;

        WTelegram.Helpers.Log = BuildTelegramLogAction();
    }

    /// <inheritdoc />
    public async Task<long> GetUserIdAsync(CancellationToken ct = default)
    {
        if (Me is null)
            await LoginUserAsync(isProxyUpdate: false);
        var userId = Me?.ID ?? 0;

        if (userId == 0)
        {
            var licenseDtos = await StorageManager.LicenseRepository.GetListDtosAsync(ct: ct);
            var licenseDto = licenseDtos.OrderByDescending(x => x.ValidTo).FirstOrDefault(x => x.UserId > 0);
            if (licenseDto is not null && licenseDto.UserId > 0)
            {
                userId = licenseDto.UserId;
            }
        }

        return userId;
    }

    public void SetupUpdateStateProxy(Func<string, Task> updateStateProxyAsync) =>
        UpdateStateProxyAsync = updateStateProxyAsync;

    public void SetupUpdateChatViewModel(Func<long, int, int, string, Task> updateChatViewModelAsync) =>
        UpdateChatViewModelAsync = updateChatViewModelAsync;

    public void SetupUpdateChatsViewModel(Func<int, int, TgEnumChatsMessageType, Task> updateChatsViewModelAsync) =>
        UpdateChatsViewModelAsync = updateChatsViewModelAsync;

    public void SetupUpdateShellViewModel(Func<bool, int, string, Task> updateShellViewModelAsync) =>
        UpdateShellViewModelAsync = updateShellViewModelAsync;

    public void SetupUpdateStateContact(Func<long, string, string, string, Task> updateStateContactAsync) =>
        UpdateStateContactAsync = updateStateContactAsync;

    public void SetupUpdateStateException(Func<string, int, string, string, Task> updateStateExceptionAsync) =>
        UpdateStateExceptionAsync = updateStateExceptionAsync;

    public void SetupUpdateException(Func<Exception, Task> updateExceptionAsync) =>
        UpdateExceptionAsync = updateExceptionAsync;

    public void SetupUpdateStateExceptionShort(Func<string, Task> updateStateExceptionShortAsync) =>
        UpdateStateExceptionShortAsync = updateStateExceptionShortAsync;

    public void SetupAfterClientConnect(Func<Task> afterClientConnectAsync) =>
        AfterClientConnectAsync = afterClientConnectAsync;

    public void SetupUpdateTitle(Func<string, Task> updateTitleAsync) =>
        UpdateTitleAsync = updateTitleAsync;

    public void SetupUpdateStateFile(Func<long, int, string, long, long, long, bool, int, Task> updateStateFileAsync) =>
        UpdateStateFileAsync = updateStateFileAsync;

    public void SetupUpdateStateMessage(Func<string, Task> updateStateMessageAsync) =>
        UpdateStateMessageAsync = updateStateMessageAsync;

    public void SetupUpdateStateMessageThread(Func<long, int, string, bool, int, Task> updateStateMessageThreadAsync) =>
        UpdateStateMessageThreadAsync = updateStateMessageThreadAsync;

    private bool ClientResultDisconnected()
    {
        UpdateStateProxyAsync(TgLocale.ProxyIsDisconnect);
        return IsReady = false;
    }

    private bool ClientResultConnected()
    {
        return IsReady = true;
    }

    public async Task ConnectClientConsoleAsync(Func<string, string?>? config, TgEfProxyDto proxyDto)
    {
        var appDto = await StorageManager.AppRepository.GetCurrentDtoAsync();
        if (appDto.UseBot)
        {
            await StorageManager.AppRepository.SetUseBotAsync(false);
            appDto = await StorageManager.AppRepository.GetCurrentDtoAsync();
            if (appDto.UseBot)
                throw new ArgumentOutOfRangeException(nameof(proxyDto), proxyDto, "Cannot set UseBot property!");
        }
        if (IsReady) return;

        await DisconnectClientAsync(isAfterClientConnect: false);
        await DisconnectBotAsync(isAfterClientConnect: false);

        Client = new(config);
        await ConnectThroughProxyAsync(proxyDto, false);
        Client.OnOther += OnClientOtherAsync;
        Client.OnOwnUpdates += OnOwnUpdatesClientAsync;
        Client.OnUpdates += OnUpdatesClientAsync;
        await LoginUserAsync(isProxyUpdate: true);
    }

    public async Task ConnectBotConsoleAsync()
    {
        var appDto = await StorageManager.AppRepository.GetCurrentDtoAsync();
        if (appDto.UseClient)
        {
            await StorageManager.AppRepository.SetUseBotAsync(true);
            appDto = await StorageManager.AppRepository.GetCurrentDtoAsync();
            if (appDto.UseClient)
                throw new ArgumentOutOfRangeException(nameof(appDto), appDto, TgLocale.TgBotSetupCannotSetUseBotProperty);
        }

        await DisconnectClientAsync(isAfterClientConnect: false);

        var isBotConnectionReady = await CheckBotConnectionReadyAsync();
        if (Bot is null || !isBotConnectionReady)
        {
            await DisconnectBotAsync(isAfterClientConnect: false);

            // https://github.com/wiz0u/WTelegramBot/blob/master/Examples/ConsoleApp/Program.cs
            try
            {
                var localFolder = Environment.CurrentDirectory;
                if (Directory.Exists(localFolder))
                {
                    BotStreamWriterLogs ??= new StreamWriter($"{localFolder}\\WTelegramBot.log", true, Encoding.UTF8) { AutoFlush = true };
                    WTelegram.Helpers.Log = WriteBotLogs;
                }

                var botToken = appDto.BotTokenKey;
                var apiId = appDto.ApiId;
                var apiHash = appDto.ApiHash.ToString().Replace("-", "");
                BotSqlConnection = new Microsoft.Data.Sqlite.SqliteConnection(@"Data Source=WTelegramBot.sqlite");

                Bot = new Bot(botToken, apiId, apiHash, BotSqlConnection);
                Bot.OnError += OnErrorBotAsync;
                Bot.OnMessage += OnMessageBotAsync;
                Bot.OnUpdate += OnUpdateBotAsync;
            }
            catch (Exception ex)
            {
                TgLogUtils.WriteException(ex);
                await SetClientExceptionAsync(ex);
            }
        }

        await AfterClientConnectAsync();
    }

    private void WriteBotLogs(int level, string message)
    {
        BotStreamWriterLogs?.WriteLine($"  {DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[level]}] {message}");
#if DEBUG
        Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[level]}] {message}");
#endif
    }

    public async Task ConnectSessionDesktopAsync(TgEfProxyDto proxyDto, Func<string, string?> config)
    {
        if (IsReady)
            return;
        await DisconnectClientAsync(isAfterClientConnect: false);
        Client = new(config);
        await ConnectThroughProxyAsync(proxyDto, true);
        Client.OnOther += OnClientOtherAsync;
        Client.OnOwnUpdates += OnOwnUpdatesClientAsync;
        Client.OnUpdates += OnUpdatesClientAsync;
        await LoginUserAsync(isProxyUpdate: true);
    }

    public async Task ConnectThroughProxyAsync(TgEfProxyDto proxyDto, bool isDesktop)
    {
        IsProxyUsage = false;
        if (!await CheckClientConnectionReadyAsync())
            return;
        if (Client is null)
            return;
        if (proxyDto.Uid == Guid.Empty)
            return;
        if (!isDesktop && !TgAppSettings.IsUseProxy)
            return;
        if (Equals(proxyDto.Type, TgEnumProxyType.None))
            return;
        if (!TgGlobalTools.GetValidDto<TgEfProxyEntity, TgEfProxyDto>(proxyDto).IsValid)
            return;

        try
        {
            ProxyException = new();
            IsProxyUsage = true;
            switch (proxyDto.Type)
            {
                case TgEnumProxyType.Http:
                case TgEnumProxyType.Socks:
                    Client.TcpHandler = (address, port) =>
                    {
                        Socks5ProxyClient proxyClient = string.IsNullOrEmpty(proxyDto.UserName) && string.IsNullOrEmpty(proxyDto.Password)
                            ? new(proxyDto.HostName, proxyDto.Port) : new(proxyDto.HostName, proxyDto.Port, proxyDto.UserName, proxyDto.Password);
                        UpdateStateProxyAsync(TgLocale.ProxyIsConnected);
                        return Task.FromResult(proxyClient.CreateConnection(address, port));
                    };
                    break;
                case TgEnumProxyType.MtProto:
                    Client.MTProxyUrl = string.IsNullOrEmpty(proxyDto.Secret)
                        ? $"https://t.me/proxy?server={proxyDto.HostName}&port={proxyDto.Port}"
                        : $"https://t.me/proxy?server={proxyDto.HostName}&port={proxyDto.Port}&secret={proxyDto.Secret}";
                    await UpdateStateProxyAsync(TgLocale.ProxyIsConnected);
                    break;
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            IsProxyUsage = false;
            await SetProxyExceptionAsync(ex);
        }
    }

    /// <summary> Reduces a Telegram chat ID by removing the "-100" prefix if it exists </summary>
    /// <remarks> This method is commonly used to normalize Telegram chat IDs for consistent processing </remarks>
	public static long ReduceChatId(long chatId) => !$"{chatId}".StartsWith("-100") ? chatId : Convert.ToInt64($"{chatId}"[4..]);

    /// <summary> Ensures that the given chat ID is in the correct format for a supergroup or channel </summary>
    /// <remarks> This method is typically used to standardize chat IDs for APIs or systems that require supergroup or channel IDs to start with "-100" </remarks>
    public static long IncreaseChatId(long chatId) => $"{chatId}".StartsWith("-100") ? chatId : Convert.ToInt64($"-100{chatId}");

    public string GetUserUpdatedName(long id) => DicUsersUpdated.TryGetValue(ReduceChatId(id), out var user) ? user.username : string.Empty;

    /// <summary> Gets a Telegram channel by ID or username with exception handling and cancellation support </summary>
    public async Task<TL.Channel?> GetChannelAsync(TgDownloadSettingsViewModel tgDownloadSettings, CancellationToken ct = default)
    {
        // Collect chats from Telegram
        if (DicChats.IsEmpty)
            await CollectAllChatsAsync(ct);

        // Search by Id if DTO is ready
        if (tgDownloadSettings.SourceVm.Dto.IsReady)
        {
            tgDownloadSettings.SourceVm.Dto.Id = ReduceChatId(tgDownloadSettings.SourceVm.Dto.Id);
            foreach (var chat in DicChats)
            {
                if (chat.Value is TL.Channel channel && Equals(channel.id, tgDownloadSettings.SourceVm.Dto.Id) &&
                    await IsChatBaseAccessAsync(channel, ct))
                    return channel;
            }
        }
        else
        {
            // Search by username if DTO is not ready
            foreach (var chat in DicChats)
            {
                if (chat.Value is TL.Channel channel && Equals(channel.username, tgDownloadSettings.SourceVm.Dto.UserName) &&
                    await IsChatBaseAccessAsync(channel, ct))
                    return channel;
            }
        }

        // If Id is not set, try to get it from username
        if (tgDownloadSettings.SourceVm.Dto.Id is 0 or 1)
            tgDownloadSettings.SourceVm.Dto.Id = await GetPeerIdAsync(tgDownloadSettings.SourceVm.Dto.UserName, ct);

        // Try to get channel directly from Telegram API
        if (Me is not null)
        {
            // Call Telegram API with cancellation support
            var messagesChats = await TelegramCallAsync<TL.Messages_Chats?>(
                apiCt => Client.Channels_GetChannels(new TL.InputChannel(tgDownloadSettings.SourceVm.Dto.Id, Me.access_hash)), isThrow: false, ct);
            if (messagesChats is not null)
            {
                foreach (var chat in messagesChats.chats)
                {
                    if (chat.Value is TL.Channel channel && Equals(channel.ID, tgDownloadSettings.SourceVm.Dto.Id))
                        return channel;
                }
            }
        }

        // Return null if channel not found
        return null;
    }

    /// <summary> Gets a chat base by ID or username with cancellation and exception handling </summary>
    public async Task<TL.ChatBase?> GetChatBaseAsync(TgDownloadSettingsViewModel tgDownloadSettings, CancellationToken ct)
    {
        // Collect chats from Telegram if not already loaded
        if (DicChats.IsEmpty)
            await CollectAllChatsAsync(ct);

        // Search by Id if DTO is ready
        if (tgDownloadSettings.SourceVm.Dto.IsReady)
        {
            tgDownloadSettings.SourceVm.Dto.Id = ReduceChatId(tgDownloadSettings.SourceVm.Dto.Id);
            foreach (var chat in DicChats)
            {
                if (chat.Value is { } chatBase && Equals(chatBase.ID, tgDownloadSettings.SourceVm.Dto.Id))
                    return chatBase;
            }
        }
        else
        {
            // Search by username if DTO is not ready
            foreach (var chat in DicChats)
            {
                if (chat.Value is { } chatBase)
                    return chatBase;
            }
        }

        // If Id is not set, try to get it from username
        if (tgDownloadSettings.SourceVm.Dto.Id is 0)
            tgDownloadSettings.SourceVm.Dto.Id = await GetPeerIdAsync(tgDownloadSettings.SourceVm.Dto.UserName, ct);

        // Try to get chat directly from Telegram API
        if (Me is not null)
        {
            // Call Telegram API with cancellation support
            var messagesChats = await TelegramCallAsync<TL.Messages_Chats?>(apiCt => Client.Channels_GetGroupsForDiscussion(), isThrow: false, ct);

            if (messagesChats is not null)
                foreach (var chat in messagesChats.chats)
                {
                    if (chat.Value is { } chatBase && Equals(chatBase.ID, tgDownloadSettings.SourceVm.Dto.Id))
                        return chatBase;
                }
        }

        // Return null if chat not found
        return null;
    }

    public async Task CreateChatBaseCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (DicChats.IsEmpty)
            await CollectAllChatsAsync();
        if (tgDownloadSettings.SourceVm.Dto.IsReady)
        {
            tgDownloadSettings.SourceVm.Dto.Id = ReduceChatId(tgDownloadSettings.SourceVm.Dto.Id);
            var chatBase = DicChats.FirstOrDefault(x => x.Key.Equals(tgDownloadSettings.SourceVm.Dto.Id)).Value;
            if (chatBase is not null)
                tgDownloadSettings.Chat.Base = chatBase;
        }
        else
        {
            tgDownloadSettings.SourceVm.Dto.UserName = tgDownloadSettings.SourceVm.Dto.UserName.Trim();
            var chatBase = DicChats.FirstOrDefault(x => !string.IsNullOrEmpty(x.Value.MainUsername) &&
                x.Value.MainUsername.Equals(tgDownloadSettings.SourceVm.Dto.UserName)).Value;
            chatBase ??= DicChats.FirstOrDefault(x => x.Value.ID.Equals(tgDownloadSettings.SourceVm.Dto.Id)).Value;
            if (chatBase is not null)
                tgDownloadSettings.Chat.Base = chatBase;
        }
    }

    public async Task<TL.Bots_BotInfo?> GetBotInfoAsync(TgDownloadSettingsViewModel tgDownloadSettings, CancellationToken ct)
    {
        if (tgDownloadSettings.SourceVm.Dto.Id is 0)
            tgDownloadSettings.SourceVm.Dto.Id = await GetPeerIdAsync(tgDownloadSettings.SourceVm.Dto.UserName, ct);
        if (!tgDownloadSettings.SourceVm.Dto.IsReady)
            tgDownloadSettings.SourceVm.Dto.Id = ReduceChatId(tgDownloadSettings.SourceVm.Dto.Id);
        if (!tgDownloadSettings.SourceVm.Dto.IsReady)
            return null;
        TL.Bots_BotInfo? botInfo = null;
        if (Me is not null)
            botInfo = await Client.Bots_GetBotInfo("en", new TL.InputUser(tgDownloadSettings.SourceVm.Dto.Id, 0));
        return botInfo;
    }

    public string GetChatUpdatedName(long id)
    {
        var isGetValue = DicChatsUpdated.TryGetValue(ReduceChatId(id), out var chat);
        if (!isGetValue || chat is null)
            return string.Empty;
        return chat.ToString() ?? string.Empty;
    }

    public string GetPeerUpdatedName(TL.Peer peer) => peer is TL.PeerUser user ? GetUserUpdatedName(user.user_id)
        : peer is TL.PeerChat or TL.PeerChannel ? GetChatUpdatedName(peer.ID) : $"TL.Peer {peer.ID}";

    /// <summary> Collects all chats from Telegram with cancellation and exception handling </summary>
    public async Task<IEnumerable<long>> CollectAllChatsAsync(CancellationToken ct = default)
    {
        // Check if client is ready before making API call
        if (!IsReady || Client is null) return [];

        // Call Telegram API with cancellation support
        var messages = await TelegramCallAsync<TL.Messages_Chats>(apiCt => Client.Messages_GetAllChats(), isThrow: false, ct);

        // Check cancellation before processing results
        if (CheckShouldStop(ct))
            return [];

        // Fill local dictionary with retrieved chats
        FillEnumerableChats(messages.chats);

        // Return chat IDs
        return messages.chats.Select(x => x.Key);
    }

    /// <summary> Collects all dialogs from Telegram with cancellation and exception handling </summary>
    public async Task<IEnumerable<long>> CollectAllDialogsAsync(CancellationToken ct = default)
    {
        // Check if client is ready before making API call
        if (!IsReady || Client is null) return [];

        // Call Telegram API with cancellation support
        var messages = await TelegramCallAsync<TL.Messages_Dialogs>(apiCt => Client.Messages_GetAllDialogs(), isThrow: false, ct);

        // Fill local collection with retrieved dialogs
        FillEnumerableDialogs(messages.Dialogs);

        // Return peer IDs from dialogs
        return messages.Dialogs.Select(x => x.Peer.ID);
    }

    /// <summary> Collect all contacts from Telegram </summary>
    public async Task<List<long>> CollectAllContactsAsync(List<long>? listIds, CancellationToken ct = default)
    {
        if (!IsReady || Client is null) return listIds ?? [];

        EnumerableUsers = [];

        // Call Telegram API with cancellation support
        var contacts = await TelegramCallAsync(apiCt => Client.Contacts_GetContacts(), isThrow: false, ct);

        // Fill local collection with retrieved dialogs
        FillEnumerableUsers(contacts.users);

        // Return peer IDs from chats
        return listIds ?? [];
    }

    /// <summary> Collect all users from Telegram by chat IDs </summary>
    public async Task<List<long>> CollectAllUsersAsync(List<long>? listIds, CancellationToken ct = default)
    {
        if (!IsReady || Client is null) return listIds ?? [];

        EnumerableUsers = [];
        if (listIds is not null && listIds.Count != 0)
        {
            foreach (var chatId in listIds)
            {
                var participants = await GetParticipantsAsync(chatId, ct);
                Dictionary<long, TL.User> users = [];
                foreach (var user in participants)
                    users.Add(user.id, user);
                FillEnumerableUsers(users);
            }
        }
        return listIds ?? [];
    }

    /// <summary> Collect all stories from Telegram </summary>
    public async Task CollectAllStoriesAsync(CancellationToken ct = default)
    {
        if (!IsReady || Client is null) return;

        // Call Telegram API with cancellation support
        var storiesBase = await TelegramCallAsync(apiCt => Client.Stories_GetAllStories(), isThrow: false, ct);
        if (storiesBase is TL.Stories_AllStories allStories)
        {
            FillEnumerableStories([.. allStories.peer_stories]);
        }
    }

    private void FillEnumerableChats(Dictionary<long, TL.ChatBase> chats)
    {
        DicChats = new ConcurrentDictionary<long, TL.ChatBase>(chats.ToDictionary());
        EnumerableChats.Clear();
        EnumerableChannels.Clear();
        EnumerableSmallGroups.Clear();
        EnumerableChannels.Clear();
        EnumerableGroups.Clear();
        // Sort
        var chatsSorted = chats.OrderBy(i => i.Value.MainUsername).ThenBy(i => i.Value.ID).ToList();
        foreach (var chat in chatsSorted)
        {
            EnumerableChats.Enqueue(chat.Value);
            switch (chat.Value)
            {
                case TL.Chat smallGroup when (smallGroup.flags & TL.Chat.Flags.deactivated) is 0:
                    EnumerableSmallGroups.Enqueue(chat.Value);
                    break;
                case TL.Channel { IsGroup: true } group:
                    EnumerableGroups.Enqueue(group);
                    break;
                case TL.Channel channel:
                    EnumerableChannels.Enqueue(channel);
                    break;
            }
        }
    }

    private void FillEnumerableDialogs(TL.DialogBase[] dialogs)
    {
        EnumerableDialogs.Clear();
        if (dialogs == null || dialogs.Length == 0) return;

        // Sort
        var dialogsSorted = dialogs
            //.OrderBy(d => d.TL.Peer is TL.ChatBase cb ? cb.MainUsername : string.Empty)
            //.ThenBy(d => d.TL.Peer is TL.ChatBase cb ? cb.ID : 0)
            .ToList();

        foreach (var dialog in dialogsSorted)
        {
            EnumerableDialogs.Enqueue(dialog);
            //if (dialog.TL.Peer is TL.ChatBase chatBase)
            //{
            //    switch (chatBase)
            //    {
            //        case TL.Chat smallGroup when (smallGroup.flags & TL.Chat.Flags.deactivated) is 0:
            //            EnumerableDialogSmallGroups.Enqueue(dialog);
            //            break;
            //        case TL.Channel { IsGroup: true } group:
            //            EnumerableDialogGroups.Enqueue(dialog);
            //            break;
            //        case TL.Channel channel:
            //            EnumerableDialogChannels.Enqueue(dialog);
            //            break;
            //    }
            //}
        }
    }

    private void AddEnumerableChats(Dictionary<long, TL.ChatBase> chats)
    {
        foreach (var chat in chats)
            if (!DicChats.ContainsKey(chat.Key))
                DicChats.AddOrUpdate(chat.Key, chat.Value, (key, oldValue) => chat.Value);
        // Sort
        var chatsSorted = chats.OrderBy(i => i.Value.MainUsername).ThenBy(i => i.Value.ID).ToList();
        foreach (var chat in chatsSorted)
        {
            if (EnumerableChats.Contains(chat.Value))
                EnumerableChats.Enqueue(chat.Value);
            switch (chat.Value)
            {
                case TL.Chat smallGroup when (smallGroup.flags & TL.Chat.Flags.deactivated) is 0:
                    if (EnumerableSmallGroups.Contains(chat.Value))
                        EnumerableSmallGroups.Enqueue(chat.Value);
                    break;
                case TL.Channel { IsGroup: true } group:
                    if (EnumerableGroups.Contains(chat.Value))
                        EnumerableGroups.Enqueue(group);
                    break;
                case TL.Channel channel:
                    if (EnumerableChannels.Contains(chat.Value))
                        EnumerableChannels.Enqueue(channel);
                    break;
            }
        }
    }

    private void FillEnumerableUsers(Dictionary<long, TL.User> users)
    {
        DicUsers = new ConcurrentDictionary<long, TL.User>(users.ToDictionary());
        EnumerableUsers.Clear();
        // Sort
        var usersSorted = users.OrderBy(i => i.Value.username).ThenBy(i => i.Value.ID);
        foreach (var user in usersSorted)
        {
            EnumerableUsers.Enqueue(user.Value);
        }
    }

    private void FillEnumerableStories(List<TL.PeerStories> peerStories)
    {
        DicStories.Clear();

        // Sort
        var peerStoriesSorted = peerStories.OrderBy(i => i.stories.Rank).ToArray();
        foreach (var peerStory in peerStoriesSorted)
            foreach (var storyBase in peerStory.stories)
                if (storyBase is TL.StoryItem story)
                    DicStories.AddOrUpdate(peerStory.peer.ID, story, (key, oldValue) => story);
    }

    private async Task OnUpdateShortClientAsync(TL.UpdateShort updateShort)
    {
        try
        {
            updateShort.CollectUsersChats(DicUsersUpdated, DicChatsUpdated);
            if (updateShort.UpdateList.Length != 0)
            {
                foreach (var update in updateShort.UpdateList)
                {
                    try
                    {
                        if (updateShort.Chats.Count != 0)
                        {
                            foreach (var chatBase in updateShort.Chats)
                            {
                                if (chatBase.Value is TL.Channel { IsActive: true } channel)
                                {
                                    await SwitchUpdateTypeAsync(update, channel);
                                }
                            }
                        }
                        else
                            await SwitchUpdateTypeAsync(update);
                    }
                    catch (Exception ex)
                    {
                        TgLogUtils.WriteException(ex);
                        await SetClientExceptionAsync(ex);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            await SetClientExceptionAsync(ex);
        }
    }

    private async Task OnUpdateClientUpdatesAsync(TL.UpdatesBase updates)
    {
        try
        {
            updates.CollectUsersChats(DicUsersUpdated, DicChatsUpdated);
            if (updates.UpdateList.Length != 0)
            {
                foreach (var update in updates.UpdateList)
                {
                    try
                    {
                        if (updates.Chats.Count != 0)
                        {
                            foreach (var chatBase in updates.Chats)
                            {
                                if (chatBase.Value is TL.Channel { IsActive: true } channel)
                                {
                                    await SwitchUpdateTypeAsync(update, channel);
                                }
                            }
                        }
                        else
                            await SwitchUpdateTypeAsync(update);
                    }
                    catch (Exception ex)
                    {
                        TgLogUtils.WriteException(ex);
                        await SetClientExceptionAsync(ex);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            await SetClientExceptionAsync(ex);
        }
    }

    // https://corefork.telegram.org/type/TL.Update
    private async Task SwitchUpdateTypeAsync(TL.Update update, TL.Channel? channel = null)
    {
        await UpdateTitleAsync(TgDataFormatUtils.GetTimeFormat(DateTime.Now));
        //var channelLabel = channel is null ? string.Empty :
        //    string.IsNullOrEmpty(channel.MainUsername) ? channel.ID.ToString() : $"{channel.ID} | {channel.MainUsername}";
        //if (!string.IsNullOrEmpty(channelLabel))
        //    channelLabel = $" for channel [{channelLabel}]";
        //var sourceId = channel?.ID ?? 0;
        //switch (update)
        //{
        //    case UpdateNewChannelMessage updateNewChannelMessage:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"New channel message [{updateNewChannelMessage}]{channelLabel}");
        //        break;
        //    case UpdateNewMessage updateNewMessage:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"New message [{updateNewMessage}]{channelLabel}");
        //        break;
        //    case UpdateMessageID updateMessageId:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Message ID [{updateMessageId}]{channelLabel}");
        //        break;
        //    case UpdateDeleteChannelMessages updateDeleteChannelMessages:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Delete channel messages [{string.Join(", ", updateDeleteChannelMessages.messages)}]{channelLabel}");
        //        break;
        //    case UpdateDeleteMessages updateDeleteMessages:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Delete messages [{string.Join(", ", updateDeleteMessages.messages)}]{channelLabel}");
        //        break;
        //    case UpdateChatUserTyping updateChatUserTyping:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.Chat user typing [{updateChatUserTyping}]{channelLabel}");
        //        break;
        //    case UpdateChatParticipants { participants: ChatParticipants chatParticipants }:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.Chat participants [{chatParticipants.ChatId} | {string.Join(", ", chatParticipants.Participants.Length)}]{channelLabel}");
        //        break;
        //    case UpdateUserStatus updateUserStatus:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.User status [{updateUserStatus.user_id} | {updateUserStatus}]{channelLabel}");
        //        break;
        //    case UpdateUserName updateUserName:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.User name [{updateUserName.user_id} | {string.Join(", ", updateUserName.usernames.Select(item => item.username))}]{channelLabel}");
        //        break;
        //    case UpdateNewEncryptedMessage updateNewEncryptedMessage:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"New encrypted message [{updateNewEncryptedMessage}]{channelLabel}");
        //        break;
        //    case UpdateEncryptedChatTyping updateEncryptedChatTyping:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Encrypted chat typing [{updateEncryptedChatTyping}]{channelLabel}");
        //        break;
        //    case UpdateEncryption updateEncryption:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Encryption [{updateEncryption}]{channelLabel}");
        //        break;
        //    case UpdateEncryptedMessagesRead updateEncryptedMessagesRead:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Encrypted message read [{updateEncryptedMessagesRead}]{channelLabel}");
        //        break;
        //    case UpdateChatParticipantAdd updateChatParticipantAdd:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.Chat participant add [{updateChatParticipantAdd}]{channelLabel}");
        //        break;
        //    case UpdateChatParticipantDelete updateChatParticipantDelete:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.Chat participant delete [{updateChatParticipantDelete}]{channelLabel}");
        //        break;
        //    case UpdateDcOptions updateDcOptions:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Dc options [{string.Join(", ", updateDcOptions.dc_options.Select(item => item.id))}]{channelLabel}");
        //        break;
        //    case UpdateNotifySettings updateNotifySettings:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Notify settings [{updateNotifySettings}]{channelLabel}");
        //        break;
        //    case UpdateServiceNotification updateServiceNotification:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Service notification [{updateServiceNotification}]{channelLabel}");
        //        break;
        //    case UpdatePrivacy updatePrivacy:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Privacy [{updatePrivacy}]{channelLabel}");
        //        break;
        //    case UpdateUserPhone updateUserPhone:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.User phone [{updateUserPhone}]{channelLabel}");
        //        break;
        //    case UpdateReadHistoryInbox updateReadHistoryInbox:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Read history inbox [{updateReadHistoryInbox}]{channelLabel}");
        //        break;
        //    case UpdateReadHistoryOutbox updateReadHistoryOutbox:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Read history outbox [{updateReadHistoryOutbox}]{channelLabel}");
        //        break;
        //    case UpdateWebPage updateWebPage:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Web page [{updateWebPage}]{channelLabel}");
        //        break;
        //    case UpdateReadMessagesContents updateReadMessagesContents:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Read messages contents [{string.Join(", ", updateReadMessagesContents.messages.Select(item => item.ToString()))}]{channelLabel}");
        //        break;
        //    case UpdateEditChannelMessage updateEditChannelMessage:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Edit channel message [{updateEditChannelMessage}]{channelLabel}");
        //        break;
        //    case UpdateEditMessage updateEditMessage:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Edit message [{updateEditMessage}]{channelLabel}");
        //        break;
        //    case UpdateUserTyping updateUserTyping:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.User typing [{updateUserTyping}]{channelLabel}");
        //        break;
        //    case UpdateChannelMessageViews updateChannelMessageViews:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.Channel message views [{updateChannelMessageViews}]{channelLabel}");
        //        break;
        //    case UpdateChannel updateChannel:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.Channel [{updateChannel}]");
        //        break;
        //    case UpdateChannelReadMessagesContents updateChannelReadMessages:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.Channel read messages [{string.Join(", ", updateChannelReadMessages.messages)}]{channelLabel}");
        //        break;
        //    case UpdateChannelUserTyping updateChannelUserTyping:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.Channel user typing [{updateChannelUserTyping}]{channelLabel}");
        //        break;
        //    case UpdateMessagePoll updateMessagePoll:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Message poll [{updateMessagePoll}]{channelLabel}");
        //        break;
        //    case UpdateChannelTooLong updateChannelTooLong:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.Channel too long [{updateChannelTooLong}]{channelLabel}");
        //        break;
        //    case UpdateReadChannelInbox updateReadChannelInbox:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.Channel inbox [{updateReadChannelInbox}]{channelLabel}");
        //        break;
        //    case UpdateChatParticipantAdmin updateChatParticipantAdmin:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"TL.Chat participant admin[{updateChatParticipantAdmin}]{channelLabel}");
        //        break;
        //    case UpdateNewStickerSet updateNewStickerSet:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"New sticker set [{updateNewStickerSet}]{channelLabel}");
        //        break;
        //    case UpdateStickerSetsOrder updateStickerSetsOrder:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Sticker sets order [{updateStickerSetsOrder}]{channelLabel}");
        //        break;
        //    case UpdateStickerSets updateStickerSets:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Sticker sets [{updateStickerSets}]{channelLabel}");
        //        break;
        //    case UpdateSavedGifs updateSavedGifs:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"SavedGifs [{updateSavedGifs}]{channelLabel}");
        //        break;
        //    case UpdateBotInlineQuery updateBotInlineQuery:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Bot inline query [{updateBotInlineQuery}]{channelLabel}");
        //        break;
        //    case UpdateBotInlineSend updateBotInlineSend:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Bot inline send [{updateBotInlineSend}]{channelLabel}");
        //        break;
        //    case UpdateBotCallbackQuery updateBotCallbackQuery:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Bot cCallback query [{updateBotCallbackQuery}]{channelLabel}");
        //        break;
        //    case UpdateInlineBotCallbackQuery updateInlineBotCallbackQuery:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Inline bot callback query [{updateInlineBotCallbackQuery}]{channelLabel}");
        //        break;
        //}
    }

    private async Task OnClientOtherAuthSentCodeAsync(TL.Auth_SentCodeBase authSentCode)
    {
#if DEBUG
        try
        {
            Debug.WriteLine($"{nameof(authSentCode)}: {authSentCode}", TgConstants.LogTypeNetwork);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            await SetClientExceptionAsync(ex);
        }
#endif
        await Task.CompletedTask;
    }

    public static IEnumerable<TL.ChatBase> SortListChats(IList<TL.ChatBase> chats)
    {
        if (!chats.Any())
            return chats;
        List<TL.ChatBase> result = [];
        List<TL.ChatBase> chatsOrders = [.. chats.OrderBy(x => x.Title)];
        foreach (var chatOrder in chatsOrders)
        {
            var chatNew = chats.First(x => Equals(x.Title, chatOrder.Title));
            if (chatNew.ID is not 0)
                result.Add(chatNew);
        }
        return result;
    }

    public static IEnumerable<TL.Channel> SortListChannels(IList<TL.Channel> channels)
    {
        if (!channels.Any())
            return channels;
        List<TL.Channel> result = [];
        List<TL.Channel> channelsOrders = [.. channels.OrderBy(x => x.username)];
        foreach (var chatOrder in channelsOrders)
        {
            var chatNew = channels.First(x => Equals(x.Title, chatOrder.Title));
            if (chatNew.ID is not 0)
                result.Add(chatNew);
        }
        return result;
    }

    public async Task PrintChatsInfoAsync(Dictionary<long, TL.ChatBase> dicChats, string name, bool isSave, CancellationToken ct)
    {
        TgLog.MarkupInfo($"Found {name}: {dicChats.Count}");
        TgLog.MarkupInfo(TgLocale.TgGetDialogsInfo);
        foreach (var dicChat in dicChats)
        {
            await TryCatchAsync(async () =>
            {
                switch (dicChat.Value)
                {
                    case TL.Channel channel:
                        await PrintChatsInfoChannelAsync(channel, false, false, isSave);
                        break;
                    default:
                        TgLog.MarkupLine(GetChatInfo(dicChat.Value));
                        break;
                }
            }, ct: ct);
        }
    }

    private async Task<TL.Messages_ChatFull?> PrintChatsInfoChannelAsync(TL.Channel channel, bool isFull, bool isSilent, bool isSave, CancellationToken ct = default)
    {
        TL.Messages_ChatFull? fullChannel = null;
        try
        {
            // Call Telegram API with cancellation support
            fullChannel = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChannel(channel.id),
                factory: SafeFactory<TL.Messages_ChatFull?>(async (ctx, innerCt) => await TelegramCallAsync(
                    apiCt => Client?.Channels_GetFullChannel(channel) ?? default!, isThrow: false, innerCt)),
                TgCacheUtils.CacheOptionsFullChat, ct);
            if (isSave)
            {
                await StorageManager.SourceRepository.SaveAsync(new() { Id = channel.id, UserName = channel.username, Title = channel.title }, isFirstTry: true, ct);
            }
            if (!isSilent)
            {
                if (fullChannel is not null)
                {
                    if (fullChannel.full_chat is TL.ChannelFull channelFull)
                        TgLog.MarkupLine(GetChannelFullInfo(channelFull, channel, isFull));
                    else
                        TgLog.MarkupLine(GetChatFullBaseInfo(fullChannel.full_chat, channel, isFull));
                }
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            await SetClientExceptionAsync(ex);
        }
        return fullChannel;
    }

    private async Task<TL.Messages_ChatFull?> PrintChatsInfoChatBaseAsync(TL.ChatBase chatBase, bool isFull, bool isSilent, CancellationToken ct)
    {
        TL.Messages_ChatFull? chatFull = null;
        if (Client is null)
            return chatFull;
        try
        {
            // Call Telegram API with cancellation support
            chatFull = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChat(chatBase.ID),
                factory: SafeFactory<TL.Messages_ChatFull?>(async (ctx, innerCt) => await TelegramCallAsync(
                    apiCt => Client?.GetFullChat(chatBase) ?? default!, isThrow: false, innerCt)),
                TgCacheUtils.CacheOptionsFullChat, ct);
            if (chatFull is null) return chatFull;
            if (!isSilent)
            {
                if (chatFull.full_chat is TL.ChannelFull channelFull)
                    TgLog.MarkupLine(GetChannelFullInfo(channelFull, chatBase, isFull));
                else
                    TgLog.MarkupLine(GetChatFullBaseInfo(chatFull.full_chat, chatBase, isFull));
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            await SetClientExceptionAsync(ex);
        }
        return chatFull;
    }

    public static string GetChatInfo(TL.ChatBase chatBase) => $"{chatBase.ID} | {chatBase.Title}";

    public static string GetChannelFullInfo(TL.ChannelFull channelFull, TL.ChatBase chatBase, bool isFull)
    {
        var result = GetChatInfo(chatBase);
        if (isFull)
            result += " | " + Environment.NewLine + channelFull.About;
        return result;
    }

    public static string GetChatFullBaseInfo(TL.ChatFullBase chatFull, TL.ChatBase chatBase, bool isFull)
    {
        var result = GetChatInfo(chatBase);
        if (isFull)
            result += " | " + Environment.NewLine + chatFull.About;
        return result;
    }

    /// <summary> Check access rights for chat </summary>
    public async Task<bool> IsChatBaseAccessAsync(TL.ChatBase chatBase, CancellationToken ct)
    {
        if (chatBase is null || chatBase.ID is 0)
            return false;

        if (Bot is null)
        {
            if (Client is null)
                return false;
            var result = false;
            await TryCatchAsync(async () =>
            {
                if (chatBase is TL.Chat chatBaseObj)
                {
                    // Call Telegram API with cancellation support
                    var full = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChat(chatBaseObj.ID),
                        factory: SafeFactory<TL.Messages_ChatFull?>(async (ctx, innerCt) => await TelegramCallAsync(
                            apiCt => Client?.Messages_GetFullChat(chatBaseObj.ID) ?? default!, isThrow: false, innerCt)),
                        TgCacheUtils.CacheOptionsFullChat, ct);
                    if (full is TL.Messages_ChatFull chatFull && chatFull.full_chat is TL.ChatFull chatFullObj)
                    {
                        if (chatFullObj.flags.HasFlag(TL.User.Flags.has_access_hash))
                        {
                            result = true;
                            return;
                        }
                    }
                }
                if (chatBase is TL.Channel channelBase)
                {
                    if (channelBase.flags.HasFlag(TL.Channel.Flags.has_access_hash))
                    {
                        result = true;
                        return;
                    }
                }
            }, ct: ct);
            return result;
        }
        else
        {
            if (chatBase.ID is 0)
                return false;
            var result = false;
            await TryCatchAsync(async () =>
            {
                var botChatFullInfo = await GetChatDetailsForBot(chatBase.ID, chatBase.MainUsername);
                result = botChatFullInfo is not null;
            }, ct: ct);
            return result;
        }
    }

    private async Task<int> GetChannelMessageIdAsync(ITgDownloadViewModel? tgDownloadSettings, TgEnumPosition position, CancellationToken ct)
    {
        if (Client is null) return 0;
        if (tgDownloadSettings is null) return 0;
        if (tgDownloadSettings.Chat.Base is not { } chatBase) return 0;
        if (chatBase.ID is 0) return 0;

        if (chatBase is TL.Channel channel)
        {
            var fullChannel = await TryGetFullChannelSafeAsync(channel, ct);
            if (fullChannel is null) return 0;
            if (fullChannel.full_chat is not TL.ChannelFull channelFull) return 0;
            // Call Telegram API with cancellation support
            var isAccessToMessages = await TelegramCallAsync(innerCt => Client.Channels_ReadMessageContents(channel), isThrow: false, ct);
            if (isAccessToMessages)
            {
                switch (position)
                {
                    case TgEnumPosition.First:
                        return await SetChannelMessageIdFirstCoreAsync(tgDownloadSettings, chatBase, channelFull, ct);
                    case TgEnumPosition.Last:
                        return GetChannelMessageIdLastCore(channelFull);
                }
            }
        }
        else
        {
            // Call Telegram API with cancellation support
            var fullChannel = await TelegramCallAsync(innerCt => Client.GetFullChat(chatBase), isThrow: false, ct);
            switch (position)
            {
                case TgEnumPosition.First:
                    return await SetChannelMessageIdFirstCoreAsync(tgDownloadSettings, chatBase, fullChannel.full_chat, ct);
                case TgEnumPosition.Last:
                    return GetChannelMessageIdLastCore(fullChannel.full_chat);
            }
        }
        return 0;
    }

    public async Task<int> GetChannelMessageIdLastAsync(ITgDownloadViewModel? tgDownloadSettings, CancellationToken ct) =>
        await GetChannelMessageIdAsync(tgDownloadSettings, TgEnumPosition.Last, ct);

    private static int GetChannelMessageIdLastCore(TL.ChannelFull channelFull) =>
        channelFull.read_inbox_max_id;

    private static int GetChannelMessageIdLastCore(TL.ChatFullBase chatFullBase) =>
        chatFullBase is TL.ChannelFull channelFull ? channelFull.read_inbox_max_id : 0;

    private async Task<int> GetChannelMessageIdLastCoreAsync(TL.Channel channel, CancellationToken ct = default)
    {
        // Call Telegram API with cancellation support
        var fullChannel = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChannel(channel.id),
            factory: SafeFactory<TL.Messages_ChatFull?>(async (ctx, innerCt) => await TelegramCallAsync(
                apiCt => Client?.Channels_GetFullChannel(channel) ?? default!, isThrow: false, innerCt)),
            TgCacheUtils.CacheOptionsFullChat, ct);
        if (fullChannel is null) return 0;
        if (fullChannel.full_chat is not TL.ChannelFull channelFull) return 0;
        return channelFull.read_inbox_max_id;
    }

    private async Task<int> GetChannelMessageIdLastCoreAsync(long chatId, CancellationToken ct = default)
    {
        var channel = EnumerableChannels.FirstOrDefault(x => x.ID == chatId);
        if (channel is not null)
            return await GetChannelMessageIdLastCoreAsync(channel, ct);
        else
        {
            var group = EnumerableGroups.FirstOrDefault(x => x.ID == chatId);
            if (group is not null)
                return await GetChannelMessageIdLastCoreAsync(group, ct);
        }
        return 0;
    }

    public async Task SetChannelMessageIdFirstAsync(ITgDownloadViewModel tgDownloadSettings, CancellationToken ct = default) =>
        await GetChannelMessageIdAsync(tgDownloadSettings, TgEnumPosition.First, ct);

    private async Task<int> SetChannelMessageIdFirstCoreAsync(ITgDownloadViewModel tgDownloadSettings, TL.ChatBase chatBase,
        TL.ChatFullBase chatFullBase, CancellationToken ct = default)
    {
        if (tgDownloadSettings is not TgDownloadSettingsViewModel tgDownloadSettings2) return 0;
        var max = chatFullBase is TL.ChannelFull channelFull ? channelFull.read_inbox_max_id : 0;
        var result = max;
        var partition = 200;
        var inputMessages = new TL.InputMessage[partition];
        var offset = 0;
        var isSkipChannelCreate = false;
        // While
        while (offset < max)
        {
            for (var i = 0; i < partition; i++)
            {
                inputMessages[i] = offset + i + 1;
            }
            //tgDownloadSettings2.SourceVm.Dto.FirstId = offset;
            var start = offset + 1;
            var end = offset + partition;
            // Call Telegram API with cancellation support
            var messages = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyMessages(chatBase.ID, start, end),
                factory: SafeFactory<TL.Messages_MessagesBase?>(async (ctx, innerCt) => await TelegramCallAsync(
                    apiCt => Client.Channels_GetMessages(chatBase as TL.Channel, inputMessages), isThrow: false, innerCt)),
                TgCacheUtils.CacheOptionsChannelMessages, ct);

            if (messages is not null)
            {
                for (var i = messages.Offset; i < messages.Count; i++)
                {
                    // Skip first message.
                    if (!isSkipChannelCreate)
                    {
                        var msg = messages.Messages[i].ToString();
                        // Magic String. It needs refactoring.
                        if (Equals(msg, $"{chatBase.ID} [ChannelCreate]"))
                        {
                            isSkipChannelCreate = true;
                        }
                    }
                    // Check message exists.
                    else if (messages.Messages[i].Date > DateTime.MinValue)
                    {
                        result = offset + i + 1;
                        break;
                    }
                }
            }
            else
                break;
            if (result < max)
                break;
            offset += partition;
        }
        // Finally.
        if (result >= max)
            result = 1;
        //tgDownloadSettings2.SourceVm.Dto.FirstId = result;
        return result;
    }

    public async Task CreateChatAsync(ITgDownloadViewModel tgDownloadSettings, bool isSilent, CancellationToken ct = default)
    {
        if (tgDownloadSettings is not TgDownloadSettingsViewModel tgDownloadSettings2) return;

        var sourceDto = tgDownloadSettings2.SourceVm.Dto;
        var sourceEntity = await StorageManager.SourceRepository.GetItemAsync(new() { Id = sourceDto.Id }, isReadOnly: false, ct);

        await CreateChatBaseCoreAsync(tgDownloadSettings2);

        if (Bot is not null)
        {
            var botChatFullInfo = await GetChatDetailsForBot(sourceDto.Id, sourceDto.UserName);
            if (botChatFullInfo?.TLInfo is TL.Messages_ChatFull { full_chat: TL.ChannelFull channelFull })
            {
                sourceEntity.IsUserAccess = true;
                sourceEntity.IsSubscribe = true;
                sourceEntity.UserName = botChatFullInfo.Username;
                sourceEntity.Count = channelFull.read_outbox_max_id > 0 ? channelFull.read_outbox_max_id : channelFull.read_inbox_max_id;
                sourceEntity.Title = botChatFullInfo.Title;
                sourceEntity.Id = ReduceChatId(channelFull.ID);
                sourceEntity.About = channelFull.About;
                sourceEntity.AccessHash = botChatFullInfo.AccessHash;
            }
        }
        else
        {
            if (tgDownloadSettings2.Chat.Base is { } chatBase && await IsChatBaseAccessAsync(chatBase, ct))
            {
                sourceEntity.IsUserAccess = true;
                sourceEntity.IsSubscribe = true;
                sourceEntity.UserName = chatBase.MainUsername ?? string.Empty;

                // TODO: use smart method from scan chat
                sourceEntity.Count = await GetChannelMessageIdLastAsync(tgDownloadSettings, ct);
                // FullChat cache
                var chatFull = await PrintChatsInfoChatBaseAsync(chatBase, isFull: true, isSilent, ct);
                sourceEntity.Title = chatBase.Title;
                if (chatFull?.full_chat is TL.ChannelFull chatBaseFull)
                {
                    sourceEntity.Id = ReduceChatId(chatBaseFull.ID);
                    sourceEntity.About = chatBaseFull.About;
                }
                if (chatBase is TL.Channel channel)
                {
                    sourceEntity.IsRestrictSavingContent = channel.flags.HasFlag(TL.Channel.Flags.noforwards);
                }
            }
            else
            {
                sourceEntity.IsUserAccess = false;
            }
        }
        sourceEntity.Directory = sourceDto.Directory;
        sourceEntity.FirstId = sourceDto.FirstId;

        await StorageManager.SourceRepository.SaveAsync(sourceEntity, isFirstTry: true, ct);
        tgDownloadSettings2.SourceVm.Fill(sourceEntity);
    }

    /// <summary> Get chat details from bot </summary>
    private async Task<WTelegram.Types.ChatFullInfo?> GetChatDetailsForBot(long dtoId, string dtoUserName)
    {
        if (Bot is null) return null;

        try
        {
            if (dtoId > 0)
            {
                var chatId = IncreaseChatId(dtoId);
                var chatInfo = await Bot.GetChat(chatId);
                if (chatInfo is not null)
                    return chatInfo;
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            TgLog.MarkupWarning($"Error in GetChatDetailsFromBot: {ex.Message}");
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(dtoUserName))
            {
                var userName = dtoUserName.StartsWith('@') ? dtoUserName : $"@{dtoUserName}";
                var chatInfo = await Bot.GetChat(userName);
                if (chatInfo is not null)
                    return chatInfo;
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            TgLog.MarkupWarning($"Error in GetChatDetailsFromBot: {ex.Message}");
        }

        TgLog.MarkupWarning($"TL.Chat not found by id ({dtoId}) or username ({dtoUserName})");
        return null;
    }

    /// <summary> TL.Update source from Telegram </summary>
    public async Task UpdateSourceDbAsync(ITgEfSourceViewModel sourceVm, ITgDownloadViewModel tgDownloadSettings, CancellationToken ct = default)
    {
        if (sourceVm is not TgEfSourceViewModel sourceVm2) return;
        if (tgDownloadSettings is not TgDownloadSettingsViewModel tgDownloadSettings2) return;
        await CreateChatAsync(tgDownloadSettings, isSilent: true, ct);
        sourceVm2.Dto.Copy(tgDownloadSettings2.SourceVm.Dto, isUidCopy: false);
    }

    /// <summary> Build chat entity from Telegram chat </summary>
    private async Task<TgEfSourceEntity> BuildChatEntityAsync(TL.ChatBase chat, string about, int messagesCount, bool isUserAccess)
    {
        var accessHash = chat is TL.Channel ch ? ch.access_hash : 0;

        var storageResult = await StorageManager.SourceRepository.GetByDtoAsync(new() { Id = chat.ID });
        var entity = storageResult.IsExists && storageResult.Item is not null
            ? storageResult.Item
            : new TgEfSourceEntity { Id = chat.ID };

        entity.DtChanged = DateTime.UtcNow;
        entity.AccessHash = accessHash;
        entity.IsActive = chat.IsActive;
        entity.UserName = chat.MainUsername;
        entity.Title = chat.Title;
        entity.About = about;
        if (messagesCount > 0)
            entity.Count = messagesCount;
        entity.IsUserAccess = isUserAccess;

        return entity;
    }

    /// <inheritdoc />
    public async Task UpdateUsersAsync(List<TgEfUserDto> users)
    {
        if (users is null || users.Count == 0) return;

        var userIds = users.Select(u => u.Id).ToList();

        // Load exists users
        var existingUsers = await StorageManager.EfContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        foreach (var userDto in users)
        {
            // Find user at storage
            var existingUser = existingUsers.FirstOrDefault(u => u.Id == userDto.Id);
            if (existingUser is not null)
            {
                // TL.Update
                UpdateUserEntityFromDto(existingUser, userDto);
            }
            else
            {
                // Add new
                var newUser = CreateUserEntityFromDto(userDto);
                StorageManager.EfContext.Users.Add(newUser);
            }
        }

        await StorageManager.EfContext.SaveChangesAsync();
    }

    /// <summary> TL.Update user entity from DTO </summary>
    private static void UpdateUserEntityFromDto(TgEfUserEntity entity, TgEfUserDto dto)
    {
        entity.DtChanged = DateTime.UtcNow;
        entity.AccessHash = dto.AccessHash;
        entity.IsActive = dto.IsContactActive;
        entity.IsBot = dto.IsBot;
        entity.FirstName = dto.FirstName ?? string.Empty;
        entity.LastName = dto.LastName ?? string.Empty;
        entity.UserName = dto.UserName ?? string.Empty;
        entity.UserNames = dto.UserNames ?? string.Empty;
        entity.PhoneNumber = dto.PhoneNumber ?? string.Empty;
        entity.Status = dto.Status ?? string.Empty;
        entity.RestrictionReason = dto.RestrictionReason ?? string.Empty;
        entity.LangCode = dto.LangCode ?? string.Empty;
        entity.StoriesMaxId = dto.StoriesMaxId;
        entity.BotInfoVersion = dto.BotInfoVersion ?? string.Empty;
        entity.BotInlinePlaceholder = dto.BotInlinePlaceholder ?? string.Empty;
        entity.BotActiveUsers = dto.BotActiveUsers;
    }

    private static TgEfUserEntity CreateUserEntityFromDto(TgEfUserDto dto) => new()
    {
        DtChanged = DateTime.UtcNow,
        Id = dto.Id,
        AccessHash = dto.AccessHash,
        IsActive = dto.IsContactActive,
        IsBot = dto.IsBot,
        FirstName = dto.FirstName ?? string.Empty,
        LastName = dto.LastName ?? string.Empty,
        UserName = dto.UserName ?? string.Empty,
        UserNames = dto.UserNames ?? string.Empty,
        PhoneNumber = dto.PhoneNumber ?? string.Empty,
        Status = dto.Status ?? string.Empty,
        RestrictionReason = dto.RestrictionReason ?? string.Empty,
        LangCode = dto.LangCode ?? string.Empty,
        StoriesMaxId = dto.StoriesMaxId,
        BotInfoVersion = dto.BotInfoVersion ?? string.Empty,
        BotInlinePlaceholder = dto.BotInlinePlaceholder ?? string.Empty,
        BotActiveUsers = dto.BotActiveUsers,
        IsContact = dto.IsContact
    };

    private static void TgEfStoryEntityByMessageType(TgEfStoryEntity storyNew, TL.MessageEntity message)
    {
        if (message is null)
            return;
        switch (message.GetType())
        {
            case var cls when cls == typeof(TL.MessageEntityUnknown):
                break;
            case var cls when cls == typeof(TL.MessageEntityMention):
                break;
            case var cls when cls == typeof(TL.MessageEntityHashtag):
                break;
            case var cls when cls == typeof(TL.MessageEntityBotCommand):
                break;
            case var cls when cls == typeof(TL.MessageEntityUrl):
                break;
            case var cls when cls == typeof(TL.MessageEntityEmail):
                break;
            case var cls when cls == typeof(TL.MessageEntityBold):
                break;
            case var cls when cls == typeof(TL.MessageEntityItalic):
                break;
            case var cls when cls == typeof(TL.MessageEntityCode):
                break;
            case var cls when cls == typeof(TL.MessageEntityPre):
                break;
            case var cls when cls == typeof(TL.MessageEntityTextUrl):
                break;
            case var cls when cls == typeof(TL.MessageEntityMentionName):
                break;
            case var cls when cls == typeof(TL.InputMessageEntityMentionName):
                break;
            case var cls when cls == typeof(TL.MessageEntityPhone):
                break;
            case var cls when cls == typeof(TL.MessageEntityCashtag):
                break;
            case var cls when cls == typeof(TL.MessageEntityUnderline):
                break;
            case var cls when cls == typeof(TL.MessageEntityStrike):
                break;
            case var cls when cls == typeof(TL.MessageEntityBankCard):
                break;
            case var cls when cls == typeof(TL.MessageEntitySpoiler):
                break;
            case var cls when cls == typeof(TL.MessageEntityCustomEmoji):
                break;
            case var cls when cls == typeof(TL.MessageEntityBlockquote):
                break;
        }
    }

    private static void TgEfStoryEntityByMediaType(TgEfStoryEntity storyNew, TL.MessageMedia media)
    {
        if (media is null)
            return;
        switch (media.GetType())
        {
            case var cls when cls == typeof(TL.MessageMediaContact):
                break;
            case var cls when cls == typeof(TL.MessageMediaDice):
                break;
            case var cls when cls == typeof(MessageMediaDocument):
                break;
            case var cls when cls == typeof(MessageMediaGame):
                break;
            case var cls when cls == typeof(MessageMediaGeo):
                break;
            case var cls when cls == typeof(MessageMediaGeoLive):
                break;
            case var cls when cls == typeof(MessageMediaGiveaway):
                break;
            case var cls when cls == typeof(MessageMediaGiveawayResults):
                break;
            case var cls when cls == typeof(MessageMediaInvoice):
                break;
            case var cls when cls == typeof(MessageMediaPaidMedia):
                break;
            case var cls when cls == typeof(MessageMediaPhoto):
                break;
            case var cls when cls == typeof(MessageMediaPoll):
                break;
            case var cls when cls == typeof(MessageMediaStory):
                break;
            case var cls when cls == typeof(MessageMediaUnsupported):
                break;
            case var cls when cls == typeof(MessageMediaVenue):
                break;
            case var cls when cls == typeof(MessageMediaWebPage):
                break;
        }
    }

    /// <summary> Search sources from Telegram </summary>
    public async Task SearchSourcesTgAsync(ITgDownloadViewModel iSettings, TgEnumSourceType sourceType, List<long>? listIds = null)
    {
        if (iSettings is not TgDownloadSettingsViewModel tgDownloadSettings) return;
        _downloadSettings = tgDownloadSettings;

        await TryCatchAsync(async () =>
        {
            await UpdateChatsViewModelAsync(0, 0, TgEnumChatsMessageType.StartScan);
            await LoginUserAsync(isProxyUpdate: false);
            switch (sourceType)
            {
                case TgEnumSourceType.Chat:
                    await SetUserAccessForAllChatsAsync(listIds, isUserAccess: false);
                    await SetSubscribeForAllChatsAsync(listIds, isSubscribe: false);
                    await UpdateChatViewModelAsync(tgDownloadSettings.SourceVm.Dto.Id, 0, tgDownloadSettings.SourceVm.Dto.Count, TgLocale.CollectChats);
                    listIds = [.. await CollectAllChatsAsync()];
                    await SetSubscribeForAllChatsAsync(listIds, isSubscribe: true);
                    tgDownloadSettings.SourceScanCount = DicChats.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    await SearchChatsAsync(tgDownloadSettings, listIds);
                    break;
                case TgEnumSourceType.Dialog:
                    await SetUserAccessForAllChatsAsync(listIds, isUserAccess: false);
                    await SetSubscribeForAllChatsAsync(listIds, isSubscribe: false);
                    await UpdateChatViewModelAsync(tgDownloadSettings.SourceVm.Dto.Id, 0, tgDownloadSettings.SourceVm.Dto.Count, TgLocale.CollectDialogs);
                    listIds = [.. await CollectAllDialogsAsync()];
                    await SetSubscribeForAllChatsAsync(listIds, isSubscribe: true);
                    tgDownloadSettings.SourceScanCount = DicChats.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    await SearchChatsAsync(tgDownloadSettings, listIds);
                    break;
                case TgEnumSourceType.Story:
                    await CollectAllStoriesAsync();
                    tgDownloadSettings.SourceScanCount = DicStories.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    await SearchStoriesAsync(tgDownloadSettings);
                    break;
                case TgEnumSourceType.Contact:
                    await UpdateChatViewModelAsync(tgDownloadSettings.ContactVm.Dto.Id, 0, 0, TgLocale.CollectContacts);
                    listIds = [.. await CollectAllContactsAsync(listIds)];
                    tgDownloadSettings.SourceScanCount = DicUsers.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    await SearchUsersAsync(tgDownloadSettings, isContact: true, listIds);
                    break;
                case TgEnumSourceType.User:
                    await UpdateChatViewModelAsync(tgDownloadSettings.ContactVm.Dto.Id, 0, 0, TgLocale.CollectUsers);
                    listIds = [.. await CollectAllUsersAsync(listIds)];
                    tgDownloadSettings.SourceScanCount = DicUsers.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    await SearchUsersAsync(tgDownloadSettings, isContact: false, listIds);
                    break;
            }
        }, async () =>
        {
            _downloadSettings = null;
            await UpdateChatsViewModelAsync(0, 0, TgEnumChatsMessageType.StopScan);
            await UpdateTitleAsync(string.Empty);
        });
    }

    /// <summary> Set user access for all chats </summary>
    private async Task SetUserAccessForAllChatsAsync(List<long>? listIds, bool isUserAccess)
    {
        try
        {
            if (listIds is null)
                await StorageManager.SourceRepository.SetIsUserAccessAsync(isUserAccess);
            else
                await StorageManager.SourceRepository.SetIsUserAccessAsync(listIds, isUserAccess);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            TgLog.MarkupWarning($"Error set user access for all chats: {ex.Message}");
        }
    }

    /// <summary> Set subscribe for all chats </summary>
    private async Task SetSubscribeForAllChatsAsync(List<long>? listIds, bool isSubscribe)
    {
        try
        {
            if (listIds is null)
                await StorageManager.SourceRepository.SetIsSubscribeAsync(isSubscribe);
            else
                await StorageManager.SourceRepository.SetIsSubscribeAsync(listIds, isSubscribe);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            TgLog.MarkupWarning($"Error set subscribe for all chats: {ex.Message}");
        }
    }

    private async Task SearchChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings, List<long>? listIds, CancellationToken ct = default)
    {
        try
        {
            var counter = 0;
            var channels = listIds is not null && listIds.Count != 0 ? EnumerableChannels.Where(x => listIds.Contains(x.ID)) : EnumerableChannels;
            var groups = (listIds is not null && listIds.Count != 0) ? EnumerableGroups.Where(x => listIds.Contains(x.ID)) : EnumerableGroups;
            var smallGroups = (listIds is not null && listIds.Count != 0) ? EnumerableSmallGroups.Where(x => listIds.Contains(x.ID)) : EnumerableSmallGroups;
            var dialogs = (listIds is not null && listIds.Count != 0) ? EnumerableDialogs.Where(x => listIds.Contains(x.Peer.ID)) : EnumerableDialogs;
            var countAll = channels.Count() + groups.Count() + smallGroups.Count() + dialogs.Count();

            // First groups (small + groups)
            counter = await SearchGroupsAsync(tgDownloadSettings, smallGroups, counter, countAll, ct);
            counter = await SearchGroupsAsync(tgDownloadSettings, groups, counter, countAll, ct);

            // Then the channels
            foreach (var channel in channels)
            {
                counter++;
                await UpdateChatsViewModelAsync(counter, countAll, TgEnumChatsMessageType.ProcessingChats);
                tgDownloadSettings.SourceScanCurrent++;
                if (!channel.IsActive) continue;

                await TryCatchAsync(async () =>
                {
                    tgDownloadSettings.Chat.Base = channel;
                    // last-count from FusionCache
                    //var messagesCount = await GetCachedChatLastCountAsync(channel.ID, () => GetChannelMessageIdLastAsync(tgDownloadSettings));
                    if (channel.IsChannel)
                    {
                        // Call Telegram API with cancellation support
                        var fullChannel = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChannel(channel.id),
                            factory: SafeFactory<TL.Messages_ChatFull?>(async (ctx, innerCt) => await TelegramCallAsync(
                                apiCt => Client?.Channels_GetFullChannel(channel) ?? default!, isThrow: false, innerCt)),
                            TgCacheUtils.CacheOptionsFullChat, ct);
                        if (fullChannel?.full_chat is TL.ChannelFull channelFull)
                        {
                            var entity = await BuildChatEntityAsync(channel, channelFull.about, messagesCount: 0, isUserAccess: true);
                            _chatBuffer.Add(entity);
                            await UpdateChatViewModelAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceScanCurrent,
                                tgDownloadSettings.SourceScanCount, $"{channel} | {TgDataFormatUtils.TrimStringEnd(channelFull.about, 40)}");
                        }
                    }
                    else
                    {
                        var entity = await BuildChatEntityAsync(channel, string.Empty, messagesCount: 0, isUserAccess: true);
                        _chatBuffer.Add(entity);
                        await UpdateChatViewModelAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceScanCurrent, tgDownloadSettings.SourceScanCount, $"{channel}");
                    }
                }, async () =>
                {
                    await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(tgDownloadSettings.SourceScanCount,
                        tgDownloadSettings.SourceScanCurrent):#00.00} %");
                }, ct: ct);

                await FlushChatBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: false, ct);
            }
        }
        finally
        {
            await FlushChatBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: true, ct);
        }
    }

    private async Task<int> SearchGroupsAsync(ITgDownloadViewModel tgDownloadSettings, IEnumerable<TL.ChatBase> groups, int counter, int countAll, CancellationToken ct)
    {
        foreach (var group in groups)
        {
            counter++;
            tgDownloadSettings.SourceScanCurrent++;
            if (!group.IsActive) continue;

            await TryCatchAsync(async () =>
            {
                await UpdateChatsViewModelAsync(counter, countAll, TgEnumChatsMessageType.ProcessingGroups);
                tgDownloadSettings.Chat.Base = group;
                // last-count from FusionCache
                //var messagesCount = await GetCachedChatLastCountAsync(group.ID, () => GetChannelMessageIdLastAsync(tgDownloadSettings));
                TgEfSourceEntity entity;
                if (group is TL.Channel ch)
                    entity = await BuildChatEntityAsync(ch, string.Empty, messagesCount: 0, isUserAccess: true);
                else
                    entity = await BuildChatEntityAsync(group, string.Empty, messagesCount: 0, isUserAccess: true);

                _chatBuffer.Add(entity);

                if (tgDownloadSettings is TgDownloadSettingsViewModel tgDownloadSettings2)
                    await UpdateChatViewModelAsync(tgDownloadSettings2.SourceVm.Dto.Id, 0, 0, $"{group}");
            }, ct: ct);
        }
        return counter;
    }

    private async Task SearchStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings, CancellationToken ct = default)
    {
        try
        {
            var counter = 0;
            foreach (var story in DicStories)
            {
                counter++;
                tgDownloadSettings.SourceScanCurrent++;
                await UpdateChatsViewModelAsync(counter, DicStories.Count, TgEnumChatsMessageType.ProcessingStories);
                _ = await FillBufferStoriesAsync(story.Key, story.Value, ct);
                await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(tgDownloadSettings.SourceScanCount, tgDownloadSettings.SourceScanCurrent):#00.00} %");

                await FlushStoryBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: false, ct);
            }
        }
        finally
        {
            await FlushStoryBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: true, ct);
        }
    }

    private async Task SearchUsersAsync(TgDownloadSettingsViewModel tgDownloadSettings, bool isContact, List<long>? listIds, CancellationToken ct = default)
    {
        try
        {
            // Prepare filtered collection if listIds provided
            var usersToProcess = (listIds != null && listIds.Count > 0)
                ? EnumerableUsers.Where(u => listIds.Contains(u.id)).ToList()
                : [.. EnumerableUsers];

            var counter = 0;

            foreach (var user in usersToProcess)
            {
                // Start processing in parallel
                try
                {
                    if (CheckShouldStop(ct)) return;

                    Interlocked.Increment(ref counter);
                    tgDownloadSettings.SourceScanCurrent++;

                    // Update UI with current progress
                    await UpdateChatsViewModelAsync(counter, DicUsers.Count, TgEnumChatsMessageType.ProcessingUsers);

                    // Fill buffer with user data
                    await FillBufferUsersAsync(user, isContact, ct);

                    // Update window title with progress percentage
                    await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(tgDownloadSettings.SourceScanCount, tgDownloadSettings.SourceScanCurrent):#00.00} %");

                    // Flush buffer without forcing
                    await FlushUserBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: false, ct);
                }
                catch (Exception ex)
                {
                    TgLogUtils.WriteException(ex);
                    throw;
                }
            }
        }
        finally
        {
            await FlushUserBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: true, ct);
        }
    }

    /// <summary> Parse Telegram chat </summary>
    public async Task ParseChatAsync(ITgDownloadViewModel tgDownloadSettings)
    {
        if (tgDownloadSettings is not TgDownloadSettingsViewModel settings) return;
        _downloadSettings = settings;

        var ct = InitializeTokenDownloadSession();
        if (CheckShouldStop(ct)) return;

        await PrepareChatAsync(_downloadSettings, ct);

        await TryCatchAsync(async () =>
        {
            if (CheckShouldStop(ct)) return;

            var parseHelper = await CheckAccessToMessagesAsync(_downloadSettings, ct);

            if (parseHelper.IsAccess)
            {
                if (CheckShouldStop(ct)) return;

                // Creating subdirectories
                if (_downloadSettings.SourceVm.Dto.IsCreatingSubdirectories)
                {
                    await EnsureForumDirectoriesAsync(_downloadSettings, parseHelper.Channel, parseHelper.ForumTopicSettings, ct);
                }

                parseHelper.MessageSettings.CurrentChatId = _downloadSettings.SourceVm.Dto.Id;
                parseHelper.ChatCache.TryAddChat(parseHelper.MessageSettings.CurrentChatId, _downloadSettings.SourceVm.Dto.AccessHash, _downloadSettings.SourceVm.Dto.Directory);
                parseHelper.ChatCache.MarkAsSaved(parseHelper.MessageSettings.CurrentChatId);

                try
                {
                    while (parseHelper.SourceFirstId <= parseHelper.SourceLastId)
                    {
                        await DownloadMessagesWithClientAsync(_downloadSettings, parseHelper, ct);

                        await DownloadMessagesWithBotAsync(_downloadSettings, parseHelper, ct);

                        // Count threads
                        parseHelper.SourceFirstId++;
                        if (parseHelper.DownloadTasks.Count == _downloadSettings.CountThreads || parseHelper.SourceFirstId >= parseHelper.SourceLastId)
                        {
                            // Await only already started tasks
                            await Task.WhenAll(parseHelper.DownloadTasks);
                            parseHelper.DownloadTasks.Clear();
                            parseHelper.MessageSettings.ThreadNumber = 0;
                        }
                    }
                }
                finally
                {
                    // We guarantee to wait for all tasks
                    if (parseHelper.DownloadTasks.Count > 0)
                    {
                        await Task.WhenAll(parseHelper.DownloadTasks);
                        parseHelper.DownloadTasks.Clear();
                        parseHelper.MessageSettings.ThreadNumber = 0;
                    }
                }

                //if (!CheckShouldStop(ct))
                //    _downloadSettings.SourceVm.Dto.FirstId = Math.Min(parseHelper.SourceFirstId, parseHelper.SourceLastId);
            }
            _downloadSettings.SourceVm.Dto.IsDownload = false;
        },
        async () => await FinalizeDownloadSessionAsync(_downloadSettings, ct),
        ct: ct);
    }

    /// <summary> Initialize new cancellation token for current download session </summary>
    private CancellationToken InitializeTokenDownloadSession()
    {
        _downloadCts?.Cancel();
        _downloadCts?.Dispose();
        _downloadCts = new CancellationTokenSource();
        DownloadToken = _downloadCts.Token;

        _logCts?.Cancel();
        _logCts?.Dispose();
        _logCts = new CancellationTokenSource();
        LogToken = _logCts.Token;

        return DownloadToken;
    }

    /// <summary> Prepare chat for downloading </summary>
    private async Task PrepareChatAsync(TgDownloadSettingsViewModel settings, CancellationToken ct)
    {
        // Clear cache
        await Cache.ClearAllAsync(ct);

        if (CheckShouldStop(ct)) return;

        // Prepare chat
        await CreateChatAsync(settings, isSilent: false, ct);
    }

    /// <summary> Check access to messages in chat </summary>
    private async Task<TgParseHelper> CheckAccessToMessagesAsync(TgDownloadSettingsViewModel settings, CancellationToken ct)
    {
        var parseHelper = new TgParseHelper();

        // Filters
        Filters = await StorageManager.FilterRepository.GetListDtosAsync(0, 0, x => x.IsEnabled, ct);

        await LoginUserAsync(isProxyUpdate: false);

        var dirExists = await CreateDestinationDirectoryIfNotExistsAsync(settings);
        if (!dirExists)
            return (parseHelper);

        settings.SourceVm.Dto.IsDownload = true;

        if (settings.Chat.Base is TL.Channel)
            parseHelper.Channel = settings.Chat.Base as TL.Channel;

        parseHelper.AppDto = await StorageManager.AppRepository.GetCurrentDtoAsync(ct);
        if (parseHelper.AppDto.UseClient && parseHelper.Channel is not null)
        {
            // Call Telegram API with cancellation support
            parseHelper.IsAccess = await TelegramCallAsync(apiCt => Client.Channels_ReadMessageContents(parseHelper.Channel), isThrow: false, ct);
        }

        if (parseHelper.AppDto.UseBot && Bot is not null)
        {
            parseHelper.BotChatFullInfo = await GetChatDetailsForBot(settings.SourceVm.Dto.Id, settings.SourceVm.Dto.UserName);
            if (parseHelper.BotChatFullInfo is not null)
            {
                parseHelper.IsAccess = true;
            }
        }

        parseHelper.SourceFirstId = settings.SourceVm.Dto.FirstId;
        // Get the last message ID in a chat 
        parseHelper.SourceLastId = settings.SourceVm.Dto.Count = await GetChatLastMessageIdAsync(settings.SourceVm.Dto.Id, ct);
        var entityLastId = await StorageManager.MessageRepository.GetLastIdAsync(settings.SourceVm.Dto.Id);
        if (entityLastId > parseHelper.SourceLastId)
            parseHelper.SourceLastId = (int)entityLastId;

        return parseHelper;
    }

    /// <summary> Ensure forum directories </summary>
    private async Task EnsureForumDirectoriesAsync(TgDownloadSettingsViewModel settings, TL.Channel? channel, TgForumTopicSettings forumTopicSettings, CancellationToken ct)
    {
        if (Client is null && Bot is not null)
            Client = Bot.Client;
        if (Client is not null && channel is not null)
        {
            try
            {
                // Call Telegram API with cancellation support
                Messages_ForumTopics forumTopics = await TelegramCallAsync(
                    apiCt => Client.Channels_GetAllForumTopics(channel), isThrow: false, ct);
                forumTopicSettings.SetTopics(forumTopics);
                foreach (var topic in forumTopicSettings.Topics)
                {
                    try
                    {
                        var dir = Path.Combine(settings.SourceVm.Dto.Directory, topic.Title);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                    }
                    catch (Exception ex)
                    {
                        TgLogUtils.WriteException(ex);
                        await SetClientExceptionAsync(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                TgLogUtils.WriteException(ex);
            }
        }
    }

    /// <summary> Finalize current download session </summary>
    private async Task FinalizeDownloadSessionAsync(TgDownloadSettingsViewModel settings, CancellationToken ct)
    {
        try
        {
            // Dispose download token
            _downloadCts?.Dispose();
            _downloadCts = null;
            DownloadToken = CancellationToken.None;

            // Dispose log token
            _logCts?.Dispose();
            _logCts = null;
            LogToken = CancellationToken.None;

            if (!CheckShouldStop(ct))
            {
                await FlushMessageBufferAsync(settings.IsSaveMessages, settings.IsRewriteMessages, isForce: true, ct);
                await FlushMessageRelationBufferAsync(settings.IsSaveMessages, settings.IsRewriteMessages, isForce: true, ct);
                await UpdateTitleAsync(string.Empty);
            }
        }
        finally
        {
            _downloadSettings = null;
        }
    }

    /// <summary> Download messages with client </summary>
    private async Task DownloadMessagesWithClientAsync(TgDownloadSettingsViewModel settings, TgParseHelper parseHelper, CancellationToken ct)
    {
        if (CheckShouldStop(ct)) return;
        if (parseHelper.AppDto is null || !parseHelper.AppDto.UseClient) return;

        if (Client is null || !settings.SourceVm.Dto.IsDownload || (Client is not null && Client.Disconnected))
        {
            //if (!CheckShouldStop(ct))
            //    settings.SourceVm.Dto.FirstId = parseHelper.SourceLastId;
            settings.SourceVm.Dto.IsDownload = false;
            parseHelper.DownloadTasks.Clear();
            return;
        }

        await TryCatchAsync(async () =>
        {
            if (Client is null) return;

            // Call Telegram API with cancellation support
            var messages = parseHelper.Channel is not null
                ? await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyMessage(parseHelper.Channel.id, parseHelper.SourceFirstId),
                    factory: SafeFactory<TL.Messages_MessagesBase?>(async (ctx, innerCt) => await TelegramCallAsync(
                        apiCt => Client.Channels_GetMessages(parseHelper.Channel, parseHelper.SourceFirstId), isThrow: false, innerCt)),
                    TgCacheUtils.CacheOptionsChannelMessages, ct)
                : await TelegramCallAsync(apiCt => Client.GetMessages(settings.Chat.Base, parseHelper.SourceFirstId), isThrow: false, ct);

            await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(parseHelper.SourceLastId, parseHelper.SourceFirstId):#00.00} %");
            // Check deleted messages and mark storage entity
            var firstMessage = messages?.Messages.FirstOrDefault();
            if (firstMessage is null)
            {
                // Mark as deleted only if not cancelled
                if (!CheckShouldStop(ct))
                    await CheckDeletedMessageAndMarkEntityAsync(settings.SourceVm.Dto.Id, parseHelper.SourceFirstId);
            }
            else
            {
                if (messages is not null)
                {
                    foreach (var message in messages.Messages)
                    {
                        if (CheckShouldStop(ct)) break;

                        // Check message exists
                        if (message is MessageBase messageBase && messageBase.Date > DateTime.MinValue)
                            parseHelper.DownloadTasks.Add(ParseChatMessageAsync(settings, messageBase, parseHelper.ChatCache, parseHelper.MessageSettings, parseHelper.ForumTopicSettings, ct));
                        else
                            await UpdateChatViewModelAsync(parseHelper.MessageSettings.CurrentChatId, message.ID, settings.SourceVm.Dto.Count, "Empty message");
                    }
                }
            }
        }, ct: ct);
    }

    /// <summary> Download messages with bot </summary>
    private async Task DownloadMessagesWithBotAsync(TgDownloadSettingsViewModel settings, TgParseHelper parseHelper, CancellationToken ct)
    {
        if (CheckShouldStop(ct)) return;
        if (parseHelper.AppDto is null || !parseHelper.AppDto.UseBot) return;

        if (Bot is null || !settings.SourceVm.Dto.IsDownload || (Bot.Client is not null && Bot.Client.Disconnected))
        {
            //if (!CheckShouldStop(ct))
            //    settings.SourceVm.Dto.FirstId = parseHelper.SourceLastId;
            settings.SourceVm.Dto.IsDownload = false;
            parseHelper.DownloadTasks.Clear();
            return;
        }

        await TryCatchAsync(async () =>
        {
            if (Bot is null || parseHelper.BotChatFullInfo is null) return;
            var messages = await Bot.GetMessagesById(parseHelper.BotChatFullInfo, [parseHelper.SourceFirstId]);
            await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(parseHelper.SourceLastId, parseHelper.SourceFirstId):#00.00} %");

            if (CheckShouldStop(ct)) return;

            foreach (var message in messages.AsReadOnly())
            {
                if (CheckShouldStop(ct)) break;

                // Check message exists
                if (message.TLMessage is MessageBase messageBase && message.Date > DateTime.MinValue)
                    parseHelper.DownloadTasks.Add(ParseChatMessageAsync(settings, messageBase, parseHelper.ChatCache, parseHelper.MessageSettings, parseHelper.ForumTopicSettings, ct));
            }

            if (CheckShouldStop(ct) || Bot.Client is null) return;

            // Call Telegram API with cancellation support
            var messages2 = parseHelper.Channel is not null
                ? await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyMessage(parseHelper.Channel.id, parseHelper.SourceFirstId),
                    factory: SafeFactory<TL.Messages_MessagesBase?>(async (ctx, innerCt) => await TelegramCallAsync(
                        apiCt => Bot.Client.Channels_GetMessages(parseHelper.Channel, parseHelper.SourceFirstId), isThrow: false, innerCt)),
                    TgCacheUtils.CacheOptionsChannelMessages, ct)
                : await TelegramCallAsync(apiCt => Bot.Client.GetMessages(settings.Chat.Base, parseHelper.SourceFirstId), isThrow: false, ct);

            await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(parseHelper.SourceLastId, parseHelper.SourceFirstId):#00.00} %");
            if (messages2 is not null)
            {
                foreach (var message in messages2.Messages)
                {
                    if (CheckShouldStop(ct)) break;

                    if (message is MessageBase messageBase && messageBase.Date > DateTime.MinValue)
                        parseHelper.DownloadTasks.Add(ParseChatMessageAsync(settings, messageBase, parseHelper.ChatCache, parseHelper.MessageSettings,
                            parseHelper.ForumTopicSettings, ct));
                }
            }
        }, ct: ct);
    }

    /// <summary> Mark storage entity as deleted </summary>
    private async Task CheckDeletedMessageAndMarkEntityAsync(long chatId, int messageId)
    {
        var storageResult = await StorageManager.MessageRepository.GetByDtoAsync(new() { SourceId = chatId, Id = messageId }, isReadOnly: false);

        if (storageResult.IsExists && storageResult.Item is { } messageEntity && !messageEntity.IsDeleted)
        {
            messageEntity.IsDeleted = true;

            await StorageManager.MessageRepository.SaveAsync(messageEntity);
        }
    }

    /// <summary> Request to cancel current download session </summary>
    public async Task SetForceStopDownloadingAsync()
    {
        try
        {
            // Signal cancellation to all awaiting operations
            _downloadCts?.Cancel();
            _logCts?.Cancel();

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // Log cancellation error
            TgLogUtils.WriteException(ex);
        }
    }

    public async Task MarkHistoryReadAsync(CancellationToken ct = default)
    {
        var messageSettings = new TgMessageSettings();
        await TryCatchAsync(async () =>
        {
            await LoginUserAsync(isProxyUpdate: false);
        }, ct: ct);

        await CollectAllChatsAsync(ct);
        await UpdateStateMessageAsync("Mark as read all message in the channels: in the progress");
        await TryCatchAsync(async () =>
        {
            if (Client is not null)
            {
                foreach (var chatBase in EnumerableChats)
                {
                    await TryCatchAsync(async () =>
                    {
                        // Call Telegram API with cancellation support
                        var isSuccess = await TelegramCallAsync(apiCt => Client.ReadHistory(chatBase), messageSettings.IsThrow, ct);
                        await UpdateStateMessageAsync(
                            $"Mark as read the source | {chatBase.ID} | " +
                            $"{(string.IsNullOrEmpty(chatBase.MainUsername) ? chatBase.Title : chatBase.MainUsername)}]: {(isSuccess ? "success" : "already read")}");
                    }, ct: ct);
                }
            }
            else
            {
                await UpdateStateMessageAsync("Mark as read all messages: Client is not connected!");
            }
        }, ct: ct);
        await UpdateStateMessageAsync("Mark as read all message in the channels: complete");
    }

    private async Task<bool> CreateDestinationDirectoryIfNotExistsAsync(ITgDownloadViewModel tgDownloadSettings)
    {
        if (tgDownloadSettings is not TgDownloadSettingsViewModel tgDownloadSettings2) return false;
        try
        {
            Directory.CreateDirectory(tgDownloadSettings2.SourceVm.Dto.Directory);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            await SetClientExceptionAsync(ex);
            return false;
        }
        return true;
    }

    /// <summary> Parse Telegram chat message </summary>
    private async Task ParseChatMessageAsync(TgDownloadSettingsViewModel tgDownloadSettings, MessageBase messageBase,
        TgChatCache chatCache, TgMessageSettings messageSettings, TgForumTopicSettings forumTopicSettings, CancellationToken ct = default)
    {
        // Check cancellation
        if (CheckShouldStop(ct)) return;

        if (messageBase is not Message message)
        {
            await UpdateChatViewModelAsync(messageSettings.CurrentChatId, messageBase.ID, tgDownloadSettings.SourceVm.Dto.Count, "Empty message");
            return;
        }

        await TryCatchAsync(async () =>
        {
            messageSettings.ThreadNumber++;
            messageSettings.CurrentChatId = messageBase.Peer.ID;
            messageSettings.CurrentMessageId = messageBase.ID;
            var rootSettings = messageSettings.Clone();

            if (CheckShouldStop(ct)) return;

            // Get chat from Cache
            if (chatCache.TryGetChat(messageSettings.CurrentChatId, out var accessHash) &&
                (messageSettings.ParentMessageId == 0 || messageSettings.ParentMessageId != messageSettings.CurrentMessageId))
            {
                // Parse Telegram chat message core logic
                await ParseChatMessageCoreAsync(tgDownloadSettings, messageBase, chatCache, forumTopicSettings, message, rootSettings, ct);

                if (CheckShouldStop(ct)) return;

                var inputPeer = GetInputPeer(messageBase.Peer, accessHash);
                if (inputPeer is not null)
                {
                    // Get discussion message (main post)
                    if (tgDownloadSettings.SourceVm.Dto.IsParsingComments && message.replies?.replies > 0)
                    {
                        // Call Telegram API with cancellation support
                        var discussionMessage = await TelegramCallAsync(apiCt => Client.Messages_GetDiscussionMessage(inputPeer, messageSettings.CurrentMessageId),
                            messageSettings.IsThrow, ct);
                        if (discussionMessage is not null && discussionMessage.messages is not null && discussionMessage.messages.Length > 0)
                        {
                            var stop = false;

                            // Looking for a discussion supergroup in chats
                            TL.Channel? discussionChat = null;

                            // First, let's try to select group from the chat set
                            if (discussionMessage.chats is not null)
                                discussionChat = discussionMessage.chats.Values.OfType<TL.Channel>().FirstOrDefault(c => c.IsGroup);

                            // Fallback: if group is not found, we will try to take linked_chat_id from the source channel
                            if (discussionChat is null && messageBase.Peer is TL.PeerChannel srcPc)
                                discussionChat = discussionMessage.chats?.Values.OfType<TL.Channel>().FirstOrDefault(c => c.ID != srcPc.ID && c.IsGroup);

                            // As a last resort — request by access_hash
                            if (discussionChat is null && inputPeer is InputPeerChannel ipc)
                            {
                                // Call Telegram API with cancellation support
                                var channels = await TelegramCallAsync(apiCt =>
                                    Client.Channels_GetChannels(new TL.InputChannel(ipc.ID, ipc.access_hash)), messageSettings.IsThrow, ct);
                                discussionChat = channels?.chats?.Values.OfType<TL.Channel>().FirstOrDefault(c => c.IsGroup);
                            }
                            if (discussionChat is null) return;

                            // Cache and save the access_hash of the supergroup
                            CheckEnumerableChatCache(discussionChat);
                            await SaveAtStorageAccessHashForChatAsync(chatCache, discussionChat, parentChatId: messageSettings.CurrentChatId);

                            // Building peer supergroups
                            if (!chatCache.TryGetChat(discussionChat.ID, out var discussionHash)) return;

                            var discussionPeer = GetInputPeer(new TL.PeerChannel { channel_id = discussionChat.ID }, discussionHash);
                            if (discussionPeer is null) return;

                            // Find the `thread head` in the messages (specifically, the message from the supergroup).
                            var headMessage = discussionMessage.messages.OfType<Message>().FirstOrDefault(m => m.Peer is TL.PeerChannel pc && pc.ID == discussionChat.ID);
                            if (headMessage is null) return;

                            // Save the `thread header` itself as plain text (optional)
                            var authorId = headMessage.From?.ID ?? messageSettings.CurrentChatId;
                            if (!CheckShouldStop(ct))
                                await SaveMessagesAsync(tgDownloadSettings, rootSettings, headMessage.Date, size: 0,
                                    headMessage.message, TgEnumMessageType.Message, isRetry: false, authorId, ct);

                            // Paginate replies to the thread strictly by (discussionPeer, headMessage.ID)
                            int offsetId = 0;
                            const int limit = 100;

                            while (true)
                            {
                                if (CheckShouldStop(ct)) { stop = true; break; }

                                // Call Telegram API with cancellation support
                                var repliesBase = await TelegramCallAsync(apiCt => Client.Messages_GetReplies(peer: discussionPeer,
                                    msg_id: headMessage.ID,         // IMPORTANT: ID from the supergroup
                                    offset_id: offsetId, offset_date: default, add_offset: 0, limit: limit, max_id: 0, min_id: 0, hash: 0L), messageSettings.IsThrow, ct);
                                if (repliesBase is null || repliesBase.Count == 0) break;

                                foreach (var replyMessage in repliesBase.Messages)
                                {
                                    if (CheckShouldStop(ct)) { stop = true; break; }

                                    if (replyMessage is Message rpl)
                                    {
                                        var replySettings = messageSettings.Clone();
                                        replySettings.ParentChatId = replySettings.CurrentChatId;
                                        replySettings.ParentMessageId = replySettings.CurrentMessageId;
                                        replySettings.CurrentChatId = replyMessage.Peer.ID; // will be the ID of the supergroup
                                        replySettings.CurrentMessageId = replyMessage.ID;
                                        await ParseChatMessageCoreAsync(tgDownloadSettings, rpl, chatCache, forumTopicSettings, rpl, replySettings, ct);
                                    }
                                }

                                offsetId = repliesBase.Messages.Last().ID;
                                if (repliesBase.Messages.Length < limit) break;
                            }

                            if (stop) return;
                            //await ParseChatMessageToDoAsync();
                        }
                    }
                }
            }
        }, ct: ct);
    }

    // TODO: fix here
    //private async Task ParseChatMessageToDoAsync()
    //{
    //foreach (var rootMessage in discussionMessage.messages)
    //{
    //    // Check force stop parsing
    //    if (CheckShouldStop) { stop = true; break; }
    //    // Add chat from Storage to Cache
    //    await AddChatFromStorageToCacheAsync(chatCache, rootMessage.TL.Peer.ID);
    //    // Check the exists chat directory
    //    await CheckExistsChatWithDirectoryAsync(chatCache, messageSettings, accessHash, rootMessage);
    //    // Get chat from Cache
    //    if (chatCache.TryGetChat(rootMessage.TL.Peer.ID, out var discussionHash))
    //    {
    //        var discussionPeer = GetInputPeer(rootMessage.TL.Peer, discussionHash);
    //        if (discussionPeer == null) return;

    //        TL.Channel? discussionChannel = null;
    //        if (discussionMessage.chats is not null && 
    //            discussionMessage.chats.TryGetValue(rootMessage.TL.Peer.ID, out var chatBase) && chatBase is TL.Channel ch)
    //        {
    //            discussionChannel = ch;
    //        }
    //        else
    //        {
    //            // fallback: get channel via API
    //            if (discussionPeer is InputPeerChannel ipc)
    //            {
    // Call Telegram API with cancellation support
    //                var channels = await TelegramCallAsync(() => Client.Channels_GetChannels(
    //                    new TL.InputChannel(ipc.ID, ipc.access_hash)), messageSettings.IsThrow);
    //                discussionChannel = channels?.chats?.Values.OfType<TL.Channel>().FirstOrDefault(c => c.ID == rootMessage.TL.Peer.ID);
    //            }
    //        }
    //        if (discussionChannel is not null)
    //        {
    //            CheckEnumerableChatCache(discussionChannel);
    //            await SaveAtStorageAccessHashForChatAsync(chatCache, discussionChannel, parentChatId: messageSettings.CurrentChatId);

    //            // Save plain text message
    //            if (rootMessage is Message msg)
    //                await SaveMessagesAsync(tgDownloadSettings, rootSettings, rootMessage.Date, size: 0,
    //                    msg.message, TgEnumMessageType.Message, isRetry: false, userId: rootMessage.From?.ID ?? 0);

    //            // Try to get replies to this comment
    //            if (rootMessage is MessageBase commentBase)
    //            {
    //                int offsetId = 0;
    //                const int limit = 100;
    //                while (true)
    //                {
    //                    if (CheckShouldStop) { stop = true; break; }

    // Call Telegram API with cancellation support
    //                    var repliesBase = await TelegramCallAsync(() =>
    //                        Client.Messages_GetReplies(discussionPeer, msg_id: commentBase.ID, offset_id: offsetId, 
    //                        offset_date: default, add_offset: 0, limit: limit, max_id: 0, min_id: 0, hash: 0L));

    //                    if (repliesBase is null || repliesBase.Count == 0) break;

    //                    foreach (var replyMessage in repliesBase.Messages)
    //                    {
    //                        if (CheckShouldStop) { stop = true; break; }
    //                        // Save plain text message
    //                        if (replyMessage is Message rpl)
    //                        {
    //                            var replySettings = messageSettings.Clone();
    //                            replySettings.ParentChatId = replySettings.CurrentChatId;
    //                            replySettings.ParentMessageId = replySettings.CurrentMessageId;
    //                            replySettings.CurrentChatId = replyMessage.TL.Peer.ID;
    //                            replySettings.CurrentMessageId = replyMessage.ID;
    //                            // Parse Telegram chat message core logic
    //                            //await ParseChatMessageCoreAsync(tgDownloadSettings, rpl, chatCache, forumTopicSettings, message, replySettings);
    //                            await ParseChatMessageCoreAsync(tgDownloadSettings, rpl, chatCache, forumTopicSettings, rpl, replySettings);
    //                        }
    //                    }

    //                    offsetId = repliesBase.Messages.Last().ID;
    //                    if (repliesBase.Messages.Length < limit)
    //                        break;
    //                }
    //            }
    //        }
    //    }
    //}
    //    await Task.CompletedTask;
    //}

    ///// <summary> Check the exists chat directory </summary>
    //private async Task CheckExistsChatWithDirectoryAsync(TgChatCache chatCache, TgMessageSettings messageSettings, MessageBase rootMessage)
    //{
    //    if (chatCache.CheckExistsDirectory(rootMessage.Peer.ID)) return;

    //    // Check if the chat is already in the enumerable collections
    //    var channelFind = EnumerableChannels.FirstOrDefault(x => x.ID == rootMessage.Peer.ID);
    //    if (channelFind is not null)
    //    {
    //        CheckEnumerableChatCache(channelFind);
    //        await SaveAtStorageAccessHashForChatAsync(chatCache, channelFind, messageSettings.CurrentChatId);
    //        return;
    //    }

    //    // Check if the chat is already in the enumerable groups
    //    var groupFind = EnumerableGroups.FirstOrDefault(x => x.ID == rootMessage.Peer.ID);
    //    if (groupFind is not null)
    //    {
    //        CheckEnumerableChatCache(groupFind);
    //        await SaveAtStorageAccessHashForChatAsync(chatCache, groupFind, messageSettings.CurrentChatId);
    //    }
    //}

    /// <summary> Parse Telegram chat message core logic </summary>
    private async Task ParseChatMessageCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings, MessageBase messageBase, TgChatCache chatCache,
        TgForumTopicSettings forumTopicSettings, Message message, TgMessageSettings messageSettings, CancellationToken ct)
    {
        // Early exit if cancellation requested
        if (CheckShouldStop(ct)) return;

        // Ensure message is processed only once using cache
        _ = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyMessageProcessed(messageSettings.CurrentChatId, messageSettings.CurrentMessageId),
            factory: SafeFactory<bool>(async (ctx, innerCt) =>
            {
                // Merge external token with DownloadToken
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(innerCt, ct);
                var innerToken = linkedCts.Token;

                // Check cancellation before heavy processing
                if (CheckShouldStop(innerToken)) return false;

                await HandleMessageContentAsync(tgDownloadSettings, messageBase, chatCache, forumTopicSettings, message, messageSettings, innerToken);

                return true;
            }), TgCacheUtils.CacheOptionsProcessMessage, ct);

        // Check cancellation before updating UI/state
        if (CheckShouldStop(ct)) return;

        var totalMessages = await TryGetCacheChatLastCountSafeAsync(messageSettings.CurrentChatId, ct);

        // Update chat view model with progress
        await UpdateChatViewModelAsync(messageSettings.CurrentChatId, messageSettings.CurrentMessageId, totalMessages,
            $"Reading message {messageSettings.CurrentMessageId} of {totalMessages}");
    }

    private async Task HandleMessageContentAsync(TgDownloadSettingsViewModel tgDownloadSettings, MessageBase messageBase, TgChatCache chatCache,
        TgForumTopicSettings forumTopicSettings, Message message, TgMessageSettings messageSettings, CancellationToken ct)
    {
        // Early exit if cancellation requested
        if (CheckShouldStop(ct)) return;

        // Parse documents and photos
        if ((message.flags & Message.Flags.has_media) != 0)
        {
            await DownloadMediaFromMessageAsync(tgDownloadSettings, messageBase, message.media, chatCache, messageSettings, forumTopicSettings, ct);

            // Check cancellation after media download
            if (CheckShouldStop(ct)) return;
        }
        // Save plain text message
        else
        {
            // Check cancellation before saving
            if (CheckShouldStop(ct)) return;

            var authorId = message.From?.ID ?? messageSettings.CurrentChatId;
            await SaveMessagesAsync(tgDownloadSettings, messageSettings, message.Date, size: 0, message.message, TgEnumMessageType.Message, isRetry: false, authorId, ct);

            // Check cancellation after saving
            if (CheckShouldStop(ct)) return;
        }
    }

    private async Task AddChatFromStorageToCacheAsync(TgChatCache chatCache, long chatId)
    {
        var needUpdate = false;

        if (!chatCache.TryGetChat(chatId, out _))
        {
            needUpdate = true;
        }
        else
        {
            var directory = chatCache.GetDirectory(chatId);
            if (string.IsNullOrEmpty(directory))
            {
                needUpdate = true;
            }
        }

        if (needUpdate)
        {
            var chatResult = await StorageManager.SourceRepository.GetByDtoAsync(new() { Id = chatId });
            if (chatResult is { IsExists: true, Item: { } chatEntity })
            {
                if (!chatCache.TryGetChat(chatId, out _))
                    chatCache.TryAddChat(chatEntity.Id, chatEntity.AccessHash, chatEntity.Directory ?? string.Empty);
                else
                    chatCache.TryUpdateChat(chatEntity.Id, chatEntity.AccessHash, false, chatEntity.Directory ?? string.Empty);
            }
        }
    }

    /// <summary> Check if the chat is already in the enumerable collections and add it if not </summary>
    private void CheckEnumerableChatCache(TL.ChatBase chatBase)
    {
        if (!EnumerableChats.Contains(chatBase))
            EnumerableChats.Enqueue(chatBase);

        if (chatBase is TL.Channel channel)
        {
            if (!channel.IsGroup && !EnumerableChannels.Contains(chatBase))
                EnumerableChannels.Enqueue(channel);

            if (channel.IsGroup && !EnumerableGroups.Contains(chatBase))
                EnumerableGroups.Enqueue(channel);
        }
    }

    /// <summary> Save access hash for chat if it is not saved or does not have an access hash </summary>
    private async Task SaveAtStorageAccessHashForChatAsync(TgChatCache chatCache, TL.Channel channel, long parentChatId)
    {
        if (channel is null) return;
        if (chatCache.IsSaved(channel.ID) && chatCache.CheckExistsDirectory(channel.ID)) return;

        var chatResult = await StorageManager.SourceRepository.GetByDtoAsync(new() { Id = channel.ID });

        if (chatResult.IsExists && chatResult.Item is TgEfSourceEntity chatEntity)
        {
            // AccessHash must be the hash of the discussion channel itself.
            if (chatEntity.AccessHash != channel.access_hash)
            {
                chatEntity.DtChanged = DateTime.UtcNow;
                chatEntity.AccessHash = channel.access_hash;
                await StorageManager.SourceRepository.SaveAsync(chatEntity);
            }
            _ = await SetAndCreateChatDirectoryIfNotExists(channel, parentChatId, chatEntity, isNew: false);
        }
        else
        {
            // The ID of the new entity = channel.ID, not parentChatId
            var newChatEntity = new TgEfSourceEntity
            {
                DtChanged = DateTime.UtcNow,
                Id = channel.ID,
                AccessHash = channel.access_hash,
                UserName = channel.username,
                Title = channel.title,
                IsUserAccess = true,
                IsSubscribe = false
            };
            _ = await SetAndCreateChatDirectoryIfNotExists(channel, parentChatId, newChatEntity, isNew: true);
        }

        chatCache.TryAddChat(channel.ID, channel.access_hash, string.Empty);
        chatCache.MarkAsSaved(channel.ID);
    }

    /// <summary> Set directory for chat and create it if it does not exist </summary>
    private async Task<string> SetAndCreateChatDirectoryIfNotExists(TL.Channel channel, long parentChatId,
        TgEfSourceEntity chatEntity, bool isNew)
    {
        var isChanged = false;

        bool IsValidDirectory(string? path) => !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);

        string GetSafeFolderName(long id, string username)
        {
            var rawName = string.IsNullOrWhiteSpace(username) ? $"{id}" : $"{id} {username}";
            var invalidChars = Path.GetInvalidFileNameChars();
            return new string([.. rawName.Where(c => !invalidChars.Contains(c))]);
        }

        string GetSiblingDirectory(string parentDirectory, long id, string username, string title)
        {
            var parentDirInfo = new DirectoryInfo(parentDirectory);
            var safeFolderName = !string.IsNullOrWhiteSpace(username) ? GetSafeFolderName(id, username) : GetSafeFolderName(id, title);
            var siblingPath = Path.Combine(parentDirInfo.Parent?.FullName ?? parentDirectory, safeFolderName);
            return siblingPath;
        }

        if (string.IsNullOrWhiteSpace(chatEntity.Directory) || !Directory.Exists(chatEntity.Directory))
        {
            var parentChatResult = await StorageManager.SourceRepository.GetByDtoAsync(new() { Id = parentChatId });
            if (parentChatResult.IsExists && parentChatResult.Item is TgEfSourceEntity parentEntity && IsValidDirectory(parentEntity.Directory))
            {
                var siblingDirectory = GetSiblingDirectory(parentEntity.Directory ?? string.Empty, channel.id, channel.username, channel.title);
                if (!string.IsNullOrWhiteSpace(siblingDirectory) && !Directory.Exists(siblingDirectory))
                {
                    try
                    {
                        if (!Directory.Exists(siblingDirectory))
                            Directory.CreateDirectory(siblingDirectory);
                    }
                    catch (Exception ex)
                    {
                        TgLogUtils.WriteException(ex);
                        await SetClientExceptionAsync(ex);
                    }
                }

                chatEntity.Directory = siblingDirectory;
                isChanged = true;
            }
        }

        if (isChanged || isNew)
            await StorageManager.SourceRepository.SaveAsync(chatEntity);

        return chatEntity.Directory ?? string.Empty;
    }

    /// <summary> Get input TL.Peer </summary>
    private static InputPeer? GetInputPeer(TL.Peer peer, long accessHash)
    {
        InputPeer? inputPeer = null;
        if (peer is TL.PeerUser peerUser)
            inputPeer = new InputPeerUser(peerUser.ID, accessHash);
        else if (peer is TL.PeerChat peerChat)
            inputPeer = new InputPeerChat(peerChat.ID);
        else if (peer is TL.PeerChannel peerChannel)
            inputPeer = new InputPeerChannel(peerChannel.ID, accessHash);
        return inputPeer;
    }

    private TgMediaInfoModel GetMediaInfo(TL.MessageMedia messageMedia, TgDownloadSettingsViewModel tgDownloadSettings, MessageBase messageBase,
        TgChatCache chatCache, TgMessageSettings messageSettings, TgForumTopicSettings forumTopicSettings)
    {
        var extensionName = string.Empty;
        TgMediaInfoModel? mediaInfo = null;
        switch (messageMedia)
        {
            case MessageMediaDocument mediaDocument:
                if ((mediaDocument.flags & MessageMediaDocument.Flags.has_document) is not 0 && mediaDocument.document is Document document)
                {
                    if (!string.IsNullOrEmpty(document.Filename) && Path.GetExtension(document.Filename).TrimStart('.') is { } str)
                        extensionName = str;
                    string fileName;
                    if (tgDownloadSettings.SourceVm.Dto.IsFileNamingByMessage && messageBase is Message msg)
                    {
                        // Build from message text without duplicating extension
                        var baseFromMessage = TgStringUtils.SanitizeForFileName(msg.message);
                        fileName = TgStringUtils.EnsureSingleExtension(baseFromMessage, extensionName);
                    }
                    else
                    {
                        // Keep original filename as-is
                        fileName = document.Filename;
                    }

                    if (!string.IsNullOrEmpty(document.Filename) && CheckFileAtFilter(document.Filename, extensionName, document.size))
                    {
                        mediaInfo = new(fileName, document.size, document.date);
                        break;
                    }
                    if (document.attributes.Length > 0)
                    {
                        // Video document attribute branch
                        if (document.attributes.Any(x => x is DocumentAttributeVideo))
                        {
                            extensionName = "mp4";
                            if (CheckFileAtFilter(string.Empty, extensionName, document.size))
                            {
                                var baseName = document.ID.ToString();
                                var safeName = TgStringUtils.EnsureSingleExtension(baseName, extensionName);
                                mediaInfo = new(safeName, document.size, document.date);
                                break;
                            }
                        }
                        // Audio document attribute branch
                        if (document.attributes.Any(x => x is DocumentAttributeAudio))
                        {
                            extensionName = "mp3";
                            if (CheckFileAtFilter(string.Empty, extensionName, document.size))
                            {
                                var baseName = document.ID.ToString();
                                var safeName = TgStringUtils.EnsureSingleExtension(baseName, extensionName);
                                mediaInfo = new(safeName, document.size, document.date);
                                break;
                            }
                        }
                    }

                    // Fallback when document.Filename is empty
                    if (string.IsNullOrEmpty(document.Filename) && CheckFileAtFilter(string.Empty, extensionName, document.size))
                    {
                        var baseName = messageBase.ID.ToString();
                        var safeName = TgStringUtils.EnsureSingleExtension(baseName, extensionName);
                        mediaInfo = new(safeName, document.size, document.date);
                        break;
                    }
                }
                break;
            case MessageMediaPhoto mediaPhoto:
                if (mediaPhoto is { photo: Photo photo })
                {
                    extensionName = "jpg";
                    var baseName = photo.ID.ToString();
                    var fileName = TgStringUtils.EnsureSingleExtension(baseName, extensionName);
                    if (CheckFileAtFilter(fileName, extensionName, photo.sizes.Last().FileSize))
                    {
                        mediaInfo = new(fileName, photo.sizes.Last().FileSize, photo.date);
                        break;
                    }
                }
                break;
        }

        mediaInfo ??= new();
        // Join ID
        if (!string.IsNullOrEmpty(mediaInfo.LocalNameOnly))
            mediaInfo.Number = tgDownloadSettings.SourceVm.Dto.Count switch
            {
                < 1000 => $"{messageBase.ID:000}",
                < 10000 => $"{messageBase.ID:0000}",
                < 100000 => $"{messageBase.ID:00000}",
                < 1000000 => $"{messageBase.ID:000000}",
                < 10000000 => $"{messageBase.ID:0000000}",
                < 100000000 => $"{messageBase.ID:00000000}",
                < 1000000000 => $"{messageBase.ID:000000000}",
                _ => $"{messageBase.ID}"
            };

        mediaInfo.LocalPathOnly = chatCache.GetDirectory(messageSettings.CurrentChatId);
        // Creating subdirectories
        if (!string.IsNullOrEmpty(mediaInfo.RemoteName) &&
            tgDownloadSettings.SourceVm.Dto.IsCreatingSubdirectories && forumTopicSettings.Topics is not null)
        {
            var topicId = messageBase.ReplyHeader?.TopicID ?? 0;
            if (topicId > 0)
            {
                var topic = forumTopicSettings.Topics.SingleOrDefault(x => x.ID == topicId);
                mediaInfo.LocalPathOnly = Path.Combine(chatCache.GetDirectory(messageSettings.CurrentChatId), topic?.Title ?? string.Empty);
            }
            else
            {
                if (forumTopicSettings.RootTopic is not null)
                {
                    mediaInfo.LocalPathOnly = Path.Combine(chatCache.GetDirectory(messageSettings.CurrentChatId), forumTopicSettings.RootTopic?.Title ?? string.Empty);
                }
            }
        }
        mediaInfo.Normalize(tgDownloadSettings.IsJoinFileNameWithMessageId);
        return mediaInfo;
    }

    public bool CheckFileAtFilter(string fileName, string extensionName, long size)
    {
        foreach (var filter in Filters)
        {
            if (!filter.IsEnabled)
                continue;
            switch (filter.FilterType)
            {
                case TgEnumFilterType.SingleName:
                    if (string.IsNullOrEmpty(fileName))
                        continue;
                    if (!TgDataFormatUtils.CheckFileAtMask(fileName, filter.Mask))
                        return false;
                    break;
                case TgEnumFilterType.SingleExtension:
                    if (string.IsNullOrEmpty(extensionName))
                        continue;
                    if (!TgDataFormatUtils.CheckFileAtMask(extensionName, filter.Mask))
                        return false;
                    break;
                case TgEnumFilterType.MultiName:
                    if (string.IsNullOrEmpty(fileName))
                        continue;
                    var isMultiName = filter.Mask.Split(',')
                        .Any(mask => TgDataFormatUtils.CheckFileAtMask(fileName, mask.Trim()));
                    if (!isMultiName)
                        return false;
                    break;
                case TgEnumFilterType.MultiExtension:
                    if (string.IsNullOrEmpty(extensionName))
                        continue;
                    var isMultiExtension = filter.Mask.Split(',')
                        .Any(mask => TgDataFormatUtils.CheckFileAtMask(extensionName, mask.Trim()));
                    if (!isMultiExtension)
                        return false;
                    break;
                case TgEnumFilterType.MinSize:
                    if (size < filter.SizeAtBytes)
                        return false;
                    break;
                case TgEnumFilterType.MaxSize:
                    if (size > filter.SizeAtBytes)
                        return false;
                    break;
            }
        }
        return true;
    }

    /// <summary> Parse the message and download media files if they exist </summary>
    private async Task DownloadMediaFromMessageAsync(TgDownloadSettingsViewModel tgDownloadSettings, MessageBase messageBase, TL.MessageMedia messageMedia,
        TgChatCache chatCache, TgMessageSettings messageSettings, TgForumTopicSettings forumTopicSettings, CancellationToken ct)
    {
        if (CheckShouldStop(ct)) return;

        // Add chat from Storage to Cache
        await AddChatFromStorageToCacheAsync(chatCache, messageSettings.CurrentChatId);
        var mediaInfo = GetMediaInfo(messageMedia, tgDownloadSettings, messageBase, chatCache, messageSettings, forumTopicSettings);
        if (string.IsNullOrEmpty(mediaInfo.LocalNameOnly)) return;

        await MoveExistsFilesAtCurrentDirAsync(tgDownloadSettings, mediaInfo, ct);

        if (Client is null && Bot is not null)
            Client = Bot.Client;

        await DeleteEmptyOrLessSizeFilesAsync(tgDownloadSettings, mediaInfo, ct);

        // Download media thumbnail if exists
        await DownloadMediaThumbnailFromMessageAsync(tgDownloadSettings, messageMedia, mediaInfo, ct);

        // Download media file if exists
        await DownloadMediaFileFromMessageAsync(tgDownloadSettings, messageBase, messageMedia, messageSettings, mediaInfo, ct);

        // Save message
        var authorId = messageBase.From?.ID ?? messageSettings.CurrentChatId;
        switch (messageMedia)
        {
            case MessageMediaDocument { document: Document document }:
                {
                    TgEfDocumentEntity doc = new()
                    {
                        Id = document.ID,
                        SourceId = tgDownloadSettings.SourceVm.Dto.Id,
                        MessageId = messageBase.ID,
                        FileName = mediaInfo.LocalPathWithNumber,
                        FileSize = mediaInfo.RemoteSize,
                        AccessHash = document.access_hash
                    };
                    await StorageManager.DocumentRepository.SaveAsync(doc, ct: ct);
                }
                await SaveMessagesAsync(tgDownloadSettings, messageSettings, mediaInfo.DtCreate, mediaInfo.RemoteSize, mediaInfo.RemoteName,
                    TgEnumMessageType.Document, isRetry: false, authorId, ct);
                break;

            case MessageMediaPhoto { photo: Photo photo }:
                var messageStr = string.Empty;
                if (messageBase is TL.Message message)
                    messageStr = message.message;
                await SaveMessagesAsync(tgDownloadSettings, messageSettings, mediaInfo.DtCreate, mediaInfo.RemoteSize,
                    $"{messageStr} | {mediaInfo.LocalPathWithNumber.Replace(tgDownloadSettings.SourceVm.Dto.Directory, string.Empty)}",
                    TgEnumMessageType.Photo, isRetry: false, authorId, ct);
                break;
        }

        // Update date time for files
        UpdateFilesDateTime(mediaInfo);
    }

    /// <summary> Download media thumbnail if exists </summary>
    private async Task DownloadMediaThumbnailFromMessageAsync(TgDownloadSettingsViewModel tgDownloadSettings, MessageMedia messageMedia, TgMediaInfoModel mediaInfo, CancellationToken ct)
    {
        if (Directory.Exists(mediaInfo.LocalPathOnly) && !File.Exists(mediaInfo.ThumbnailPathWithNumber))
        {
            if (Client is not null)
            {
                switch (messageMedia)
                {
                    case MessageMediaDocument { document: Document doc }:
                        {
                            // Select best quality thumbnail
                            var thumb = doc.thumbs?.OfType<PhotoSizeBase>().OrderByDescending(x => x.FileSize).FirstOrDefault();
                            if (thumb is not null)
                            {
                                await DeleteEmptyOrLessSizeThumbFilesAsync(tgDownloadSettings, mediaInfo, ct);
                                await using var localFileStream = File.Create(mediaInfo.ThumbnailPathWithNumber);
                                // Call Telegram API with cancellation support
                                await TelegramCallAsync(apiCt => Client.DownloadFileAsync(doc, localFileStream, thumb), isThrow: false, ct);
                                localFileStream.Close();
                            }
                        }
                        break;
                }
            }
        }
    }

    /// <summary> Download media file if exists </summary>
    private async Task DownloadMediaFileFromMessageAsync(TgDownloadSettingsViewModel tgDownloadSettings, MessageBase messageBase, MessageMedia messageMedia, TgMessageSettings messageSettings, TgMediaInfoModel mediaInfo, CancellationToken ct)
    {
        if (Directory.Exists(mediaInfo.LocalPathOnly) && !File.Exists(mediaInfo.LocalPathWithNumber))
        {
            if (Client is not null)
            {
                switch (messageMedia)
                {
                    case MessageMediaDocument { document: Document doc }:
                        {
                            await using var localFileStream = File.Create(mediaInfo.LocalPathWithNumber);
                            // Call Telegram API with cancellation support
                            await TelegramCallAsync(apiCt => Client.DownloadFileAsync(doc, localFileStream, null,
                                ClientProgressForFile(messageSettings, mediaInfo.LocalNameWithNumber)), isThrow: false, ct);
                            var authorId = messageBase.From?.ID ?? messageSettings.CurrentChatId;
                            await SaveMessagesAsync(tgDownloadSettings, messageSettings, mediaInfo.DtCreate, mediaInfo.RemoteSize, mediaInfo.RemoteName,
                                TgEnumMessageType.Document, isRetry: false, authorId, ct);
                            localFileStream.Close();
                        }
                        break;

                    case MessageMediaPhoto { photo: Photo photo }:
                        {
                            // Select best quality photo
                            var thumb = photo.sizes?.OfType<PhotoSizeBase>().OrderByDescending(x => x.FileSize).FirstOrDefault();
                            if (thumb is not null)
                            {
                                await DeleteEmptyOrLessSizeThumbFilesAsync(tgDownloadSettings, mediaInfo, ct);
                                await using var localFileStream = File.Create(mediaInfo.LocalPathWithNumber);
                                // Call Telegram API with cancellation support
                                await TelegramCallAsync(apiCt => Client.DownloadFileAsync(photo, localFileStream, thumb), isThrow: false, ct);
                                localFileStream.Close();
                            }
                        }
                        break;
                }
            }
        }
    }

    /// <summary> Delete empty or less size files </summary>
    private async Task DeleteEmptyOrLessSizeFilesAsync(TgDownloadSettingsViewModel tgDownloadSettings, TgMediaInfoModel mediaInfo, CancellationToken ct)
    {
        await TryCatchAsync(async () =>
        {
            if (File.Exists(mediaInfo.LocalPathWithNumber))
            {
                var fileSize = TgFileUtils.CalculateFileSize(mediaInfo.LocalPathWithNumber);
                // If file size is less then original size
                if (fileSize == 0 || (fileSize < mediaInfo.RemoteSize && tgDownloadSettings.IsRewriteFiles))
                    File.Delete(mediaInfo.LocalPathWithNumber);
            }
            await Task.CompletedTask;
        }, ct: ct);
    }

    /// <summary> Delete empty or less size files </summary>
    private async Task DeleteEmptyOrLessSizeThumbFilesAsync(TgDownloadSettingsViewModel tgDownloadSettings, TgMediaInfoModel mediaInfo, CancellationToken ct)
    {
        await TryCatchAsync(async () =>
        {
            if (File.Exists(mediaInfo.ThumbnailPathWithNumber))
            {
                var fileSize = TgFileUtils.CalculateFileSize(mediaInfo.ThumbnailPathWithNumber);
                // If file size is less then original size
                if (fileSize == 0)
                    File.Delete(mediaInfo.ThumbnailPathWithNumber);
            }
            await Task.CompletedTask;
        }, ct: ct);
    }

    /// <summary> Move existing files at the current directory </summary>
    private async Task MoveExistsFilesAtCurrentDirAsync(TgDownloadSettingsViewModel tgDownloadSettings, TgMediaInfoModel mediaInfo, CancellationToken ct)
    {
        if (!tgDownloadSettings.IsJoinFileNameWithMessageId) return;

        await TryCatchAsync(async () =>
        {
            // File is already exists and size is correct
            var currentFileName = mediaInfo.LocalPathWithNumber;
            var fileSize = TgFileUtils.CalculateFileSize(currentFileName);
            if (File.Exists(currentFileName) && fileSize == mediaInfo.RemoteSize)
                return;
            // Other existing files
            var files = Directory.GetFiles(mediaInfo.LocalPathOnly, mediaInfo.LocalNameOnly).ToList();
            if (files.Count == 0)
                return;
            foreach (var file in files)
            {
                fileSize = TgFileUtils.CalculateFileSize(file);
                // Find other file with name and size
                if (fileSize == mediaInfo.RemoteSize)
                    File.Move(file, mediaInfo.LocalPathWithNumber, overwrite: true);
            }
            await Task.CompletedTask;
        }, ct: ct);
    }

    /// <summary> Update date time for files </summary>
    private static void UpdateFilesDateTime(TgMediaInfoModel mediaInfo)
    {
        if (File.Exists(mediaInfo.ThumbnailPathWithNumber))
        {
            File.SetCreationTimeUtc(mediaInfo.ThumbnailPathWithNumber, mediaInfo.DtCreate);
            File.SetLastAccessTimeUtc(mediaInfo.ThumbnailPathWithNumber, mediaInfo.DtCreate);
            File.SetLastWriteTimeUtc(mediaInfo.ThumbnailPathWithNumber, mediaInfo.DtCreate);
        }

        if (File.Exists(mediaInfo.LocalPathWithNumber))
        {
            File.SetCreationTimeUtc(mediaInfo.LocalPathWithNumber, mediaInfo.DtCreate);
            File.SetLastAccessTimeUtc(mediaInfo.LocalPathWithNumber, mediaInfo.DtCreate);
            File.SetLastWriteTimeUtc(mediaInfo.LocalPathWithNumber, mediaInfo.DtCreate);
        }
    }

    /// <summary> Save messages to the storage </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task SaveMessagesAsync(TgDownloadSettingsViewModel tgDownloadSettings, TgMessageSettings messageSettings,
        DateTime dtCreated, long size, string message, TgEnumMessageType messageType, bool isRetry, long authorId, CancellationToken ct)
    {
        // Early exit if saving is disabled or cancellation requested
        if (!tgDownloadSettings.IsSaveMessages || CheckShouldStop(ct)) return;

        await TgCacheUtils.Locker.WaitAsync(ct);

        try
        {
            // Check cancellation before processing
            if (CheckShouldStop(ct)) return;

            // Save message entity
            if (authorId > 0 && messageSettings.CurrentMessageId > 0 && messageSettings.CurrentChatId > 0)
            {
                var messageItem = new TgEfMessageEntity
                {
                    Id = messageSettings.CurrentMessageId,
                    SourceId = messageSettings.CurrentChatId,
                    DtCreated = dtCreated,
                    Type = messageType,
                    Size = size,
                    Message = message,
                    UserId = authorId,
                };
                // Check if the message is not already in the batch
                if (_messageBuffer.FirstOrDefault(x => x.Id == messageItem.Id && x.SourceId == messageItem.SourceId && x.UserId == messageItem.UserId) is null)
                {
                    _messageBuffer.Add(messageItem);
                }
            }

            // Save message relation entity
            if (messageSettings.ParentChatId > 0 && messageSettings.ParentMessageId > 0 && messageSettings.CurrentChatId > 0 && messageSettings.CurrentMessageId > 0)
            {
                var relation = new TgEfMessageRelationEntity
                {
                    ParentSourceId = messageSettings.ParentChatId,
                    ParentMessageId = messageSettings.ParentMessageId,
                    ChildSourceId = messageSettings.CurrentChatId,
                    ChildMessageId = messageSettings.CurrentMessageId
                };
                if (_messageRelationBuffer.FirstOrDefault(x => x.ParentSourceId == messageSettings.ParentChatId && x.ParentMessageId == messageSettings.ParentMessageId &&
                    x.ChildSourceId == messageSettings.CurrentChatId && x.ChildMessageId == messageSettings.CurrentMessageId) is null)
                {
                    _messageRelationBuffer.Add(relation);
                }
            }

            if (messageType == TgEnumMessageType.Document)
            {
                if (CheckShouldStop(ct)) return;

                await UpdateStateFileAsync(messageSettings.CurrentChatId, messageSettings.CurrentMessageId, string.Empty, 0, 0, 0, false, messageSettings.ThreadNumber);
            }
        }
        catch (Exception ex)
        {
            if (!isRetry)
            {
                if (CheckShouldStop(ct)) return;

                await Task.Delay(TimeSpan.FromSeconds(FloodControlService.WaitFallbackFast), ct);
                await SaveMessagesAsync(tgDownloadSettings, messageSettings, dtCreated, size, message, messageType, isRetry: true, authorId, ct);
            }
            else
            {
                TgLogUtils.WriteExceptionWithMessage(ex, TgConstants.LogTypeStorage);
                if (!CheckShouldStop(ct))
                    await SetClientExceptionAsync(ex, ct);
            }
        }
        finally
        {
            TgCacheUtils.Locker.Release();

            if (!CheckShouldStop(ct))
            {
                await UpdateStateMessageThreadAsync(
                    messageSettings.CurrentChatId, messageSettings.CurrentMessageId, message, false, messageSettings.ThreadNumber);

                await FlushMessageBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: false, ct);
                await FlushMessageRelationBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: false, ct);
            }
        }
    }

    /// <inheritdoc />
    public bool CheckFileLocked(string filePath)
    {
        var isLocked = false;
        FileStream? fileStream = null;
        try
        {
            fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException)
        {
            isLocked = true;
        }
        finally
        {
            fileStream?.Close();
        }
        return isLocked;
    }

    private Client.ProgressCallback ClientProgressForFile(TgMessageSettings messageSettings, string fileName)
    {
        // Start stopwatch for measuring transfer speed
        var stopwatch = Stopwatch.StartNew();

        // Capture required values to avoid closure over mutable object
        var chatId = messageSettings.CurrentChatId;
        var messageId = messageSettings.CurrentMessageId;
        var threadNumber = messageSettings.ThreadNumber;
        var shortName = Path.GetFileName(fileName);

        return async (transmitted, totalSize) =>
        {
            // Skip if file name is not provided
            if (string.IsNullOrEmpty(shortName)) return;

            // Calculate elapsed time in seconds
            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

            // Calculate speed in bytes per second
            long speed = elapsedSeconds > 0 ? (long)(transmitted / elapsedSeconds) : 0;

            // Update file transfer state
            await UpdateStateFileAsync(chatId, messageId, shortName, totalSize > 0 ? totalSize : 0, transmitted, speed, true, threadNumber);
        };
    }

    private async Task<long> GetPeerIdAsync(string userName, CancellationToken ct = default)
    {
        // Call Telegram API with cancellation support
        return (await TelegramCallAsync(apiCt => Client.Contacts_ResolveUsername(userName), isThrow: false, ct)).peer.ID;
    }

    public virtual async Task LoginUserAsync(bool isProxyUpdate) => await UseOverrideMethodAsync();

    public async Task DisconnectClientAsync(bool isAfterClientConnect = true)
    {
        try
        {
            IsProxyUsage = false;
            await UpdateStateProxyAsync(TgLocale.ProxyIsDisconnect);
            Me = null;
            if (Client is not null)
            {
                Client.OnOther -= OnClientOtherAsync;
                Client.OnOwnUpdates -= OnOwnUpdatesClientAsync;
                Client.OnUpdates -= OnUpdatesClientAsync;
                await Client.DisposeAsync();
                Client = null;
            }
            ClientException = new();
            await CheckClientConnectionReadyAsync();
            if (isAfterClientConnect)
                await AfterClientConnectAsync();
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            await SetClientExceptionAsync(ex);
        }
    }

    public async Task DisconnectBotAsync(bool isAfterClientConnect = true)
    {
        try
        {
            // Clear DTO
            if (BotInfoDto is not null)
            {
                BotInfoDto = null;
            }

            // Ensure the connection is closed only after all operations are done
            if (Bot is not null)
            {
                Bot.OnError -= OnErrorBotAsync;
                Bot.OnMessage -= OnMessageBotAsync;
                Bot.OnUpdate -= OnUpdateBotAsync;
                if (Bot.Client is { Disconnected: false })
                    await Bot.Client.DisposeAsync();
                Bot.Dispose();
            }
            Bot = null;

            // Dispose and nullify the connection only if it's open
            if (BotSqlConnection is not null)
            {
                if (BotSqlConnection.State != System.Data.ConnectionState.Closed)
                    BotSqlConnection.Close();
                BotSqlConnection.Dispose();
                BotSqlConnection = null;
            }

            // Dispose bot logs
            if (BotStreamWriterLogs is not null)
            {
                BotStreamWriterLogs.Close();
                await BotStreamWriterLogs.DisposeAsync();
                BotStreamWriterLogs = null;
            }

            if (isAfterClientConnect)
                await AfterClientConnectAsync();
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            await SetClientExceptionAsync(ex);
        }
    }

    protected async Task SetClientExceptionAsync(Exception ex, CancellationToken ct = default,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        // Early exit if saving is disabled or cancellation requested
        if (CheckShouldStop(ct)) return;

        ClientException.Set(ex);
        await UpdateExceptionAsync(ex);
        await UpdateStateExceptionAsync(TgFileUtils.GetShortFilePath(filePath), lineNumber, memberName, ClientException.Message);
    }

    private async Task SetClientExceptionShortAsync(Exception ex)
    {
        ClientException.Set(ex);
        await UpdateStateExceptionShortAsync(ClientException.Message);
    }

    private async Task SetProxyExceptionAsync(Exception ex,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        ProxyException.Set(ex);
        await UpdateExceptionAsync(ex);
        await UpdateStateExceptionAsync(TgFileUtils.GetShortFilePath(filePath), lineNumber, memberName, ProxyException.Message);
    }

    #endregion

    #region Methods

    private async Task<T?> TryGetCatchAsync<T>(Func<Task<T>> call, Func<Task>? callFinally = null, CancellationToken ct = default,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        try
        {
            for (int attempt = 0; attempt < FloodControlService.MaxRetryCount; attempt++)
            {
                if (CheckShouldStop(ct)) return default;

                try
                {
                    return await call();
                }
                catch (OperationCanceledException)
                {
                    // Return default immediately when cancellation requested
                    return default;
                }
                catch (Exception ex)
                {
                    TgLogUtils.WriteExceptionWithMessage(ex, TgConstants.LogTypeNetwork);

#if DEBUG
                    Debug.WriteLine($"[{attempt + 1}/{FloodControlService.MaxRetryCount}] {TgFileUtils.GetShortFilePath(filePath)} | {memberName} | {lineNumber} | {ex.Message}", TgConstants.LogTypeNetwork);
#endif

                    await HandleTelegramExceptionAsync(ex);

                    if (attempt < FloodControlService.MaxRetryCount - 1)
                        await Task.Delay(TimeSpan.FromSeconds(FloodControlService.WaitFallbackFlood));
                    else
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Return default immediately when cancellation requested
            return default;
        }
        finally
        {
            if (callFinally is not null)
                await callFinally.Invoke();
        }

        TgLogUtils.WriteLog($"[{memberName}] All retry attempts failed.");
        return default;
    }

    private async Task TryCatchAsync(Func<Task> call, Func<Task>? callFinally = null, CancellationToken ct = default,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        try
        {
            for (int attempt = 0; attempt < FloodControlService.MaxRetryCount; attempt++)
            {
                if (CheckShouldStop(ct)) return;

                try
                {
                    await call();
                    return;
                }
                catch (OperationCanceledException)
                {
                    // Stop retrying when cancellation requested
                    return;
                }
                catch (Exception ex)
                {
                    if (ex is SyntheticTimeoutException)
                    {
                        TgLogUtils.WriteLog($"FusionCache SyntheticTimeout on key: {ex.Message}");
                        return;
                    }

                    TgLogUtils.WriteExceptionWithMessage(ex, TgConstants.LogTypeNetwork);
#if DEBUG
                    Debug.WriteLine($"[{attempt + 1}/{FloodControlService.MaxRetryCount}] {TgFileUtils.GetShortFilePath(filePath)} | {memberName} | {lineNumber} | {ex.Message}", TgConstants.LogTypeNetwork);
#endif

                    await HandleTelegramExceptionAsync(ex);

                    if (attempt < FloodControlService.MaxRetryCount - 1 && (
                        FloodControlService.IsFlood(ex.Message) || FloodControlService.IsFlood(ex.InnerException?.Message ?? string.Empty)))
                        await Task.Delay(TimeSpan.FromSeconds(FloodControlService.WaitFallbackFlood));
                    else
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Quietly exit without logging in
        }
        finally
        {
            if (callFinally is not null)
                await callFinally.Invoke();
        }

        TgLogUtils.WriteLog($"[{memberName}] All retry attempts failed.");
    }

    private async Task HandleTelegramExceptionAsync(Exception ex)
    {
        var message = ex.Message ?? string.Empty;

        if (message.Contains("You must connect to Telegram first", StringComparison.OrdinalIgnoreCase))
        {
            await SetClientExceptionShortAsync(ex);
            await UpdateStateMessageAsync("Reconnect client ...");
            await LoginUserAsync(isProxyUpdate: false);
            return;
        }

        if (ex.Source == "WTelegramClient")
        {
            switch (message)
            {
                case "PEER_ID_INVALID":
                    await UpdateStateExceptionShortAsync("The source is invalid!");
                    break;
                case "CHANNEL_PRIVATE":
                    await UpdateStateExceptionShortAsync("The channel is private!");
                    break;
                case "CHANNEL_INVALID":
                    await UpdateStateExceptionShortAsync("The channel is invalid!");
                    break;
                default:
                    await SetClientExceptionShortAsync(ex);
                    break;
            }
        }
        else
        {
            await SetClientExceptionAsync(ex);
        }
    }

    private static async Task UseOverrideMethodAsync()
    {
        await Task.CompletedTask;
        throw new NotImplementedException(TgConstants.UseOverrideMethod);
    }

    #endregion

    #region Methods - Connections

    /// <inheritdoc />
    public async Task<bool> CheckClientConnectionReadyAsync()
    {
        if (Client is null) return ClientResultDisconnected();
        if (Client is { Disconnected: true }) return ClientResultDisconnected();
        if (Me is null) return ClientResultDisconnected();
        if (Me.ID <= 0) return ClientResultDisconnected();
        switch (TgGlobalTools.AppType)
        {
            case TgEnumAppType.Console:
                if (!TgAppSettings.AppXml.IsExistsFileSession)
                    return ClientResultDisconnected();
                break;
        }
        var appDto = await StorageManager.AppRepository.GetCurrentDtoAsync();
        var proxyDto = await StorageManager.ProxyRepository.GetDtoAsync(appDto.ProxyUid);
        if (TgAppSettings.IsUseProxy && proxyDto.Uid != Guid.Empty)
            return ClientResultDisconnected();
        if (ProxyException.IsExist || ClientException.IsExist)
            return ClientResultDisconnected();
        return ClientResultConnected();
    }

    /// <inheritdoc />
    public async Task<bool> CheckBotConnectionReadyAsync()
    {
        var result = BotSqlConnection is not null && BotSqlConnection.State == System.Data.ConnectionState.Open &&
            Bot is not null && Bot.BotId > 0 && Bot.Client.Disconnected == false;
        if (!result)
            return ClientResultDisconnected();

        var appDto = await StorageManager.AppRepository.GetCurrentDtoAsync();
        var proxyDto = await StorageManager.ProxyRepository.GetDtoAsync(appDto.ProxyUid);
        if (TgAppSettings.IsUseProxy && proxyDto.Uid != Guid.Empty)
            return ClientResultDisconnected();
        if (ProxyException.IsExist || ClientException.IsExist)
            return ClientResultDisconnected();
        return ClientResultConnected();
    }

    /// <inheritdoc />
    public void SetBotInfoDto(TgBotInfoDto botInfoDto) => BotInfoDto = botInfoDto;

    /// <inheritdoc />
    public string GetChatUserName(long chatId)
    {
        if (DicChats.TryGetValue(ReduceChatId(chatId), out var chatBase) && chatBase is not null)
        {
            if (!string.IsNullOrEmpty(chatBase.MainUsername))
                return chatBase.MainUsername;
            if (chatBase is TL.Channel channel && !string.IsNullOrEmpty(channel.username))
                return channel.username;
        }
        return string.Empty;
    }

    /// <inheritdoc />
    public (string, string) GetChatUserLink(long chatId, int messageId) => TgStringUtils.FormatChatLink(GetChatUserName(chatId), chatId, messageId);

    /// <inheritdoc />
    public (string, string) GetChatUserLink(long chatId) => TgStringUtils.FormatChatLink(GetChatUserName(chatId), chatId);

    /// <inheritdoc />
    public async Task<(string, string)> GetUserLink(long chatId, int messageId, TL.Peer? peer, CancellationToken ct = default)
    {
        if (peer is not TL.PeerUser peerUser) return GetChatUserLink(chatId);
        if (chatId == peerUser.user_id) return GetChatUserLink(chatId);

        try
        {
            // Optimization: check cache
            if (_tlUserBuffer.FirstOrDefault(x => x.id == peerUser.user_id) is TL.User user)
                return TgStringUtils.FormatUserLink(user.username, user.id, string.Join(" ", user.first_name, user.last_name));

            // Retrieving data through a chat participant
            var userData = await GetParticipantAsync(chatId, peerUser.user_id, ct);
            if (userData is null)
                return GetChatUserLink(chatId);

            // Optimization: update cache
            _tlUserBuffer.Add(userData);
            return TgStringUtils.FormatUserLink(userData.username, userData.id, string.Join(" ", userData.first_name, userData.last_name));
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
        return GetChatUserLink(chatId);
    }

    /// <summary> Get input channel </summary>
    private TL.InputChannel? GetInputChannelFromUserChats(long chatId)
    {
        try
        {
            // Optimization: check cache
            if (InputChannelCache.TryGetValue(chatId, out var inputChannel))
                return inputChannel;

            long channelAccessHash = 0;
            if (!DicChats.IsEmpty)
            {
                var chatBase = DicChats.FirstOrDefault(x => x.Key == chatId).Value;
                if (chatBase is TL.Channel channel)
                    channelAccessHash = channel.access_hash;
                var result = new TL.InputChannel(chatId, channelAccessHash);

                // Optimization: update cache
                InputChannelCache.TryAdd(chatId, result);

                return result;
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
        return null;
    }

    /// <summary> Get chat </summary>
    private TL.ChatBase? GetChatBaseFromUserChats(long chatId)
    {
        try
        {
            // Optimization: check cache
            if (ChatCache.TryGetValue(chatId, out var chat))
                return chat;

            if (!DicChats.IsEmpty)
            {
                var chatBase = DicChats.FirstOrDefault(x => x.Key == chatId).Value;

                // Optimization: update cache
                ChatCache.TryAdd(chatId, chatBase);

                return chatBase;
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
        return null;
    }

    private TL.InputChannel? GetInputChannelFromChatBase(TL.ChatBase? chatBase)
    {
        if (chatBase is null) return null;

        // Optimization: check cache
        if (InputChannelCache.TryGetValue(chatBase.ID, out var inputChannel))
            return inputChannel;

        long channelAccessHash = 0;
        if (chatBase is TL.Channel channel)
            channelAccessHash = channel.access_hash;
        var result = new TL.InputChannel(chatBase.ID, channelAccessHash);

        // Optimization: update cache
        InputChannelCache.TryAdd(chatBase.ID, result);

        return result;
    }

    /// <inheritdoc />
    public async Task<TL.User?> GetParticipantAsync(long chatId, long? userId, CancellationToken ct)
    {
        var chatBase = GetChatBaseFromUserChats(chatId);
        var inputChannel = GetInputChannelFromChatBase(chatBase);
        if (inputChannel is null || inputChannel.access_hash == 0) return null;

        // Optimization: check cache
        if (userId is not null)
        {
            if (_tlUserBuffer.FirstOrDefault(x => x.id == userId) is TL.User user)
                return user;
            // Get one participant: need access hash
            try
            {
                // Call Telegram API with cancellation support
                var response = await TelegramCallAsync(apiCt => Client.Channels_GetParticipant(inputChannel, new TL.InputUser((long)userId, access_hash: 0)),
                    isThrow: false, ct);
                if (response is not null)
                {
                    // Optimization: update cache
                    var userData = response.users.FirstOrDefault(x => x.Value.id == userId).Value;
                    if (userData is not null)
                    {
                        _tlUserBuffer.Add(userData);
                        return userData;
                    }
                }
            }
            catch (RpcException ex) when (ex.Code == 400) // USER_NOT_PARTICIPANT
            {
                TgLogUtils.WriteException(ex);
            }
        }

        // Get
        try
        {
            int limit = 200;
            int offset = 0;
            while (true)
            {
                ChannelParticipantsFilter filter = new ChannelParticipantsRecent();
                // Call Telegram API with cancellation support
                var participants = await TelegramCallAsync(apiCt => Client.Channels_GetParticipants(inputChannel, filter, offset, limit, 0), isThrow: false, ct);
                if (participants is null || participants.participants.Length == 0)
                    break;

                foreach (var userItem in participants.users)
                {
                    if (userId is not null)
                    {
                        if (userItem.Value.id == userId)
                        {
                            // Optimization: update cache
                            _tlUserBuffer.Add(userItem.Value);
                            return userItem.Value;
                        }
                    }
                    else
                    {
                        // Optimization: update cache
                        _tlUserBuffer.Add(userItem.Value);
                    }
                }

                if (participants.participants.Length < limit)
                    break;
                offset += participants.participants.Length;
            }
            return null;
        }
        catch (RpcException ex) when (ex.Code == 400) // USER_NOT_PARTICIPANT
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<List<TL.User>> GetParticipantsAsync(long chatId, CancellationToken ct = default)
    {
        var chatDetailsDto = await GetChatDetailsByClientAsync(chatId, ct);

        _tlUserBuffer.Clear();
        await GetParticipantAsync(chatId, null, ct);

        // Sort participants
        var userList = _tlUserBuffer.GetList()
            .OrderByDescending(x => x.id == Client?.UserId)
            .ThenBy(x => x.LastSeenAgo)
            .Select(x => x)
            .ToList();

        return userList;
    }

    /// <inheritdoc />
    public async Task MakeFuncWithMessagesAsync(TgDownloadSettingsViewModel tgDownloadSettings,
        long chatId, Func<TgDownloadSettingsViewModel, TL.ChatBase, MessageBase, Task> func, CancellationToken ct = default)
    {
        var chatBase = GetChatBaseFromUserChats(chatId);
        if (chatBase is null) return;

        var inputChannel = GetInputChannelFromChatBase(chatBase);
        if (inputChannel is null || inputChannel.access_hash == 0) return;

        // Get
        try
        {
            int limit = 100;
            int offsetId = 0;
            int minId = 0;
            // Get the last message ID in a chat 
            int maxId = await GetChatLastMessageIdAsync(chatId, ct);
            while (true)
            {
                // Call Telegram API with cancellation support
                var messages = await TelegramCallAsync(apiCt => Client.Messages_GetHistory(inputChannel, offset_id: 0, limit: limit,
                    max_id: maxId, min_id: minId, add_offset: offsetId, hash: inputChannel.access_hash), isThrow: false, ct);
                if (messages is not null)
                {
                    foreach (var messageItem in messages.Messages)
                    {
                        await func(tgDownloadSettings, chatBase, messageItem);
                    }
                    maxId -= limit;
                }
                offsetId += 100;
                if (maxId <= 0)
                    break;
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
    }

    /// <inheritdoc />
    public async Task<int> GetChatLastMessageIdAsync(long chatId, CancellationToken ct)
    {
        var chatBase = GetChatBaseFromUserChats(chatId);
        if (chatBase is null) return 0;

        var inputChannel = GetInputChannelFromChatBase(chatBase);
        if (inputChannel is null || inputChannel.access_hash == 0) return 0;

        // Get
        try
        {
            // Call Telegram API with cancellation support
            var messages = await TelegramCallAsync(apiCt => Client.Messages_GetHistory(inputChannel, offset_id: 0, limit: 1,
                max_id: 0, min_id: 0, add_offset: 0, hash: inputChannel.access_hash), isThrow: false, ct);
            var lastNumber = messages.Messages.FirstOrDefault()?.ID ?? 0;
            return lastNumber;
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
        return 0;
    }

    /// <inheritdoc />
    public void ClearCaches()
    {
        InputChannelCache.Clear();
        ChatCache.Clear();
        _tlUserBuffer.Clear();
    }

    /// <inheritdoc />
    public async Task<TgChatDetailsDto> GetChatDetailsByClientAsync(string userName, TgDownloadSettingsViewModel tgDownloadSettings, CancellationToken ct = default)
    {
        userName = TgStringUtils.NormalizedTgName(userName, isAddAt: false);

        tgDownloadSettings.SourceVm = new TgEfSourceViewModel(TgGlobalTools.Container) { Dto = new TgEfSourceDto { UserName = userName } };
        await CreateChatBaseCoreAsync(tgDownloadSettings);

        if (Client is null) return new();

        // Get details about a public chat (even if client is not a member of that chat)
        if (tgDownloadSettings.Chat.Base is not null)
        {
            // Call Telegram API with cancellation support
            var chatFull = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChat(tgDownloadSettings.Chat.Base.ID),
                factory: SafeFactory<TL.Messages_ChatFull?>(async (ctx, innerCt) => await TelegramCallAsync(
                    apiCt => Client?.GetFullChat(tgDownloadSettings.Chat.Base) ?? default!, isThrow: false, innerCt)),
                TgCacheUtils.CacheOptionsFullChat, ct);
            WTelegram.Types.ChatFullInfo? chatDetails;
            if (chatFull is null)
                TgLog.WriteLine(TgLocale.TgGetChatDetailsError);
            else
            {
                chatDetails = ConvertToChatFullInfo(chatFull);
                await GetParticipantsAsync(chatDetails.Id, ct);
                return FillChatDetailsDto(chatDetails);
            }
        }
        return new();
    }

    /// <inheritdoc />
    public async Task<TgChatDetailsDto> GetChatDetailsByBotAsync(string userName)
    {
        userName = TgStringUtils.NormalizedTgName(userName, isAddAt: false);

        if (Bot is null) return new();

        // Get details about a public chat (even if bot is not a member of that chat)
        var chatDetails = await Bot.GetChat(userName);
        if (chatDetails is null)
            TgLog.MarkupInfo(TgLocale.TgGetChatDetailsError);
        else
        {
            return FillChatDetailsDto(chatDetails);
        }
        return new();
    }

    /// <summary> Get details about a public chat (even if client is not a member of that chat) </summary>
    private async Task<TgChatDetailsDto> GetChatDetailsByClientCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings, CancellationToken ct = default)
    {
        await CreateChatBaseCoreAsync(tgDownloadSettings);
        if (Client is null) return new();
        if (tgDownloadSettings.Chat.Base is null) return new();

        var key = TgCacheUtils.GetCacheKeyFullChat(tgDownloadSettings.Chat.Base.ID);
        Messages_ChatFull? chatFull = null;
        WTelegram.Types.ChatFullInfo? chatDetails = null;

        // Call Telegram API with cancellation support
        try
        {
            // Try to retrieve from cache in a strictly typed manner
            var maybe = await Cache.TryGetAsync<TL.Messages_ChatFull?>(key, TgCacheUtils.CacheOptionsFullChat, ct);
            if (maybe.HasValue)
                chatFull = maybe.Value;
            else
                // Cache miss: retrieve from server and save
                chatFull = await GetChatDetailsByClientSafeCoreAsync(tgDownloadSettings, key, ct);
        }
        catch (OperationCanceledException)
        {
            // Quietly exit without logging in
        }
        catch (InvalidCastException)
        {
            // There is another type in the cache: clear and reload
            await Cache.RemoveAsync(key, token: ct);
            chatFull = await GetChatDetailsByClientSafeCoreAsync(tgDownloadSettings, key, ct);
        }

        if (chatFull is null)
            TgLog.WriteLine(TgLocale.TgGetChatDetailsError);
        else
        {
            chatDetails = ConvertToChatFullInfo(chatFull);
            return FillChatDetailsDto(chatDetails);
        }
        return new();
    }

    /// <summary> Retrieves full channel info from Telegram API and caches it </summary>
    private async Task<TL.Messages_ChatFull?> GetChatDetailsByClientSafeCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings, string key, CancellationToken ct)
    {
        // Call Telegram API with cancellation support
        var fresh = await TelegramCallAsync<TL.Messages_ChatFull?>(apiCt => Client?.GetFullChat(tgDownloadSettings.Chat.Base) ?? default!,
            isThrow: false, ct);

        // Save to cache
        await Cache.SetAsync(key, fresh, TgCacheUtils.CacheOptionsFullChat, ct);
        return fresh;
    }

    /// <inheritdoc />
    public async Task<TgChatDetailsDto> GetChatDetailsByClientAsync(Guid uid, CancellationToken ct = default)
    {
        if (uid == Guid.Empty) return new();

        var dto = await StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == uid, ct);
        var tgDownloadSettings = new TgDownloadSettingsViewModel { SourceVm = new TgEfSourceViewModel(TgGlobalTools.Container) { Dto = dto } };
        return await GetChatDetailsByClientCoreAsync(tgDownloadSettings, ct);
    }

    /// <inheritdoc />
    public async Task<TgChatDetailsDto> GetChatDetailsByClientAsync(long id, CancellationToken ct)
    {
        if (id <= 0) return new();

        var dto = await StorageManager.SourceRepository.GetDtoAsync(x => x.Id == id, ct);
        var tgDownloadSettings = new TgDownloadSettingsViewModel { SourceVm = new TgEfSourceViewModel(TgGlobalTools.Container) { Dto = dto } };
        return await GetChatDetailsByClientCoreAsync(tgDownloadSettings, ct);
    }

    /// <inheritdoc />
    public WTelegram.Types.ChatFullInfo ConvertToChatFullInfo(TL.Messages_ChatFull messagesChatFull)
    {
        var chatFullInfo = new WTelegram.Types.ChatFullInfo();

        if (messagesChatFull.chats.Select(x => x.Value).FirstOrDefault() is not TL.ChatBase chatBase)
            return new WTelegram.Types.ChatFullInfo();

        chatFullInfo.Title = chatBase.Title;
        chatFullInfo.Id = chatBase.ID;
        chatFullInfo.Username = chatBase.MainUsername;
        chatFullInfo.TLInfo = messagesChatFull;

        if (messagesChatFull.full_chat is TL.ChannelFull channelFull)
        {
            chatFullInfo.Description = channelFull.about;
            chatFullInfo.InviteLink = channelFull.exported_invite?.ToString() ?? "-";
        }

        if (messagesChatFull.full_chat is TL.ChatFull chatFull)
        {
            chatFullInfo.Description = chatFull.about;
            chatFullInfo.InviteLink = chatFull.exported_invite?.ToString() ?? "-";
        }

        return chatFullInfo;
    }

    private static TgChatDetailsDto FillChatDetailsDto(WTelegram.Types.ChatFullInfo chatDetails)
    {
        TgChatDetailsDto chatDetailsDto = new()
        {
            Title = chatDetails.Title ?? "-",
            Type = chatDetails.Type switch
            {
                ChatType.Private => "Normal one-to-one chat with a user or bot",
                ChatType.Group => "Normal group chat",
                ChatType.Channel => "A channel",
                ChatType.Supergroup => "A supergroup",
                ChatType.Sender => "Value possible only in InlineQuery.ChatType: private chat with the inline query sender",
                _ => "-",
            },
            Id = ReduceChatId(chatDetails.Id).ToString(),
            UserName = chatDetails.Username ?? "-",
            InviteLink = chatDetails.InviteLink ?? "-",
            Description = !string.IsNullOrEmpty(chatDetails.Description) ? chatDetails.Description : "-" ?? "-"
        };

        FillChatDetailsPermissions(chatDetailsDto, chatDetails);

        if (chatDetails.TLInfo is TL.Messages_ChatFull messagesChatFull)
        {
            if (messagesChatFull.full_chat is TL.ChannelFull channelFull)
            {
                chatDetailsDto.IsChatFull = true;
                chatDetailsDto.About = channelFull.About;
                chatDetailsDto.ParticipantsCount = channelFull.participants_count;
                chatDetailsDto.OnlineCount = channelFull.online_count;
                chatDetailsDto.SlowMode = channelFull.slowmode_seconds > 0
                    ? $"enabled: {channelFull.slowmode_seconds} seconds" : $"disabled";
                chatDetailsDto.AvailableReactions = channelFull.available_reactions is TL.ChatReactionsAll { flags: TL.ChatReactionsAll.Flags.allow_custom }
                    ? "allows custom emojis as reactions" : "does not allow the use of custom emojis as a reaction";
                chatDetailsDto.TtlPeriod = channelFull.TtlPeriod;
            }
            if (messagesChatFull.chats.Select(x => x.Value).FirstOrDefault() is TL.ChatBase chatBase)
            {
                chatDetailsDto.IsActiveChat = chatBase.IsActive;
                chatDetailsDto.IsBanned = chatBase.IsBanned();
                chatDetailsDto.IsChannel = chatBase.IsChannel;
                chatDetailsDto.IsGroup = chatBase.IsGroup;
            }
            chatDetailsDto.IsForum = chatDetails.IsForum;
        }

        return chatDetailsDto;
    }

    private static void FillChatDetailsPermissions(TgChatDetailsDto chatDetailsDto, WTelegram.Types.ChatFullInfo chatDetails)
    {
        if (chatDetails.Permissions is null)
            chatDetailsDto.Permissions = "-";
        else
        {
            var permissions = new List<string>();
            if (chatDetails.Permissions.CanSendMessages)
                permissions.Add(nameof(chatDetails.Permissions.CanSendAudios));
            if (chatDetails.Permissions.CanSendAudios)
                permissions.Add(nameof(chatDetails.Permissions.CanSendMessages));
            if (chatDetails.Permissions.CanSendDocuments)
                permissions.Add(nameof(chatDetails.Permissions.CanSendDocuments));
            if (chatDetails.Permissions.CanSendPhotos)
                permissions.Add(nameof(chatDetails.Permissions.CanSendPhotos));
            if (chatDetails.Permissions.CanSendVideos)
                permissions.Add(nameof(chatDetails.Permissions.CanSendVideos));
            if (chatDetails.Permissions.CanSendVideoNotes)
                permissions.Add(nameof(chatDetails.Permissions.CanSendVideoNotes));
            if (chatDetails.Permissions.CanSendVoiceNotes)
                permissions.Add(nameof(chatDetails.Permissions.CanSendVoiceNotes));
            if (chatDetails.Permissions.CanSendPolls)
                permissions.Add(nameof(chatDetails.Permissions.CanSendPolls));
            if (chatDetails.Permissions.CanSendOtherMessages)
                permissions.Add(nameof(chatDetails.Permissions.CanSendOtherMessages));
            if (chatDetails.Permissions.CanAddWebPagePreviews)
                permissions.Add(nameof(chatDetails.Permissions.CanAddWebPagePreviews));
            if (chatDetails.Permissions.CanChangeInfo)
                permissions.Add(nameof(chatDetails.Permissions.CanChangeInfo));
            if (chatDetails.Permissions.CanInviteUsers)
                permissions.Add(nameof(chatDetails.Permissions.CanInviteUsers));
            if (chatDetails.Permissions.CanPinMessages)
                permissions.Add(nameof(chatDetails.Permissions.CanPinMessages));
            if (chatDetails.Permissions.CanManageTopics)
                permissions.Add(nameof(chatDetails.Permissions.CanManageTopics));
            chatDetailsDto.Permissions = string.Join(", ", permissions);
        }
    }

    #endregion
}
