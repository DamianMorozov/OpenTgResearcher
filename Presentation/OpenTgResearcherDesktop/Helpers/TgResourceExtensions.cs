// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com


namespace OpenTgResearcherDesktop.Helpers;

public static class TgResourceExtensions
{
    #region Fields, properties, constructor

    private static ResourceLoader LocalResourceLoader { get; } = new();

    internal static string GetLocalizedResource(this string resourceKey) => LocalResourceLoader.GetString(resourceKey);

	#endregion

	#region Methods

    internal static string GetProcessingChats() => "ProcessingChats".GetLocalizedResource();
    internal static string GetProcessingDialogs() => "ProcessingDialogs".GetLocalizedResource();
    internal static string GetProcessingGroups() => "ProcessingGroups".GetLocalizedResource();
    internal static string GetStartScan() => "StartScan".GetLocalizedResource();
    internal static string GetStopScan() => "StopScan".GetLocalizedResource();
	internal static string ActionStorageCreateBackupFailed() => "ActionStorageCreateBackupFailed".GetLocalizedResource();
	internal static string ActionStorageCreateBackupFile() => "ActionStorageCreateBackupFile".GetLocalizedResource();
	internal static string ActionStorageCreateBackupSuccess() => "ActionStorageCreateBackupSuccess".GetLocalizedResource();
	internal static string ActionStorageResetAutoUpdateFailed() => "ActionStorageResetAutoUpdateFailed".GetLocalizedResource();
	internal static string ActionStorageResetAutoUpdateSuccess() => "ActionStorageResetAutoUpdateSuccess".GetLocalizedResource();
	internal static string AskActionStorageBackup() => "AskActionStorageBackup".GetLocalizedResource();
	internal static string AskActionStorageClear() => "AskActionStorageClear".GetLocalizedResource();
	internal static string AskActionStorageResetAutoUpdate() => "AskActionStorageResetAutoUpdate".GetLocalizedResource();
	internal static string AskActionStorageShrink() => "AskActionStorageShrink".GetLocalizedResource();
	internal static string AskAddRecord() => "AskAddRecord".GetLocalizedResource();
	internal static string AskCalcChatStatistics() => "AskCalcChatStatistics".GetLocalizedResource();
	internal static string AskCalcContentStatistics() => "AskCalcContentStatistics".GetLocalizedResource();
	internal static string AskClientConnect() => "AskClientConnect".GetLocalizedResource();
	internal static string AskClientDisconnect() => "AskClientDisconnect".GetLocalizedResource();
	internal static string AskDataClear() => "AskDataClear".GetLocalizedResource();
	internal static string AskDataLoad() => "AskDataLoad".GetLocalizedResource();
	internal static string AskDeleteFile() => "AskDeleteFile".GetLocalizedResource();
	internal static string AskGetParticipants() => "AskGetParticipants".GetLocalizedResource();
	internal static string AskLicenseCheck() => "AskLicenseCheck".GetLocalizedResource();
	internal static string AskLicenseClear() => "AskLicenseClear".GetLocalizedResource();
	internal static string AskRestartApp() => "AskRestartApp".GetLocalizedResource();
	internal static string AskSettingsClear() => "AskSettingsClear".GetLocalizedResource();
	internal static string AskSettingsDefault() => "AskSettingsDefault".GetLocalizedResource();
	internal static string AskSettingsDelete() => "AskSettingsDelete".GetLocalizedResource();
	internal static string AskSettingsLoad() => "AskSettingsLoad".GetLocalizedResource();
	internal static string AskSettingsSave() => "AskSettingsSave".GetLocalizedResource();
	internal static string AskStartMonitoring() => "AskStartMonitoring".GetLocalizedResource();
	internal static string AskStopDownloading() => "AskStopDownloading".GetLocalizedResource();
	internal static string AskStopMonitoring() => "AskStopMonitoring".GetLocalizedResource();
	internal static string AskUpdateChatDetails() => "AskUpdateChatDetails".GetLocalizedResource();
	internal static string AskUpdateOnline() => "AskUpdateOnline".GetLocalizedResource();
	internal static string AskUpdatePage() => "AskUpdatePage".GetLocalizedResource();
	internal static string AskUpdatePreviewApp() => "AskUpdatePreviewApp".GetLocalizedResource();
	internal static string AskUpdateReleaseApp() => "AskUpdateReleaseApp".GetLocalizedResource();
	internal static string AssertionRestartApp() => "AssertionRestartApp".GetLocalizedResource();
	internal static string ClientSettingsAreNotValid() => "ClientSettingsAreNotValid".GetLocalizedResource();
	internal static string GetActionCheckLicenseMsg() => "ActionLicenseCheckMsg".GetLocalizedResource();
	internal static string GetActionLicenseBuyMsg() => "ActionLicenseBuyMsg".GetLocalizedResource();
	internal static string GetAppDisplayName() => "AppDisplayName".GetLocalizedResource();
	internal static string GetAppLoading() => "AppLoading".GetLocalizedResource();
	internal static string GetAppVersion() => "AppVersion".GetLocalizedResource();
	internal static string GetCancelButton() => "CancelButton".GetLocalizedResource();
	internal static string GetClientDataRequestEmptyResponse() => "ClientDataRequestEmptyResponse".GetLocalizedResource();
	internal static string GetClientEnterLoginCode() => "ClientEnterLoginCode".GetLocalizedResource();
	internal static string GetClientEnterPassword() => "ClientEnterPassword".GetLocalizedResource();
	internal static string GetClientFloodWait() => "ClientFloodWait".GetLocalizedResource();
	internal static string GetClientIsConnected() => "ClientIsConnected".GetLocalizedResource();
	internal static string GetClientIsDisconnected() => "ClientIsDisconnected".GetLocalizedResource();
	internal static string GetClipboard() => "Clipboard".GetLocalizedResource();
	internal static string GetCloseButton() => "CloseButton".GetLocalizedResource();
	internal static string GetError() => "Error".GetLocalizedResource();
	internal static string GetErrorOccurredWhileLoadingLogs() => "ErrorOccurredWhileLoadingLogs".GetLocalizedResource();
	internal static string GetFrom() => "From".GetLocalizedResource();
	internal static string GetInDevelopment() => "InDevelopment".GetLocalizedResource();
	internal static string GetInstallerLoading() => "InstallerLoading".GetLocalizedResource();
	internal static string GetLicenseFreeDescription() => "LicenseFreeDescription".GetLocalizedResource();
	internal static string GetLicenseLoading() => "LicenseLoading".GetLocalizedResource();
	internal static string GetLicensePaidDescription() => "LicensePaidDescription".GetLocalizedResource();
	internal static string GetLicensePremiumDescription() => "LicensePremiumDescription".GetLocalizedResource();
	internal static string GetLicenseTestDescription() => "LicenseTestDescription".GetLocalizedResource();
	internal static string GetLoggerLoading() => "LoggerLoading".GetLocalizedResource();
	internal static string GetManualDeleteFile(string fileName) => $"{"GetManualDeleteFile".GetLocalizedResource()}: {fileName}";
	internal static string GetMenuClientIsQuery() => "MenuClientIsQuery".GetLocalizedResource();
	internal static string GetMenuLicenseCheckServer() => "MenuLicenseCheckServer".GetLocalizedResource();
	internal static string GetMenuLicenseIsNotConfirmed() => "MenuLicenseIsNotConfirmed".GetLocalizedResource();
	internal static string GetMenuLicenseResponseStatusCode() => "MenuLicenseResponseStatusCode".GetLocalizedResource();
	internal static string GetMenuLicenseUpdatedSuccessfully() => "MenuLicenseUpdatedSuccessfully".GetLocalizedResource();
	internal static string GetMenuLicenseUserNotLoggedIn() => "MenuLicenseUserNotLoggedIn".GetLocalizedResource();
	internal static string GetNoButton() => "NoButton".GetLocalizedResource();
	internal static string GetNotificationLoading() => "NotificationLoading".GetLocalizedResource();
	internal static string GetOkButton() => "OkButton".GetLocalizedResource();
	internal static string GetRpcErrorFloodWait() => "RpcErrorFloodWait".GetLocalizedResource();
	internal static string GetRpcErrorPasswordHashInvalid() => "RpcErrorPasswordHashInvalid".GetLocalizedResource();
	internal static string GetRpcErrorPhoneCodeInvalid() => "RpcErrorPhoneCodeInvalid".GetLocalizedResource();
	internal static string GetRpcErrorPhonePasswordFlood() => "RpcErrorPhonePasswordFlood".GetLocalizedResource();
	internal static string GetSettingAppLanguageTooltip => "SettingAppLanguageTooltip".GetLocalizedResource();
	internal static string GetSettingAppThemeTooltip => "SettingAppThemeTooltip".GetLocalizedResource();
	internal static string GetSettingsThemeDark() => "SettingsThemeDark".GetLocalizedResource();
	internal static string GetSettingsThemeDefault() => "SettingsThemeDefault".GetLocalizedResource();
	internal static string GetSettingsThemeLight() => "SettingsThemeLight".GetLocalizedResource();
	internal static string GetStorage() => "Storage".GetLocalizedResource();
	internal static string GetStorageLoading() => "StorageLoading".GetLocalizedResource();
	internal static string GetTableNameApps() => "TableNameApps".GetLocalizedResource();
	internal static string GetTableNameChats() => "TableNameChats".GetLocalizedResource();
	internal static string GetTableNameDocuments() => "TableNameDocuments".GetLocalizedResource();
	internal static string GetTableNameFilters() => "TableNameFilters".GetLocalizedResource();
	internal static string GetTableNameMessages() => "TableNameMessages".GetLocalizedResource();
	internal static string GetTableNameProxies() => "TableNameProxies".GetLocalizedResource();
	internal static string GetTableNameStories() => "TableNameStories".GetLocalizedResource();
	internal static string GetTableNameUsers() => "TableNameUsers".GetLocalizedResource();
	internal static string GetTableNameVersions() => "TableNameVersions".GetLocalizedResource();
	internal static string GetTextBlockFiltered() => "TextBlockFiltered".GetLocalizedResource();
	internal static string GetTextBlockLoaded() => "TextBlockLoaded".GetLocalizedResource();
	internal static string GetTextBlockSubscribed() => "TextBlockSubscribed".GetLocalizedResource();
	internal static string GetTextBlockTotalAmount() => "TextBlockTotalAmount".GetLocalizedResource();
	internal static string GetTgClientLoading() => "TgClientLoading".GetLocalizedResource();
	internal static string GetYesButton() => "YesButton".GetLocalizedResource();
	internal static string ViewImage() => "ViewImage".GetLocalizedResource();

    #endregion
}
