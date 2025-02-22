// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Common;

/// <summary> Empty DTO </summary>
public sealed partial class TgEfEmptyDto : TgDtoBase, ITgDto<TgEmptyEntity, TgEfEmptyDto>
{
	#region Public and private methods

	public override string ToString() => base.ToString();

	public TgEfEmptyDto Copy(TgEfEmptyDto dto, bool isUidCopy)
	{
		base.Copy(dto, isUidCopy);
		return this;
	}

	public TgEfEmptyDto Copy(TgEmptyEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		return this;
	}

	public TgEfEmptyDto GetNewDto(TgEmptyEntity item) => new TgEfEmptyDto().Copy(item, isUidCopy: true);

	public TgEmptyEntity GetNewEntity(TgEfEmptyDto dto) => new()
	{
		Uid = dto.Uid,
	};

	public TgEmptyEntity GetNewEntity() => new()
	{
		Uid = Uid,
	};

	#endregion
}
