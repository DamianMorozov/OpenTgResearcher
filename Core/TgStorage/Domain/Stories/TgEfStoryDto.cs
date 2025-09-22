namespace TgStorage.Domain.Stories;

/// <summary> EF proxy DTO </summary>
public sealed partial class TgEfStoryDto : TgDtoBase, ITgEfStoryDto
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial DateTime DtChanged { get; set; }
	[ObservableProperty]
	public partial long Id { get; set; }
	[ObservableProperty]
	public partial long FromId { get; set; }
	[ObservableProperty]
	public partial string FromName { get; set; }
	[ObservableProperty]
	public partial DateTime Date { get; set; }
	[ObservableProperty]
	public partial DateTime ExpireDate { get; set; }
	[ObservableProperty]
	public partial string Caption { get; set; }
	[ObservableProperty]
	public partial string Type { get; set; }
	[ObservableProperty]
	public partial int Offset { get; set; }
	[ObservableProperty]
	public partial int Length { get; set; }
	[ObservableProperty]
	public partial string Message { get; set; }
	[ObservableProperty]
	public partial bool IsDownload { get; set; }
	public bool IsReady => Id > 0;

	public TgEfStoryDto() : base()
	{
		DtChanged = DateTime.MinValue;
		Id = 0;
		FromId = 0;
		FromName = string.Empty;
		Date = DateTime.MinValue;
		ExpireDate = DateTime.MinValue;
		Caption = string.Empty;
		Type = string.Empty;
		Offset = 0;
		Length = 0;
		Message = string.Empty;
		IsDownload = false;
	}

    #endregion

    #region Methods

    /// <inheritdoc />
    public string DtChangedString => $"{DtChanged:yyyy-MM-dd HH:mm:ss}";

    /// <inheritdoc />
    public override string ToConsoleString()
    {
        var captionTrimmed = string.IsNullOrEmpty(Caption) ? string.Empty
            : Caption.Contains('\n') ? Caption[..Caption.IndexOf('\n')] : Caption;
        return $"{Id,11} | " +
        $"{TgDataFormatUtils.GetFormatString(FromName, 25).TrimEnd(),-25} | " +
        $"{Date,19} | " +
        $"{TgDataFormatUtils.GetFormatString(captionTrimmed, 64).TrimEnd(),64}";
    }

    /// <inheritdoc />
    public override string ToConsoleHeaderString() =>
        $"{TgDataFormatUtils.GetFormatString(nameof(Id), 11).TrimEnd(),-11} | " +
        $"{TgDataFormatUtils.GetFormatString(nameof(FromName), 25).TrimEnd(),-25} | " +
        $"{TgDataFormatUtils.GetFormatString(nameof(Date), 19).TrimEnd(),-19} | " +
        $"Caption";

	#endregion
}
