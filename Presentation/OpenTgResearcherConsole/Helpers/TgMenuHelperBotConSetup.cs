namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
    #region Methods

    private TgEnumMenuBotConSetup SetMenuBotConSetup()
    {
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"  {TgLocale.MenuSwitchNumber}")
            .PageSize(Console.WindowHeight - 17)
            .MoreChoicesText(TgLocale.MoveUpDown);
        selectionPrompt.AddChoices(
            TgLocale.MenuReturn,
            TgLocale.MenuRegisterTelegramApp,
            TgLocale.MenuRegisterTelegramBot,
            TgLocale.MenuBotUseBot,
            TgLocale.MenuSetBotToken,
            TgLocale.MenuBotConnect,
            TgLocale.MenuBotDisconnect
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuRegisterTelegramApp))
            return TgEnumMenuBotConSetup.RegisterTelegramApp;
        if (prompt.Equals(TgLocale.MenuRegisterTelegramBot))
            return TgEnumMenuBotConSetup.RegisterTelegramBot;
        if (prompt.Equals(TgLocale.MenuBotUseBot))
            return TgEnumMenuBotConSetup.UseBot;
        if (prompt.Equals(TgLocale.MenuSetBotToken))
            return TgEnumMenuBotConSetup.SetBotToken;
        if (prompt.Equals(TgLocale.MenuBotConnect))
            return TgEnumMenuBotConSetup.BotConnect;
        if (prompt.Equals(TgLocale.MenuBotDisconnect))
            return TgEnumMenuBotConSetup.BotDisconnect;

        return TgEnumMenuBotConSetup.Return;
    }

    public async Task SetupBotConSetupAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        TgEnumMenuBotConSetup menu;
        do
        {
            await ShowTableBotConSetupAsync(tgDownloadSettings);

            menu = SetMenuBotConSetup();
            switch (menu)
            {
                case TgEnumMenuBotConSetup.RegisterTelegramApp:
                    await WebSiteOpenAsync(TgConstants.LinkTelegramApps);
                    break;
                case TgEnumMenuBotConSetup.RegisterTelegramBot:
                    await WebSiteOpenAsync(TgConstants.LinkTelegramBot);
                    break;
                case TgEnumMenuBotConSetup.UseBot:
                    await SetUseBotAsync(tgDownloadSettings, isSilent: false);
                    break;
                case TgEnumMenuBotConSetup.SetBotToken:
                    await SetBotTokenAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuBotConSetup.BotConnect:
                    await BotConnectAsync(tgDownloadSettings, isSilent: false);
                    break;
                case TgEnumMenuBotConSetup.BotDisconnect:
                    await BotDisconnectAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuBotConSetup.Return:
                    break;
            }
        } while (menu is not TgEnumMenuBotConSetup.Return);
    }

    public async Task<bool> SetUseBotAsync(TgDownloadSettingsViewModel tgDownloadSettings, bool isSilent)
    {
        var useBot = isSilent || AskQuestionYesNoReturnPositive(TgLocale.MenuBotUseBot);
        return await BusinessLogicManager.StorageManager.AppRepository.SetUseBotAsync(useBot);
    }

    public async Task SetBotTokenAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        var input = AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.MenuSetBotToken}:"));
        if (!string.IsNullOrEmpty(input))
        {
            await BusinessLogicManager.StorageManager.AppRepository.SetBotTokenKeyAsync(input);
        }
    }

    private async Task BotConnectAsync(TgDownloadSettingsViewModel tgDownloadSettings, bool isSilent)
    {
        try
        {
            // Check connect
            var isClientConnect = await BusinessLogicManager.ConnectClient.CheckBotConnectionReadyAsync();

            // Switch flag
            var isUseBot = await SetUseBotAsync(tgDownloadSettings, isSilent);
            await SetBotTokenAsync(tgDownloadSettings);

            if (!isSilent)
            {
                // Question
                if (AskQuestionYesNoReturnNegative(TgLocale.MenuBotConnect)) return;
                // Connect
                TgLog.WriteLine("  TG bot connect ...");
            }

            await BusinessLogicManager.ConnectClient.ConnectBotConsoleAsync();
            
            if (!isSilent)
            {
                if (BusinessLogicManager.ConnectClient.ClientException.IsExist || BusinessLogicManager.ConnectClient.ProxyException.IsExist)
                    TgLog.WriteLine($"  {TgLocale.TgBotSetupCompleteError}");
                else
                    await PrintTableWithBotInfoAsync();
                TgLog.TypeAnyKeyForReturn();
                TgLog.WriteLine("  TG bot connect   v");
            }
        }
        catch (Exception ex)
        {
            CatchException(ex, TgLocale.TgBotSetupCompleteError);
        }
    }

    /// <summary> Print table with bot info </summary>
    private async Task PrintTableWithBotInfoAsync()
    {
        TgLog.WriteLine($"  {TgLocale.TgBotSetupCompleteSuccess}");
        
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold yellow]{TgLocale.FieldBotInfo}[/]")
            .AddColumn($"[bold]{TgLocale.Field}[/]")
            .AddColumn($"[bold]{TgLocale.FieldValue}[/]")
            .Expand();

        // Ensure that the BotInfoDto is populated before printing
        if (BusinessLogicManager.ConnectClient.BotInfoDto is null)
        {
            var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();
            if (appDto.UseBot && BusinessLogicManager.ConnectClient.Bot is Bot bot)
            {
                var me = await bot.GetMe();
                var botInfoNew = new TgBotInfoDto()
                {
                    Username = me.Username ?? "-",
                    Id = me.Id.ToString(),
                    AccessHash = me.AccessHash.ToString(),
                    FirstName = me.FirstName ?? "-",
                    LastName = me.LastName ?? "-",
                    IsBot = me.IsBot.ToString(),
                    IsPremium = me.IsPremium.ToString(),
                    AddedToAttachmentMenu = me.AddedToAttachmentMenu.ToString(),
                    CanConnectToBusiness = me.CanConnectToBusiness.ToString(),
                    CanJoinGroups = me.CanJoinGroups.ToString(),
                    CanReadAllGroupMessages = me.CanReadAllGroupMessages.ToString(),
                    HasMainWebApp = me.HasMainWebApp.ToString(),
                    SupportsInlineQueries = me.SupportsInlineQueries.ToString()
                };

                // TLUser
                if (me.TLUser is not null)
                {
                    botInfoNew.IsTlUser = true;
                    botInfoNew.IsActive = me.TLUser.IsActive.ToString();
                    botInfoNew.LastSeenAgo = TgDtUtils.FormatLastSeenAgo(me.TLUser.LastSeenAgo);
                    botInfoNew.MainUsername = me.TLUser.MainUsername ?? "-";
                    botInfoNew.BotActiveUsers = me.TLUser.bot_active_users.ToString();
                    botInfoNew.BotInfoVersion = me.TLUser.bot_info_version.ToString();
                    botInfoNew.BotInlinePlaceholder = me.TLUser.bot_inline_placeholder ?? "-";
                    botInfoNew.Flags = string.Join(',', me.TLUser.flags, me.TLUser.flags2);
                }

                // Client
                botInfoNew.FloodRetryThreshold = bot.Client.FloodRetryThreshold;
                botInfoNew.IsMainDC = bot.Client.IsMainDC;
                botInfoNew.MTProxyUrl = bot.Client.MTProxyUrl ?? "-";
                botInfoNew.MaxAutoReconnects = bot.Client.MaxAutoReconnects;
                botInfoNew.MaxCodePwdAttempts = bot.Client.MaxCodePwdAttempts;
                botInfoNew.PingInterval = bot.Client.PingInterval;

                BusinessLogicManager.ConnectClient.SetBotInfoDto(botInfoNew);
            }
        }

        // Print the bot information table
        if (BusinessLogicManager.ConnectClient.BotInfoDto is { } botInfoDto)
        {
            table.AddRow(TgLocale.FieldUserName, botInfoDto.Username);
            table.AddRow(TgLocale.FieldId, botInfoDto.Id);
            table.AddRow(TgLocale.FieldAccessHash, botInfoDto.AccessHash);
            table.AddRow(TgLocale.FieldFirstName, botInfoDto.FirstName);
            table.AddRow(TgLocale.FieldLastName, botInfoDto.LastName);
            table.AddRow(TgLocale.FieldIsBot, botInfoDto.IsBot);
            table.AddRow(TgLocale.FieldIsPremium, botInfoDto.IsPremium);
            table.AddRow(TgLocale.FieldAddedToAttachmentMenu, botInfoDto.AddedToAttachmentMenu);
            table.AddRow(TgLocale.FieldCanConnectToBusiness, botInfoDto.CanConnectToBusiness);
            table.AddRow(TgLocale.FieldCanJoinGroups, botInfoDto.CanJoinGroups);
            table.AddRow(TgLocale.FieldCanReadAllGroupMessages, botInfoDto.CanReadAllGroupMessages);
            table.AddRow(TgLocale.FieldHasMainWebApp, botInfoDto.HasMainWebApp);
            table.AddRow(TgLocale.FieldSupportsInlineQueries, botInfoDto.SupportsInlineQueries);

            if (botInfoDto.IsTlUser)
            {
                table.AddRow("[grey]--- TLUser ---[/]", "");
                table.AddRow(TgLocale.FieldIsActive, botInfoDto.IsActive);
                table.AddRow(TgLocale.FieldLastSeenAgo, botInfoDto.LastSeenAgo);
                table.AddRow(TgLocale.FieldMainUsername, botInfoDto.MainUsername);
                table.AddRow(TgLocale.FieldBotActiveUsers, botInfoDto.BotActiveUsers);
                table.AddRow(TgLocale.FieldBotInfoVersion, botInfoDto.BotInfoVersion);
                table.AddRow(TgLocale.FieldBotInlinePlaceholder, botInfoDto.BotInlinePlaceholder);
                table.AddRow(TgLocale.FieldFlags, botInfoDto.Flags);
            }

            table.AddRow("[grey]--- Client ---[/]", "");
            table.AddRow(TgLocale.FieldFloodRetryThreshold, botInfoDto.FloodRetryThreshold.ToString());
            table.AddRow(TgLocale.FieldIsMainDC, botInfoDto.IsMainDC ? "yes" : "no");
            table.AddRow(TgLocale.FieldMTProxyUrl, botInfoDto.MTProxyUrl);
            table.AddRow(TgLocale.FieldMaxAutoReconnects, botInfoDto.MaxAutoReconnects.ToString());
            table.AddRow(TgLocale.FieldFloodRetryThreshold, botInfoDto.MaxCodePwdAttempts.ToString());
            table.AddRow(TgLocale.FieldPingInterval, botInfoDto.PingInterval.ToString());
        }

        AnsiConsole.Write(table);
    }

    public async Task BotDisconnectAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        // Check connect
        var isBotConnect = await BusinessLogicManager.ConnectClient.CheckBotConnectionReadyAsync();
        if (!isBotConnect) return;

        // Question
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuBotDisconnect)) return;

        // Disconnect
        await BusinessLogicManager.ConnectClient.DisconnectBotAsync();
    }

    #endregion
}
