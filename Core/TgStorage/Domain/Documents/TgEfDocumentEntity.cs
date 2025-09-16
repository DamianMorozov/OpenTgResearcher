namespace TgStorage.Domain.Documents;

/// <summary> Document entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(SourceId))]
[Index(nameof(Id))]
[Index(nameof(MessageId))]
[Index(nameof(FileName))]
[Index(nameof(FileSize))]
[Index(nameof(AccessHash))]
public sealed class TgEfDocumentEntity : ITgEfEntity<TgEfDocumentEntity>
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
    [Column(TgEfConstants.ColumnId, TypeName = "LONG(20)")]
    public long Id { get; set; }

    [DefaultValue(0)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnMessageId, TypeName = "INT")]
    public int MessageId { get; set; }

    [DefaultValue("")]
    [ConcurrencyCheck]
    [MaxLength(256)]
    [Column(TgEfConstants.ColumnFileName, TypeName = "NVARCHAR(256)")]
    public string FileName { get; set; } = null!;

    [DefaultValue(0)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnFileSize, TypeName = "LONG(20)")]
    public long FileSize { get; set; }

    [DefaultValue(0)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnAccessHash, TypeName = "LONG(20)")]
    public long AccessHash { get; set; }

    public TgEfDocumentEntity()
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
	    Id = this.GetDefaultPropertyLong(nameof(Id));
	    MessageId = this.GetDefaultPropertyInt(nameof(MessageId));
	    FileName = this.GetDefaultPropertyString(nameof(FileName));
	    FileSize = this.GetDefaultPropertyLong(nameof(FileSize));
	    AccessHash = this.GetDefaultPropertyLong(nameof(AccessHash));
	}

    public TgEfDocumentEntity Copy(TgEfDocumentEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		SourceId = item.SourceId;
		Id = item.Id;
		MessageId = item.MessageId;
		FileName = item.FileName;
		FileSize = item.FileSize;
		AccessHash = item.AccessHash;
		return this;
	}

	#endregion
}
