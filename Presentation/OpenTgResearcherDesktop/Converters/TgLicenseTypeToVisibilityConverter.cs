// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Converters;

public sealed partial class TgLicenseTypeToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		var visible = Visibility.Collapsed;
		if (value is TgEnumLicenseType licenseType)
		{
			visible = licenseType == TgEnumLicenseType.Test || licenseType == TgEnumLicenseType.Paid || licenseType == TgEnumLicenseType.Gift || 
                licenseType == TgEnumLicenseType.Premium ? Visibility.Visible : Visibility.Collapsed;
		}
		return visible;
	}

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}