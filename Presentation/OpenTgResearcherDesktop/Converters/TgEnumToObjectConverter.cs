namespace OpenTgResearcherDesktop.Converters;

/// <summary> Converts enum value to mapped object using XAML collection or ConverterParameter string </summary>
public sealed partial class TgEnumToObjectConverter : IValueConverter
{
    public List<TgEnumToObjectMapping> EnumValueMappings { get; } = new();

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        try
        {
            if (value == null)
                return string.Empty;

            // Try XAML-based mappings first
            if (EnumValueMappings.Count > 0)
            {
                var match = EnumValueMappings
                    .FirstOrDefault(m => string.Equals(m.EnumValue, value.ToString(), StringComparison.OrdinalIgnoreCase));
                if (match != null)
                    return match.ObjectValue ?? string.Empty;
            }

            // Fallback to ConverterParameter string mapping
            if (parameter is string str && !string.IsNullOrWhiteSpace(str))
            {
                var mappings = str.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var mapping in mappings)
                {
                    var parts = mapping.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2 && parts[0].Equals(value.ToString(), StringComparison.OrdinalIgnoreCase))
                        return parts[1];
                }
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in {nameof(TgEnumToObjectConverter)}: {ex}");
            return string.Empty;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
