// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Versions;

/// <summary> Version entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(Version), IsUnique = true)]
[Index(nameof(Description))]
public sealed class TgEfVersionEntity : ITgEfEntity<TgEfVersionEntity>
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

    public TgEfVersionEntity() : base()
    {
        Default();
    }

	#endregion

	#region Public and private methods

	public string ToConsoleString() => $"{Version,3} | " +
		$"{TgDataFormatUtils.GetFormatString(Description, 50).TrimEnd(),-50}";

	public string ToDebugString() => TgObjectUtils.ToDebugString(this);

	public void Default()
    {
		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
		Version = this.GetDefaultPropertyShort(nameof(Version));
	    Description = this.GetDefaultPropertyString(nameof(Description));
    }

    public TgEfVersionEntity Copy(TgEfVersionEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		if (Version == this.GetDefaultPropertyShort(nameof(Version)))
			Version = item.Version;
		Description = item.Description;
        return this;
	}

	#endregion
}