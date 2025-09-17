namespace TgStorage.Domain.Stories;

/// <summary> Proxy DTO </summary>
public sealed partial class TgEfStoryDto : TgDtoBase, ITgDto<TgEfStoryEntity, TgEfStoryDto>
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

	public string DtChangedString => $"{DtChanged:yyyy-MM-dd HH:mm:ss}";

    public override string ToConsoleString()
    {
        var captionTrimmed = string.IsNullOrEmpty(Caption) ? string.Empty
            : Caption.Contains('\n') ? Caption[..Caption.IndexOf('\n')] : Caption;
        return $"{Id,11} | " +
        $"{TgDataFormatUtils.GetFormatString(FromName, 25).TrimEnd(),-25} | " +
        $"{Date,19} | " +
        $"{TgDataFormatUtils.GetFormatString(captionTrimmed, 64).TrimEnd(),64}";
    }

    public override string ToConsoleHeaderString() =>
        $"{TgDataFormatUtils.GetFormatString(nameof(Id), 11).TrimEnd(),-11} | " +
        $"{TgDataFormatUtils.GetFormatString(nameof(FromName), 25).TrimEnd(),-25} | " +
        $"{TgDataFormatUtils.GetFormatString(nameof(Date), 19).TrimEnd(),-19} | " +
        $"Caption";

    public TgEfStoryDto Copy(TgEfStoryDto dto, bool isUidCopy)
	{
		base.Copy(dto, isUidCopy);
		DtChanged = dto.DtChanged;
		Id = dto.Id;
		FromId = dto.FromId;
		FromName = dto.FromName;
		Date = dto.Date;
		ExpireDate = dto.ExpireDate;
		Caption = dto.Caption;
		Type = dto.Type;
		Offset = dto.Offset;
		Length = dto.Length;
		Message = dto.Message;
		IsDownload = dto.IsDownload;
		return this;
	}

	public TgEfStoryDto Copy(TgEfStoryEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		DtChanged = item.DtChanged;
		Id = item.Id;
		FromId = item.FromId ?? 0;
		FromName = item.FromName ?? string.Empty;
		Date = item.Date ?? DateTime.MinValue;
		ExpireDate = item.ExpireDate ?? DateTime.MinValue;
		Caption = item.Caption ?? string.Empty;
		Type = item.Type ?? string.Empty;
		Offset = item.Offset;
		Length = item.Length;
		Message = item.Message ?? string.Empty;

		IsDownload = false;

		return this;
	}

	public TgEfStoryEntity GetEntity() => new()
	{
		Uid = Uid,
		DtChanged = DtChanged,
		Id = Id,
		FromId = FromId,
		FromName = FromName,
		Date = Date,
		ExpireDate = ExpireDate,
		Caption = Caption,
		Type = Type,
		Offset = Offset,
		Message = Message,
	};

	#endregion
}
