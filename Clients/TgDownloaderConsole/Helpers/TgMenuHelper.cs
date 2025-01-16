﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace TgDownloaderConsole.Helpers;

[DebuggerDisplay("{ToDebugString()}")]
internal sealed partial class TgMenuHelper() : ITgHelper
{
	#region Public and internal fields, properties, constructor

	internal TgAppSettingsHelper TgAppSettings => TgAppSettingsHelper.Instance;
	internal TgLocaleHelper TgLocale => TgLocaleHelper.Instance;
	internal TgLogHelper TgLog => TgLogHelper.Instance;
	internal TgClientHelper TgClient => TgClientHelper.Instance;
	internal Style StyleMain => new(Color.White, null, Decoration.Bold | Decoration.Conceal | Decoration.Italic);
	internal TgEnumMenuMain Value { get; set; }
	private TgEfContext EfContext { get; } = TgEfUtils.CreateEfContext();
	private TgEfAppRepository AppRepository { get; } = new(TgEfUtils.EfContext);
	private TgEfContactRepository ContactRepository { get; } = new(TgEfUtils.EfContext);
	private TgEfDocumentRepository DocumentRepository { get; } = new(TgEfUtils.EfContext);
	private TgEfFilterRepository FilterRepository { get; } = new(TgEfUtils.EfContext);
	private TgEfMessageRepository MessageRepository { get; } = new(TgEfUtils.EfContext);
	private TgEfProxyRepository ProxyRepository { get; } = new(TgEfUtils.EfContext);
	private TgEfSourceRepository SourceRepository { get; } = new(TgEfUtils.EfContext);
	private TgEfStoryRepository StoryRepository { get; } = new(TgEfUtils.EfContext);
	private TgEfVersionRepository VersionRepository { get; } = new(TgEfUtils.EfContext);

	#endregion

	#region Public and internal methods

	public string ToDebugString() => TgLocale.UseOverrideMethod;

	internal async Task ShowTableCoreAsync(TgDownloadSettingsViewModel tgDownloadSettings, string title, Action<Table> fillTableColumns,
		Func<TgDownloadSettingsViewModel, Table, Task> fillTableRowsAsync)
	{
		AnsiConsole.Clear();
		AnsiConsole.Write(new FigletText(TgConstants.AppTitle).Centered().Color(Color.Yellow));
		Table table = new()
		{
			ShowHeaders = true,
			Border = TableBorder.Rounded,
			Title = new(title, Style.Plain),
		};

		fillTableColumns(table);

		if (table.Rows.Count > 0)
			table.Rows.Clear();
		await fillTableRowsAsync(tgDownloadSettings, table);

		table.Expand();
		AnsiConsole.Write(table);
	}

	internal async Task ShowTableMainAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
		await ShowTableCoreAsync(tgDownloadSettings, TgLocale.MenuMain, FillTableColumns, FillTableRowsMainAsync);

	internal async Task ShowTableStorageSettingsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
		await ShowTableCoreAsync(tgDownloadSettings, TgLocale.MenuMainStorage, FillTableColumns, FillTableRowsStorageAsync);

	internal async Task ShowTableFiltersSettingsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
		await ShowTableCoreAsync(tgDownloadSettings, TgLocale.MenuMainFilters, FillTableColumns, FillTableRowsFiltersAsync);

	internal async Task ShowTableAppSettingsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
		await ShowTableCoreAsync(tgDownloadSettings, TgLocale.MenuMainApp, FillTableColumns, FillTableRowsAppAsync);

	internal async Task ShowTableClientAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
		await ShowTableCoreAsync(tgDownloadSettings, TgLocale.MenuMainClient, FillTableColumns, FillTableRowsClientAsync);

	internal async Task ShowTableDownloadAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
		await ShowTableCoreAsync(tgDownloadSettings, TgLocale.MenuMainDownload, FillTableColumns, FillTableRowsDownloadAsync);

	internal async Task ShowTableAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
		await ShowTableCoreAsync(tgDownloadSettings, TgLocale.MenuMainAdvanced, FillTableColumns, FillTableRowsAdvancedAsync);

	internal async Task ShowTableViewContactsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
		await ShowTableCoreAsync(tgDownloadSettings, TgLocale.MenuMainAdvanced, FillTableColumns, FillTableRowsViewDownloadedContacts);

	internal async Task ShowTableViewSourcesAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
		await ShowTableCoreAsync(tgDownloadSettings, TgLocale.MenuMainAdvanced, FillTableColumns, FillTableRowsViewDownloadedChatsAsync);

	internal async Task ShowTableViewStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
		await ShowTableCoreAsync(tgDownloadSettings, TgLocale.MenuMainAdvanced, FillTableColumns, FillTableRowsViewDownloadedStoriesAsync);

	internal async Task ShowTableViewVersionsAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
		await ShowTableCoreAsync(tgDownloadSettings, TgLocale.MenuMainAdvanced, FillTableColumns, FillTableRowsViewDownloadedVersionsAsync);

	internal async Task ShowTableMarkHistoryReadProgressAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
		await ShowTableCoreAsync(tgDownloadSettings, TgLocale.MenuMainAdvanced, FillTableColumns, FillTableRowsMarkHistoryReadProgressAsync);

	internal async Task ShowTableMarkHistoryReadCompleteAsync(TgDownloadSettingsViewModel tgDownloadSettings) =>
		await ShowTableCoreAsync(tgDownloadSettings, TgLocale.MenuMainAdvanced, FillTableColumns, FillTableRowsMarkHistoryReadCompleteAsync);

	internal void FillTableColumns(Table table)
	{
		if (table.Columns.Count > 0) return;
		table.AddColumn(new TableColumn(new Markup(TgLocale.AppName, StyleMain)) { Width = 20 }.LeftAligned());
		table.AddColumn(new TableColumn(new Markup(TgLocale.AppValue, StyleMain)) { Width = 80 }.LeftAligned());
	}

	internal async Task FillTableRowsMainAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
	{
		// App version
		table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.AppVersion)), new Markup(TgAppSettings.AppVersion));
		// Storage version
		TgEfVersionEntity version = (await VersionRepository.GetListAsync(TgEnumTableTopRecords.All, 0)).
	            Items.Single(x => x.Version == VersionRepository.LastVersion);
		table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.StorageVersion)), new Markup($"v{version.Version}"));

		// App settings
		table.AddRow(new Markup(TgAppSettings.IsReady
				? TgLocale.InfoMessage(TgLocale.MenuMainApp) : TgLocale.WarningMessage(TgLocale.MenuMainApp)),
			new Markup(TgAppSettings.IsReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

		// Storage settings
		table.AddRow(new Markup(TgEfContext.IsXmlReady
				? TgLocale.InfoMessage(TgLocale.MenuMainStorage) : TgLocale.WarningMessage(TgLocale.MenuMainStorage)),
			new Markup(TgEfContext.IsXmlReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

		// TG client settings
		table.AddRow(new Markup(TgClient.IsReady ?
			TgLocale.InfoMessage(TgLocale.MenuMainClient) : TgLocale.WarningMessage(TgLocale.MenuMainClient)),
			new Markup(TgClient.IsReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

		// Download settings
		table.AddRow(new Markup(tgDownloadSettings.SourceVm.Dto.IsReady
			? TgLocale.InfoMessage(TgLocale.MenuMainDownload) : TgLocale.WarningMessage(TgLocale.MenuMainDownload)),
			new Markup(tgDownloadSettings.SourceVm.Dto.IsReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

		await Task.CompletedTask;
	}

	internal async Task FillTableRowsAppAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
	{
		// App xml settings
		table.AddRow(new Markup(TgAppSettings.IsReady ?
				TgLocale.InfoMessage(TgLocale.MenuMainApp) : TgLocale.WarningMessage(TgLocale.MenuMainApp)),
			new Markup(TgAppSettings.IsReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

		// File session is exists
		if (TgAppSettings.AppXml.IsExistsFileSession)
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.FileSession)),
				new Markup(TgLog.GetMarkupString(TgAppSettings.AppXml.XmlFileSession)));
		else
			table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.FileSession)),
				new Markup(TgLog.GetMarkupString(TgAppSettings.AppXml.XmlFileSession)));

		// File storage is existing
		if (TgAppSettings.AppXml.IsExistsEfStorage)
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.EfStorage)),
				new Markup(TgLog.GetMarkupString(TgAppSettings.AppXml.XmlEfStorage)));
		else
			table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.EfStorage)),
				new Markup(TgLog.GetMarkupString(TgAppSettings.AppXml.XmlEfStorage)));

		// Usage proxy
		if (TgAppSettings.IsUseProxy)
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.MenuAppUseProxy)),
				new Markup(TgLog.GetMarkupString(TgAppSettings.IsUseProxy.ToString())));
		else
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.MenuAppUseProxy, true)),
				new Markup(TgLog.GetMarkupString(TgAppSettings.IsUseProxy.ToString())));

		await Task.CompletedTask;
	}

	/// <summary>
	/// Storage settings.
	/// </summary>
	/// <param name="tgDownloadSettings"></param>
	/// <param name="table"></param>
	internal async Task FillTableRowsStorageAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
	{
		table.AddRow(new Markup(TgEfContext.IsXmlReady
				? TgLocale.InfoMessage(TgLocale.MenuMainStorage) : TgLocale.WarningMessage(TgLocale.MenuMainStorage)),
			new Markup(TgEfContext.IsXmlReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));
		await Task.CompletedTask;
	}

	/// <summary>
	/// Filters settings.
	/// </summary>
	/// <param name="tgDownloadSettings"></param>
	/// <param name="table"></param>
	internal async Task FillTableRowsFiltersAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
	{
		IEnumerable<TgEfFilterEntity> filters = (await FilterRepository.GetListAsync(TgEnumTableTopRecords.All, 0)).Items;
		table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.MenuFiltersAllCount)), 
			new Markup($"{filters.Count()}"));
	}

	internal async Task FillTableRowsClientAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
	{
		// TG client settings
		table.AddRow(new Markup(TgClient.IsReady ?
			TgLocale.InfoMessage(TgLocale.MenuMainClient) : TgLocale.WarningMessage(TgLocale.MenuMainClient)),
			new Markup(TgClient.IsReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

		if (TgClient.Me is null)
		{
			table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.TgClientUserName)),
				new Markup(TgLog.GetMarkupString(TgLocale.SettingsIsNeedSetup)));
			table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.TgClientUserId)),
				new Markup(TgLog.GetMarkupString(TgLocale.SettingsIsNeedSetup)));
			table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.TgClientUserIsActive)),
				new Markup(TgLog.GetMarkupString(TgLocale.SettingsIsNeedSetup)));
		}
		else
		{
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.TgClientUserName)),
				new Markup(TgLog.GetMarkupString(TgClient.Me.username)));
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.TgClientUserId)),
				new Markup(TgLog.GetMarkupString(TgClient.Me.id.ToString())));
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.TgClientUserIsActive)),
				new Markup(TgLog.GetMarkupString(TgClient.Me.IsActive.ToString())));
		}

		// Proxy setup.
		if (Equals(await ProxyRepository.GetCurrentProxyUidAsync(await AppRepository.GetCurrentAppAsync()), Guid.Empty))
		{
			if (TgAppSettings.IsUseProxy)
				table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.TgClientProxySetup)),
					new Markup(TgLog.GetMarkupString(TgLocale.SettingsIsNeedSetup)));
			else
				table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.TgClientProxySetup)),
					new Markup(TgLog.GetMarkupString(TgLocale.SettingsIsOk)));
		}
		else
		{
			// Proxy is not found.
			if (!(await ProxyRepository.GetCurrentProxyAsync(await AppRepository.GetCurrentAppAsync())).IsExists || TgClient.Me is null)
			{
				table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.TgClientProxySetup)),
					new Markup(TgLog.GetMarkupString(TgLocale.SettingsIsNeedSetup)));
				table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.TgClientProxyType)),
					new Markup(TgLog.GetMarkupString(TgLocale.SettingsIsNeedSetup)));
				table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.TgClientProxyHostName)),
					new Markup(TgLog.GetMarkupString(TgLocale.SettingsIsNeedSetup)));
				table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.TgClientProxyPort)),
					new Markup(TgLog.GetMarkupString(TgLocale.SettingsIsNeedSetup)));
				table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.TgClientProxySecret)),
					new Markup(TgLog.GetMarkupString(TgLocale.SettingsIsNeedSetup)));
			}
			// Proxy is found.
			else
			{
				table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.TgClientProxySetup)),
					new Markup(TgLog.GetMarkupString(TgLocale.SettingsIsOk)));
				table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.TgClientProxyType)),
					new Markup(TgLog.GetMarkupString((await ProxyRepository.GetCurrentProxyAsync(await AppRepository.GetCurrentAppAsync())).Item.Type.ToString())));
				table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.TgClientProxyHostName)),
					new Markup(TgLog.GetMarkupString((await ProxyRepository.GetCurrentProxyAsync(await AppRepository.GetCurrentAppAsync())).Item.HostName)));
				table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.TgClientProxyPort)),
					new Markup(TgLog.GetMarkupString((await ProxyRepository.GetCurrentProxyAsync(await AppRepository.GetCurrentAppAsync())).Item.Port.ToString())));
				if (Equals((await ProxyRepository.GetCurrentProxyAsync(await AppRepository.GetCurrentAppAsync())).Item.Type, TgEnumProxyType.MtProto))
					table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.TgClientProxySecret)),
						new Markup(TgLog.GetMarkupString((await ProxyRepository.GetCurrentProxyAsync(await AppRepository.GetCurrentAppAsync())).Item.Secret)));
			}
		}

		// Exceptions
		if (TgClient.ProxyException.IsExist)
		{
			table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.TgClientProxyException)),
				new Markup(TgLog.GetMarkupString(TgClient.ProxyException.Message)));
		}
		if (TgClient.ClientException.IsExist)
		{
			table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.TgClientException)),
				new Markup(TgLog.GetMarkupString(TgClient.ClientException.Message)));
			table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.TgClientFix)),
				new Markup(TgLog.GetMarkupString(TgLocale.TgClientFixTryToDeleteSession)));
		}

		await Task.CompletedTask;
	}

	/// <summary> Contact info </summary>
	internal async Task FillTableRowsDownloadedContactsAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
	{
		if (!tgDownloadSettings.ContactVm.Dto.IsReady)
			table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.SettingsContact)),
				new Markup(TgLocale.SettingsIsNeedSetup));
		else
		{
			var contact = (await ContactRepository.GetAsync(new() { Id = tgDownloadSettings.ContactVm.Dto.Id })).Item;
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.SettingsContact)),
				new Markup(TgLog.GetMarkupString(contact.ToConsoleString())));
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.SettingsDtChanged)),
				new Markup(TgDataFormatUtils.GetDtFormat(contact.DtChanged)));
		}
	}

	/// <summary> Chat info </summary>
	internal async Task FillTableRowsDownloadedChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
	{
		if (!tgDownloadSettings.SourceVm.Dto.IsReady)
			table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.SettingsChat)),
				new Markup(TgLocale.SettingsIsNeedSetup));
		else
		{
			var source = await SourceRepository.GetItemAsync(new() { Id = tgDownloadSettings.SourceVm.Dto.Id });
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.SettingsChat)),
				new Markup(TgLog.GetMarkupString(source.ToConsoleString())));
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.SettingsDtChanged)),
				new Markup(TgDataFormatUtils.GetDtFormat(source.DtChanged)));
		}
	}

	/// <summary> Story info </summary>
	internal async Task FillTableRowsDownloadedStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
	{
		if (!tgDownloadSettings.StoryVm.Dto.IsReady)
			table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.SettingsStory)),
				new Markup(TgLocale.SettingsIsNeedSetup));
		else
		{
			var story = (await StoryRepository.GetAsync(new() { Id = tgDownloadSettings.StoryVm.Dto.Id })).Item;
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.SettingsStory)),
				new Markup(TgLog.GetMarkupString(story.ToConsoleString())));
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.SettingsDtChanged)),
				new Markup(TgDataFormatUtils.GetDtFormat(story.DtChanged)));
		}
	}

	/// <summary> Version info </summary>
	internal async Task FillTableRowsDownloadedVersionsAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
	{
		if (!tgDownloadSettings.SourceVm.Dto.IsReady)
			table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.SettingsChat)),
				new Markup(TgLocale.SettingsIsNeedSetup));
		else
		{
			TgEfSourceEntity source = await SourceRepository.GetItemAsync(new() { Id = tgDownloadSettings.SourceVm.Dto.Id });
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.SettingsChat)),
				new Markup(TgLog.GetMarkupString(source.ToConsoleString())));
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.SettingsDtChanged)),
				new Markup(TgDataFormatUtils.GetDtFormat(source.DtChanged)));
		}
	}

	/// <summary> Mark history read </summary>
	internal async Task FillTableRowsMarkHistoryReadProgressAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
	{
		table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.MenuMarkAllMessagesAsRead)),
			new Markup($"{TgLocale.MenuClientProgress} ..."));
		await Task.CompletedTask;
	}

	/// <summary> Mark history read </summary>
	internal async Task FillTableRowsMarkHistoryReadCompleteAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
	{
		table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.MenuMarkAllMessagesAsRead)),
			new Markup($"{TgLocale.MenuClientComplete} ..."));
		await Task.CompletedTask;
	}

	internal async Task FillTableRowsDownloadAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
	{
		// Download
		table.AddRow(new Markup(tgDownloadSettings.SourceVm.Dto.IsReady
				? TgLocale.InfoMessage(TgLocale.MenuMainDownload) : TgLocale.WarningMessage(TgLocale.MenuMainDownload)),
			new Markup(tgDownloadSettings.SourceVm.Dto.IsReady ? TgLocale.SettingsIsOk : TgLocale.SettingsIsNeedSetup));

		// Source info
		await FillTableRowsDownloadedChatsAsync(tgDownloadSettings, table);

		// Destination dir
		if (string.IsNullOrEmpty(tgDownloadSettings.SourceVm.Dto.Directory))
			table.AddRow(new Markup(TgLocale.WarningMessage(TgLocale.TgSettingsDestDirectory)),
				new Markup(TgLocale.SettingsIsNeedSetup));
		else
			table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.TgSettingsDestDirectory)),
				new Markup(TgLogHelper.Instance.GetMarkupString(tgDownloadSettings.SourceVm.Dto.Directory, isReplaceSpec: true)));

		// First/last ID
		table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.TgSettingsSourceFirstLastId)),
			new Markup($"{tgDownloadSettings.SourceVm.Dto.FirstId} / {tgDownloadSettings.SourceVm.Dto.Count}"));

		// Rewrite files
		table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.TgSettingsIsRewriteFiles)),
			new Markup(tgDownloadSettings.IsRewriteFiles.ToString()));

		// Rewrite messages
		table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.TgSettingsIsRewriteMessages)),
			new Markup(tgDownloadSettings.IsRewriteMessages.ToString()));

		// Join message ID
		table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.TgSettingsIsJoinFileNameWithMessageId)),
			new Markup(tgDownloadSettings.IsJoinFileNameWithMessageId.ToString()));

		// Enable auto update
		table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsAutoUpdate)),
			new Markup(tgDownloadSettings.SourceVm.Dto.IsAutoUpdate.ToString()));

        // Enabled filters
        IEnumerable<TgEfFilterEntity> filters = (await FilterRepository.GetListAsync(TgEnumTableTopRecords.All, 0))
	        .Items.Where(f => f.IsEnabled);
		table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.MenuFiltersEnabledCount)), new Markup($"{filters.Count()}"));

        // Count of threads
		table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetCountThreads)), new Markup($"{tgDownloadSettings.CountThreads}"));

		await Task.CompletedTask;
	}

	internal async Task FillTableRowsAdvancedAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table)
	{
		// Is auto update.
		table.AddRow(new Markup(TgLocale.InfoMessage(TgLocale.MenuDownloadSetIsAutoUpdate)),
			new Markup(tgDownloadSettings.SourceVm.Dto.IsAutoUpdate.ToString()));
		await Task.CompletedTask;
	}

	/// <summary> User ID/username </summary>
	internal async Task FillTableRowsViewDownloadedContacts(TgDownloadSettingsViewModel tgDownloadSettings, Table table) =>
		await FillTableRowsDownloadedContactsAsync(tgDownloadSettings, table);

	/// <summary> Source ID/username </summary>
	internal async Task FillTableRowsViewDownloadedChatsAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table) => 
        await FillTableRowsDownloadedChatsAsync(tgDownloadSettings, table);

    /// <summary> User ID/username </summary>
    internal async Task FillTableRowsViewDownloadedStoriesAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table) => 
        await FillTableRowsDownloadedStoriesAsync(tgDownloadSettings, table);

    /// <summary> Version ID/username </summary>
    internal async Task FillTableRowsViewDownloadedVersionsAsync(TgDownloadSettingsViewModel tgDownloadSettings, Table table) => 
        await FillTableRowsDownloadedVersionsAsync(tgDownloadSettings, table);

    public bool AskQuestionReturnPositive(string title, bool isTrueFirst = false)
	{
		string prompt = AnsiConsole.Prompt(new SelectionPrompt<string>()
			.Title($"{title}?")
			.PageSize(Console.WindowHeight - 17)
			.AddChoices(isTrueFirst
				? new() { TgLocale.MenuIsTrue, TgLocale.MenuIsFalse }
				: new List<string> { TgLocale.MenuIsFalse, TgLocale.MenuIsTrue }));
		return prompt.Equals(TgLocale.MenuIsTrue);
    }

	public bool AskQuestionReturnNegative(string question, bool isTrueFirst = false) =>
		!AskQuestionReturnPositive(question, isTrueFirst);

	public async Task<TgEfContactEntity> GetContactFromEnumerableAsync(string title, IEnumerable<TgEfContactEntity> items)
	{
		items = items.OrderBy(x => x.Id);
		List<string> list = [TgLocale.MenuMainReturn];
		list.AddRange(items.Select(item => TgLog.GetMarkupString(item.ToConsoleString())));
		string sourceString = AnsiConsole.Prompt(new SelectionPrompt<string>()
			.Title(title)
			.PageSize(Console.WindowHeight - 17)
			.AddChoices(list));
		if (!Equals(sourceString, TgLocale.MenuMainReturn))
		{
			string[] parts = sourceString.Split('|');
			if (parts.Length > 3)
			{
				string sourceId = parts[2].TrimEnd(' ');
				if (long.TryParse(sourceId, out long id))
					return (await ContactRepository.GetAsync(new() { Id = id })).Item;
			}
		}
		return (await ContactRepository.GetNewAsync()).Item;
	}

	public async Task<TgEfFilterEntity> GetFilterFromEnumerableAsync(string title, IEnumerable<TgEfFilterEntity> items)
	{
		items = items.OrderBy(x => x.Name);
		List<string> list = [TgLocale.MenuMainReturn];
		list.AddRange(items.Select(item => TgLog.GetMarkupString(item.ToConsoleString())));
		string sourceString = AnsiConsole.Prompt(new SelectionPrompt<string>()
			.Title(title)
			.PageSize(Console.WindowHeight - 17)
			.AddChoices(list));
		if (!Equals(sourceString, TgLocale.MenuMainReturn))
		{
			string[] parts = sourceString.Split('|');
			if (parts.Length > 3)
			{
				string name = parts[0].TrimEnd(' ');
				return (await FilterRepository.GetAsync(new() { Name = name })).Item;
			}
		}
		return (await FilterRepository.GetNewAsync()).Item;
	}

	public async Task<TgEfSourceEntity> GetSourceFromEnumerableAsync(string title, IEnumerable<TgEfSourceEntity> items)
	{
		items = items.OrderBy(x => x.UserName).ThenBy(x => x.Title);
		List<string> list = [TgLocale.MenuMainReturn];
		list.AddRange(items.Select(item => TgLog.GetMarkupString(item.ToConsoleString())));
		string sourceString = AnsiConsole.Prompt(new SelectionPrompt<string>()
			.Title(title)
			.PageSize(Console.WindowHeight - 17)
			.AddChoices(list));
		if (!Equals(sourceString, TgLocale.MenuMainReturn))
		{
			string[] parts = sourceString.Split('|');
			if (parts.Length != 0)
			{
				string sourceId = parts[0].TrimEnd(' ');
				if (long.TryParse(sourceId, out long id))
					return await SourceRepository.GetItemAsync(new() { Id = id });
			}
		}
		return new();
	}

	public async Task<TgEfStoryEntity> GetStoryFromEnumerableAsync(string title, IEnumerable<TgEfStoryEntity> stories)
	{
		stories = stories.OrderBy(x => x.Id);
		List<string> list = [TgLocale.MenuMainReturn];
		list.AddRange(stories.Select(story => TgLog.GetMarkupString(story.ToConsoleString())));
		string storyString = AnsiConsole.Prompt(new SelectionPrompt<string>()
			.Title(title)
			.PageSize(Console.WindowHeight - 17)
			.AddChoices(list));
		if (!Equals(storyString, TgLocale.MenuMainReturn))
		{
			string[] parts = storyString.Split('|');
			if (parts.Length > 3)
			{
				string sourceId = parts[2].TrimEnd(' ');
				if (long.TryParse(sourceId, out long id))
					return (await StoryRepository.GetAsync(new() { Id = id })).Item;
			}
		}
		return (await StoryRepository.GetNewAsync()).Item;
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