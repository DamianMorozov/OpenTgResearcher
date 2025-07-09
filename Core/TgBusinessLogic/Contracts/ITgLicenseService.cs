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
		string licenseFreeDescription = "Free license", string licenseTestDescription = "Test license", string licensePaidDescription = "Paid license", string licensePremiumDescription = "Premium license");
	public void ActivateLicense(bool isConfirmed, Guid licenseKey, TgEnumLicenseType licenseType, long userId, DateOnly validTo, 
		string licenseFreeDescription = "Free license", string licenseTestDescription = "Test license", string licensePaidDescription = "Paid license", string licensePremiumDescription = "Premium license");
	public void ActivateLicenseWithDescriptions(string licenseFreeDescription, string licenseTestDescription, string licensePaidDescription, string licensePremiumDescription);
	public Task LicenseActivateAsync();
	public Task LicenseClearAsync();
	public Task LicenseUpdateAsync(TgLicenseDto licenseDto);
	public Task<TgApiResult> GetApiCreatedAsync();
	public Task<TgApiResult> GetApiValidAsync();
    public Task<TgApiResult> GetApiPromoStatisticAsync(DateOnly lastPromoDay);
}
