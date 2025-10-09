namespace OpenTgResearcherDesktop.Converters;

/// <summary> ID format converter </summary>
public sealed class TgIdFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is long lid)
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            return lid.ToString("#,0 ", nfi);
        }
        else if (value is int id)
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            return id.ToString("#,0 ", nfi);
        }
        else if (value is double did)
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            return did.ToString("#,0 ", nfi);
        }
        return value?.ToString() ?? string.Empty;
    }

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
