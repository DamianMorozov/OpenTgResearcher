// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgSharedData.Dtos;

public sealed class TgLicenseCountDto
{
	#region Public and private fields, properties, constructor

	public int TestCount { get; set; }
	public int PaidCount { get; set; }
	public int PreimumCount { get; set; }
	public string Description =>
		$"Test licenses: {TestCount} pcs." + Environment.NewLine +
		$"Paid licenses: {PaidCount} pcs." + Environment.NewLine +
		$"Premium licenses: {PreimumCount} pcs.";

	public TgLicenseCountDto(int testCount, int paidCount, int preimumCount)
	{
		TestCount = testCount;
		PaidCount = paidCount;
		PreimumCount = preimumCount;
	}

	public TgLicenseCountDto()
	{
		TestCount = 0;
		PaidCount = 0;
		PreimumCount = 0;
	}

	#endregion
}