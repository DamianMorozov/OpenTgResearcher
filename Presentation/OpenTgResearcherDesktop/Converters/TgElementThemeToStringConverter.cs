// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Converters;

public sealed partial class TgElementThemeToStringConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is ElementTheme theme)
		{
			return TgThemeUtils.GetThemeName(theme);
		}
		return "Unknown";
	}

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
