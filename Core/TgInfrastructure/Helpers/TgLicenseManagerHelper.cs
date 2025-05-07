// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Helpers;

public sealed class TgLicenseManagerHelper
{
	#region Design pattern "Lazy Singleton"

	private static TgLicenseManagerHelper _instance = null!;
	public static TgLicenseManagerHelper Instance => LazyInitializer.EnsureInitialized(ref _instance);

	#endregion

	#region Public and private methods

	public TgLicense CurrentLicense { get; private set; } = null!;

	public void ActivateDefaultLicense() => ActivateLicense(Guid.Empty, TgEnumLicenseType.Free, 0, DateTime.MinValue);

	public void ActivateLicenseWithDescriptions(string licenseFreeDescription, string licenseTestDescription, 
		string licensePaidDescription, string licensePremiumDescription) => 
		ActivateLicense(Guid.Empty, TgEnumLicenseType.Free, 0, DateTime.MinValue, 
		licenseFreeDescription, licenseTestDescription, licensePaidDescription, licensePremiumDescription);

	public void ActivateLicense(Guid licenseKey, TgEnumLicenseType licenseType, long userId, DateTime validTo, 
		string licenseFreeDescription = "Free license", string licenseTestDescription = "Test license",
		string licensePaidDescription = "Paid license", string licensePremiumDescription = "Premium license")
	{
		CurrentLicense = new TgLicense() { LicenseKey = licenseKey, LicenseType = licenseType, ValidTo = validTo, UserId = userId };
		switch (licenseType)
		{
			case TgEnumLicenseType.Free:
				CurrentLicense.SetDescription(licenseFreeDescription);
				break;
			case TgEnumLicenseType.Test:
				CurrentLicense.SetDescription(licenseTestDescription);
				break;
			case TgEnumLicenseType.Paid:
				CurrentLicense.SetDescription(licensePaidDescription);
				break;
			case TgEnumLicenseType.Premium:
				CurrentLicense.SetDescription(licensePremiumDescription);
				break;
		}
	}

	public bool IsLicenseValid() => CurrentLicense.IsValid();

	#endregion
}
