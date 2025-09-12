namespace OpenTgResearcherDesktop.Converters;

public sealed partial class TgSafeImagePathConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        try
        {
            var bitmap = new BitmapImage
            {
                UriSource = new Uri(path, UriKind.Absolute)
            };
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
