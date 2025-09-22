namespace TgStorage.Domain.Apps;

/// <summary> EF license DTO </summary>
public sealed partial class TgEfLicenseDto : TgDtoBase, ITgEfLicenseDto
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial Guid LicenseKey { get; set; } = Guid.Empty;
    [ObservableProperty]
    public partial long UserId { get; set; }
    [ObservableProperty]
    public partial TgEnumLicenseType LicenseType { get; set; } = TgEnumLicenseType.No;
    [ObservableProperty]
    [JsonConverter(typeof(TgDateOnlyJsonConverter))]
    public partial DateOnly ValidTo { get; set; } = DateOnly.MinValue;
    [ObservableProperty]
    public partial bool IsConfirmed { get; set; }

    public string ApiHashString
    {
        get => LicenseKey.ToString();
        set => LicenseKey = Guid.TryParse(value, out var apiHash) ? apiHash : Guid.Empty;
    }

    public string UserIdString
    {
        get => UserId.ToString();
        set => UserId = int.TryParse(value, out var apiId) ? apiId : 0;
    }

    public TgEfLicenseDto() : base()
    {
        //
    }

    public TgEfLicenseDto(TgLicenseDto licenseDto) : base()
    {
        LicenseKey = licenseDto.LicenseKey;
        UserId = licenseDto.UserId;
        LicenseType = licenseDto.LicenseType;
        ValidTo = licenseDto.ValidTo;
        IsConfirmed = licenseDto.IsConfirmed;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override string ToString() => $"{LicenseKey} | {UserId}";

    #endregion
}
