﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderDesktop.Helpers;

public static class TgThemeHelper
{
	public static string GetThemeName(ElementTheme appTheme) =>
		appTheme switch
		{
			ElementTheme.Default => "Default",
			ElementTheme.Light => "Light",
			ElementTheme.Dark => "Dark",
			_ => "Unknown"
		};

	public static ElementTheme GetElementTheme(TgEnumTheme appTheme) =>
		appTheme switch
		{
			TgEnumTheme.Default => ElementTheme.Default,
			TgEnumTheme.Light => ElementTheme.Light,
			TgEnumTheme.Dark => ElementTheme.Dark,
			_ => ElementTheme.Default
		};
}
