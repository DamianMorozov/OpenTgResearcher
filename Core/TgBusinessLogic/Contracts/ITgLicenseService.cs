namespace TgBusinessLogic.Contracts;

public interface ITgLicenseService : IDisposable
{
	public string MenuWebSiteGlobalUrl { get; }
	public string MenuWebSiteGlobalLicenseBuyUrl { get; }
	public TgLicenseDto CurrentLicense { get; }

	public void ActivateDefaultLicense();
	public void ActivateLicense(bool isConfirmed, Guid licenseKey, TgEnumLicenseType licenseType, long userId, DateTime validTo, 
		string licenseNoDescription = "No license", 
        string licenseCommunityDescription = "Community license", 
        string licensePaidDescription = "Paid license", 
        string licenseGiftDescription = "Gift license", 
        string licensePremiumDescription = "Premium license");
	public void ActivateLicense(bool isConfirmed, Guid licenseKey, TgEnumLicenseType licenseType, long userId, DateOnly validTo, 
		string licenseNoDescription = "No license", 
        string licenseCommunityDescription = "Community license", 
        string licensePaidDescription = "Paid license", 
        string licenseGiftDescription = "Gift license", 
        string licensePremiumDescription = "Premium license");
	public void ActivateLicenseWithDescriptions(string licenseNoDescription, string licenseCommunityDescription, string licensePaidDescription, 
        string licenseGiftDescription, string licensePremiumDescription);
	public Task LicenseActivateAsync();
	public Task LicenseClearAsync();
	public Task LicenseUpdateAsync(TgLicenseDto licenseDto);
    public Task<TgApiResult> GetApiLicenseStatisticAsync(DateOnly lastPromoDay);
    /// <summary> Create a new license for the user with the specified ID </summary>
    public Task<TgApiResult> CreateAsync(long userId, TgEnumLicenseType licenseType, DateOnly validTo);
}
