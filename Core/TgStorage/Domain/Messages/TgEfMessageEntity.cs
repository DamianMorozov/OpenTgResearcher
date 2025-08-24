// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Messages;

/// <summary> Message entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(SourceId))]
[Index(nameof(Id))]
[Index(nameof(DtCreated))]
[Index(nameof(Type))]
[Index(nameof(Size))]
[Index(nameof(Message))]
public sealed class TgEfMessageEntity : ITgEfEntity<TgEfMessageEntity>
{
	#region Fields, properties, constructor

	[DefaultValue("00000000-0000-0000-0000-000000000000")]
	[Key]
	[Required]
	[Column(TgEfConstants.ColumnUid, TypeName = "CHAR(36)")]
	[SQLite.Collation("NOCASE")]
	public Guid Uid { get; set; }

	[DefaultValue(0)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnSourceId, TypeName = "LONG(20)")]
    public long SourceId { get; set; }

	[ForeignKey(nameof(SourceId))]
	public TgEfSourceEntity? Source { get; set; }

	[DefaultValue(0)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnId, TypeName = "INT")]
    public int Id { get; set; }

    [DefaultValue("0001-01-01 00:00:00")]
	[ConcurrencyCheck]
    [Column(TgEfConstants.ColumnDtCreated, TypeName = "DATETIME")]
    public DateTime DtCreated { get; set; }

    [DefaultValue(TgEnumMessageType.Message)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnType, TypeName = "INT")]
    public TgEnumMessageType Type { get; set; }

    [DefaultValue(0)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnSize, TypeName = "LONG(20)")]
    public long Size { get; set; }

    [DefaultValue("")]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnMessage, TypeName = "NVARCHAR(100)")]
    public string Message { get; set; } = null!;

    [DefaultValue(0)]
    [Column(TgEfConstants.ColumnUserId, TypeName = "LONG(20)")]
    public long UserId { get; set; }
    
    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsDeleted, TypeName = "BIT")]
    public bool IsDeleted { get; set; }

    public TgEfMessageEntity()
    {
        Default();
	}

	#endregion

	#region Methods

	public string ToDebugString() => TgObjectUtils.ToDebugString(this);

	public void Default()
    {
		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
		SourceId = this.GetDefaultPropertyLong(nameof(SourceId));
		Id = this.GetDefaultPropertyInt(nameof(Id));
	    DtCreated = this.GetDefaultPropertyDateTime(nameof(DtCreated));
		Type = this.GetDefaultPropertyGeneric<TgEnumMessageType>(nameof(Type));
	    Size = this.GetDefaultPropertyLong(nameof(Size));
	    Message = this.GetDefaultPropertyString(nameof(Message));
        UserId = this.GetDefaultPropertyLong(nameof(UserId));
        IsDeleted = this.GetDefaultPropertyBool(nameof(IsDeleted));
    }

    public TgEfMessageEntity Copy(TgEfMessageEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		SourceId = item.SourceId;
		Id = item.Id;
		DtCreated = item.DtCreated > DateTime.MinValue ? item.DtCreated : DateTime.Now;
		Type = item.Type;
		Size = item.Size;
		Message = item.Message;
        UserId = item.UserId;
        IsDeleted = item.IsDeleted;
        return this;
	}

	#endregion
}