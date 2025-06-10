// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

using TL;

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
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
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuBotSearchChat))
            return TgEnumMenuBotConSearch.Chat;
        if (prompt.Equals(TgLocale.MenuBotSearchUser))
            return TgEnumMenuBotConSearch.User;

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
                case TgEnumMenuBotConSearch.Return:
                    break;
            }
        } while (menu is not TgEnumMenuBotConSearch.Return);
    }

    /// <summary> Normilize TG name </summary>
    private string NormilizeTgName(string name)
    {
        if (name.StartsWith("https://t.me/"))
            name = name.Substring(13, name.Length - 13);
        if (!name.StartsWith("@"))
            name = $"@{name}";
        return name;
    }

    /// <summary> Bot search chat </summary>
    private async Task BotSearchChatAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        // Check connect
        await BotConnectAsync(tgDownloadSettings, isSilent: true);
        var isBotConnect = await BusinessLogicManager.ConnectClient.CheckBotConnectionReadyAsync();
        if (!isBotConnect) return;

        if (BusinessLogicManager.ConnectClient.Bot is not Bot bot) return;

        var input = AnsiConsole.Ask<string>($"  {TgLocale.MenuSetChatName}:");
        if (string.IsNullOrEmpty(input))
        {
            TgLog.MarkupWarning(TgLocale.MenuSetChatNameIsEmpty);
            return;
        }

        input = NormilizeTgName(input);

        // get details about a public chat (even if bot is not a member of that chat)
        var chatDetails = await bot.GetChat(input);
        if (chatDetails is null)
            TgLog.MarkupInfo(TgLocale.TgBotGetChatDetailsError);
        else
            PrintTableWithChatInfo(chatDetails);
        TgLog.TypeAnyKeyForReturn();
    }

    /// <summary> Print table with chat info </summary>
    private void PrintTableWithChatInfo(WTelegram.Types.ChatFullInfo chatDetails)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold yellow]{TgLocale.FieldBotInfoChat}[/]")
            .AddColumn($"[bold]{TgLocale.Field}[/]")
            .AddColumn($"[bold]{TgLocale.FieldValue}[/]")
            .Expand();

        table.AddRow(TgLocale.FieldTitle, chatDetails.Title ?? "-");
        table.AddRow(TgLocale.FieldType, chatDetails.Type.ToString());
        table.AddRow(TgLocale.FieldId, TgConnectClientBase.ReduceChatId(chatDetails.Id).ToString());
        table.AddRow(TgLocale.FieldUserName, chatDetails.Username ?? "-");
        table.AddRow(TgLocale.FieldInviteLink, chatDetails.InviteLink ?? "-");
        table.AddRow(TgLocale.FieldIsForum, chatDetails.IsForum ? "yes" : "no");
        table.AddRow(TgLocale.FieldDescription, chatDetails.Description ?? "-");
        if (chatDetails.Permissions is null)
            table.AddRow(TgLocale.FieldPermissions, "");
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
            table.AddRow(TgLocale.FieldPermissions, string.Join(", ", permissions));
        }
        if (chatDetails.TLInfo is TL.Messages_ChatFull { full_chat: TL.ChannelFull channelFull })
        {
            table.AddRow(TgLocale.FieldAbout, channelFull.About);
            table.AddRow(TgLocale.FieldParticipantsCount, channelFull.participants_count.ToString());
            table.AddRow(TgLocale.FieldOnlineCount, channelFull.online_count.ToString());
            table.AddRow(TgLocale.FieldSlowmode, channelFull.slowmode_seconds > 0
                ? $"slowmode enabled: {channelFull.slowmode_seconds} seconds" : $"slowmode disbaled");
            table.AddRow(TgLocale.FieldAvailableReactions,
                channelFull.available_reactions is TL.ChatReactionsAll { flags: TL.ChatReactionsAll.Flags.allow_custom }
                ? "allows custom emojis as reactions" : "does not allow the use of custom emojis as a reaction");
            table.AddRow(TgLocale.FieldTtlPeriod, channelFull.TtlPeriod.ToString());
        }

        AnsiConsole.Write(table);
    }

    /// <summary> Bot search user </summary>
    private async Task BotSearchUserAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        // Check connect
        await BotConnectAsync(tgDownloadSettings, isSilent: true);
        var isBotConnect = await BusinessLogicManager.ConnectClient.CheckBotConnectionReadyAsync();
        if (!isBotConnect) return;

        if (BusinessLogicManager.ConnectClient.Bot is not Bot bot) return;

        var input = AnsiConsole.Ask<string>($"  {TgLocale.MenuSetUserName}:");
        if (string.IsNullOrEmpty(input))
        {
            TgLog.MarkupWarning(TgLocale.MenuSetUserNameIsEmpty);
            return;
        }

        input = NormilizeTgName(input);

        // get details about a user via the public username (even if not in discussion with bot)
        if (await bot.InputUser(input) is { user_id: var userId })
        {
            var userDetails = await bot.GetChat(userId);
            if (userDetails.TLInfo is null)
                TgLog.MarkupInfo(TgLocale.TgBotGetUserDetailsError);
            else
                PrintTableWithUserInfo(userDetails.TLInfo, userId);
            TgLog.TypeAnyKeyForReturn();
        }
    }

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

    #endregion
}
