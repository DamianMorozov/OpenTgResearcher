namespace TgStorage.Domain.ChatUsers;

/// <summary> EF chat user DTO </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfChatUserDto : TgDtoBase, ITgEfChatUserDto
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial DateTime DtChanged { get; set; }
    public string DtChangedString => $"{DtChanged:yyyy-MM-dd HH:mm:ss}";
    [ObservableProperty]
	public partial long ChatId { get; set; }
    [ObservableProperty]
	public partial long UserId { get; set; }
	[ObservableProperty]
	public partial TgEnumChatUserRole Role { get; set; }
	[ObservableProperty]
	public partial DateTime JoinedAt { get; set; }
	[ObservableProperty]
	public partial bool IsMuted { get; set; }
	[ObservableProperty]
	public partial DateTime? MutedUntil { get; set; }
    [ObservableProperty]
    public partial bool IsDeleted { get; set; }

    public TgEfChatUserDto() : base()
	{
		DtChanged = DateTime.MinValue;
		ChatId = 0;
		UserId = 0;
		Role = TgEnumChatUserRole.Member;
        JoinedAt = DateTime.MinValue;
        IsMuted = false;
        MutedUntil = null;
        IsDeleted = false;
    }

	#endregion
}
