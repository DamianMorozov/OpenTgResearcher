// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Stories;

/// <summary> Story entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(DtChanged))]
[Index(nameof(Id), IsUnique = true)]
[Index(nameof(FromId))]
[Index(nameof(FromName))]
[Index(nameof(Date))]
[Index(nameof(ExpireDate))]
[Index(nameof(Caption))]
[Index(nameof(Type))]
public sealed class TgEfStoryEntity : ITgEfEntity<TgEfStoryEntity>
{
	#region Public and private fields, properties, constructor

	[DefaultValue("00000000-0000-0000-0000-000000000000")]
	[Key]
	[Required]
	[Column(TgEfConstants.ColumnUid, TypeName = "CHAR(36)")]
	[SQLite.Collation("NOCASE")]
	public Guid Uid { get; set; }

	[Timestamp]
	[Column(TgEfConstants.ColumnRowVersion)]
	public byte[]? RowVersion { get; set; }

	[DefaultValue("0001-01-01 00:00:00")]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnDtChanged, TypeName = "DATETIME")]
	public DateTime DtChanged { get; set; }

	[DefaultValue(-1)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnId, TypeName = "LONG(20)")]
	public long Id { get; set; }
	
	[DefaultValue(-1)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnFromId, TypeName = "LONG(20)")]
	public long? FromId { get; set; }

	[DefaultValue("")]
    [ConcurrencyCheck]
    [MaxLength(128)]
    [Column(TgEfConstants.ColumnFromName, TypeName = "NVARCHAR(128)")]
    public string? FromName { get; set; }

	[DefaultValue("0001-01-01 00:00:00")]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnDate, TypeName = "DATETIME")]
	public DateTime? Date { get; set; }
	
	[DefaultValue("0001-01-01 00:00:00")]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnExpireDate, TypeName = "DATETIME")]
	public DateTime? ExpireDate { get; set; }
	
	[DefaultValue("")]
    [ConcurrencyCheck]
    [MaxLength(128)]
    [Column(TgEfConstants.ColumnCaption, TypeName = "NVARCHAR(128)")]
    public string? Caption { get; set; }

	[DefaultValue("")]
    [ConcurrencyCheck]
    [MaxLength(128)]
    [Column(TgEfConstants.ColumnType, TypeName = "NVARCHAR(128)")]
    public string? Type { get; set; }

	[DefaultValue(-1)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnOffset, TypeName = "INT(20)")]
	public int Offset { get; set; }

	[DefaultValue(-1)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnLength, TypeName = "INT(20)")]
	public int Length { get; set; }

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(256)]
	[Column(TgEfConstants.ColumnMessage, TypeName = "NVARCHAR(256)")]
	public string? Message { get; set; } = null!;
	
    public TgEfStoryEntity() : base()
    {
        Default();
    }

	#endregion

	#region Public and private methods

	public string ToDebugString() => TgObjectUtils.ToDebugString(this);

    public void Default()
    {
		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
		DtChanged = this.GetDefaultPropertyDateTime(nameof(DtChanged));
		Id = this.GetDefaultPropertyLong(nameof(Id));
		FromId = this.GetDefaultPropertyLong(nameof(FromId));
		FromName = this.GetDefaultPropertyString(nameof(FromName));
		Date = this.GetDefaultPropertyDateTime(nameof(Date));
		ExpireDate = this.GetDefaultPropertyDateTime(nameof(ExpireDate));
		Caption = this.GetDefaultPropertyString(nameof(Caption));
		Type = this.GetDefaultPropertyString(nameof(Type));
		Offset = this.GetDefaultPropertyInt(nameof(Offset));
		Length = this.GetDefaultPropertyInt(nameof(Length));
		Message = this.GetDefaultPropertyString(nameof(Message));
    }

    public TgEfStoryEntity Copy(TgEfStoryEntity item, bool isUidCopy)
    {
		if (isUidCopy)
			Uid = item.Uid;
		DtChanged = item.DtChanged > DateTime.MinValue ? item.DtChanged : DateTime.Now;
		if (Id == this.GetDefaultPropertyLong(nameof(Id)))
			Id = item.Id;
		FromId = item.FromId;
		FromName = item.FromName;
		Date = item.Date;
		ExpireDate = item.ExpireDate;
		Caption = item.Caption;
		Type = item.Type;
		Offset = item.Offset;
		Length = item.Length;
		Message = item.Message;
        return this;
	}

	public string ToConsoleString()
	{
		var captionTrimmed = string.IsNullOrEmpty(Caption) ? string.Empty
			: Caption.Contains('\n')
				? Caption[..Caption.IndexOf('\n')] : Caption;
		return $"{Id,11} | " +
		$"{TgDataFormatUtils.GetFormatString(FromName, 25).TrimEnd(),-25} | " +
		$"{(Date is null ? "" : Date.ToString()),19} | " +
		$"{TgDataFormatUtils.GetFormatString(captionTrimmed, 64).TrimEnd(),64}";
	}

	#endregion
}