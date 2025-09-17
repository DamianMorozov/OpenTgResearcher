namespace TgStorage.Common;

/// <summary> Empty DTO </summary>
public sealed partial class TgEfEmptyDto : TgDtoBase, ITgDto<TgEmptyEntity, TgEfEmptyDto>
{
	#region Fields, properties, constructor

	public TgEfEmptyDto() : base()
	{
		//
	}

	#endregion

	#region Methods

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

	public TgEmptyEntity GetEntity() => new()
	{
		Uid = Uid,
	};

	#endregion
}
