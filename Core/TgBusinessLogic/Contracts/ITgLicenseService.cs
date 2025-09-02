// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Contracts;

public interface ITgLicenseService : IDisposable
{
	public string MenuWebSiteGlobalUrl { get; }
	public string MenuWebSiteGlobalLicenseBuyUrl { get; }
	public TgLicenseDto CurrentLicense { get; }

	public void ActivateDefaultLicense();
	public void ActivateLicense(bool isConfirmed, Guid licenseKey, TgEnumLicenseType licenseType, long userId, DateTime validTo, 
		string licenseFreeDescription = "Free license", string licenseTestDescription = "Test license", string licensePaidDescription = "Paid license", 
        string licenseGiftDescription = "Gift license", string licensePremiumDescription = "Premium license");
	public void ActivateLicense(bool isConfirmed, Guid licenseKey, TgEnumLicenseType licenseType, long userId, DateOnly validTo, 
		string licenseFreeDescription = "Free license", string licenseTestDescription = "Test license", string licensePaidDescription = "Paid license", 
        string licenseGiftDescription = "Gift license", string licensePremiumDescription = "Premium license");
	public void ActivateLicenseWithDescriptions(string licenseFreeDescription, string licenseTestDescription, string licensePaidDescription, 
        string licenseGiftDescription, string licensePremiumDescription);
	public Task LicenseActivateAsync();
	public Task LicenseClearAsync();
	public Task LicenseUpdateAsync(TgLicenseDto licenseDto, bool isUidCopy = false);
    public Task<TgApiResult> GetApiLicenseStatisticAsync(DateOnly lastPromoDay);
    /// <summary> Create a new license for the user with the specified ID </summary>
    public Task<TgApiResult> CreateAsync(long userId, TgEnumLicenseType licenseType, DateOnly validTo, string password, Guid? uid = null, Guid? licenseKey = null);
}
