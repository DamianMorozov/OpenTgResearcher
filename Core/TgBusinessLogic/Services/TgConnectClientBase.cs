// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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
    public User? Me { get; protected set; } = default!;
    public SqliteConnection? BotSqlConnection { get; private set; } = default!;
    protected ITgStorageManager StorageManager { get; set; } = default!;
    protected ITgFloodControlService FloodControlService { get; set; } = default!;

    public ConcurrentDictionary<long, ChatBase> ChatCache { get; private set; } = [];
    public ConcurrentDictionary<long, ChatBase> DicChats { get; private set; } = [];
    public ConcurrentDictionary<long, ChatBase> DicChatsUpdated { get; private set; } = [];
    public ConcurrentDictionary<long, InputChannel> InputChannelCache { get; private set; } = [];
    public ConcurrentDictionary<long, StoryItem> DicStories { get; private set; } = [];
    public ConcurrentDictionary<long, User> DicUsers { get; private set; } = [];
    public ConcurrentDictionary<long, User> DicUsersUpdated { get; private set; } = [];
    public ConcurrentDictionary<long, User> UserCache { get; private set; } = [];
    public ConcurrentQueue<Channel> EnumerableChannels { get; private set; } = [];
    public ConcurrentQueue<Channel> EnumerableGroups { get; private set; } = [];
    public ConcurrentQueue<ChatBase> EnumerableChats { get; private set; } = [];
    public ConcurrentQueue<ChatBase> EnumerableSmallGroups { get; private set; } = [];
    public ConcurrentQueue<DialogBase> EnumerableDialogs { get; private set; } = [];
    public ConcurrentQueue<User> EnumerableUsers { get; private set; } = [];

    public bool IsClientUpdateStatus { get; set; }
    public bool IsBotUpdateStatus { get; set; }
    /// <summary> Force stop parsing </summary>
    public bool IsForceStopDownloading { get; set; } = default!;
    private bool CheckShouldStop => IsForceStopDownloading || Client is null;
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

    private ITgDownloadViewModel? _tgDownloadSettings;

    protected TgConnectClientBase(ITgStorageManager storageManager, ITgFloodControlService floodControlService, IFusionCache cache) : base()
    {
        InitializeClient();
        // Services
        StorageManager = storageManager;
        FloodControlService = floodControlService;
        // FusionCache
        Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _chatBuffer = new(Cache, TgCacheUtils.GetCacheKeyChatPrefix());
        _messageBuffer = new(Cache, TgCacheUtils.GetCacheKeyMessagePrefix());
        _messageRelationBuffer = new(Cache, TgCacheUtils.GetCacheKeyMessageRelationPrefix());
        _storyBuffer = new(Cache, TgCacheUtils.GetCacheKeyStoryPrefix());
        _userBuffer = new(Cache, TgCacheUtils.GetCacheKeyUserPrefix());
    }

    protected TgConnectClientBase(IWebHostEnvironment webHostEnvironment, ITgStorageManager storageManager, ITgFloodControlService floodControlService, IFusionCache cache) : 
        base(webHostEnvironment)
    {
        InitializeClient();
        // Services
        StorageManager = storageManager;
        FloodControlService = floodControlService;
        // FusionCache
        Cache = cache;
        _chatBuffer = new(Cache, TgCacheUtils.GetCacheKeyChatPrefix());
        _messageBuffer = new(Cache, TgCacheUtils.GetCacheKeyChatPrefix());
        _messageRelationBuffer = new(Cache, TgCacheUtils.GetCacheKeyChatPrefix());
        _storyBuffer = new(Cache, TgCacheUtils.GetCacheKeyStoryPrefix());
        _userBuffer = new(Cache, TgCacheUtils.GetCacheKeyUserPrefix());
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
    private Action<int, string> BuildTelegramLogAction() => async (level, message) => await ProcessLogAndCheckFloodAsync(level, message);

    /// <summary> Processes a log message, checking for flood control and handling it if necessary </summary>
    private async Task ProcessLogAndCheckFloodAsync(int level, string message)
    {
        try
        {
            if (!FloodControlService.IsFlood(message)) return;

            await UpdateShellViewModelAsync(true, FloodControlService.TryExtractFloodWaitSeconds(message), message);
            if (_tgDownloadSettings is not null)
                await FlushChatBufferAsync(_tgDownloadSettings.IsSaveMessages, _tgDownloadSettings.IsRewriteMessages, isForce: true);
            await FloodControlService.WaitIfFloodAsync(message);
            await UpdateShellViewModelAsync(false, 0, string.Empty);
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

    private Task<T> TelegramCallAsync<T>(Func<Task<T>> telegramCall, bool isThrow = false)
    {
        if (CheckShouldStop) return default!;

        return FloodControlService.ExecuteWithFloodControlAsync(telegramCall, isThrow);
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
    public async Task<long> GetUserIdAsync()
    {
        if (Me is null)
            await LoginUserAsync(isProxyUpdate: false);
        var userId = Me?.ID ?? 0;

        if (userId == 0)
        {
            var licenseDtos = await StorageManager.LicenseRepository.GetListDtosAsync();
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

        await DisconnectClientAsync();
        await DisconnectBotAsync();

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

        await DisconnectClientAsync();

        var isBotConnectionReady = await CheckBotConnectionReadyAsync();
        if (Bot is null || !isBotConnectionReady)
        {
            await DisconnectBotAsync();

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
        await DisconnectClientAsync();
        Client = new(config);
        await ConnectThroughProxyAsync(proxyDto, true);
        Client.OnUpdates += OnUpdatesClientAsync;
        Client.OnOther += OnClientOtherAsync;
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

    public async Task<Channel?> GetChannelAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        // Collect chats from Telegram
        if (DicChats.IsEmpty)
            await CollectAllChatsAsync();

        if (tgDownloadSettings.SourceVm.Dto.IsReady)
        {
            tgDownloadSettings.SourceVm.Dto.Id = ReduceChatId(tgDownloadSettings.SourceVm.Dto.Id);
            foreach (var chat in DicChats)
            {
                if (chat.Value is Channel channel && Equals(channel.id, tgDownloadSettings.SourceVm.Dto.Id) &&
                    await IsChatBaseAccessAsync(channel))
                    return channel;
            }
        }
        else
        {
            foreach (var chat in DicChats)
            {
                if (chat.Value is Channel channel && Equals(channel.username, tgDownloadSettings.SourceVm.Dto.UserName) &&
                    await IsChatBaseAccessAsync(channel))
                    return channel;
            }
        }

        if (tgDownloadSettings.SourceVm.Dto.Id is 0 or 1)
            tgDownloadSettings.SourceVm.Dto.Id = await GetPeerIdAsync(tgDownloadSettings.SourceVm.Dto.UserName);

        Messages_Chats? messagesChats = null;
        if (Me is not null)
            messagesChats = await TelegramCallAsync(() => Client.Channels_GetChannels(new InputChannel(tgDownloadSettings.SourceVm.Dto.Id, Me.access_hash)),
                isThrow: false);
        if (messagesChats is not null)
        {
            foreach (var chat in messagesChats.chats)
            {
                if (chat.Value is Channel channel && Equals(channel.ID, tgDownloadSettings.SourceVm.Dto.Id))
                    return channel;
            }
        }
        return null;
    }

    public async Task<ChatBase?> GetChatBaseAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        // Collect chats from Telegram.
        if (DicChats.IsEmpty)
            await CollectAllChatsAsync();

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
            foreach (var chat in DicChats)
            {
                if (chat.Value is { } chatBase)
                    return chatBase;
            }
        }

        if (tgDownloadSettings.SourceVm.Dto.Id is 0)
            tgDownloadSettings.SourceVm.Dto.Id = await GetPeerIdAsync(tgDownloadSettings.SourceVm.Dto.UserName);

        Messages_Chats? messagesChats = null;
        if (Me is not null)
            messagesChats = await TelegramCallAsync(Client.Channels_GetGroupsForDiscussion);

        if (messagesChats is not null)
            foreach (var chat in messagesChats.chats)
            {
                if (chat.Value is { } chatBase && Equals(chatBase.ID, tgDownloadSettings.SourceVm.Dto.Id))
                    return chatBase;
            }

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

    public async Task<Bots_BotInfo?> GetBotInfoAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (tgDownloadSettings.SourceVm.Dto.Id is 0)
            tgDownloadSettings.SourceVm.Dto.Id = await GetPeerIdAsync(tgDownloadSettings.SourceVm.Dto.UserName);
        if (!tgDownloadSettings.SourceVm.Dto.IsReady)
            tgDownloadSettings.SourceVm.Dto.Id = ReduceChatId(tgDownloadSettings.SourceVm.Dto.Id);
        if (!tgDownloadSettings.SourceVm.Dto.IsReady)
            return null;
        Bots_BotInfo? botInfo = null;
        if (Me is not null)
            botInfo = await Client.Bots_GetBotInfo("en", new InputUser(tgDownloadSettings.SourceVm.Dto.Id, 0));
        return botInfo;
    }

    public string GetChatUpdatedName(long id)
    {
        var isGetValue = DicChatsUpdated.TryGetValue(ReduceChatId(id), out var chat);
        if (!isGetValue || chat is null)
            return string.Empty;
        return chat.ToString() ?? string.Empty;
    }

    public string GetPeerUpdatedName(Peer peer) => peer is PeerUser user ? GetUserUpdatedName(user.user_id)
        : peer is PeerChat or PeerChannel ? GetChatUpdatedName(peer.ID) : $"Peer {peer.ID}";

    /// <summary> Collect all chats from Telegram </summary>
    public async Task<IEnumerable<long>> CollectAllChatsAsync()
    {
        if (!IsReady || Client is null) return [];

        var messages = await TelegramCallAsync(Client.Messages_GetAllChats);
        FillEnumerableChats(messages.chats);
        return messages.chats.Select(x => x.Key);
    }

    /// <summary> Collect all dialogs from Telegram </summary>
    public async Task<IEnumerable<long>> CollectAllDialogsAsync()
    {
        if (!IsReady || Client is null) return [];

        var messages = await TelegramCallAsync(() => Client.Messages_GetAllDialogs());
        FillEnumerableDialogs(messages.Dialogs);
        return messages.Dialogs.Select(x => x.Peer.ID);
    }

    /// <summary> Collect all contacts from Telegram </summary>
    public async Task<List<long>> CollectAllContactsAsync(List<long>? chatIds)
    {
        if (!IsReady || Client is null) return chatIds ?? [];

        EnumerableUsers = [];
        var contacts = await TelegramCallAsync(() => Client.Contacts_GetContacts());
        FillEnumerableUsers(contacts.users);
        return chatIds ?? [];
    }

    /// <summary> Collect all users from Telegram by chat IDs </summary>
    public async Task<List<long>> CollectAllUsersAsync(List<long>? chatIds)
    {
        if (!IsReady || Client is null) return chatIds ?? [];

        EnumerableUsers = [];
        if (chatIds is not null && chatIds.Count != 0)
        {
            foreach (var chatId in chatIds)
            {
                var participants = await GetParticipantsAsync(chatId);
                Dictionary<long, User> users = [];
                foreach (var user in participants)
                    users.Add(user.id, user);
                FillEnumerableUsers(users);
            }
        }
        return chatIds ?? [];
    }

    /// <summary> Collect all stories from Telegram </summary>
    public async Task CollectAllStoriesAsync()
    {
        if (!IsReady || Client is null) return;

        var storiesBase = await TelegramCallAsync(() => Client.Stories_GetAllStories());
        if (storiesBase is Stories_AllStories allStories)
        {
            FillEnumerableStories([.. allStories.peer_stories]);
        }
    }

    private void FillEnumerableChats(Dictionary<long, ChatBase> chats)
    {
        DicChats = new ConcurrentDictionary<long, ChatBase>(chats.ToDictionary());
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
                case Chat smallGroup when (smallGroup.flags & Chat.Flags.deactivated) is 0:
                    EnumerableSmallGroups.Enqueue(chat.Value);
                    break;
                case Channel { IsGroup: true } group:
                    EnumerableGroups.Enqueue(group);
                    break;
                case Channel channel:
                    EnumerableChannels.Enqueue(channel);
                    break;
            }
        }
    }

    private void FillEnumerableDialogs(DialogBase[] dialogs)
    {
        EnumerableDialogs.Clear();
        if (dialogs == null || dialogs.Length == 0) return;

        // Sort
        var dialogsSorted = dialogs
            //.OrderBy(d => d.Peer is ChatBase cb ? cb.MainUsername : string.Empty)
            //.ThenBy(d => d.Peer is ChatBase cb ? cb.ID : 0)
            .ToList();

        foreach (var dialog in dialogsSorted)
        {
            EnumerableDialogs.Enqueue(dialog);
            //if (dialog.Peer is ChatBase chatBase)
            //{
            //    switch (chatBase)
            //    {
            //        case Chat smallGroup when (smallGroup.flags & Chat.Flags.deactivated) is 0:
            //            EnumerableDialogSmallGroups.Enqueue(dialog);
            //            break;
            //        case Channel { IsGroup: true } group:
            //            EnumerableDialogGroups.Enqueue(dialog);
            //            break;
            //        case Channel channel:
            //            EnumerableDialogChannels.Enqueue(dialog);
            //            break;
            //    }
            //}
        }
    }

    private void AddEnumerableChats(Dictionary<long, ChatBase> chats)
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
                case Chat smallGroup when (smallGroup.flags & Chat.Flags.deactivated) is 0:
                    if (EnumerableSmallGroups.Contains(chat.Value))
                        EnumerableSmallGroups.Enqueue(chat.Value);
                    break;
                case Channel { IsGroup: true } group:
                    if (EnumerableGroups.Contains(chat.Value))
                        EnumerableGroups.Enqueue(group);
                    break;
                case Channel channel:
                    if (EnumerableChannels.Contains(chat.Value))
                        EnumerableChannels.Enqueue(channel);
                    break;
            }
        }
    }

    private void FillEnumerableUsers(Dictionary<long, User> users)
    {
        DicUsers = new ConcurrentDictionary<long, User>(users.ToDictionary());
        EnumerableUsers.Clear();
        // Sort
        var usersSorted = users.OrderBy(i => i.Value.username).ThenBy(i => i.Value.ID);
        foreach (var user in usersSorted)
        {
            EnumerableUsers.Enqueue(user.Value);
        }
    }

    private void FillEnumerableStories(List<PeerStories> peerStories)
    {
        DicStories.Clear();

        // Sort
        var peerStoriesSorted = peerStories.OrderBy(i => i.stories.Rank).ToArray();
        foreach (var peerStory in peerStoriesSorted)
            foreach (var storyBase in peerStory.stories)
                if (storyBase is StoryItem story)
                    DicStories.AddOrUpdate(peerStory.peer.ID, story, (key, oldValue) => story);
    }

    private async Task OnUpdateShortClientAsync(UpdateShort updateShort)
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
                                if (chatBase.Value is Channel { IsActive: true } channel)
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

    private async Task OnUpdateClientUpdatesAsync(UpdatesBase updates)
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
                                if (chatBase.Value is Channel { IsActive: true } channel)
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

    // https://corefork.telegram.org/type/Update
    private async Task SwitchUpdateTypeAsync(Update update, Channel? channel = null)
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
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Chat user typing [{updateChatUserTyping}]{channelLabel}");
        //        break;
        //    case UpdateChatParticipants { participants: ChatParticipants chatParticipants }:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Chat participants [{chatParticipants.ChatId} | {string.Join(", ", chatParticipants.Participants.Length)}]{channelLabel}");
        //        break;
        //    case UpdateUserStatus updateUserStatus:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"User status [{updateUserStatus.user_id} | {updateUserStatus}]{channelLabel}");
        //        break;
        //    case UpdateUserName updateUserName:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"User name [{updateUserName.user_id} | {string.Join(", ", updateUserName.usernames.Select(item => item.username))}]{channelLabel}");
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
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Chat participant add [{updateChatParticipantAdd}]{channelLabel}");
        //        break;
        //    case UpdateChatParticipantDelete updateChatParticipantDelete:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Chat participant delete [{updateChatParticipantDelete}]{channelLabel}");
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
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"User phone [{updateUserPhone}]{channelLabel}");
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
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"User typing [{updateUserTyping}]{channelLabel}");
        //        break;
        //    case UpdateChannelMessageViews updateChannelMessageViews:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Channel message views [{updateChannelMessageViews}]{channelLabel}");
        //        break;
        //    case UpdateChannel updateChannel:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Channel [{updateChannel}]");
        //        break;
        //    case UpdateChannelReadMessagesContents updateChannelReadMessages:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Channel read messages [{string.Join(", ", updateChannelReadMessages.messages)}]{channelLabel}");
        //        break;
        //    case UpdateChannelUserTyping updateChannelUserTyping:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Channel user typing [{updateChannelUserTyping}]{channelLabel}");
        //        break;
        //    case UpdateMessagePoll updateMessagePoll:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Message poll [{updateMessagePoll}]{channelLabel}");
        //        break;
        //    case UpdateChannelTooLong updateChannelTooLong:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Channel too long [{updateChannelTooLong}]{channelLabel}");
        //        break;
        //    case UpdateReadChannelInbox updateReadChannelInbox:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Channel inbox [{updateReadChannelInbox}]{channelLabel}");
        //        break;
        //    case UpdateChatParticipantAdmin updateChatParticipantAdmin:
        //        await UpdateChatViewModelAsync(sourceId, 0, 0, $"Chat participant admin[{updateChatParticipantAdmin}]{channelLabel}");
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

    private async Task OnClientOtherAuthSentCodeAsync(Auth_SentCodeBase authSentCode)
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

    public static IEnumerable<ChatBase> SortListChats(IList<ChatBase> chats)
    {
        if (!chats.Any())
            return chats;
        List<ChatBase> result = [];
        List<ChatBase> chatsOrders = [.. chats.OrderBy(x => x.Title)];
        foreach (var chatOrder in chatsOrders)
        {
            var chatNew = chats.First(x => Equals(x.Title, chatOrder.Title));
            if (chatNew.ID is not 0)
                result.Add(chatNew);
        }
        return result;
    }

    public static IEnumerable<Channel> SortListChannels(IList<Channel> channels)
    {
        if (!channels.Any())
            return channels;
        List<Channel> result = [];
        List<Channel> channelsOrders = [.. channels.OrderBy(x => x.username)];
        foreach (var chatOrder in channelsOrders)
        {
            var chatNew = channels.First(x => Equals(x.Title, chatOrder.Title));
            if (chatNew.ID is not 0)
                result.Add(chatNew);
        }
        return result;
    }

    public async Task PrintChatsInfoAsync(Dictionary<long, ChatBase> dicChats, string name, bool isSave)
    {
        TgLog.MarkupInfo($"Found {name}: {dicChats.Count}");
        TgLog.MarkupInfo(TgLocale.TgGetDialogsInfo);
        foreach (var dicChat in dicChats)
        {
            await TryCatchAsync(async () =>
            {
                switch (dicChat.Value)
                {
                    case Channel channel:
                        await PrintChatsInfoChannelAsync(channel, false, false, isSave);
                        break;
                    default:
                        TgLog.MarkupLine(GetChatInfo(dicChat.Value));
                        break;
                }
            });
        }
    }

    private async Task<Messages_ChatFull?> PrintChatsInfoChannelAsync(Channel channel, bool isFull, bool isSilent, bool isSave)
    {
        Messages_ChatFull? fullChannel = null;
        try
        {
            fullChannel = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChannel(channel.id), 
                factory: SafeFactory<TL.Messages_ChatFull?>(async (ctx, ct) => await TelegramCallAsync(() => Client?.Channels_GetFullChannel(channel) ?? default!)), 
                TgCacheUtils.CacheOptionsFullChat);
            if (isSave)
            {
                await StorageManager.SourceRepository.SaveAsync(new() { Id = channel.id, UserName = channel.username, Title = channel.title });
            }
            if (!isSilent)
            {
                if (fullChannel is not null)
                {
                    if (fullChannel.full_chat is ChannelFull channelFull)
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

    private async Task<Messages_ChatFull?> PrintChatsInfoChatBaseAsync(ChatBase chatBase, bool isFull, bool isSilent)
    {
        Messages_ChatFull? chatFull = null;
        if (Client is null)
            return chatFull;
        try
        {
            chatFull = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChat(chatBase.ID), 
                factory: SafeFactory<TL.Messages_ChatFull?>(async (ctx, ct) => await TelegramCallAsync(() => Client?.GetFullChat(chatBase) ?? default!)), 
                TgCacheUtils.CacheOptionsFullChat);
            if (chatFull is null) return chatFull;
            if (!isSilent)
            {
                if (chatFull.full_chat is ChannelFull channelFull)
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

    public static string GetChatInfo(ChatBase chatBase) => $"{chatBase.ID} | {chatBase.Title}";

    public static string GetChannelFullInfo(ChannelFull channelFull, ChatBase chatBase, bool isFull)
    {
        var result = GetChatInfo(chatBase);
        if (isFull)
            result += " | " + Environment.NewLine + channelFull.About;
        return result;
    }

    public static string GetChatFullBaseInfo(ChatFullBase chatFull, ChatBase chatBase, bool isFull)
    {
        var result = GetChatInfo(chatBase);
        if (isFull)
            result += " | " + Environment.NewLine + chatFull.About;
        return result;
    }

    /// <summary> Check access rights for chat </summary>
    public async Task<bool> IsChatBaseAccessAsync(ChatBase chatBase)
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
                if (chatBase is Chat chatBaseObj)
                {
                    var full = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChat(chatBaseObj.ID),
                        factory: SafeFactory<TL.Messages_ChatFull?>(async (ctx, ct) => await TelegramCallAsync(() => Client?.Messages_GetFullChat(chatBaseObj.ID) ?? default!)), 
                        TgCacheUtils.CacheOptionsFullChat);
                    if (full is TL.Messages_ChatFull chatFull && chatFull.full_chat is TL.ChatFull chatFullObj)
                    {
                        if (chatFullObj.flags.HasFlag(User.Flags.has_access_hash))
                        {
                            result = true;
                            return;
                        }
                    }
                }
                if (chatBase is Channel channelBase)
                {
                    if (channelBase.flags.HasFlag(TL.Channel.Flags.has_access_hash))
                    {
                        result = true;
                        return;
                    }
                }
            });
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
            });
            return result;
        }
    }

    private async Task<int> GetChannelMessageIdAsync(ITgDownloadViewModel? tgDownloadSettings, TgEnumPosition position)
    {
        if (Client is null) return 0;
        if (tgDownloadSettings is null) return 0;
        if (tgDownloadSettings.Chat.Base is not { } chatBase) return 0;
        if (chatBase.ID is 0) return 0;

        if (chatBase is Channel channel)
        {
            var fullChannel = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChannel(channel.id),
                factory: SafeFactory<TL.Messages_ChatFull?>(async (ctx, ct) => await TelegramCallAsync(() => Client?.Channels_GetFullChannel(channel) ?? default!)), 
                TgCacheUtils.CacheOptionsFullChat);
            if (fullChannel is null) return 0;
            if (fullChannel.full_chat is not ChannelFull channelFull) return 0;
            var isAccessToMessages = await TelegramCallAsync(() => Client.Channels_ReadMessageContents(channel));
            if (isAccessToMessages)
            {
                switch (position)
                {
                    case TgEnumPosition.First:
                        return await SetChannelMessageIdFirstCoreAsync(tgDownloadSettings, chatBase, channelFull);
                    case TgEnumPosition.Last:
                        return GetChannelMessageIdLastCore(channelFull);
                }
            }
        }
        else
        {
            var fullChannel = await TelegramCallAsync(() => Client.GetFullChat(chatBase));
            switch (position)
            {
                case TgEnumPosition.First:
                    return await SetChannelMessageIdFirstCoreAsync(tgDownloadSettings, chatBase, fullChannel.full_chat);
                case TgEnumPosition.Last:
                    return GetChannelMessageIdLastCore(fullChannel.full_chat);
            }
        }
        return 0;
    }

    public async Task<int> GetChannelMessageIdLastAsync(ITgDownloadViewModel? tgDownloadSettings) =>
        await GetChannelMessageIdAsync(tgDownloadSettings, TgEnumPosition.Last);

    private static int GetChannelMessageIdLastCore(ChannelFull channelFull) =>
        channelFull.read_inbox_max_id;

    private static int GetChannelMessageIdLastCore(ChatFullBase chatFullBase) =>
        chatFullBase is ChannelFull channelFull ? channelFull.read_inbox_max_id : 0;

    private async Task<int> GetChannelMessageIdLastCoreAsync(Channel channel)
    {
        var fullChannel = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChannel(channel.id),
            factory: SafeFactory<TL.Messages_ChatFull?>(async (ctx, ct) => await TelegramCallAsync(() => Client?.Channels_GetFullChannel(channel) ?? default!)), 
            TgCacheUtils.CacheOptionsFullChat);
        if (fullChannel is null) return 0;
        if (fullChannel.full_chat is not ChannelFull channelFull) return 0;
        return channelFull.read_inbox_max_id;
    }

    private async Task<int> GetChannelMessageIdLastCoreAsync(long chatId)
    {
        var messagesCount = 0;
        var cacheChannel = EnumerableChannels.FirstOrDefault(x => x.ID == chatId);
        if (cacheChannel is not null)
        {
            messagesCount = await GetChannelMessageIdLastCoreAsync(cacheChannel);
        }
        return messagesCount;
    }

    public async Task SetChannelMessageIdFirstAsync(ITgDownloadViewModel tgDownloadSettings) =>
        await GetChannelMessageIdAsync(tgDownloadSettings, TgEnumPosition.First);

    private async Task<int> SetChannelMessageIdFirstCoreAsync(ITgDownloadViewModel tgDownloadSettings, ChatBase chatBase,
        ChatFullBase chatFullBase)
    {
        if (tgDownloadSettings is not TgDownloadSettingsViewModel tgDownloadSettings2) return 0;
        var max = chatFullBase is ChannelFull channelFull ? channelFull.read_inbox_max_id : 0;
        var result = max;
        var partition = 200;
        var inputMessages = new InputMessage[partition];
        var offset = 0;
        var isSkipChannelCreate = false;
        // While.
        while (offset < max)
        {
            for (var i = 0; i < partition; i++)
            {
                inputMessages[i] = offset + i + 1;
            }
            tgDownloadSettings2.SourceVm.Dto.FirstId = offset;
            var start = offset + 1;
            var end = offset + partition;
            var messages = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyMessages(chatBase.ID, start, end),
                factory: SafeFactory<TL.Messages_MessagesBase?>(async (ctx, ct) => await TelegramCallAsync(() => Client.Channels_GetMessages(chatBase as Channel, inputMessages))),
                TgCacheUtils.CacheOptionsChannelMessages);

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
        tgDownloadSettings2.SourceVm.Dto.FirstId = result;
        return result;
    }

    public async Task CreateChatAsync(ITgDownloadViewModel tgDownloadSettings, bool isSilent)
    {
        if (tgDownloadSettings is not TgDownloadSettingsViewModel tgDownloadSettings2) return;

        var sourceDto = tgDownloadSettings2.SourceVm.Dto;
        var sourceEntity = await StorageManager.SourceRepository.GetItemAsync(new() { Id = sourceDto.Id }, isReadOnly: false);
        
        await CreateChatBaseCoreAsync(tgDownloadSettings2);

        if (Bot is not null)
        {
            var botChatFullInfo = await GetChatDetailsForBot(sourceDto.Id, sourceDto.UserName);
            if (botChatFullInfo?.TLInfo is Messages_ChatFull { full_chat: ChannelFull channelFull })
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
            if (tgDownloadSettings2.Chat.Base is { } chatBase && await IsChatBaseAccessAsync(chatBase))
            {
                sourceEntity.IsUserAccess = true;
                sourceEntity.IsSubscribe = true;
                sourceEntity.UserName = chatBase.MainUsername ?? string.Empty;
                
                // TODO: use smart method from scan chat
                sourceEntity.Count = await GetChannelMessageIdLastAsync(tgDownloadSettings);
                // FullChat cache
                var chatFull = await PrintChatsInfoChatBaseAsync(chatBase, isFull: true, isSilent);
                sourceEntity.Title = chatBase.Title;
                if (chatFull?.full_chat is ChannelFull chatBaseFull)
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

        await StorageManager.SourceRepository.SaveAsync(sourceEntity);
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

        TgLog.MarkupWarning($"Chat not found by id ({dtoId}) or username ({dtoUserName})");
        return null;
    }

    /// <summary> Update source from Telegram </summary>
    public async Task UpdateSourceDbAsync(ITgEfSourceViewModel sourceVm, ITgDownloadViewModel tgDownloadSettings)
    {
        if (sourceVm is not TgEfSourceViewModel sourceVm2) return;
        if (tgDownloadSettings is not TgDownloadSettingsViewModel tgDownloadSettings2) return;
        await CreateChatAsync(tgDownloadSettings, isSilent: true);
        sourceVm2.Dto.Copy(tgDownloadSettings2.SourceVm.Dto, isUidCopy: false);
    }

    /// <summary> Build chat entity from Telegram chat </summary>
    private async Task<TgEfSourceEntity> BuildChatEntityAsync(ChatBase chat, string about, int messagesCount, bool isUserAccess)
    {
        var accessHash = chat is Channel ch ? ch.access_hash : 0;

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
                // Update
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

    /// <summary> Update user entity from DTO </summary>
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

    private static void TgEfStoryEntityByMessageType(TgEfStoryEntity storyNew, MessageEntity message)
    {
        if (message is null)
            return;
        switch (message.GetType())
        {
            case var cls when cls == typeof(MessageEntityUnknown):
                break;
            case var cls when cls == typeof(MessageEntityMention):
                break;
            case var cls when cls == typeof(MessageEntityHashtag):
                break;
            case var cls when cls == typeof(MessageEntityBotCommand):
                break;
            case var cls when cls == typeof(MessageEntityUrl):
                break;
            case var cls when cls == typeof(MessageEntityEmail):
                break;
            case var cls when cls == typeof(MessageEntityBold):
                break;
            case var cls when cls == typeof(MessageEntityItalic):
                break;
            case var cls when cls == typeof(MessageEntityCode):
                break;
            case var cls when cls == typeof(MessageEntityPre):
                break;
            case var cls when cls == typeof(MessageEntityTextUrl):
                break;
            case var cls when cls == typeof(MessageEntityMentionName):
                break;
            case var cls when cls == typeof(InputMessageEntityMentionName):
                break;
            case var cls when cls == typeof(MessageEntityPhone):
                break;
            case var cls when cls == typeof(MessageEntityCashtag):
                break;
            case var cls when cls == typeof(MessageEntityUnderline):
                break;
            case var cls when cls == typeof(MessageEntityStrike):
                break;
            case var cls when cls == typeof(MessageEntityBankCard):
                break;
            case var cls when cls == typeof(MessageEntitySpoiler):
                break;
            case var cls when cls == typeof(MessageEntityCustomEmoji):
                break;
            case var cls when cls == typeof(MessageEntityBlockquote):
                break;
        }
    }

    private static void TgEfStoryEntityByMediaType(TgEfStoryEntity storyNew, MessageMedia media)
    {
        if (media is null)
            return;
        switch (media.GetType())
        {
            case var cls when cls == typeof(MessageMediaContact):
                break;
            case var cls when cls == typeof(MessageMediaDice):
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
    public async Task SearchSourcesTgAsync(ITgDownloadViewModel iSettings, TgEnumSourceType sourceType, List<long>? chatIds = null)
    {
        if (iSettings is not TgDownloadSettingsViewModel tgDownloadSettings) return;
        _tgDownloadSettings = tgDownloadSettings;

        await TryCatchAsync(async () =>
        {
            await UpdateChatsViewModelAsync(0, 0, TgEnumChatsMessageType.StartScan);
            await LoginUserAsync(isProxyUpdate: false);
            switch (sourceType)
            {
                case TgEnumSourceType.Chat:
                    await SetUserAccessForAllChatsAsync(chatIds, isUserAccess: false);
                    await SetSubscribeForAllChatsAsync(chatIds, isSubscribe: false);
                    await UpdateChatViewModelAsync(tgDownloadSettings.SourceVm.Dto.Id, 0, tgDownloadSettings.SourceVm.Dto.Count, TgLocale.CollectChats);
                    chatIds = [.. await CollectAllChatsAsync()];
                    await SetSubscribeForAllChatsAsync(chatIds, isSubscribe: true);
                    tgDownloadSettings.SourceScanCount = DicChats.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    await SearchChatsAsync(tgDownloadSettings, chatIds);
                    break;
                case TgEnumSourceType.Dialog:
                    await SetUserAccessForAllChatsAsync(chatIds, isUserAccess: false);
                    await SetSubscribeForAllChatsAsync(chatIds, isSubscribe: false);
                    await UpdateChatViewModelAsync(tgDownloadSettings.SourceVm.Dto.Id, 0, tgDownloadSettings.SourceVm.Dto.Count, TgLocale.CollectDialogs);
                    chatIds = [.. await CollectAllDialogsAsync()];
                    await SetSubscribeForAllChatsAsync(chatIds, isSubscribe: true);
                    tgDownloadSettings.SourceScanCount = DicChats.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    await SearchChatsAsync(tgDownloadSettings, chatIds);
                    break;
                case TgEnumSourceType.Story:
                    await CollectAllStoriesAsync();
                    tgDownloadSettings.SourceScanCount = DicStories.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    await SearchStoriesAsync(tgDownloadSettings);
                    break;
                case TgEnumSourceType.Contact:
                    await UpdateChatViewModelAsync(tgDownloadSettings.ContactVm.Dto.Id, 0, 0, TgLocale.CollectContacts);
                    chatIds = [.. await CollectAllContactsAsync(chatIds)];
                    tgDownloadSettings.SourceScanCount = DicUsers.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    await SearchUsersAsync(tgDownloadSettings, isContact: true, chatIds);
                    break;
                case TgEnumSourceType.User:
                    await UpdateChatViewModelAsync(tgDownloadSettings.ContactVm.Dto.Id, 0, 0, TgLocale.CollectUsers);
                    chatIds = [.. await CollectAllUsersAsync(chatIds)];
                    tgDownloadSettings.SourceScanCount = DicUsers.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    await SearchUsersAsync(tgDownloadSettings, isContact: false, chatIds);
                    break;
            }
        }, async () => {
            _tgDownloadSettings = null;
            await UpdateChatsViewModelAsync(0, 0, TgEnumChatsMessageType.StopScan);
            await UpdateTitleAsync(string.Empty);
        });
    }

    /// <summary> Set user access for all chats </summary>
    private async Task SetUserAccessForAllChatsAsync(List<long>? chatIds, bool isUserAccess)
    {
        try
        {
            if (chatIds is null)
                await StorageManager.SourceRepository.SetIsUserAccessAsync(isUserAccess);
            else
                await StorageManager.SourceRepository.SetIsUserAccessAsync(chatIds, isUserAccess);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            TgLog.MarkupWarning($"Error set user access for all chats: {ex.Message}");
        }
    }

    /// <summary> Set subscribe for all chats </summary>
    private async Task SetSubscribeForAllChatsAsync(List<long>? chatIds, bool isSubscribe)
    {
        try
        {
            if (chatIds is null)
                await StorageManager.SourceRepository.SetIsSubscribeAsync(isSubscribe);
            else
                await StorageManager.SourceRepository.SetIsSubscribeAsync(chatIds, isSubscribe);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            TgLog.MarkupWarning($"Error set subscribe for all chats: {ex.Message}");
        }
    }

    private async Task SearchChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings, List<long>? chatIds)
    {
        try
        {
            var counter = 0;
            var channels = chatIds is not null && chatIds.Count != 0 ? EnumerableChannels.Where(x => chatIds.Contains(x.ID)) : EnumerableChannels;
            var groups = (chatIds is not null && chatIds.Count != 0) ? EnumerableGroups.Where(x => chatIds.Contains(x.ID)) : EnumerableGroups;
            var smallGroups = (chatIds is not null && chatIds.Count != 0) ? EnumerableSmallGroups.Where(x => chatIds.Contains(x.ID)) : EnumerableSmallGroups;
            var dialogs = (chatIds is not null && chatIds.Count != 0) ? EnumerableDialogs.Where(x => chatIds.Contains(x.Peer.ID)) : EnumerableDialogs;
            var countAll = channels.Count() + groups.Count() + smallGroups.Count() + dialogs.Count();

            // First groups (small + groups)
            counter = await SearchGroupsAsync(tgDownloadSettings, smallGroups, counter, countAll);
            counter = await SearchGroupsAsync(tgDownloadSettings, groups, counter, countAll);

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
                        var fullChannel = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChannel(channel.id),
                            factory: SafeFactory<TL.Messages_ChatFull?>(async (ctx, ct) => await TelegramCallAsync(() => Client?.Channels_GetFullChannel(channel) ?? default!)), 
                            TgCacheUtils.CacheOptionsFullChat);
                        if (fullChannel?.full_chat is ChannelFull channelFull)
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
                }, async () => {
                    await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(tgDownloadSettings.SourceScanCount,
                        tgDownloadSettings.SourceScanCurrent):#00.00} %");
                });
                
                await FlushChatBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: false);
            }
        }
        finally
        {
            await FlushChatBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: true);
        }
    }

    private async Task<int> SearchGroupsAsync(ITgDownloadViewModel tgDownloadSettings, IEnumerable<ChatBase> groups, int counter, int countAll)
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
                if (group is Channel ch)
                    entity = await BuildChatEntityAsync(ch, string.Empty, messagesCount: 0, isUserAccess: true);
                else
                    entity = await BuildChatEntityAsync(group, string.Empty, messagesCount: 0, isUserAccess: true);

                _chatBuffer.Add(entity);

                if (tgDownloadSettings is TgDownloadSettingsViewModel tgDownloadSettings2)
                    await UpdateChatViewModelAsync(tgDownloadSettings2.SourceVm.Dto.Id, 0, 0, $"{group}");
            });
        }
        return counter;
    }

    private async Task SearchStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        try
        {
            var counter = 0;
            foreach (var story in DicStories)
            {
                counter++;
                tgDownloadSettings.SourceScanCurrent++;
                await UpdateChatsViewModelAsync(counter, DicStories.Count, TgEnumChatsMessageType.ProcessingStories);
                _ = await FillBufferStoriesAsync(story.Key, story.Value);
                await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(tgDownloadSettings.SourceScanCount, tgDownloadSettings.SourceScanCurrent):#00.00} %");
                
                await FlushStoryBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: false);
            }
        }
        finally
        {
            await FlushStoryBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: true);
        }
    }

    private async Task SearchUsersAsync(TgDownloadSettingsViewModel tgDownloadSettings, bool isContact, List<long>? chatIds)
    {
        try
        {
            var counter = 0;
            foreach (var user in EnumerableUsers)
            {
                counter++;
                tgDownloadSettings.SourceScanCurrent++;
                await UpdateChatsViewModelAsync(counter, DicUsers.Count, TgEnumChatsMessageType.ProcessingUsers);
                _ = await FillBufferUsersAsync(user, isContact);
                await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(tgDownloadSettings.SourceScanCount, tgDownloadSettings.SourceScanCurrent):#00.00} %");

                await FlushUserBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: false);
            }
        }
        finally
        {
            await FlushUserBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: true);
        }
    }

    //public async Task ScanSourcesTgDesktopAsync(TgEnumSourceType sourceType, Func<ITgEfSourceViewModel, Task> afterScanAsync)
    //{
    //    await TryCatchAsync(async () =>
    //    {
    //        switch (sourceType)
    //        {
    //            case TgEnumSourceType.Chat:
    //                await UpdateChatViewModelAsync(0, 0, 0, TgLocale.CollectChats);
    //                switch (IsReady)
    //                {
    //                    case true when Client is not null:
    //                        var messages = await TelegramCallAsync(() => Client.Messages_GetAllChats());
    //                        FillEnumerableChats(messages.chats);
    //                        break;
    //                }
    //                await AfterCollectSourcesAsync(afterScanAsync);
    //                break;
    //            case TgEnumSourceType.Dialog:
    //                await UpdateChatViewModelAsync(0, 0, 0, TgLocale.CollectDialogs);
    //                switch (IsReady)
    //                {
    //                    case true when Client is not null:
    //                        {
    //                            var messages = await TelegramCallAsync(() => Client.Messages_GetAllDialogs());
    //                            FillEnumerableChats(messages.chats);
    //                            break;
    //                        }
    //                }
    //                break;
    //        }
    //    });
    //}

    //private async Task AfterCollectSourcesAsync(Func<TgEfSourceViewModel, Task> afterScanAsync)
    //{
    //    var i = 0;
    //    foreach (var channel in EnumerableChannels)
    //    {
    //        if (channel.IsActive)
    //        {
    //            await TryCatchAsync(async () =>
    //            {
    //                TgEfSourceEntity source = new() { Id = channel.ID };
    //                var messagesCount = await GetChannelMessageIdLastCoreAsync(channel);
    //                source.Count = messagesCount;
    //                if (channel.IsChannel)
    //                {
    //                    var fullChannel = await GetCachedFullChannelAsync(channel);
    //                    if (fullChannel is not null)
    //                    {
    //                        if (fullChannel.full_chat is ChannelFull channelFull)
    //                        {
    //                            source.About = channelFull.about;
    //                            source.UserName = channel.username;
    //                            source.Title = channel.title;
    //                        }
    //                    }
    //                }
    //                await afterScanAsync(new(TgGlobalTools.Container, source));
    //            });
    //        }
    //        i++;
    //    }

    //    i = 0;
    //    foreach (var group in EnumerableGroups)
    //    {
    //        await TryCatchAsync(async () =>
    //        {
    //            TgEfSourceEntity source = new() { Id = group.ID };
    //            if (group.IsActive)
    //            {
    //                var messagesCount = await GetChannelMessageIdLastCoreAsync(group);
    //                source.Count = messagesCount;
    //            }
    //            await afterScanAsync(new(TgGlobalTools.Container, source));
    //        });
    //        i++;
    //    }
    //}

    /// <summary> Parse Telegram chat </summary>
    public async Task ParseChatAsync(ITgDownloadViewModel tgDownloadSettings)
    {
        if (tgDownloadSettings is not TgDownloadSettingsViewModel tgDownloadSettings2) return;
        _tgDownloadSettings = tgDownloadSettings2;
        // Check force stop parsing
        IsForceStopDownloading = false;
        await CreateChatAsync(tgDownloadSettings, isSilent: false);

        await TryCatchAsync(async () =>
        {
            var isAccessToMessages = false;
            // Filters
            Filters = await StorageManager.FilterRepository.GetListDtosAsync(0, 0, x => x.IsEnabled);

            await LoginUserAsync(isProxyUpdate: false);

            var dirExists = await CreateDestinationDirectoryIfNotExistsAsync(tgDownloadSettings);
            if (!dirExists)
            {
                tgDownloadSettings2.SourceVm.Dto.FirstId = tgDownloadSettings2.SourceVm.Dto.Count;
                return;
            }

            tgDownloadSettings2.SourceVm.Dto.IsDownload = true;
            TgForumTopicSettings forumTopicSettings = new();
            Channel? channel = null;
            WTelegram.Types.ChatFullInfo? botChatFullInfo = null;

            if (tgDownloadSettings.Chat.Base is Channel)
                channel = tgDownloadSettings.Chat.Base as Channel;

            var appDto = await StorageManager.AppRepository.GetCurrentDtoAsync();
            if (appDto.UseClient && channel is not null)
            {
                isAccessToMessages = await TelegramCallAsync(() => Client.Channels_ReadMessageContents(channel));
            }
            if (appDto.UseBot && Bot is not null)
            {
                botChatFullInfo = await GetChatDetailsForBot(tgDownloadSettings2.SourceVm.Dto.Id, tgDownloadSettings2.SourceVm.Dto.UserName);
                if (botChatFullInfo is not null)
                {
                    isAccessToMessages = true;
                }
            }

            var sourceFirstId = tgDownloadSettings2.SourceVm.Dto.FirstId;
            // Get the last message ID in a chat 
            var sourceLastId = tgDownloadSettings2.SourceVm.Dto.Count = await GetChatLastMessageIdAsync(tgDownloadSettings2.SourceVm.Dto.Id);
            var entityLastId = await StorageManager.MessageRepository.GetLastIdAsync(tgDownloadSettings2.SourceVm.Dto.Id);
            if (entityLastId > sourceLastId)
                sourceLastId = (int)entityLastId;

            if (isAccessToMessages)
            {
                // Creating subdirectories
                if (tgDownloadSettings2.SourceVm.Dto.IsCreatingSubdirectories)
                {
                    if (Client is null && Bot is not null)
                        Client = Bot.Client;
                    if (Client is not null && channel is not null)
                    {
                        try
                        {
                            Messages_ForumTopics forumTopics = await TelegramCallAsync(() => Client.Channels_GetAllForumTopics(channel));
                            forumTopicSettings.SetTopics(forumTopics);
                            foreach (var topic in forumTopicSettings.Topics)
                            {
                                try
                                {
                                    var dir = Path.Combine(tgDownloadSettings2.SourceVm.Dto.Directory, topic.Title);
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

                List<Task> downloadTasks = [];
                var chatCache = new TgChatCache();
                var messageSettings = new TgMessageSettings() { CurrentChatId = tgDownloadSettings2.SourceVm.Dto.Id };
                chatCache.TryAddChat(messageSettings.CurrentChatId, tgDownloadSettings2.SourceVm.Dto.AccessHash, tgDownloadSettings2.SourceVm.Dto.Directory);
                chatCache.MarkAsSaved(messageSettings.CurrentChatId);

                while (sourceFirstId <= sourceLastId)
                {
                    if (appDto.UseClient)
                    {
                        if (Client is null || !tgDownloadSettings2.SourceVm.Dto.IsDownload || (Client is not null && Client.Disconnected))
                        {
                            tgDownloadSettings2.SourceVm.Dto.FirstId = sourceLastId;
                            tgDownloadSettings2.SourceVm.Dto.IsDownload = false;
                            downloadTasks.Clear();
                            break;
                        }
                        await TryCatchAsync(async () =>
                        {
                            if (Client is null) return;

                            var messages = channel is not null
                                ? await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyMessage(channel.id, sourceFirstId),
                                    factory: SafeFactory<TL.Messages_MessagesBase?>(async (ctx, ct) => await TelegramCallAsync(() => Client.Channels_GetMessages(channel, sourceFirstId))), 
                                    TgCacheUtils.CacheOptionsChannelMessages)
                                : await TelegramCallAsync(() => Client.GetMessages(tgDownloadSettings.Chat.Base, sourceFirstId));

                            await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(sourceLastId, sourceFirstId):#00.00} %");
                            // Check deleted messages and mark storage entity
                            var firstMessage = messages?.Messages.FirstOrDefault();
                            if (firstMessage is null)
                            {
                                // Mark message entity as deleted 
                                await CheckDeletedMessageAndMarkEntityAsync(tgDownloadSettings2.SourceVm.Dto.Id, sourceFirstId);
                            }
                            else
                            {
                                if (messages is not null)
                                {
                                    foreach (var message in messages.Messages)
                                    {
                                        // Check message exists
                                        if (message is MessageBase messageBase && messageBase.Date > DateTime.MinValue)
                                            downloadTasks.Add(ParseChatMessageAsync(tgDownloadSettings2, messageBase, chatCache, messageSettings, forumTopicSettings));
                                    }
                                }
                            }
                        });
                    }
                    else if (appDto.UseBot)
                    {
                        if (Bot is null || !tgDownloadSettings2.SourceVm.Dto.IsDownload || (Bot.Client is not null && Bot.Client.Disconnected))
                        {
                            tgDownloadSettings2.SourceVm.Dto.FirstId = sourceLastId;
                            tgDownloadSettings2.SourceVm.Dto.IsDownload = false;
                            downloadTasks.Clear();
                            break;
                        }
                        await TryCatchAsync(async () =>
                        {
                            if (Bot is null || botChatFullInfo is null) return;
                            var messages = await Bot.GetMessagesById(botChatFullInfo, [sourceFirstId]);
                            await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(sourceLastId, sourceFirstId):#00.00} %");
                            foreach (var message in messages.AsReadOnly())
                            {
                                // Check message exists
                                if (message.TLMessage is MessageBase messageBase && message.Date > DateTime.MinValue)
                                    downloadTasks.Add(ParseChatMessageAsync(tgDownloadSettings2, messageBase, chatCache, messageSettings, forumTopicSettings));
                            }

                            if (Bot.Client is null) return;
                            var messages2 = channel is not null
                                ? await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyMessage(channel.id, sourceFirstId),
                                    factory: SafeFactory<TL.Messages_MessagesBase?>(async (ctx, ct) => await TelegramCallAsync(() => Bot.Client.Channels_GetMessages(channel, sourceFirstId))), 
                                    TgCacheUtils.CacheOptionsChannelMessages)
                                : await TelegramCallAsync(() => Bot.Client.GetMessages(tgDownloadSettings.Chat.Base, sourceFirstId));

                            await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(sourceLastId, sourceFirstId):#00.00} %");
                            if (messages2 is not null)
                            {
                                foreach (var message in messages2.Messages)
                                {
                                    // Check message exists
                                    if (message is MessageBase messageBase && messageBase.Date > DateTime.MinValue)
                                        downloadTasks.Add(ParseChatMessageAsync(tgDownloadSettings2, messageBase, chatCache, messageSettings, forumTopicSettings));
                                }
                            }
                        });
                    }

                    // Count threads
                    sourceFirstId++;
                    if (downloadTasks.Count == tgDownloadSettings2.CountThreads || sourceFirstId >= sourceLastId)
                    {
                        await Task.WhenAll(downloadTasks);
                        downloadTasks.Clear();
                        messageSettings.ThreadNumber = 0;
                        // Check force stop parsing
                        if (CheckShouldStop)
                        {
                            tgDownloadSettings2.SourceVm.Dto.FirstId = sourceLastId;
                            break;
                        }
                    }
                }
                tgDownloadSettings2.SourceVm.Dto.FirstId = sourceFirstId > sourceLastId ? sourceLastId : sourceFirstId;
            }
            else
            {
                tgDownloadSettings2.SourceVm.Dto.FirstId = sourceLastId;
            }
            tgDownloadSettings2.SourceVm.Dto.IsDownload = false;
        }, async () => {
            _tgDownloadSettings = null;
            await FlushMessageBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: true);
            await FlushMessageRelationBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: true);
            await UpdateTitleAsync(string.Empty);
        });
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

    /// <summary> Set force stop parsing </summary>
    public void SetForceStopDownloading() => IsForceStopDownloading = true;

    public async Task MarkHistoryReadAsync()
    {
        var messageSettings = new TgMessageSettings();
        await TryCatchAsync(async () =>
        {
            await LoginUserAsync(isProxyUpdate: false);
        });

        await CollectAllChatsAsync();
        await UpdateStateMessageAsync("Mark as read all message in the channels: in the progress");
        await TryCatchAsync(async () =>
        {
            if (Client is not null)
            {
                foreach (var chatBase in EnumerableChats)
                {
                    await TryCatchAsync(async () =>
                    {
                        var isSuccess = await TelegramCallAsync(() => Client.ReadHistory(chatBase), messageSettings.IsThrow);
                        await UpdateStateMessageAsync(
                            $"Mark as read the source | {chatBase.ID} | " +
                            $"{(string.IsNullOrEmpty(chatBase.MainUsername) ? chatBase.Title : chatBase.MainUsername)}]: {(isSuccess ? "success" : "already read")}");
                    });
                }
            }
            else
            {
                await UpdateStateMessageAsync("Mark as read all messages: Client is not connected!");
            }
        });
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
        TgChatCache chatCache, TgMessageSettings messageSettings, TgForumTopicSettings forumTopicSettings)
    {
        // Check force stop parsing
        if (CheckShouldStop) return;
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

            // Add chat from Storage to Cache
            await AddChatFromStorageToCacheAsync(chatCache, messageSettings.CurrentChatId);
            // Get chat from Cache
            if (chatCache.TryGetChat(messageSettings.CurrentChatId, out var accessHash) &&
                (messageSettings.ParentMessageId == 0 || messageSettings.ParentMessageId != messageSettings.CurrentMessageId))
            {
                // Parse Telegram chat message core logic
                await ParseChatMessageCoreAsync(tgDownloadSettings, messageBase, chatCache, forumTopicSettings, message, rootSettings);

                var inputPeer = GetInputPeer(messageBase.Peer, accessHash);
                if (inputPeer is not null)
                {
                    // Get discussion message (main post)
                    if (tgDownloadSettings.SourceVm.Dto.IsParsingComments && message.replies?.replies > 0)
                    {
                        var discussionMessage = await TelegramCallAsync(() => Client.Messages_GetDiscussionMessage(inputPeer, messageSettings.CurrentMessageId), 
                            messageSettings.IsThrow);
                        if (discussionMessage is not null && discussionMessage.messages is not null && discussionMessage.messages.Length > 0)
                        {
                            var stop = false;

                            // Looking for a discussion supergroup in chats
                            TL.Channel? discussionChat = null;

                            // First, let's try to select group from the chat set
                            if (discussionMessage.chats is not null)
                                discussionChat = discussionMessage.chats.Values.OfType<TL.Channel>().FirstOrDefault(c => c.IsGroup);

                            // Fallback: if group is not found, we will try to take linked_chat_id from the source channel
                            if (discussionChat is null && messageBase.Peer is PeerChannel srcPc)
                                discussionChat = discussionMessage.chats?.Values.OfType<TL.Channel>().FirstOrDefault(c => c.ID != srcPc.ID && c.IsGroup);

                            // As a last resort — request by access_hash
                            if (discussionChat is null && inputPeer is InputPeerChannel ipc)
                            {
                                var channels = await TelegramCallAsync(() =>
                                    Client.Channels_GetChannels(new InputChannel(ipc.ID, ipc.access_hash)), messageSettings.IsThrow);
                                discussionChat = channels?.chats?.Values.OfType<TL.Channel>().FirstOrDefault(c => c.IsGroup);
                            }
                            if (discussionChat is null) return;

                            // Cache and save the access_hash of the supergroup
                            CheckEnumerableChatCache(discussionChat);
                            await SaveAtStorageAccessHashForChatAsync(chatCache, discussionChat, parentChatId: messageSettings.CurrentChatId);

                            // Building peer supergroups
                            if (!chatCache.TryGetChat(discussionChat.ID, out var discussionHash)) return;

                            var discussionPeer = GetInputPeer(new PeerChannel { channel_id = discussionChat.ID }, discussionHash);
                            if (discussionPeer is null) return;

                            // Find the `thread head` in the messages (specifically, the message from the supergroup).
                            var headMessage = discussionMessage.messages.OfType<Message>().FirstOrDefault(m => m.Peer is PeerChannel pc && pc.ID == discussionChat.ID);
                            if (headMessage is null) return;

                            // Save the `thread header` itself as plain text (optional)
                            await SaveMessagesAsync(tgDownloadSettings, rootSettings, headMessage.Date, size: 0,
                                headMessage.message, TgEnumMessageType.Message, isRetry: false, userId: headMessage.From?.ID ?? 0);

                            // Paginate replies to the thread strictly by (discussionPeer, headMessage.ID)
                            int offsetId = 0;
                            const int limit = 100;

                            while (true)
                            {
                                if (CheckShouldStop) { stop = true; break; }

                                var repliesBase = await TelegramCallAsync(() => Client.Messages_GetReplies(peer: discussionPeer,
                                    msg_id: headMessage.ID,         // IMPORTANT: ID from the supergroup
                                    offset_id: offsetId, offset_date: default, add_offset: 0, limit: limit, max_id: 0, min_id: 0, hash: 0L), messageSettings.IsThrow);
                                if (repliesBase is null || repliesBase.Count == 0) break;

                                foreach (var replyMessage in repliesBase.Messages)
                                {
                                    if (CheckShouldStop) { stop = true; break; }

                                    if (replyMessage is Message rpl)
                                    {
                                        var replySettings = messageSettings.Clone();
                                        replySettings.ParentChatId = replySettings.CurrentChatId;
                                        replySettings.ParentMessageId = replySettings.CurrentMessageId;
                                        replySettings.CurrentChatId = replyMessage.Peer.ID; // will be the ID of the supergroup
                                        replySettings.CurrentMessageId = replyMessage.ID;
                                        await ParseChatMessageCoreAsync(tgDownloadSettings, rpl, chatCache, forumTopicSettings, rpl, replySettings);
                                    }
                                }

                                offsetId = repliesBase.Messages.Last().ID;
                                if (repliesBase.Messages.Length < limit) break;
                            }

                            if (stop) return;

                            // TODO: fix here
                            //foreach (var rootMessage in discussionMessage.messages)
                            //{
                            //    // Check force stop parsing
                            //    if (CheckShouldStop) { stop = true; break; }
                            //    // Add chat from Storage to Cache
                            //    await AddChatFromStorageToCacheAsync(chatCache, rootMessage.Peer.ID);
                            //    // Check the exists chat directory
                            //    await CheckExistsChatWithDirectoryAsync(chatCache, messageSettings, accessHash, rootMessage);
                            //    // Get chat from Cache
                            //    if (chatCache.TryGetChat(rootMessage.Peer.ID, out var discussionHash))
                            //    {
                            //        var discussionPeer = GetInputPeer(rootMessage.Peer, discussionHash);
                            //        if (discussionPeer == null) return;

                            //        TL.Channel? discussionChannel = null;
                            //        if (discussionMessage.chats is not null && 
                            //            discussionMessage.chats.TryGetValue(rootMessage.Peer.ID, out var chatBase) && chatBase is TL.Channel ch)
                            //        {
                            //            discussionChannel = ch;
                            //        }
                            //        else
                            //        {
                            //            // fallback: get channel via API
                            //            if (discussionPeer is InputPeerChannel ipc)
                            //            {
                            //                var channels = await TelegramCallAsync(() => Client.Channels_GetChannels(
                            //                    new InputChannel(ipc.ID, ipc.access_hash)), messageSettings.IsThrow);
                            //                discussionChannel = channels?.chats?.Values.OfType<TL.Channel>().FirstOrDefault(c => c.ID == rootMessage.Peer.ID);
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
                            //                            replySettings.CurrentChatId = replyMessage.Peer.ID;
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
                            if (stop) return;
                        }
                    }
                }
            }
        });
    }

    /// <summary> Check the exists chat directory </summary>
    private async Task CheckExistsChatWithDirectoryAsync(TgChatCache chatCache, TgMessageSettings messageSettings, MessageBase rootMessage)
    {
        if (chatCache.CheckExistsDirectory(rootMessage.Peer.ID)) return;

        // Check if the chat is already in the enumerable collections
        var channelFind = EnumerableChannels.FirstOrDefault(x => x.ID == rootMessage.Peer.ID);
        if (channelFind is not null)
        {
            CheckEnumerableChatCache(channelFind);
            await SaveAtStorageAccessHashForChatAsync(chatCache, channelFind, messageSettings.CurrentChatId);
            return;
        }

        // Check if the chat is already in the enumerable groups
        var groupFind = EnumerableGroups.FirstOrDefault(x => x.ID == rootMessage.Peer.ID);
        if (groupFind is not null)
        {
            CheckEnumerableChatCache(groupFind);
            await SaveAtStorageAccessHashForChatAsync(chatCache, groupFind, messageSettings.CurrentChatId);
        }
    }

    /// <summary> Parse Telegram chat message core logic </summary>
    private async Task ParseChatMessageCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings, MessageBase messageBase, TgChatCache chatCache, 
        TgForumTopicSettings forumTopicSettings, Message message, TgMessageSettings messageSettings)
    {
        var chatId = messageSettings.CurrentChatId;
        var messageId = messageSettings.CurrentMessageId;

        try
        {
            // Anti-stampede: if a pair of threads process the same message, only one will be executed
            _ = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyMessageProcessed(chatId, messageId),
                factory: SafeFactory<bool>(async (ctx, ct) =>
                {
                    // Parse documents and photos
                    if ((message.flags & Message.Flags.has_media) is not 0)
                    {
                        await DownloadMediaFromMessageAsync(tgDownloadSettings, messageBase, message.media, chatCache, messageSettings, forumTopicSettings);
                    }
                    // Save plain text message
                    else
                    {
                        //var authorId = messageBase.From?.ID ?? 0; // Do not substitute ChatId if there is no author
                        var authorId = messageBase.From?.ID ?? messageSettings.CurrentChatId;
                        await SaveMessagesAsync(tgDownloadSettings, messageSettings, message.Date, size: 0,
                            message.message, TgEnumMessageType.Message, isRetry: false, userId: authorId);
                    }
                    return true;
                }), TgCacheUtils.CacheOptionsProcessMessage);
        }
        finally
        {
            var messagesCount = await TryGetCacheChatLastCountSafeAsync(chatId);
            // Update download progress for the message
            await UpdateChatViewModelAsync(chatId, messageId, messagesCount, $"Reading the message {messageId} from {messagesCount}");
        }
    }

    private async Task AddChatFromStorageToCacheAsync(TgChatCache chatCache, long chatId)
    {
        if (!chatCache.TryGetChat(chatId, out _))
        {
            var chatResult = await StorageManager.SourceRepository.GetByDtoAsync(new() { Id = chatId });
            if (chatResult.IsExists && chatResult.Item is { } chatEntity)
            {
                chatCache.TryAddChat(chatEntity.Id, chatEntity.AccessHash, chatEntity.Directory ?? string.Empty);
            }
        }
    }

    /// <summary> Check if the chat is already in the enumerable collections and add it if not </summary>
    private void CheckEnumerableChatCache(ChatBase chatBase)
    {
        if (!EnumerableChats.Contains(chatBase))
            EnumerableChats.Enqueue(chatBase);

        if (chatBase is Channel channel)
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

    /// <summary> Get input Peer </summary>
    private static InputPeer? GetInputPeer(Peer peer, long accessHash)
    {
        InputPeer? inputPeer = null;
        if (peer is PeerUser peerUser)
            inputPeer = new InputPeerUser(peerUser.ID, accessHash);
        else if (peer is PeerChat peerChat)
            inputPeer = new InputPeerChat(peerChat.ID);
        else if (peer is PeerChannel peerChannel)
            inputPeer = new InputPeerChannel(peerChannel.ID, accessHash);
        return inputPeer;
    }

    /// <summary> Download comment data </summary>
    private async Task DownloadCommentAsync(TgDownloadSettingsViewModel tgDownloadSettings, MessageBase commentBase, TgChatCache chatCache,
        TgMessageSettings messageSettings, TgForumTopicSettings forumTopicSettings)
    {
        var cloneSettings = messageSettings.Clone();
        //cloneSettings.ParentId = messageSettings.MessageId;
        cloneSettings.ParentMessageId = commentBase.ID;
        await ParseChatMessageAsync(tgDownloadSettings, commentBase, chatCache, cloneSettings, forumTopicSettings);
        var messagesCount = await GetChannelMessageIdLastCoreAsync(cloneSettings.CurrentChatId);
        // Update download progress for the comment
        await UpdateChatViewModelAsync(cloneSettings.CurrentChatId, cloneSettings.CurrentMessageId, messagesCount,
            $"Reading the comment {cloneSettings.CurrentMessageId} from {messagesCount}");
    }

    private TgMediaInfoModel GetMediaInfo(MessageMedia messageMedia, TgDownloadSettingsViewModel tgDownloadSettings, MessageBase messageBase,
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
                    var fileName = tgDownloadSettings.SourceVm.Dto.IsFileNamingByMessage && messageBase is Message message
                        ? $"{message.message}.{extensionName}" : document.Filename;
                    if (!string.IsNullOrEmpty(document.Filename) && CheckFileAtFilter(document.Filename, extensionName, document.size))
                    {
                        mediaInfo = new(fileName, document.size, document.date);
                        break;
                    }
                    if (document.attributes.Length > 0)
                    {
                        if (document.attributes.Any(x => x is DocumentAttributeVideo))
                        {
                            extensionName = "mp4";
                            if (CheckFileAtFilter(string.Empty, extensionName, document.size))
                            {
                                mediaInfo = new($"{document.ID}.{extensionName}", document.size, document.date);
                                break;
                            }
                        }
                        if (document.attributes.Any(x => x is DocumentAttributeAudio))
                        {
                            extensionName = "mp3";
                            if (CheckFileAtFilter(string.Empty, extensionName, document.size))
                            {
                                mediaInfo = new($"{document.ID}.{extensionName}", document.size, document.date);
                                break;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(document.Filename) && CheckFileAtFilter(string.Empty, extensionName, document.size))
                    {
                        mediaInfo = new($"{messageBase.ID}.{extensionName}", document.size, document.date);
                        break;
                    }
                }
                break;
            case MessageMediaPhoto mediaPhoto:
                if (mediaPhoto is { photo: Photo photo })
                {
                    extensionName = "jpg";
                    //return photo.sizes.Select(x => ($"{photo.ID} {x.Width}x{x.Height}.{GetPhotoExt(x.Type)}", Convert.ToInt64(x.FileSize), photo.date, string.Empty, string.Empty)).ToArray();
                    //var fileName = tgDownloadSettings.IsJoinFileNameWithMessageId && messageBase is TL.Message message
                    //	? $"{message.message}.{extensionName}" : $"{photo.ID}.{extensionName}";
                    var fileName = $"{photo.ID}.{extensionName}";
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
        // Join with directory
        //mediaInfo.LocalPathOnly = tgDownloadSettings.SourceVm.Dto.Directory;
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
    private async Task DownloadMediaFromMessageAsync(TgDownloadSettingsViewModel tgDownloadSettings, MessageBase messageBase, MessageMedia messageMedia,
        TgChatCache chatCache, TgMessageSettings messageSettings, TgForumTopicSettings forumTopicSettings)
    {
        var mediaInfo = GetMediaInfo(messageMedia, tgDownloadSettings, messageBase, chatCache, messageSettings, forumTopicSettings);
        if (string.IsNullOrEmpty(mediaInfo.LocalNameOnly)) return;

        // Delete files
        await DeleteExistsFilesAsync(tgDownloadSettings, mediaInfo);

        // Move exists file at current directory
        await MoveExistsFilesAtCurrentDirAsync(tgDownloadSettings, mediaInfo);

        if (Client is null && Bot is not null)
            Client = Bot.Client;

        // Download new file
        if (!File.Exists(mediaInfo.LocalPathWithNumber))
        {
            await using var localFileStream = File.Create(mediaInfo.LocalPathWithNumber);
            if (Client is not null)
            {
                switch (messageMedia)
                {
                    case MessageMediaDocument { document: Document doc }:
                        await TelegramCallAsync(() => Client.DownloadFileAsync(doc, localFileStream, null, ClientProgressForFile(messageSettings, mediaInfo.LocalNameWithNumber)));
                        await SaveMessagesAsync(tgDownloadSettings, messageSettings, mediaInfo.DtCreate, mediaInfo.RemoteSize, mediaInfo.RemoteName,
                            TgEnumMessageType.Document, isRetry: false, userId: messageBase.From?.ID ?? 0);
                        break;
                    case MessageMediaPhoto { photo: Photo photo }:
                        //var fileReferenceBase64 = Convert.ToBase64String(photo.file_reference);
                        //var fileReferenceHex = BitConverter.ToString(photo.file_reference).Replace("-", "");
                        await TelegramCallAsync(() => Client.DownloadFileAsync(photo, localFileStream, (PhotoSizeBase?)null,
                            ClientProgressForFile(messageSettings, mediaInfo.LocalNameWithNumber)));
                        break;
                }
            }
            localFileStream.Close();
        }

        // Save message
        if (Client is not null)
        {
            switch (messageMedia)
            {
                case MessageMediaDocument mediaDocument:
                    if ((mediaDocument.flags & MessageMediaDocument.Flags.has_document) is not 0 && mediaDocument.document is Document document)
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
                        await StorageManager.DocumentRepository.SaveAsync(doc);
                    }
                    await SaveMessagesAsync(tgDownloadSettings, messageSettings, mediaInfo.DtCreate, mediaInfo.RemoteSize, mediaInfo.RemoteName,
                        TgEnumMessageType.Document, isRetry: false, userId: messageBase.From?.ID ?? 0);
                    break;
                case MessageMediaPhoto mediaPhoto:
                    var messageStr = string.Empty;
                    if (messageBase is TL.Message message)
                        messageStr = message.message;
                    await SaveMessagesAsync(tgDownloadSettings, messageSettings, mediaInfo.DtCreate, mediaInfo.RemoteSize,
                        $"{messageStr} | {mediaInfo.LocalPathWithNumber.Replace(tgDownloadSettings.SourceVm.Dto.Directory, string.Empty)}",
                        TgEnumMessageType.Photo, isRetry: false, userId: messageBase.From?.ID ?? 0);
                    break;
            }
            // Save user info
        }

        // Set file date time
        if (File.Exists(mediaInfo.LocalPathWithNumber))
        {
            File.SetCreationTimeUtc(mediaInfo.LocalPathWithNumber, mediaInfo.DtCreate);
            File.SetLastAccessTimeUtc(mediaInfo.LocalPathWithNumber, mediaInfo.DtCreate);
            File.SetLastWriteTimeUtc(mediaInfo.LocalPathWithNumber, mediaInfo.DtCreate);
        }
    }

    private async Task DeleteExistsFilesAsync(TgDownloadSettingsViewModel tgDownloadSettings, TgMediaInfoModel mediaInfo)
    {
        if (!tgDownloadSettings.IsRewriteFiles) return;

        await TryCatchAsync(async () =>
        {
            if (File.Exists(mediaInfo.LocalPathWithNumber))
            {
                var fileSize = TgFileUtils.CalculateFileSize(mediaInfo.LocalPathWithNumber);
                // If file size is less then original size
                if (fileSize == 0 || fileSize < mediaInfo.RemoteSize)
                    File.Delete(mediaInfo.LocalPathWithNumber);
            }
            await Task.CompletedTask;
        });
    }

    /// <summary> Move existing files at the current directory </summary>
    private async Task MoveExistsFilesAtCurrentDirAsync(TgDownloadSettingsViewModel tgDownloadSettings, TgMediaInfoModel mediaInfo)
    {
        if (!tgDownloadSettings.IsJoinFileNameWithMessageId)
            return;
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
        });
    }

    /// <summary> Save messages to the storage </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task SaveMessagesAsync(TgDownloadSettingsViewModel tgDownloadSettings, TgMessageSettings messageSettings,
        DateTime dtCreated, long size, string message, TgEnumMessageType messageType, bool isRetry, long userId)
    {
        if (!tgDownloadSettings.IsSaveMessages) return;

        await TgCacheUtils.SaveLock.WaitAsync();
        try
        {
            // Save message entity
            if (userId > 0 && messageSettings.CurrentMessageId > 0 && messageSettings.CurrentChatId > 0)
            {
                var messageItem = new TgEfMessageEntity
                {
                    Id = messageSettings.CurrentMessageId,
                    SourceId = messageSettings.CurrentChatId,
                    DtCreated = dtCreated,
                    Type = messageType,
                    Size = size,
                    Message = message,
                    UserId = userId,
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
                await UpdateStateFileAsync(messageSettings.CurrentChatId, messageSettings.CurrentMessageId, string.Empty, 0, 0, 0, false, messageSettings.ThreadNumber);
            }
        }
        catch (Exception ex)
        {
            if (!isRetry)
            {
                await Task.Delay(TimeSpan.FromSeconds(FloodControlService.WaitFallbackFast));
                await SaveMessagesAsync(tgDownloadSettings, messageSettings, dtCreated, size, message, messageType, isRetry: true, userId);
            }
            else
            {
                TgLogUtils.WriteExceptionWithMessage(ex, TgConstants.LogTypeStorage);
                await SetClientExceptionAsync(ex);
            }
        }
        finally
        {
            TgCacheUtils.SaveLock.Release();

            //await ClientProgressForMessageThreadAsync(messageSettings.CurrentChatId, messageSettings.CurrentMessageId, message, isStartTask: true, messageSettings.ThreadNumber);
            await ClientProgressForMessageThreadAsync(messageSettings.CurrentChatId, messageSettings.CurrentMessageId, message, isStartTask: false, messageSettings.ThreadNumber);
            
            await FlushMessageBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: false);
            await FlushMessageRelationBufferAsync(tgDownloadSettings.IsSaveMessages, tgDownloadSettings.IsRewriteMessages, isForce: false);
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
        var sw = Stopwatch.StartNew();
        var isStartTask = true;
        return async (transmitted, size) =>
        {
            if (string.IsNullOrEmpty(fileName))
            {
                isStartTask = false;
            }
            else
            {
                isStartTask = true;
                var fileSpeed = transmitted <= 0 || sw.Elapsed.Seconds <= 0 ? 0 : transmitted / sw.Elapsed.Seconds;
                await UpdateStateFileAsync(messageSettings.CurrentChatId, messageSettings.CurrentMessageId,
                    Path.GetFileName(fileName), size > 0 ? size : 0, transmitted, fileSpeed > 0 ? fileSpeed : 0, isStartTask, messageSettings.ThreadNumber);
            }
        };
    }

    private async Task ClientProgressForMessageThreadAsync(long sourceId, int messageId, string message, bool isStartTask, int threadNumber) =>
        await UpdateStateMessageThreadAsync(sourceId, messageId, message, isStartTask, threadNumber);

    private async Task<long> GetPeerIdAsync(string userName) => (await TelegramCallAsync(() => Client.Contacts_ResolveUsername(userName))).peer.ID;

    public virtual async Task LoginUserAsync(bool isProxyUpdate) => await UseOverrideMethodAsync();

    public async Task DisconnectClientAsync()
    {
        try
        {
            IsProxyUsage = false;
            await UpdateStateProxyAsync(TgLocale.ProxyIsDisconnect);
            Me = null;
            if (Client is not null)
            {
                Client.OnUpdates -= OnUpdatesClientAsync;
                Client.OnOther -= OnClientOtherAsync;
                await Client.DisposeAsync();
                Client = null;
            }
            ClientException = new();
            await CheckClientConnectionReadyAsync();
            await AfterClientConnectAsync();
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            await SetClientExceptionAsync(ex);
        }
    }

    public async Task DisconnectBotAsync()
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
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            await SetClientExceptionAsync(ex);
        }
    }

    public async Task ReleaseBuffersAsync()
    {
        _chatBuffer.Dispose();
        _storyBuffer.Dispose();
        _userBuffer.Dispose();
        Cache.Dispose();
        await Task.CompletedTask;
    }

    protected async Task SetClientExceptionAsync(Exception ex,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
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

    private async Task<T?> TryCatchAsync<T>(Func<Task<T>> call, Func<Task>? callFinally = null, 
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        try
        {
            for (int attempt = 0; attempt < FloodControlService.MaxRetryCount; attempt++)
            {
                try
                {
                    return await call();
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
        finally
        {
            if (callFinally is not null)
                await callFinally.Invoke();
        }

        TgLogUtils.WriteLog($"[{memberName}] All retry attempts failed.");
        return default;
    }

    private async Task TryCatchAsync(Func<Task> call, Func<Task>? callFinally = null,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        try
        {
            for (int attempt = 0; attempt < FloodControlService.MaxRetryCount; attempt++)
            {
                try
                {
                    await call();
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
            if (chatBase is Channel channel && !string.IsNullOrEmpty(channel.username))
                return channel.username;
        }
        return string.Empty;
    }

    /// <inheritdoc />
    public (string, string) GetChatUserLink(long chatId, int messageId) =>
        TgStringUtils.FormatChatLink(GetChatUserName(chatId), chatId, messageId);

    /// <inheritdoc />
    public (string, string) GetChatUserLink(long chatId) =>
        TgStringUtils.FormatChatLink(GetChatUserName(chatId), chatId);

    /// <inheritdoc />
    public async Task<(string, string)> GetUserLink(long chatId, int messageId, Peer? peer)
    {
        if (peer is not PeerUser peerUser) return GetChatUserLink(chatId);
        if (chatId == peerUser.user_id) return GetChatUserLink(chatId);

        try
        {
            // Optimization: check cache
            if (UserCache.TryGetValue(peerUser.user_id, out var user))
                return TgStringUtils.FormatUserLink(user.username, user.id, string.Join(" ", user.first_name, user.last_name));

            // Retrieving data through a chat participant
            var userData = await GetParticipantAsync(chatId, peerUser.user_id);
            if (userData is null)
                return GetChatUserLink(chatId);

            // Optimization: update cache
            UserCache.TryAdd(userData.id, userData);
            return TgStringUtils.FormatUserLink(userData.username, userData.id, string.Join(" ", userData.first_name, userData.last_name));
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
        return GetChatUserLink(chatId);
    }

    /// <summary> Get input channel </summary>
    private InputChannel? GetInputChannelFromUserChats(long chatId)
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
                var result = new InputChannel(chatId, channelAccessHash);

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
    private ChatBase? GetChatBaseFromUserChats(long chatId)
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

    private InputChannel? GetInputChannelFromChatBase(ChatBase? chatBase)
    {
        if (chatBase is null) return null;

        // Optimization: check cache
        if (InputChannelCache.TryGetValue(chatBase.ID, out var inputChannel))
            return inputChannel;

        long channelAccessHash = 0;
        if (chatBase is TL.Channel channel)
            channelAccessHash = channel.access_hash;
        var result = new InputChannel(chatBase.ID, channelAccessHash);

        // Optimization: update cache
        InputChannelCache.TryAdd(chatBase.ID, result);

        return result;
    }

    /// <inheritdoc />
    public async Task<User?> GetParticipantAsync(long chatId, long? userId)
    {
        var chatBase = GetChatBaseFromUserChats(chatId);
        var inputChannel = GetInputChannelFromChatBase(chatBase);
        if (inputChannel is null || inputChannel.access_hash == 0) return null;

        // Optimization: check cache
        if (userId is not null)
        {
            if (UserCache.TryGetValue((long)userId, out var user))
                return user;
            // Get one participant: need access hash
            try
            {
                var response = await TelegramCallAsync(() => Client.Channels_GetParticipant(inputChannel, new InputUser((long)userId, access_hash: 0)));
                if (response is not null)
                {
                    // Optimization: update cache
                    var userData = response.users.FirstOrDefault(x => x.Value.id == userId).Value;
                    if (userData is not null)
                    {
                        UserCache.TryAdd((long)userId, userData);
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
                var participants = await TelegramCallAsync(() => Client.Channels_GetParticipants(inputChannel, filter, offset, limit, 0));
                if (participants is null || participants.participants.Length == 0)
                    break;

                foreach (var userItem in participants.users)
                {
                    if (userId is not null)
                    {
                        if (userItem.Value.id == userId)
                        {
                            // Optimization: update cache
                            UserCache.TryAdd(userItem.Value.id, userItem.Value);
                            return userItem.Value;
                        }
                    }
                    else
                    {
                        // Optimization: update cache
                        UserCache.TryAdd(userItem.Value.id, userItem.Value);
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
    public async Task<List<TL.User>> GetParticipantsAsync(long chatId)
    {
        await GetChatDetailsByClientAsync(chatId);

        UserCache.Clear();
        await GetParticipantAsync(chatId, null);

        // Sort participants
        var userList = UserCache
            .OrderByDescending(x => x.Key == Client?.UserId)
            .ThenBy(x => x.Value.LastSeenAgo)
            .Select(x => x.Value)
            .ToList();

        return userList;
    }

    /// <inheritdoc />
    public async Task MakeFuncWithMessagesAsync(TgDownloadSettingsViewModel tgDownloadSettings,
        long chatId, Func<TgDownloadSettingsViewModel, ChatBase, MessageBase, Task> func)
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
            int maxId = await GetChatLastMessageIdAsync(chatId);
            while (true)
            {
                var messages = await TelegramCallAsync(() => Client.Messages_GetHistory(inputChannel, offset_id: 0, limit: limit,
                    max_id: maxId, min_id: minId, add_offset: offsetId, hash: inputChannel.access_hash));
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
    public async Task<int> GetChatLastMessageIdAsync(long chatId)
    {
        var chatBase = GetChatBaseFromUserChats(chatId);
        if (chatBase is null) return 0;

        var inputChannel = GetInputChannelFromChatBase(chatBase);
        if (inputChannel is null || inputChannel.access_hash == 0) return 0;

        // Get
        try
        {
            var messages = await TelegramCallAsync(() => Client.Messages_GetHistory(inputChannel, offset_id: 0, limit: 1,
                max_id: 0, min_id: 0, add_offset: 0, hash: inputChannel.access_hash));
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
        UserCache.Clear();
    }

    /// <inheritdoc />
    public async Task<TgChatDetailsDto> GetChatDetailsByClientAsync(string userName, TgDownloadSettingsViewModel tgDownloadSettings)
    {
        userName = TgStringUtils.NormilizeTgName(userName, isAddAt: false);

        tgDownloadSettings.SourceVm = new TgEfSourceViewModel(TgGlobalTools.Container) { Dto = new TgEfSourceDto { UserName = userName } };
        await CreateChatBaseCoreAsync(tgDownloadSettings);

        if (Client is null) return new();

        // Get details about a public chat (even if client is not a member of that chat)
        if (tgDownloadSettings.Chat.Base is not null)
        {
            var chatFull = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChat(tgDownloadSettings.Chat.Base.ID),
                factory: SafeFactory<TL.Messages_ChatFull?>(async (ctx, ct) => await TelegramCallAsync(() => Client?.GetFullChat(tgDownloadSettings.Chat.Base) ?? default!)), 
                TgCacheUtils.CacheOptionsFullChat);
            WTelegram.Types.ChatFullInfo? chatDetails;
            if (chatFull is null)
                TgLog.WriteLine(TgLocale.TgGetChatDetailsError);
            else
            {
                chatDetails = ConvertToChatFullInfo(chatFull);
                await GetParticipantsAsync(chatDetails.Id);
                return FillChatDetailsDto(chatDetails);
            }
        }
        return new();
    }

    /// <inheritdoc />
    public async Task<TgChatDetailsDto> GetChatDetailsByBotAsync(string userName)
    {
        userName = TgStringUtils.NormilizeTgName(userName, isAddAt: false);

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

    private async Task<TgChatDetailsDto> GetChatDetailsByClientCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await CreateChatBaseCoreAsync(tgDownloadSettings);

        if (Client is null) return new();

        // Get details about a public chat (even if client is not a member of that chat)
        if (tgDownloadSettings.Chat.Base is not null)
        {
            var chatFull = await Cache.GetOrSetAsync(TgCacheUtils.GetCacheKeyFullChat(tgDownloadSettings.Chat.Base.ID),
                factory: SafeFactory<TL.Messages_ChatFull?>(async (ctx, ct) => await TelegramCallAsync(() => Client?.GetFullChat(tgDownloadSettings.Chat.Base) ?? default!)), 
                TgCacheUtils.CacheOptionsFullChat);
            WTelegram.Types.ChatFullInfo? chatDetails;
            if (chatFull is null)
                TgLog.WriteLine(TgLocale.TgGetChatDetailsError);
            else
            {
                chatDetails = ConvertToChatFullInfo(chatFull);
                return FillChatDetailsDto(chatDetails);
            }
        }
        return new();
    }

    /// <inheritdoc />
    public async Task<TgChatDetailsDto> GetChatDetailsByClientAsync(Guid uid)
    {
        if (uid == Guid.Empty) return new();

        var dto = await StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == uid);
        var tgDownloadSettings = new TgDownloadSettingsViewModel { SourceVm = new TgEfSourceViewModel(TgGlobalTools.Container) { Dto = dto } };
        return await GetChatDetailsByClientCoreAsync(tgDownloadSettings);
    }

    /// <inheritdoc />
    public async Task<TgChatDetailsDto> GetChatDetailsByClientAsync(long id)
    {
        if (id <= 0) return new();

        var dto = await StorageManager.SourceRepository.GetDtoAsync(x => x.Id == id);
        var tgDownloadSettings = new TgDownloadSettingsViewModel { SourceVm = new TgEfSourceViewModel(TgGlobalTools.Container) { Dto = dto } };
        return await GetChatDetailsByClientCoreAsync(tgDownloadSettings);
    }

    /// <inheritdoc />
    public WTelegram.Types.ChatFullInfo ConvertToChatFullInfo(TL.Messages_ChatFull messagesChatFull)
    {
        var chatFullInfo = new WTelegram.Types.ChatFullInfo();

        if (messagesChatFull.chats.Select(x => x.Value).FirstOrDefault() is not ChatBase chatBase)
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

        FillShatDetailsPermissions(chatDetailsDto, chatDetails);

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
            if (messagesChatFull.chats.Select(x => x.Value).FirstOrDefault() is ChatBase chatBase)
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

    private static void FillShatDetailsPermissions(TgChatDetailsDto chatDetailsDto, WTelegram.Types.ChatFullInfo chatDetails)
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
