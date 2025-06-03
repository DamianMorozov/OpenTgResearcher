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
				.MoreChoicesText(TgLocale.MoveUpDown)
				.AddChoices(
					TgLocale.MenuMainReturn,
                    TgLocale.MenuBotClearConnectionData,
                    TgLocale.MenuRegisterTelegramApp,
                    TgLocale.MenuRegisterTelegramBot,
                    TgLocale.MenuBotUseBot,
                    TgLocale.MenuSetBotToken,
                    TgLocale.MenuBotConnect,
					TgLocale.MenuBotDisconnect);

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
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuBotConnect)) return;

        TgLog.WriteLine("  TG bot connect ...");
        await ConnectBotAsync(tgDownloadSettings, isSilent);
        TgLog.WriteLine("  TG bot connect success");
    }

    public async Task ConnectBotAsync(TgDownloadSettingsViewModel tgDownloadSettings, bool isSilent)
	{
		if (!isSilent)
			await ShowTableBotConnectionAsync(tgDownloadSettings);
		var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();
		var proxyResult = await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(appDto.ProxyUid);
		var proxy = proxyResult.Item;

		try
		{
			await BusinessLogicManager.ConnectClient.ConnectBotConsoleAsync();
			appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();

			if (!isSilent)
			{
				if (BusinessLogicManager.ConnectClient.ClientException.IsExist || BusinessLogicManager.ConnectClient.ProxyException.IsExist)
					TgLog.MarkupInfo(TgLocale.TgBotSetupCompleteError);
				else
                {
                    TgLog.MarkupInfo(TgLocale.TgBotSetupCompleteSuccess);
                    if (appDto.UseBot && BusinessLogicManager.ConnectClient.Bot is Bot bot)
                    {
                        await PrintBotInfoAsync(bot);
                    }
                }
				TgLog.TypeAnyKeyForReturn();
			}
		}
		catch (Exception ex)
		{
            CatchException(ex, TgLocale.TgBotSetupCompleteError);
		}
	}

    private static async Task PrintBotInfoAsync(Bot bot)
    {
        var me = await bot.GetMe();
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold yellow]{TgLocale.FieldBotInfo}[/]")
            .AddColumn($"[bold]{TgLocale.Field}[/]")
            .AddColumn($"[bold]{TgLocale.FieldValue}[/]")
            .Expand();

        table.AddRow(TgLocale.FieldUserName, me.Username ?? "-");
        table.AddRow(TgLocale.FieldId, me.Id.ToString());
        table.AddRow(TgLocale.FieldAccessHash, me.AccessHash.ToString() ?? "-");
        table.AddRow(TgLocale.FieldFirstName, me.FirstName ?? "-");
        table.AddRow(TgLocale.FieldLastName, me.LastName ?? "-");
        table.AddRow(TgLocale.FieldIsBot, me.IsBot.ToString());
        table.AddRow(TgLocale.FieldIsPremium, me.IsPremium.ToString());
        table.AddRow(TgLocale.FieldAddedToAttachmentMenu, me.AddedToAttachmentMenu.ToString());
        table.AddRow(TgLocale.FieldCanConnectToBusiness, me.CanConnectToBusiness.ToString());
        table.AddRow(TgLocale.FieldCanJoinGroups, me.CanJoinGroups.ToString());
        table.AddRow(TgLocale.FieldCanReadAllGroupMessages, me.CanReadAllGroupMessages.ToString());
        table.AddRow(TgLocale.FieldHasMainWebApp, me.HasMainWebApp.ToString());
        table.AddRow(TgLocale.FieldSupportsInlineQueries, me.SupportsInlineQueries.ToString());

        if (me.TLUser is not null)
        {
            table.AddRow("[grey]--- TLUser ---[/]", "");
            table.AddRow(TgLocale.FieldIsActive, me.TLUser.IsActive.ToString());
            table.AddRow(TgLocale.FieldLastSeenAgo, TgDtUtils.FormatLastSeenAgo(me.TLUser.LastSeenAgo));
            table.AddRow(TgLocale.FieldMainUsername, me.TLUser.MainUsername ?? "-");
            table.AddRow(TgLocale.FieldBotActiveUsers, me.TLUser.bot_active_users.ToString() ?? "-");
            table.AddRow(TgLocale.FieldBotInfoVersion, me.TLUser.bot_info_version.ToString() ?? "-");
            table.AddRow(TgLocale.FieldBotInlinePlaceholder, me.TLUser.bot_inline_placeholder ?? "-");
            table.AddRow(TgLocale.FieldFlags, string.Join(',', me.TLUser.flags, me.TLUser.flags2));
        }

        AnsiConsole.Write(table);
    }

	public async Task DisconnectBotAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await ShowTableBotConnectionAsync(tgDownloadSettings);
		await BusinessLogicManager.ConnectClient.DisconnectBotAsync();
	}

	#endregion
}