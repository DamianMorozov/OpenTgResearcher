// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Dtos;

[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgLicenseDto : ObservableRecipient
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial string Description { get; private set; } = string.Empty;
	[ObservableProperty]
	public partial Guid LicenseKey { get; set; } = Guid.Empty;
	[ObservableProperty]
	public partial TgEnumLicenseType LicenseType { get; set; } = TgEnumLicenseType.Free;
	[ObservableProperty]
	public partial long UserId { get; set; }
	[ObservableProperty]
    [JsonConverter(typeof(TgDateOnlyJsonConverter))]
    public partial DateOnly ValidTo { get; set; } = DateOnly.MinValue;
    [ObservableProperty]
	public partial bool IsConfirmed { get; set; }

    #endregion

    #region Methods

    /// <summary> Check paid license </summary>
    public bool CheckPaidLicense() => LicenseType switch
	{
		TgEnumLicenseType.Test or TgEnumLicenseType.Paid or TgEnumLicenseType.Gift or TgEnumLicenseType.Premium => 
            IsConfirmed && DateOnly.FromDateTime(DateTime.UtcNow) <= ValidTo,
		_ => false,
	};

	public string ToDebugString() => Description;

	public void SetDescription(string description) => Description = description;

	public string GetLicenseKeyString() => LicenseKey == Guid.Empty ? "-" : $"{LicenseKey:D}";

	public string GetUserIdString() => UserId == 0 ? "-" : $"{UserId}";

	public string GetValidToString() => ValidTo <= DateOnly.MinValue ? "-" : $"{ValidTo:yyyy-MM-dd}";

	#endregion
}
