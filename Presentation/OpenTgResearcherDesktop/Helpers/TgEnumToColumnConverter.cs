// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Helpers;

public sealed partial class TgEnumToColumnConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is TgEnumDirection align)
		{
			return align switch
			{
				TgEnumDirection.From => 0,
				TgEnumDirection.To => 1,
				_ => 0
			};
		}
		return 0;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}
