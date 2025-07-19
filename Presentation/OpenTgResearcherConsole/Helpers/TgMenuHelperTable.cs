// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
    #region Public and private methods

    private async Task ShowTableCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings, Action<Table> fillTableColumns,
        Func<TgDownloadSettingsViewModel, Table, Task> fillTableRowsAsync)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText(TgConstants.OpenTgResearcher).Centered().Color(Color.Yellow));
        Table table = new()
        {
            ShowHeaders = false,
            Border = TableBorder.Rounded,
            // App version + Storage version + License
            Title = new($"{TgLocale.AppVersionShort} {TgAppSettings.AppVersion} / {TgLocale.StorageVersionShort} {TgAppSettings.StorageVersion} / " +
                $"{BusinessLogicManager.LicenseService.CurrentLicense.Description ?? string.Empty}", Style.Plain),
        };
        fillTableColumns(table);
        await fillTableRowsAsync(tgDownloadSettings, table);
        table.Expand();
        AnsiConsole.Write(table);
    }

    private async Task ShowTableCoreAsync(Action<Table> fillTableColumns,
        Func<Table, Task> fillTableRowsAsync)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText(TgConstants.OpenTgResearcher).Centered().Color(Color.Yellow));
        Table table = new()
        {
            ShowHeaders = false,
            Border = TableBorder.Rounded,
            // App version + Storage version + License
            Title = new($"{TgLocale.AppVersionShort} {TgAppSettings.AppVersion} / {TgLocale.StorageVersionShort} {TgAppSettings.StorageVersion} / " +
                $"{BusinessLogicManager.LicenseService.CurrentLicense.Description ?? string.Empty}", Style.Plain),
        };
        fillTableColumns(table);
        await fillTableRowsAsync(table);
        table.Expand();
        AnsiConsole.Write(table);
    }

    public async Task ShowTableMainAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsMainAsync);

    private async Task ShowTableStorageAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsStorageAsync);

    private async Task ShowTableStorageSetupAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsStorageSetupAsync);

    private async Task ShowTableStorageAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsStorageAdvancedAsync);

    private async Task ShowTableStorageClearAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsStorageClearingAsync);

    private async Task ShowTableStorageChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, async (tgDownloadSettings, table) => await
            FillTableRowsStorageTableAsync(table, BusinessLogicManager.StorageManager.SourceRepository, TgLocale.MenuTableChatsRecordsCount));

    private async Task ShowTableStorageFiltersAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, async (tgDownloadSettings, table) => await
            FillTableRowsStorageTableAsync(table, BusinessLogicManager.StorageManager.FilterRepository, TgLocale.MenuTableFiltersRecordsCount));

    private async Task ShowTableLicenseFullInfoAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsLicenseFullInfoAsync);

    private async Task ShowTableUpdateAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsUpdateAsync);

    private async Task ShowTableApplicationAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(FillTableColumns, FillTableRowsApplicationAsync);

    private async Task ShowTableBotConAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsBotConAsync);

    private async Task ShowTableBotConSetupAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsBotConSetupAsync);

    private async Task ShowTableBotConAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsBotConAdvancedAsync);

    private async Task ShowTableBotConDownloadAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsBotConDownloadAsync);

    private async Task ShowTableBotConSearchAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsBotConSearchAsync);

    private async Task ShowTableClientConAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(FillTableColumns, FillTableRowsClientConAsync);

    private async Task ShowTableClientConSetupAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsClientConSetupAsync);

    private async Task ShowTableClientConAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsClientConAdvancedAsync);

    private async Task ShowTableClientConDownloadAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsClientConDownloadAsync);

    private async Task ShowTableClientConSearchAsync() =>
        await ShowTableCoreAsync(FillTableColumns, FillTableRowsClientConSearchAsync);

    private async Task ShowTableViewContactsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, async (_, __) => await Task.CompletedTask);

    private async Task ShowTableViewChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, async (_, __) => await Task.CompletedTask);

    private async Task ShowTableViewFiltersAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, async (_, __) => await Task.CompletedTask);

    private async Task ShowTableViewStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, async (_, __) => await Task.CompletedTask);

    private async Task ShowTableViewVersionsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, async (_, __) => await Task.CompletedTask);

    private async Task ShowTableMarkHistoryReadProgressAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsMarkHistoryReadProgressAsync);

    private async Task ShowTableMarkHistoryReadCompleteAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsMarkHistoryReadCompleteAsync);

    internal void FillTableColumns(Table table)
    {
        if (table.Columns.Count > 0) return;
        table.AddColumn(new TableColumn(GetMarkup(TgLocale.AppName, StyleMain)) { Width = 23 }.LeftAligned());
        table.AddColumn(new TableColumn(GetMarkup(TgLocale.AppValue, StyleMain)) { Width = 77 }.LeftAligned());
    }

    private async Task FillTableRowsEmpty(Table table)
    {
        //table.AddEmptyRow();
        await Task.CompletedTask;
    }

    private async Task FillTableRowsMainAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        // Application health check
        var isAppReady = TgAppSettings.AppXml.IsExistsFileSession && TgAppSettings.AppXml.IsExistsEfStorage;
        table.AddRow(GetMarkup(isAppReady
            ? TgLocale.InfoMessage(TgLocale.MenuMainAppHealthCheck) : TgLocale.WarningMessage(TgLocale.MenuMainAppHealthCheck)),
            GetMarkup(isAppReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

        // Storage health check
        table.AddRow(GetMarkup(TgGlobalTools.IsXmlReady
                ? TgLocale.InfoMessage(TgLocale.MenuMainStorageHealthCheck)
                : TgLocale.WarningMessage(TgLocale.MenuMainStorageHealthCheck)),
            GetMarkup(TgGlobalTools.IsXmlReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

        var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();
        if (appDto.UseClient)
        {
            // Client health check
            var clientConnectReady = BusinessLogicManager.ConnectClient.IsReady;
            table.AddRow(GetMarkup(clientConnectReady
                ? TgLocale.InfoMessage(TgLocale.MenuMainClientConnectionHealthCheck)
                : TgLocale.WarningMessage(TgLocale.MenuMainClientConnectionHealthCheck)),
                GetMarkup(clientConnectReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));
        }
        else
        {
            // Bot health check
            var isBotConnectionReady = await BusinessLogicManager.ConnectClient.CheckBotConnectionReadyAsync();
            table.AddRow(GetMarkup(isBotConnectionReady
                ? TgLocale.InfoMessage(TgLocale.MenuMainBotConnectionHealthCheck)
                : TgLocale.WarningMessage(TgLocale.MenuMainBotConnectionHealthCheck)),
                GetMarkup(isBotConnectionReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));
        }

        await Task.CompletedTask;
    }

    private async Task FillTableRowsApplicationAsync(Table table)
    {
        // Current directory
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.DirectoryCurrent)), GetMarkup(Environment.CurrentDirectory));

        // Storage
        var fileSize = TgFileUtils.GetFileSizeAsString(TgFileUtils.CalculateFileSize(TgAppSettings.AppXml.XmlEfStorage));
        table.AddRow(TgAppSettings.AppXml.IsExistsEfStorage
            ? GetMarkup(TgLocale.InfoMessage(TgLocale.EfStorage)) : GetMarkup(TgLocale.WarningMessage(TgLocale.EfStorage)),
            GetMarkup($"{Path.Combine(Environment.CurrentDirectory, TgAppSettings.AppXml.XmlEfStorage)} ({fileSize})"));

        // Session
        fileSize = TgFileUtils.GetFileSizeAsString(TgFileUtils.CalculateFileSize(TgAppSettings.AppXml.XmlFileSession));
        table.AddRow(TgAppSettings.AppXml.IsExistsFileSession
            ? GetMarkup(TgLocale.InfoMessage(TgLocale.FileSession)) : GetMarkup(TgLocale.WarningMessage(TgLocale.FileSession)),
            GetMarkup($"{Path.Combine(Environment.CurrentDirectory, TgAppSettings.AppXml.XmlFileSession)} ({fileSize})"));

        // Using a client's proxy
        if (TgAppSettings.IsUseProxy)
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuAppUseProxy)), GetMarkup(TgAppSettings.IsUseProxy.ToString()));
        else
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuAppUseProxy, true)), GetMarkup(TgAppSettings.IsUseProxy.ToString()));

        await Task.CompletedTask;
    }

    /// <summary> Storage </summary>
    private async Task FillTableRowsStorageAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        // Storage
        table.AddRow(GetMarkup(TgGlobalTools.IsXmlReady
            ? TgLocale.InfoMessage(TgLocale.MenuMainStorage) : TgLocale.WarningMessage(TgLocale.MenuMainStorage)),
            GetMarkup(TgGlobalTools.IsXmlReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

        // Counts
        await FillTableRowsStorageTableAsync(table, BusinessLogicManager.StorageManager.AppRepository, TgLocale.MenuTableAppsRecordsCount);
        await FillTableRowsStorageTableAsync(table, BusinessLogicManager.StorageManager.SourceRepository, TgLocale.MenuTableChatsRecordsCount);
        await FillTableRowsStorageTableAsync(table, BusinessLogicManager.StorageManager.UserRepository, TgLocale.MenuTableContactsRecordsCount);
        await FillTableRowsStorageTableAsync(table, BusinessLogicManager.StorageManager.DocumentRepository, TgLocale.MenuTableDocumentsRecordsCount);
        await FillTableRowsStorageTableAsync(table, BusinessLogicManager.StorageManager.FilterRepository, TgLocale.MenuTableFiltersRecordsCount);
        await FillTableRowsStorageTableAsync(table, BusinessLogicManager.StorageManager.MessageRepository, TgLocale.MenuTableMessagesRecordsCount);
        await FillTableRowsStorageTableAsync(table, BusinessLogicManager.StorageManager.ProxyRepository, TgLocale.MenuTableProxiesRecordsCount);
        await FillTableRowsStorageTableAsync(table, BusinessLogicManager.StorageManager.StoryRepository, TgLocale.MenuTableStoriesRecordsCount);
        await FillTableRowsStorageTableAsync(table, BusinessLogicManager.StorageManager.VersionRepository, TgLocale.MenuTableVersionsRecordsCount);

        await Task.CompletedTask;
    }

    /// <summary> Storage setup </summary>
    private async Task FillTableRowsStorageSetupAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        await Task.CompletedTask;
    }

    /// <summary> Storage advanced </summary>
    private async Task FillTableRowsStorageAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        await Task.CompletedTask;
    }

    /// <summary> Storage clearing </summary>
    private async Task FillTableRowsStorageClearingAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        await Task.CompletedTask;
    }

    /// <summary> Storage table </summary>
    private async Task FillTableRowsStorageTableAsync<TEfEntity, TDto>(Table table,
        ITgEfRepository<TEfEntity, TDto> repository, string messageCount)
        where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
        where TDto : class, ITgDto<TEfEntity, TDto>, new()
    {
        var count = await repository.GetListCountAsync();
        table.AddRow(GetMarkup(TgLocale.InfoMessage(messageCount)), GetMarkup($"{count}"));

        await Task.CompletedTask;
    }

    /// <summary> License view info </summary>
    private async Task FillTableRowsLicenseFullInfoAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuLicenseIsConfirmed)), GetMarkup(BusinessLogicManager.LicenseService.CurrentLicense.IsConfirmed.ToString()));
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuLicenseKey)), GetMarkup(BusinessLogicManager.LicenseService.CurrentLicense.GetLicenseKeyString()));
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuLicenseDescription)), GetMarkup(BusinessLogicManager.LicenseService.CurrentLicense.Description));
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuUserId)), GetMarkup(BusinessLogicManager.LicenseService.CurrentLicense.GetUserIdString()));
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuLicenseExpiration)), GetMarkup(BusinessLogicManager.LicenseService.CurrentLicense.GetValidToString()));
        await Task.CompletedTask;
    }

    /// <summary> Update settings </summary>
    private async Task FillTableRowsUpdateAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        // App version
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.AppVersion)), GetMarkup(TgAppSettings.AppVersion));
        await Task.CompletedTask;
    }

    private async Task FillTableRowsBotConAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        // Bot connection
        var isBotConnectionReady = await BusinessLogicManager.ConnectClient.CheckBotConnectionReadyAsync();
        table.AddRow(GetMarkup(isBotConnectionReady
            ? TgLocale.InfoMessage(TgLocale.MenuMainBotConnection)
            : TgLocale.WarningMessage(TgLocale.MenuMainBotConnection)),
                GetMarkup(isBotConnectionReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));
        await Task.CompletedTask;
    }

    private async Task FillTableRowsBotConSetupAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();

        // Use bot
        if (appDto.UseBot)
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotUseBot)), GetMarkup(appDto.UseBot.ToString()));
        else
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotUseBot, isUseX: true)), GetMarkup(appDto.UseBot.ToString()));

        if (appDto.UseBot)
        {
            // Bot token
            if (!string.IsNullOrEmpty(appDto.BotTokenKey))
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotToken)), GetMarkup(appDto.BotTokenKey));
            else
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotToken, isUseX: true)), GetMarkup(appDto.BotTokenKey));
        }

        // Bot connection
        await FillTableRowsBotConAsync(tgDownloadSettings, table);

        await Task.CompletedTask;
    }

    private async Task FillTableRowsBotConAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();

        // Use bot
        if (appDto.UseBot)
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotUseBot)), GetMarkup(appDto.UseBot.ToString()));
        else
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotUseBot, isUseX: true)), GetMarkup(appDto.UseBot.ToString()));

        if (appDto.UseBot)
        {
            // Bot token
            if (!string.IsNullOrEmpty(appDto.BotTokenKey))
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotToken)), GetMarkup(appDto.BotTokenKey));
            else
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotToken, isUseX: true)), GetMarkup(appDto.BotTokenKey));
        }

        // Bot connection
        await FillTableRowsBotConAsync(tgDownloadSettings, table);

        await Task.CompletedTask;
    }

    private async Task FillTableRowsBotConDownloadAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        // Bot connection
        await FillTableRowsBotConAsync(tgDownloadSettings, table);

        await Task.CompletedTask;
    }

    private async Task FillTableRowsBotConSearchAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        // Bot connection
        await FillTableRowsBotConAsync(tgDownloadSettings, table);

        await Task.CompletedTask;
    }

    private async Task FillTableRowsClientConAsync(Table table)
    {
        var isClientConnectionReady = await BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync();

        // Client connection
        table.AddRow(GetMarkup(isClientConnectionReady
            ? TgLocale.InfoMessage(TgLocale.MenuMainClientConnection)
            : TgLocale.WarningMessage(TgLocale.MenuMainClientConnection)),
                GetMarkup(isClientConnectionReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

        // Exceptions
        if (BusinessLogicManager.ConnectClient.ProxyException.IsExist)
        {
            table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientProxyException)), GetMarkup(BusinessLogicManager.ConnectClient.ProxyException.Message));
        }
        if (BusinessLogicManager.ConnectClient.ClientException.IsExist)
        {
            table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientException)), GetMarkup(BusinessLogicManager.ConnectClient.ClientException.Message));
            table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientFix)), GetMarkup(TgLocale.TgClientFixTryToDeleteSession));
        }

        await Task.CompletedTask;
    }

    private async Task FillTableRowsClientConSetupAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();

        // Use client
        if (appDto.UseClient)
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotUseClient)), GetMarkup(appDto.UseClient.ToString()));
        else
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotUseClient, isUseX: true)), GetMarkup(appDto.UseClient.ToString()));

        if (appDto.UseClient)
        {
            // User name, user id, user active
            if (BusinessLogicManager.ConnectClient.Me is null)
            {
                table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientUserName)),
                    GetMarkup(TgLocale.SettingsIsNeedSetup));
                table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientUserId)),
                    GetMarkup(TgLocale.SettingsIsNeedSetup));
                table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientUserIsActive)),
                    GetMarkup(TgLocale.SettingsIsNeedSetup));
            }
            else
            {
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientUserName)),
                    GetMarkup(BusinessLogicManager.ConnectClient.Me.username));
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientUserId)),
                    GetMarkup(BusinessLogicManager.ConnectClient.Me.id.ToString()));
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientUserIsActive)),
                    GetMarkup(BusinessLogicManager.ConnectClient.Me.IsActive.ToString()));
            }

            // Proxy setup
            if (Equals(await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyUidAsync(), Guid.Empty))
            {
                if (TgAppSettings.IsUseProxy)
                    table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientProxySetup)),
                        GetMarkup(TgLocale.SettingsIsNeedSetup));
                else
                    table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxySetup)),
                        GetMarkup(TgLocale.SettingsIsOk));
            }
            else
            {
                // Proxy is not found
                if (!(await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(appDto.ProxyUid)).IsExists || BusinessLogicManager.ConnectClient.Me is null)
                {
                    table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientProxySetup)), GetMarkup(TgLocale.SettingsIsNeedSetup));
                    table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientProxyType)), GetMarkup(TgLocale.SettingsIsNeedSetup));
                    table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientProxyHostName)), GetMarkup(TgLocale.SettingsIsNeedSetup));
                    table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientProxyPort)), GetMarkup(TgLocale.SettingsIsNeedSetup));
                    table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientProxySecret)), GetMarkup(TgLocale.SettingsIsNeedSetup));
                }
                // Proxy is found
                else
                {
                    var proxyEntity = (await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(appDto.ProxyUid)).Item;
                    table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxySetup)), GetMarkup(TgLocale.SettingsIsOk));
                    table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxyType)),
                        GetMarkup(proxyEntity.Type.ToString()));
                    table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxyHostName)),
                        GetMarkup(proxyEntity.HostName));
                    table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxyPort)),
                        GetMarkup(proxyEntity.Port.ToString()));
                    if (Equals(proxyEntity.Type, TgEnumProxyType.MtProto))
                        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxySecret)),
                            GetMarkup(proxyEntity.Secret));
                }
            }

            // Enable auto update
            if (tgDownloadSettings.SourceVm.Dto.IsAutoUpdate)
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsAutoUpdate)),
                    GetMarkup(tgDownloadSettings.SourceVm.Dto.IsAutoUpdate.ToString()));
            else
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsAutoUpdate, isUseX: true)),
                    GetMarkup(tgDownloadSettings.SourceVm.Dto.IsAutoUpdate.ToString()));
        }

        // Client connection
        await FillTableRowsClientConAsync(table);

        await Task.CompletedTask;
    }

    private async Task FillTableRowsClientConAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();

        // Use client
        if (appDto.UseClient)
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotUseClient)), GetMarkup(appDto.UseClient.ToString()));
        else
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotUseClient, isUseX: true)), GetMarkup(appDto.UseClient.ToString()));

        if (appDto.UseClient)
        {
            // User name, user id, user active
            if (BusinessLogicManager.ConnectClient.Me is null)
            {
                table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientUserName)),
                    GetMarkup(TgLocale.SettingsIsNeedSetup));
                table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientUserId)),
                    GetMarkup(TgLocale.SettingsIsNeedSetup));
                table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientUserIsActive)),
                    GetMarkup(TgLocale.SettingsIsNeedSetup));
            }
            else
            {
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientUserName)),
                    GetMarkup(BusinessLogicManager.ConnectClient.Me.username));
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientUserId)),
                    GetMarkup(BusinessLogicManager.ConnectClient.Me.id.ToString()));
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientUserIsActive)),
                    GetMarkup(BusinessLogicManager.ConnectClient.Me.IsActive.ToString()));
            }

            // Proxy setup
            if (Equals(await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyUidAsync(), Guid.Empty))
            {
                if (TgAppSettings.IsUseProxy)
                    table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientProxySetup)),
                        GetMarkup(TgLocale.SettingsIsNeedSetup));
                else
                    table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxySetup)),
                        GetMarkup(TgLocale.SettingsIsOk));
            }
            else
            {
                // Proxy is not found
                if (!(await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(appDto.ProxyUid)).IsExists || BusinessLogicManager.ConnectClient.Me is null)
                {
                    table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientProxySetup)), GetMarkup(TgLocale.SettingsIsNeedSetup));
                    table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientProxyType)), GetMarkup(TgLocale.SettingsIsNeedSetup));
                    table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientProxyHostName)), GetMarkup(TgLocale.SettingsIsNeedSetup));
                    table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientProxyPort)), GetMarkup(TgLocale.SettingsIsNeedSetup));
                    table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgClientProxySecret)), GetMarkup(TgLocale.SettingsIsNeedSetup));
                }
                // Proxy is found
                else
                {
                    table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxySetup)), GetMarkup(TgLocale.SettingsIsOk));
                    table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxyType)),
                        GetMarkup((await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(appDto.ProxyUid)).Item.Type.ToString()));
                    table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxyHostName)),
                        GetMarkup((await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(appDto.ProxyUid)).Item.HostName));
                    table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxyPort)),
                        GetMarkup((await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(appDto.ProxyUid)).Item.Port.ToString()));
                    if (Equals((await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(appDto.ProxyUid)).Item.Type, TgEnumProxyType.MtProto))
                        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxySecret)),
                            GetMarkup((await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(appDto.ProxyUid)).Item.Secret));
                }
            }

            // Enable auto update
            if (tgDownloadSettings.SourceVm.Dto.IsAutoUpdate)
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsAutoUpdate)),
                    GetMarkup(tgDownloadSettings.SourceVm.Dto.IsAutoUpdate.ToString()));
            else
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsAutoUpdate, isUseX: true)),
                    GetMarkup(tgDownloadSettings.SourceVm.Dto.IsAutoUpdate.ToString()));
        }

        // Client connection
        await FillTableRowsClientConAsync(table);

        await Task.CompletedTask;
    }

    private async Task FillTableRowsClientConDownloadAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        // Download
        table.AddRow(GetMarkup(tgDownloadSettings.SourceVm.Dto.IsReady
                ? TgLocale.InfoMessage(TgLocale.MenuMainClientDownload) : TgLocale.WarningMessage(TgLocale.MenuMainClientDownload)),
            GetMarkup(tgDownloadSettings.SourceVm.Dto.IsReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

        // Chat info
        await FillTableRowsDownloadedChatsAsync(tgDownloadSettings, table);

        // Destination directory
        if (string.IsNullOrEmpty(tgDownloadSettings.SourceVm.Dto.Directory))
            table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.TgSettingsDestDirectory)),
                GetMarkup(TgLocale.SettingsIsNeedSetup));
        else
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgSettingsDestDirectory)),
                GetMarkup(TgLogHelper.Instance.GetMarkupString(tgDownloadSettings.SourceVm.Dto.Directory, isReplaceSpec: true)));

        // First/last ID
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgSettingsSourceFirstLastId)),
            GetMarkup($"{tgDownloadSettings.SourceVm.Dto.FirstId} / {tgDownloadSettings.SourceVm.Dto.Count}"));

        // Rewrite files
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsRewriteFiles)),
            GetMarkup(tgDownloadSettings.IsRewriteFiles.ToString()));

        // Save messages
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsSaveMessages)),
            GetMarkup(tgDownloadSettings.IsSaveMessages.ToString()));

        // Rewrite messages
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsRewriteMessages)),
            GetMarkup(tgDownloadSettings.IsRewriteMessages.ToString()));

        // Join message ID
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsAddMessageId)),
            GetMarkup(tgDownloadSettings.IsJoinFileNameWithMessageId.ToString()));

        // Enable auto update
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsAutoUpdate)),
            GetMarkup(tgDownloadSettings.SourceVm.Dto.IsAutoUpdate.ToString()));

        // Enable creating subdirectories
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsCreatingSubdirectories)),
            GetMarkup(tgDownloadSettings.SourceVm.Dto.IsCreatingSubdirectories.ToString()));

        // Enable file naming by message
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsFileNamingByMessage)),
            GetMarkup(tgDownloadSettings.SourceVm.Dto.IsFileNamingByMessage.ToString()));

        // Restrict saving content
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsRestrictSavingContent)),
            GetMarkup(tgDownloadSettings.SourceVm.Dto.IsRestrictSavingContent.ToString()));

        // Enabled filters
        var filters = (await BusinessLogicManager.StorageManager.FilterRepository.GetListAsync(TgEnumTableTopRecords.All, 0))
            .Items.Where(f => f.IsEnabled);
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuFiltersEnabledCount)), GetMarkup($"{filters.Count()}"));

        // Count of threads
        switch (BusinessLogicManager.LicenseService.CurrentLicense.LicenseType)
        {
            case TgEnumLicenseType.Test:
            case TgEnumLicenseType.Paid:
            case TgEnumLicenseType.Premium:
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetCountThreadsByPaidLicense)), GetMarkup($"{tgDownloadSettings.CountThreads}"));
                break;
            default:
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetCountThreadsByFreeLicense)), GetMarkup($"{tgDownloadSettings.CountThreads}"));
                break;
        }

        // User access
        if (tgDownloadSettings.SourceVm.Dto.IsUserAccess)
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadUserAccess)), GetMarkup($"{true}"));
        else
            table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.MenuDownloadUserAccess)), GetMarkup($"{false}"));

        // Client connection
        await FillTableRowsClientConAsync(table);

        await Task.CompletedTask;
    }

    private async Task FillTableRowsClientConSearchAsync(Table table)
    {
        // Client connection
        await FillTableRowsClientConAsync(table);

        // Check if the client is start monitoring chats
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuClientStateMonitoringChats)),
                GetMarkup(ClientMonitoringVm.IsStartMonitoring ? TgLocale.MenuYes : TgLocale.MenuNo));

        // Check if the client is start searching chats
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuClientStateSearchingChats)),
                GetMarkup(ClientMonitoringVm.IsStartSearching ? TgLocale.MenuYes : TgLocale.MenuNo));

        // Flag start monitoring or Flag start searching
        if (ClientMonitoringVm.IsStartMonitoring || ClientMonitoringVm.IsStartSearching)
        {
            // Send user
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuClientSendMessagesToUser)),
                GetMarkup(!ClientMonitoringVm.IsSendMessages 
                ? TgLocale.MenuNo
                : ClientMonitoringVm.IsSendToMyself ? TgLocale.MenuClientSendMessagesToMyself
                : $"{ClientMonitoringVm.UserName} with ID {ClientMonitoringVm.UserId}"));
            // Search chats
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuClientSearchChats)),
                GetMarkup(ClientMonitoringVm.IsSearchAtAllChats 
                ? TgLocale.MenuClientSearchAtAllChats : string.Join(", ", ClientMonitoringVm.ChatNames)));
            // Search keywords
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuKeywordsForMessageFiltering)),
                GetMarkup(ClientMonitoringVm.IsSkipKeywords
                    ? TgLocale.MenuClientSkipKeywords
                    : string.Join(", ", ClientMonitoringVm.Keywords)));
            // Catch messages count
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuClientCatchMessages)),
                GetMarkup(ClientMonitoringVm.CatchMessages.ToString()));
            // Last message
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuClientLastMessageDateTime)),
                GetMarkup(ClientMonitoringVm.LastDateTime));
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuClientLastMessageUserLink)),
                GetMarkup(ClientMonitoringVm.LastUserLink));
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuClientLastMessageLink)),
                GetMarkup(ClientMonitoringVm.LastMessageLink));
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuClientLastMessageText)),
                GetMarkup(ClientMonitoringVm.LastMessageText));
        }

        await Task.CompletedTask;
    }

    /// <summary> Chat info </summary>
    private async Task FillTableRowsDownloadedChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        if (!tgDownloadSettings.SourceVm.Dto.IsReady)
            table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.SettingsChat)),
                GetMarkup(TgLocale.SettingsIsNeedSetup));
        else
        {
            var chatDto = await BusinessLogicManager.StorageManager.SourceRepository.GetDtoAsync(x => x.Id == tgDownloadSettings.SourceVm.Dto.Id);
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.SettingsChat)),
                GetMarkup(chatDto.ToConsoleString()));
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.SettingsDtChanged)),
                GetMarkup(TgDataFormatUtils.GetDtFormat(chatDto.DtChanged)));
        }
    }

    /// <summary> Mark history read </summary>
    private async Task FillTableRowsMarkHistoryReadProgressAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuClientAdvancedMarkAllMessagesAsRead)),
            GetMarkup($"{TgLocale.MenuClientProgress} ..."));
        await Task.CompletedTask;
    }

    /// <summary> Mark history read </summary>
    private async Task FillTableRowsMarkHistoryReadCompleteAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuClientAdvancedMarkAllMessagesAsRead)),
            GetMarkup($"{TgLocale.MenuClientComplete} ..."));
        await Task.CompletedTask;
    }

    #endregion
}
