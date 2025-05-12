// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgDownloaderConsole.Helpers;

public sealed class TgLicenseApiResponse
{
	#region Public and private fields, properties, constructor

	public bool IsConfirmed { get; set; }
	public Guid LicenseKey { get; set; } = Guid.Empty;
	public TgEnumLicenseType LicenseType { get; set; } = TgEnumLicenseType.Free;
	public long UserId { get; set; }
	public DateOnly ValidTo { get; set; } = DateOnly.MinValue;

	#endregion
}