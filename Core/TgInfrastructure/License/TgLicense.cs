// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.License;

[DebuggerDisplay("{ToDebugString()}")]
public abstract partial class TgLicense : ObservableRecipient
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial string Description { get; set; } = "Empty license";
	[ObservableProperty]
	public partial string LicenseKey { get; set; } = string.Empty;
	public TgEnumLicenseType LicenseType { get; set; } = TgEnumLicenseType.Free;

	#endregion

	#region Public and private methods

	public virtual bool IsValid() => false;

	public string ToDebugString() => Description;

	#endregion
}
