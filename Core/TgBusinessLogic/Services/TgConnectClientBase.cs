// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TL;
using WTelegram;

namespace TgBusinessLogic.Services;

/// <summary> Base connection client </summary>
public abstract partial class TgConnectClientBase : TgWebDisposable, ITgConnectClient
{
    #region Public and private fields, properties, constructor

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

    public Dictionary<long, ChatBase> DicChatsAll { get; private set; } = default!;
    public Dictionary<long, User> DicUsersAll { get; private set; } = default!;
    public Dictionary<long, StoryItem> DicStoriesAll { get; private set; } = default!;
    public Dictionary<long, ChatBase> DicChatsUpdated { get; private set; } = default!;
    public Dictionary<long, User> DicUsersUpdated { get; private set; } = default!;

    public ConcurrentQueue<Channel> EnumerableChannels { get; private set; } = default!;
    public ConcurrentQueue<Channel> EnumerableGroups { get; private set; } = default!;
    public ConcurrentQueue<ChatBase> EnumerableChats { get; private set; } = default!;
    public ConcurrentQueue<ChatBase> EnumerableSmallGroups { get; private set; } = default!;
    public ConcurrentQueue<User> EnumerableUsers { get; private set; } = default!;
    public ConcurrentQueue<StoryItem> EnumerableStories { get; private set; } = default!;

    public Dictionary<long, InputChannel> InputChannelCache { get; private set; } = [];
    public Dictionary<long, ChatBase> ChatCache { get; private set; } = [];
    public Dictionary<long, User> UserCache { get; private set; } = [];

    public bool IsClientUpdateStatus { get; set; }
    public bool IsBotUpdateStatus { get; set; }
    public bool IsForceStopDownloading { get; set; } = default!;
    private IEnumerable<TgEfFilterDto> Filters { get; set; } = default!;
    public Func<string, Task> UpdateTitleAsync { get; private set; } = default!;
    public Func<string, Task> UpdateStateProxyAsync { get; private set; } = default!;
    public Func<long, int, int, string, Task> UpdateStateSourceAsync { get; private set; } = default!;
    public Func<long, string, string, string, Task> UpdateStateContactAsync { get; private set; } = default!;
    public Func<long, int, int, string, Task> UpdateStateStoryAsync { get; private set; } = default!;
    public Func<string, int, string, string, Task> UpdateStateExceptionAsync { get; private set; } = default!;
    public Func<Exception, Task> UpdateExceptionAsync { get; private set; } = default!;
    public Func<string, Task> UpdateStateExceptionShortAsync { get; private set; } = default!;
    public Func<Task> AfterClientConnectAsync { get; private set; } = default!;
    public Func<long, Task> UpdateStateItemSourceAsync { get; private set; } = default!;
    public Func<long, int, string, long, long, long, bool, int, Task> UpdateStateFileAsync { get; private set; } = default!;
    public Func<string, Task> UpdateStateMessageAsync { get; private set; } = default!;
    public Func<long, int, string, bool, int, Task> UpdateStateMessageThreadAsync { get; private set; } = default!;

    public Microsoft.Data.Sqlite.SqliteConnection? BotSqlConnection { get; private set; } = default!;

    private List<TgEfMessageEntity> MessageEntities { get; set; } = default!;
    private int BatchMessagesCount { get; set; } = default!;
    private int MaxRetryCount { get; set; } = 6;
    private int DelayBetweenRetriyCount { get; set; } = 10_000;

    protected ITgStorageManager StorageManager { get; set; } = default!;

    protected TgConnectClientBase(ITgStorageManager storageManager) : base()
    {
        InitializeClient();

        StorageManager = storageManager;
    }

    protected TgConnectClientBase(IWebHostEnvironment webHostEnvironment, ITgStorageManager storageManager) : base(webHostEnvironment)
    {
        InitializeClient();

        StorageManager = storageManager;
    }

    #endregion

    #region IDisposable

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        var tasks = new List<Task>
        {
            DisconnectClientAsync(),
            DisconnectBotAsync()
        };
        Task.WaitAll(tasks);
    }

    /// <summary> Release unmanaged resources </summary>
    public override void ReleaseUnmanagedResources()
    {
        //
    }

    #endregion

    #region Public and private methods

    public string ToDebugString() => $"{TgDataUtils.GetIsReady(IsReady)} | {Me}";

    private void InitializeClient()
    {
        DicChatsAll = [];
        DicUsersAll = [];
        DicStoriesAll = [];
        DicChatsUpdated = [];
        DicUsersUpdated = [];
        EnumerableChannels = [];
        EnumerableChats = [];
        EnumerableGroups = [];
        EnumerableSmallGroups = [];
        EnumerableUsers = [];
        EnumerableStories = [];
        ClientException = new();
        ProxyException = new();
        Filters = [];
        MessageEntities = [];

        UpdateTitleAsync = _ => Task.CompletedTask;
        UpdateStateProxyAsync = _ => Task.CompletedTask;
        UpdateStateExceptionAsync = (_, _, _, _) => Task.CompletedTask;
        UpdateExceptionAsync = _ => Task.CompletedTask;
        UpdateStateExceptionShortAsync = _ => Task.CompletedTask;
        UpdateStateSourceAsync = (_, _, _, _) => Task.CompletedTask;
        UpdateStateContactAsync = (_, _, _, _) => Task.CompletedTask;
        UpdateStateStoryAsync = (_, _, _, _) => Task.CompletedTask;
        AfterClientConnectAsync = () => Task.CompletedTask;
        UpdateStateItemSourceAsync = _ => Task.CompletedTask;
        UpdateStateFileAsync = (_, _, _, _, _, _, _, _) => Task.CompletedTask;
        UpdateStateMessageAsync = _ => Task.CompletedTask;
        UpdateStateMessageThreadAsync = (_, _, _, _, _) => Task.CompletedTask;

#if DEBUG
        // TgLog to VS Output debugging pane in addition.
        WTelegram.Helpers.Log = (i, str) => Debug.WriteLine($"{i} | {str}", TgConstants.LogTypeNetwork);
#else
        // Disable logging in Console.
        WTelegram.Helpers.Log = (_, _) => { };
#endif
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

    public void SetupUpdateStateSource(Func<long, int, int, string, Task> updateStateSourceAsync) =>
        UpdateStateSourceAsync = updateStateSourceAsync;

    public void SetupUpdateStateContact(Func<long, string, string, string, Task> updateStateContactAsync) =>
        UpdateStateContactAsync = updateStateContactAsync;

    public void SetupUpdateStateStory(Func<long, int, int, string, Task> updateStateStoryAsync) =>
        UpdateStateStoryAsync = updateStateStoryAsync;

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

    public void SetupUpdateStateItemSource(Func<long, Task> updateStateItemSourceAsync) =>
        UpdateStateItemSourceAsync = updateStateItemSourceAsync;

    public void SetupUpdateStateFile(Func<long, int, string, long, long, long, bool, int, Task> updateStateFileAsync) =>
        UpdateStateFileAsync = updateStateFileAsync;

    public void SetupUpdateStateMessage(Func<string, Task> updateStateMessageAsync) =>
        UpdateStateMessageAsync = updateStateMessageAsync;

    public void SetupUpdateStateMessageThread(Func<long, int, string, bool, int, Task> updateStateMessageThreadAsync) =>
        UpdateStateMessageThreadAsync = updateStateMessageThreadAsync;

    private bool ClientResultDisconnected()
    {
        UpdateStateSourceAsync(0, 0, 0, string.Empty);
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
                throw new ArgumentOutOfRangeException(nameof(appDto), appDto, "Cannot set UseBot property!");
        }
        if (IsReady) return;

        await DisconnectClientAsync();
        await DisconnectBotAsync();

        Client = new(config);
        await ConnectThroughProxyAsync(proxyDto, false);
        Client.OnOther += (obj) => OnClientOtherAsync(obj);
        Client.OnOwnUpdates += (updateBase) => OnOwnUpdatesClientAsync(updateBase);
        Client.OnUpdates += (updateBase) => OnUpdatesClientAsync(updateBase);
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

    //public async Task ConnectSessionAsync(TgEfProxyDto proxyDto)
    //{
    //    if (IsReady)
    //        return;
    //    await DisconnectClientAsync();
    //    Client = new(ConfigClientDesktop);
    //    await ConnectThroughProxyAsync(proxyDto, true);
    //    Client.OnUpdates += OnUpdatesClientAsync;
    //    Client.OnOther += OnClientOtherAsync;
    //    await LoginUserAsync(isProxyUpdate: true);
    //}

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
        // Collect chats from Telegram.
        if (DicChatsAll.Count == 0)
            await CollectAllChatsAsync();

        if (tgDownloadSettings.SourceVm.Dto.IsReady)
        {
            tgDownloadSettings.SourceVm.Dto.Id = ReduceChatId(tgDownloadSettings.SourceVm.Dto.Id);
            foreach (var chat in DicChatsAll)
            {
                if (chat.Value is Channel channel && Equals(channel.id, tgDownloadSettings.SourceVm.Dto.Id) &&
                    await IsChatBaseAccessAsync(channel))
                    return channel;
            }
        }
        else
        {
            foreach (var chat in DicChatsAll)
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
            messagesChats = await ExecuteWithFloodControlAsync(() => Client.Channels_GetChannels(new InputChannel(tgDownloadSettings.SourceVm.Dto.Id, Me.access_hash)));
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
        if (DicChatsAll.Count == 0)
            await CollectAllChatsAsync();

        if (tgDownloadSettings.SourceVm.Dto.IsReady)
        {
            tgDownloadSettings.SourceVm.Dto.Id = ReduceChatId(tgDownloadSettings.SourceVm.Dto.Id);
            foreach (var chat in DicChatsAll)
            {
                if (chat.Value is { } chatBase && Equals(chatBase.ID, tgDownloadSettings.SourceVm.Dto.Id))
                    return chatBase;
            }
        }
        else
        {
            foreach (var chat in DicChatsAll)
            {
                if (chat.Value is { } chatBase)
                    return chatBase;
            }
        }

        if (tgDownloadSettings.SourceVm.Dto.Id is 0)
            tgDownloadSettings.SourceVm.Dto.Id = await GetPeerIdAsync(tgDownloadSettings.SourceVm.Dto.UserName);

        Messages_Chats? messagesChats = null;
        if (Me is not null)
            messagesChats = await ExecuteWithFloodControlAsync(() => Client.Channels_GetGroupsForDiscussion());

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
        if (DicChatsAll.Count == 0)
            await CollectAllChatsAsync();
        if (tgDownloadSettings.SourceVm.Dto.IsReady)
        {
            tgDownloadSettings.SourceVm.Dto.Id = ReduceChatId(tgDownloadSettings.SourceVm.Dto.Id);
            var chatBase = DicChatsAll.FirstOrDefault(x => x.Key.Equals(tgDownloadSettings.SourceVm.Dto.Id)).Value;
            if (chatBase is not null)
                tgDownloadSettings.Chat.Base = chatBase;
        }
        else
        {
            tgDownloadSettings.SourceVm.Dto.UserName = tgDownloadSettings.SourceVm.Dto.UserName.Trim();
            var chatBase = DicChatsAll.FirstOrDefault(x => !string.IsNullOrEmpty(x.Value.MainUsername) &&
                x.Value.MainUsername.Equals(tgDownloadSettings.SourceVm.Dto.UserName)).Value;
            if (chatBase is null)
                chatBase = DicChatsAll.FirstOrDefault(x => x.Value.ID.Equals(tgDownloadSettings.SourceVm.Dto.Id)).Value;
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

    public async Task<Dictionary<long, ChatBase>> CollectAllChatsAsync()
    {
        switch (IsReady)
        {
            case true when Client is not null:
                {
                    var messages = await ExecuteWithFloodControlAsync(() => Client.Messages_GetAllChats());
                    FillEnumerableChats(messages.chats);
                    return messages.chats;
                }
        }
        return [];
    }

    public async Task<Dictionary<long, ChatBase>> CollectAllDialogsAsync()
    {
        switch (IsReady)
        {
            case true when Client is not null:
                {
                    var messages = await ExecuteWithFloodControlAsync(() => Client.Messages_GetAllDialogs());
                    FillEnumerableChats(messages.chats);
                    return messages.chats;
                }
        }
        return [];
    }

    public async Task CollectAllContactsAsync()
    {
        switch (IsReady)
        {
            case true when Client is not null:
                {
                    EnumerableUsers = [];
                    var contacts = await ExecuteWithFloodControlAsync(() => Client.Contacts_GetContacts());
                    FillEnumerableUsers(contacts.users);
                    break;
                }
        }
    }

    public async Task CollectAllUsersAsync(List<long>? chatIds)
    {
        switch (IsReady)
        {
            case true when Client is not null:
                {
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
                    break;
                }
        }
    }

    public async Task CollectAllStoriesAsync()
    {
        switch (IsReady)
        {
            case true when Client is not null:
                {
                    var storiesBase = await ExecuteWithFloodControlAsync(() => Client.Stories_GetAllStories());
                    if (storiesBase is Stories_AllStories allStories)
                    {
                        FillEnumerableStories([.. allStories.peer_stories]);
                    }
                    break;
                }
        }
    }

    private void FillEnumerableChats(Dictionary<long, ChatBase> chats)
    {
        DicChatsAll = chats;
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

    private void FillEnumerableUsers(Dictionary<long, User> users)
    {
        DicUsersAll = users;
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
        DicStoriesAll = [];
        EnumerableStories.Clear();

        // Sort
        var peerStoriesSorted = peerStories.OrderBy(i => i.stories.Rank).ToArray();
        foreach (var peerStory in peerStories)
        {
            foreach (var storyBase in peerStory.stories)
            {
                if (storyBase is StoryItem story)
                    EnumerableStories.Enqueue(story);
            }
        }
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
        var channelLabel = channel is null ? string.Empty :
            string.IsNullOrEmpty(channel.MainUsername) ? channel.ID.ToString() : $"{channel.ID} | {channel.MainUsername}";
        if (!string.IsNullOrEmpty(channelLabel))
            channelLabel = $" for channel [{channelLabel}]";
        var sourceId = channel?.ID ?? 0;
        switch (update)
        {
            case UpdateNewChannelMessage updateNewChannelMessage:
                //if (channel is not null && updateNewChannelMessage.message.Peer.ID.Equals(channel.ID))
                await UpdateStateSourceAsync(sourceId, 0, 0, $"New channel message [{updateNewChannelMessage}]{channelLabel}");
                break;
            case UpdateNewMessage updateNewMessage:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"New message [{updateNewMessage}]{channelLabel}");
                break;
            case UpdateMessageID updateMessageId:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Message ID [{updateMessageId}]{channelLabel}");
                break;
            case UpdateDeleteChannelMessages updateDeleteChannelMessages:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Delete channel messages [{string.Join(", ", updateDeleteChannelMessages.messages)}]{channelLabel}");
                break;
            case UpdateDeleteMessages updateDeleteMessages:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Delete messages [{string.Join(", ", updateDeleteMessages.messages)}]{channelLabel}");
                break;
            case UpdateChatUserTyping updateChatUserTyping:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Chat user typing [{updateChatUserTyping}]{channelLabel}");
                break;
            case UpdateChatParticipants { participants: ChatParticipants chatParticipants }:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Chat participants [{chatParticipants.ChatId} | {string.Join(", ", chatParticipants.Participants.Length)}]{channelLabel}");
                break;
            case UpdateUserStatus updateUserStatus:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"User status [{updateUserStatus.user_id} | {updateUserStatus}]{channelLabel}");
                break;
            case UpdateUserName updateUserName:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"User name [{updateUserName.user_id} | {string.Join(", ", updateUserName.usernames.Select(item => item.username))}]{channelLabel}");
                break;
            case UpdateNewEncryptedMessage updateNewEncryptedMessage:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"New encrypted message [{updateNewEncryptedMessage}]{channelLabel}");
                break;
            case UpdateEncryptedChatTyping updateEncryptedChatTyping:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Encrypted chat typing [{updateEncryptedChatTyping}]{channelLabel}");
                break;
            case UpdateEncryption updateEncryption:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Encryption [{updateEncryption}]{channelLabel}");
                break;
            case UpdateEncryptedMessagesRead updateEncryptedMessagesRead:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Encrypted message read [{updateEncryptedMessagesRead}]{channelLabel}");
                break;
            case UpdateChatParticipantAdd updateChatParticipantAdd:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Chat participant add [{updateChatParticipantAdd}]{channelLabel}");
                break;
            case UpdateChatParticipantDelete updateChatParticipantDelete:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Chat participant delete [{updateChatParticipantDelete}]{channelLabel}");
                break;
            case UpdateDcOptions updateDcOptions:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Dc options [{string.Join(", ", updateDcOptions.dc_options.Select(item => item.id))}]{channelLabel}");
                break;
            case UpdateNotifySettings updateNotifySettings:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Notify settings [{updateNotifySettings}]{channelLabel}");
                break;
            case UpdateServiceNotification updateServiceNotification:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Service notification [{updateServiceNotification}]{channelLabel}");
                break;
            case UpdatePrivacy updatePrivacy:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Privacy [{updatePrivacy}]{channelLabel}");
                break;
            case UpdateUserPhone updateUserPhone:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"User phone [{updateUserPhone}]{channelLabel}");
                break;
            case UpdateReadHistoryInbox updateReadHistoryInbox:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Read history inbox [{updateReadHistoryInbox}]{channelLabel}");
                break;
            case UpdateReadHistoryOutbox updateReadHistoryOutbox:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Read history outbox [{updateReadHistoryOutbox}]{channelLabel}");
                break;
            case UpdateWebPage updateWebPage:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Web page [{updateWebPage}]{channelLabel}");
                break;
            case UpdateReadMessagesContents updateReadMessagesContents:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Read messages contents [{string.Join(", ", updateReadMessagesContents.messages.Select(item => item.ToString()))}]{channelLabel}");
                break;
            case UpdateEditChannelMessage updateEditChannelMessage:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Edit channel message [{updateEditChannelMessage}]{channelLabel}");
                break;
            case UpdateEditMessage updateEditMessage:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Edit message [{updateEditMessage}]{channelLabel}");
                break;
            case UpdateUserTyping updateUserTyping:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"User typing [{updateUserTyping}]{channelLabel}");
                break;
            case UpdateChannelMessageViews updateChannelMessageViews:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Channel message views [{updateChannelMessageViews}]{channelLabel}");
                break;
            case UpdateChannel updateChannel:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Channel [{updateChannel}]");
                break;
            case UpdateChannelReadMessagesContents updateChannelReadMessages:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Channel read messages [{string.Join(", ", updateChannelReadMessages.messages)}]{channelLabel}");
                break;
            case UpdateChannelUserTyping updateChannelUserTyping:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Channel user typing [{updateChannelUserTyping}]{channelLabel}");
                break;
            case UpdateMessagePoll updateMessagePoll:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Message poll [{updateMessagePoll}]{channelLabel}");
                break;
            case UpdateChannelTooLong updateChannelTooLong:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Channel too long [{updateChannelTooLong}]{channelLabel}");
                break;
            case UpdateReadChannelInbox updateReadChannelInbox:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Channel inbox [{updateReadChannelInbox}]{channelLabel}");
                break;
            case UpdateChatParticipantAdmin updateChatParticipantAdmin:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Chat participant admin[{updateChatParticipantAdmin}]{channelLabel}");
                break;
            case UpdateNewStickerSet updateNewStickerSet:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"New sticker set [{updateNewStickerSet}]{channelLabel}");
                break;
            case UpdateStickerSetsOrder updateStickerSetsOrder:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Sticker sets order [{updateStickerSetsOrder}]{channelLabel}");
                break;
            case UpdateStickerSets updateStickerSets:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Sticker sets [{updateStickerSets}]{channelLabel}");
                break;
            case UpdateSavedGifs updateSavedGifs:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"SavedGifs [{updateSavedGifs}]{channelLabel}");
                break;
            case UpdateBotInlineQuery updateBotInlineQuery:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Bot inline query [{updateBotInlineQuery}]{channelLabel}");
                break;
            case UpdateBotInlineSend updateBotInlineSend:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Bot inline send [{updateBotInlineSend}]{channelLabel}");
                break;
            case UpdateBotCallbackQuery updateBotCallbackQuery:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Bot cCallback query [{updateBotCallbackQuery}]{channelLabel}");
                break;
            case UpdateInlineBotCallbackQuery updateInlineBotCallbackQuery:
                await UpdateStateSourceAsync(sourceId, 0, 0, $"Inline bot callback query [{updateInlineBotCallbackQuery}]{channelLabel}");
                break;
        }
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

    public IEnumerable<ChatBase> SortListChats(IList<ChatBase> chats)
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

    public IEnumerable<Channel> SortListChannels(IList<Channel> channels)
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
            await TryCatchFuncAsync(async () =>
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
            fullChannel = await ExecuteWithFloodControlAsync(() => Client.Channels_GetFullChannel(channel));
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
            chatFull = await ExecuteWithFloodControlAsync(() => Client.GetFullChat(chatBase));
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

    public string GetChatInfo(ChatBase chatBase) => $"{chatBase.ID} | {chatBase.Title}";

    public string GetChannelFullInfo(ChannelFull channelFull, ChatBase chatBase, bool isFull)
    {
        var result = GetChatInfo(chatBase);
        if (isFull)
            result += " | " + Environment.NewLine + channelFull.About;
        return result;
    }

    public string GetChatFullBaseInfo(ChatFullBase chatFull, ChatBase chatBase, bool isFull)
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
            await TryCatchFuncAsync(async () =>
            {
                if (chatBase is Chat chatBaseObj)
                {
                    var full = await ExecuteWithFloodControlAsync(() => Client.Messages_GetFullChat(chatBaseObj.ID));
                    if (full is TL.Messages_ChatFull chatFull &&
                        chatFull.full_chat is TL.ChatFull chatFullObj)
                    {
                        if (chatFullObj.flags.HasFlag(TL.User.Flags.has_access_hash))
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
            await TryCatchFuncAsync(async () =>
            {
                var botChatFullInfo = await GetChatDetailsForBot(chatBase.ID, chatBase.MainUsername);
                result = botChatFullInfo is not null;
            });
            return result;
        }
    }

    public async Task<int> GetChannelMessageIdWithLockAsync(ITgDownloadViewModel? tgDownloadSettings, TgEnumPosition position) =>
        await GetChannelMessageIdCoreAsync(tgDownloadSettings, position);

    public async Task<int> GetChannelMessageIdWithLockAsync(ChatBase chatBase, TgEnumPosition position) =>
        await GetChannelMessageIdCoreAsync(null, position);

    private async Task<int> GetChannelMessageIdAsync(ITgDownloadViewModel? tgDownloadSettings, TgEnumPosition position) =>
        await GetChannelMessageIdCoreAsync(tgDownloadSettings, position);

    private async Task<int> GetChannelMessageIdCoreAsync(ITgDownloadViewModel? tgDownloadSettings, TgEnumPosition position)
    {
        if (Client is null)
            return 0;
        if (tgDownloadSettings is null)
            return 0;
        if (tgDownloadSettings.Chat.Base is not { } chatBase)
            return 0;
        if (chatBase.ID is 0)
            return 0;

        if (chatBase is Channel channel)
        {
            var fullChannel = await ExecuteWithFloodControlAsync(() => Client.Channels_GetFullChannel(channel));
            if (fullChannel.full_chat is not ChannelFull channelFull)
                return 0;
            var isAccessToMessages = await ExecuteWithFloodControlAsync(() => Client.Channels_ReadMessageContents(channel));
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
            var fullChannel = await ExecuteWithFloodControlAsync(() => Client.GetFullChat(chatBase));
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

    // TODO: use smart method from scan chat
    public async Task<int> GetChannelMessageIdLastAsync(ITgDownloadViewModel? tgDownloadSettings) =>
        await GetChannelMessageIdWithLockAsync(tgDownloadSettings, TgEnumPosition.Last);

    private int GetChannelMessageIdLastCore(ChannelFull channelFull) =>
        channelFull.read_inbox_max_id;

    private int GetChannelMessageIdLastCore(ChatFullBase chatFullBase) =>
        chatFullBase is ChannelFull channelFull ? channelFull.read_inbox_max_id : 0;

    private async Task<int> GetChannelMessageIdLastCoreAsync(Channel channel)
    {
        var fullChannel = await ExecuteWithFloodControlAsync(() => Client.Channels_GetFullChannel(channel));
        if (fullChannel.full_chat is not ChannelFull channelFull)
            return 0;
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
            await UpdateStateSourceAsync(chatBase.ID, 0, 0, $"Read from {offset} to {offset + partition} messages");
            var messages = await ExecuteWithFloodControlAsync(() => Client.Channels_GetMessages(chatBase as Channel, inputMessages));
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
            if (result < max)
                break;
            offset += partition;
        }
        // Finally.
        if (result >= max)
            result = 1;
        tgDownloadSettings2.SourceVm.Dto.FirstId = result;
        await UpdateStateSourceAsync(chatBase.ID, 0, 0, $"Get the first ID message '{result}' is complete.");
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
            if (botChatFullInfo is not null)
            {
                if (botChatFullInfo.TLInfo is Messages_ChatFull { full_chat: ChannelFull channelFull })
                {
                    //if (channelFull.slowmode_seconds > 0)
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

    private async Task UpdateSourceTgCoreAsync(Channel channel, string about, int count, bool isUserAccess)
    {
        var storageResult = await StorageManager.SourceRepository.GetByDtoAsync(new() { Id = channel.id });
        var sourceNew = storageResult.IsExists && storageResult.Item is not null ? storageResult.Item : new();
        sourceNew.Id = channel.id;
        sourceNew.AccessHash = channel.access_hash;
        sourceNew.IsActive = channel.IsActive;
        sourceNew.UserName = channel.username;
        sourceNew.Title = channel.title;
        sourceNew.About = about;
        sourceNew.Count = count;
        sourceNew.IsUserAccess = isUserAccess;
        // Save
        await StorageManager.SourceRepository.SaveAsync(sourceNew);
    }

    private async Task UpdateSourceTgCoreAsync(ChatBase chatBase, string about, int count, bool isUserAccess)
    {
        var storageResult = await StorageManager.SourceRepository.GetByDtoAsync(new() { Id = chatBase.ID });
        var sourceNew = storageResult.IsExists && storageResult.Item is not null ? storageResult.Item : new();
        sourceNew.Id = chatBase.ID;
        sourceNew.AccessHash = 0;
        sourceNew.IsActive = chatBase.IsActive;
        sourceNew.UserName = chatBase.MainUsername;
        sourceNew.Title = chatBase.Title;
        sourceNew.About = about;
        sourceNew.Count = count;
        sourceNew.IsUserAccess = isUserAccess;
        // Save
        await StorageManager.SourceRepository.SaveAsync(sourceNew);
    }

    /// <inheritdoc />
    public async Task UpdateUserAsync(TL.User user, bool isContact)
    {
        var storageResult = await StorageManager.UserRepository.GetByDtoAsync(new() { Id = user.id });
        var userNew = storageResult.IsExists && storageResult.Item is not null ? storageResult.Item : new();
        userNew.DtChanged = DateTime.UtcNow;
        userNew.Id = user.id;
        userNew.AccessHash = user.access_hash;
        userNew.IsActive = user.IsActive;
        userNew.IsBot = user.IsBot;
        userNew.FirstName = user.first_name;
        userNew.LastName = user.last_name;
        userNew.UserName = user.username;
        userNew.UserNames = user.usernames is null ? string.Empty : string.Join("|", user.usernames.ToList());
        userNew.PhoneNumber = user.phone;
        userNew.Status = user.status is null ? string.Empty : user.status.ToString();
        userNew.RestrictionReason = user.restriction_reason is null ? string.Empty : string.Join("|", user.restriction_reason.ToList());
        userNew.LangCode = user.lang_code;
        userNew.StoriesMaxId = user.stories_max_id;
        userNew.BotInfoVersion = user.bot_info_version.ToString();
        userNew.BotInlinePlaceholder = user.bot_inline_placeholder is null ? string.Empty : user.bot_inline_placeholder.ToString();
        userNew.BotActiveUsers = user.bot_active_users;
        userNew.IsContact = isContact;
        // Save
        await StorageManager.UserRepository.SaveAsync(userNew);
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

    private TgEfUserEntity CreateUserEntityFromDto(TgEfUserDto dto) => new TgEfUserEntity
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

    private async Task UpdateStoryTgAsync(StoryItem story)
    {
        if (story.entities is not null)
        {
            foreach (var message in story.entities)
            {
                await UpdateStoryItemTgAsync(story, message);
            }
        }
        else
        {
            await UpdateStoryItemTgAsync(story, message: null);
        }
    }

    private async Task UpdateStoryItemTgAsync(StoryItem story, MessageEntity? message)
    {
        var storageResult = await StorageManager.StoryRepository.GetByDtoAsync(new() { Id = story.id });
        var storyNew = storageResult.IsExists && storageResult.Item is not null ? storageResult.Item : new();
        storyNew.DtChanged = DateTime.UtcNow;
        storyNew.Id = story.id;
        storyNew.FromId = story.from_id?.ID;
        storyNew.FromName = story.fwd_from?.from_name;
        storyNew.Date = story.date;
        storyNew.ExpireDate = story.expire_date;
        storyNew.Caption = story.caption;
        if (message is not null)
        {
            storyNew.Type = message.Type;
            storyNew.Offset = message.Offset;
            storyNew.Length = message.Length;
            // Switch message type
            TgEfStoryEntityByMessageType(storyNew, message);
        }
        // Switch media type
        TgEfStoryEntityByMediaType(storyNew, story.media);
        // Save
        await StorageManager.StoryRepository.SaveAsync(storyNew);
    }

    private void TgEfStoryEntityByMessageType(TgEfStoryEntity storyNew, MessageEntity message)
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

    private void TgEfStoryEntityByMediaType(TgEfStoryEntity storyNew, MessageMedia media)
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

    private async Task UpdateSourceTgAsync(Channel channel, int count, bool isUserAccess) =>
        await UpdateSourceTgCoreAsync(channel, string.Empty, count, isUserAccess);

    private async Task UpdateSourceTgAsync(ChatBase chatBase, int count, bool isUserAccess) =>
        await UpdateSourceTgCoreAsync(chatBase, string.Empty, count, isUserAccess);

    /// <summary> Search sources from Telegram </summary>
    public async Task SearchSourcesTgAsync(ITgDownloadViewModel tgDownloadSettings, TgEnumSourceType sourceType, List<long>? chatIds = null)
    {
        if (tgDownloadSettings is not TgDownloadSettingsViewModel tgDownloadSettings2) return;
        await TryCatchFuncAsync(async () =>
        {
            await LoginUserAsync(isProxyUpdate: false);
            switch (sourceType)
            {
                case TgEnumSourceType.Chat:
                    await SetUserAccessForAllChatsAsync(chatIds, isUserAccess: false);
                    await SetSibscribeForAllChatsAsync(chatIds, isSubscribe: false);
                    await UpdateStateSourceAsync(tgDownloadSettings2.SourceVm.Dto.Id, 0, tgDownloadSettings2.SourceVm.Dto.Count, TgLocale.CollectChats);
                    chatIds = [.. (await CollectAllChatsAsync()).Select(x => x.Key)];
                    await SetSibscribeForAllChatsAsync(chatIds, isSubscribe: true);
                    tgDownloadSettings.SourceScanCount = DicChatsAll.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    // List groups
                    await SearchSourcesTgForGroupsAsync(tgDownloadSettings, chatIds);
                    // List channels
                    await SearchSourcesTgForChannelsAsync(tgDownloadSettings2, chatIds);
                    break;
                case TgEnumSourceType.Dialog:
                    await SetUserAccessForAllChatsAsync(chatIds, isUserAccess: false);
                    await SetSibscribeForAllChatsAsync(chatIds, isSubscribe: false);
                    await UpdateStateSourceAsync(tgDownloadSettings2.SourceVm.Dto.Id, 0, tgDownloadSettings2.SourceVm.Dto.Count, TgLocale.CollectDialogs);
                    chatIds = [.. (await CollectAllDialogsAsync()).Select(x => x.Key)];
                    await SetSibscribeForAllChatsAsync(chatIds, isSubscribe: true);
                    tgDownloadSettings.SourceScanCount = DicChatsAll.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    // List groups
                    await SearchSourcesTgForGroupsAsync(tgDownloadSettings, chatIds);
                    // List channels
                    await SearchSourcesTgForChannelsAsync(tgDownloadSettings2, chatIds);
                    break;
                case TgEnumSourceType.Story:
                    await UpdateStateStoryAsync(tgDownloadSettings2.StoryVm.Dto.Id, 0, 0, TgLocale.CollectStories);
                    await CollectAllStoriesAsync();
                    tgDownloadSettings.SourceScanCount = DicStoriesAll.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    // List stories
                    await SearchSourcesTgConsoleForStoriesAsync(tgDownloadSettings2);
                    break;
                case TgEnumSourceType.Contact:
                    await UpdateStateSourceAsync(tgDownloadSettings2.ContactVm.Dto.Id, 0, 0, TgLocale.CollectContacts);
                    await CollectAllContactsAsync();
                    tgDownloadSettings.SourceScanCount = DicUsersAll.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    await SearchSourcesTgForContactsAsync(tgDownloadSettings2, isContact: true);
                    break;
                case TgEnumSourceType.User:
                    await UpdateStateSourceAsync(tgDownloadSettings2.ContactVm.Dto.Id, 0, 0, TgLocale.CollectUsers);
                    await CollectAllUsersAsync(chatIds);
                    tgDownloadSettings.SourceScanCount = DicUsersAll.Count;
                    tgDownloadSettings.SourceScanCurrent = 0;
                    await SearchSourcesTgForContactsAsync(tgDownloadSettings2, isContact: false);
                    break;
                case TgEnumSourceType.Default:
                    break;
                case TgEnumSourceType.ChatBase:
                    break;
                case TgEnumSourceType.Channel:
                    break;
            }
        });
        await UpdateTitleAsync(string.Empty);
    }

    /// <summary> Set user access for all chats </summary>
    private async Task SetUserAccessForAllChatsAsync(List<long>? chatIds, bool isUserAccess)
    {
        try
        {
            if (chatIds is null)
                await StorageManager.SourceRepository.SetIsUserAccess(isUserAccess);
            else
                await StorageManager.SourceRepository.SetIsUserAccess(chatIds, isUserAccess);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            TgLog.MarkupWarning($"Error set user access for all chats: {ex.Message}");
        }
    }

    /// <summary> Set subscribe for all chats </summary>
    private async Task SetSibscribeForAllChatsAsync(List<long>? chatIds, bool isSubscribe)
    {
        try
        {
            if (chatIds is null)
                await StorageManager.SourceRepository.SetIsSubscribe(isSubscribe);
            else
                await StorageManager.SourceRepository.SetIsSubscribe(chatIds, isSubscribe);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            TgLog.MarkupWarning($"Error set subscribe for all chats: {ex.Message}");
        }
    }

    private async Task SearchSourcesTgForChannelsAsync(TgDownloadSettingsViewModel tgDownloadSettings, List<long>? chatIds)
    {
        var channels = chatIds is not null && chatIds.Count != 0 ? EnumerableChannels.Where(x => chatIds.Contains(x.ID)) : EnumerableChannels;
        foreach (var channel in channels)
        {
            tgDownloadSettings.SourceScanCurrent++;
            if (channel.IsActive)
            {
                await TryCatchFuncAsync(async () =>
                {
                    tgDownloadSettings.Chat.Base = channel;
                    var messagesCount = await GetChannelMessageIdLastAsync(tgDownloadSettings);
                    if (channel.IsChannel)
                    {
                        var fullChannel = await ExecuteWithFloodControlAsync(() => Client.Channels_GetFullChannel(channel));
                        if (fullChannel?.full_chat is ChannelFull channelFull)
                        {
                            await UpdateSourceTgCoreAsync(channel, channelFull.about, messagesCount, isUserAccess: true);
                            await UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceScanCurrent,
                                tgDownloadSettings.SourceScanCount, $"{channel} | {TgDataFormatUtils.TrimStringEnd(channelFull.about, 40)}");
                        }
                    }
                    else
                    {
                        await UpdateSourceTgAsync(channel, messagesCount, isUserAccess: true);
                        await UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceScanCurrent,
                            tgDownloadSettings.SourceScanCount, $"{channel}");
                    }
                });
            }
            await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(tgDownloadSettings.SourceScanCount,
                tgDownloadSettings.SourceScanCurrent):#00.00} %");
        }
    }

    private async Task SearchSourcesTgForGroupsAsync(ITgDownloadViewModel tgDownloadSettings, List<long>? chatIds)
    {
        var smallGroups = (chatIds is not null && chatIds.Count != 0) ? EnumerableSmallGroups.Where(x => chatIds.Contains(x.ID)) : EnumerableSmallGroups;
        var groups = (chatIds is not null && chatIds.Count != 0) ? EnumerableGroups.Where(x => chatIds.Contains(x.ID)) : EnumerableGroups;

        await SearchSourcesTgForGroupsCoreAsync(tgDownloadSettings, smallGroups);
        await SearchSourcesTgForGroupsCoreAsync(tgDownloadSettings, groups);
    }

    private async Task SearchSourcesTgForGroupsCoreAsync(ITgDownloadViewModel tgDownloadSettings, IEnumerable<ChatBase> groups)
    {
        foreach (var group in groups)
        {
            tgDownloadSettings.SourceScanCurrent++;
            if (group.IsActive)
            {
                await TryCatchFuncAsync(async () =>
                {
                    tgDownloadSettings.Chat.Base = group;
                    var messagesCount = await GetChannelMessageIdLastAsync(tgDownloadSettings);
                    if (group is Channel channel)
                        await UpdateSourceTgAsync(channel, messagesCount, isUserAccess: true);
                    else
                        await UpdateSourceTgAsync(group, messagesCount, isUserAccess: true);
                    if (tgDownloadSettings is TgDownloadSettingsViewModel tgDownloadSettings2)
                        await UpdateStateSourceAsync(tgDownloadSettings2.SourceVm.Dto.Id, 0, 0, $"{group} | {messagesCount}");
                });
            }
        }
    }

    private async Task SearchSourcesTgForContactsAsync(TgDownloadSettingsViewModel tgDownloadSettings, bool isContact)
    {
        foreach (var user in EnumerableUsers)
        {
            tgDownloadSettings.SourceScanCurrent++;
            await TryCatchFuncAsync(async () =>
            {
                await UpdateUserAsync(user, isContact);
                await UpdateStateContactAsync(user.id, user.first_name, user.last_name, user.username);
            });
            await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(tgDownloadSettings.SourceScanCount,
                tgDownloadSettings.SourceScanCurrent):#00.00} %");
        }
    }

    private async Task SearchSourcesTgConsoleForStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        foreach (var story in EnumerableStories)
        {
            tgDownloadSettings.SourceScanCurrent++;
            await TryCatchFuncAsync(async () =>
            {
                await UpdateStoryTgAsync(story);
                await UpdateStateStoryAsync(story.id, 0, 0, story.caption);
            });
            await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(tgDownloadSettings.SourceScanCount,
                tgDownloadSettings.SourceScanCurrent):#00.00} %");
        }
    }

    private async Task SearchStoriesTgConsoleForContactsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        foreach (var story in EnumerableStories)
        {
            tgDownloadSettings.SourceScanCurrent++;
            await TryCatchFuncAsync(async () =>
            {
                await UpdateStoryTgAsync(story);
                await UpdateStateStoryAsync(story.id, 0, 0, story.caption);
            });
            await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(tgDownloadSettings.SourceScanCount,
                tgDownloadSettings.SourceScanCurrent):#00.00} %");
        }
    }

    public async Task ScanSourcesTgDesktopAsync(TgEnumSourceType sourceType, Func<ITgEfSourceViewModel, Task> afterScanAsync)
    {
        await TryCatchFuncAsync(async () =>
        {
            switch (sourceType)
            {
                case TgEnumSourceType.Chat:
                    await UpdateStateSourceAsync(0, 0, 0, TgLocale.CollectChats);
                    switch (IsReady)
                    {
                        case true when Client is not null:
                            var messages = await ExecuteWithFloodControlAsync(() => Client.Messages_GetAllChats());
                            FillEnumerableChats(messages.chats);
                            break;
                    }
                    await AfterCollectSourcesAsync(afterScanAsync);
                    break;
                case TgEnumSourceType.Dialog:
                    await UpdateStateSourceAsync(0, 0, 0, TgLocale.CollectDialogs);
                    switch (IsReady)
                    {
                        case true when Client is not null:
                            {
                                var messages = await ExecuteWithFloodControlAsync(() => Client.Messages_GetAllDialogs());
                                FillEnumerableChats(messages.chats);
                                break;
                            }
                    }
                    break;
            }
        });
    }

    private async Task AfterCollectSourcesAsync(Func<TgEfSourceViewModel, Task> afterScanAsync)
    {
        // ListChannels.
        var i = 0;
        var count = EnumerableChannels.Count();
        foreach (var channel in EnumerableChannels)
        {
            if (channel.IsActive)
            {
                await TryCatchFuncAsync(async () =>
                {
                    TgEfSourceEntity source = new() { Id = channel.ID };
                    var messagesCount = await GetChannelMessageIdLastCoreAsync(channel);
                    source.Count = messagesCount;
                    if (channel.IsChannel)
                    {
                        var fullChannel = await ExecuteWithFloodControlAsync(() => Client.Channels_GetFullChannel(channel));
                        if (fullChannel.full_chat is ChannelFull channelFull)
                        {
                            source.About = channelFull.about;
                            source.UserName = channel.username;
                            source.Title = channel.title;
                        }
                    }
                    await afterScanAsync(new(source));
                });
            }
            i++;
            await UpdateStateSourceAsync(channel.ID, 0, 0, $"Reading the channel '{channel.ID}' | {channel.IsActive} | {i} from {count}");
        }

        // ListGroups.
        i = 0;
        count = EnumerableGroups.Count();
        foreach (var group in EnumerableGroups)
        {
            await TryCatchFuncAsync(async () =>
            {
                TgEfSourceEntity source = new() { Id = group.ID };
                if (group.IsActive)
                {
                    var messagesCount = await GetChannelMessageIdLastCoreAsync(group);
                    source.Count = messagesCount;
                }
                await afterScanAsync(new(source));
            });
            i++;
            await UpdateStateSourceAsync(group.ID, 0, 0, $"Reading the channel '{group.ID}' | {group.IsActive} | {i} from {count}");
        }
    }

    public async Task DownloadAllDataAsync(ITgDownloadViewModel tgDownloadSettings)
    {
        if (tgDownloadSettings is not TgDownloadSettingsViewModel tgDownloadSettings2) return;
        IsForceStopDownloading = false;
        await CreateChatAsync(tgDownloadSettings, isSilent: false);
        await TryCatchFuncAsync(async () =>
        {
            var isAccessToMessages = false;
            // Filters
            Filters = await StorageManager.FilterRepository.GetListDtosAsync(0, 0, x => x.IsEnabled);

            await LoginUserAsync(isProxyUpdate: false);

            var dirExists = await CreateDestDirectoryIfNotExistsAsync(tgDownloadSettings);
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
                isAccessToMessages = await ExecuteWithFloodControlAsync(() => Client.Channels_ReadMessageContents(channel));
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
            var counterForSave = 0;

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
                            Messages_ForumTopics forumTopics = await ExecuteWithFloodControlAsync(() => Client.Channels_GetAllForumTopics(channel));
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
                        await TryCatchFuncAsync(async () =>
                        {
                            if (Client is null) return;

                            var messages = await ExecuteWithFloodControlAsync(async () => channel is not null
                                ? await Client.Channels_GetMessages(channel, sourceFirstId)
                                : await Client.GetMessages(tgDownloadSettings.Chat.Base, sourceFirstId));
                            await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(sourceLastId, sourceFirstId):#00.00} %");
                            // Check deleted messages and mark storage entity
                            var firstMessage = messages.Messages.FirstOrDefault();
                            if (firstMessage is null)
                            {
                                // Mark message entity as deleted 
                                await CheckDeletedMessageAndMarkEntityAsync(tgDownloadSettings2.SourceVm.Dto.Id, sourceFirstId);
                            }
                            else
                            {
                                foreach (var message in messages.Messages)
                                {
                                    // Check message exists
                                    if (message is MessageBase messageBase)
                                        downloadTasks.Add(messageBase.Date > DateTime.MinValue
                                            ? DownloadDataAsync(tgDownloadSettings2, messageBase, chatCache, messageSettings, forumTopicSettings)
                                            : UpdateStateSourceAsync(tgDownloadSettings2.SourceVm.Dto.Id, messageBase.ID, tgDownloadSettings2.SourceVm.Dto.Count,
                                                $"Message {messageBase.ID} is not exists in {tgDownloadSettings2.SourceVm.Dto.Id}!"));
                                    // Counter
                                    counterForSave++;
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
                        await TryCatchFuncAsync(async () =>
                        {
                            if (Bot is null || botChatFullInfo is null) return;
                            var messages = await Bot.GetMessagesById(botChatFullInfo, [sourceFirstId]);
                            await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(sourceLastId, sourceFirstId):#00.00} %");
                            foreach (var message in messages.AsReadOnly())
                            {
                                // Check message exists
                                if (message.TLMessage is MessageBase messageBase)
                                    downloadTasks.Add(message.Date > DateTime.MinValue
                                        ? DownloadDataAsync(tgDownloadSettings2, messageBase, chatCache, messageSettings, forumTopicSettings)
                                        : UpdateStateSourceAsync(tgDownloadSettings2.SourceVm.Dto.Id, messageBase.ID, tgDownloadSettings2.SourceVm.Dto.Count,
                                            $"Message {messageBase.ID} is not exists in {tgDownloadSettings2.SourceVm.Dto.Id}!"));
                                counterForSave++;
                            }

                            if (Bot.Client is null) return;
                            var messages2 = await ExecuteWithFloodControlAsync(async () => channel is not null
                                ? await Bot.Client.Channels_GetMessages(channel, sourceFirstId)
                                : await Bot.Client.GetMessages(tgDownloadSettings.Chat.Base, sourceFirstId));

                            await UpdateTitleAsync($"{TgDataUtils.CalcSourceProgress(sourceLastId, sourceFirstId):#00.00} %");
                            foreach (var message in messages2.Messages)
                            {
                                // Check message exists
                                if (message is MessageBase messageBase)
                                    downloadTasks.Add(messageBase.Date > DateTime.MinValue
                                        ? DownloadDataAsync(tgDownloadSettings2, messageBase, chatCache, messageSettings, forumTopicSettings)
                                        : UpdateStateSourceAsync(tgDownloadSettings2.SourceVm.Dto.Id, messageBase.ID, tgDownloadSettings2.SourceVm.Dto.Count,
                                            $"Message {messageBase.ID} is not exists in {tgDownloadSettings2.SourceVm.Dto.Id}!"));
                                counterForSave++;
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
                        if (IsForceStopDownloading)
                        {
                            tgDownloadSettings2.SourceVm.Dto.FirstId = sourceLastId;
                            break;
                        }
                    }
                    if (counterForSave > 49)
                    {
                        counterForSave = 0;
                        tgDownloadSettings2.SourceVm.Dto.FirstId = sourceFirstId;
                        await StorageManager.SourceRepository.SaveAsync(tgDownloadSettings2.SourceVm.Dto.GetNewEntity());
                    }
                }
                tgDownloadSettings2.SourceVm.Dto.FirstId = sourceFirstId > sourceLastId ? sourceLastId : sourceFirstId;
                // Finally save all not stored messages
                await SaveAllWaitingMessagesAsync(tgDownloadSettings);
            }
            else
            {
                tgDownloadSettings2.SourceVm.Dto.FirstId = sourceLastId;
            }
            tgDownloadSettings2.SourceVm.Dto.IsDownload = false;
        });
        await UpdateTitleAsync(string.Empty);
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

    public void SetForceStopDownloading() => IsForceStopDownloading = true;

    public async Task MarkHistoryReadAsync()
    {
        await TryCatchFuncAsync(async () =>
        {
            await LoginUserAsync(isProxyUpdate: false);
        });

        await CollectAllChatsAsync();
        await UpdateStateMessageAsync("Mark as read all message in the channels: in the progress");
        await TryCatchFuncAsync(async () =>
        {
            if (Client is not null)
            {
                foreach (var chatBase in EnumerableChats)
                {
                    await TryCatchFuncAsync(async () =>
                    {
                        var isSuccess = await ExecuteWithFloodControlAsync(() => Client.ReadHistory(chatBase));
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

    private async Task<bool> CreateDestDirectoryIfNotExistsAsync(ITgDownloadViewModel tgDownloadSettings)
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

    private async Task DownloadDataAsync(TgDownloadSettingsViewModel tgDownloadSettings, MessageBase messageBase,
        TgChatCache chatCache, TgMessageSettings messageSettings, TgForumTopicSettings forumTopicSettings)
    {
        if (Client is null) return;
        if (messageBase is not Message message)
        {
            await UpdateStateSourceAsync(messageSettings.CurrentChatId, messageBase.ID, tgDownloadSettings.SourceVm.Dto.Count, "Empty message");
            return;
        }

        await TryCatchFuncAsync(async () =>
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
                var inputPeer = GetInputPeer(messageBase.Peer, accessHash);
                if (inputPeer is not null)
                {
                    // Get discussion message (main post + first-level comments)
                    var discussionMessage = await ExecuteWithFloodControlAsync(() => Client.Messages_GetDiscussionMessage(inputPeer, messageSettings.CurrentMessageId));
                    if (discussionMessage is not null && discussionMessage.messages is not null)
                    {
                        foreach (var rootMessage in discussionMessage.messages)
                        {
                            // Add chat from Storage to Cache
                            await AddChatFromStorageToCacheAsync(chatCache, rootMessage.Peer.ID);
                            // Get chat from Cache
                            if (chatCache.TryGetChat(rootMessage.Peer.ID, out var discussionHash))
                            {
                                var discussionPeer = GetInputPeer(rootMessage.Peer, discussionHash);
                                var discussionChat = discussionMessage.chats?.Where(x => x.Value.ID == rootMessage.Peer.ID).Select(x => x.Value).FirstOrDefault();
                                if (discussionChat is TL.Channel channel)
                                {
                                    CheckEnumerableChatCache(channel);
                                    await SaveAtStorageAccessHashForChatAsync(chatCache, channel, messageSettings.CurrentChatId);

                                    var clonedSettings = messageSettings.Clone();
                                    clonedSettings.ParentChatId = messageBase.Peer.ID;
                                    clonedSettings.ParentMessageId = messageBase.ID;
                                    //await TryCatchFuncAsync(async () =>
                                    //    await DownloadCommentAsync(tgDownloadSettings, commentMessage, chatCache, clonedSettings, forumTopicSettings));
                                    if (rootMessage is Message msg)
                                        // Save plain text message
                                        await SaveMessageAsync(tgDownloadSettings, rootSettings, rootMessage.Date, size: 0,
                                            msg.message, TgEnumMessageType.Message, isRetry: false, userId: rootMessage.From?.ID ?? 0);

                                    // Try to get replies to this comment
                                    if (rootMessage is MessageBase commentBase)
                                    {
                                        int offsetId = 0;
                                        const int limit = 100;
                                        while (true)
                                        {
                                            var repliesBase = await ExecuteWithFloodControlAsync(() =>
                                                Client.Messages_GetReplies(discussionPeer, commentBase.ID, offset_id: offsetId, limit: limit));
                                            if (repliesBase.Count == 0)
                                                break;

                                            foreach (var replyMessage in repliesBase.Messages)
                                            {
                                                if (replyMessage is Message rpl)
                                                {
                                                    var replySettings = messageSettings.Clone();
                                                    replySettings.ParentChatId = replySettings.CurrentChatId;
                                                    replySettings.ParentMessageId = replySettings.CurrentMessageId;
                                                    replySettings.CurrentChatId = replyMessage.Peer.ID;
                                                    replySettings.CurrentMessageId = replyMessage.ID;
                                                    // Save plain text message
                                                    await SaveMessageAsync(tgDownloadSettings, replySettings, replyMessage.Date, size: 0,
                                                        rpl.message, TgEnumMessageType.Message, isRetry: false, userId: replyMessage.From?.ID ?? 0);
                                                }
                                            }

                                            offsetId = repliesBase.Messages.Last().ID;
                                            if (repliesBase.Messages.Length < limit)
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Parse documents and photos
            if ((message.flags & Message.Flags.has_media) is not 0)
            {
                await DownloadDataCoreAsync(tgDownloadSettings, messageBase, message.media, chatCache, rootSettings, forumTopicSettings);
            }
            // Save plain text message
            else
            {
                await SaveMessageAsync(tgDownloadSettings, rootSettings, message.Date, size: 0,
                    message.message, TgEnumMessageType.Message, isRetry: false, userId: messageBase.From?.ID ?? 0);
            }
            // Update download progress for the message
            var messagesCount = await GetChannelMessageIdLastCoreAsync(rootSettings.CurrentChatId);
            await UpdateStateSourceAsync(rootSettings.CurrentChatId, rootSettings.CurrentMessageId, messagesCount,
                $"Reading the message {rootSettings.CurrentMessageId} from {messagesCount}");

            // Signal that all messages are processed
            await UpdateStateItemSourceAsync(rootSettings.CurrentChatId);
        });
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
    private void CheckEnumerableChatCache(Channel channel)
    {
        var checkChat = EnumerableChats.FirstOrDefault(x => x.ID == channel.id);
        if (checkChat is null)
            EnumerableChats.Enqueue(channel);

        var checkChannel = EnumerableChannels.FirstOrDefault(x => x.ID == channel.id);
        if (checkChannel is null)
            EnumerableChannels.Enqueue(channel);

        var checkGroup = EnumerableGroups.FirstOrDefault(x => x.ID == channel.id);
        if (checkGroup is null)
            EnumerableGroups.Enqueue(channel);
    }

    /// <summary> Save access hash for chat if it is not saved or does not have an access hash </summary>
    private async Task SaveAtStorageAccessHashForChatAsync(TgChatCache chatCache, TL.Channel channel, long chatId)
    {
        if (channel is null) return;
        if (chatCache.IsSaved(chatId)) return;

        long parentChatId = chatId;
        var directory = string.Empty;
        var chatResult = await StorageManager.SourceRepository.GetByDtoAsync(new() { Id = chatId });
        // Check if the chat is not saved or does not have an access hash
        if (chatResult.IsExists && chatResult.Item is TgEfSourceEntity chatEntity)
        {
            if (chatCache.TryGetChat(chatId, out var accessHash))
            {
                if (chatEntity.AccessHash != accessHash)
                {
                    chatEntity.DtChanged = DateTime.UtcNow;
                    chatEntity.AccessHash = accessHash;
                    await StorageManager.SourceRepository.SaveAsync(chatEntity);
                }
                directory = await SetAndCreateChatDirectoryIfNotExists(channel, parentChatId, chatEntity, isNew: false);
            }
        }
        else
        {
            var newChatEntity = new TgEfSourceEntity
            {
                DtChanged = DateTime.UtcNow,
                Id = chatId,
                AccessHash = channel.access_hash,
                UserName = channel.username,
                Title = channel.title,
                IsUserAccess = true,
                IsSubscribe = false
            };
            directory = await SetAndCreateChatDirectoryIfNotExists(channel, parentChatId, newChatEntity, isNew: true);
        }

        chatCache.TryAddChat(chatId, channel.access_hash, directory);
        chatCache.MarkAsSaved(chatId);
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
        await DownloadDataAsync(tgDownloadSettings, commentBase, chatCache, cloneSettings, forumTopicSettings);
        var messagesCount = await GetChannelMessageIdLastCoreAsync(cloneSettings.CurrentChatId);
        // Update download progress for the comment
        await UpdateStateSourceAsync(cloneSettings.CurrentChatId, cloneSettings.CurrentMessageId, messagesCount,
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

    private async Task DownloadDataCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings, MessageBase messageBase, MessageMedia messageMedia,
        TgChatCache chatCache, TgMessageSettings messageSettings, TgForumTopicSettings forumTopicSettings)
    {
        var mediaInfo = GetMediaInfo(messageMedia, tgDownloadSettings, messageBase, chatCache, messageSettings, forumTopicSettings);
        if (string.IsNullOrEmpty(mediaInfo.LocalNameOnly)) return;

        // Delete files
        DeleteExistsFiles(tgDownloadSettings, mediaInfo);

        // Move exists file at current directory
        MoveExistsFilesAtCurrentDir(tgDownloadSettings, mediaInfo);

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
                    case MessageMediaDocument mediaDocument:
                        if ((mediaDocument.flags & MessageMediaDocument.Flags.has_document) is not 0 && mediaDocument.document is Document document)
                        {
                            await ExecuteWithFloodControlAsync(() => Client.DownloadFileAsync(document, localFileStream, null,
                                ClientProgressForFile(messageSettings, mediaInfo.LocalNameWithNumber)));
                        }
                        // Store message
                        await SaveMessageAsync(tgDownloadSettings, messageSettings, mediaInfo.DtCreate, mediaInfo.RemoteSize, mediaInfo.RemoteName,
                            TgEnumMessageType.Document, isRetry: false, userId: messageBase.From?.ID ?? 0);
                        break;
                    case MessageMediaPhoto mediaPhoto:
                        if (mediaPhoto is { photo: Photo photo })
                        {
                            //var fileReferenceBase64 = Convert.ToBase64String(photo.file_reference);
                            //var fileReferenceHex = BitConverter.ToString(photo.file_reference).Replace("-", "");
                            await ExecuteWithFloodControlAsync(() => Client.DownloadFileAsync(photo, localFileStream, (PhotoSizeBase?)null,
                                ClientProgressForFile(messageSettings, mediaInfo.LocalNameWithNumber)));
                        }
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
                    // Store message
                    await SaveMessageAsync(tgDownloadSettings, messageSettings, mediaInfo.DtCreate, mediaInfo.RemoteSize, mediaInfo.RemoteName,
                        TgEnumMessageType.Document, isRetry: false, userId: messageBase.From?.ID ?? 0);
                    break;
                case MessageMediaPhoto mediaPhoto:
                    var messageStr = string.Empty;
                    if (messageBase is TL.Message message)
                        messageStr = message.message;
                    await SaveMessageAsync(tgDownloadSettings, messageSettings, mediaInfo.DtCreate, mediaInfo.RemoteSize,
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

    private void DeleteExistsFiles(TgDownloadSettingsViewModel tgDownloadSettings, TgMediaInfoModel mediaInfo)
    {
        if (!tgDownloadSettings.IsRewriteFiles) return;

        TryCatchAction(() =>
        {
            if (File.Exists(mediaInfo.LocalPathWithNumber))
            {
                var fileSize = TgFileUtils.CalculateFileSize(mediaInfo.LocalPathWithNumber);
                // If file size is less then original size
                if (fileSize == 0 || fileSize < mediaInfo.RemoteSize)
                    File.Delete(mediaInfo.LocalPathWithNumber);
            }
        });
    }

    /// <summary> Move existing files at the current directory </summary>
    private void MoveExistsFilesAtCurrentDir(TgDownloadSettingsViewModel tgDownloadSettings, TgMediaInfoModel mediaInfo)
    {
        if (!tgDownloadSettings.IsJoinFileNameWithMessageId)
            return;
        TryCatchAction(() =>
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
        });
    }

    /// <summary> Save message to the storage </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task SaveMessageAsync(TgDownloadSettingsViewModel tgDownloadSettings, TgMessageSettings messageSettings,
        DateTime dtCreated, long size, string message, TgEnumMessageType messageType, bool isRetry, long userId)
    {
        try
        {
            if (messageSettings.CurrentChatId == 0)
                throw new ArgumentException($"{nameof(messageSettings.CurrentChatId)} cannot be zero!", nameof(messageSettings.CurrentChatId));

            await ClientProgressForMessageThreadAsync(messageSettings.CurrentChatId, messageSettings.CurrentMessageId, message, isStartTask: true, messageSettings.ThreadNumber);

            var isExistsMessage = await StorageManager.MessageRepository.CheckExistsByDtoAsync(new()
            {
                SourceId = messageSettings.CurrentChatId,
                Id = messageSettings.CurrentMessageId
            });

            if (!isExistsMessage || isExistsMessage && tgDownloadSettings.IsRewriteMessages)
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

                if (tgDownloadSettings.IsSaveMessages)
                {
                    if (BatchMessagesCount < TgGlobalTools.BatchMessagesLimit)
                    {
                        MessageEntities.Add(messageItem);
                        BatchMessagesCount++;
                    }
                    else
                    {
                        int[] timeOuts = [15_000, 30_000, 45_000];
                        var canProceed = false;
                        foreach (var timeOut in timeOuts)
                        {
                            if (StorageManager.EfContext.Database.CurrentTransaction is null)
                            {
                                canProceed = true;
                                break;
                            }
                            await Task.Delay(timeOut);
                        }
                        if (!canProceed && StorageManager.EfContext.Database.CurrentTransaction is not null)
                        {
                            throw new InvalidOperationException("Transaction is still active after waiting for timeouts!");
                        }

                        await StorageManager.MessageRepository.SaveListAsync(MessageEntities);
                        BatchMessagesCount = 0;
                        MessageEntities.Clear();
                    }
                }
            }

            // Save relation if parent message exists
            if (messageSettings.ParentChatId > 0 && messageSettings.ParentMessageId > 0)
            {
                if (messageSettings.ParentMessageId == messageSettings.CurrentMessageId && messageSettings.ParentChatId != messageSettings.CurrentChatId)
                    TgLogUtils.WriteException(new Exception($"{nameof(messageSettings.ParentMessageId)} == {nameof(messageSettings.ParentMessageId)} | {messageSettings.ToDebugString()}"));
                else
                    await StorageManager.MessageRepository.SaveRelationAsync(
                        messageSettings.ParentChatId,
                        messageSettings.ParentMessageId,
                        messageSettings.CurrentChatId,
                        messageSettings.CurrentMessageId);
            }

            if (messageType == TgEnumMessageType.Document)
            {
                await UpdateStateFileAsync(messageSettings.CurrentChatId, messageSettings.CurrentMessageId, string.Empty, 0, 0, 0, false, messageSettings.ThreadNumber);
            }
        }
        catch (Exception ex)
        {
            BatchMessagesCount = 0;
            MessageEntities.Clear();
            if (!isRetry)
            {
                await Task.Delay(500);
                await SaveMessageAsync(tgDownloadSettings, messageSettings, dtCreated, size, message, messageType, isRetry: true, userId);
            }
            else
            {
                TgLogUtils.WriteExceptionWithMessage(ex, TgConstants.LogTypeStorage);
                await SetClientExceptionAsync(ex);
            }
        }
        finally
        {
            await ClientProgressForMessageThreadAsync(messageSettings.CurrentChatId, messageSettings.CurrentMessageId, message, isStartTask: false, messageSettings.ThreadNumber);
        }
    }

    /// <summary> Save all waiting messages to the storage </summary>
    private async Task SaveAllWaitingMessagesAsync(ITgDownloadViewModel tgDownloadSettings)
    {
        BatchMessagesCount = 0;
        if (MessageEntities.Count == 0) return;
        if (tgDownloadSettings.IsSaveMessages)
            await StorageManager.MessageRepository.SaveListAsync(MessageEntities);
        MessageEntities.Clear();
    }

    private bool IsFileLocked(string filePath)
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

    //   private async Task CreateFileWatcher(long sourceId, int messageId, string fileName, long fileSize)
    //   {
    //	long previousSize = 0;
    //    double milliseconds = 1_000;
    //    bool isFileNewDownload = true;

    //	while (IsFileLocked(fileName))
    //    {
    //		long transmitted = new FileInfo(fileName).Length;
    //		long sizeDiff = transmitted - previousSize;
    //		long fileSpeed = sizeDiff > 0 ? (long)(sizeDiff / milliseconds * 1000) : 0;
    //		previousSize = transmitted;
    //		await UpdateStateFileAsync(sourceId, messageId, fileName, fileSize, transmitted, fileSpeed, isFileNewDownload);
    //		isFileNewDownload = false;
    //	}
    //}

    private Client.ProgressCallback ClientProgressForFile(TgMessageSettings messageSettings, string fileName)
    {
        var sw = Stopwatch.StartNew();
        var isStartTask = true;
        return (transmitted, size) =>
        {
            if (string.IsNullOrEmpty(fileName))
            {
                isStartTask = false;
            }
            else
            {
                isStartTask = true;
                var fileSpeed = transmitted <= 0 || sw.Elapsed.Seconds <= 0 ? 0 : transmitted / sw.Elapsed.Seconds;
                var task = UpdateStateFileAsync(messageSettings.CurrentChatId, messageSettings.CurrentMessageId,
                    Path.GetFileName(fileName), size > 0 ? size : 0, transmitted, fileSpeed > 0 ? fileSpeed : 0, isStartTask, messageSettings.ThreadNumber);
                task.Wait();
            }
        };
    }

    private async Task ClientProgressForMessageThreadAsync(long sourceId, int messageId, string message, bool isStartTask, int threadNumber) =>
        await UpdateStateMessageThreadAsync(sourceId, messageId, message, isStartTask, threadNumber);

    private async Task<long> GetPeerIdAsync(string userName) => (await ExecuteWithFloodControlAsync(() => Client.Contacts_ResolveUsername(userName))).peer.ID;

    public virtual async Task LoginUserAsync(bool isProxyUpdate) => await UseOverrideMethodAsync();

    public async Task DisconnectClientAsync()
    {
        try
        {
            IsProxyUsage = false;
            await UpdateStateSourceAsync(0, 0, 0, string.Empty);
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

    #region Public and private methods

    private async Task TryCatchFuncAsync(Func<Task> actionAsync,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        for (var attempt = 0; attempt < MaxRetryCount; attempt++)
        {
            try
            {
                await actionAsync();
                break;
            }
            catch (Exception ex)
            {
                TgLogUtils.WriteExceptionWithMessage(ex, TgConstants.LogTypeNetwork);
#if DEBUG
                Debug.WriteLine($"{TgFileUtils.GetShortFilePath(filePath)} | {memberName} | {lineNumber}", TgConstants.LogTypeNetwork);
#endif
                if (attempt < MaxRetryCount - 1)
                    await Task.Delay(DelayBetweenRetriyCount);
                else
                    break;
                // CHANNEL_INVALID | BadMsgNotification 48
                if (ClientException.Message.Contains("You must connect to Telegram first"))
                {
                    await SetClientExceptionShortAsync(ex);
                    await UpdateStateMessageAsync("Reconnect client ...");
                    await LoginUserAsync(isProxyUpdate: false);
                }
                else
                {
                    if (!string.IsNullOrEmpty(ex.Source) && ex.Source.Equals("WTelegramClient"))
                    {
                        switch (ex.Message)
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
                        await SetClientExceptionAsync(ex);
                }
            }
        }
    }

    private async Task<T> TryCatchFuncTAsync<T>(Func<Task<T>> actionAsync) where T : new()
    {
        try
        {
            return await actionAsync();
        }
        catch (Exception ex)
        {
            // CHANNEL_INVALID | BadMsgNotification 48
            if (ClientException.Message.Contains("You must connect to Telegram first"))
            {
                await SetClientExceptionShortAsync(ex);
                await UpdateStateMessageAsync("Reconnect client ...");
                await LoginUserAsync(isProxyUpdate: false);
            }
            else
            {
                if (!string.IsNullOrEmpty(ex.Source) && ex.Source.Equals("WTelegramClient"))
                {
                    switch (ex.Message)
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
                    await SetClientExceptionAsync(ex);
            }
            return new();
        }
    }

    private void TryCatchAction(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            // CHANNEL_INVALID | BadMsgNotification 48
            if (ClientException.Message.Contains("You must connect to Telegram first"))
            {
                var task = SetClientExceptionShortAsync(ex);
                task.Wait();
                UpdateStateMessageAsync("Reconnect client ...");
                task = LoginUserAsync(isProxyUpdate: false);
                task.Wait();
            }
            else
            {
                Task? task = null;
                if (!string.IsNullOrEmpty(ex.Source) && ex.Source.Equals("WTelegramClient"))
                {
                    switch (ex.Message)
                    {
                        case "PEER_ID_INVALID":
                            task = UpdateStateExceptionShortAsync("The source is invalid!");
                            task.Wait();
                            break;
                        case "CHANNEL_PRIVATE":
                            task = UpdateStateExceptionShortAsync("The channel is private!");
                            task.Wait();
                            break;
                        case "CHANNEL_INVALID":
                            task = UpdateStateExceptionShortAsync("The channel is invalid!");
                            task.Wait();
                            break;
                        default:
                            task = SetClientExceptionShortAsync(ex);
                            task.Wait();
                            break;
                    }
                }
                else
                {
                    task = SetClientExceptionAsync(ex);
                    task.Wait();
                }
            }
        }
    }

    [GeneratedRegex(@"FLOOD_WAIT_(\d+)")]
    private static partial Regex RegexFloodWait();

    private static int ExtractFloodWaitSeconds(string message)
    {
        var match = RegexFloodWait().Match(message);
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    /// <inheritdoc />
    public async Task<T> ExecuteWithFloodControlAsync<T>(Func<Task<T>> telegramCall)
    {
        while (true)
        {
            try
            {
                return await telegramCall();
            }
            catch (RpcException ex) when (ex.Code == 420 && ex.Message.StartsWith("FLOOD_WAIT_"))
            {
                var waitSeconds = ExtractFloodWaitSeconds(ex.Message);
#if DEBUG
                Debug.WriteLine($"Flood wait: {waitSeconds} seconds. Pausing...");
#endif
                await Task.Delay(TimeSpan.FromSeconds(waitSeconds));
            }
        }
    }

    private static async Task UseOverrideMethodAsync()
    {
        await Task.CompletedTask;
        throw new NotImplementedException(TgConstants.UseOverrideMethod);
    }

    #endregion

    #region Public and private methods - Connections

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
        if (DicChatsAll.TryGetValue(ReduceChatId(chatId), out var chatBase) && chatBase is not null)
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
            if (DicChatsAll.Count != 0)
            {
                var chatBase = DicChatsAll.FirstOrDefault(x => x.Key == chatId).Value;
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

            if (DicChatsAll.Count != 0)
            {
                var chatBase = DicChatsAll.FirstOrDefault(x => x.Key == chatId).Value;

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
                var response = await ExecuteWithFloodControlAsync(() => Client.Channels_GetParticipant(inputChannel, new InputUser((long)userId, access_hash: 0)));
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
                var participants = await ExecuteWithFloodControlAsync(() => Client.Channels_GetParticipants(inputChannel, filter, offset, limit, 0));
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
                var messages = await ExecuteWithFloodControlAsync(() => Client.Messages_GetHistory(inputChannel, offset_id: 0, limit: limit,
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
            var messages = await ExecuteWithFloodControlAsync(() => Client.Messages_GetHistory(inputChannel, offset_id: 0, limit: 1,
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

        tgDownloadSettings.SourceVm = new TgEfSourceViewModel { Dto = new TgEfSourceDto { UserName = userName } };
        await CreateChatBaseCoreAsync(tgDownloadSettings);

        if (Client is null) return new();

        // Get details about a public chat (even if client is not a member of that chat)
        var fullChat = await ExecuteWithFloodControlAsync(() => Client.GetFullChat(tgDownloadSettings.Chat.Base));
        WTelegram.Types.ChatFullInfo? chatDetails;
        if (fullChat is null)
            TgLog.WriteLine(TgLocale.TgGetChatDetailsError);
        else
        {
            chatDetails = ConvertToChatFullInfo(fullChat);
            await GetParticipantsAsync(chatDetails.Id);
            return FillChatDetailsDto(chatDetails);
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
        var fullChat = await ExecuteWithFloodControlAsync(() => Client.GetFullChat(tgDownloadSettings.Chat.Base));
        WTelegram.Types.ChatFullInfo? chatDetails;
        if (fullChat is null)
            TgLog.WriteLine(TgLocale.TgGetChatDetailsError);
        else
        {
            chatDetails = ConvertToChatFullInfo(fullChat);
            return FillChatDetailsDto(chatDetails);
        }
        return new();
    }

    /// <inheritdoc />
    public async Task<TgChatDetailsDto> GetChatDetailsByClientAsync(Guid uid)
    {
        if (uid == Guid.Empty) return new();

        var dto = await StorageManager.SourceRepository.GetDtoAsync(x => x.Uid == uid);
        var tgDownloadSettings = new TgDownloadSettingsViewModel { SourceVm = new TgEfSourceViewModel { Dto = dto } };
        return await GetChatDetailsByClientCoreAsync(tgDownloadSettings);
    }

    /// <inheritdoc />
    public async Task<TgChatDetailsDto> GetChatDetailsByClientAsync(long id)
    {
        if (id <= 0) return new();

        var dto = await StorageManager.SourceRepository.GetDtoAsync(x => x.Id == id);
        var tgDownloadSettings = new TgDownloadSettingsViewModel { SourceVm = new TgEfSourceViewModel { Dto = dto } };
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
                    ? $"enabled: {channelFull.slowmode_seconds} seconds" : $"disbaled";
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
