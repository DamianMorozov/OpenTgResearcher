// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Common;

/// <summary> Empty DTO </summary>
public sealed partial class TgEfEmptyDto : TgDtoBase, ITgDto<TgEfEmptyDto, TgEmptyEntity>
{
	#region Public and private fields, properties, constructor

	//

	#endregion

	#region Public and private methods

	public TgEfEmptyDto Fill(TgEfEmptyDto dto, bool isUidCopy)
	{
		base.Fill(dto, isUidCopy);
		return this;
	}

	public TgEfEmptyDto Fill(TgEmptyEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		return this;
	}

	public TgEfEmptyDto GetDto(TgEmptyEntity item)
	{
		var dto = new TgEfEmptyDto();
		dto.Fill(item, isUidCopy: true);
		return dto;
	}

	public TgEmptyEntity GetEntity() => new()
	{
		Uid = Uid,
	};

	#endregion
}
