namespace OpenTgResearcherDesktop.Helpers;

public static class TgThemeUtils
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
