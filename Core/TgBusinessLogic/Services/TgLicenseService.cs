// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Services;

public sealed class TgLicenseService : ITgLicenseService
{
	#region Public and private fields, properties, constructor

	public string MenuWebSiteGlobalUrl => "http://opentgresearcher.online/";
	public string MenuWebSiteGlobalLicenseBuyUrl => "http://opentgresearcher.online/licenses/";
	public string MenuWebSiteRussianUrl => "http://opentgresearcher.ru/";
	public string MenuWebSiteRussianLicenseBuyUrl => "http://opentgresearcher.ru/licenses/";
	public TgLicenseDto CurrentLicense { get; private set; } = null!;

	private ITgEfAppRepository AppRepository { get; }
	private ITgEfLicenseRepository LicenseRepository { get; }

	public TgLicenseService()
	{
        AppRepository = new TgEfAppRepository();
		LicenseRepository = new TgEfLicenseRepository();
	}

	public TgLicenseService(IWebHostEnvironment webHostEnvironment)
	{
		AppRepository = new TgEfAppRepository(webHostEnvironment);
        LicenseRepository = new TgEfLicenseRepository(webHostEnvironment);
	}

	#endregion

	#region Public and private methods

	public void ActivateDefaultLicense() => ActivateLicense(false, Guid.Empty, TgEnumLicenseType.Free, 0, DateOnly.MinValue);

	public void ActivateLicenseWithDescriptions(string licenseFreeDescription, string licenseTestDescription,
		string licensePaidDescription, string licensePremiumDescription) =>
		ActivateLicense(false, Guid.Empty, TgEnumLicenseType.Free, 0, DateOnly.MinValue,
		licenseFreeDescription, licenseTestDescription, licensePaidDescription, licensePremiumDescription);

	public void ActivateLicense(bool isConfirmed, Guid licenseKey, TgEnumLicenseType licenseType, long userId, DateTime validTo,
		string licenseFreeDescription = "Free license", string licenseTestDescription = "Test license",
		string licensePaidDescription = "Paid license", string licensePremiumDescription = "Premium license") =>
		ActivateLicense(isConfirmed, licenseKey, licenseType, userId, DateOnly.FromDateTime(validTo), licenseFreeDescription,
		licenseTestDescription, licensePaidDescription, licensePremiumDescription);

	public void ActivateLicense(bool isConfirmed, Guid licenseKey, TgEnumLicenseType licenseType, long userId, DateOnly validTo,
		string licenseFreeDescription = "Free license", string licenseTestDescription = "Test license",
		string licensePaidDescription = "Paid license", string licensePremiumDescription = "Premium license")
	{
		CurrentLicense = new TgLicenseDto() { IsConfirmed = isConfirmed, LicenseKey = licenseKey, LicenseType = licenseType, ValidTo = validTo, UserId = userId };
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

	public async Task LicenseActivateAsync()
	{
		var userId = await GetUserIdAsync();
        if (userId > 0)
		{
            var licenseDtos = await LicenseRepository.GetListDtosAsync();
            var currentLicenseDto = licenseDtos.FirstOrDefault(x => x.IsConfirmed && DateTime.Parse($"{x.ValidTo:yyyy-MM-dd}") >= DateTime.UtcNow.Date && x.UserId == userId);
            if (currentLicenseDto is not null)
			{
				ActivateLicense(currentLicenseDto.IsConfirmed, currentLicenseDto.LicenseKey,
					currentLicenseDto.LicenseType, currentLicenseDto.UserId, currentLicenseDto.ValidTo);
				return;
			}
        }
        ActivateDefaultLicense();
    }

    public async Task LicenseClearAsync()
	{
		await LicenseRepository.DeleteAllAsync();
        var app = (await AppRepository.GetCurrentAppAsync(isReadOnly: false)).Item;
		if (app.UseBot)
		{
			app.UseBot = false;
			await AppRepository.SaveAsync(app);
		}
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

        var userId = await GetUserIdAsync();
        var licenseDtos = await LicenseRepository.GetListDtosAsync();
		var currentLicenseDto = licenseDtos.FirstOrDefault(x => x.IsConfirmed && DateTime.Parse($"{x.ValidTo:yyyy-MM-dd}") >= DateTime.UtcNow.Date && x.UserId == userId);
        if (currentLicenseDto is null)
		{
			await LicenseRepository.SaveAsync(licenseEntity);
		}
		else
		{
			var licenseExists = await LicenseRepository.GetItemAsync(licenseEntity, isReadOnly: false);
			licenseExists.Copy(licenseEntity, isUidCopy: false);
			await LicenseRepository.SaveAsync(licenseEntity);
		}
	}

	public async Task<TgApiResult> GetApiCreatedAsync()
	{
		var result = new TgApiResult();
		var licenseCountDto = new TgLicenseCountDto();
		// Search license
		var licenseDtos = await LicenseRepository.GetListDtosAsync(take: 0, skip: 0);
		if (licenseDtos.Any())
		{
			licenseCountDto.TestCount = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Test).Count();
			licenseCountDto.PaidCount = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Paid).Count();
			licenseCountDto.PreimumCount = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Premium).Count();
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
		var licenseDtos = await LicenseRepository.GetListDtosAsync(take: 0, skip: 0, x => x.ValidTo >= DateTime.UtcNow.Date);
		if (licenseDtos.Any())
		{
			licenseCountDto.TestCount = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Test).Count();
			licenseCountDto.PaidCount = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Paid).Count();
			licenseCountDto.PreimumCount = licenseDtos.Where(x => x.LicenseType == TgEnumLicenseType.Premium).Count();
			result.IsOk = true;
		}
		result.Value = licenseCountDto;
		return result;
	}

    public async Task<long> GetUserIdAsync()
    {
        if (TgGlobalTools.ConnectClient.Me is null)
            await TgGlobalTools.ConnectClient.LoginUserAsync(isProxyUpdate: false);
        var userId = TgGlobalTools.ConnectClient.Me?.ID ?? 0;
        return userId;
    }

    #endregion
}
