namespace TgStorage.Domain.Apps;

/// <summary> App DTO </summary>
public sealed partial class TgEfLicenseDto : TgDtoBase, ITgDto<TgEfLicenseEntity, TgEfLicenseDto>
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

    public override string ToString() => $"{LicenseKey} | {UserId}";

    public TgEfLicenseDto Copy(TgEfLicenseDto dto, bool isUidCopy)
    {
        base.Copy(dto, isUidCopy);
        LicenseKey = dto.LicenseKey;
        UserId = dto.UserId;
        LicenseType = dto.LicenseType;
        ValidTo = dto.ValidTo;
        IsConfirmed = dto.IsConfirmed;
        return this;
    }

    public TgEfLicenseDto Copy(TgEfLicenseEntity item, bool isUidCopy)
    {
        if (isUidCopy)
            Uid = item.Uid;
        LicenseKey = item.LicenseKey;
        UserId = item.UserId;
        LicenseType = item.LicenseType;
        ValidTo = DateOnly.FromDateTime(item.ValidTo);
        IsConfirmed = item.IsConfirmed;
        return this;
    }

    public TgEfLicenseEntity GetEntity() => new()
    {
        Uid = Uid,
        LicenseKey = LicenseKey,
        UserId = UserId,
        LicenseType = LicenseType,
        ValidTo = DateTime.Parse($"{ValidTo:yyyy-MM-d}"),
        IsConfirmed = IsConfirmed,
    };

    #endregion
}
