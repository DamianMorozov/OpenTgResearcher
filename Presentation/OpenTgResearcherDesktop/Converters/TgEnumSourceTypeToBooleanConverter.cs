namespace OpenTgResearcherDesktop.Converters;

public sealed partial class TgEnumSourceTypeToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || parameter == null) return false;
        var enumString = parameter.ToString();
        return (value.ToString() ?? string.Empty).Equals(enumString, StringComparison.InvariantCultureIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isChecked && isChecked && parameter != null)
        {
            return Enum.Parse(typeof(TgEnumSourceType), parameter.ToString()!, ignoreCase: true);
        }
        return DependencyProperty.UnsetValue;
    }
}
