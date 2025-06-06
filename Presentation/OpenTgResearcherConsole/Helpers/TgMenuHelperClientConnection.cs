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
			.MoreChoicesText(TgLocale.MoveUpDown);
		selectionPrompt.AddChoices(
			TgLocale.MenuReturn,
            TgLocale.MenuClientClearConnectionData,
            TgLocale.MenuRegisterTelegramApp,
            TgLocale.MenuClientUseClient,
			TgLocale.MenuClientSetProxy,
			TgLocale.MenuClientConnect,
			TgLocale.MenuClientDisconnect,
            // Advanced
            TgLocale.MenuAdvanced,
            TgLocale.MenuClientAdvancedStartAutoDownload,
            TgLocale.MenuClientAdvancedAutoViewEvents,
            TgLocale.MenuClientAdvancedSearchContacts,
            TgLocale.MenuClientAdvancedSearchChats,
            TgLocale.MenuClientAdvancedSearchDialogs,
            TgLocale.MenuClientAdvancedSearchStories,
            TgLocale.MenuClientAdvancedMarkAllMessagesAsRead,
            TgLocale.MenuStorageViewChats,
            // Download
            TgLocale.MenuDownload,
            TgLocale.MenuDownloadSetSource,
            TgLocale.MenuDownloadSetFolder,
            TgLocale.MenuDownloadSetSourceFirstIdAuto,
            TgLocale.MenuDownloadSetSourceFirstIdManual,
            TgLocale.MenuDownloadSetIsRewriteFiles,
            TgLocale.MenuDownloadSetIsSaveMessages,
            TgLocale.MenuDownloadSetIsRewriteMessages,
            TgLocale.MenuDownloadSetIsAddMessageId,
            TgLocale.MenuDownloadSetIsAutoUpdate,
            TgLocale.MenuDownloadSetIsCreatingSubdirectories,
            TgLocale.MenuDownloadSetIsFileNamingByMessage
        );
        // Check paid license
        if (BusinessLogicManager.LicenseService.CurrentLicense.CheckPaidLicense())
            selectionPrompt.AddChoice(TgLocale.MenuDownloadSetCountThreadsByPaidLicense);
        else
            selectionPrompt.AddChoice(TgLocale.MenuDownloadSetCountThreadsByFreeLicense);
        selectionPrompt.AddChoices(
            TgLocale.MenuSaveSettings, 
            TgLocale.MenuManualDownload);

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

        // Advanced
        if (prompt.Equals(TgLocale.MenuClientAdvancedStartAutoDownload))
            return TgEnumMenuClient.AdvancedStartAutoDownload;
        if (prompt.Equals(TgLocale.MenuClientAdvancedAutoViewEvents))
            return TgEnumMenuClient.AdvancedAutoViewEvents;
        if (prompt.Equals(TgLocale.MenuClientAdvancedSearchChats))
            return TgEnumMenuClient.AdvancedSearchChats;
        if (prompt.Equals(TgLocale.MenuClientAdvancedSearchDialogs))
            return TgEnumMenuClient.AdvancedSearchDialogs;
        if (prompt.Equals(TgLocale.MenuClientAdvancedSearchContacts))
            return TgEnumMenuClient.AdvancedSearchContacts;
        if (prompt.Equals(TgLocale.MenuClientAdvancedSearchStories))
            return TgEnumMenuClient.AdvancedSearchStories;
        if (prompt.Equals(TgLocale.MenuClientAdvancedMarkAllMessagesAsRead))
            return TgEnumMenuClient.AdvancedMarkAllMessagesAsRead;
        if (prompt.Equals(TgLocale.MenuStorageViewChats))
            return TgEnumMenuClient.AdvancedViewChats;

        // Download
        if (prompt.Equals(TgLocale.MenuDownloadSetSource))
            return TgEnumMenuClient.DownloadSetSource;
        if (prompt.Equals(TgLocale.MenuDownloadSetFolder))
            return TgEnumMenuClient.DownloadSetDestDirectory;
        if (prompt.Equals(TgLocale.MenuDownloadSetSourceFirstIdAuto))
            return TgEnumMenuClient.DownloadSetSourceFirstIdAuto;
        if (prompt.Equals(TgLocale.MenuDownloadSetSourceFirstIdManual))
            return TgEnumMenuClient.DownloadSetSourceFirstIdManual;
        if (prompt.Equals(TgLocale.MenuDownloadSetIsSaveMessages))
            return TgEnumMenuClient.DownloadSetIsSaveMessages;
        if (prompt.Equals(TgLocale.MenuDownloadSetIsRewriteFiles))
            return TgEnumMenuClient.DownloadSetIsRewriteFiles;
        if (prompt.Equals(TgLocale.MenuDownloadSetIsRewriteMessages))
            return TgEnumMenuClient.DownloadSetIsRewriteMessages;
        if (prompt.Equals(TgLocale.MenuDownloadSetIsAddMessageId))
            return TgEnumMenuClient.DownloadSetIsAddMessageId;
        if (prompt.Equals(TgLocale.MenuDownloadSetIsAutoUpdate))
            return TgEnumMenuClient.DownloadSetIsAutoUpdate;
        if (prompt.Equals(TgLocale.MenuDownloadSetIsCreatingSubdirectories))
            return TgEnumMenuClient.DownloadSetIsCreatingSubdirectories;
        if (prompt.Equals(TgLocale.MenuDownloadSetIsFileNamingByMessage))
            return TgEnumMenuClient.DownloadSetIsFileNamingByMessage;
        if (prompt.Equals(TgLocale.MenuDownloadSetCountThreadsByFreeLicense))
            return TgEnumMenuClient.DownloadSetCountThreads;
        if (prompt.Equals(TgLocale.MenuDownloadSetCountThreadsByPaidLicense))
            return TgEnumMenuClient.DownloadSetCountThreads;
        if (prompt.Equals(TgLocale.MenuSaveSettings))
            return TgEnumMenuClient.DownloadSettingsSave;
        if (prompt.Equals(TgLocale.MenuManualDownload))
            return TgEnumMenuClient.DownloadManual;

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
                case TgEnumMenuClient.UseClient:
                    await UseClientAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.ClearClientConnectionData:
                    await ClearClientConnectionDataAsync(tgDownloadSettings);
					break;
                case TgEnumMenuClient.RegisterTelegramApp:
                    await WebSiteOpenAsync(TgConstants.LinkTelegramApps);
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
                // Advanced
                case TgEnumMenuClient.AdvancedStartAutoDownload:
                    await RunTaskStatusAsync(tgDownloadSettings, ClientAdvancedStartAutoDownloadAsync,
                        isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
                    break;
                case TgEnumMenuClient.AdvancedAutoViewEvents:
                    await RunTaskStatusAsync(tgDownloadSettings, ClientAdvancedAutoViewEventsAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
                    break;
                case TgEnumMenuClient.AdvancedSearchChats:
                    await ClientAdvancedSearchAsync(tgDownloadSettings, TgEnumSourceType.Chat);
                    break;
                case TgEnumMenuClient.AdvancedSearchDialogs:
                    await ClientAdvancedSearchAsync(tgDownloadSettings, TgEnumSourceType.Dialog);
                    break;
                case TgEnumMenuClient.AdvancedSearchContacts:
                    await ClientAdvancedSearchAsync(tgDownloadSettings, TgEnumSourceType.Contact);
                    break;
                case TgEnumMenuClient.AdvancedSearchStories:
                    await ClientAdvancedSearchAsync(tgDownloadSettings, TgEnumSourceType.Story);
                    break;
                case TgEnumMenuClient.AdvancedMarkAllMessagesAsRead:
                    await ClientAdvancedMarkAllMessagesAsReadAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.AdvancedViewChats:
                    await StorageViewChatsAsync(tgDownloadSettings);
                    break;
                // Download
                case TgEnumMenuClient.DownloadSetSource:
                    tgDownloadSettings = await DownloadSetSourceAsync();
                    break;
                case TgEnumMenuClient.DownloadSetSourceFirstIdAuto:
                    await RunTaskStatusAsync(tgDownloadSettings, DownloadSetSourceFirstIdAutoAsync, isSkipCheckTgSettings: true,
                        isScanCount: false, isWaitComplete: true);
                    break;
                case TgEnumMenuClient.DownloadSetSourceFirstIdManual:
                    await DownloadSetSourceFirstIdManualAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.DownloadSetDestDirectory:
                    SetupDownloadDestDirectory(tgDownloadSettings);
                    if (!tgDownloadSettings.SourceVm.Dto.IsAutoUpdate)
                        DownloadSetDestDirectory(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.DownloadSetIsSaveMessages:
                    DownloadSetIsSaveMessages(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.DownloadSetIsRewriteFiles:
                    DownloadSetIsRewriteFiles(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.DownloadSetIsRewriteMessages:
                    DownloadSetIsRewriteMessages(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.DownloadSetIsAddMessageId:
                    DownloadSetIsAddMessageId(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.DownloadSetIsAutoUpdate:
                    DownloadSetDestDirectory(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.DownloadSetIsCreatingSubdirectories:
                    DownloadSetIsCreatingSubdirectories(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.DownloadSetIsFileNamingByMessage:
                    DownloadSetIsFileNamingByMessage(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.DownloadSetCountThreads:
                    await DownloadSetCountThreadsAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClient.DownloadSettingsSave:
                    await RunTaskStatusAsync(tgDownloadSettings, DownloadSettingsSaveAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: false);
                    break;
                case TgEnumMenuClient.DownloadManual:
                    await RunTaskProgressAsync(tgDownloadSettings, DownloadManualAsync, isSkipCheckTgSettings: false, isScanCount: false);
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
        var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();
        if (!appDto.UseClient)
            await UseClientAsync(tgDownloadSettings);
        appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();
        if (!appDto.UseClient) return;

        if (AskQuestionYesNoReturnNegative(TgLocale.MenuClientConnect)) return;

        TgLog.WriteLine("  TG client connect ...");
        await ConnectClientAsync(tgDownloadSettings, isSilent);
        TgLog.WriteLine("  TG client connect   v");
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

    #region Public and private methods - Advanced

    private async Task ClientAdvancedStartAutoDownloadAsync(TgDownloadSettingsViewModel _)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuClientAdvancedStartAutoDownload)) return;

        var chats = (await BusinessLogicManager.StorageManager.SourceRepository.GetListAsync(TgEnumTableTopRecords.All, 0)).Items;
        chats = [.. chats.Where(sourceSetting => sourceSetting.IsAutoUpdate)];
        foreach (var chat in chats)
        {
            var tgDownloadSettings = await SetupDownloadSourceByIdAsync(chat.Id);
            var chatId = string.IsNullOrEmpty(chat.UserName) ? $"{chat.Id}" : $"{chat.Id} | @{chat.UserName}";
            // StatusContext
            await BusinessLogicManager.ConnectClient.UpdateStateSourceAsync(chat.Id, chat.FirstId, chat.Count,
                chat.Count <= 0
                    ? $"The source {chatId} hasn't any messages!"
                    : $"The source {chatId} has {chat.Count} messages.");
            // ManualDownload
            if (chat.Count > 0)
                await DownloadManualAsync(tgDownloadSettings);
        }
    }

    private async Task ClientAdvancedAutoViewEventsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        BusinessLogicManager.ConnectClient.IsClientUpdateStatus = true;
        await BusinessLogicManager.ConnectClient.UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceVm.Dto.FirstId,
            tgDownloadSettings.SourceVm.Dto.Count, "Client auto view updates is started");
        TgLog.TypeAnyKeyForReturn();
        BusinessLogicManager.ConnectClient.IsClientUpdateStatus = false;
        await Task.CompletedTask;
    }

    private async Task ClientAdvancedSearchAsync(TgDownloadSettingsViewModel tgDownloadSettings, TgEnumSourceType sourceType)
    {
        await ShowTableClientConnectionAsync(tgDownloadSettings);
        if (!BusinessLogicManager.ConnectClient.IsReady)
        {
            TgLog.MarkupWarning(TgLocale.TgMustClientConnect);
            Console.ReadKey();
            return;
        }

        await RunTaskStatusAsync(tgDownloadSettings, async _ => { await BusinessLogicManager.ConnectClient.SearchSourcesTgAsync(tgDownloadSettings, sourceType); },
            isSkipCheckTgSettings: true, isScanCount: true, isWaitComplete: true);
    }

    private async Task ClientAdvancedMarkAllMessagesAsReadAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuClientAdvancedMarkAllMessagesAsRead)) return;
        await RunTaskStatusAsync(tgDownloadSettings, MarkHistoryReadCoreAsync, isSkipCheckTgSettings: true, isScanCount: false, isWaitComplete: true);
    }

    #endregion

    #region Public and private methods - Download

    private async Task<TgDownloadSettingsViewModel> SetupDownloadSourceByIdAsync(long id)
    {
        var tgDownloadSettings = SetupDownloadSourceByIdCore(id);
        await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
        await BusinessLogicManager.ConnectClient.CreateChatAsync(tgDownloadSettings, isSilent: true);
        return tgDownloadSettings;
    }

    private async Task<TgDownloadSettingsViewModel> DownloadSetSourceAsync()
    {
        var tgDownloadSettings = SetupDownloadSourceCore();
        if (string.IsNullOrEmpty(tgDownloadSettings.SourceVm.Dto.UserName))
            await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
        else
            await LoadTgClientSettingsByNameAsync(tgDownloadSettings);
        await BusinessLogicManager.ConnectClient.CreateChatAsync(tgDownloadSettings, isSilent: true);
        return tgDownloadSettings;
    }

    private TgDownloadSettingsViewModel SetupDownloadSourceByIdCore(long id)
    {
        TgDownloadSettingsViewModel tgDownloadSettings = new();
        tgDownloadSettings.SourceVm.Dto.Id = id;
        return tgDownloadSettings;
    }

    private TgDownloadSettingsViewModel SetupDownloadSourceCore()
    {
        TgDownloadSettingsViewModel tgDownloadSettings = new();
        var input = AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.MenuDownloadSetSource}:"));
        if (!string.IsNullOrEmpty(input))
        {
            if (long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sourceId))
                return SetupDownloadSourceByIdCore(sourceId);
            input = TgDataUtils.ClearTgPeer(input);
            tgDownloadSettings.SourceVm.Dto.UserName = input;
            if (!string.IsNullOrEmpty(tgDownloadSettings.SourceVm.Dto.UserName))
                return tgDownloadSettings;
        }
        return tgDownloadSettings;
    }

    private async Task DownloadSetSourceFirstIdAutoAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
        await BusinessLogicManager.ConnectClient.CreateChatAsync(tgDownloadSettings, isSilent: true);
        await BusinessLogicManager.ConnectClient.SetChannelMessageIdFirstAsync(tgDownloadSettings);
    }

    private async Task DownloadSetSourceFirstIdManualAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
        do
        {
            tgDownloadSettings.SourceVm.Dto.FirstId = AnsiConsole.Ask<int>(TgLog.GetLineStampInfo($"{TgLocale.TypeTgSourceFirstId}:"));
        } while (!tgDownloadSettings.SourceVm.Dto.IsReadySourceFirstId);
    }

    private void SetupDownloadDestDirectory(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        do
        {
            tgDownloadSettings.SourceVm.Dto.Directory = AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.DirectoryDestType}:"));
            if (!Directory.Exists(tgDownloadSettings.SourceVm.Dto.Directory))
            {
                TgLog.MarkupInfo(TgLocale.DirectoryIsNotExists(tgDownloadSettings.SourceVm.Dto.Directory));
                if (AskQuestionTrueFalseReturnPositive(TgLocale.DirectoryCreate, true))
                {
                    try
                    {
                        Directory.CreateDirectory(tgDownloadSettings.SourceVm.Dto.Directory);
                    }
                    catch (Exception ex)
                    {
                        CatchException(ex, TgLocale.DirectoryCreateIsException(ex));
                    }
                }
            }
        } while (!Directory.Exists(tgDownloadSettings.SourceVm.Dto.Directory));
    }

    private void DownloadSetIsSaveMessages(TgDownloadSettingsViewModel tgDownloadSettings) =>
        tgDownloadSettings.IsSaveMessages = AskQuestionTrueFalseReturnPositive(TgLocale.MenuDownloadSetIsSaveMessages, true);

    private void DownloadSetIsRewriteFiles(TgDownloadSettingsViewModel tgDownloadSettings) =>
        tgDownloadSettings.IsRewriteFiles = AskQuestionTrueFalseReturnPositive(TgLocale.MenuDownloadSetIsRewriteFiles, true);

    private void DownloadSetIsRewriteMessages(TgDownloadSettingsViewModel tgDownloadSettings) =>
        tgDownloadSettings.IsRewriteMessages = AskQuestionTrueFalseReturnPositive(TgLocale.MenuDownloadSetIsRewriteMessages, true);

    private void DownloadSetIsAddMessageId(TgDownloadSettingsViewModel tgDownloadSettings) =>
        tgDownloadSettings.IsJoinFileNameWithMessageId = AskQuestionTrueFalseReturnPositive(TgLocale.MenuDownloadSetIsAddMessageId, true);

    private void DownloadSetDestDirectory(TgDownloadSettingsViewModel tgDownloadSettings) =>
        tgDownloadSettings.SourceVm.Dto.IsAutoUpdate = AskQuestionTrueFalseReturnPositive(TgLocale.MenuDownloadSetIsAutoUpdate, true);

    private void DownloadSetIsCreatingSubdirectories(TgDownloadSettingsViewModel tgDownloadSettings) =>
        tgDownloadSettings.SourceVm.Dto.IsCreatingSubdirectories = AskQuestionTrueFalseReturnPositive(TgLocale.MenuDownloadSetIsCreatingSubdirectories, true);

    private void DownloadSetIsFileNamingByMessage(TgDownloadSettingsViewModel tgDownloadSettings) =>
        tgDownloadSettings.SourceVm.Dto.IsFileNamingByMessage = AskQuestionTrueFalseReturnPositive(TgLocale.MenuDownloadSetIsFileNamingByMessage, true);

    private async Task DownloadSetCountThreadsAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await LoadTgClientSettingsByIdAsync(tgDownloadSettings);
        tgDownloadSettings.CountThreads = AnsiConsole.Ask<int>(TgLog.GetLineStampInfo($"{TgLocale.MenuDownloadSetCountThreadsByFreeLicense}:"));
        if (tgDownloadSettings.CountThreads < 1)
            tgDownloadSettings.CountThreads = 1;
        else
        {
            switch (BusinessLogicManager.LicenseService.CurrentLicense.LicenseType)
            {
                case TgEnumLicenseType.Test:
                case TgEnumLicenseType.Paid:
                case TgEnumLicenseType.Premium:
                    if (tgDownloadSettings.CountThreads > TgGlobalTools.DownloadCountThreadsLimitPaid)
                        tgDownloadSettings.CountThreads = TgGlobalTools.DownloadCountThreadsLimitPaid;
                    break;
                default:
                    if (tgDownloadSettings.CountThreads > TgGlobalTools.DownloadCountThreadsLimitFree)
                        tgDownloadSettings.CountThreads = TgGlobalTools.DownloadCountThreadsLimitFree;
                    break;
            }
        }
    }

    private async Task DownloadSettingsSaveAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await tgDownloadSettings.UpdateSourceWithSettingsAsync();
        await BusinessLogicManager.ConnectClient.UpdateStateSourceAsync(tgDownloadSettings.SourceVm.Dto.Id, tgDownloadSettings.SourceVm.Dto.FirstId, tgDownloadSettings.SourceVm.Dto.Count,
            TgLocale.SettingsChat);
    }

    private async Task LoadTgClientSettingsByIdAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        // Save manual settings
        var directory = tgDownloadSettings.SourceVm.Dto.Directory;
        var firstId = tgDownloadSettings.SourceVm.Dto.FirstId;
        // Find by ID
        var storageResult = await BusinessLogicManager.StorageManager.SourceRepository.GetAsync(new() { Id = tgDownloadSettings.SourceVm.Dto.Id });
        if (storageResult.IsExists)
        {
            if (string.IsNullOrEmpty(directory))
                directory = storageResult.Item.Directory;
            tgDownloadSettings.SourceVm.Dto = new TgEfSourceDto().Copy(storageResult.Item, isUidCopy: true);
        }
        // Restore manual settings
        if (!string.IsNullOrEmpty(directory))
            tgDownloadSettings.SourceVm.Dto.Directory = directory;
        if (firstId > 1)
            tgDownloadSettings.SourceVm.Dto.FirstId = firstId;
    }

    private async Task LoadTgClientSettingsByNameAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        var directory = tgDownloadSettings.SourceVm.Dto.Directory;
        // Find by UserName
        var storageResult = await BusinessLogicManager.StorageManager.SourceRepository.GetAsync(new() { UserName = tgDownloadSettings.SourceVm.Dto.UserName });
        if (storageResult.IsExists)
        {
            if (string.IsNullOrEmpty(directory))
                directory = storageResult.Item.Directory;
            tgDownloadSettings.SourceVm.Dto = new TgEfSourceDto().Copy(storageResult.Item, isUidCopy: true);
        }
        // Restore directory
        if (!string.IsNullOrEmpty(directory) && string.IsNullOrEmpty(tgDownloadSettings.SourceVm.Dto.Directory))
            tgDownloadSettings.SourceVm.Dto.Directory = directory;
    }

    private async Task DownloadManualAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ShowTableDownloadAsync(tgDownloadSettings);
        await DownloadSettingsSaveAsync(tgDownloadSettings);
        try
        {
            await BusinessLogicManager.ConnectClient.DownloadAllDataAsync(tgDownloadSettings);
        }
        catch (Exception ex)
        {
            CatchException(ex);
            var floodWait = BusinessLogicManager.ConnectClient.Client?.FloodRetryThreshold ?? 60;
            TgLog.MarkupWarning($"Flood control: waiting {floodWait} seconds");
            await Task.Delay(floodWait * 1_000);
            // Repeat request after waiting
            await DownloadManualAsync(tgDownloadSettings);
        }
        await DownloadSettingsSaveAsync(tgDownloadSettings);
    }

    private async Task MarkHistoryReadCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        await ShowTableMarkHistoryReadProgressAsync(tgDownloadSettings);
        await BusinessLogicManager.ConnectClient.MarkHistoryReadAsync();
        await ShowTableMarkHistoryReadCompleteAsync(tgDownloadSettings);
    }

    #endregion
}