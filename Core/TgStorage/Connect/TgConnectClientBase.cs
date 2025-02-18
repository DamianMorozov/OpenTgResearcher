// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TL;

namespace TgStorage.Connect;

/// <summary> Base connection client </summary>
public abstract partial class TgConnectClientBase : ObservableRecipient, ITgConnectClient
{
	#region Public and private fields, properties, constructor
	
	private static TgAppSettingsHelper TgAppSettings => TgAppSettingsHelper.Instance;
	private static TgLocaleHelper TgLocale => TgLocaleHelper.Instance;
	protected TgEfAppRepository AppRepository { get; } = new();
	protected TgEfContactRepository ContactRepository { get; } = new();
	protected TgEfDocumentRepository DocumentRepository { get; } = new();
	protected TgEfFilterRepository FilterRepository { get; } = new();
	protected TgEfMessageRepository MessageRepository { get; } = new();
	protected TgEfProxyRepository ProxyRepository { get; } = new();
	protected TgEfSourceRepository SourceRepository { get; } = new();
	protected TgEfStoryRepository StoryRepository { get; } = new();

	private static TgLogHelper TgLog => TgLogHelper.Instance;
	public WTelegram.Client? Client { get; set; }
	public TgExceptionViewModel ClientException { get; set; }
	public TgExceptionViewModel ProxyException { get; set; }
	public bool IsReady { get; private set; }
	public bool IsNotReady => !IsReady;
	public bool IsProxyUsage { get; private set; }
	public User? Me { get; protected set; }
	public Dictionary<long, ChatBase> DicChatsAll { get; private set; }
	public Dictionary<long, User> DicContactsAll { get; private set; }
	public Dictionary<long, StoryItem> DicStoriesAll { get; private set; }
	public Dictionary<long, ChatBase> DicChatsUpdated { get; }
	public Dictionary<long, User> DicContactsUpdated { get; }
	public IEnumerable<Channel> EnumerableChannels { get; set; }
	public IEnumerable<Channel> EnumerableGroups { get; set; }
	public IEnumerable<ChatBase> EnumerableChats { get; set; }
	public IEnumerable<ChatBase> EnumerableSmallGroups { get; set; }
	public IEnumerable<User> EnumerableContacts { get; set; }
	public IEnumerable<StoryItem> EnumerableStories { get; set; }
	public bool IsUpdateStatus { get; set; }
	public bool IsForceStopDownloading { get; set; }
	private IEnumerable<TgEfFilterDto> Filters { get; set; }
	public Func<string, Task> UpdateTitleAsync { get; private set; }
	public Func<string, Task> UpdateStateConnectAsync { get; private set; }
	public Func<string, Task> UpdateStateProxyAsync { get; private set; }
	public Func<long, int, int, string, Task> UpdateStateSourceAsync { get; private set; }
	public Func<long, string, string, string, Task> UpdateStateContactAsync { get; private set; }
	public Func<long, int, int, string, Task> UpdateStateStoryAsync { get; private set; }
	public Func<string, int, string, string, Task> UpdateStateExceptionAsync { get; private set; }
	public Func<Exception, Task> UpdateExceptionAsync { get; private set; }
	public Func<string, Task> UpdateStateExceptionShortAsync { get; private set; }
	public Func<Task> AfterClientConnectAsync { get; private set; }
	public Func<string, string?> ConfigClientDesktop { get; private set; }
	public Func<long, Task> UpdateStateItemSourceAsync { get; private set; }
	public Func<long, int, string, long, long, long, bool, int, Task> UpdateStateFileAsync { get; private set; }
	public Func<string, Task> UpdateStateMessageAsync { get; private set; }
	public Func<long, int, string, bool, int, Task> UpdateStateMessageThreadAsync { get; private set; }

	public Microsoft.Data.Sqlite.SqliteConnection? BotSqlConnection { get; private set; }
	public WTelegram.Bot? Bot { get; private set; }

	protected TgConnectClientBase()
	{
		DicChatsAll = [];
		DicContactsAll = [];
		DicStoriesAll = [];
		DicChatsUpdated = [];
		DicContactsUpdated = [];
		EnumerableChannels = [];
		EnumerableChats = [];
		EnumerableGroups = [];
		EnumerableSmallGroups = [];
		EnumerableContacts = [];
		EnumerableStories = [];
		ClientException = new();
		ProxyException = new();
		Filters = [];

		UpdateTitleAsync = _ => Task.CompletedTask;
		UpdateStateConnectAsync = _ => Task.CompletedTask;
		UpdateStateProxyAsync = _ => Task.CompletedTask;
		UpdateStateExceptionAsync = (_, _, _, _) => Task.CompletedTask;
		UpdateExceptionAsync = _ => Task.CompletedTask;
		UpdateStateExceptionShortAsync = _ => Task.CompletedTask;
		UpdateStateSourceAsync = (_, _, _, _) => Task.CompletedTask;
		UpdateStateContactAsync = (_, _, _, _) => Task.CompletedTask;
		UpdateStateStoryAsync = (_, _, _, _) => Task.CompletedTask;
		AfterClientConnectAsync = () => Task.CompletedTask;
		ConfigClientDesktop = _ => string.Empty;
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

	#endregion

	#region IDisposable

	public void Dispose()
	{
		Client?.Dispose();
		BotSqlConnection?.Dispose();
		Bot?.Dispose();
	}

	#endregion

	#region Public and private methods

	public string ToDebugString() => $"{TgCommonUtils.GetIsReady(IsReady)} | {Me}";

	public void SetupUpdateStateConnect(Func<string, Task> updateStateConnectAsync) =>
		UpdateStateConnectAsync = updateStateConnectAsync;

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

	public void SetupGetClientDesktopConfig(Func<string, string?> getClientDesktopConfig) =>
		ConfigClientDesktop = getClientDesktopConfig;

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

	public async Task<bool> CheckClientIsReadyAsync()
	{
		if (Bot is null)
		{
			var result = Client is { Disconnected: false };
			if (!result)
				return ClientResultDisconnected();
			switch (TgGlobalTools.AppType)
			{
				case TgEnumAppType.Console:
					if (!TgAppSettings.AppXml.IsExistsFileSession)
						return ClientResultDisconnected();
					break;
			}
			var storageResult = await ProxyRepository.GetCurrentProxyAsync(await AppRepository.GetCurrentAppAsync());
			if (TgAppSettings.IsUseProxy && !storageResult.IsExists)
				return ClientResultDisconnected();
			if (ProxyException.IsExist || ClientException.IsExist)
				return ClientResultDisconnected();
			return ClientResultConnected();
		}
		else
		{
			var result = Bot is not null && Bot.BotId > 0;
			if (!result)
				return ClientResultDisconnected();
			//switch (TgAsyncUtils.AppType)
			//{
			//	case TgEnumAppType.Console:
			//		if (!TgAppSettings.AppXml.IsExistsFileSession)
			//			return ClientResultDisconnected();
			//		break;
			//}
			var storageResult = await ProxyRepository.GetCurrentProxyAsync(await AppRepository.GetCurrentAppAsync());
			if (TgAppSettings.IsUseProxy && !storageResult.IsExists)
				return ClientResultDisconnected();
			if (ProxyException.IsExist || ClientException.IsExist)
				return ClientResultDisconnected();
			return ClientResultConnected();
		}
	}

	private bool ClientResultDisconnected()
	{
		UpdateStateSourceAsync(0, 0, 0, string.Empty);
		UpdateStateProxyAsync(TgLocale.ProxyIsDisconnect);
		UpdateStateConnectAsync(TgLocale.MenuClientIsDisconnected);
		return IsReady = false;
	}

	private bool ClientResultConnected()
	{
		UpdateStateConnectAsync(TgLocale.MenuClientIsConnected);
		return IsReady = true;
	}

	public async Task ConnectSessionConsoleAsync(Func<string, string?>? config, ITgDbProxy proxy)
	{
		if (IsReady)
			return;
		await DisconnectAsync();
		Client = new(config);
		await ConnectThroughProxyAsync(proxy, false);
		Client.OnUpdates += OnUpdatesClientAsync;
		Client.OnOther += OnClientOtherAsync;
		await LoginUserAsync(true);
	}

	public async Task ConnectSessionAsync(ITgDbProxy? proxy)
	{
		if (IsReady)
			return;
		await DisconnectAsync();
		Client = new(ConfigClientDesktop);
		await ConnectThroughProxyAsync(proxy, true);
		Client.OnUpdates += OnUpdatesClientAsync;
		Client.OnOther += OnClientOtherAsync;
		await LoginUserAsync(true);
	}

	public async Task ConnectSessionDesktopAsync(ITgDbProxy? proxy, Func<string, string?> config)
	{
		if (IsReady)
			return;
		await DisconnectAsync();
		Client = new(config);
		await ConnectThroughProxyAsync(proxy, true);
		Client.OnUpdates += OnUpdatesClientAsync;
		Client.OnOther += OnClientOtherAsync;
		await LoginUserAsync(true);
	}

	public async Task ConnectThroughProxyAsync(ITgDbProxy? proxy, bool isDesktop)
	{
		IsProxyUsage = false;
		if (!await CheckClientIsReadyAsync())
			return;
		if (Client is null)
			return;
		if (proxy?.Uid == Guid.Empty)
			return;
		if (!isDesktop && !TgAppSettings.IsUseProxy)
			return;
		if (Equals(proxy?.Type, TgEnumProxyType.None))
			return;
		if (proxy is TgEfProxyEntity efProxy)
			if (!TgEfUtils.GetEfValid(efProxy).IsValid)
				return;

		try
		{
			ProxyException = new();
			IsProxyUsage = true;
			switch (proxy?.Type)
			{
				case TgEnumProxyType.Http:
				case TgEnumProxyType.Socks:
					Client.TcpHandler = (address, port) =>
					{
						Socks5ProxyClient proxyClient = string.IsNullOrEmpty(proxy.UserName) && string.IsNullOrEmpty(proxy.Password)
							? new(proxy.HostName, proxy.Port) : new(proxy.HostName, proxy.Port, proxy.UserName, proxy.Password);
						UpdateStateProxyAsync(TgLocale.ProxyIsConnected);
						return Task.FromResult(proxyClient.CreateConnection(address, port));
					};
					break;
				case TgEnumProxyType.MtProto:
					Client.MTProxyUrl = string.IsNullOrEmpty(proxy.Secret)
						? $"https://t.me/proxy?server={proxy.HostName}&port={proxy.Port}"
						: $"https://t.me/proxy?server={proxy.HostName}&port={proxy.Port}&secret={proxy.Secret}";
					await UpdateStateProxyAsync(TgLocale.ProxyIsConnected);
					break;
			}
		}
		catch (Exception ex)
		{
			IsProxyUsage = false;
			await SetProxyExceptionAsync(ex);
		}
	}

	public async Task ConnectBotDesktopAsync(string botToken, int apiId, string apiHash, string localFolder)
	{
		// https://github.com/wiz0u/WTelegramBot/blob/master/Examples/ConsoleApp/Program.cs
		//if (IsReady)
		//	return;
		//await DisconnectAsync();
		//Client = new(ConfigClientDesktop);
		//await ConnectThroughProxyAsync(proxy, true);
		//Client.OnUpdates += OnUpdatesClientAsync;
		//Client.OnOther += OnClientOtherAsync;
		//await LoginUserAsync(true);
		if (!Directory.Exists(localFolder))
			return;
		var WTelegramLogs = new StreamWriter($"{localFolder}\\WTelegramBot.log", true, Encoding.UTF8) { AutoFlush = true };
		WTelegram.Helpers.Log = (lvl, str) => WTelegramLogs.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");

		BotSqlConnection = new Microsoft.Data.Sqlite.SqliteConnection(@"Data Source=WTelegramBot.sqlite");
		Bot = new WTelegram.Bot(botToken, apiId, apiHash, BotSqlConnection);
		var my = await Bot.GetMe();

		await AfterClientConnectAsync();
	}

	public static long ReduceChatId(long chatId) => !$"{chatId}".StartsWith("-100") ? chatId : Convert.ToInt64($"{chatId}"[4..]);

	public string GetUserUpdatedName(long id) => DicContactsUpdated.TryGetValue(ReduceChatId(id), out var user) ? user.username : string.Empty;

	public async Task<Channel?> GetChannelAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		// Collect chats from Telegram.
		if (!DicChatsAll.Any())
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
			messagesChats = await Client.Channels_GetChannels(new InputChannel(tgDownloadSettings.SourceVm.Dto.Id, Me.access_hash));
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
		if (!DicChatsAll.Any())
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
			messagesChats = await Client.Channels_GetGroupsForDiscussion();

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
		if (!DicChatsAll.Any())
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
				var messages = await Client.Messages_GetAllChats();
				FillEnumerableChats(messages.chats);
				return messages.chats;
			}
		}
		return [];
	}

	public async Task CollectAllDialogsAsync()
	{
		switch (IsReady)
		{
			case true when Client is not null:
			{
				var messages = await Client.Messages_GetAllDialogs();
				FillEnumerableChats(messages.chats);
				break;
			}
		}
	}

	//public async Task AddChatAsync(string userName)
	//{
	//	switch (IsReady)
	//	{
	//		case true when Client is not null:
	//		{
	//			Contacts_ResolvedPeer peer = await Client.Contacts_ResolveUsername(userName);
	//			if (peer?.peer is PeerChannel peerChannel)
	//			{
	//				//var messages = await Client.Messages_GetChats(peerChannel.channel_id);
	//				//var messages = await Client.Channels_GetChannels(new InputChannel[] { new InputChannel(id, 0) } );
	//				long accessHash = peerChannel.access_hash;
	//				//var channels = await Client.Channels_GetChannels(new[] { new InputChannel(, 0) });
	//				var channels = await Client.Channels_GetChannels(new[] { new InputChannel(peerChannel.channel_id, 0) });
	//				//var channel = channels.chats[0];
	//				//if (channel.IsChannel)
	//				//{
	//				//	var fullChannel = await Client.Channels_GetFullChannel(channel);
	//				//}
	//				//	var channelId = channel.id;
	//				//var accessHash = channel.access_hash;
	//				//AddEnumerableChats(messages.chats);
	//			}
	//			break;
	//		}
	//	}
	//}

	public async Task CollectAllContactsAsync()
	{
		switch (IsReady)
		{
			case true when Client is not null:
			{
				var contacts = await Client.Contacts_GetContacts();
				FillEnumerableContacts(contacts.users);
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
				var storiesBase = await Client.Stories_GetAllStories();
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
		var listChats = new List<ChatBase>();
		var listSmallGroups = new List<ChatBase>();
		var listChannels = new List<Channel>();
		var listGroups = new List<Channel>();
		// Sort
		var chatsSorted = chats.OrderBy(i => i.Value.MainUsername).ThenBy(i => i.Value.ID).ToList();
		foreach (var chat in chatsSorted)
		{
			listChats.Add(chat.Value);
			switch (chat.Value)
			{
				case Chat smallGroup when (smallGroup.flags & Chat.Flags.deactivated) is 0:
					listSmallGroups.Add(chat.Value);
					break;
				case Channel { IsGroup: true } group:
					listGroups.Add(group);
					break;
				case Channel channel:
					listChannels.Add(channel);
					break;
			}
		}
		EnumerableChannels = listChannels;
		EnumerableChats = listChats;
		EnumerableGroups = listGroups;
		EnumerableSmallGroups = listSmallGroups;
	}

	private void FillEnumerableContacts(Dictionary<long, User> users)
	{
		DicContactsAll = users;
		var listContacts = new List<User>();
		// Sort
		var usersSorted = users.OrderBy(i => i.Value.username).ThenBy(i => i.Value.ID);
		foreach (var user in usersSorted)
		{
			listContacts.Add(user.Value);
		}
		EnumerableContacts = listContacts;
	}

	private void FillEnumerableStories(List<PeerStories> peerStories)
	{
		DicStoriesAll = [];
		var listStories = new List<StoryItem>();
		// Sort
		var peerStoriesSorted = peerStories.OrderBy(i => i.stories.Rank);
		foreach (var peerStory in peerStories)
		{
			foreach (var storyBase in peerStory.stories)
			{
				if (storyBase is StoryItem story)
					listStories.Add(story);
			}
		}
		EnumerableStories = listStories;
	}

	public async Task OnUpdatesClientAsync(IObject arg)
	{
		if (!IsUpdateStatus)
			return;
		if (arg is UpdateShort updateShort)
			await OnUpdateShortClientAsync(updateShort);
		if (arg is UpdatesBase updates)
			await OnUpdateClientUpdatesAsync(updates);
	}

	private async Task OnUpdateShortClientAsync(UpdateShort updateShort)
	{
		try
		{
			updateShort.CollectUsersChats(DicContactsUpdated, DicChatsUpdated);
			if (updateShort.UpdateList.Any())
			{
				foreach (var update in updateShort.UpdateList)
				{
					try
					{
						if (updateShort.Chats.Any())
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
						await SetClientExceptionAsync(ex);
					}
				}
			}
		}
		catch (Exception ex)
		{
			await SetClientExceptionAsync(ex);
		}
	}

	private async Task OnUpdateClientUpdatesAsync(UpdatesBase updates)
	{
		try
		{
			updates.CollectUsersChats(DicContactsUpdated, DicChatsUpdated);
			if (updates.UpdateList.Any())
			{
				foreach (var update in updates.UpdateList)
				{
					try
					{
						if (updates.Chats.Any())
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
						await SetClientExceptionAsync(ex);
					}
				}
			}
		}
		catch (Exception ex)
		{
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

	private async Task OnClientOtherAsync(IObject arg)
	{
		if (!IsUpdateStatus)
			return;
		if (arg is Auth_SentCodeBase authSentCode)
			await OnClientOtherAuthSentCodeAsync(authSentCode);
	}

	private async Task OnClientOtherAuthSentCodeAsync(Auth_SentCodeBase authSentCode)
	{
		try
		{
#if DEBUG
			Debug.WriteLine($"{nameof(authSentCode)}: {authSentCode}", TgConstants.LogTypeNetwork);
#endif
		}
		catch (Exception ex)
		{
			await SetClientExceptionAsync(ex);
		}
	}


	//private void Client_DisplayMessage(MessageBase messageBase, bool edit = false)
	//{
	//	if (edit) Console.Write("(Edit): ");
	//	switch (messageBase)
	//	{
	//		case TL.Message message:
	//			TgLog.MarkupLine($"{GetPeerUpdatedName(message.from_id)} in {GetPeerUpdatedName(message.peer_id)}> {message.message}");
	//			break;
	//		case MessageService messageService:
	//			TgLog.MarkupLine($"{GetPeerUpdatedName(messageService.from_id)} in {GetPeerUpdatedName(messageService.peer_id)} [{messageService.action.GetType().Name[13..]}]");
	//			break;
	//	}
	//}

	//public async Task PrintSendAsync(Messages_Chats messagesChats)
	//{
	//	Console.Write("Type a chat ID to send a message: ");
	//	string? input = Console.ReadLine();
	//	if (!string.IsNullOrEmpty(input))
	//	{
	//		long chatId = long.Parse(input);
	//		TL.ChatBase target = messagesChats.chats[chatId];
	//		TgLog.MarkupLine($"Type the message into the chat: {target.Title} | {chatId}");
	//		input = Console.ReadLine();
	//		if (Client is { })
	//			await Client.SendMessageAsync(target, input);
	//	}
	//}

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
			fullChannel = await Client.Channels_GetFullChannel(channel);
			if (isSave)
				await SourceRepository.SaveAsync(new() { Id = channel.id, UserName = channel.username, Title = channel.title });
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
			chatFull = await Client.GetFullChat(chatBase);
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

	public async Task<bool> IsChatBaseAccessAsync(ChatBase chatBase)
	{
		if (Bot is null)
		{
			if (Client is null || chatBase.ID is 0)
				return false;
			var result = false;
			await TryCatchFuncAsync(async () =>
			{
				await Client.ReadHistory(chatBase);
				result = true;
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
				var chatDetails = await GetChatDetails(chatBase.MainUsername, chatBase.ID);
				result = true;
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
			var fullChannel = await Client.Channels_GetFullChannel(channel);
			if (fullChannel.full_chat is not ChannelFull channelFull)
				return 0;
			var isAccessToMessages = await Client.Channels_ReadMessageContents(channel);
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
			var fullChannel = await Client.GetFullChat(chatBase);
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
		await GetChannelMessageIdWithLockAsync(tgDownloadSettings, TgEnumPosition.Last);

	private int GetChannelMessageIdLastCore(ChannelFull channelFull) =>
		channelFull.read_inbox_max_id;

	private int GetChannelMessageIdLastCore(ChatFullBase chatFullBase) =>
		chatFullBase is ChannelFull channelFull ? channelFull.read_inbox_max_id : 0;

	private async Task<int> GetChannelMessageIdLastCoreAsync(Channel channel)
	{
		var fullChannel = await Client.Channels_GetFullChannel(channel);
		if (fullChannel.full_chat is not ChannelFull channelFull)
			return 0;
		return channelFull.read_inbox_max_id;
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
			var messages = await Client.Channels_GetMessages(chatBase as Channel, inputMessages);
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
		var dto = tgDownloadSettings2.SourceVm.Dto;
		var source = await SourceRepository.GetItemAsync(new() { Id = dto.Id }, isReadOnly: false);
		await CreateChatBaseCoreAsync(tgDownloadSettings2);

		if (Bot is not null)
		{
			//await GetChatTestAsync();
			var chatDetails = await GetChatDetails(dto.UserName, dto.Id);
			if (chatDetails is not null)
			{
				if (chatDetails.TLInfo is Messages_ChatFull { full_chat: ChannelFull channelFull })
				{
					if (channelFull.slowmode_seconds > 0)
						source.UserName = chatDetails.Username;
					source.Count = channelFull.read_inbox_max_id;
					source.Title = chatDetails.Title;
					source.Id = ReduceChatId(channelFull.ID);
					source.About = channelFull.About;
					source.AccessHash = chatDetails.AccessHash;
				}
				//if (chatDetails.TLInfo is TL.Messages_ChatFull chatFull)
				//{
				//	if (chatFull.chats.Any())
				//	{
				//		if (chatFull.chats.FirstOrDefault().Value is TL.Channel channel)
				//			source.AccessHash = channel.access_hash;
				//	}
				//}
			}
		}
		else
		{
			if (tgDownloadSettings2.Chat.Base is { } chatBase && await IsChatBaseAccessAsync(chatBase))
			{
				source.IsUserAccess = true;
				source.UserName = chatBase.MainUsername ?? string.Empty;
				source.Count = await GetChannelMessageIdLastAsync(tgDownloadSettings);
				var chatFull = await PrintChatsInfoChatBaseAsync(chatBase, isFull: true, isSilent);
				source.Title = chatBase.Title;
				if (chatFull?.full_chat is ChannelFull chatBaseFull)
				{
					source.Id = ReduceChatId(chatBaseFull.ID);
					source.About = chatBaseFull.About;
				}
			}
			else
			{
				source.IsUserAccess = false;
			}
		}
		source.Directory = dto.Directory;
		source.FirstId = dto.FirstId;

		await SourceRepository.SaveAsync(source);
		tgDownloadSettings2.SourceVm.Fill(source);
	}

	private async Task<WTelegram.Types.ChatFullInfo?> GetChatDetails(string dtoUserName, long dtoId)
	{
		if (Bot is null)
			return null;
		WTelegram.Types.ChatFullInfo? chatDetails = null;
		if (!string.IsNullOrEmpty(dtoUserName))
		{
			var userName = dtoUserName.StartsWith('@') ? dtoUserName : $"@{dtoUserName}";
			chatDetails = await Bot.GetChat(userName);
		}
		else
		{
			long? chatId = dtoId.ToString().StartsWith("-100") ? dtoId :
				long.TryParse($"-100{dtoId}", out var id) ? id : null;
			if (chatId is not null)
				chatDetails = await Bot.GetChat(chatId);
		}
		return chatDetails;
	}

	//private async Task GetChatTestAsync()
	//{
	//	// get details about a public chat (even if bot is not a member of that chat)
	//	if (Bot is null)
	//		return;
	//	var chatDetails = await Bot.GetChat("@tdlibchat");
	//	if (chatDetails.TLInfo is TL.Messages_ChatFull { full_chat: TL.ChannelFull channelFull })
	//	{
	//		Console.WriteLine($"@{chatDetails.Username} has {channelFull.participants_count} members, {channelFull.online_count} online");
	//		if (channelFull.slowmode_seconds > 0)
	//			Console.WriteLine($"@{chatDetails.Username} has slowmode enabled: {channelFull.slowmode_seconds} seconds");
	//		if (channelFull.available_reactions is TL.ChatReactionsAll { flags: TL.ChatReactionsAll.Flags.allow_custom })
	//			Console.WriteLine($"@{chatDetails.Username} allows custom emojis as reactions");
	//	}
	//}

	/// <summary> Update source from Telegram </summary>
	public async Task UpdateSourceDbAsync(ITgEfSourceViewModel sourceVm, ITgDownloadViewModel tgDownloadSettings)
	{
		if (sourceVm is not TgEfSourceViewModel sourceVm2) return;
		if (tgDownloadSettings is not TgDownloadSettingsViewModel tgDownloadSettings2) return;
		await CreateChatAsync(tgDownloadSettings, isSilent: true);
		sourceVm2.Dto.Fill(tgDownloadSettings2.SourceVm.Dto, isUidCopy: false);
	}

	private async Task UpdateSourceTgCoreAsync(Channel channel, string about, int count)
	{
		var storageResult = await SourceRepository.GetAsync(new() { Id = channel.id });
		var sourceNew = storageResult.IsExists ? storageResult.Item : new();
		sourceNew.Id = channel.id;
		sourceNew.AccessHash = channel.access_hash;
		sourceNew.IsActive = channel.IsActive;
		sourceNew.UserName = channel.username;
		sourceNew.Title = channel.title;
		sourceNew.About = about;
		sourceNew.Count = count;
		// Save
		await SourceRepository.SaveAsync(sourceNew);
	}

	private async Task UpdateSourceTgCoreAsync(ChatBase chatBase, string about, int count)
	{
		var storageResult = await SourceRepository.GetAsync(new() { Id = chatBase.ID });
		var sourceNew = storageResult.IsExists ? storageResult.Item : new();
		sourceNew.Id = chatBase.ID;
		sourceNew.AccessHash = 0;
		sourceNew.IsActive = chatBase.IsActive;
		sourceNew.UserName = chatBase.MainUsername;
		sourceNew.Title = chatBase.Title;
		sourceNew.About = about;
		sourceNew.Count = count;
		// Save
		await SourceRepository.SaveAsync(sourceNew);
	}

	private async Task UpdateContactTgAsync(User user)
	{
		var storageResult = await ContactRepository.GetAsync(new() { Id = user.id });
		var contactNew = storageResult.IsExists ? storageResult.Item : new();
		contactNew.DtChanged = DateTime.UtcNow;
		contactNew.Id = user.id;
		contactNew.AccessHash = user.access_hash;
		contactNew.IsActive = user.IsActive;
		contactNew.IsBot = user.IsBot;
		contactNew.FirstName = user.first_name;
		contactNew.LastName = user.last_name;
		contactNew.UserName = user.username;
		contactNew.UserNames = user.usernames is null ? string.Empty : string.Join("|", user.usernames.ToList());
		contactNew.PhoneNumber = user.phone;
		contactNew.Status = user.status is null ? string.Empty : user.status.ToString();
		contactNew.RestrictionReason = user.restriction_reason is null ? string.Empty : string.Join("|", user.restriction_reason.ToList());
		contactNew.LangCode = user.lang_code;
		contactNew.StoriesMaxId = user.stories_max_id;
		contactNew.BotInfoVersion = user.bot_info_version.ToString();
		contactNew.BotInlinePlaceholder = user.bot_inline_placeholder is null ? string.Empty : user.bot_inline_placeholder.ToString();
		contactNew.BotActiveUsers = user.bot_active_users;
		// Save
		await ContactRepository.SaveAsync(contactNew);
	}

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
		var storageResult = await StoryRepository.GetAsync(new() { Id = story.id });
		var storyNew = storageResult.IsExists ? storageResult.Item : new();
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
			//storyNew.Message = message.ToString();
			// Switch message type
			TgEfStoryEntityByMessageType(storyNew, message);
		}
		// Switch media type
		TgEfStoryEntityByMediaType(storyNew, story.media);
		// Save
		await StoryRepository.SaveAsync(storyNew);
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
				//storyNew.Message = ((TL.MessageEntityUrl)message);
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

	private async Task UpdateSourceTgAsync(Channel channel, int count) =>
		await UpdateSourceTgCoreAsync(channel, string.Empty, count);

	private async Task UpdateSourceTgAsync(ChatBase chatBase, int count) =>
		await UpdateSourceTgCoreAsync(chatBase, string.Empty, count);

	public async Task SearchSourcesTgAsync(ITgDownloadViewModel tgDownloadSettings, TgEnumSourceType sourceType)
	{
		if (tgDownloadSettings is not TgDownloadSettingsViewModel tgDownloadSettings2) return;
		await TryCatchFuncAsync(async () =>
		{
			await LoginUserAsync();
			switch (sourceType)
			{
				case TgEnumSourceType.Chat:
					await UpdateStateSourceAsync(tgDownloadSettings2.SourceVm.Dto.Id, 0, tgDownloadSettings2.SourceVm.Dto.Count, TgLocale.CollectChats);
					await CollectAllChatsAsync();
					tgDownloadSettings.SourceScanCount = DicChatsAll.Count;
					tgDownloadSettings.SourceScanCurrent = 0;
					// List groups
					await SearchSourcesTgConsoleForGroupsAsync(tgDownloadSettings);
					// List channels
					await SearchSourcesTgConsoleForChannelsAsync(tgDownloadSettings2);
					break;
				case TgEnumSourceType.Dialog:
					await UpdateStateSourceAsync(tgDownloadSettings2.SourceVm.Dto.Id, 0, tgDownloadSettings2.SourceVm.Dto.Count, TgLocale.CollectDialogs);
					await CollectAllDialogsAsync();
					tgDownloadSettings.SourceScanCount = DicChatsAll.Count;
					tgDownloadSettings.SourceScanCurrent = 0;
					// List groups
					await SearchSourcesTgConsoleForGroupsAsync(tgDownloadSettings);
					// List channels
					await SearchSourcesTgConsoleForChannelsAsync(tgDownloadSettings2);
					break;
				case TgEnumSourceType.Contact:
					await UpdateStateSourceAsync(tgDownloadSettings2.ContactVm.Dto.Id, 0, 0, TgLocale.CollectContacts);
					await CollectAllContactsAsync();
					tgDownloadSettings.SourceScanCount = DicContactsAll.Count;
					tgDownloadSettings.SourceScanCurrent = 0;
					// List contacts
					await SearchSourcesTgConsoleForContactsAsync(tgDownloadSettings2);
					break;
				case TgEnumSourceType.Story:
					await UpdateStateStoryAsync(tgDownloadSettings2.StoryVm.Dto.Id, 0, 0, TgLocale.CollectStories);
					await CollectAllStoriesAsync();
					tgDownloadSettings.SourceScanCount = DicStoriesAll.Count;
					tgDownloadSettings.SourceScanCurrent = 0;
					// List stories
					await SearchSourcesTgConsoleForStoriesAsync(tgDownloadSettings2);
					break;
			}
		});
		await UpdateTitleAsync(string.Empty);
	}

	private async Task SearchSourcesTgConsoleForChannelsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		foreach (var channel in EnumerableChannels)
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
						var fullChannel = await Client.Channels_GetFullChannel(channel);
						if (fullChannel?.full_chat is ChannelFull channelFull)
						{
							await UpdateSourceTgCoreAsync(channel, channelFull.about, messagesCount);
							await UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceScanCurrent,
								tgDownloadSettings.SourceScanCount, $"{channel} | {TgDataFormatUtils.TrimStringEnd(channelFull.about, 40)}");
						}
					}
					else
					{
						await UpdateSourceTgAsync(channel, messagesCount);
						await UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceScanCurrent,
							tgDownloadSettings.SourceScanCount, $"{channel}");
					}
				});
			}
			await UpdateTitleAsync($"{TgCommonUtils.CalcSourceProgress(tgDownloadSettings.SourceScanCount,
				tgDownloadSettings.SourceScanCurrent):#00.00} %");
		}
	}

	private async Task SearchSourcesTgConsoleForGroupsAsync(ITgDownloadViewModel tgDownloadSettings)
	{
		await SearchSourcesTgConsoleForGroupsCoreAsync(tgDownloadSettings, EnumerableSmallGroups);
		await SearchSourcesTgConsoleForGroupsCoreAsync(tgDownloadSettings, EnumerableGroups);
	}

	private async Task SearchSourcesTgConsoleForGroupsCoreAsync(ITgDownloadViewModel tgDownloadSettings, IEnumerable<ChatBase> groups)
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
						await UpdateSourceTgAsync(channel, messagesCount);
					else
						await UpdateSourceTgAsync(group, messagesCount);
					if (tgDownloadSettings is TgDownloadSettingsViewModel tgDownloadSettings2)
						await UpdateStateSourceAsync(tgDownloadSettings2.SourceVm.Dto.Id, 0, 0, $"{group} | {messagesCount}");
				});
			}
		}
	}

	private async Task SearchSourcesTgConsoleForContactsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		foreach (var user in EnumerableContacts)
		{
			tgDownloadSettings.SourceScanCurrent++;
			await TryCatchFuncAsync(async () =>
			{
				await UpdateContactTgAsync(user);
				await UpdateStateContactAsync(user.id, user.first_name, user.last_name, user.username);
			});
			await UpdateTitleAsync($"{TgCommonUtils.CalcSourceProgress(tgDownloadSettings.SourceScanCount,
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
			await UpdateTitleAsync($"{TgCommonUtils.CalcSourceProgress(tgDownloadSettings.SourceScanCount,
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
			await UpdateTitleAsync($"{TgCommonUtils.CalcSourceProgress(tgDownloadSettings.SourceScanCount,
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
							var messages = await Client.Messages_GetAllChats();
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
							var messages = await Client.Messages_GetAllDialogs();
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
						var fullChannel = await Client.Channels_GetFullChannel(channel);
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
			// Filters
			Filters = await FilterRepository.GetListDtosAsync(0, 0, x => x.IsEnabled);

			await LoginUserAsync();

			var dirExists = await CreateDestDirectoryIfNotExistsAsync(tgDownloadSettings);
			if (!dirExists)
			{
				tgDownloadSettings2.SourceVm.Dto.FirstId = tgDownloadSettings2.SourceVm.Dto.Count;
				return;
			}

			tgDownloadSettings2.SourceVm.Dto.IsDownload = true;
			var isAccessToMessages = tgDownloadSettings.Chat.Base is Channel channel && await Client.Channels_ReadMessageContents(channel);
			var sourceFirstId = tgDownloadSettings2.SourceVm.Dto.FirstId;
			var sourceLastId = tgDownloadSettings2.SourceVm.Dto.Count;
			var counterForSave = 0;
			if (isAccessToMessages)
			{
				List<Task> downloadTasks = [];
				var threadNumber = 0;
				while (sourceFirstId <= sourceLastId)
				{
					if (Client is null)
					{
						tgDownloadSettings2.SourceVm.Dto.FirstId = sourceLastId;
						tgDownloadSettings2.SourceVm.Dto.IsDownload = false;
						downloadTasks.Clear();
						break;
					}
					if (Client is not null && Client.Disconnected || !tgDownloadSettings2.SourceVm.Dto.IsDownload)
					{
						tgDownloadSettings2.SourceVm.Dto.FirstId = sourceLastId;
						tgDownloadSettings2.SourceVm.Dto.IsDownload = false;
						downloadTasks.Clear();
						break;
					}
					await TryCatchFuncAsync(async () =>
					{
						if (Client is null)
							return;
						var messages = tgDownloadSettings.Chat.Base is not null
							? await Client.Channels_GetMessages(tgDownloadSettings.Chat.Base as Channel, sourceFirstId)
							: await Client.GetMessages(tgDownloadSettings.Chat.Base, sourceFirstId);
						await UpdateTitleAsync($"{TgCommonUtils.CalcSourceProgress(sourceLastId, sourceFirstId):#00.00} %");
						foreach (var message in messages.Messages)
						{
							// Check message exists
							downloadTasks.Add(message.Date > DateTime.MinValue
								? DownloadDataAsync(tgDownloadSettings2, message, threadNumber)
								: UpdateStateSourceAsync(tgDownloadSettings2.SourceVm.Dto.Id, message.ID, tgDownloadSettings2.SourceVm.Dto.Count,
									$"Message {message.ID} is not exists in {tgDownloadSettings2.SourceVm.Dto.Id}!"));
							counterForSave++;
						}
					});
					sourceFirstId++;
					threadNumber++;
					// CountThreads
					if (downloadTasks.Count == tgDownloadSettings2.CountThreads || sourceFirstId >= sourceLastId)
					{
						await Task.WhenAll(downloadTasks);
						downloadTasks.Clear();
						threadNumber = 0;
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
						await SourceRepository.SaveAsync(tgDownloadSettings2.SourceVm.Dto.GetEntity());
					}
				}
				tgDownloadSettings2.SourceVm.Dto.FirstId = sourceFirstId > sourceLastId ? sourceLastId : sourceFirstId;
			}
			else
			{
				tgDownloadSettings2.SourceVm.Dto.FirstId = sourceLastId;
			}
			tgDownloadSettings2.SourceVm.Dto.IsDownload = false;
		});
		await UpdateTitleAsync(string.Empty);
	}

	public void SetForceStopDownloading() => IsForceStopDownloading = true;

	public async Task MarkHistoryReadAsync()
	{
		await TryCatchFuncAsync(async () =>
		{
			await LoginUserAsync();
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
						var isSuccess = await Client.ReadHistory(chatBase);
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
			await SetClientExceptionAsync(ex);
			return false;
		}
		return true;
	}

	private async Task DownloadDataAsync(TgDownloadSettingsViewModel tgDownloadSettings, MessageBase messageBase, int threadNumber,
		int maxRetries = 6, int delayBetweenRetries = 10_000)
	{
		if (messageBase is not Message message)
		{
			await UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, messageBase.ID, tgDownloadSettings.SourceVm.Dto.Count, "Empty message");
			return;
		}
		await TryCatchFuncAsync(async () =>
		{
			// Store message
			await MessageSaveAsync(tgDownloadSettings, message.ID, message.Date, 0, message.message, TgEnumMessageType.Message, threadNumber);
			// Parse documents and photos
			if ((message.flags & Message.Flags.has_media) is not 0)
			{
				await DownloadDataCoreAsync(tgDownloadSettings, messageBase, message.media, threadNumber);
			}
			await UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, message.ID, tgDownloadSettings.SourceVm.Dto.Count,
				$"Reading the message {message.ID} from {tgDownloadSettings.SourceVm.Dto.Count}");
			await UpdateStateItemSourceAsync(tgDownloadSettings.SourceVm.Dto.Id);
		}, maxRetries, delayBetweenRetries);
	}

	private TgMediaInfoModel GetMediaInfo(MessageMedia messageMedia, TgDownloadSettingsViewModel tgDownloadSettings, MessageBase messageBase)
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
					if (!string.IsNullOrEmpty(document.Filename) &&
						CheckFileAtFilter(document.Filename, extensionName, document.size))
					{
						mediaInfo = new(document.Filename, document.size, document.date);
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
					if (CheckFileAtFilter(string.Empty, extensionName, photo.sizes.Last().FileSize))
					{
						mediaInfo = new($"{photo.ID}.{extensionName}", photo.sizes.Last().FileSize, photo.date);
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
		// Join tgDownloadSettings.DestDirectory
		mediaInfo.LocalPathOnly = tgDownloadSettings.SourceVm.Dto.Directory;
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

	private async Task DownloadDataCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings, MessageBase messageBase,
		MessageMedia messageMedia, int threadNumber)
	{
		var mediaInfo = GetMediaInfo(messageMedia, tgDownloadSettings, messageBase);
		if (string.IsNullOrEmpty(mediaInfo.LocalNameOnly))
			return;
		// Delete files
		DeleteExistsFiles(tgDownloadSettings, mediaInfo);
		// Move exists file at current directory
		MoveExistsFilesAtCurrentDir(tgDownloadSettings, mediaInfo);
		// Download new file
		if (!File.Exists(mediaInfo.LocalPathWithNumber))
		{
			await using var localFileStream = File.Create(mediaInfo.LocalPathWithNumber);
#if DEBUG
			Debug.WriteLine($"{nameof(DownloadDataCoreAsync)} | {mediaInfo.LocalPathWithNumber}", TgConstants.LogTypeSystem);
#endif
			if (Client is not null)
			{
				switch (messageMedia)
				{
					case MessageMediaDocument mediaDocument:
						if ((mediaDocument.flags & MessageMediaDocument.Flags.has_document) is not 0 && mediaDocument.document is Document document)
						{
							await Client.DownloadFileAsync(document, localFileStream, null,
								ClientProgressForFile(tgDownloadSettings.SourceVm.Dto.Id, messageBase.ID, mediaInfo.LocalNameWithNumber, threadNumber));
							TgEfDocumentEntity doc = new()
							{
								Id = document.ID,
								SourceId = tgDownloadSettings.SourceVm.Dto.Id,
								MessageId = messageBase.ID,
								FileName = mediaInfo.LocalPathWithNumber,
								FileSize = mediaInfo.RemoteSize,
								AccessHash = document.access_hash
							};
							await DocumentRepository.SaveAsync(doc);
						}
						break;
					case MessageMediaPhoto mediaPhoto:
						if (mediaPhoto is { photo: Photo photo })
						{
							await Client.DownloadFileAsync(photo, localFileStream, null,
								ClientProgressForFile(tgDownloadSettings.SourceVm.Dto.Id, messageBase.ID, mediaInfo.LocalNameWithNumber, threadNumber));
						}
						break;
				}
			}
			localFileStream.Close();
		}
		// Store message
		await MessageSaveAsync(tgDownloadSettings, messageBase.ID, mediaInfo.DtCreate, mediaInfo.RemoteSize, mediaInfo.RemoteName, TgEnumMessageType.Document, threadNumber);
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
		if (!tgDownloadSettings.IsRewriteFiles)
			return;
		TryCatchAction(() =>
		{
			if (File.Exists(mediaInfo.LocalPathWithNumber))
			{
				var fileSize = TgFileUtils.CalculateFileSize(mediaInfo.LocalPathWithNumber);
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
			if (!files.Any())
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

	private async Task MessageSaveAsync(TgDownloadSettingsViewModel tgDownloadSettings, int messageId, DateTime dtCreated, long size, string message,
		TgEnumMessageType messageType, int threadNumber, bool isRetry = false)
	{
		try
		{
			await ClientProgressForMessageThreadAsync(tgDownloadSettings.SourceVm.Dto.Id, messageId, message, isStartTask: true, threadNumber);
			var storageResult = await MessageRepository.GetAsync(
				new() { SourceId = tgDownloadSettings.SourceVm.Dto.Id, Id = tgDownloadSettings.SourceVm.Dto.FirstId }, isReadOnly: true);
			if (!storageResult.IsExists || storageResult.IsExists && tgDownloadSettings.IsRewriteMessages)
			{
				var sourceItem = await SourceRepository.GetItemAsync(new() { Id = tgDownloadSettings.SourceVm.Dto.Id }, isReadOnly: true);
				var messageItem = new TgEfMessageEntity
				{
					Id = messageId,
					Source = sourceItem,
					SourceId = sourceItem.Id,
					DtCreated = dtCreated,
					Type = messageType,
					Size = size,
					Message = message,
				};
#if DEBUG
				Debug.WriteLine($"MessageSaveAsync source: {sourceItem.ToConsoleString()}");
				Debug.WriteLine($"MessageSaveAsync message: {messageItem.ToConsoleString()}");
#endif
				await MessageRepository.SaveAsync(messageItem);
			}
			if (messageType == TgEnumMessageType.Document)
				await UpdateStateFileAsync(tgDownloadSettings.SourceVm.Dto.Id, messageId, string.Empty, 0, 0, 0, false, threadNumber);
		}
		catch (Exception ex)
		{
#if DEBUG
			Debug.WriteLine(ex);
			Debug.WriteLine(ex.StackTrace);
#endif
			if (!isRetry)
			{
				await Task.Delay(500);
				await MessageSaveAsync(tgDownloadSettings, messageId, dtCreated, size, message, messageType, threadNumber, isRetry: true);
			}
		}
		finally
		{
			await ClientProgressForMessageThreadAsync(tgDownloadSettings.SourceVm.Dto.Id, messageId, message, isStartTask: false, threadNumber);
		}
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

	private WTelegram.Client.ProgressCallback ClientProgressForFile(long sourceId, int messageId, string fileName, int threadNumber)
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
				var task = UpdateStateFileAsync(sourceId, messageId, Path.GetFileName(fileName), size > 0 ? size : 0, transmitted,
					fileSpeed > 0 ? fileSpeed : 0, isStartTask, threadNumber);
				task.Wait();
			}
		};
	}

	private async Task ClientProgressForMessageThreadAsync(long sourceId, int messageId, string message, bool isStartTask, int threadNumber) =>
		await UpdateStateMessageThreadAsync(sourceId, messageId, message, isStartTask, threadNumber);

	//private long GetAccessHash(long channelId) => Client?.GetAccessHashFor<Channel>(channelId) ?? 0;

	//private long GetAccessHash(string userName)
	//{
	//	Contacts_ResolvedPeer contactsResolved = Client.Contacts_ResolveUsername(userName);
	//	//WClient.Channels_JoinChannel(new InputChannel(channelId, accessHash));
	//	return GetAccessHash(contactsResolved.peer.ID);
	//}

	private async Task<long> GetPeerIdAsync(string userName) => (await Client.Contacts_ResolveUsername(userName)).peer.ID;

	//private async Task<long> GetChannelIdAsync(string userName) => (await Client.Channels_(userName)).peer.ID;

	// AUTH_KEY_DUPLICATED  | rpcException.Code, 406
	// "Could not read payload length : Connection shut down"
	//public string GetClientExceptionMessage() =>
	//    ClientException switch
	//    {
	//        RpcException rpcException => rpcException.InnerException is null
	//            ? $"{rpcException.Code} | {rpcException.Message}"
	//            : $"{rpcException.Code} | {rpcException.Message} | {rpcException.InnerException.Message}",
	//        { } ex => ex.InnerException is null ? ex.Message : $"{ex.Message} | {ex.InnerException.Message}",
	//        _ => string.Empty
	//    };

	public virtual async Task LoginUserAsync(bool isProxyUpdate = false) => await UseOverrideMethodAsync();

	public async Task DisconnectAsync()
	{
		IsProxyUsage = false;
		await UpdateStateSourceAsync(0, 0, 0, string.Empty);
		await UpdateStateProxyAsync(TgLocale.ProxyIsDisconnect);
		await UpdateStateConnectAsync(TgLocale.MenuClientIsDisconnected);
		if (Client is null)
			return;
		Client.OnUpdates -= OnUpdatesClientAsync;
		Client.OnOther -= OnClientOtherAsync;
		await Client.DisposeAsync();
		Client = null;
		ClientException = new();
		Me = null;
		await CheckClientIsReadyAsync();
		await AfterClientConnectAsync();

		// Bot
		BotSqlConnection?.Dispose();
		BotSqlConnection = null;
		Bot?.Dispose();
		Bot = null;
	}

	protected async Task SetClientExceptionAsync(Exception ex,
		[CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
	{
		ClientException.Set(ex);
		await UpdateExceptionAsync(ex);
		await UpdateStateExceptionAsync(filePath, lineNumber, memberName, ClientException.Message);
	}

	//private void SetClientExceptionShort(Exception ex)
	//{
	//	ClientException.Set(ex);
	//	UpdateStateExceptionShortAsync(ClientException.Message);
	//}

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
		await UpdateStateExceptionAsync(filePath, lineNumber, memberName, ProxyException.Message);
	}

	#endregion

	#region Public and private methods

	private async Task TryCatchFuncAsync(Func<Task> actionAsync,
		int maxRetries = 6, int delayBetweenRetries = 10_000,
		[CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
	{
		for (var attempt = 0; attempt < maxRetries; attempt++)
		{
			try
			{
				await actionAsync();
				break;
			}
			catch (Exception ex)
			{
#if DEBUG
				Debug.WriteLine($"{filePath} | {memberName} | {lineNumber}", TgConstants.LogTypeNetwork);
				Debug.WriteLine(ex, TgConstants.LogTypeNetwork);
#endif
				if (attempt < maxRetries - 1)
					await Task.Delay(delayBetweenRetries);
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
				await LoginUserAsync();
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
			// CHANNEL_INVALID | BadMsgNotification 48
			if (ClientException.Message.Contains("You must connect to Telegram first"))
			{
				var task = SetClientExceptionShortAsync(ex);
				task.Wait();
				UpdateStateMessageAsync("Reconnect client ...");
				task = LoginUserAsync();
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

	private static async Task UseOverrideMethodAsync()
	{
		await Task.CompletedTask;
		throw new NotImplementedException(TgLocaleHelper.Instance.UseOverrideMethod);
	}

	#endregion
}