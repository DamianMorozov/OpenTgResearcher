namespace OpenTgResearcherDesktop.Converters;

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

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
