namespace TgStorage.Domain.Versions;

/// <summary> EF version entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(Version), IsUnique = true)]
[Index(nameof(Description))]
public sealed class TgEfVersionEntity : ITgEfVersionEntity
{
	#region Fields, properties, constructor

	[DefaultValue("00000000-0000-0000-0000-000000000000")]
	[Key]
	[Required]
	[Column(TgEfConstants.ColumnUid, TypeName = "CHAR(36)")]
	[SQLite.Collation("NOCASE")]
	public Guid Uid { get; set; }

	[DefaultValue(1024)]
    [MaxLength(4)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnVersion, TypeName = "SMALLINT")]
    public short Version { get; set; }

    [DefaultValue("New version")]
    [ConcurrencyCheck]
    [MaxLength(128)]
    [Column(TgEfConstants.ColumnDescription, TypeName = "NVARCHAR(128)")]
    public string Description { get; set; } = null!;

    public TgEfVersionEntity() => Default();

    #endregion

    #region Methods

    /// <inheritdoc />
    public string ToDebugString() => TgObjectUtils.ToDebugString(this);

    /// <inheritdoc />
	public void Default()
    {
		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
		Version = this.GetDefaultPropertyShort(nameof(Version));
	    Description = this.GetDefaultPropertyString(nameof(Description));
    }

	#endregion
}
