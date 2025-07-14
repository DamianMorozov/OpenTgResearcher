// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace TgStorage.Utils;

/// <summary> SQL constants </summary>
public static class TgEfConstants
{
	#region Public and private fields, properties, constructor - Tables

	public const string TableApps = "APPS";
	public const string TableBots = "BOTS";
	public const string TableContacts = "CONTACTS";
	public const string TableDocuments = "DOCUMENTS";
	public const string TableFilters = "FILTERS";
	public const string TableLicenses = "LICENSES";
	public const string TableMessages = "MESSAGES";
	public const string TableProxies = "PROXIES";
	public const string TableSources = "SOURCES";
	public const string TableStories = "STORIES";
	public const string TableVersions = "VERSIONS";

    #endregion

	#region Public and private fields, properties, constructor - Columns

	public const string ColumnAbout = "ABOUT";
	public const string ColumnAccessHash = "ACCESS_HASH";
	public const string ColumnApiHash = "API_HASH";
	public const string ColumnApiId = "API_ID";
	public const string ColumnBotActiveUsers = "BOT_ACTIVE_USERS";
	public const string ColumnBotInfoVersion = "BOT_INFO_VERSION";
	public const string ColumnBotInlinePlaceholder = "BOT_INLINE_PLACEHOLDER";
	public const string ColumnBotToken = "BOT_TOKEN";
	public const string ColumnBotTokenKey = "BOT_TOKEN_KEY";
	public const string ColumnCaption = "CAPTION";
	public const string ColumnCount = "COUNT";
	public const string ColumnCountThreads = "COUNT_THREADS";
	public const string ColumnDate = "DATE";
	public const string ColumnDescription = "DESCRIPTION";
	public const string ColumnDirectory = "DIRECTORY";
	public const string ColumnDtChanged = "DT_CHANGED";
	public const string ColumnDtCreated = "DT_CREATED";
	public const string ColumnExpireDate = "EXPIRE_DATE";
	public const string ColumnFileName = "FILE_NAME";
	public const string ColumnFileSize = "FILE_SIZE";
	public const string ColumnFilterType = "FILTER_TYPE";
	public const string ColumnFirstId = "FIRST_ID";
	public const string ColumnFirstName = "FIRST_NAME";
	public const string ColumnFromId = "FROM_ID";
	public const string ColumnFromName = "FROM_NAME";
	public const string ColumnHostName = "HOST_NAME";
	public const string ColumnId = "ID";
	public const string ColumnIsActive = "IS_ACTIVE";
	public const string ColumnIsAutoUpdate = "IS_AUTO_UPDATE";
	public const string ColumnIsBot = "IS_BOT";
	public const string ColumnIsConfirmed = "IS_CONFIRMED";
	public const string ColumnIsCreatingSubdirectories = "IS_CREATING_SUBDIRS";
	public const string ColumnIsDeleted = "IS_DELETED";
	public const string ColumnIsEnabled = "IS_ENABLED";
	public const string ColumnIsFileNamingByMessage = "IS_FILE_NAMING_BY_MESSAGE";
	public const string ColumnIsRestrictSavingContent = "IS_RESTRICT_SAVING_CONTENT";
	public const string ColumnIsUserAccess = "IS_USER_ACCESS";
	public const string ColumnLangCode = "LANG_CODE";
	public const string ColumnLastName = "LAST_NAME";
	public const string ColumnLength = "LENGTH";
	public const string ColumnLicenseKey = "LICENSE_KEY";
	public const string ColumnLicenseType = "LICENSE_TYPE";
	public const string ColumnMask = "MASK";
	public const string ColumnMessage = "MESSAGE";
	public const string ColumnMessageId = "MESSAGE_ID";
	public const string ColumnName = "NAME";
	public const string ColumnOffset = "OFFSET";
	public const string ColumnPassword = "PASSWORD";
	public const string ColumnPhoneNumber = "PHONE_NUMBER";
	public const string ColumnPort = "PORT";
	public const string ColumnProxyUid = "PROXY_UID";
	public const string ColumnRestrictionReason = "RESTRICTION_REASON";
	public const string ColumnRowVersion = "RowVersion";
	public const string ColumnSecret = "SECRET";
	public const string ColumnSize = "SIZE";
	public const string ColumnSizeType = "SIZE_TYPE";
	public const string ColumnSourceId = "SOURCE_ID";
	public const string ColumnStatus = "STATUS";
	public const string ColumnStoriesMaxId = "STORIES_MAX_ID";
	public const string ColumnTitle = "TITLE";
	public const string ColumnType = "TYPE";
	public const string ColumnUid = "UID";
	public const string ColumnUseBot = "USE_BOT";
	public const string ColumnUseClient = "USE_CLIENT";
	public const string ColumnUserId = "USER_ID";
	public const string ColumnUserName = "USER_NAME";
	public const string ColumnUserNames = "USER_NAMES";
	public const string ColumnValidTo = "VALID_TO";
	public const string ColumnVersion = "VERSION";

	#endregion
}