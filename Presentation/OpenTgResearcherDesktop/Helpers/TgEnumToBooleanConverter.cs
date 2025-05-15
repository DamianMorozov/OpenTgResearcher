// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Helpers;

public sealed partial class TgEnumToBooleanConverter : IValueConverter
{
	public TgEnumToBooleanConverter()
	{
	}

	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (parameter is string enumString)
		{
			if (!Enum.IsDefined(typeof(ElementTheme), value))
			{
				throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");
			}

			var enumValue = Enum.Parse(typeof(ElementTheme), enumString);

			return enumValue.Equals(value);
		}

		throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		if (parameter is string enumString)
		{
			return Enum.Parse(typeof(ElementTheme), enumString);
		}
		throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
	}
}
