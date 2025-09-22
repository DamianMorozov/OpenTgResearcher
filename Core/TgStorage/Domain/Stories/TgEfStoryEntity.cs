namespace TgStorage.Domain.Stories;

/// <summary> EF story entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(DtChanged))]
[Index(nameof(Id))]
[Index(nameof(FromId))]
[Index(nameof(FromName))]
[Index(nameof(Date))]
[Index(nameof(ExpireDate))]
[Index(nameof(Caption))]
[Index(nameof(Type))]
public sealed class TgEfStoryEntity : ITgEfStoryEntity
{
	#region Fields, properties, constructor

	[DefaultValue("00000000-0000-0000-0000-000000000000")]
	[Key]
	[Required]
	[Column(TgEfConstants.ColumnUid, TypeName = "CHAR(36)")]
	[SQLite.Collation("NOCASE")]
	public Guid Uid { get; set; }

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

    public TgEfStoryEntity() => Default();

    #endregion

    #region Methods

    /// <inheritdoc />
    public string ToDebugString() => TgObjectUtils.ToDebugString(this);

    /// <inheritdoc />
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

	#endregion
}
