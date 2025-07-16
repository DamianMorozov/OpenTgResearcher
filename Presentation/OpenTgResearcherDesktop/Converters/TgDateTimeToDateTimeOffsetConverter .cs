// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Converters;

public sealed partial class TgDateTimeToDateTimeOffsetConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dt)
        {
            // Process DateTime.MinValue and DateTime.MaxValue otherwise, e.g. return null or DateTimeOffset.MinValue
            if (dt == DateTime.MinValue || dt == DateTime.MaxValue)
                return null;

            // Set Kind to UTC if not set
            if (dt.Kind == DateTimeKind.Unspecified)
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);

            try
            {
                return new DateTimeOffset(dt);
            }
            catch (ArgumentOutOfRangeException)
            {
                // If DateTimeOffset cannot be created, return null or default value
                return null;
            }
        }
        return null;
    }


    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTimeOffset dto)
            return dto.DateTime;
        return DateTime.MinValue;
    }
}