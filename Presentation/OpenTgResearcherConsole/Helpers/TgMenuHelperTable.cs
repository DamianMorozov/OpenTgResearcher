// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal partial class TgMenuHelper
{
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

    public async Task ShowTableMainAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsMainAsync);

    private async Task ShowTableStorageAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsStorageAsync);

    private async Task ShowTableFiltersAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsFiltersAsync);

    private async Task ShowTableLicenseFullInfoAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsLicenseFullInfoAsync);

    private async Task ShowTableUpdateAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsUpdateAsync);

    private async Task ShowTableApplicationAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsApplicationAsync);

    private async Task ShowTableBotConnectionAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsBotConnectionAsync);

    private async Task ShowTableClientConnectionAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsClientConnectionAsync);

    private async Task ShowTableDownloadAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsDownloadAsync);

    private async Task ShowTableAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsAdvancedAsync);

    private async Task ShowTableViewContactsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsViewDownloadedContacts);

    private async Task ShowTableViewChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsViewDownloadedChatsAsync);

    private async Task ShowTableViewStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsViewDownloadedStoriesAsync);

    private async Task ShowTableViewVersionsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsViewDownloadedVersionsAsync);

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
        // App version
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.AppVersion)), GetMarkup(TgAppSettings.AppVersion));
        // Storage version
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.StorageVersionShort)), GetMarkup(TgAppSettings.StorageVersion));
        // License version
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.LicenseVersionShort)), GetMarkup(BusinessLogicManager.LicenseService.CurrentLicense.Description));

        // App settings
        var isReady = TgAppSettings.AppXml.IsExistsFileSession && TgAppSettings.AppXml.IsExistsEfStorage;
        table.AddRow(GetMarkup(isReady ? TgLocale.InfoMessage(TgLocale.MenuMainApp) : TgLocale.WarningMessage(TgLocale.MenuMainApp)),
            GetMarkup(isReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

        // Storage settings
        table.AddRow(GetMarkup(TgGlobalTools.IsXmlReady
                ? TgLocale.InfoMessage(TgLocale.MenuMainStorageHealthCheck)
                : TgLocale.WarningMessage(TgLocale.MenuMainStorageHealthCheck)),
            GetMarkup(TgGlobalTools.IsXmlReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

        var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();
        if (!appDto.UseBot)
        {
            // TG client settings
            var clientConnectReady = BusinessLogicManager.ConnectClient.IsReady;
            table.AddRow(GetMarkup(clientConnectReady
                ? TgLocale.InfoMessage(TgLocale.MenuMainClientConnectionHealthCheck)
                : TgLocale.WarningMessage(TgLocale.MenuMainClientConnectionHealthCheck)),
                GetMarkup(clientConnectReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));
        }
        else
        {
            // TG bot settings
            var isBotConnectionReady = await BusinessLogicManager.ConnectClient.CheckBotConnectionReadyAsync();
            table.AddRow(GetMarkup(isBotConnectionReady
                ? TgLocale.InfoMessage(TgLocale.MenuMainBotConnectionHealthCheck)
                : TgLocale.WarningMessage(TgLocale.MenuMainBotConnectionHealthCheck)),
                GetMarkup(isBotConnectionReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));
        }

        // Download settings
        table.AddRow(GetMarkup(tgDownloadSettings.SourceVm.Dto.IsReady
            ? TgLocale.InfoMessage(TgLocale.MenuMainDownloadHealthCheck)
            : TgLocale.WarningMessage(TgLocale.MenuMainDownloadHealthCheck)),
            GetMarkup(tgDownloadSettings.SourceVm.Dto.IsReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

        await Task.CompletedTask;
    }

    private async Task FillTableRowsApplicationAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        // Current dirictory
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.DirectoryCurrent)), GetMarkup(Environment.CurrentDirectory));

        // Storage
        var fileSize = TgFileUtils.GetFileSizeString(TgFileUtils.CalculateFileSize(TgAppSettings.AppXml.XmlEfStorage));
        table.AddRow(TgAppSettings.AppXml.IsExistsEfStorage
            ? GetMarkup(TgLocale.InfoMessage(TgLocale.EfStorage)) : GetMarkup(TgLocale.WarningMessage(TgLocale.EfStorage)),
            GetMarkup($"{Path.Combine(Environment.CurrentDirectory, TgAppSettings.AppXml.XmlEfStorage)} ({fileSize})"));

        // Session
        fileSize = TgFileUtils.GetFileSizeString(TgFileUtils.CalculateFileSize(TgAppSettings.AppXml.XmlFileSession));
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

    /// <summary> Storage settings </summary>
    private async Task FillTableRowsStorageAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        table.AddRow(GetMarkup(TgGlobalTools.IsXmlReady
                ? TgLocale.InfoMessage(TgLocale.MenuMainStorage) : TgLocale.WarningMessage(TgLocale.MenuMainStorage)),
            GetMarkup(TgGlobalTools.IsXmlReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));
        await Task.CompletedTask;
    }

    /// <summary> Filters settings </summary>
    private async Task FillTableRowsFiltersAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        var filters = (await BusinessLogicManager.StorageManager.FilterRepository.GetListAsync(TgEnumTableTopRecords.All, 0)).Items;
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuFiltersAllCount)),
            GetMarkup($"{filters.Count()}"));
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

    private async Task FillTableRowsBotConnectionAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();
        var isBotConnectionReady = await BusinessLogicManager.ConnectClient.CheckBotConnectionReadyAsync();

        // Use bot
        if (appDto.UseBot)
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotUseBot)), GetMarkup(appDto.UseBot.ToString()));
        else
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotUseBot, isUseX: true)), GetMarkup(appDto.UseBot.ToString()));

        // Bot token
        if (!string.IsNullOrEmpty(appDto.BotTokenKey))
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotToken)), GetMarkup(appDto.BotTokenKey));
        else
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotToken, isUseX: true)), GetMarkup(appDto.BotTokenKey));

        await Task.CompletedTask;
    }

    private async Task FillTableRowsClientConnectionAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();
        var isClientConnectionReady = await BusinessLogicManager.ConnectClient.CheckClientConnectionReadyAsync();

        // Use client
        if (appDto.UseClient)
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotUseClient)), GetMarkup(appDto.UseClient.ToString()));
        else
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuBotUseClient, isUseX: true)), GetMarkup(appDto.UseClient.ToString()));

        // TG client settings
        table.AddRow(GetMarkup(isClientConnectionReady
            ? TgLocale.InfoMessage(TgLocale.MenuMainClientConnection)
            : TgLocale.WarningMessage(TgLocale.MenuMainClientConnection)),
                GetMarkup(isClientConnectionReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

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
        if (Equals(await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyUidAsync(
            await BusinessLogicManager.StorageManager.AppRepository.GetCurrentAppAsync()), Guid.Empty))
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

    /// <summary> Chat info </summary>
    private async Task FillTableRowsDownloadedChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        if (!tgDownloadSettings.SourceVm.Dto.IsReady)
            table.AddRow(GetMarkup(TgLocale.WarningMessage(TgLocale.SettingsChat)),
                GetMarkup(TgLocale.SettingsIsNeedSetup));
        else
        {
            var source = await BusinessLogicManager.StorageManager.SourceRepository.GetItemAsync(new() { Id = tgDownloadSettings.SourceVm.Dto.Id });
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.SettingsChat)),
                GetMarkup(source.ToConsoleString()));
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.SettingsDtChanged)),
                GetMarkup(TgDataFormatUtils.GetDtFormat(source.DtChanged)));
        }
    }

    /// <summary> Mark history read </summary>
    private async Task FillTableRowsMarkHistoryReadProgressAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuClientMarkAllMessagesAsRead)),
            GetMarkup($"{TgLocale.MenuClientProgress} ..."));
        await Task.CompletedTask;
    }

    /// <summary> Mark history read </summary>
    private async Task FillTableRowsMarkHistoryReadCompleteAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuClientMarkAllMessagesAsRead)),
            GetMarkup($"{TgLocale.MenuClientComplete} ..."));
        await Task.CompletedTask;
    }

    private async Task FillTableRowsDownloadAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        // Download
        table.AddRow(GetMarkup(tgDownloadSettings.SourceVm.Dto.IsReady
                ? TgLocale.InfoMessage(TgLocale.MenuMainDownload) : TgLocale.WarningMessage(TgLocale.MenuMainDownload)),
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

        await Task.CompletedTask;
    }

    private async Task FillTableRowsAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        // Enable auto update
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsAutoUpdate)),
            GetMarkup(tgDownloadSettings.SourceVm.Dto.IsAutoUpdate.ToString()));
        await Task.CompletedTask;
    }

    /// <summary> Contacts </summary>
    private async Task FillTableRowsViewDownloadedContacts(TgDownloadSettingsViewModel tgDownloadSettings, Table table) =>
        await FillTableRowsEmpty(table);

    /// <summary> Chats </summary>
    private async Task FillTableRowsViewDownloadedChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table) =>
        await FillTableRowsEmpty(table);

    /// <summary> Stories </summary>
    private async Task FillTableRowsViewDownloadedStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table) =>
        await FillTableRowsEmpty(table);

    /// <summary> Versions </summary>
    private async Task FillTableRowsViewDownloadedVersionsAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table) =>
        await FillTableRowsEmpty(table);
}
