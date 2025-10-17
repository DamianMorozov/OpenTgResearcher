namespace TgBusinessLogic.Contracts;

/// <summary> Telegram client </summary>
public interface ITgConnectClient : ITgDebug, IDisposable
{
    public bool IsReady { get; }
	public TL.User? Me { get; }
	public TgExceptionViewModel ClientException { get; set; }
	public TgExceptionViewModel ProxyException { get; set; }
	public WTelegram.Client? Client { get; set; }
    public WTelegram.Bot? Bot { get; }
    public TgBotInfoDto? BotInfoDto { get; }
    public bool IsClientUpdateStatus { get; set; }
    public bool IsBotUpdateStatus { get; set; }

    public ConcurrentDictionary<long, TL.ChatBase> DicChats { get; }

    public Func<Task> AfterClientConnectAsync { get; }
    public Func<long, int, int, string, Task> UpdateChatViewModelAsync { get; }
    public Func<int, int, TgEnumChatsMessageType, Task> UpdateChatsViewModelAsync { get; }

    public Task LoginUserAsync(bool isProxyUpdate);
	public Task DisconnectClientAsync(bool isAfterClientConnect = true);
    public Task DisconnectBotAsync(bool isAfterClientConnect = true);
    public Task ConnectBotConsoleAsync();
	public Task ConnectSessionAsync(Func<string, string?>? config, TgEfProxyDto proxyDto, bool isDesktop);

	public Task<IEnumerable<long>> CollectAllChatsAsync(CancellationToken ct = default);

    public void SetupUpdateTitle(Func<string, Task> updateTitleAsync);
    public void SetupUpdateChatViewModel(Func<long, int, int, string, Task> updateChatViewModelAsync);
    public void SetupUpdateChatsViewModel(Func<int, int, TgEnumChatsMessageType, Task> updateChatsViewModel);
	public void SetupUpdateShellViewModel(Func<bool, int, string, Task> updateShellViewModel);
	public void SetupUpdateStateFile(Func<long, int, string, long, long, long, bool, int, Task> updateStateFileAsync);
	public void SetupUpdateStateMessageThread(Func<long, int, string, bool, int, Task> updateStateMessageThreadAsync);
	public void SetupUpdateStateContact(Func<long, string, string, string, Task> updateStateContactAsync);
	public void SetupUpdateStateMessage(Func<string, Task> updateStateMessageAsync);
	public void SetupUpdateStateProxy(Func<string, Task> updateStateProxyAsync);
	public void SetupUpdateStateException(Func<string, int, string, string, Task> updateStateExceptionAsync);
	public void SetupUpdateStateExceptionShort(Func<string, Task> updateStateExceptionShortAsync);
    public void SetupAfterClientConnect(Func<Task> afterClientConnectAsync);
	public void SetupUpdateException(Action<Exception> updateException);

	public Task SearchSourcesTgAsync(ITgDownloadViewModel tgDownloadSettings, TgEnumSourceType sourceType, List<long>? listIds = null);
	public Task CreateChatAsync(ITgDownloadViewModel tgDownloadSettings, bool isSilent, CancellationToken ct = default);
    public Task CreateChatBaseCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings);

    public Task SetChannelMessageIdFirstAsync(ITgDownloadViewModel tgDownloadSettings, CancellationToken ct = default);
	public Task ParseChatAsync(ITgDownloadViewModel tgDownloadSettings);
	public Task MarkHistoryReadAsync(CancellationToken ct = default);
	public Task UpdateSourceDbAsync(ITgEfSourceViewModel sourceVm, ITgDownloadViewModel tgDownloadSettings, CancellationToken ct = default);
    /// <summary> Get user id </summary>
    public Task<long> GetUserIdAsync(CancellationToken ct = default);
    /// <summary> Checks if the client connection is ready </summary>
    public Task<bool> CheckClientConnectionReadyAsync();
    /// <summary> Checks if the bot connection is ready </summary>
    public Task<bool> CheckBotConnectionReadyAsync();
    public void SetBotInfoDto(TgBotInfoDto botInfoDto);
    /// <summary> Gets the chat username </summary>
    public string GetChatUserName(long chatId);
    /// <summary> Gets the chat user link </summary>
    public (string, string) GetChatUserLink(long chatId, int messageId);
    /// <summary> Gets the chat user link </summary>
    public (string, string) GetChatUserLink(long chatId);
    /// <summary> Get user link </summary>
    public Task<(string, string)> GetUserLink(long chatId, int messageId, TL.Peer? peer, CancellationToken ct = default);
    /// <summary> Get chat participants information </summary>
    public Task<List<TgParticipantDto>> GetParticipantsAsync(long chatId, CancellationToken ct = default);
    /// <summary> Get chat participant information </summary>
    public Task<TgParticipantDto?> GetParticipantsAsync(long chatId, long? userId, long accessHash = 0, CancellationToken ct = default);
    /// <summary> Make an action with messages in a chat </summary>
    public Task MakeFuncWithMessagesAsync(TgDownloadSettingsViewModel tgDownloadSettings, long chatId, Func<TgDownloadSettingsViewModel, TL.ChatBase, TL.MessageBase, Task> func, CancellationToken ct = default);
    /// <summary> Get the last message ID in a chat </summary>
    public Task<int> GetChatLastMessageIdAsync(long chatId, CancellationToken ct);
    /// <summary> Clear caches </summary>
    public void ClearCaches();
    /// <summary> Get chat details </summary>
    public Task<TgChatDetailsDto> GetChatDetailsByClientAsync(string userName, TgDownloadSettingsViewModel tgDownloadSettings, CancellationToken ct = default);
    /// <summary> Get chat details </summary>
    public Task<TgChatDetailsDto> GetChatDetailsByBotAsync(string userName);
    /// <summary> Get chat details </summary>
    public Task<TgChatDetailsDto> GetChatDetailsByClientAsync(Guid uid, CancellationToken ct = default);
    /// <summary> Get chat details </summary>
    public Task<TgChatDetailsDto> GetChatDetailsByClientAsync(long id, CancellationToken ct);
    /// <summary> Convert TL.Messages_ChatFull to WTelegram.Types.ChatFullInfo </summary>
    public WTelegram.Types.ChatFullInfo ConvertToChatFullInfo(TL.Messages_ChatFull messagesChatFull);
    /// <summary> Update users </summary>
    public Task UpdateUsersAsync(List<TgEfUserDto> users);
    /// <summary> Checks if a file is locked by another process </summary>
    public bool CheckFileLocked(string filePath);
    /// <summary> Check if user is a member of the chat </summary>
    public Task<bool> CheckUserMemberAsync(TL.InputChannel inputChannel, long userId, long accessHash);
    /// <summary> Check if user is a member of the chat </summary>
    public Task<bool> CheckUserMemberAsync(WTelegram.Client? client, TL.InputChannel inputChannel, long userId, long accessHash);
    /// <summary> Opens or creates the "Saved Messages" chat </summary>
    public Task<Guid> OpenOrCreateSavedMessagesAsync();
}
