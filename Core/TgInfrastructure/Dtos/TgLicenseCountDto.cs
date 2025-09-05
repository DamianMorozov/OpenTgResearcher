// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Dtos;

/// <summary> License count DTO </summary>
public sealed class TgLicenseCountDto
{
    #region Fields, properties, constructor

    public DateOnly LastPromoDay { get; set; } = DateOnly.MinValue;
    public int CreatedGiftCount { get; set; }
	public int CreatedCommunityCount { get; set; }
	public int CreatedNoneCount { get; set; }
	public int CreatedPaidCount { get; set; }
	public int CreatedPremiumCount { get; set; }
	public int ValidCommunityCount { get; set; }
	public int ValidGiftCount { get; set; }
	public int ValidNoneCount { get; set; }
	public int ValidPaidCount { get; set; }
	public int ValidPremiumCount { get; set; }

	#endregion
}
