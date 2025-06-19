// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

using TL;

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
    #region Public and private fields, properties, constructor

    public TgClientMonitoringViewModel ClientMonitoringVm { get; set; } = new();
    public Client? ClientForSendData { get; set; }

    #endregion

    #region Public and private methods

    private TgEnumMenuClientConSearch SetMenuClientConSearch()
    {
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(
            TgLocale.MenuReturn,
            TgLocale.MenuClientSearchChat,
            TgLocale.MenuClientSearchUser
        );
        // Check paid license
        if (BusinessLogicManager.LicenseService.CurrentLicense.CheckPaidLicense())
            selectionPrompt.AddChoices(
                TgLocale.MenuClientStartMonitoringChats,
                TgLocale.MenuClientStopMonitoringChats
            );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuClientSearchChat))
            return TgEnumMenuClientConSearch.Chat;
        if (prompt.Equals(TgLocale.MenuClientSearchUser))
            return TgEnumMenuClientConSearch.User;
        if (prompt.Equals(TgLocale.MenuClientStartMonitoringChats))
            return TgEnumMenuClientConSearch.StartMonitoringChats;
        if (prompt.Equals(TgLocale.MenuClientStopMonitoringChats))
            return TgEnumMenuClientConSearch.StopMonitoringChats;

        return TgEnumMenuClientConSearch.Return;
    }

    public async Task SetupClientConSearchAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        TgEnumMenuClientConSearch menu;
        do
        {
            await ShowTableClientConSearchAsync();

            menu = SetMenuClientConSearch();
            switch (menu)
            {
                case TgEnumMenuClientConSearch.Chat:
                    await ClientSearchChatAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConSearch.User:
                    await ClientSearchUserAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConSearch.StartMonitoringChats:
                    await ClientStartMonitoringChatsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConSearch.StopMonitoringChats:
                    await ClientStopMonitoringChatsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConSearch.Return:
                    await ClientStopMonitoringChatsAsync(tgDownloadSettings);
                    break;
            }
        } while (menu is not TgEnumMenuClientConSearch.Return);
    }

    /// <summary> Check client connection </summary>
    private async Task<Client?> CheckClientConnectionAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ClientConnectAsync(tgDownloadSettings, isSilent: true);
        var isClientConnect = await BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync();
        if (!isClientConnect) return null;

        if (BusinessLogicManager.ConnectClient.Client is not Client client) return null;

        return client;
    }

    /// <summary> Client search chat </summary>
    private async Task ClientSearchChatAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        var client = await CheckClientConnectionAsync(tgDownloadSettings);
        if (client is null)
        {
            TgLog.WriteLine(TgLocale.MenuClientConfigurationError);
            return;
        }

        // Get the name of the public chat to search for
        var input = AnsiConsole.Ask<string>($"  {TgLocale.MenuSetChatName}:");
        if (string.IsNullOrEmpty(input))
        {
            TgLog.WriteLine(TgLocale.MenuSetChatNameIsEmpty);
            return;
        }
        input = TgStringUtils.NormilizeTgName(input, isAddAt: false);

        // Get details about a public chat (even if client is not a member of that chat)
        tgDownloadSettings.SourceVm = new TgEfSourceViewModel();
        tgDownloadSettings.SourceVm.Dto = new TgEfSourceDto();
        tgDownloadSettings.SourceVm.Dto.UserName = input;
        await BusinessLogicManager.ConnectClient.CreateChatBaseCoreAsync(tgDownloadSettings);
        var fullChat = await client.GetFullChat(tgDownloadSettings.Chat.Base);
        if (fullChat is null)
            TgLog.WriteLine(TgLocale.TgGetChatDetailsError);
        else
        {
            var chatDetails = ConvertToChatFullInfo(fullChat);
            PrintTableWithChatInfo(chatDetails);
        }
        TgLog.TypeAnyKeyForReturn();
    }

    /// <summary> Convert TL.Messages_ChatFull to WTelegram.Types.ChatFullInfo </summary>
    private WTelegram.Types.ChatFullInfo ConvertToChatFullInfo(TL.Messages_ChatFull messagesChatFull)
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

    /// <summary> Get InputUser by username </summary>
    private async Task<TL.InputUser?> GetInputUserAsync(Client client, string username)
    {
        try
        {
            var result = await client.Contacts_ResolveUsername(username);
            if (result?.users?.Values?.FirstOrDefault() is TL.User user)
            {
                return new TL.InputUser(user.ID, user.access_hash);
            }
        }
        catch (Exception ex)
        {
            TgLog.WriteLine($"{TgLocale.TgErrorAnalyzingUsername} '{username}': {ex.Message}");
        }
        return null;
    }

    /// <summary> Client search user </summary>
    private async Task ClientSearchUserAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        var client = await CheckClientConnectionAsync(tgDownloadSettings);
        if (client is null)
        {
            TgLog.WriteLine(TgLocale.MenuClientConfigurationError);
            return;
        }

        // Get the public username of the user to search for
        var input = AnsiConsole.Ask<string>($"  {TgLocale.MenuSetUserName}:");
        if (string.IsNullOrEmpty(input))
        {
            TgLog.WriteLine(TgLocale.MenuSetUserNameIsEmpty);
            return;
        }
        input = TgStringUtils.NormilizeTgName(input, isAddAt: false);

        // Get details about a user via the public username (even if not in discussion with client)
        var inputUser = await GetInputUserAsync(client, input);
        if (inputUser is { user_id: var userId })
        {
            var userDetails = await client.Users_GetFullUser(inputUser);
            if (userDetails is null)
                TgLog.WriteLine(TgLocale.TgGetUserDetailsError);
            else
                PrintTableWithUserInfo(userDetails, userId);
            TgLog.TypeAnyKeyForReturn();
        }
    }

    /// <summary> Start monitoring chats for new messages </summary>
    private async Task ClientStartMonitoringChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (ClientMonitoringVm.IsStartMonitoring) return;

        ClientMonitoringVm.Default();

        var client = await CheckClientConnectionAsync(tgDownloadSettings);
        if (client is null)
        {
            TgLog.WriteLine(TgLocale.MenuClientConfigurationError);
            return;
        }
        ClientForSendData = client;

        // Get username for send messages
        ClientMonitoringVm.IsSendToMyself = AskQuestionYesNoReturnPositive(TgLocale.MenuClientSendMessagesToMyself);
        if (!ClientMonitoringVm.IsSendToMyself)
        {
            var userNameInput = AnsiConsole.Ask<string>($"  {TgLocale.MenuSetUserNameForSendMessages}:");
            if (string.IsNullOrEmpty(userNameInput))
            {
                TgLog.WriteLine(TgLocale.MenuSetUserNameIsEmpty);
                return;
            }
            ClientMonitoringVm.UserName = TgStringUtils.NormilizeTgName(userNameInput, isAddAt: false);
            var inputUser = await GetInputUserAsync(client, ClientMonitoringVm.UserName);
            if (inputUser is { user_id: var userId })
            {
                var userDetails = await client.Users_GetFullUser(inputUser);
                if (userDetails is null)
                    TgLog.WriteLine(TgLocale.TgGetUserDetailsError);
                else
                    ClientMonitoringVm.UserId = userId;
            }
        }

        // Get chats
        List<string>? chatNames = null;
        ClientMonitoringVm.IsSearchAtAllChats = AskQuestionYesNoReturnPositive(TgLocale.MenuClientSearchAtAllChats);
        if (!ClientMonitoringVm.IsSearchAtAllChats)
        {
            var chatNamesInput = AnsiConsole.Ask<string>($"  {TgLocale.MenuSetChatNames}:");
            if (string.IsNullOrEmpty(chatNamesInput))
            {
                TgLog.WriteLine(TgLocale.MenuSetUserNameIsEmpty);
                return;
            }
            chatNames = TgStringUtils.NormilizeTgNames(chatNamesInput, isAddAt: false);
        }

        // Get keywords to filter messages
        ClientMonitoringVm.IsSkipKeywords = AskQuestionYesNoReturnPositive(TgLocale.MenuClientSkipKeywords);
        if (!ClientMonitoringVm.IsSkipKeywords)
        {
            var keywordsInput = AnsiConsole.Ask<string>($"  {TgLocale.MenuSetKeywordsForMessageFiltering}:");
            if (string.IsNullOrEmpty(keywordsInput))
            {
                TgLog.WriteLine(TgLocale.MenuSetKeywordsForFilterMessagesIsEmpty);
                return;
            }
            ClientMonitoringVm.Keywords = [.. keywordsInput
            .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(keyword => keyword.Trim().ToUpper())];
        }

        // Start monitoring chats for new messages
        if (!ClientMonitoringVm.IsSearchAtAllChats && chatNames is not null)
        {
            TgLog.WriteLine($"  {TgLocale.MenuClientStartMonitoringChats} {chatNames.Count} chats...");
            foreach (var chatName in chatNames)
            {
                var joined = await TryJoinChatAsClientAsync(tgDownloadSettings, client, chatName);
                if (!joined)
                    continue;
                ClientMonitoringVm.ChatNames.Add(chatName.ToUpper());
                ClientMonitoringVm.ChatIds.Add(tgDownloadSettings.SourceVm.Dto.Id);
            }
        }

        // Subscribe
        if (ClientForSendData is not null)
        {
            BusinessLogicManager.ConnectClient.ClearCaches();
            if (!BusinessLogicManager.ConnectClient.DicChatsAll.Any())
                await BusinessLogicManager.ConnectClient.CollectAllChatsAsync();
            ClientForSendData.OnUpdates -= OnClientUpdatesAsync;
            ClientForSendData.OnUpdates += OnClientUpdatesAsync;
            TgLog.WriteLine($"  {TgLocale.MenuClientMonitoringStarted}");
            ClientMonitoringVm.IsStartMonitoring = true;
        }
        TgLog.TypeAnyKeyForReturn();
    }

    // <summary> Stop monitoring chats for new messages </summary>
    private async Task ClientStopMonitoringChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (!ClientMonitoringVm.IsStartMonitoring) return;

        ClientMonitoringVm.Default();

        // Unsubscribe
        if (ClientForSendData is not null)
        {
            BusinessLogicManager.ConnectClient.ClearCaches();
            ClientForSendData?.OnUpdates -= OnClientUpdatesAsync;
            ClientMonitoringVm.IsStartMonitoring = false;
            TgLog.WriteLine($"  {TgLocale.MenuClientMonitoringStopped}");
        }
        await Task.CompletedTask;
    }

    /// <summary> Join client to chat by username or id if it is not already a member </summary>
    private async Task<bool> TryJoinChatAsClientAsync(TgDownloadSettingsViewModel tgDownloadSettings, Client client, string chatNameOrId)
    {
        try
        {
            // Get details about a public chat (even if client is not a member of that chat)
            tgDownloadSettings.SourceVm = new TgEfSourceViewModel();
            tgDownloadSettings.SourceVm.Dto = new TgEfSourceDto();
            tgDownloadSettings.SourceVm.Dto.UserName = chatNameOrId;
            await BusinessLogicManager.ConnectClient.CreateChatBaseCoreAsync(tgDownloadSettings);
            var fullChat = await client.GetFullChat(tgDownloadSettings.Chat.Base);
            // Get chat details
            if (fullChat is null)
            {
                TgLog.WriteLine($"{TgLocale.TgGetChatDetailsError} {chatNameOrId}");
                return false;
            }

            // Check that full_chat is ChannelFull
            if (fullChat.full_chat is not TL.ChannelFull channelFull)
            {
                TgLog.WriteLine($"  Chat {chatNameOrId} is not a channel or supergroup");
                return false;
            }

            // Look for the Channel object in the chats dictionary with the required id to get access_hash
            if (!fullChat.chats.TryGetValue(channelFull.id, out var chatBase) || chatBase is not TL.Channel channel)
            {
                TgLog.WriteLine($"  Unable to find Channel with id {channelFull.id} to get the access_hash");
                return false;
            }

            // Create InputChannel for API calls
            var inputChannel = new TL.InputChannel(channel.id, channel.access_hash);

            // Check if client is already a member of the chat
            var isMember = await CheckUserMemberAsync(client, inputChannel, ClientMonitoringVm.UserId);
            if (!isMember)
            {
                TgLog.WriteLine($"  The client is not a member of the chat {inputChannel.channel_id}");
                // Try to join the chat
                await client.Channels_JoinChannel(inputChannel);
                isMember = await CheckUserMemberAsync(client, inputChannel, ClientMonitoringVm.UserId);
                if (!isMember)
                    TgLog.WriteLine($"  Can not join to the chat {chatNameOrId}");
            }

            // Save ID
            tgDownloadSettings.SourceVm.Dto.Id = channel.id;
            return true;
        }
        catch (Exception ex)
        {
            TgLog.WriteLine($"  Error when joining the client to the chat {chatNameOrId}: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> CheckUserMemberAsync(Client client, TL.InputChannel inputChannel, long userId)
    {
        try
        {
            var inputUser = new TL.InputUser(userId, 0);
            var participant = await client.Channels_GetParticipant(inputChannel, inputUser);
            return participant is not null;
        }
        catch (Exception ex)
        {
            TgLog.WriteLine($"{TgLocale.TgErrorCheckUserMember}: user ID {userId}, chat ID {inputChannel.channel_id}: {ex.Message}");
            return false;
        }
    }

    /// <summary> Client update event </summary>
    private async Task OnClientUpdatesAsync(UpdatesBase update)
    {
        if (!ClientMonitoringVm.IsStartMonitoring) return;
        if (update is null || update.UpdateList is null) return;
        if (!ClientMonitoringVm.IsSendToMyself && string.IsNullOrEmpty(ClientMonitoringVm.UserName)) return;

        // Check if the message is from a chat that we are monitoring
        foreach (var itemUpdate in update.UpdateList)
        {
            // Process new messages
            if (itemUpdate is TL.UpdateNewMessage updateNewMessage)
            {
                if (updateNewMessage.message is Message message)
                {
                    var chatId = message.Peer.ID;

                    // If the chat is not in the monitoring list - skip it
                    if (!ClientMonitoringVm.IsSearchAtAllChats && !ClientMonitoringVm.ChatIds.Contains(chatId)) continue;

                    // Getting the message text
                    var text = (message.message ?? string.Empty).ToUpper();

                    // Filter by keywords
                    if (ClientMonitoringVm.IsSkipKeywords || ClientMonitoringVm.Keywords.Any(keyword => text.Contains(keyword)))
                    {
                        if (ClientForSendData is Client client)
                        {
                            await SendMonitoredMessageAsync(message, chatId, client);
                        }
                    }
                }
            }
        }

        await Task.CompletedTask;
    }

    /// <summary> Send a monitored message to the specified chat </summary>
    private async Task SendMonitoredMessageAsync(Message message, long chatId, Client client)
    {
        if (!ClientMonitoringVm.IsStartMonitoring) return;

        var lastDateTime = string.Empty;
        (string, string) lastUserLink = new();
        (string, string) lastMessageLink = new();
        string lastMessageText = string.Empty;
        try
        {
            // Add the date time
            lastDateTime = $"{message.Date.ToLocalTime():yyyy-MM-dd HH:mm:ss}";
            lastMessageText = $"{TgLocale.MenuClientLocalMessageDateTime}: {lastDateTime}";

            // Add the sender's username if available
            if (message.from_id is not null)
            {
                lastUserLink = await BusinessLogicManager.ConnectClient.GetUserLink(chatId, message.id, message.from_id);
                if (!string.IsNullOrEmpty(lastUserLink.Item2))
                    lastMessageText += $"{Environment.NewLine}[{lastUserLink.Item1}]({lastUserLink.Item2})";
                else if (!string.IsNullOrEmpty(lastUserLink.Item1))
                    lastMessageText += $"{Environment.NewLine}{lastUserLink.Item1}";
            }

            // Add the chat name if available
            lastMessageLink = BusinessLogicManager.ConnectClient.GetChatUserLink(chatId, message.id);
            if (!string.IsNullOrEmpty(lastMessageLink.Item2))
                lastMessageText += $"{Environment.NewLine}[{lastMessageLink.Item1}]({lastMessageLink.Item2})";
            else if (!string.IsNullOrEmpty(lastMessageLink.Item1))
                lastMessageText += $"{Environment.NewLine}{lastMessageLink.Item1}";

            // Add the message text
            if (!string.IsNullOrEmpty(message.message))
            {
                lastMessageText += 
                    $"{Environment.NewLine}```{Environment.NewLine}{message.message}{Environment.NewLine}```";
            }

            // Convert Markdown to entities
            var entities = client.MarkdownToEntities(ref lastMessageText);

            // Get the InputPeer of the user to whom we are sending the message
            if (!ClientMonitoringVm.IsSendToMyself)
            {
                if (ClientMonitoringVm.ResolvedPeer is null)
                    ClientMonitoringVm.ResolvedPeer = await client.Contacts_ResolveUsername(ClientMonitoringVm.UserName);
            }
            else
            {
                if (ClientMonitoringVm.ResolvedPeer is null)
                    ClientMonitoringVm.ResolvedPeer = await client.Contacts_ResolveUsername(client.User.MainUsername);
            }

            // Send message with entities
            ClientMonitoringVm.LastDateTime = lastDateTime;
            ClientMonitoringVm.LastUserLink = lastUserLink.Item2 ?? "-";
            ClientMonitoringVm.LastMessageLink = lastMessageLink.Item2 ?? "-";
            ClientMonitoringVm.LastMessageText = message.message ?? "-";
            ClientMonitoringVm.CatchMessages++;
            await ShowTableClientConSearchAsync();
            await client.SendMessageAsync(ClientMonitoringVm.ResolvedPeer, lastMessageText, entities: entities);
        }
        catch (Exception ex)
        {
            TgLog.WriteLine($"{TgLocale.MenuClientSendingMessageError}: {ex.Message}");
#if DEBUG
            Debug.WriteLine(ex);
            Debug.WriteLine(ex.StackTrace);
#endif
        }
    }

    #endregion
}
