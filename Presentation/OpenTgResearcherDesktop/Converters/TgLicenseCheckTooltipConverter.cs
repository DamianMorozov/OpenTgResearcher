// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Converters;

public sealed partial class TgLicenseCheckTooltipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || parameter == null)
            return string.Empty;

        // Convert the current and required license types to enum
        if (!Enum.TryParse<TgEnumLicenseType>(value.ToString(), out var current))
            return string.Empty;
        if (!Enum.TryParse<TgEnumLicenseType>(parameter.ToString(), out var required))
            return string.Empty;

        // Display a hint
        return current < required 
            ? TgResourceExtensions.GetFeatureRequiresLicense(required) 
            : TgResourceExtensions.GetFeatureSucessLicense(App.BusinessLogicManager.LicenseService.CurrentLicense?.LicenseType ?? TgEnumLicenseType.No);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
