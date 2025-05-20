// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Helpers;

/// <summary> Localization helper </summary>
public sealed class TgLocaleHelper : ObservableObject
{
	#region Design pattern "Lazy Singleton"

	private static TgLocaleHelper _instance = null!;
	public static TgLocaleHelper Instance => LazyInitializer.EnsureInitialized(ref _instance);

	#endregion

	#region App

	public string AppInfo => "App info";
	public string AppName => "Name";
	public string AppValue => "Value";
	public string AppVersion => "Application version";
	public string AppVersionShort => "Application";
	public string Exception => "Exception";
	public string FoundRows => "Found rows";
	public string From => "from";
	public string HomeResetToDefault => "Reset to default";
	public string HomeSaveToXml => "Save to XML";
	public string MenuLocateStorage => "Specify the path to the EF storage file";
	public string MenuLocateSession => "Specify the path to the session file";
	public string MenuAppUseProxy => "Using a proxy";
	public string MenuAppUseProxyDisable => "Disable proxy";
	public string MenuAppUseProxyEnable => "Enable proxy";
	public string MenuException => "Exception";
	public string Messages => "messages";
	public string ServerMessage => "Server message";
	public string SqliteDataSource => "Data Source";
	public string StorageVersion => "Storage version";
	public string StorageVersionShort => "Storage";
	public string UrlOpened => "URL opened";

	#endregion

	#region Advanced

	public string CollectChats => "Collect chats...";
	public string CollectContacts => "Collect contacts ...";
	public string CollectDialogs => "Collect dialogs ...";
	public string CollectStories => "Collect stories ...";

	#endregion

	#region Client

    public string MenuClientMessage => "Client message";
	public string MenuClientApiHash => "API hash";
	public string MenuClientApiId => "API ID";
	public string MenuClientComplete => "Complete";
	public string MenuClientConnect => "Connect the client to TG server";
	public string MenuClientConnectStatus => "Connect status";
	public string MenuClientDisconnect => "Disconnect the client from TG server";
	public string MenuClientFirstName => "First name";
	public string MenuClientIsConnected => "Client is connected";
	public string MenuClientIsDisconnected => "Client is disconnected";
	public string MenuClientIsQuery => "Client is query";
	public string MenuClientLastName => "Last name";
	public string MenuClientPassword => "Password";
	public string MenuClientPhoneNumber => "Phone number";
	public string MenuClientProgress => "Progress";
	public string MenuClientProxy => "Proxy";
	public string MenuClientVerificationCode => "Verification code";

    #endregion

    #region Download

    public string MenuClearAutoDownload => "Clear automatically updated chats (reset auto update field)";
    public string MenuStartAutoDownload => "Start downloading automatically updated chats";
	public string MenuAutoViewEvents => "Auto view events";
	public string MenuClearChats => "Clear all chats (delete from storage)";
	public string MenuClearConnectionData => "Clear connection data";
	public string MenuDownloadSetCountThreadsByFreeLicense => "Count of threads (1-10)";
	public string MenuDownloadSetCountThreadsByPaidLicense => "Count of threads (1-100)";
	public string MenuDownloadSetFolder => "Setup download folder";
	public string MenuDownloadSetIsAddMessageId => "Enable join message ID with file";
	public string MenuDownloadSetIsAutoUpdate => "Enable auto update";
	public string MenuDownloadSetIsCreatingSubdirectories => "Enable creating subdirectories";
	public string MenuDownloadSetIsFileNamingByMessage => "Enable naming media files by message";
	public string MenuDownloadSetIsRewriteFiles => "Enable rewrite exists files";
	public string MenuDownloadSetIsRewriteMessages => "Enable rewrite exists messages";
	public string MenuDownloadSetIsSaveMessages => "Enable save messages";
	public string MenuDownloadSetSource => "Setup source (ID/username)";
	public string MenuDownloadSetSourceFirstIdAuto => "Setup first ID auto";
	public string MenuDownloadSetSourceFirstIdManual => "Setup first ID manual";
	public string MenuDownloadUserAccess => "User access";
	public string MenuManualDownload => "Manual download";
	public string MenuMarkAllMessagesAsRead => "Mark all messages as read";
	public string MenuMarkAsRead => "Mark as read";
	public string MenuRegisterTelegramApp => "Register Telegram app";
	public string MenuSaveSettings => "Save settings";
	public string MenuSearchChats => "Search chats";
	public string MenuSearchContacts => "Search contacts";
	public string MenuSearchDialogs => "Search dialogs";
	public string MenuSearchStories => "Search stories";
	public string MenuSetProxy => "Setup proxy";
	public string MenuViewChats => "View chats";
	public string MenuViewContacts => "View contacts";
	public string MenuViewFilters => "View filters";
	public string MenuViewStories => "View stories";
	public string MenuViewVersions => "View versions";

	#endregion

	#region Filters

	public string MenuFiltersAdd => "Add filter";
	public string MenuFiltersAllCount => "All filters count";
	public string MenuFiltersEnabledCount => "Enabled filters count";
	public string MenuFiltersEdit => "Edit filter";
	public string MenuFiltersError => "Error";
	public string MenuFiltersRemove => "Remove filter";
	public string MenuFiltersClear => "Clear filters";
	public string MenuFiltersSetEnabled => "Set filter enabled";
	public string MenuFiltersSetIsEnabled => "Is enabled";
	public string MenuFiltersSetMask => "Set mask";
	public string MenuFiltersSetMaxSize => "File maximum size";
	public string MenuFiltersSetMinSize => "File minimum size";
	public string MenuFiltersSetMultiExtension => "Multi extension";
	public string MenuFiltersSetMultiName => "Multi name";
	public string MenuFiltersSetName => "Set name";
	public string MenuFiltersSetSingleExtension => "Single extension";
	public string MenuFiltersSetSingleName => "Single name";
	public string MenuFiltersSetSizeType => "Set file size type";
	public string MenuFiltersSetType => "Set filter type";
	public string MenuFiltersView => "View filters";

	#endregion

	#region License

	public string MenuLicenseBuy => "Buy license";
	public string MenuLicenseCheck => "Check current or get test license";
	public string MenuLicenseCheckServer => "Check license server";
	public string MenuLicenseCheckUrl => "Check URL";
	public string MenuLicenseCheckWithUserId => "To verify the license, the user ID is passed, continue";
	public string MenuLicenseClear => "Clear license";
	public string MenuLicenseCurrent => "Current license";
	public string MenuLicenseDescription => "License description";
	public string MenuLicenseEndpoint => "Endpoint";
	public string MenuLicenseExpiration => "Expiration";
	public string MenuLicenseFolowThisLink => "Follow this link";
	public string MenuLicenseInfo => "View license info";
	public string MenuLicenseIsConfirmed => "Confirmed";
	public string MenuLicenseIsNotCofirmed => "Is not cofirmed";
	public string MenuLicenseKey => "License key";
	public string MenuLicenseRequestError => "Request error";
	public string MenuLicenseResponseStatusCode => "Status code";
	public string MenuLicenseUpdatedSuccessfully => "License updated successfully";
	public string MenuLicenseUserNotLoggedIn => "User is not authorized, try connecting again";
	public string MenuUserId => "User ID";

	#endregion

	#region Update

	public string MenuUpdatePreviewCheck => "Checking for preview updates";
	public string MenuUpdateReleaseCheck => "Checking for release updates";

	#endregion

	#region Main menu

    public string MenuMainProxies => "Proxies";
    public string MenuMainSources => "Sources";
    public string MenuMainVersions => "Versions";
	public string MenuMain => "Main menu";
	public string MenuMainAdvanced => "Advanced";
	public string MenuMainApp => "Application";
	public string MenuMainApps => "Apps";
	public string MenuMainConnection => "Connection";
	public string MenuMainConnectionHealthCheck => "Connection health check";
	public string MenuMainDownload => "Download";
	public string MenuMainDownloadHealthCheck => "Download health check";
	public string MenuMainDownloads => "Downloads";
	public string MenuMainExit => "Exit";
	public string MenuMainFilters => "Filters";
	public string MenuMainLicense => "License";
	public string MenuMainReset => "Reset";
	public string MenuMainClearAppData => "Clear application data";
	public string MenuMainReturn => "Return";
	public string MenuMainSettings => "Settings";
	public string MenuMainStop => "Stop";
	public string MenuMainStorage => "Storage";
	public string MenuMainStorageHealthCheck => "Storage health check";
	public string MenuMainUpdate => "Update";

	#endregion

	#region Menu

	public string MenuNo => "No";
	public string MenuYes => "Yes";
	public string MenuSwitchNumber => "Switch menu number";
	public string MenuIsFalse => "False";
	public string MenuIsTrue => "True";

	#endregion

	#region Storage

	public string MenuStorageBackupDirectory => "Backup directory";
	public string MenuStorageBackupFailed => "Backup storage was failed";
	public string MenuStorageBackupFile => "Backup file";
	public string MenuStorageBackupSuccess => "Backup storage was successful";
	public string MenuStorageDbBackup => "Create backup";
	public string MenuStorageDbCreateNew => "Create new storage";
	public string MenuStorageDbClear => "Clear storage data";
	public string MenuStorageDbUpgradeUid => "Update the UID field";
	public string MenuStorageExitProgram => "Exit the program";
	public string MenuStoragePerformSteps => "Perform the following set of steps";
	public string MenuStorageTablesClear => "Clear tables";
	public string MenuStorageTablesClearFinished => "Clear tables was finished";
	public string MenuStorageTablesCompact => "Compact storage";
	public string MenuStorageTablesCompactFinished => "Compact storage was finished";
	public string MenuStorageTablesVersionsView => "Versions info";

	#endregion

	#region Public and private fields, properties, constructor

    public string TgDirectoryIsExists => "The directory is exist";
    public string TgDirectoryIsNotExists => "The directory is not exists";
	public string AddNew => "Add new";
	public string Clear => "Clear";
	public string ClearView => "Clear view";
	public string DirectoryCreate => "Create directory";
	public string DirectoryCreateIsException(Exception ex) => $"Exception of create directory: {(ex.InnerException is null ? ex.Message : ex.Message + $" | {ex.InnerException.Message}")}";
	public string DirectoryCurrent => "Current directory";
	public string DirectoryDestType => "Type destination directory";
	public string DirectoryIsNotExists(string dir = "") => string.IsNullOrEmpty(dir) ? "The directory is not exists!" : $"The directory \"{dir}\" is not exists!";
	public string EfStorage => "EF storage";
	public string Empty => "Empty";
	public string FileSession => "File session";
	public string InDevelopment => "In development";
	public string Load=> "Load";
	public string MoveUpDown => "(Move up and down to switch select)";
	public string ObjectHasBeenDisposedOff => "object has been disposed off";
	public string Save => "Save";
	public string SettingCheck => "Check";
	public string SettingName => "Setting";
	public string SettingsChat => "Chat info";
	public string SettingsContact => "Contact info";
	public string SettingsDtChanged => "Changed";
	public string SettingsIsNeedSetup => "Something is need setup";
	public string SettingsIsOk => "Everything is ok";
	public string SettingsStory => "Story info";
	public string SettingValue => "Value";
	public string SortView => "Sort view";
	public string StatusException => "Exception";
	public string StatusInnerException => "Inner exception";
	public string TablesAppsException => "Table APPS exception!";
	public string TablesDocumentsException => "Table DOCUMENTS exception!";
	public string TablesFiltersException => "Table FILTERS exception!";
	public string TablesMessagesException => "Table MESSAGES exception!";
	public string TablesProxiesException => "Table PROXIES exception!";
	public string TablesSourcesException => "Table SOURCES exception!";
	public string TablesVersionsException => "Table VERSIONS exception!";
	public string TgClientException => "Client exception";
	public string TgClientFix => "Client fix";
	public string TgClientFixTryToDeleteSession => "Try to delete session file";
	public string TgClientIsBot => "Is bot";
	public string TgClientProxyException => "Proxy exception";
	public string TgClientProxyHostName => "Proxy hostname";
	public string TgClientProxyPort => "Proxy port";
	public string TgClientProxySecret => "Proxy secret";
	public string TgClientProxySetup => "Proxy setup";
	public string TgClientProxyType => "Proxy type";
	public string TgClientSetupCompleteError => "The TG client setup was completed with errors";
	public string TgClientSetupCompleteSuccess => "The TG client setup was completed successfully";
	public string TgClientUseBot => "Use bot";
	public string TgClientBotToken => "Bot token";
	public string TgClientUserId => "User ID";
	public string TgClientUserIsActive => "User active";
	public string TgClientUserName => "User name";
	public string TgGetDialogsInfo => "Getting all dialogs info";
	public string TgMustClientConnect => "You must connect the client before";
	public string TgMustSetSettings => "You must setup the settings before";
	public string TgSettingsDestDirectory => "Destination";
	public string TgSettingsSourceFirstLastId => "First/last ID";
	public string TgSetupApiHash => "Type API hash";
	public string TgSetupAppId => "Type APP ID";
	public string TgSetupFirstName => "Type first name";
	public string TgSetupLastName => "Type last name";
	public string TgSetupNotifications => "Type use notifications";
	public string TgSetupPassword => "Type password";
	public string TgSetupPhone => "Type phone number";
	public string TgVerificationCode => "Verification code";
	public string TypeAnyKeyForReturn => "Type any key to return into the main menu";
	public string TypeTgProxyHostName => "Type the proxy host name or ip-address";
	public string TypeTgProxyPort => "Type the proxy port";
	public string TypeTgProxySecret => "Type the secret";
	public string TypeTgSourceFirstId => "Type the source first ID";
	public string WaitDownloadComplete => "Wait download complete";
	public string WaitDownloadCompleteWithQuit => "Wait download complete (q - quit)";

	#endregion

	#region Public and private fields, properties, constructor

	public string InfoMessage(string message, bool isUseX = false) => !isUseX ? $"[green]v {message}[/]" : $"[green]x {message}[/]";
	public string LinkWebSite(string linkText, string url) => $"\x1B]8;;{url}\x1B\\{linkText}\x1B]8;;\x1B\\";
	public string WarningMessage(string message) => $"[red]x {message}[/]";
	public TgEnumLanguage Language { get; set; } = TgEnumLanguage.Default;

	#endregion

	#region Menu storage

	public string MenuStorageDbIsNotFound(string fileName) => $"Storage was not found: {fileName}!";
	public string MenuStorageDbIsZeroSize(string fileName) => $"Storage is zero size: {fileName}!";
	public string MenuStorageDeleteExistsInfo(string fileName) => $"Manual delete the file: {fileName}";
	public string MenuStorageUpgradeUid(string fileName) => $"Update the UID field in the storage: {fileName}!";

	#endregion

	#region Fields

	public string FieldDescription => "Description";
    public string FieldFilterType => "Filter type";
    public string FieldHostname=> "Host name";
    public string FieldIsEnabled => "Enabled";
    public string FieldMask => "Mask";
    public string FieldName => "Name";
    public string FieldPort => "Port";
    public string FieldSize=> "Size";
    public string FieldSizeType => "Size type";
    public string FieldType => "Type";
    public string FieldUserName => "Username";
    public string FieldVersion => "Version";
	public string FieldCount => "Count";
	public string FieldFirstId => "First ID";
	public string FieldId => "ID";
	public string FieldTitle => "Title";

    #endregion

	#region System

    public string UseOverrideMethod => "Use override method!";

	#endregion

	#region Proxies

    public string ProxiesUserPassword => "Password";
    public string ProxiesUserSecret => "Secret";
    public string ProxyIsConnected => "Proxy is connected";
    public string ProxyIsDisconnect => "Proxy is disconnected";

	#endregion

	#region License

	public string LicenseFreeDescription => "Free license";
	public string LicensePaidDescription => "Paid license";
	public string LicensePremiumDescription => "Premium license";
	public string LicenseTestDescription => "Test license";
	public string LicenseVersionShort => "License";

	#endregion
}