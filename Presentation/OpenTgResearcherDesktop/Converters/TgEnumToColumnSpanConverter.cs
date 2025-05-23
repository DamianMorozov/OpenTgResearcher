// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Converters;

public sealed partial class TgEnumToColumnSpanConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is TgEnumDirection align)
		{
			return align switch
			{
				TgEnumDirection.From => 2,
				TgEnumDirection.To => 2,
				_ => 3
			};
		}
		return 3;
	}

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
