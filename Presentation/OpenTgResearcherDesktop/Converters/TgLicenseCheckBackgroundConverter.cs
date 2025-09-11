// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Converters;

/// <summary> License check background converter </summary>
public sealed partial class TgLicenseCheckBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var brush = new SolidColorBrush(Colors.Gray);
        if (value == null || parameter == null) return brush;
        if (!Enum.TryParse<TgEnumLicenseType>(value.ToString(), out var current)) return brush;
        if (!Enum.TryParse<TgEnumLicenseType>(parameter.ToString(), out var required)) return brush;

        return current >= required ? TgDesktopUtils.GetResourceBrush("ControlFillColorDefaultBrush") : brush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
