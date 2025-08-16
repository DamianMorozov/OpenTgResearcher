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
                TgLocale.MenuClientStopMonitoringChats,
                TgLocale.MenuClientSearchForKeywordsInChatsStart,
                TgLocale.MenuClientSearchForKeywordsInChatsStop
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
        if (prompt.Equals(TgLocale.MenuClientSearchForKeywordsInChatsStart))
            return TgEnumMenuClientConSearch.StartSearchForKeywordsInChats;
        if (prompt.Equals(TgLocale.MenuClientSearchForKeywordsInChatsStop))
            return TgEnumMenuClientConSearch.StopSearchForKeywordsInChats;

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
                    await ClientMonitoringStartAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConSearch.StopMonitoringChats:
                    await ClientMonitoringStopAsync(isSilent: false);
                    break;
                case TgEnumMenuClientConSearch.StartSearchForKeywordsInChats:
                    await ClientSearchingStartAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConSearch.StopSearchForKeywordsInChats:
                    await ClientSearchingStopAsync(isSilent: false);
                    break;
                case TgEnumMenuClientConSearch.Return:
                    await ClientSearchingStopAsync(isSilent: true);
                    await ClientMonitoringStopAsync(isSilent: true);
                    break;
            }
        } while (menu is not TgEnumMenuClientConSearch.Return);
    }

    /// <summary> Get client connection </summary>
    private async Task<Client?> GetClientConnectionAsync(TgDownloadSettingsViewModel tgDownloadSettings)
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
        var client = await GetClientConnectionAsync(tgDownloadSettings);
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

        var chatDetailsDto = await BusinessLogicManager.ConnectClient.GetChatDetailsByClientAsync(input, tgDownloadSettings);
        PrintTableWithChatInfo(chatDetailsDto);
        TgLog.TypeAnyKeyForReturn();
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
            TgLogUtils.WriteException(ex);
            TgLog.WriteLine($"{TgLocale.TgErrorAnalyzingUsername} '{username}': {ex.Message}");
        }
        return null;
    }

    /// <summary> Client search user </summary>
    private async Task ClientSearchUserAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        var client = await GetClientConnectionAsync(tgDownloadSettings);
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
    private async Task ClientMonitoringStartAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuClientStartMonitoringChats)) return;

        await ClientSearchingStopAsync(isSilent: true);
        if (ClientMonitoringVm.IsStartMonitoring) return;
        if (ClientMonitoringVm.IsStartSearching) return;

        ClientMonitoringVm.Default();

        // Get client connection
        ClientForSendData = await GetClientConnectionAsync(tgDownloadSettings);
        if (ClientForSendData is null)
        {
            TgLog.WriteLine(TgLocale.MenuClientConfigurationError);
            return;
        }

        // Get username for send messages
        await ClientMonitoringGetUserNameAsync();
        // Get chats
        var chatNames = ClientMonitoringGetChats();
        // Get keywords to filter messages
        GetKeywordsForMonitoring();
        // Add chats for monitoring
        await ClientMonitoringAddChatsAsync(tgDownloadSettings, chatNames);

        // Subscribe to client updates for monitoring new messages in chats
        await ClientMonitoringSubscribeAsync();

        TgLog.TypeAnyKeyForReturn();
    }

    /// <summary> Get username for send messages </summary>
    private async Task ClientMonitoringGetUserNameAsync()
    {
        if (ClientForSendData is null) return;

        // Flag for sending messages
        ClientMonitoringVm.IsSendMessages = AskQuestionYesNoReturnPositive(TgLocale.MenuClientSendMessages);
        if (!ClientMonitoringVm.IsSendMessages)
        {
            ClientMonitoringVm.UserName = TgStringUtils.NormilizeTgName(ClientForSendData.User.MainUsername, isAddAt: false);
            var inputUser = await GetInputUserAsync(ClientForSendData, ClientMonitoringVm.UserName);
            if (inputUser is { user_id: var userId })
            {
                var userDetails = await ClientForSendData.Users_GetFullUser(inputUser);
                if (userDetails is null)
                    TgLog.WriteLine(TgLocale.TgGetUserDetailsError);
                else
                    ClientMonitoringVm.UserId = userId;
            }
            return;
        }

        // Flag for sending messages to myself
        ClientMonitoringVm.IsSendToMyself = AskQuestionYesNoReturnPositive(TgLocale.MenuClientSendMessagesToMyself);
        if (!ClientMonitoringVm.IsSendToMyself)
        {
            // Get user name for sending messages
            var userNameInput = AnsiConsole.Ask<string>($"  {TgLocale.MenuSetUserNameForSendMessages}:");
            if (string.IsNullOrEmpty(userNameInput))
            {
                TgLog.WriteLine(TgLocale.MenuSetUserNameIsEmpty);
                return;
            }
            ClientMonitoringVm.UserName = TgStringUtils.NormilizeTgName(userNameInput, isAddAt: false);
            var inputUser = await GetInputUserAsync(ClientForSendData, ClientMonitoringVm.UserName);
            if (inputUser is { user_id: var userId })
            {
                var userDetails = await ClientForSendData.Users_GetFullUser(inputUser);
                if (userDetails is null)
                    TgLog.WriteLine(TgLocale.TgGetUserDetailsError);
                else
                    ClientMonitoringVm.UserId = userId;
            }
        }
    }

    /// <summary> Get chats for monitoring </summary>
    private List<string> ClientMonitoringGetChats()
    {
        List<string> chatNames = [];
        ClientMonitoringVm.IsSearchAtAllChats = AskQuestionYesNoReturnPositive(TgLocale.MenuClientSearchAtAllChats);
        if (!ClientMonitoringVm.IsSearchAtAllChats)
        {
            var chatNamesInput = AnsiConsole.Ask<string>($"  {TgLocale.MenuSetChatNames}:");
            if (string.IsNullOrEmpty(chatNamesInput))
            {
                TgLog.WriteLine(TgLocale.MenuSetUserNameIsEmpty);
                return chatNames;
            }
            chatNames = TgStringUtils.NormilizeTgNames(chatNamesInput, isAddAt: false);
        }
        return chatNames;
    }

    /// <summary> Get keywords for monitoring </summary>
    private void GetKeywordsForMonitoring()
    {
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
    }

    /// <summary> Add chats for monitoring </summary>
    private async Task ClientMonitoringAddChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings, List<string> chatNames)
    {
        if (ClientForSendData is null) return;

        if (!ClientMonitoringVm.IsSearchAtAllChats && chatNames is not null)
        {
            TgLog.WriteLine($"  {TgLocale.MenuClientStartMonitoringChats} {chatNames.Count} chats...");
            foreach (var chatName in chatNames)
            {
                var joined = await TryJoinChatAsClientAsync(tgDownloadSettings, ClientForSendData, chatName);
                if (!joined)
                    continue;
                ClientMonitoringVm.ChatNames.Add(chatName.ToUpper());
                ClientMonitoringVm.ChatIds.Add(tgDownloadSettings.SourceVm.Dto.Id);
            }
        }
    }

    /// <summary> Subscribe to client updates for monitoring new messages in chats </summary>
    private async Task ClientMonitoringSubscribeAsync()
    {
        if (ClientMonitoringVm.IsStartMonitoring) return;
        if (ClientForSendData is null) return;

        BusinessLogicManager.ConnectClient.ClearCaches();
        if (!BusinessLogicManager.ConnectClient.DicChatsAll.Any())
            await BusinessLogicManager.ConnectClient.CollectAllChatsAsync();

        ClientForSendData.OnUpdates -= OnClientUpdatesAsync;
        ClientForSendData.OnUpdates += OnClientUpdatesAsync;

        ClientMonitoringVm.IsStartMonitoring = true;

        TgLog.WriteLine($"  {TgLocale.MenuClientMonitoringStarted}");
    }

    /// <summary> Stop monitoring chats for new messages </summary>
    private async Task ClientMonitoringStopAsync(bool isSilent)
    {
        if (!isSilent && AskQuestionYesNoReturnNegative(TgLocale.MenuClientStopMonitoringChats)) return;
        if (!ClientMonitoringVm.IsStartMonitoring) return;

        ClientMonitoringVm.Default();

        // Unsubscribe to client updates for monitoring new messages in chats
        await ClientMonitoringUnsubscribeAsync();

        await Task.CompletedTask;
    }

    /// <summary> Unsubscribe to client updates for monitoring new messages in chats </summary>
    private async Task ClientMonitoringUnsubscribeAsync()
    {
        if (!ClientMonitoringVm.IsStartMonitoring) return;
        if (ClientForSendData is null) return;

        BusinessLogicManager.ConnectClient.ClearCaches();
        ClientForSendData.OnUpdates -= OnClientUpdatesAsync;
        ClientMonitoringVm.IsStartMonitoring = false;
        TgLog.WriteLine($"  {TgLocale.MenuClientMonitoringStopped}");

        await Task.CompletedTask;
    }

    /// <summary> Join client to chat by username or id if it is not already a member </summary>
    private async Task<bool> TryJoinChatAsClientAsync(TgDownloadSettingsViewModel tgDownloadSettings, Client client, string chatNameOrId)
    {
        try
        {
            // Get details about a public chat (even if client is not a member of that chat)
            tgDownloadSettings.SourceVm = new TgEfSourceViewModel(TgGlobalTools.Container) { Dto = new TgEfSourceDto { UserName = chatNameOrId } };
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
            TgLogUtils.WriteException(ex);
            TgLog.WriteLine($"  Error when joining the client to the chat {chatNameOrId}: {ex.Message}");
            return false;
        }
    }

    /// <summary> Check if user is a member of the chat </summary>
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
            TgLogUtils.WriteException(ex);
            TgLog.WriteLine($"{TgLocale.TgErrorCheckUserMember}: user ID {userId}, chat ID {inputChannel.channel_id}: {ex.Message}");
            return false;
        }
    }

    /// <summary> Client update event </summary>
    private async Task OnClientUpdatesAsync(UpdatesBase update)
    {
        if (!ClientMonitoringVm.IsStartMonitoring) return;
        if (update is null || update.UpdateList is null) return;

        // Check if the message is from a chat that we are monitoring
        foreach (var itemUpdate in update.UpdateList)
        {
            // Process new messages
            if (itemUpdate is TL.UpdateNewMessage updateNewMessage)
            {
                if (updateNewMessage.message is TL.Message message)
                {
                    var chatId = message.Peer.ID;

                    // If the chat is not in the monitoring list - skip it
                    if (!ClientMonitoringVm.IsSearchAtAllChats && !ClientMonitoringVm.ChatIds.Contains(chatId)) continue;

                    // Send message
                    await SendMonitoredMessageAsync(message, chatId, isMessageTextAtOneString: false);
                }
            }
        }

        await Task.CompletedTask;
    }

    /// <summary> Send message to the specified chat </summary>
    private async Task SendMonitoredMessageAsync(TL.Message message, long chatId, bool isMessageTextAtOneString)
    {
        if (ClientForSendData is not Client client) return;
        // Filter by keywords
        var text = (message.message ?? string.Empty).ToUpper();
        if (!ClientMonitoringVm.IsSkipKeywords && !ClientMonitoringVm.Keywords.Any(text.Contains)) return;

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
            if (ClientMonitoringVm.IsSendMessages)
            {
                if (message.from_id is not null)
                {
                    lastUserLink = await BusinessLogicManager.ConnectClient.GetUserLink(chatId, message.id, message.from_id);
                    if (!string.IsNullOrEmpty(lastUserLink.Item2))
                        lastMessageText += $"{Environment.NewLine}[{lastUserLink.Item1}]({lastUserLink.Item2})";
                    else if (!string.IsNullOrEmpty(lastUserLink.Item1))
                        lastMessageText += $"{Environment.NewLine}{lastUserLink.Item1}";
                }
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
                //lastMessageText += $"{Environment.NewLine}```{Environment.NewLine}{message.message}{Environment.NewLine}```";
                lastMessageText += $"{Environment.NewLine}{Environment.NewLine}{message.message}{Environment.NewLine}";
            }

            // Convert Markdown to entities
            var entities = client.MarkdownToEntities(ref lastMessageText);

            // Get the InputPeer of the user to whom we are sending the message
            if (ClientMonitoringVm.IsSendMessages)
            {
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
            }

            // Show message
            ClientMonitoringVm.LastDateTime = lastDateTime;
            ClientMonitoringVm.LastUserLink = lastUserLink.Item2 ?? "-";
            ClientMonitoringVm.LastMessageLink = lastMessageLink.Item2 ?? "-";
            ClientMonitoringVm.LastMessageText = string.IsNullOrEmpty(message.message)
                ? "-"
                : !isMessageTextAtOneString ? message.message : TgDataFormatUtils.TrimStringEndOrNewLine(message.message, 55).TrimEnd();
            ClientMonitoringVm.CatchMessages++;

            // Update table
            await ShowTableClientConSearchAsync();

            // Send message
            if (ClientMonitoringVm.IsSendMessages)
                await client.SendMessageAsync(ClientMonitoringVm.ResolvedPeer, lastMessageText, entities: entities);
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            TgLog.WriteLine($"{TgLocale.MenuClientSendingMessageError}: {ex.Message}");
        }
    }

    /// <summary> Start search for keywords in chats </summary>
    private async Task ClientSearchingStartAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuClientSearchForKeywordsInChatsStart)) return;

        await ClientMonitoringStopAsync(isSilent: true);
        if (ClientMonitoringVm.IsStartMonitoring) return;
        if (ClientMonitoringVm.IsStartSearching) return;

        // Get client connection
        ClientForSendData = await GetClientConnectionAsync(tgDownloadSettings);
        if (ClientForSendData is null)
        {
            TgLog.WriteLine(TgLocale.MenuClientConfigurationError);
            return;
        }

        // Get username for send messages
        await ClientMonitoringGetUserNameAsync();
        // Get chats
        var chatNames = ClientMonitoringGetChats();
        // Get keywords to filter messages
        GetKeywordsForMonitoring();
        // Add chats for monitoring
        await ClientMonitoringAddChatsAsync(tgDownloadSettings, chatNames);

        // Subscribe to start search for keywords in chats
        await ClientSearchForKeywordsInChatsSubscribeAsync(tgDownloadSettings);
    }

    /// <summary> Subscribe to start search for keywords in chats </summary>
    private async Task ClientSearchForKeywordsInChatsSubscribeAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (ClientMonitoringVm.IsStartSearching) return;
        if (ClientForSendData is null) return;

        BusinessLogicManager.ConnectClient.ClearCaches();
        if (!BusinessLogicManager.ConnectClient.DicChatsAll.Any())
            await BusinessLogicManager.ConnectClient.CollectAllChatsAsync();

        ClientMonitoringVm.IsStartSearching = true;
        await ClientSearchForKeywordsInChatsProcessAllChatsAsync(tgDownloadSettings);

        TgLog.WriteLine($"  {TgLocale.MenuClientSearchingStarted}");
        ClientMonitoringVm.IsStartSearching = false;
    }

    /// <summary> Process all chats for search keywords </summary>
    private async Task ClientSearchForKeywordsInChatsProcessAllChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (ClientForSendData is not Client client) return;
        if (!ClientMonitoringVm.IsStartSearching) return;

        try
        {
            // Get chats
            var chats = BusinessLogicManager.ConnectClient.DicChatsAll.Values.Where(x => !string.IsNullOrEmpty(x.MainUsername))
                .Where(chat => ClientMonitoringVm.IsSearchAtAllChats || ClientMonitoringVm.ChatNames.Contains(chat.MainUsername.ToUpper()))
                .ToList();

            foreach (var chat in chats)
            {
                if (chat is null) continue;
                // Get the last message ID in a chat 
                tgDownloadSettings.SourceVm.Dto.Count = await BusinessLogicManager.ConnectClient.GetChatLastMessageIdAsync(chat.ID);
                // Update table
                await ShowTableClientConSearchAsync();

                await RunTaskProgressAsync(tgDownloadSettings, async (tgDownloadSettings) => {
                    await BusinessLogicManager.ConnectClient.MakeFuncWithMessagesAsync(tgDownloadSettings, 
                        chat.ID, ClientSearchForKeywordsInChatsProcessAllMessagesInChatAsync);
                }, isSkipCheckTgSettings: true, isScanCount: false);
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
            TgLog.WriteLine($"{TgLocale.MenuClientSendingMessageError}: {ex.Message}");
        }
    }

    /// <summary> Process all messages in chat for search keywords </summary>
    private async Task ClientSearchForKeywordsInChatsProcessAllMessagesInChatAsync(TgDownloadSettingsViewModel tgDownloadSettings, 
        TL.ChatBase chatBase, TL.MessageBase message)
    {
        if (message is not TL.Message msg) return;

        // StatusContext
        var chatId = string.IsNullOrEmpty(chatBase.MainUsername) ? $"{chatBase.ID}" : $"{chatBase.ID} | @{chatBase.MainUsername}";
        await BusinessLogicManager.ConnectClient.UpdateStateSourceAsync(chatBase.ID, 
            tgDownloadSettings.SourceVm.Dto.FirstId, tgDownloadSettings.SourceVm.Dto.Count,
            tgDownloadSettings.SourceVm.Dto.Count <= 0
                ? $"The source {chatId}: hasn't any messages!"
                : $"The source {chatId}: read {tgDownloadSettings.SourceVm.Dto.FirstId} from {tgDownloadSettings.SourceVm.Dto.Count} messages.");

        // Send message
        await SendMonitoredMessageAsync(msg, msg.Peer.ID, isMessageTextAtOneString: true);

        tgDownloadSettings.SourceVm.Dto.FirstId = tgDownloadSettings.SourceVm.Dto.Count - message.ID + 1;
    }

    /// <summary> Stop search for keywords in chats </summary>
    private async Task ClientSearchingStopAsync(bool isSilent)
    {
        if (!isSilent && AskQuestionYesNoReturnNegative(TgLocale.MenuClientSearchForKeywordsInChatsStop)) return;
        
        if (!ClientMonitoringVm.IsStartSearching) return;
        if (ClientForSendData is null) return;

        BusinessLogicManager.ConnectClient.ClearCaches();
        ClientMonitoringVm.IsStartSearching = false;
        TgLog.WriteLine($"  {TgLocale.MenuClientSearchingStopped}");

        await Task.CompletedTask;
    }

    #endregion
}
