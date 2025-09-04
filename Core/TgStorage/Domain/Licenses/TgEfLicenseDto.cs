// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Apps;

/// <summary> App DTO </summary>
public sealed partial class TgEfLicenseDto : TgDtoBase, ITgDto<TgEfLicenseEntity, TgEfLicenseDto>
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial Guid LicenseKey { get; set; }
	[ObservableProperty]
	public partial long UserId { get; set; }
	[ObservableProperty]
	public partial TgEnumLicenseType LicenseType { get; set; }
	[ObservableProperty]
    [JsonConverter(typeof(TgDateOnlyJsonConverter))]
    public partial DateOnly ValidTo { get; set; }
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
		LicenseKey = Guid.Empty;
		UserId = 0;
		LicenseType = TgEnumLicenseType.Free;
		ValidTo = DateOnly.MinValue;
		IsConfirmed = false;
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

	public TgEfLicenseDto GetNewDto(TgEfLicenseEntity item) => new TgEfLicenseDto().Copy(item, isUidCopy: true);

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
