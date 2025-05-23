﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

[DebuggerDisplay("{ToDebugString()}")]
internal sealed partial class TgMenuHelper : TgDisposable, ITgHelper
{
    #region Public and internal fields, properties, constructor

    internal static TgAppSettingsHelper TgAppSettings => TgAppSettingsHelper.Instance;
    internal static TgLocaleHelper TgLocale => TgLocaleHelper.Instance;
    internal static TgLogHelper TgLog => TgLogHelper.Instance;
    internal static Style StyleMain => new(Color.White, null, Decoration.Bold | Decoration.Conceal | Decoration.Italic);
    internal TgEnumMenuMain Value { get; set; }
    internal ITgBusinessLogicManager BusinessLogicManager { get; } = default!;

    public TgMenuHelper()
    {
        BusinessLogicManager = TgGlobalTools.Container.Resolve<ITgBusinessLogicManager>();
    }

    #endregion

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        //
    }

    /// <summary> Release unmanaged resources </summary>
    public override void ReleaseUnmanagedResources()
    {
        BusinessLogicManager.Dispose();
    }

    #region Public and internal methods

    public string ToDebugString() => TgConstants.UseOverrideMethod;

    internal Markup GetMarkup(string message, Style? style = null) => new(message, style);

    internal async Task SetStorageVersionAsync()
    {
        var version = (await BusinessLogicManager.StorageManager.VersionRepository.GetListAsync(TgEnumTableTopRecords.All, 0))
            .Items.Single(x => x.Version == BusinessLogicManager.StorageManager.VersionRepository.LastVersion);
        TgAppSettings.StorageVersion = $"v{version.Version}";
    }

    internal async Task ShowTableCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings, Action<Table> fillTableColumns,
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

    internal async Task ShowTableMainAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsMainAsync);

    internal async Task ShowTableStorageAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsStorageAsync);

    internal async Task ShowTableFiltersAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsFiltersAsync);

    internal async Task ShowTableLicenseFullInfoAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsLicenseFullInfoAsync);

    internal async Task ShowTableUpdateAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsUpdateAsync);

    internal async Task ShowTableApplicationAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsApplicationAsync);

    internal async Task ShowTableConnectionAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsConnectionAsync);

    internal async Task ShowTableDownloadAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsDownloadAsync);

    internal async Task ShowTableAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsAdvancedAsync);

    internal async Task ShowTableViewContactsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsViewDownloadedContacts);

    internal async Task ShowTableViewChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsViewDownloadedChatsAsync);

    internal async Task ShowTableViewStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsViewDownloadedStoriesAsync);

    internal async Task ShowTableViewVersionsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsViewDownloadedVersionsAsync);

    internal async Task ShowTableMarkHistoryReadProgressAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsMarkHistoryReadProgressAsync);

    internal async Task ShowTableMarkHistoryReadCompleteAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
        await ShowTableCoreAsync(tgDownloadSettings, FillTableColumns, FillTableRowsMarkHistoryReadCompleteAsync);

    internal void FillTableColumns(Table table)
    {
        if (table.Columns.Count > 0) return;
        table.AddColumn(new TableColumn(GetMarkup(TgLocale.AppName, StyleMain)) { Width = 23 }.LeftAligned());
        table.AddColumn(new TableColumn(GetMarkup(TgLocale.AppValue, StyleMain)) { Width = 77 }.LeftAligned());
    }


    internal async Task FillTableRowsEmpty(Table table)
    {
        //table.AddEmptyRow();
        await Task.CompletedTask;
    }

    internal async Task FillTableRowsMainAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
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

        // TG client settings
        table.AddRow(GetMarkup(BusinessLogicManager.ConnectClient.IsReady
            ? TgLocale.InfoMessage(TgLocale.MenuMainConnectionHealthCheck)
            : TgLocale.WarningMessage(TgLocale.MenuMainConnectionHealthCheck)),
            GetMarkup(BusinessLogicManager.ConnectClient.IsReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

        // Download settings
        table.AddRow(GetMarkup(tgDownloadSettings.SourceVm.Dto.IsReady
            ? TgLocale.InfoMessage(TgLocale.MenuMainDownloadHealthCheck)
            : TgLocale.WarningMessage(TgLocale.MenuMainDownloadHealthCheck)),
            GetMarkup(tgDownloadSettings.SourceVm.Dto.IsReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

        await Task.CompletedTask;
    }

    internal async Task FillTableRowsApplicationAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
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

        // Using a proxy
        if (TgAppSettings.IsUseProxy)
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuAppUseProxy)),
                GetMarkup(TgAppSettings.IsUseProxy.ToString()));
        else
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuAppUseProxy, true)),
                GetMarkup(TgAppSettings.IsUseProxy.ToString()));

        await Task.CompletedTask;
    }

    /// <summary> Storage settings </summary>
    internal async Task FillTableRowsStorageAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        table.AddRow(GetMarkup(TgGlobalTools.IsXmlReady
                ? TgLocale.InfoMessage(TgLocale.MenuMainStorage) : TgLocale.WarningMessage(TgLocale.MenuMainStorage)),
            GetMarkup(TgGlobalTools.IsXmlReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));
        await Task.CompletedTask;
    }

    /// <summary> Filters settings </summary>
    internal async Task FillTableRowsFiltersAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        var filters = (await BusinessLogicManager.StorageManager.FilterRepository.GetListAsync(TgEnumTableTopRecords.All, 0)).Items;
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuFiltersAllCount)),
            GetMarkup($"{filters.Count()}"));
    }

    /// <summary> License view info </summary>
    internal async Task FillTableRowsLicenseFullInfoAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuLicenseIsConfirmed)), GetMarkup(BusinessLogicManager.LicenseService.CurrentLicense.IsConfirmed.ToString()));
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuLicenseKey)), GetMarkup(BusinessLogicManager.LicenseService.CurrentLicense.GetLicenseKeyString()));
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuLicenseDescription)), GetMarkup(BusinessLogicManager.LicenseService.CurrentLicense.Description));
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuUserId)), GetMarkup(BusinessLogicManager.LicenseService.CurrentLicense.GetUserIdString()));
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuLicenseExpiration)), GetMarkup(BusinessLogicManager.LicenseService.CurrentLicense.GetValidToString()));
        await Task.CompletedTask;
    }

    /// <summary> Update settings </summary>
    internal async Task FillTableRowsUpdateAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        // App version
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.AppVersion)), GetMarkup(TgAppSettings.AppVersion));
        await Task.CompletedTask;
    }

    internal async Task FillTableRowsConnectionAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        // TG client settings
        table.AddRow(GetMarkup(BusinessLogicManager.ConnectClient.IsReady ?
            TgLocale.InfoMessage(TgLocale.MenuMainConnection) : TgLocale.WarningMessage(TgLocale.MenuMainConnection)),
            GetMarkup(BusinessLogicManager.ConnectClient.IsReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

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
            var app = (await BusinessLogicManager.StorageManager.AppRepository.GetCurrentAppAsync()).Item;
            if (!(await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(app.ProxyUid)).IsExists || BusinessLogicManager.ConnectClient.Me is null)
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
                    GetMarkup((await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(app.ProxyUid)).Item.Type.ToString()));
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxyHostName)),
                    GetMarkup((await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(app.ProxyUid)).Item.HostName));
                table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxyPort)),
                    GetMarkup((await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(app.ProxyUid)).Item.Port.ToString()));
                if (Equals((await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(app.ProxyUid)).Item.Type, TgEnumProxyType.MtProto))
                    table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientProxySecret)),
                        GetMarkup((await BusinessLogicManager.StorageManager.ProxyRepository.GetCurrentProxyAsync(app.ProxyUid)).Item.Secret));
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

        // Check paid license
        if (BusinessLogicManager.LicenseService.CurrentLicense.CheckPaidLicense())
        {
            var appDto = await BusinessLogicManager.StorageManager.AppRepository.GetCurrentDtoAsync();
            // Use bot
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientUseBot)), GetMarkup(appDto.UseBot.ToString()));
            // Bot token
            table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.TgClientBotToken)), GetMarkup(appDto.BotTokenKey));
        }

        await Task.CompletedTask;
    }

    /// <summary> Chat info </summary>
    internal async Task FillTableRowsDownloadedChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
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
    internal async Task FillTableRowsMarkHistoryReadProgressAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuMarkAllMessagesAsRead)),
            GetMarkup($"{TgLocale.MenuClientProgress} ..."));
        await Task.CompletedTask;
    }

    /// <summary> Mark history read </summary>
    internal async Task FillTableRowsMarkHistoryReadCompleteAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuMarkAllMessagesAsRead)),
            GetMarkup($"{TgLocale.MenuClientComplete} ..."));
        await Task.CompletedTask;
    }

    internal async Task FillTableRowsDownloadAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
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

    internal async Task FillTableRowsAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
    {
        // Enable auto update
        table.AddRow(GetMarkup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsAutoUpdate)),
            GetMarkup(tgDownloadSettings.SourceVm.Dto.IsAutoUpdate.ToString()));
        await Task.CompletedTask;
    }

    /// <summary> Contacts </summary>
    internal async Task FillTableRowsViewDownloadedContacts(TgDownloadSettingsViewModel tgDownloadSettings, Table table) =>
        await FillTableRowsEmpty(table);

    /// <summary> Chats </summary>
    internal async Task FillTableRowsViewDownloadedChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table) =>
        await FillTableRowsEmpty(table);

    /// <summary> Stories </summary>
    internal async Task FillTableRowsViewDownloadedStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table) =>
        await FillTableRowsEmpty(table);

    /// <summary> Versions </summary>
    internal async Task FillTableRowsViewDownloadedVersionsAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table) =>
        await FillTableRowsEmpty(table);

    public bool AskQuestionTrueFalseReturnPositive(string title, bool isTrueFirst = false)
    {
        var prompt = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"{title}?")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(isTrueFirst
                ? new() { TgLocale.MenuIsTrue, TgLocale.MenuIsFalse }
                : new List<string> { TgLocale.MenuIsFalse, TgLocale.MenuIsTrue }));
        return prompt.Equals(TgLocale.MenuIsTrue);
    }

    public bool AskQuestionTrueFalseReturnNegative(string question, bool isTrueFirst = false) =>
        !AskQuestionTrueFalseReturnPositive(question, isTrueFirst);

    public bool AskQuestionYesNoReturnPositive(string title, bool isYesFirst = false)
    {
        var prompt = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"{title}?")
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(isYesFirst
                ? new() { TgLocale.MenuYes, TgLocale.MenuNo }
                : new List<string> { TgLocale.MenuNo, TgLocale.MenuYes }));
        return prompt.Equals(TgLocale.MenuYes);
    }

    public bool AskQuestionYesNoReturnNegative(string question, bool isYesFirst = false) =>
        !AskQuestionYesNoReturnPositive(question, isYesFirst);

    public async Task<TgEfContactEntity> GetContactFromEnumerableAsync(string title, IEnumerable<TgEfContactEntity> items)
    {
        items = items.OrderBy(x => x.Id);
        List<string> list = [TgLocale.MenuMainReturn];
        list.AddRange(items.Select(item => TgLog.GetMarkupString(item.ToConsoleString())));
        var sourceString = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title(title)
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(list));
        if (!Equals(sourceString, TgLocale.MenuMainReturn))
        {
            var parts = sourceString.Split('|');
            if (parts.Length > 3)
            {
                var sourceId = parts[2].TrimEnd(' ');
                if (long.TryParse(sourceId, out var id))
                    return (await BusinessLogicManager.StorageManager.ContactRepository.GetAsync(new() { Id = id })).Item;
            }
        }
        return (await BusinessLogicManager.StorageManager.ContactRepository.GetNewAsync()).Item;
    }

    public async Task<TgEfFilterEntity> GetFilterFromEnumerableAsync(string title, IEnumerable<TgEfFilterEntity> items)
    {
        items = items.OrderBy(x => x.Name);
        List<string> list = [TgLocale.MenuMainReturn];
        list.AddRange(items.Select(item => TgLog.GetMarkupString(item.ToConsoleString())));
        var sourceString = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title(title)
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(list));
        if (!Equals(sourceString, TgLocale.MenuMainReturn))
        {
            var parts = sourceString.Split('|');
            if (parts.Length > 3)
            {
                var name = parts[0].TrimEnd(' ');
                return (await BusinessLogicManager.StorageManager.FilterRepository.GetAsync(new() { Name = name })).Item;
            }
        }
        return (await BusinessLogicManager.StorageManager.FilterRepository.GetNewAsync()).Item;
    }

    public async Task<TgEfSourceEntity> GetChatFromEnumerableAsync(string title, IEnumerable<TgEfSourceEntity> items)
    {
        items = items.OrderBy(x => x.UserName).ThenBy(x => x.Title);
        List<string> list = [TgLocale.MenuMainReturn, TgEfSourceEntity.ToHeaderString()];
        list.AddRange(items.Select(item => TgLog.GetMarkupString(item.ToConsoleString())));
        var sourceString = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title(title)
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(list));
        if (!Equals(sourceString, TgLocale.MenuMainReturn))
        {
            var parts = sourceString.Split('|');
            if (parts.Length != 0)
            {
                var sourceId = parts[0].TrimEnd(' ');
                if (long.TryParse(sourceId, out var id))
                    return await BusinessLogicManager.StorageManager.SourceRepository.GetItemAsync(new() { Id = id });
            }
        }
        return new();
    }

    public async Task<TgEfStoryEntity> GetStoryFromEnumerableAsync(string title, IEnumerable<TgEfStoryEntity> stories)
    {
        stories = stories.OrderBy(x => x.Id);
        List<string> list = [TgLocale.MenuMainReturn];
        list.AddRange(stories.Select(story => TgLog.GetMarkupString(story.ToConsoleString())));
        var storyString = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title(title)
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(list));
        if (!Equals(storyString, TgLocale.MenuMainReturn))
        {
            var parts = storyString.Split('|');
            if (parts.Length > 3)
            {
                var sourceId = parts[2].TrimEnd(' ');
                if (long.TryParse(sourceId, out var id))
                    return (await BusinessLogicManager.StorageManager.StoryRepository.GetAsync(new() { Id = id })).Item;
            }
        }
        return (await BusinessLogicManager.StorageManager.StoryRepository.GetNewAsync()).Item;
    }

    public void GetVersionFromEnumerable(string title, IEnumerable<TgEfVersionEntity> versions)
    {
        List<TgEfVersionEntity> versionsList = [.. versions.OrderBy(x => x.Version)];
        List<string> list = [TgLocale.MenuMainReturn];
        list.AddRange(versionsList.Select(version => TgLog.GetMarkupString(version.ToConsoleString())));
        AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title(title)
            .PageSize(Console.WindowHeight - 17)
            .AddChoices(list));
    }

    #endregion
}