namespace OpenTgResearcherDesktop.Converters;

/// <summary> ID format converter </summary>
public sealed class TgIdFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        TgDataFormatUtils.GetNumberFormatString(value);

    //public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    => throw new NotImplementedException();
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string s)
        {
            if (double.TryParse(s.Replace(" ", ""), out var d))
                return d;
            else if (int.TryParse(s.Replace(" ", ""), out var id))
                return id;
            else if (long.TryParse(s.Replace(" ", ""), out var lid))
                return lid;
        }
        return 0d;
    }
}
