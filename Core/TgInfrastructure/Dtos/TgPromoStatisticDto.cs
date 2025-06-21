// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Dtos;

public sealed class TgPromoStatisticDto
{
    #region Public and private fields, properties, constructor

    public DateOnly PromoDate { get; set; } = DateOnly.MinValue;
    public int TestCount { get; set; }
    public int Limit { get; set; }

	public TgPromoStatisticDto(DateOnly promoDate, int testCount, int limit)
	{
        PromoDate = promoDate;
        TestCount = testCount;
        Limit = limit;
	}

	public TgPromoStatisticDto()
	{
        PromoDate = DateOnly.MinValue;
        TestCount = 0;
        Limit = 0;
    }

	#endregion
}