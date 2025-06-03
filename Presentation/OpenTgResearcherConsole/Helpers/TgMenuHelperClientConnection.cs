// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
	#region Public and private methods

	private TgEnumMenuClient SetMenuClientConnection()
	{
		var selectionPrompt = new SelectionPrompt<string>()
				.Title($"  {TgLocale.MenuSwitchNumber}")
				.PageSize(Console.WindowHeight - 17)
				.MoreChoicesText(TgLocale.MoveUpDown)
				.AddChoices(
					TgLocale.MenuMainReturn,
                    TgLocale.MenuClientClearConnectionData,
                    TgLocale.MenuRegisterTelegramApp,
                    TgLocale.MenuClientUseClient,
					TgLocale.MenuClientSetProxy,
					TgLocale.MenuClientConnect,
					TgLocale.MenuClientDisconnect);

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuClientClearConnectionData))
			return TgEnumMenuClient.ClearClientConnectionData;
        if (prompt.Equals(TgLocale.MenuRegisterTelegramApp))
            return TgEnumMenuClient.RegisterTelegramApp;
        if (prompt.Equals(TgLocale.MenuClientUseClient))
            return TgEnumMenuClient.UseClient;
		if (prompt.Equals(TgLocale.MenuClientSetProxy))
			return TgEnumMenuClient.ClientSetProxy;
		if (prompt.Equals(TgLocale.MenuClientConnect))
			return TgEnumMenuClient.ClientConnect;
		if (prompt.Equals(TgLocale.MenuClientDisconnect))
			return TgEnumMenuClient.ClientDisconnect;

        return TgEnumMenuClient.Return;
    }

    public async Task SetupClientConnectionAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		TgEnumMenuClient menu;
		do
		{
			await ShowTableClientConnectionAsync(tgDownloadSettings);
			menu = SetMenuClientConnection();
			switch (menu)
			{
                case TgEnumMenuClient.ClearClientConnectionData:
					await ClearClientConnectionDataAsync(tgDownloadSettings);
					break;
                case TgEnumMenuClient.RegisterTelegramApp:
                    await WebSiteOpenAsync(TgConstants.LinkTelegramApps);
                    break;
                case TgEnumMenuClient.UseClient:
                    await UseClientAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.ClientSetProxy:
					await SetupClientProxyAsync();
					await AskClientConnectAsync(tgDownloadSettings, isSilent: false);
                    break;
				case TgEnumMenuClient.ClientConnect:
                    await AskClientConnectAsync(tgDownloadSettings, isSilent: false);
					break;
				case TgEnumMenuClient.ClientDisconnect:
					await DisconnectClientAsync(tgDownloadSettings);
					break;
                case TgEnumMenuClient.Return:
					break;
			}
		} while (menu is not TgEnumMenuClient.Return);
	}

    public async Task UseClientAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        var useClient = AskQuestionYesNoReturnPositive(TgLocale.MenuClientUseClient);
        await BusinessLogicManager.StorageManager.AppRepository.SetUseClientAsync(useClient);
    }

    public async Task ClearClientConnectionDataAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuClientClearConnectionData)) return;

        await ShowTableClientConnectionAsync(tgDownloadSettings);
        await BusinessLogicManager.StorageManager.AppRepository.DeleteAllAsync();
        await BusinessLogicManager.ConnectClient.DisconnectClientAsync();
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
		var storageResult = await BusinessLogicManager.StorageManager.ProxyRepository.GetAsync(
			new TgEfProxyEntity { Type = proxy.Type, HostName = proxy.HostName, Port = proxy.Port }, isReadOnly: false);
		if (!storageResult.IsExists)
			await BusinessLogicManager.StorageManager.ProxyRepository.SaveAsync(proxy);
		proxy = (await BusinessLogicManager.StorageManager.ProxyRepository.GetAsync(
			new TgEfProxyEntity { Type = proxy.Type, HostName = proxy.HostName, Port = proxy.Port }, isReadOnly: false)).Item;

		var appEntity = (await BusinessLogicManager.StorageManager.AppRepository.GetCurrentAppAsync(isReadOnly: false)).Item;
		appEntity.ProxyUid = proxy.Uid;
		await BusinessLogicManager.StorageManager.AppRepository.SaveAsync(appEntity);

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
        await BusinessLogicManager.StorageManager.ProxyRepository.SaveAsync(proxy);
		//SetupClientProxyCore();
	}

	private string? ConfigClientConsole(string what)
	{
		var appNew = BusinessLogicManager.StorageManager.AppRepository.GetNewItem();
		var app = BusinessLogicManager.StorageManager.AppRepository.GetCurrentApp(isReadOnly: false).Item;
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
					BusinessLogicManager.StorageManager.AppRepository.Save(app);
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
					BusinessLogicManager.StorageManager.AppRepository.Save(app);
				}
				return apiId;
			case "phone_number":
				var phoneNumber = !Equals(app.PhoneNumber, appNew.PhoneNumber)
					? app.PhoneNumber
					: AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TgSetupPhone}:"));
				if (app.PhoneNumber != phoneNumber)
				{
					app.PhoneNumber = phoneNumber;
					BusinessLogicManager.StorageManager.AppRepository.Save(app);
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

	private async Task AskClientConnectAsync(TgDownloadSettingsViewModel tgDownloadSettings, bool isSilent)
	{
		if (AskQuestionYesNoReturnNegative(TgLocale.MenuClientConnect)) return;

        TgLog.WriteLine("  TG client connect ...");
        await ConnectClientAsync(tgDownloadSettings, isSilent);
        TgLog.WriteLine("  TG client connect success");
	}

	public async Task ConnectClientAsync(TgDownloadSettingsViewModel tgDownloadSettings, bool isSilent)
	{
		if (!isSilent)
			await ShowTableClientConnectionAsync(tgDownloadSettings);
		var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();
		var proxyResult = await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(appDto.ProxyUid);
		var proxy = proxyResult.Item;
		await BusinessLogicManager.ConnectClient.ConnectClientConsoleAsync(ConfigClientConsole, proxy);
		if (!isSilent)
		{
			if (BusinessLogicManager.ConnectClient.ClientException.IsExist || BusinessLogicManager.ConnectClient.ProxyException.IsExist)
				TgLog.MarkupInfo(TgLocale.TgClientSetupCompleteError);
			else
				TgLog.MarkupInfo(TgLocale.TgClientSetupCompleteSuccess);
            TgLog.TypeAnyKeyForReturn();
        }
	}

    public async Task DisconnectClientAsync(TgDownloadSettingsViewModel tgDownloadSettings)
	{
		await ShowTableClientConnectionAsync(tgDownloadSettings);
		await BusinessLogicManager.ConnectClient.DisconnectClientAsync();
	}

    #endregion
}