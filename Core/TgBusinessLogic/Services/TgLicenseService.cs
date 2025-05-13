// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Services;

public sealed class TgLicenseService() : ITgLicenseService
{
	#region Public and private fields, properties, constructor

	public string MenuWebSiteGlobalUrl => "http://opentgresearcher.online/";
	public string MenuWebSiteGlobalLicenseBuyUrl => "http://opentgresearcher.online/licenses/";
	public string MenuWebSiteRussianUrl => "http://opentgresearcher.ru/";
	public string MenuWebSiteRussianLicenseBuyUrl => "http://opentgresearcher.ru/licenses/";
	public TgLicenseDto CurrentLicense { get; private set; } = null!;

	private ITgEfLicenseRepository LicenseRepository { get; } = new TgEfLicenseRepository();

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
		var licenseDtos = await LicenseRepository.GetListDtosAsync();
		var currentLicenseDto = licenseDtos.FirstOrDefault(x => x.IsConfirmed && x.ValidTo >= DateTime.UtcNow);
		if (currentLicenseDto is not null)
			ActivateLicense(currentLicenseDto.IsConfirmed, currentLicenseDto.LicenseKey,
				currentLicenseDto.LicenseType, currentLicenseDto.UserId, currentLicenseDto.ValidTo);
		else
			ActivateDefaultLicense();
	}

	public async Task LicenseClearAsync()
	{
		await LicenseRepository.DeleteAllAsync();
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

		var licenseDtos = await LicenseRepository.GetListDtosAsync();
		var currentLicenseDto = licenseDtos.FirstOrDefault(x => x.IsConfirmed && x.ValidTo >= DateTime.UtcNow);
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

	#endregion
}
