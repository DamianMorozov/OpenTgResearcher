// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Helpers;

public static class TgResourceExtensions
{
    #region Public and private fields, properties, constructor

    private static ResourceLoader LocalResourceLoader { get; } = new();

    public static string GetLocalizedResource(this string resourceKey) => LocalResourceLoader.GetString(resourceKey);

	#endregion

	#region Public and private methods

	public static string ActionStorageCreateBackupFailed() => "ActionStorageCreateBackupFailed".GetLocalizedResource();
	public static string ActionStorageCreateBackupFile() => "ActionStorageCreateBackupFile".GetLocalizedResource();
	public static string ActionStorageCreateBackupSuccess() => "ActionStorageCreateBackupSuccess".GetLocalizedResource();
	public static string ActionStorageResetAutoUpdateFailed() => "ActionStorageResetAutoUpdateFailed".GetLocalizedResource();
	public static string ActionStorageResetAutoUpdateSuccess() => "ActionStorageResetAutoUpdateSuccess".GetLocalizedResource();
	public static string AskActionStorageBackup() => "AskActionStorageBackup".GetLocalizedResource();
	public static string AskActionStorageClear() => "AskActionStorageClear".GetLocalizedResource();
	public static string AskActionStorageResetAutoUpdate() => "AskActionStorageResetAutoUpdate".GetLocalizedResource();
	public static string AskActionStorageShrink() => "AskActionStorageShrink".GetLocalizedResource();
	public static string AskAddRecord() => "AskAddRecord".GetLocalizedResource();
	public static string AskCalcChatStatistics() => "AskCalcChatStatistics".GetLocalizedResource();
	public static string AskCalcContentStatistics() => "AskCalcContentStatistics".GetLocalizedResource();
	public static string AskClientConnect() => "AskClientConnect".GetLocalizedResource();
	public static string AskClientDisconnect() => "AskClientDisconnect".GetLocalizedResource();
	public static string AskDataClear() => "AskDataClear".GetLocalizedResource();
	public static string AskDataLoad() => "AskDataLoad".GetLocalizedResource();
	public static string AskDeleteFile() => "AskDeleteFile".GetLocalizedResource();
	public static string AskGetParticipants() => "AskGetParticipants".GetLocalizedResource();
	public static string AskLicenseCheck() => "AskLicenseCheck".GetLocalizedResource();
	public static string AskLicenseClear() => "AskLicenseClear".GetLocalizedResource();
	public static string AskRestartApp() => "AskRestartApp".GetLocalizedResource();
	public static string AskSettingsClear() => "AskSettingsClear".GetLocalizedResource();
	public static string AskSettingsDefault() => "AskSettingsDefault".GetLocalizedResource();
	public static string AskSettingsDelete() => "AskSettingsDelete".GetLocalizedResource();
	public static string AskSettingsLoad() => "AskSettingsLoad".GetLocalizedResource();
	public static string AskSettingsSave() => "AskSettingsSave".GetLocalizedResource();
	public static string AskStopDownloading() => "AskStopDownloading".GetLocalizedResource();
	public static string AskUpdateChatDetails() => "AskUpdateChatDetails".GetLocalizedResource();
	public static string AskUpdateOnline() => "AskUpdateOnline".GetLocalizedResource();
	public static string AskUpdatePage() => "AskUpdatePage".GetLocalizedResource();
	public static string AskUpdatePreviewApp() => "AskUpdatePreviewApp".GetLocalizedResource();
	public static string AskUpdateReleaseApp() => "AskUpdateReleaseApp".GetLocalizedResource();
	public static string AssertionRestartApp() => "AssertionRestartApp".GetLocalizedResource();
	public static string ClientSettingsAreNotValid() => "ClientSettingsAreNotValid".GetLocalizedResource();
	public static string GetActionCheckLicenseMsg() => "ActionLicenseCheckMsg".GetLocalizedResource();
	public static string GetActionLicenseBuyMsg() => "ActionLicenseBuyMsg".GetLocalizedResource();
	public static string GetAppDisplayName() => "AppDisplayName".GetLocalizedResource();
	public static string GetAppLoading() => "AppLoading".GetLocalizedResource();
	public static string GetAppVersion() => "AppVersion".GetLocalizedResource();
	public static string GetCancelButton() => "CancelButton".GetLocalizedResource();
	public static string GetClientDataRequestEmptyResponse() => "ClientDataRequestEmptyResponse".GetLocalizedResource();
	public static string GetClientEnterLoginCode() => "ClientEnterLoginCode".GetLocalizedResource();
	public static string GetClientEnterPassword() => "ClientEnterPassword".GetLocalizedResource();
	public static string GetClientFloodWait() => "ClientFloodWait".GetLocalizedResource();
	public static string GetClientIsConnected() => "ClientIsConnected".GetLocalizedResource();
	public static string GetClientIsDisconnected() => "ClientIsDisconnected".GetLocalizedResource();
	public static string GetClipboard() => "Clipboard".GetLocalizedResource();
	public static string GetCloseButton() => "CloseButton".GetLocalizedResource();
	public static string GetError() => "Error".GetLocalizedResource();
	public static string GetErrorOccurredWhileLoadingLogs() => "ErrorOccurredWhileLoadingLogs".GetLocalizedResource();
	public static string GetInDevelopment() => "InDevelopment".GetLocalizedResource();
	public static string GetInstallerLoading() => "InstallerLoading".GetLocalizedResource();
	public static string GetLicenseFreeDescription() => "LicenseFreeDescription".GetLocalizedResource();
	public static string GetLicenseLoading() => "LicenseLoading".GetLocalizedResource();
	public static string GetLicensePaidDescription() => "LicensePaidDescription".GetLocalizedResource();
	public static string GetLicensePremiumDescription() => "LicensePremiumDescription".GetLocalizedResource();
	public static string GetLicenseTestDescription() => "LicenseTestDescription".GetLocalizedResource();
	public static string GetLoggerLoading() => "LoggerLoading".GetLocalizedResource();
	public static string GetManualDeleteFile(string fileName) => $"{"GetManualDeleteFile".GetLocalizedResource()}: {fileName}";
	public static string GetMenuClientIsQuery() => "MenuClientIsQuery".GetLocalizedResource();
	public static string GetMenuLicenseCheckServer() => "MenuLicenseCheckServer".GetLocalizedResource();
	public static string GetMenuLicenseIsNotCofirmed() => "MenuLicenseIsNotCofirmed".GetLocalizedResource();
	public static string GetMenuLicenseResponseStatusCode() => "MenuLicenseResponseStatusCode".GetLocalizedResource();
	public static string GetMenuLicenseUpdatedSuccessfully() => "MenuLicenseUpdatedSuccessfully".GetLocalizedResource();
	public static string GetMenuLicenseUserNotLoggedIn() => "MenuLicenseUserNotLoggedIn".GetLocalizedResource();
	public static string GetNoButton() => "NoButton".GetLocalizedResource();
	public static string GetNotificationLoading() => "NotificationLoading".GetLocalizedResource();
	public static string GetOkButton() => "OkButton".GetLocalizedResource();
	public static string GetRpcErrorFloodWait() => "RpcErrorFloodWait".GetLocalizedResource();
	public static string GetRpcErrorPasswordHashInvalid() => "RpcErrorPasswordHashInvalid".GetLocalizedResource();
	public static string GetRpcErrorPhoneCodeInvalid() => "RpcErrorPhoneCodeInvalid".GetLocalizedResource();
	public static string GetRpcErrorPhonePasswordFlood() => "RpcErrorPhonePasswordFlood".GetLocalizedResource();
	public static string GetSettingAppLanguageTooltip => "SettingAppLanguageTooltip".GetLocalizedResource();
	public static string GetSettingAppThemeTooltip => "SettingAppThemeTooltip".GetLocalizedResource();
	public static string GetSettingsThemeDark() => "SettingsThemeDark".GetLocalizedResource();
	public static string GetSettingsThemeDefault() => "SettingsThemeDefault".GetLocalizedResource();
	public static string GetSettingsThemeLight() => "SettingsThemeLight".GetLocalizedResource();
	public static string GetStorage() => "Storage".GetLocalizedResource();
	public static string GetStorageLoading() => "StorageLoading".GetLocalizedResource();
	public static string GetTableNameApps() => "TableNameApps".GetLocalizedResource();
	public static string GetTableNameChats() => "TableNameChats".GetLocalizedResource();
	public static string GetTableNameDocuments() => "TableNameDocuments".GetLocalizedResource();
	public static string GetTableNameFilters() => "TableNameFilters".GetLocalizedResource();
	public static string GetTableNameMessages() => "TableNameMessages".GetLocalizedResource();
	public static string GetTableNameProxies() => "TableNameProxies".GetLocalizedResource();
	public static string GetTableNameStories() => "TableNameStories".GetLocalizedResource();
	public static string GetTableNameUsers() => "TableNameUsers".GetLocalizedResource();
	public static string GetTableNameVersions() => "TableNameVersions".GetLocalizedResource();
	public static string GetTextBlockFiltered() => "TextBlockFiltered".GetLocalizedResource();
	public static string GetTextBlockLoaded() => "TextBlockLoaded".GetLocalizedResource();
	public static string GetTextBlockSubscribed() => "TextBlockSubscribed".GetLocalizedResource();
	public static string GetTextBlockTotalAmount() => "TextBlockTotalAmount".GetLocalizedResource();
	public static string GetTgClientLoading() => "TgClientLoading".GetLocalizedResource();
	public static string GetYesButton() => "YesButton".GetLocalizedResource();
	public static string ViewImage() => "ViewImage".GetLocalizedResource();

	#endregion
}
