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

    public Dictionary<long, TL.ChatBase> DicChatsAll { get; }

    public Func<long, int, int, string, Task> UpdateStateSourceAsync { get; }

	public Task LoginUserAsync(bool isProxyUpdate);
	public Task DisconnectClientAsync();
    public Task DisconnectBotAsync();
    public Task ConnectClientConsoleAsync(Func<string, string?>? config, TgEfProxyDto proxyDto);
    public Task ConnectBotConsoleAsync();
	public Task ConnectSessionDesktopAsync(TgEfProxyDto proxyDto, Func<string, string?> config);
    public Task ConnectSessionAsync(TgEfProxyDto proxyDto);

	public Task<Dictionary<long, TL.ChatBase>> CollectAllChatsAsync();

    public void SetupUpdateTitle(Func<string, Task> updateTitleAsync);
	public void SetupUpdateStateSource(Func<long, int, int, string, Task> updateStateSourceAsync);
	public void SetupUpdateStateFile(Func<long, int, string, long, long, long, bool, int, Task> updateStateFileAsync);
	public void SetupUpdateStateMessageThread(Func<long, int, string, bool, int, Task> updateStateMessageThreadAsync);
	public void SetupUpdateStateContact(Func<long, string, string, string, Task> updateStateContactAsync);
	public void SetupUpdateStateStory(Func<long, int, int, string, Task> updateStateStoryAsync);
	public void SetupUpdateStateMessage(Func<string, Task> updateStateMessageAsync);
	public void SetupUpdateStateConnect(Func<string, Task> updateStateConnectAsync);
	public void SetupUpdateStateProxy(Func<string, Task> updateStateProxyAsync);
	public void SetupUpdateStateException(Func<string, int, string, string, Task> updateStateExceptionAsync);
	public void SetupUpdateStateExceptionShort(Func<string, Task> updateStateExceptionShortAsync);
    public void SetupAfterClientConnect(Func<Task> afterClientConnectAsync);
	public void SetupGetClientDesktopConfig(Func<string, string?> getClientDesktopConfig);
	public void SetupUpdateException(Func<Exception, Task> updateExceptionAsync);
	public void SetupUpdateStateItemSource(Func<long, Task> updateStateItemSourceAsync);

	public Task SearchSourcesTgAsync(ITgDownloadViewModel tgDownloadSettings, TgEnumSourceType sourceType);
	public Task CreateChatAsync(ITgDownloadViewModel tgDownloadSettings, bool isSilent);
    public Task CreateChatBaseCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings);

    public Task SetChannelMessageIdFirstAsync(ITgDownloadViewModel tgDownloadSettings);
	public Task DownloadAllDataAsync(ITgDownloadViewModel tgDownloadSettings);
	public Task MarkHistoryReadAsync();
	public void SetForceStopDownloading();
	public Task UpdateSourceDbAsync(ITgEfSourceViewModel sourceVm, ITgDownloadViewModel tgDownloadSettings);
	public Task ScanSourcesTgDesktopAsync(TgEnumSourceType sourceType, Func<ITgEfSourceViewModel, Task> afterScanAsync);
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
    /// <summary> Get participant information from chat </summary>
    Task<User?> GetParticipantAsync(long chatId, long userId);
    /// <summary> Clear caches </summary>
    public void ClearCaches();
}