namespace TgStorage.Domain.Messages;

/// <summary> Message DTO </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfMessageRelationDto : TgDtoBase, ITgDto<TgEfMessageRelationEntity, TgEfMessageRelationDto>
{
	#region Fields, properties, constructor

    [ObservableProperty]
	public partial long ParentSourceId { get; set; }
	[ObservableProperty]
	public partial int ParentMessageId { get; set; }
    [ObservableProperty]
	public partial long ChildSourceId { get; set; }
	[ObservableProperty]
	public partial int ChildMessageId { get; set; }

    public TgEfMessageRelationDto() : base()
	{
        ParentSourceId = 0;
        ParentMessageId = 0;
        ChildSourceId = 0;
        ChildMessageId = 0;
    }

	#endregion

	#region Methods

    public TgEfMessageRelationDto Copy(TgEfMessageRelationDto dto, bool isUidCopy)
	{
		base.Copy(dto, isUidCopy);
        ParentSourceId = dto.ParentSourceId;
        ParentMessageId = dto.ParentMessageId;
        ChildSourceId = dto.ChildSourceId;
        ChildMessageId = dto.ChildMessageId;
        return this;
	}

	public TgEfMessageRelationDto Copy(TgEfMessageRelationEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
        ParentSourceId = item.ParentSourceId;
        ParentMessageId = item.ParentMessageId;
        ChildSourceId = item.ChildSourceId;
        ChildMessageId = item.ChildMessageId;
        return this;
	}

	public TgEfMessageRelationEntity GetEntity() => new()
	{
		Uid = Uid,
        ParentSourceId = ParentSourceId,
        ParentMessageId = ParentMessageId,
        ChildSourceId = ChildSourceId,
        ChildMessageId = ChildMessageId,
    };

    #endregion
}
