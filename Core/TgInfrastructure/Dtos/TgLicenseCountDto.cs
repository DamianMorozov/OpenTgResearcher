// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Dtos;

public sealed class TgLicenseCountDto
{
    #region Fields, properties, constructor

    public DateOnly LastPromoDay { get; set; } = DateOnly.MinValue;
    public int CreatedGiftCount { get; set; }
	public int CreatedPaidCount { get; set; }
	public int CreatedPremiumCount { get; set; }
	public int CreatedTestCount { get; set; }
	public int ValidGiftCount { get; set; }
	public int ValidPaidCount { get; set; }
	public int ValidPremiumCount { get; set; }
	public int ValidTestCount { get; set; }

	#endregion
}
