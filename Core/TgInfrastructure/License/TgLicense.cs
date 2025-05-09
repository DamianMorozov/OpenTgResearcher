// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.License;

[DebuggerDisplay("{ToDebugString()}")]
public partial class TgLicense : ObservableRecipient
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial string Description { get; private set; } = "Empty license";
	[ObservableProperty]
	public partial Guid LicenseKey { get; set; } = Guid.Empty;
	[ObservableProperty]
	public partial TgEnumLicenseType LicenseType { get; set; } = TgEnumLicenseType.Free;
	[ObservableProperty]
	public partial long UserId { get; set; }
	[ObservableProperty]
	public partial DateTime ValidTo { get; set; } = DateTime.MinValue;
	[ObservableProperty]
	public partial bool IsConfirmed { get; set; }

	#endregion

	#region Public and private methods

	public bool IsValid()
	{
		switch (LicenseType)
		{
			case TgEnumLicenseType.Free:
				return true;
			case TgEnumLicenseType.Test:
			case TgEnumLicenseType.Paid:
			case TgEnumLicenseType.Premium:
				return IsConfirmed && DateTime.Now <= ValidTo;
		}
		return false;
	}

	public string ToDebugString() => Description;

	public void SetDescription(string description)
	{
		Description = description;
	}

	public string GetLicenseKeyString() => LicenseKey == Guid.Empty ? "-" : $"{LicenseKey:D}";

	public string GetUserIdString() => UserId == 0 ? "-" : $"{UserId}";

	public string GetValidToString() => ValidTo <= DateTime.MinValue ? "-" : $"{ValidTo:yyyy-MM-dd}";

	#endregion
}
