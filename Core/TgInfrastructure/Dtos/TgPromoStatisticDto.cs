// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Dtos;

public sealed class TgPromoStatisticDto
{
    #region Public and private fields, properties, constructor

    public int TestCount { get; set; }
    public int PaidCount { get; set; }
    public int GiftCount { get; set; }
    public int PremiumCount { get; set; }
    public int Limit { get; set; }

	public TgPromoStatisticDto()
	{
        TestCount = 0;
        PaidCount = 0;
        GiftCount = 0;
        PremiumCount = 0;
        Limit = 0;
    }

	#endregion
}
