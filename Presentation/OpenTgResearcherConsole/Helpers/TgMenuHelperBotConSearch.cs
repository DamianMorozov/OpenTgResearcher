﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

using TL;

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
    #region Public and private fields, properties, constructor

    private List<string> BotMonitoringChatNames { get; set; } = [];
    private List<string> BotMonitoringKeywords { get; set; } = [];
    private string BotMonitoringUserName { get; set; } = string.Empty;
    private long BotMonitoringUserId { get; set; }
    public Bot? BotForSendData { get; set; }

    #endregion

    #region Public and private methods

    private TgEnumMenuBotConSearch SetMenuBotConSearch()
    {
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(
            TgLocale.MenuReturn,
            TgLocale.MenuBotSearchChat,
            TgLocale.MenuBotSearchUser
        //TgLocale.MenuBotStartMonitoringChats,
        //TgLocale.MenuBotStopMonitoringChats
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuBotSearchChat))
            return TgEnumMenuBotConSearch.Chat;
        if (prompt.Equals(TgLocale.MenuBotSearchUser))
            return TgEnumMenuBotConSearch.User;
        if (prompt.Equals(TgLocale.MenuBotStartMonitoringChats))
            return TgEnumMenuBotConSearch.StartMonitoringChats;
        if (prompt.Equals(TgLocale.MenuBotStopMonitoringChats))
            return TgEnumMenuBotConSearch.StopMonitoringChats;

        return TgEnumMenuBotConSearch.Return;
    }

    public async Task SetupBotConSearchAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        TgEnumMenuBotConSearch menu;
        do
        {
            await ShowTableBotConSearchAsync(tgDownloadSettings);

            menu = SetMenuBotConSearch();
            switch (menu)
            {
                case TgEnumMenuBotConSearch.Chat:
                    await BotSearchChatAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuBotConSearch.User:
                    await BotSearchUserAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuBotConSearch.StartMonitoringChats:
                    await BotStartMonitoringChatsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuBotConSearch.StopMonitoringChats:
                    await BotStopMonitoringChatsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuBotConSearch.Return:
                    break;
            }
        } while (menu is not TgEnumMenuBotConSearch.Return);
    }

    /// <summary> Check bot connection </summary>
    private async Task<Bot?> CheckBotConnectionAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await BotConnectAsync(tgDownloadSettings, isSilent: true);
        var isBotConnect = await BusinessLogicManager.ConnectClient.CheckBotConnectionReadyAsync();
        if (!isBotConnect) return null;

        if (BusinessLogicManager.ConnectClient.Bot is not Bot bot) return null;

        return bot;
    }

    /// <summary> Bot search chat </summary>
    private async Task BotSearchChatAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        var bot = await CheckBotConnectionAsync(tgDownloadSettings);
        if (bot is null)
        {
            TgLog.WriteLine(TgLocale.MenuBotConfigurationError);
            return;
        }

        // Get the name of the public chat to search for
        var input = AnsiConsole.Ask<string>($"  {TgLocale.MenuSetChatName}:");
        if (string.IsNullOrEmpty(input))
        {
            TgLog.WriteLine(TgLocale.MenuSetChatNameIsEmpty);
            return;
        }

        var chatDetailsDto = await BusinessLogicManager.ConnectClient.GetChatDetailsByBotAsync(input);
        PrintTableWithChatInfo(chatDetailsDto);
        TgLog.TypeAnyKeyForReturn();
    }

    /// <summary> Print table with chat info </summary>
    private void PrintTableWithChatInfo(TgChatDetailsDto chatDetailsDto)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold yellow]{TgLocale.FieldBotInfoChat}[/]")
            .AddColumn($"[bold]{TgLocale.Field}[/]")
            .AddColumn($"[bold]{TgLocale.FieldValue}[/]")
            .Expand();

        table.AddRow(TgLocale.FieldTitle, chatDetailsDto.Title);
        table.AddRow(TgLocale.FieldType, chatDetailsDto.Type);
        table.AddRow(TgLocale.FieldId, chatDetailsDto.Id);
        table.AddRow(TgLocale.FieldUserName, chatDetailsDto.UserName);
        table.AddRow(TgLocale.FieldInviteLink, chatDetailsDto.InviteLink);
        table.AddRow(TgLocale.FieldDescription, chatDetailsDto.Description);
        table.AddRow(TgLocale.FieldPermissions, chatDetailsDto.Permissions);

        if (chatDetailsDto.IsChatFull)
        {
            table.AddRow(TgLocale.FieldAbout, chatDetailsDto.About);
            table.AddRow(TgLocale.FieldParticipantsCount, chatDetailsDto.ParticipantsCount.ToString());
            table.AddRow(TgLocale.FieldOnlineCount, chatDetailsDto.OnlineCount.ToString());
            table.AddRow(TgLocale.FieldSlowMode, chatDetailsDto.SlowMode);
            table.AddRow(TgLocale.FieldAvailableReactions, chatDetailsDto.AvailableReactions);
            table.AddRow(TgLocale.FieldTtlPeriod, chatDetailsDto.TtlPeriod.ToString());
            table.AddRow(TgLocale.FieldIsActive, chatDetailsDto.IsActiveChat ? "yes" : "no");
            table.AddRow(TgLocale.FieldIsBanned, chatDetailsDto.IsBanned ? "yes" : "no");
            table.AddRow(TgLocale.FieldIsChannel, chatDetailsDto.IsChannel ? "yes" : "no");
            table.AddRow(TgLocale.FieldIsGroup, chatDetailsDto.IsGroup ? "yes" : "no");
            table.AddRow(TgLocale.FieldIsForum, chatDetailsDto.IsForum ? "yes" : "no");
        }

        AnsiConsole.Write(table);
    }

    /// <summary> Bot search user </summary>
    private async Task BotSearchUserAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        var bot = await CheckBotConnectionAsync(tgDownloadSettings);
        if (bot is null)
        {
            TgLog.WriteLine(TgLocale.MenuBotConfigurationError);
            return;
        }

        // Get the public username of the user to search for
        var input = AnsiConsole.Ask<string>($"  {TgLocale.MenuSetUserName}:");
        if (string.IsNullOrEmpty(input))
        {
            TgLog.WriteLine(TgLocale.MenuSetUserNameIsEmpty);
            return;
        }
        input = TgStringUtils.NormilizeTgName(input);

        // Get details about a user via the public username (even if not in discussion with bot)
        var inputUser = await bot.InputUser(input);
        if (inputUser is { user_id: var userId })
        {
            var userDetails = await bot.GetChat(userId);
            if (userDetails.TLInfo is null)
                TgLog.MarkupInfo(TgLocale.TgGetUserDetailsError);
            else
                PrintTableWithUserInfo(userDetails.TLInfo, userId);
            TgLog.TypeAnyKeyForReturn();
        }
    }

    /// <summary> Print table with user info </summary>
    private void PrintTableWithUserInfo(IObject tLInfo, long userId)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold yellow]{TgLocale.FieldBotInfoUser}[/]")
            .AddColumn($"[bold]{TgLocale.Field}[/]")
            .AddColumn($"[bold]{TgLocale.FieldValue}[/]")
            .Expand();

        var full = (TL.Users_UserFull)tLInfo!;
        var tlUser = full.users[userId];
        var fullUser = full.full_user;
        table.AddRow(TgLocale.FieldId, tlUser.ID.ToString());
        table.AddRow(TgLocale.FieldUserName, tlUser.username);
        table.AddRow(TgLocale.FieldIsActive, tlUser.IsActive ? $"yes" : $"no");
        table.AddRow(TgLocale.FieldIsActive, tlUser.IsBot ? $"yes" : $"no");
        table.AddRow(TgLocale.FieldLastSeenAgo, TgDtUtils.FormatLastSeenAgo(tlUser.LastSeenAgo));
        //table.AddRow(TgLocale.FieldIsBot, tlUser.flags.HasFlag(TL.User.Flags.bot) ? $"yes" : $"no");
        table.AddRow(TgLocale.FieldIsScam, tlUser.flags.HasFlag(TL.User.Flags.scam)
            ? $"is reported as scam" : $"is not reported as scam");
        table.AddRow(TgLocale.FieldIsVerified, tlUser.flags.HasFlag(TL.User.Flags.verified)
            ? $"is verified" : $"is not verified");
        table.AddRow(TgLocale.FieldIsRestricted, tlUser.flags.HasFlag(TL.User.Flags.restricted)
            ? $"is restricted: {tlUser.restriction_reason?[0].reason}" : $"is not restricted");
        if (fullUser.bot_info is { commands: { } botCommands })
        {
            table.AddRow(TgLocale.FieldCommands, $"has {botCommands.Length} bot commands:");
            foreach (var command in botCommands)
                table.AddRow(TgLocale.FieldCommands, $"    /{command.command,-20} {command.description}");
        }
        table.AddRow(TgLocale.FieldFirstName, tlUser.first_name ?? "-");
        table.AddRow(TgLocale.FieldLastName, tlUser.last_name ?? "-");
        table.AddRow(TgLocale.FieldFlags, string.Join(',', tlUser.flags, tlUser.flags2));
        table.AddRow(TgLocale.FieldPhone, tlUser.phone ?? "-");
        table.AddRow(TgLocale.FieldSendPaidMessagesStars, tlUser.send_paid_messages_stars.ToString());
        table.AddRow(TgLocale.FieldStoriesMaxId, tlUser.stories_max_id.ToString());

        AnsiConsole.Write(table);
    }

    /// <summary> Start monitoring chats for new messages </summary>
    private async Task BotStartMonitoringChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        BotMonitoringUserName = string.Empty;
        BotMonitoringUserId = 0;
        BotMonitoringChatNames.Clear();
        BotMonitoringKeywords.Clear();

        var bot = await CheckBotConnectionAsync(tgDownloadSettings);
        if (bot is null)
        {
            TgLog.WriteLine(TgLocale.MenuBotConfigurationError);
            return;
        }
        BotForSendData = bot;

        // Get username for send messages
        var userNameInput = AnsiConsole.Ask<string>($"  {TgLocale.MenuSetUserNameForSendMessages}:");
        if (string.IsNullOrEmpty(userNameInput))
        {
            TgLog.WriteLine(TgLocale.MenuSetUserNameIsEmpty);
            return;
        }
        BotMonitoringUserName = TgStringUtils.NormilizeTgName(userNameInput);
        var inputUser = await bot.InputUser(BotMonitoringUserName);
        if (inputUser is { user_id: var userId })
        {
            var userDetails = await bot.GetChat(userId);
            if (userDetails.TLInfo is null)
                TgLog.MarkupInfo(TgLocale.TgGetUserDetailsError);
            else
                BotMonitoringUserId = userId;
        }

        // Get the list of chats that the bot is a member of
        var chatNamesInput = AnsiConsole.Ask<string>($"  {TgLocale.MenuSetChatNames}:");
        if (string.IsNullOrEmpty(chatNamesInput))
        {
            TgLog.WriteLine(TgLocale.MenuSetUserNameIsEmpty);
            return;
        }
        var chatNames = TgStringUtils.NormilizeTgNames(chatNamesInput);

        // Get the list of keywords to filter messages
        var keywordsInput = AnsiConsole.Ask<string>($"  {TgLocale.MenuSetKeywordsForMessageFiltering}:");
        if (string.IsNullOrEmpty(keywordsInput))
        {
            TgLog.WriteLine(TgLocale.MenuSetKeywordsForFilterMessagesIsEmpty);
            return;
        }
        BotMonitoringKeywords = [.. keywordsInput
            .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(keyword => keyword.Trim().ToUpper())];

        // Start monitoring chats for new messages
        TgLog.MarkupInfo($"  {TgLocale.MenuBotStartMonitoringChats} {chatNames.Count} chats...");
        foreach (var chatName in chatNames)
        {
            var joined = await TryJoinChatAsBotAsync(bot, chatName);
            if (!joined)
                continue;
            BotMonitoringChatNames.Add(chatName.ToUpper());
        }

        // Subscribe
        BotForSendData.OnMessage += OnBotMessageAsync;
        BotForSendData.OnUpdate += OnBotUpdateAsync;
        TgLog.WriteLine("  Monitoring started");
        TgLog.TypeAnyKeyForReturn();

        // Unsubscribe
        BotForSendData.OnMessage -= OnBotMessageAsync;
        BotForSendData.OnUpdate -= OnBotUpdateAsync;
    }

    private async Task OnBotMessageAsync(WTelegram.Types.Message message, UpdateType type)
    {
        // Check if the message is from a chat that we are monitoring
        var chatName = message.Chat?.Username?.ToUpper() ?? "";
        if (string.IsNullOrEmpty(chatName)) return;

        if (BotMonitoringChatNames.Contains(chatName))
        {
            // Filter messages by keywords
            var text = (message.Text ?? string.Empty).ToUpper();
            if (BotMonitoringKeywords.Any(k => text.Contains(k)))
            {
                // Send a message to the specified userName
                TgLog.MarkupInfo($"[bold green]New message for {BotMonitoringUserName} in {chatName}:[/] {message.Text}");
                if (BotForSendData is Bot bot && message.Chat?.Id is long chatId)
                    await bot.SendMessage(BotMonitoringUserId, message.Text ?? string.Empty);
            }
        }

        await Task.CompletedTask;
    }

    private async Task OnBotUpdateAsync(WTelegram.Types.Update update)
    {
        var foo = update;
        // Check if the message is from a chat that we are monitoring
        //var chatName = message.Chat?.Username?.ToUpper() ?? "";
        //if (string.IsNullOrEmpty(chatName)) return;

        //if (BotMonitoringChatNames.Contains(chatName))
        //{
        //    // Filter messages by keywords
        //    var text = (message.Text ?? string.Empty).ToUpper();
        //    if (BotMonitoringKeywords.Any(k => text.Contains(k)))
        //    {
        //        // Send a message to the specified userName
        //        TgLog.MarkupInfo($"[bold green]New message for {BotMonitoringUserName} in {chatName}:[/] {message.Text}");
        //        if (BotForSendData is Bot bot && message.Chat?.Id is long chatId)
        //            await bot.SendMessage(BotMonitoringUserId, message.Text ?? string.Empty);
        //    }
        //}

        await Task.CompletedTask;
    }

    // <summary> Stop monitoring chats for new messages </summary>
    private async Task BotStopMonitoringChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        BotMonitoringUserName = string.Empty;
        BotMonitoringUserId = 0;
        BotMonitoringChatNames.Clear();
        BotMonitoringKeywords.Clear();

        // Unsubscribe
        BotForSendData?.OnMessage -= OnBotMessageAsync;
        BotForSendData?.OnUpdate -= OnBotUpdateAsync;

        await Task.CompletedTask;
    }

    /// <summary> Join bot to chat by username or id if it is not already a member </summary>
    private async Task<bool> TryJoinChatAsBotAsync(Bot bot, string chatNameOrId)
    {
        try
        {
            // Get chat details
            var chatInfo = await bot.GetChat(chatNameOrId);
            if (chatInfo is null)
            {
                TgLog.WriteLine($"{TgLocale.TgGetChatDetailsError} {chatNameOrId}");
                return false;
            }

            // Check if bot is already a member of the chat
            var isMember = false;
            var members = await bot.GetChatMemberList(chatInfo.Id);
            if (members is not null && members.Any())
            {
                isMember = members.Any(m => m.User?.Username?.Equals(BotMonitoringUserName, StringComparison.OrdinalIgnoreCase) == true);
            }
            if (!isMember)
            {
                TgLog.WriteLine($"  The bot is not a member of the chat {chatNameOrId}");
                return false;
            }

            // Try to join the chat
            //await bot.JoinChat(chatNameOrId);
            //TgLog.WriteLine($"  The bot has successfully joined the chat {chatNameOrId}");
            return true;
        }
        catch (Exception ex)
        {
            TgLog.WriteLine($"  Error when joining the bot to the chat {chatNameOrId}: {ex.Message}");
            return false;
        }
    }

    #endregion
}
