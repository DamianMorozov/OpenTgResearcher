namespace TgStorage.Domain.Messages;

/// <summary> EF message relation entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(ParentSourceId))]
[Index(nameof(ParentMessageId))]
[Index(nameof(ChildSourceId))]
[Index(nameof(ChildMessageId))]
public sealed class TgEfMessageRelationEntity : ITgEfMessageRelationEntity, IEquatable<TgEfMessageRelationEntity>
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

    /// <inheritdoc />
    public string ToDebugString() => TgObjectUtils.ToDebugString(this);

    /// <inheritdoc />
    public override string ToString() => ToDebugString();

    /// <inheritdoc />
    public void Default()
    {
		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
        ParentSourceId = this.GetDefaultPropertyLong(nameof(ParentSourceId));
        ParentMessageId = this.GetDefaultPropertyInt(nameof(ParentMessageId));
        ChildSourceId = this.GetDefaultPropertyLong(nameof(ChildSourceId));
        ChildMessageId = this.GetDefaultPropertyInt(nameof(ChildMessageId));
    }

    public bool Equals(TgEfMessageRelationEntity? other)
    {
        if (other is null) return false;
        return ParentSourceId == other.ParentSourceId && ParentMessageId == other.ParentMessageId && 
            ChildSourceId == other.ChildSourceId && ChildMessageId == other.ChildMessageId;
    }

    public override bool Equals(object? obj) =>
        Equals(obj as TgEfMessageRelationEntity);

    public override int GetHashCode() =>
        HashCode.Combine(ParentSourceId, ParentMessageId, ChildSourceId, ChildMessageId);

	#endregion
}
