namespace TgStorage.Domain.Messages;

/// <summary> EF message DTO </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfMessageRelationDto : TgDtoBase, ITgEfMessageRelationDto
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
}
