// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Converters;

public sealed partial class TgInverseBooleanConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is bool boolValue)
		{
			return !boolValue;
		}
		return DependencyProperty.UnsetValue;
	}

    public object ConvertBack(object value, Type targetType, object parameter, string language) => 
        Convert(value, targetType, parameter, language);
}
