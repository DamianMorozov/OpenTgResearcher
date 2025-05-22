// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TL;

namespace TgBusinessLogic.Contracts;

/// <summary> Telegram client </summary>
public interface ITgConnectClient : ITgCommon, IDisposable
{
	public bool IsReady { get; }
	public User? Me { get; }
	public TgExceptionViewModel ClientException { get; set; }
	public TgExceptionViewModel ProxyException { get; set; }
	public WTelegram.Client? Client { get; set; }
	public bool IsUpdateStatus { get; set; }
	public Dictionary<long, ChatBase> DicChatsAll { get; }
	public Func<long, int, int, string, Task> UpdateStateSourceAsync { get; }

	public Task LoginUserAsync(bool isProxyUpdate);
	public Task DisconnectAsync();
	public Task ConnectSessionConsoleAsync<TEfEntity>(Func<string, string?>? config, ITgDbProxy<TEfEntity> proxy) where TEfEntity : class, ITgEfEntity<TEfEntity>, new();
	public Task<bool> CheckClientIsReadyAsync();
	public Task ConnectSessionDesktopAsync<TEfEntity>(ITgDbProxy<TEfEntity>? proxy, Func<string, string?> config) where TEfEntity : class, ITgEfEntity<TEfEntity>, new();
	public Task ConnectBotDesktopAsync(string botToken, int apiId, string apiHash, string localFolder);
	public Task ConnectSessionAsync<TEfEntity>(ITgDbProxy<TEfEntity>? proxy) where TEfEntity : class, ITgEfEntity<TEfEntity>, new();

	public Task<Dictionary<long, ChatBase>> CollectAllChatsAsync();

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
	public Task SetChannelMessageIdFirstAsync(ITgDownloadViewModel tgDownloadSettings);
	public Task DownloadAllDataAsync(ITgDownloadViewModel tgDownloadSettings);
	public Task MarkHistoryReadAsync();
	public void SetForceStopDownloading();
	public Task UpdateSourceDbAsync(ITgEfSourceViewModel sourceVm, ITgDownloadViewModel tgDownloadSettings);
	public Task ScanSourcesTgDesktopAsync(TgEnumSourceType sourceType, Func<ITgEfSourceViewModel, Task> afterScanAsync);
    Task<long> GetUserIdAsync();
}