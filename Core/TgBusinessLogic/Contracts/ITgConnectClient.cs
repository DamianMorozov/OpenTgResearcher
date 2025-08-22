// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TL;

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
	public Task DisconnectClientAsync();
    public Task DisconnectBotAsync();
    public Task ConnectClientConsoleAsync(Func<string, string?>? config, TgEfProxyDto proxyDto);
    public Task ConnectBotConsoleAsync();
	public Task ConnectSessionDesktopAsync(TgEfProxyDto proxyDto, Func<string, string?> config);

	public Task<IEnumerable<long>> CollectAllChatsAsync();

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
	public void SetupUpdateException(Func<Exception, Task> updateExceptionAsync);

	public Task SearchSourcesTgAsync(ITgDownloadViewModel tgDownloadSettings, TgEnumSourceType sourceType, List<long>? chatIds = null);
	public Task CreateChatAsync(ITgDownloadViewModel tgDownloadSettings, bool isSilent);
    public Task CreateChatBaseCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings);

    public Task SetChannelMessageIdFirstAsync(ITgDownloadViewModel tgDownloadSettings);
	public Task ParseChatAsync(ITgDownloadViewModel tgDownloadSettings);
	public Task MarkHistoryReadAsync();
	public void SetForceStopDownloading();
	public Task UpdateSourceDbAsync(ITgEfSourceViewModel sourceVm, ITgDownloadViewModel tgDownloadSettings);
    /// <summary> Get user id </summary>
    public Task<long> GetUserIdAsync();
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
    public Task<(string, string)> GetUserLink(long chatId, int messageId, TL.Peer? peer);
    /// <summary> Get chat participant information </summary>
    Task<User?> GetParticipantAsync(long chatId, long? userId);
    /// <summary> Get chat participants information </summary>
    Task<List<TL.User>> GetParticipantsAsync(long chatId);
    /// <summary> Make an action with messages in a chat </summary>
    Task MakeFuncWithMessagesAsync(TgDownloadSettingsViewModel tgDownloadSettings, long chatId, Func<TgDownloadSettingsViewModel, ChatBase, MessageBase, Task> func);
    /// <summary> Get the last message ID in a chat </summary>
    Task<int> GetChatLastMessageIdAsync(long chatId);
    /// <summary> Clear caches </summary>
    public void ClearCaches();
    /// <summary> Get chat details </summary>
    public Task<TgChatDetailsDto> GetChatDetailsByClientAsync(string userName, TgDownloadSettingsViewModel tgDownloadSettings);
    /// <summary> Get chat details </summary>
    public Task<TgChatDetailsDto> GetChatDetailsByBotAsync(string userName);
    /// <summary> Get chat details </summary>
    public Task<TgChatDetailsDto> GetChatDetailsByClientAsync(Guid uid);
    /// <summary> Get chat details </summary>
    public Task<TgChatDetailsDto> GetChatDetailsByClientAsync(long id);
    /// <summary> Convert TL.Messages_ChatFull to WTelegram.Types.ChatFullInfo </summary>
    public WTelegram.Types.ChatFullInfo ConvertToChatFullInfo(Messages_ChatFull messagesChatFull);
    /// <summary> Update users </summary>
    public Task UpdateUsersAsync(List<TgEfUserDto> users);
    /// <summary> Checks if a file is locked by another process </summary>
    public bool CheckFileLocked(string filePath);
}
