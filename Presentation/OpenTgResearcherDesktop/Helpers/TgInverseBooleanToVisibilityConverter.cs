// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Helpers;

public sealed partial class TgInverseBooleanToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is bool boolValue)
			return boolValue ? Visibility.Collapsed : Visibility.Visible;
		return value is Visibility visibility
			? visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed
			: (object)Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language) =>
		value is Visibility visibility ? visibility == Visibility.Collapsed : (object)Visibility.Collapsed;
}