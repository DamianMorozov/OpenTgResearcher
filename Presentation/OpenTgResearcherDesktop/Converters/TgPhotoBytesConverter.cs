namespace OpenTgResearcherDesktop.Converters;

/// <summary> Отображает фотографию Telegram из памяти без промежуточного файла </summary>
public sealed class TgPhotoBytesConverter : IValueConverter
{
    /// <summary> Создаёт источник изображения; при ошибке декодирования сохраняются инициалы чата </summary>
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not byte[] { Length: > 0 } bytes) return null;
        try
        {
            using var input = new MemoryStream(bytes, writable: false);
            using var stream = System.IO.WindowsRuntimeStreamExtensions.AsRandomAccessStream(input);
            var image = new BitmapImage();
            image.SetSource(stream);
            return image;
        }
        catch (Exception ex) { TgLogUtils.WriteException(ex); return null; }
    }

    /// <summary> Обратное преобразование фотографии не используется </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException("Обратное преобразование фотографии Telegram не поддерживается");
}
