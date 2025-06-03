// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Dtos;

public sealed class TgPromoStatisticSummaryDto
{
    #region Public and private fields, properties, constructor

    public DateOnly LastPromoDay { get; set; } = DateOnly.MinValue;
    public List<TgPromoStatisticDto> Items { get; set; }

	public TgPromoStatisticSummaryDto(DateOnly lastPromoDay, List<TgPromoStatisticDto>? items = null)
	{
        LastPromoDay = lastPromoDay;
        Items = items ?? [];
	}

	public TgPromoStatisticSummaryDto()
	{
        LastPromoDay = DateOnly.MinValue;
        Items = [];
	}

	#endregion
}