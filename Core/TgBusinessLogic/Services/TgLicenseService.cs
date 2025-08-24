// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Services;

public sealed class TgLicenseService : TgWebDisposable, ITgLicenseService
{
	#region Fields, properties, constructor

	public string MenuWebSiteGlobalUrl => "https://opentgresearcher.online/";
	public string MenuWebSiteGlobalLicenseBuyUrl => "https://opentgresearcher.online/licenses/";
	public TgLicenseDto CurrentLicense { get; private set; } = default!;
    
    private ITgStorageManager StorageManager { get; } = default!;

    public TgLicenseService(ITgStorageManager storageManager) : base()
    {
        StorageManager = storageManager;
    }

    public TgLicenseService(IWebHostEnvironment webHostEnvironment, ITgStorageManager storageManager) : base(webHostEnvironment)
    {
        StorageManager = storageManager;
    }

    #endregion

    #region TgDisposable

    /// <summary> Release managed resources </summary>
    public override void ReleaseManagedResources()
    {
        StorageManager.Dispose();
    }

    /// <summary> Release unmanaged resources </summary>
    public override void ReleaseUnmanagedResources()
    {
        //
    }

    #endregion

    #region Methods

    public void ActivateDefaultLicense() => ActivateLicense(false, Guid.Empty, TgEnumLicenseType.Free, 0, DateOnly.MinValue);

	public void ActivateLicenseWithDescriptions(string licenseFreeDescription, string licenseTestDescription,
		string licensePaidDescription, string licenseGiftDescription, string licensePremiumDescription) =>
		ActivateLicense(false, Guid.Empty, TgEnumLicenseType.Free, 0, DateOnly.MinValue,
		licenseFreeDescription, licenseTestDescription, licensePaidDescription, licenseGiftDescription, licensePremiumDescription);

	public void ActivateLicense(bool isConfirmed, Guid licenseKey, TgEnumLicenseType licenseType, long userId, DateTime validTo,
		string licenseFreeDescription = "Free license", string licenseTestDescription = "Test license",
		string licensePaidDescription = "Paid license", string licenseGiftDescription = "Gift license", 
        string licensePremiumDescription = "Premium license") =>
		ActivateLicense(isConfirmed, licenseKey, licenseType, userId, DateOnly.FromDateTime(validTo), licenseFreeDescription,
		licenseTestDescription, licensePaidDescription, licenseGiftDescription, licensePremiumDescription);

	public void ActivateLicense(bool isConfirmed, Guid licenseKey, TgEnumLicenseType licenseType, long userId, DateOnly validTo,
		string licenseFreeDescription = "Free license", string licenseTestDescription = "Test license",
		string licensePaidDescription = "Paid license", string licenseGiftDescription = "Gift license", 
        string licensePremiumDescription = "Premium license")
	{
		CurrentLicense = new TgLicenseDto() 
            { IsConfirmed = isConfirmed, LicenseKey = licenseKey, LicenseType = licenseType, ValidTo = validTo, UserId = userId };
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
			case TgEnumLicenseType.Gift:
				CurrentLicense.SetDescription(licenseGiftDescription);
				break;
			case TgEnumLicenseType.Premium:
				CurrentLicense.SetDescription(licensePremiumDescription);
				break;
		}
	}

	public async Task LicenseActivateAsync()
	{
        var licenseDtos = await StorageManager.LicenseRepository.GetListDtosAsync();
        var currentLicenseDto = licenseDtos.FirstOrDefault(x => x.IsConfirmed && DateTime.Parse($"{x.ValidTo:yyyy-MM-dd}") >= DateTime.UtcNow.Date);
        if (currentLicenseDto is not null)
		{
			ActivateLicense(currentLicenseDto.IsConfirmed, currentLicenseDto.LicenseKey, currentLicenseDto.LicenseType, currentLicenseDto.UserId, currentLicenseDto.ValidTo);
			return;
		}
        ActivateDefaultLicense();
    }

    public async Task LicenseClearAsync()
	{
		await StorageManager.LicenseRepository.DeleteAllAsync();
        await StorageManager.AppRepository.SetUseBotAsync(false);
    }

    public async Task LicenseUpdateAsync(TgLicenseDto licenseDto)
	{
		var licenseEntity = new TgEfLicenseEntity
		{
			IsConfirmed = licenseDto.IsConfirmed,
			LicenseKey = licenseDto.LicenseKey,
			LicenseType = licenseDto.LicenseType,
			UserId = licenseDto.UserId,
			ValidTo = DateTime.Parse($"{licenseDto.ValidTo:yyyy-MM-dd}")
		};

        var licenseDtos = await StorageManager.LicenseRepository.GetListDtosAsync();
		var currentLicenseDto = licenseDtos.FirstOrDefault(x => x.IsConfirmed && DateTime.Parse($"{x.ValidTo:yyyy-MM-dd}") >= DateTime.UtcNow.Date);
        if (currentLicenseDto is null)
		{
			await StorageManager.LicenseRepository.SaveAsync(licenseEntity);
		}
		else
		{
			var licenseExists = await StorageManager.LicenseRepository.GetItemAsync(licenseEntity, isReadOnly: false);
			licenseExists.Copy(licenseEntity, isUidCopy: false);
			await StorageManager.LicenseRepository.SaveAsync(licenseEntity);
		}
	}

	public async Task<TgApiResult> GetApiCreatedAsync()
	{
		var result = new TgApiResult();
		var licenseCountDto = new TgLicenseCountDto();
		// Search license
		var licenseDtos = await StorageManager.LicenseRepository.GetListDtosAsync(take: 0, skip: 0);
		if (licenseDtos.Count != 0)
		{
			licenseCountDto.TestCount = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Test).Count();
			licenseCountDto.PaidCount = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Paid).Count();
			licenseCountDto.PremiumCount = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Premium).Count();
			result.IsOk = true;
		}
		result.Value = licenseCountDto;
		return result;
	}

	public async Task<TgApiResult> GetApiValidAsync()
	{
		var result = new TgApiResult();
		var licenseCountDto = new TgLicenseCountDto();
		// Search license
		var licenseDtos = await StorageManager.LicenseRepository.GetListDtosAsync(take: 0, skip: 0, x => x.ValidTo >= DateTime.UtcNow.Date);
		if (licenseDtos.Count != 0)
		{
			licenseCountDto.TestCount = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Test).Count();
			licenseCountDto.PaidCount = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Paid).Count();
			licenseCountDto.PremiumCount = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Premium).Count();
			licenseCountDto.PremiumCount = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Premium).Count();
			result.IsOk = true;
		}
		result.Value = licenseCountDto;
		return result;
	}

    public async Task<TgApiResult> GetApiLicenseStatisticAsync(DateOnly lastPromoDay)
    {
        var result = new TgApiResult();
        var promoStatisticSummaryDto = new TgPromoStatisticSummaryDto(lastPromoDay);
        // Search promo license
        var licenseDtos = await StorageManager.LicenseRepository.GetListDtosAsync(take: 0, skip: 0, x => x.LicenseType != TgEnumLicenseType.Free);
        if (licenseDtos.Count > 0)
        {
            var dtNow = DateOnly.FromDateTime(DateTime.UtcNow);
            // Test group
            var groupTest = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Test && x.ValidTo >= dtNow).ToList();
            promoStatisticSummaryDto.Items.Add(new TgPromoStatisticDto { TestCount = groupTest.Count, Limit = 0 });
            // Paid group
            var groupPaid = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Paid && x.ValidTo >= dtNow).ToList();
            promoStatisticSummaryDto.Items.Add(new TgPromoStatisticDto { PaidCount = groupPaid.Count });
            // Gift group
            var groupGift = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Gift && x.ValidTo >= dtNow).ToList();
            promoStatisticSummaryDto.Items.Add(new TgPromoStatisticDto { GiftCount = groupGift.Count });
            result.IsOk = true;
            // Premium group
            var groupPremium = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Premium && x.ValidTo >= dtNow).ToList();
            promoStatisticSummaryDto.Items.Add(new TgPromoStatisticDto { PremiumCount = groupPremium.Count });
            result.IsOk = true;
        }
        // Order items
        promoStatisticSummaryDto.Items = [.. promoStatisticSummaryDto.Items
            .OrderByDescending(x => x.TestCount > 0)
            .OrderByDescending(x => x.PaidCount > 0)
            .OrderByDescending(x => x.GiftCount > 0)
            .OrderByDescending(x => x.PremiumCount > 0)
        ];
        result.Value = promoStatisticSummaryDto;
        return result;
    }

    /// <inheritdoc />
    public async Task<TgApiResult> CreateAsync(long userId, TgEnumLicenseType licenseType, DateOnly validTo, string password)
    {
        var result = new TgApiResult();
        if (userId <= 0)
        {
            result.IsOk = false;
            result.Value = "User ID must be greater than zero!";
            return await Task.FromResult(result);
        }
        var licenseDto = new TgLicenseDto
        {
            IsConfirmed = true,
            LicenseKey = Guid.NewGuid(),
            LicenseType = licenseType,
            UserId = userId,
            ValidTo = validTo
        };
        await LicenseUpdateAsync(licenseDto);
        ActivateLicense(licenseDto.IsConfirmed, licenseDto.LicenseKey, licenseDto.LicenseType, licenseDto.UserId, licenseDto.ValidTo);
        result.IsOk = true;
        result.Value = licenseDto;
        return await Task.FromResult(result);
    }

    #endregion
}
