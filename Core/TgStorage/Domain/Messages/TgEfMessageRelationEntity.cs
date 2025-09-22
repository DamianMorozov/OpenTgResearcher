namespace TgStorage.Domain.Messages;

/// <summary> Message relation entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(ParentSourceId))]
[Index(nameof(ParentMessageId))]
[Index(nameof(ChildSourceId))]
[Index(nameof(ChildMessageId))]
public sealed class TgEfMessageRelationEntity : ITgEfEntity<TgEfMessageRelationEntity>
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
    [Column(TgEfConstants.ColumnParentSourceId, TypeName = "LONG(20)")]
    [Required]
    public long ParentSourceId { get; set; }

    [DefaultValue(0)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnParentMessageId, TypeName = "INT")]
    public int ParentMessageId { get; set; }

    [DefaultValue(0)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnChildSourceId, TypeName = "LONG(20)")]
    [Required]
    public long ChildSourceId { get; set; }

    [DefaultValue(0)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnChildMessageId, TypeName = "INT")]
    public int ChildMessageId { get; set; }

    public TgEfSourceEntity? ParentSource { get; set; }
    public TgEfMessageEntity? ParentMessage { get; set; }
    public TgEfSourceEntity? ChildSource { get; set; }
    public TgEfMessageEntity? ChildMessage { get; set; }

    public TgEfMessageRelationEntity() => Default();

    #endregion

    #region Methods

    public string ToDebugString() => TgObjectUtils.ToDebugString(this);

    public override string ToString() => ToDebugString();

    public void Default()
    {
		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
        ParentSourceId = this.GetDefaultPropertyLong(nameof(ParentSourceId));
        ParentMessageId = this.GetDefaultPropertyInt(nameof(ParentMessageId));
        ChildSourceId = this.GetDefaultPropertyLong(nameof(ChildSourceId));
        ChildMessageId = this.GetDefaultPropertyInt(nameof(ChildMessageId));
    }

    public TgEfMessageRelationEntity Copy(TgEfMessageRelationEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
        ParentSourceId = item.ParentSourceId;
        ParentMessageId = item.ParentMessageId;
        ChildSourceId = item.ChildSourceId;
        ChildMessageId = item.ChildMessageId;
        
        ParentSource = item.ParentSource?.Copy(item.ParentSource, isUidCopy);
        ParentMessage = item.ParentMessage?.Copy(item.ParentMessage, isUidCopy);
        ChildSource = item.ChildSource?.Copy(item.ChildSource, isUidCopy);
        ChildMessage = item.ChildMessage?.Copy(item.ChildMessage, isUidCopy);
        
        return this;
	}

	#endregion
}
