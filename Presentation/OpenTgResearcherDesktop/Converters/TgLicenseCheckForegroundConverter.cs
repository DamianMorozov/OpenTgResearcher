namespace OpenTgResearcherDesktop.Converters;

/// <summary> License check foreground converter </summary>
public sealed partial class TgLicenseCheckForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var brush = new SolidColorBrush(Colors.Yellow);
        if (value == null || parameter == null) return brush;
        if (!Enum.TryParse<TgEnumLicenseType>(value.ToString(), out var current)) return brush;
        if (!Enum.TryParse<TgEnumLicenseType>(parameter.ToString(), out var required)) return brush;

        return current >= required ? TgDesktopUtils.GetResourceBrush("TextFillColorPrimaryBrush") : brush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
