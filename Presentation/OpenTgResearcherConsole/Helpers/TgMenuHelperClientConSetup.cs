namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
    #region Methods

    private TgEnumMenuClientConSetup SetMenuClientConSetup()
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
            TgLocale.MenuClientDisconnect
        );

        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt.Equals(TgLocale.MenuClientClearConnectionData))
            return TgEnumMenuClientConSetup.ClearClientConnectionData;
        if (prompt.Equals(TgLocale.MenuRegisterTelegramApp))
            return TgEnumMenuClientConSetup.RegisterTelegramApp;
        if (prompt.Equals(TgLocale.MenuClientUseClient))
            return TgEnumMenuClientConSetup.UseClient;
        if (prompt.Equals(TgLocale.MenuClientSetProxy))
            return TgEnumMenuClientConSetup.ClientSetProxy;
        if (prompt.Equals(TgLocale.MenuClientConnect))
            return TgEnumMenuClientConSetup.ClientConnect;
        if (prompt.Equals(TgLocale.MenuClientDisconnect))
            return TgEnumMenuClientConSetup.ClientDisconnect;

        return TgEnumMenuClientConSetup.Return;
    }

    public async Task SetupClientConnectionSetupAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        TgEnumMenuClientConSetup menu;
        do
        {
            await ShowTableClientConSetupAsync(tgDownloadSettings);

            menu = SetMenuClientConSetup();
            switch (menu)
            {
                case TgEnumMenuClientConSetup.UseClient:
                    await SetUseClientAsync(isSilent: false);
                    break;
                case TgEnumMenuClientConSetup.ClearClientConnectionData:
                    await ClearClientConnectionDataAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConSetup.RegisterTelegramApp:
                    await WebSiteOpenAsync(TgConstants.LinkTelegramApps);
                    break;
                case TgEnumMenuClientConSetup.ClientSetProxy:
                    await SetupClientProxyAsync();
                    await ClientConnectAsync(tgDownloadSettings, isSilent: false);
                    break;
                case TgEnumMenuClientConSetup.ClientConnect:
                    await ClientConnectAsync(tgDownloadSettings, isSilent: false);
                    break;
                case TgEnumMenuClientConSetup.ClientDisconnect:
                    await ClientDisconnectAsync(tgDownloadSettings);
                    break;
                case TgEnumMenuClientConSetup.Return:
                    break;
            }
        } while (menu is not TgEnumMenuClientConSetup.Return);
    }

    /// <summary> Set UseClient property </summary>
    public async Task<bool> SetUseClientAsync(bool isSilent)
    {
        var useClient = isSilent || AskQuestionYesNoReturnPositive(TgLocale.MenuClientUseClient, isYesFirst: true);
        return await BusinessLogicManager.StorageManager.AppRepository.SetUseClientAsync(useClient);
    }

    public async Task ClearClientConnectionDataAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuClientClearConnectionData)) return;
        await ShowTableClientConSetupAsync(tgDownloadSettings);

        await BusinessLogicManager.StorageManager.AppRepository.DeleteAllAsync();
        await BusinessLogicManager.ConnectClient.DisconnectClientAsync();
    }

    private async Task SetupClientProxyAsync()
    {
        var proxyDto = await AddOrUpdateProxyAsync();

        if (proxyDto.Type == TgEnumProxyType.MtProto)
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
                proxyDto.Secret = AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TypeTgProxySecret}:"));
        }
        await BusinessLogicManager.StorageManager.ProxyRepository.SaveAsync(proxyDto);
    }

    private async Task<TgEfProxyDto> AddOrUpdateProxyAsync()
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
        var proxyDto = GetProxyFromPrompt(prompt);
        if (!Equals(proxyDto.Type, TgEnumProxyType.None))
        {
            proxyDto.HostName = AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TypeTgProxyHostName}:"));
            proxyDto.Port = AnsiConsole.Ask<ushort>(TgLog.GetLineStampInfo($"{TgLocale.TypeTgProxyPort}:"));
        }

        await BusinessLogicManager.StorageManager.ProxyRepository.SaveAsync(proxyDto);
        proxyDto = await BusinessLogicManager.StorageManager.ProxyRepository.GetDtoAsync(
            where: x => x.Type == proxyDto.Type && x.HostName == proxyDto.HostName && x.Port == proxyDto.Port);

        var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentAppDtoAsync();
        if (appDto.Uid != Guid.Empty && proxyDto.Uid != Guid.Empty)
        {
            appDto.ProxyUid = proxyDto.Uid;
            await BusinessLogicManager.StorageManager.AppRepository.SaveAsync(appDto);
        }
        
        return proxyDto;
    }

    private static TgEfProxyDto GetProxyFromPrompt(string prompt) => new()
    {
        Type = prompt switch
        {
            nameof(TgEnumProxyType.Http) => TgEnumProxyType.Http,
            nameof(TgEnumProxyType.Socks) => TgEnumProxyType.Socks,
            nameof(TgEnumProxyType.MtProto) => TgEnumProxyType.MtProto,
            _ => TgEnumProxyType.None
        },
    };

    public async Task ClientConnectAsync(TgDownloadSettingsViewModel tgDownloadSettings, bool isSilent)
    {
        try
        {
            // Check connect
            var isClientConnect = await BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync();

            // Switch flag
            var isUseClient = await SetUseClientAsync(isSilent: true);

            if (!isSilent)
            {
                // Question
                if (AskQuestionYesNoReturnNegative(TgLocale.MenuClientConnect, isYesFirst: true)) return;
                // Connect
                TgLog.WriteLine("  TG client connect ...");
            }

            var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();
            var proxyDto = await BusinessLogicManager.StorageManager.ProxyRepository.GetDtoAsync(x => x.Uid == appDto.ProxyUid);
            await BusinessLogicManager.ConnectClient.ConnectSessionAsync(ConfigClientConsole, proxyDto, isDesktop: false);
            
            if (!isSilent)
            {
                if (BusinessLogicManager.ConnectClient.ClientException.IsExist || BusinessLogicManager.ConnectClient.ProxyException.IsExist)
                    TgLog.WriteLine($"  {TgLocale.TgClientSetupCompleteError}");
                else
                    TgLog.WriteLine($"  {TgLocale.TgClientSetupCompleteSuccess}");
                TgLog.TypeAnyKeyForReturn();
                
                TgLog.WriteLine("  TG client connect   v");
                
                await ShowTableClientConSetupAsync(tgDownloadSettings);
            }
        }
        catch (Exception ex)
        {
            CatchException(ex, TgLocale.TgBotSetupCompleteError);
        }
    }

    private string? ConfigClientConsole(string what)
    {
        var appNew = BusinessLogicManager.StorageManager.AppRepository.GetNewItem();
        var appDto = BusinessLogicManager.StorageManager.AppRepository.GetCurrentAppDtoAsync().GetAwaiter().GetResult() ?? new();
        switch (what)
        {
            case "api_hash":
                var apiHash = !Equals(appDto.ApiHash, appNew.ApiHash)
                    ? TgDataFormatUtils.ParseGuidToString(appDto.ApiHash)
                    : TgDataFormatUtils.ParseGuidToString(TgDataFormatUtils.ParseStringToGuid(
                        AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TgSetupApiHash}:"))));
                if (appDto.ApiHash != TgDataFormatUtils.ParseStringToGuid(apiHash))
                {
                    appDto.ApiHash = TgDataFormatUtils.ParseStringToGuid(apiHash);
                    BusinessLogicManager.StorageManager.AppRepository.SaveAsync(appDto).GetAwaiter().GetResult();
                }
                return apiHash;
            case "api_id":
                var apiId = !Equals(appDto.ApiId, appNew.ApiId)
                    ? appDto.ApiId.ToString()
                    : AnsiConsole.Ask<int>(TgLog.GetLineStampInfo($"{TgLocale.TgSetupAppId}:"))
                    .ToString();
                if (appDto.ApiId != int.Parse(apiId))
                {
                    appDto.ApiId = int.Parse(apiId);
                    BusinessLogicManager.StorageManager.AppRepository.SaveAsync(appDto).GetAwaiter().GetResult();
                }
                return apiId;
            case "phone_number":
                var phoneNumber = !Equals(appDto.PhoneNumber, appNew.PhoneNumber)
                    ? appDto.PhoneNumber
                    : AnsiConsole.Ask<string>(TgLog.GetLineStampInfo($"{TgLocale.TgSetupPhone}:"));
                if (appDto.PhoneNumber != phoneNumber)
                {
                    appDto.PhoneNumber = phoneNumber;
                    BusinessLogicManager.StorageManager.AppRepository.SaveAsync(appDto).GetAwaiter().GetResult();
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

    public async Task ClientDisconnectAsync(TgDownloadSettingsViewModel tgDownloadSettings)
    {
        // Check connect
        var isClientConnect = await BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync();
        if (!isClientConnect) return;

        // Question
        if (AskQuestionYesNoReturnNegative(TgLocale.MenuClientDisconnect)) return;

        // Disconnect
        await BusinessLogicManager.ConnectClient.DisconnectClientAsync();
    }

    #endregion
}
