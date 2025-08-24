// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Dtos;

public sealed class TgLicenseCountDto
{
	#region Fields, properties, constructor

	public int TestCount { get; set; }
	public int PaidCount { get; set; }
	public int GiftCount { get; set; }
	public int PremiumCount { get; set; }
	public string Description =>
		$"Test licenses: {TestCount} pcs." + Environment.NewLine +
		$"Paid licenses: {PaidCount} pcs." + Environment.NewLine +
		$"Gift licenses: {GiftCount} pcs." + Environment.NewLine +
		$"Premium licenses: {PremiumCount} pcs.";

	public TgLicenseCountDto(int testCount, int paidCount, int giftCount, int premiumCount)
	{
		TestCount = testCount;
		PaidCount = paidCount;
        GiftCount = giftCount;
        PremiumCount = premiumCount;
	}

	public TgLicenseCountDto()
	{
		TestCount = 0;
		PaidCount = 0;
        GiftCount = 0;
        PremiumCount = 0;
	}

	#endregion
}
