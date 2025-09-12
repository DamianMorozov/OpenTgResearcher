namespace TgInfrastructure.Dtos;

/// <summary> License DTO </summary>
public sealed partial class TgLicenseDto : ObservableRecipient
{
    #region Fields, properties, constructor

    [ObservableProperty]
	public partial string Description { get; private set; } = string.Empty;
	[ObservableProperty]
	public partial Guid LicenseKey { get; private set; } = Guid.Empty;
	[ObservableProperty]
	public partial TgEnumLicenseType LicenseType { get; private set; } = TgEnumLicenseType.No;
	[ObservableProperty]
	public partial long UserId { get; private set; }
	[ObservableProperty]
    [JsonConverter(typeof(TgDateOnlyJsonConverter))]
    public partial DateOnly ValidTo { get; private set; } = DateOnly.MinValue;
    [ObservableProperty]
	public partial bool IsConfirmed { get; private set; }

    public string GetLicenseKeyString => LicenseKey == Guid.Empty ? "-" : $"{LicenseKey:D}";
    public string GetUserIdString => UserId == 0 ? "-" : $"{UserId}";
    public string GetValidToString => ValidTo <= DateOnly.MinValue ? "-" : $"{ValidTo:yyyy-MM-dd}";

    public TgLicenseDto(string description, Guid licenseKey, TgEnumLicenseType licenseType, long userId, DateOnly validTo, bool isConfirmed)
    {
        Description = description;
        LicenseKey = licenseKey;
        LicenseType = licenseType;
        UserId = userId;
        ValidTo = validTo;
        IsConfirmed = isConfirmed;
    }

    public TgLicenseDto(Guid licenseKey, TgEnumLicenseType licenseType, long userId, DateOnly validTo, bool isConfirmed)
    {
        LicenseKey = licenseKey;
        LicenseType = licenseType;
        UserId = userId;
        ValidTo = validTo;
        IsConfirmed = isConfirmed;
    }

    public TgLicenseDto()
    {
        //
    }

    #endregion

    #region Methods

    /// <summary> Check paid license </summary>
    public bool CheckPaidLicense() => LicenseType switch
	{
		TgEnumLicenseType.Paid or TgEnumLicenseType.Gift or TgEnumLicenseType.Premium => IsConfirmed && DateOnly.FromDateTime(DateTime.UtcNow) <= ValidTo,
		_ => false,
	};

	#endregion
}
