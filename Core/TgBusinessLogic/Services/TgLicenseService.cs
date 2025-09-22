namespace TgBusinessLogic.Services;

public sealed class TgLicenseService : TgWebDisposable, ITgLicenseService
{
	#region Fields, properties, constructor

	public string MenuWebSiteGlobalUrl => "https://opentgresearcher.online/";
	public string MenuWebSiteGlobalLicenseBuyUrl => "https://opentgresearcher.online/licenses/";
	public TgLicenseDto CurrentLicense { get; private set; } = default!;
    
    private ITgStorageService StorageManager { get; } = default!;

    public TgLicenseService(ITgStorageService storageManager) : base()
    {
        StorageManager = storageManager;
    }

    public TgLicenseService(IWebHostEnvironment webHostEnvironment, ITgStorageService storageManager) : base(webHostEnvironment)
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

    public void ActivateDefaultLicense() => ActivateLicense(false, Guid.Empty, TgEnumLicenseType.No, 0, DateOnly.MinValue);

	public void ActivateLicenseWithDescriptions(string licenseNoDescription, string licenseCommunityDescription,
		string licensePaidDescription, string licenseGiftDescription, string licensePremiumDescription) =>
		ActivateLicense(false, Guid.Empty, TgEnumLicenseType.No, 0, DateOnly.MinValue,
		licenseNoDescription, licenseCommunityDescription, licensePaidDescription, licenseGiftDescription, licensePremiumDescription);

	public void ActivateLicense(bool isConfirmed, Guid licenseKey, TgEnumLicenseType licenseType, long userId, DateTime validTo,
		string licenseNoDescription = "No license", 
        string licenseCommunityDescription = "Community license",
		string licensePaidDescription = "Paid license", 
        string licenseGiftDescription = "Gift license", 
        string licensePremiumDescription = "Premium license") =>
		ActivateLicense(isConfirmed, licenseKey, licenseType, userId, DateOnly.FromDateTime(validTo), licenseNoDescription,
            licenseCommunityDescription, licensePaidDescription, licenseGiftDescription, licensePremiumDescription);

	public void ActivateLicense(bool isConfirmed, Guid licenseKey, TgEnumLicenseType licenseType, long userId, DateOnly validTo,
		string licenseNoDescription = "No license", 
        string licenseCommunityDescription = "Community license",
		string licensePaidDescription = "Paid license", 
        string licenseGiftDescription = "Gift license", 
        string licensePremiumDescription = "Premium license")
	{
        var description = licenseType switch
        {
            TgEnumLicenseType.No => licenseNoDescription,
            TgEnumLicenseType.Community => licenseCommunityDescription,
            TgEnumLicenseType.Paid => licensePaidDescription,
            TgEnumLicenseType.Gift => licenseGiftDescription,
            TgEnumLicenseType.Premium => licensePremiumDescription,
            _ => licenseNoDescription
        };
        CurrentLicense = new TgLicenseDto(description, licenseKey, licenseType, userId, validTo, isConfirmed);
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

    /// <summary> Updates existing license or creates a new one if it does not exist </summary>
    public async Task LicenseUpdateAsync(TgLicenseDto licenseDto)
	{
        try
        {
            var licenseEntity = new TgEfLicenseEntity
            {
                IsConfirmed = licenseDto.IsConfirmed,
                LicenseKey = licenseDto.LicenseKey,
                LicenseType = licenseDto.LicenseType,
                UserId = licenseDto.UserId,
                ValidTo = DateTime.Parse($"{licenseDto.ValidTo:yyyy-MM-dd}")
            };

            var listDtos = await StorageManager.LicenseRepository.GetListDtosAsync(take: 0, skip: 0);
            if (listDtos is null || listDtos.Count == 0)
            {
                await StorageManager.LicenseRepository.SaveAsync(licenseEntity);
            }
            else
            {
                var licenseExists = await StorageManager.LicenseRepository.GetItemAsync(licenseEntity, isReadOnly: false);
                licenseEntity = TgEfDomainUtils.CreateNewEntity(licenseExists, isUidCopy: false);
                await StorageManager.LicenseRepository.SaveAsync(licenseEntity);
            }
        }
        catch (Exception ex)
        {
            TgLogUtils.WriteException(ex);
        }
	}

    public async Task<TgApiResult> GetApiLicenseStatisticAsync(DateOnly lastPromoDay)
    {
        var result = new TgApiResult();
        var licenseCountDto = new TgLicenseCountDto() { LastPromoDay = lastPromoDay };

        // Search created licenses
        var licenseCreatedDtos = await StorageManager.LicenseRepository.GetListDtosAsync(take: 0, skip: 0);
        if (licenseCreatedDtos.Count != 0)
        {
            licenseCountDto.CreatedNoneCount = licenseCreatedDtos.Where(x => x.LicenseType == TgEnumLicenseType.No).Count();
            licenseCountDto.CreatedCommunityCount = licenseCreatedDtos.Where(x => x.LicenseType == TgEnumLicenseType.Community).Count();
            licenseCountDto.CreatedPaidCount = licenseCreatedDtos.Where(x => x.LicenseType == TgEnumLicenseType.Paid).Count();
            licenseCountDto.CreatedGiftCount = licenseCreatedDtos.Where(x => x.LicenseType == TgEnumLicenseType.Gift).Count();
            licenseCountDto.CreatedPremiumCount = licenseCreatedDtos.Where(x => x.LicenseType == TgEnumLicenseType.Premium).Count();
            result.IsOk = true;
        }

        // Search valid licenses
        var licenseValidDtos = await StorageManager.LicenseRepository.GetListDtosAsync(take: 0, skip: 0, x => x.ValidTo >= DateTime.UtcNow.Date);
        if (licenseValidDtos.Count != 0)
        {
            licenseCountDto.ValidNoneCount = licenseValidDtos.Where(x => x.LicenseType == TgEnumLicenseType.No).Count();
            licenseCountDto.ValidCommunityCount = licenseValidDtos.Where(x => x.LicenseType == TgEnumLicenseType.Community).Count();
            licenseCountDto.ValidPaidCount = licenseValidDtos.Where(x => x.LicenseType == TgEnumLicenseType.Paid).Count();
            licenseCountDto.ValidGiftCount = licenseValidDtos.Where(x => x.LicenseType == TgEnumLicenseType.Gift).Count();
            licenseCountDto.ValidPremiumCount = licenseValidDtos.Where(x => x.LicenseType == TgEnumLicenseType.Premium).Count();
            result.IsOk = true;
        }

        result.Value = licenseCountDto;
        return result;
    }

    /// <inheritdoc />
    public async Task<TgApiResult> CreateAsync(long userId, TgEnumLicenseType licenseType, DateOnly validTo)
    {
        var result = new TgApiResult();
        if (userId <= 0)
        {
            result.IsOk = false;
            result.Value = "User ID must be greater than zero!";
            return await Task.FromResult(result);
        }
        var licenseDto = new TgLicenseDto(licenseKey: Guid.NewGuid(), licenseType, userId, validTo, isConfirmed: true);
        await LicenseUpdateAsync(licenseDto);
        ActivateLicense(licenseDto.IsConfirmed, licenseDto.LicenseKey, licenseDto.LicenseType, licenseDto.UserId, licenseDto.ValidTo);
        result.IsOk = true;
        result.Value = licenseDto;
        return await Task.FromResult(result);
    }

    #endregion
}
