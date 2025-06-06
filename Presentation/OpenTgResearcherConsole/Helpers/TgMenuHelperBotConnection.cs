// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	private TgEnumMenuBot SetMenuBotConnection()
	{
		var selectionPrompt = new SelectionPrompt<string>()
			.Title($"  {TgLocale.MenuSwitchNumber}")
			.PageSize(Console.WindowHeight - 17)
			.MoreChoicesText(TgLocale.MoveUpDown);
		selectionPrompt.AddChoices(
				TgLocale.MenuReturn,
                TgLocale.MenuBotClearConnectionData,
                TgLocale.MenuRegisterTelegramApp,
                TgLocale.MenuRegisterTelegramBot,
                TgLocale.MenuBotUseBot,
                TgLocale.MenuSetBotToken,
                TgLocale.MenuBotConnect,
				TgLocale.MenuBotDisconnect,
                // Advanced
                TgLocale.MenuAdvanced,
                TgLocale.MenuBotAutoViewEvents
            );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuBotClearConnectionData))
			return TgEnumMenuBot.ClearBotConnectionData;
        if (prompt.Equals(TgLocale.MenuRegisterTelegramApp))
            return TgEnumMenuBot.RegisterTelegramApp;
        if (prompt.Equals(TgLocale.MenuRegisterTelegramBot))
            return TgEnumMenuBot.RegisterTelegramBot;
        if (prompt.Equals(TgLocale.MenuBotUseBot))
            return TgEnumMenuBot.UseBot;
        if (prompt.Equals(TgLocale.MenuSetBotToken))
            return TgEnumMenuBot.SetBotToken;
        if (prompt.Equals(TgLocale.MenuBotConnect))
            return TgEnumMenuBot.BotConnect;
        if (prompt.Equals(TgLocale.MenuBotDisconnect))
            return TgEnumMenuBot.BotDisconnect;
        if (prompt.Equals(TgLocale.MenuBotAutoViewEvents))
            return TgEnumMenuBot.BotAutoViewEvents;

        return TgEnumMenuBot.Return;
    }

    public async Task SetupBotConnectionAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuBot menu;
		do
		{
			await ShowTableBotConnectionAsync(tgDownloadSettings);

			menu = SetMenuBotConnection();
            switch (menu)
            {
                case TgEnumMenuBot.ClearBotConnectionData:
                    await ClearBotConnectionDataAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuBot.RegisterTelegramApp:
                    await WebSiteOpenAsync(TgConstants.LinkTelegramApps);
                    break;
                case TgEnumMenuBot.RegisterTelegramBot:
                    await WebSiteOpenAsync(TgConstants.LinkTelegramBot);
                    break;
                case TgEnumMenuBot.UseBot:
                    await UseBotAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuBot.SetBotToken:
                    await SetBotTokenAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuBot.BotConnect:
                    await AskBotConnectAsync(tgDownloadSettings, isSilent: false);
                    break;
                case TgEnumMenuBot.BotDisconnect:
                    await DisconnectBotAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuBot.BotAutoViewEvents:
                    await RunTaskStatusAsync(tgDownloadSettings, BotAutoViewEventsAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
                    break;
                case TgEnumMenuBot.Return:
                    break;
            }
        } while (menu is not TgEnumMenuBot.Return);
    }

    public async Task UseBotAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        var useBot = AskQuestionYesNoReturnPositive(TgLocale.MenuBotUseBot);
        await BusinessLogicManager.StorageManager.AppRepository.SetUseBotAsync(useBot);
    }

    public async Task ClearBotConnectionDataAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuBotClearConnectionData)) return;

        await ShowTableBotConnectionAsync(tgDownloadSettings);
        await BusinessLogicManager.StorageManager.AppRepository.DeleteAllAsync();
        await BusinessLogicManager.ConnectClient.DisconnectBotAsync();
    }

    public async Task SetBotTokenAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        var input = AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.MenuSetBotToken}:"));
        if (!string.IsNullOrEmpty(input))
        {
            await BusinessLogicManager.StorageManager.AppRepository.SetBotTokenKeyAsync(input);
        }
    }

    private async Task AskBotConnectAsync(TgDownloadSettingsViewModel tgDownloadSettings, bool isSilent)
    {
        try
        {
            TgLog.WriteLine("  TG bot connect ...");

            if (!isSilent)
            {
                if (AskQuestionYesNoReturnNegative(TgLocale.MenuBotConnect)) return;
                
                await ShowTableBotConnectionAsync(tgDownloadSettings);
            }
            //var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();
            //var proxyResult = await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(appDto.ProxyUid);
            //var proxy = proxyResult.Item;

            await BusinessLogicManager.ConnectClient.ConnectBotConsoleAsync();

            if (!isSilent)
            {
                if (BusinessLogicManager.ConnectClient.ClientException.IsExist || BusinessLogicManager.ConnectClient.ProxyException.IsExist)
                    TgLog.MarkupInfo(TgLocale.TgBotSetupCompleteError);
                else
                {
                    TgLog.MarkupInfo(TgLocale.TgBotSetupCompleteSuccess);
                    await PrintBotInfoAsync();
                }
                TgLog.TypeAnyKeyForReturn();
            }
        }
        catch (Exception ex)
        {
            CatchException(ex, TgLocale.TgBotSetupCompleteError);
        }
        finally
        {
            TgLog.WriteLine("  TG bot connect   v");
        }
    }

    private async Task PrintBotInfoAsync()
    {
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
        }

        AnsiConsole.Write(table);
    }

	public async Task DisconnectBotAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await ShowTableBotConnectionAsync(tgDownloadSettings);
		await BusinessLogicManager.ConnectClient.DisconnectBotAsync();
	}

    private async Task BotAutoViewEventsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        BusinessLogicManager.ConnectClient.IsBotUpdateStatus = true;
        await BusinessLogicManager.ConnectClient.UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceVm.Dto.FirstId,
            tgDownloadSettings.SourceVm.Dto.Count, "Bot auto view updates is started");
        TgLog.TypeAnyKeyForReturn();
        BusinessLogicManager.ConnectClient.IsBotUpdateStatus = false;
        await Task.CompletedTask;
    }

    #endregion
}