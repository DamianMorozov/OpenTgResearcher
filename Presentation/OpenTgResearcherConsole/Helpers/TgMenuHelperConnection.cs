// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	private TgEnumMenuClient SetMenuConnection()
	{
		var selectionPrompt = new SelectionPrompt<string>()
				.Title($"  {TgLocale.MenuSwitchNumber}")
				.PageSize(Console.WindowHeight - 17)
				.MoreChoicesText(TgLocale.MoveUpDown)
				.AddChoices(
					TgLocale.MenuMainReturn,
					TgLocale.MenuClearConnectionData,
					TgLocale.MenuRegisterTelegramApp,
					TgLocale.MenuSetProxy,
					TgLocale.MenuClientConnect,
					TgLocale.MenuClientDisconnect);
        // Check paid license
        if (LicenseService.CurrentLicense.CheckPaidLicense())
		{
			selectionPrompt.AddChoice(TgLocale.TgClientUseBot);
			selectionPrompt.AddChoice(TgLocale.TgClientBotToken);
		}

        var prompt = AnsiConsole.Prompt(selectionPrompt);
		if (prompt.Equals(TgLocale.MenuClearConnectionData))
			return TgEnumMenuClient.ClearConnectionData;
		if (prompt.Equals(TgLocale.MenuRegisterTelegramApp))
			return TgEnumMenuClient.RegisterTelegramApp;
		if (prompt.Equals(TgLocale.MenuSetProxy))
			return TgEnumMenuClient.SetProxy;
		if (prompt.Equals(TgLocale.MenuClientConnect))
			return TgEnumMenuClient.Connect;
		if (prompt.Equals(TgLocale.MenuClientDisconnect))
			return TgEnumMenuClient.Disconnect;

        // Check paid license
        if (LicenseService.CurrentLicense.CheckPaidLicense())
		{
			if (prompt.Equals(TgLocale.TgClientUseBot))
				return TgEnumMenuClient.UseBot;
			if (prompt.Equals(TgLocale.TgClientBotToken))
				return TgEnumMenuClient.BotToken;
		}

        return TgEnumMenuClient.Return;
    }

    public async Task SetupConnectionAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuClient menu;
		do
		{
			await ShowTableConnectionAsync(tgDownloadSettings);
			menu = SetMenuConnection();
			switch (menu)
			{
				case TgEnumMenuClient.ClearConnectionData:
					await ClientClearAsync(tgDownloadSettings);
					break;
				case TgEnumMenuClient.RegisterTelegramApp:
					await WebSiteOpenAsync(TgConstants.LinkTelegramApps);
					break;
				case TgEnumMenuClient.SetProxy:
					await SetupClientProxyAsync();
					await AskClientConnectAsync(tgDownloadSettings);
					break;
				case TgEnumMenuClient.Connect:
					TgLog.WriteLine("TG client connect ...");
					await ConnectClientAsync(tgDownloadSettings, isSilent: false);
					TgLog.WriteLine("TG client connect success");
					break;
				case TgEnumMenuClient.Disconnect:
					await DisconnectClientAsync(tgDownloadSettings);
					break;
                case TgEnumMenuClient.UseBot:
                    await ClientUseBotAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.BotToken:
                    await ClientBotTokenAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.Return:
					break;
			}
		} while (menu is not TgEnumMenuClient.Return);
	}

	private async Task<TgEfProxyEntity> AddOrUpdateProxyAsync()
	{
		var prompt = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
				.Title($"  {TgLocale.MenuSwitchNumber}")
				.PageSize(Console.WindowHeight - 17)
				.MoreChoicesText(TgLocale.MoveUpDown)
				.AddChoices(
					nameof(TgEnumProxyType.None),
					nameof(TgEnumProxyType.Http),
					nameof(TgEnumProxyType.Socks),
					nameof(TgEnumProxyType.MtProto)));
		var proxy = GetProxyFromPrompt(prompt);
		if (!Equals(proxy.Type, TgEnumProxyType.None))
		{
			proxy.HostName = AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TypeTgProxyHostName}:"));
			proxy.Port = AnsiConsole.Ask<ushort>(TgLog.GetLineStampInfo($"{TgLocale.TypeTgProxyPort}:"));
		}
		var storageResult = await ProxyRepository.GetAsync(
			new TgEfProxyEntity { Type = proxy.Type, HostName = proxy.HostName, Port = proxy.Port }, isReadOnly: false);
		if (!storageResult.IsExists)
			await ProxyRepository.SaveAsync(proxy);
		proxy = (await ProxyRepository.GetAsync(
			new TgEfProxyEntity { Type = proxy.Type, HostName = proxy.HostName, Port = proxy.Port }, isReadOnly: false)).Item;

		var app = (await AppRepository.GetCurrentAppAsync(isReadOnly: false)).Item;
		app.ProxyUid = proxy.Uid;
		await AppRepository.SaveAsync(app);

		return proxy;
	}

	private static TgEfProxyEntity GetProxyFromPrompt(string prompt) => new()
	{
		Type = prompt switch
		{
			nameof(TgEnumProxyType.Http) => TgEnumProxyType.Http,
			nameof(TgEnumProxyType.Socks) => TgEnumProxyType.Socks,
			nameof(TgEnumProxyType.MtProto) => TgEnumProxyType.MtProto,
			_ => TgEnumProxyType.None
		},
	};

	private async Task SetupClientProxyAsync()
	{
		var proxy = await AddOrUpdateProxyAsync();

		if (proxy.Type == TgEnumProxyType.MtProto)
		{
			var prompt = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title($"  {TgLocale.MenuSwitchNumber}")
					.PageSize(Console.WindowHeight - 17)
					.MoreChoicesText(TgLocale.MoveUpDown)
					.AddChoices("Use secret", "Do not use secret"));
			var isSecret = prompt switch
			{
				"Use secret" => true,
				_ => false
			};
			if (isSecret)
				proxy.Secret = AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TypeTgProxySecret}:"));
		}
        await ProxyRepository.SaveAsync(proxy);
		//SetupClientProxyCore();
	}

	private string? ConfigClientConsole(string what)
	{
		var appNew = AppRepository.GetNewItem();
		var app = AppRepository.GetCurrentApp(isReadOnly: false).Item;
		switch (what)
		{
			case "api_hash":
				var apiHash = !Equals(app.ApiHash, appNew.ApiHash)
					? TgDataFormatUtils.ParseGuidToString(app.ApiHash)
					: TgDataFormatUtils.ParseGuidToString(TgDataFormatUtils.ParseStringToGuid(
						AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TgSetupApiHash}:"))));
				if (app.ApiHash != TgDataFormatUtils.ParseStringToGuid(apiHash))
				{
					app.ApiHash = TgDataFormatUtils.ParseStringToGuid(apiHash);
					AppRepository.Save(app);
				}
				return apiHash;
			case "api_id":
				var apiId = !Equals(app.ApiId, appNew.ApiId)
					? app.ApiId.ToString()
					: AnsiConsole.Ask<int>(TgLog.GetLineStampInfo($"{TgLocale.TgSetupAppId}:"))
					.ToString();
				if (app.ApiId != int.Parse(apiId))
				{
					app.ApiId = int.Parse(apiId);
					AppRepository.Save(app);
				}
				return apiId;
			case "phone_number":
				var phoneNumber = !Equals(app.PhoneNumber, appNew.PhoneNumber)
					? app.PhoneNumber
					: AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TgSetupPhone}:"));
				if (app.PhoneNumber != phoneNumber)
				{
					app.PhoneNumber = phoneNumber;
					AppRepository.Save(app);
				}
				return phoneNumber;
			case "verification_code":
				return AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TgVerificationCode}:"));
			case "notifications":
				return AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TgSetupNotifications}:"));
			case "first_name":
				return AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TgSetupFirstName}:"));
			case "last_name":
				return AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TgSetupLastName}:"));
			case "session_pathname":
				if (string.IsNullOrEmpty(TgAppSettings.AppXml.XmlFileSession))
					TgAppSettings.AppXml.XmlFileSession = TgFileUtils.FileTgSession;
				var sessionPath = Path.Combine(Directory.GetCurrentDirectory(), TgAppSettings.AppXml.XmlFileSession);
				return sessionPath;
			case "password":
				return AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TgSetupPassword}:"));
			//case "session_key":
			//case "server_address":
			//case "device_model":
			//case "system_version":
			//case "app_version":
			//case "system_lang_code":
			//case "lang_pack":
			//case "lang_code":
			//case "init_params":
			default:
				return null;
		}
	}

	private async Task AskClientConnectAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuClientConnect)) return;
		
		await ConnectClientAsync(tgDownloadSettings, isSilent: false);
	}

	public async Task ConnectClientAsync(TgDownloadSettingsViewModel tgDownloadSettings, bool isSilent)
	{
		if (!isSilent)
			await ShowTableConnectionAsync(tgDownloadSettings);
		var app = (await AppRepository.GetCurrentAppAsync()).Item;
		var proxyResult = await ProxyRepository.GetCurrentProxyAsync(app.ProxyUid);
		var proxy = proxyResult.Item;
		await TgGlobalTools.ConnectClient.ConnectSessionConsoleAsync(ConfigClientConsole, proxy);
		if (!isSilent)
		{
			if (TgGlobalTools.ConnectClient.ClientException.IsExist || TgGlobalTools.ConnectClient.ProxyException.IsExist)
				TgLog.MarkupInfo(TgLocale.TgClientSetupCompleteError);
			else
				TgLog.MarkupInfo(TgLocale.TgClientSetupCompleteSuccess);
		}
		if (!isSilent)
		    Console.ReadKey();
	}

	public async Task DisconnectClientAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await ShowTableConnectionAsync(tgDownloadSettings);
		await TgGlobalTools.ConnectClient.DisconnectAsync();
	}

	public async Task ClientClearAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuClearConnectionData)) return;

		await ShowTableConnectionAsync(tgDownloadSettings);
		await AppRepository.DeleteAllAsync();
		await TgGlobalTools.ConnectClient.DisconnectAsync();
	}

	public async Task ClientUseBotAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		// Check paid license
		if (!LicenseService.CurrentLicense.CheckPaidLicense()) return;

        var useBot = AskQuestionYesNoReturnPositive(TgLocale.TgClientUseBot);

		var storageResult = await AppRepository.GetCurrentAppAsync();
		if (storageResult.IsExists)
		{
			storageResult.Item.UseBot = useBot;
			await AppRepository.SaveAsync(storageResult.Item);
		}
	}

	public async Task ClientBotTokenAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
        // Check paid license
        if (!LicenseService.CurrentLicense.CheckPaidLicense()) return;

        var input = AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TgClientBotToken}:"));
		if (!string.IsNullOrEmpty(input))
		{
            var storageResult = await AppRepository.GetCurrentAppAsync();
            if (storageResult.IsExists)
            {
                storageResult.Item.BotTokenKey = input;
                await AppRepository.SaveAsync(storageResult.Item);
            }
        }
    }

    #endregion
}