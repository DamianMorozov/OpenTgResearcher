// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Helpers;

public static class TgResourceExtensions
{
    #region Public and private fields, properties, constructor

    private static ResourceLoader LocalResourceLoader { get; } = new();

    public static string GetLocalized(this string resourceKey) => LocalResourceLoader.GetString(resourceKey);

	#endregion

	#region Public and private methods

	public static string ActionStorageCreateBackupFailed() => "ActionStorageCreateBackupFailed".GetLocalized();
	public static string ActionStorageCreateBackupFile() => "ActionStorageCreateBackupFile".GetLocalized();
	public static string ActionStorageCreateBackupSuccess() => "ActionStorageCreateBackupSuccess".GetLocalized();
	public static string ActionStorageResetAutoUpdateFailed() => "ActionStorageResetAutoUpdateFailed".GetLocalized();
	public static string ActionStorageResetAutoUpdateSuccess() => "ActionStorageResetAutoUpdateSuccess".GetLocalized();
	public static string AskActionStorageBackup() => "AskActionStorageBackup".GetLocalized();
	public static string AskActionStorageClear() => "AskActionStorageClear".GetLocalized();
	public static string AskActionStorageResetAutoUpdate() => "AskActionStorageResetAutoUpdate".GetLocalized();
	public static string AskActionStorageShrink() => "AskActionStorageShrink".GetLocalized();
	public static string AskAddRecord() => "AskAddRecord".GetLocalized();
	public static string AskCalcChatStatistics() => "AskCalcChatStatistics".GetLocalized();
	public static string AskCalcContentStatistics() => "AskCalcContentStatistics".GetLocalized();
	public static string AskClientConnect() => "AskClientConnect".GetLocalized();
	public static string AskClientDisconnect() => "AskClientDisconnect".GetLocalized();
	public static string AskDataClear() => "AskDataClear".GetLocalized();
	public static string AskDataLoad() => "AskDataLoad".GetLocalized();
	public static string AskDeleteFile() => "AskDeleteFile".GetLocalized();
	public static string AskGetParticipants() => "AskGetParticipants".GetLocalized();
	public static string AskLicenseCheck() => "AskLicenseCheck".GetLocalized();
	public static string AskLicenseClear() => "AskLicenseClear".GetLocalized();
	public static string AskRestartApp() => "AskRestartApp".GetLocalized();
	public static string AskSettingsClear() => "AskSettingsClear".GetLocalized();
	public static string AskSettingsDefault() => "AskSettingsDefault".GetLocalized();
	public static string AskSettingsDelete() => "AskSettingsDelete".GetLocalized();
	public static string AskSettingsLoad() => "AskSettingsLoad".GetLocalized();
	public static string AskSettingsSave() => "AskSettingsSave".GetLocalized();
	public static string AskStopDownloading() => "AskStopDownloading".GetLocalized();
	public static string AskUpdateChatDetails() => "AskUpdateChatDetails".GetLocalized();
	public static string AskUpdateOnline() => "AskUpdateOnline".GetLocalized();
	public static string AskUpdatePage() => "AskUpdatePage".GetLocalized();
	public static string AskUpdatePreviewApp() => "AskUpdatePreviewApp".GetLocalized();
	public static string AskUpdateReleaseApp() => "AskUpdateReleaseApp".GetLocalized();
	public static string AssertionRestartApp() => "AssertionRestartApp".GetLocalized();
	public static string ClientSettingsAreNotValid() => "ClientSettingsAreNotValid".GetLocalized();
	public static string GetActionCheckLicenseMsg() => "ActionLicenseCheckMsg".GetLocalized();
	public static string GetActionLicenseBuyMsg() => "ActionLicenseBuyMsg".GetLocalized();
	public static string GetAppDisplayName() => "AppDisplayName".GetLocalized();
	public static string GetAppLoading() => "AppLoading".GetLocalized();
	public static string GetAppVersion() => "AppVersion".GetLocalized();
	public static string GetCancelButton() => "CancelButton".GetLocalized();
	public static string GetClientDataRequestEmptyResponse() => "ClientDataRequestEmptyResponse".GetLocalized();
	public static string GetClientEnterLoginCode() => "ClientEnterLoginCode".GetLocalized();
	public static string GetClientEnterPassword() => "ClientEnterPassword".GetLocalized();
	public static string GetClientFloodWait() => "ClientFloodWait".GetLocalized();
	public static string GetClientIsConnected() => "ClientIsConnected".GetLocalized();
	public static string GetClientIsDisconnected() => "ClientIsDisconnected".GetLocalized();
	public static string GetClipboard() => "Clipboard".GetLocalized();
	public static string GetCloseButton() => "CloseButton".GetLocalized();
	public static string GetError() => "Error".GetLocalized();
	public static string GetErrorOccurredWhileLoadingLogs() => "ErrorOccurredWhileLoadingLogs".GetLocalized();
	public static string GetInDevelopment() => "InDevelopment".GetLocalized();
	public static string GetInstallerLoading() => "InstallerLoading".GetLocalized();
	public static string GetLicenseFreeDescription() => "LicenseFreeDescription".GetLocalized();
	public static string GetLicenseLoading() => "LicenseLoading".GetLocalized();
	public static string GetLicensePaidDescription() => "LicensePaidDescription".GetLocalized();
	public static string GetLicensePremiumDescription() => "LicensePremiumDescription".GetLocalized();
	public static string GetLicenseTestDescription() => "LicenseTestDescription".GetLocalized();
	public static string GetLoggerLoading() => "LoggerLoading".GetLocalized();
	public static string GetManualDeleteFile(string fileName) => $"{"GetManualDeleteFile".GetLocalized()}: {fileName}";
	public static string GetMenuClientIsQuery() => "MenuClientIsQuery".GetLocalized();
	public static string GetMenuLicenseCheckServer() => "MenuLicenseCheckServer".GetLocalized();
	public static string GetMenuLicenseIsNotCofirmed() => "MenuLicenseIsNotCofirmed".GetLocalized();
	public static string GetMenuLicenseResponseStatusCode() => "MenuLicenseResponseStatusCode".GetLocalized();
	public static string GetMenuLicenseUpdatedSuccessfully() => "MenuLicenseUpdatedSuccessfully".GetLocalized();
	public static string GetMenuLicenseUserNotLoggedIn() => "MenuLicenseUserNotLoggedIn".GetLocalized();
	public static string GetNoButton() => "NoButton".GetLocalized();
	public static string GetNotificationLoading() => "NotificationLoading".GetLocalized();
	public static string GetOkButton() => "OkButton".GetLocalized();
	public static string GetRpcErrorFloodWait() => "RpcErrorFloodWait".GetLocalized();
	public static string GetRpcErrorPasswordHashInvalid() => "RpcErrorPasswordHashInvalid".GetLocalized();
	public static string GetRpcErrorPhoneCodeInvalid() => "RpcErrorPhoneCodeInvalid".GetLocalized();
	public static string GetRpcErrorPhonePasswordFlood() => "RpcErrorPhonePasswordFlood".GetLocalized();
	public static string GetSettingAppLanguageTooltip => "SettingAppLanguageTooltip".GetLocalized();
	public static string GetSettingAppThemeTooltip => "SettingAppThemeTooltip".GetLocalized();
	public static string GetSettingsThemeDark() => "SettingsThemeDark".GetLocalized();
	public static string GetSettingsThemeDefault() => "SettingsThemeDefault".GetLocalized();
	public static string GetSettingsThemeLight() => "SettingsThemeLight".GetLocalized();
	public static string GetStorage() => "Storage".GetLocalized();
	public static string GetStorageLoading() => "StorageLoading".GetLocalized();
	public static string GetTableNameApps() => "TableNameApps".GetLocalized();
	public static string GetTableNameChats() => "TableNameChats".GetLocalized();
	public static string GetTableNameDocuments() => "TableNameDocuments".GetLocalized();
	public static string GetTableNameFilters() => "TableNameFilters".GetLocalized();
	public static string GetTableNameMessages() => "TableNameMessages".GetLocalized();
	public static string GetTableNameProxies() => "TableNameProxies".GetLocalized();
	public static string GetTableNameStories() => "TableNameStories".GetLocalized();
	public static string GetTableNameUsers() => "TableNameUsers".GetLocalized();
	public static string GetTableNameVersions() => "TableNameVersions".GetLocalized();
	public static string GetTextBlockFiltered() => "TextBlockFiltered".GetLocalized();
	public static string GetTextBlockLoaded() => "TextBlockLoaded".GetLocalized();
	public static string GetTextBlockTotalAmount() => "TextBlockTotalAmount".GetLocalized();
	public static string GetTgClientLoading() => "TgClientLoading".GetLocalized();
	public static string GetYesButton() => "YesButton".GetLocalized();
	public static string ViewImage() => "ViewImage".GetLocalized();

	#endregion
}
