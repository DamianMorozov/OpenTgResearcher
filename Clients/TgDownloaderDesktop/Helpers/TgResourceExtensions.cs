﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Helpers;

public static class TgResourceExtensions
{
    #region Public and private fields, properties, constructor

    private static ResourceLoader LocalResourceLoader { get; } = new();

    public static string GetLocalized(this string resourceKey) => LocalResourceLoader.GetString(resourceKey);

	#endregion

	#region Public and private methods

	public static string AskClientConnect() => "AskClientConnect".GetLocalized();
	public static string AskClientDisconnect() => "AskClientDisconnect".GetLocalized();
	public static string AskDataClear() => "AskDataClear".GetLocalized();
	public static string AskDataLoad() => "AskDataLoad".GetLocalized();
	public static string AskRestartApp() => "AskRestartApp".GetLocalized();
	public static string AskSettingsClear() => "AskSettingsClear".GetLocalized();
	public static string AskSettingsDefault() => "AskSettingsDefault".GetLocalized();
	public static string AskSettingsDelete() => "AskSettingsDelete".GetLocalized();
	public static string AskSettingsLoad() => "AskSettingsLoad".GetLocalized();
	public static string AskSettingsSave() => "AskSettingsSave".GetLocalized();
	public static string AskUpdateOnline() => "AskUpdateOnline".GetLocalized();
	public static string AssertionRestartApp() => "AssertionRestartApp".GetLocalized();
	public static string ClientSettingsAreNotValid() => "ClientSettingsAreNotValid".GetLocalized();
	public static string GetAppVersion() => "AppVersion".GetLocalized();
	public static string GetCancelButton() => "CancelButton".GetLocalized();
	public static string GetClientEnterLoginCode() => "ClientEnterLoginCode".GetLocalized();
	public static string GetClientEnterPassword() => "ClientEnterPassword".GetLocalized();
	public static string GetClientFloodWait() => "ClientFloodWait".GetLocalized();
	public static string GetClientIsConnected() => "ClientIsConnected".GetLocalized();
	public static string GetClientIsDisconnected() => "ClientIsDisconnected".GetLocalized();
	public static string GetClipboard() => "Clipboard".GetLocalized();
	public static string GetLicenseFreeDescription() => "LicenseFreeDescription".GetLocalized();
	public static string GetLicensePaidDescription() => "LicensePaidDescription".GetLocalized();
	public static string GetLicensePremiumDescription() => "LicensePremiumDescription".GetLocalized();
	public static string GetMenuClientIsQuery() => "MenuClientIsQuery".GetLocalized();
	public static string GetNoButton() => "NoButton".GetLocalized();
	public static string GetOkButton() => "OkButton".GetLocalized();
	public static string GetSettingAppLanguageTooltip => "SettingAppLanguageTooltip".GetLocalized();
	public static string GetSettingAppThemeTooltip => "SettingAppThemeTooltip".GetLocalized();
	public static string GetSettingsThemeDark() => "SettingsThemeDark".GetLocalized();
	public static string GetSettingsThemeDefault() => "SettingsThemeDefault".GetLocalized();
	public static string GetSettingsThemeLight() => "SettingsThemeLight".GetLocalized();
	public static string GetYesButton() => "YesButton".GetLocalized();

	#endregion
}
