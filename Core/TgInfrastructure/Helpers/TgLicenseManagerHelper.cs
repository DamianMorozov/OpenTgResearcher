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

	public TgLicense CurrentLicense { get; private set; } = default!;

	public void ActivateLicense(string licenseKey = "", string licenseFreeDescription = "Free license", 
		string licensePaidDescription = "Paid license", string licensePremiumDescription = "Premium license")
	{
		if (licenseKey.StartsWith("PAID"))
		{
			CurrentLicense = new TgLicensePaid
			{
				LicenseKey = licenseKey,
				LicenseType = TgEnumLicenseType.Paid,
				ExpirationDate = DateTime.Now.AddYears(1),
				Description = licensePaidDescription
			};
			return;
		}
		if (licenseKey.StartsWith("PREMIUM"))
		{
			CurrentLicense = new TgLicensePremium
			{
				LicenseKey = licenseKey,
				LicenseType = TgEnumLicenseType.Premium,
				ExpirationDate = DateTime.Now.AddYears(1),
				PrioritySupport = true,
				Description = licensePremiumDescription
			};
			return;
		}
		CurrentLicense = new TgLicenseFree
		{
			LicenseKey = licenseKey,
			Description = licenseFreeDescription
		};
	}

	public bool IsLicenseValid() => CurrentLicense.IsValid();

	#endregion
}
