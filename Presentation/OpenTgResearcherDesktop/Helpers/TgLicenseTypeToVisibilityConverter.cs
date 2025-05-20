// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace OpenTgResearcherDesktop.Helpers;

public sealed partial class TgLicenseTypeToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is TgEnumLicenseType licenseType)
		{
			return licenseType == TgEnumLicenseType.Test || licenseType == TgEnumLicenseType.Paid || licenseType == TgEnumLicenseType.Premium
                ? Visibility.Visible : Visibility.Collapsed;
		}
		return Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}