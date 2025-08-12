// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Licenses;

/// <summary> License entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(LicenseKey), IsUnique = true)]
[Index(nameof(UserId))]
[Index(nameof(LicenseType))]
[Index(nameof(ValidTo))]
[Index(nameof(IsConfirmed))]
public sealed class TgEfLicenseEntity : ITgEfEntity<TgEfLicenseEntity>
{
	#region Public and private fields, properties, constructor

	[DefaultValue("00000000-0000-0000-0000-000000000000")]
	[Key]
	[Required]
	[Column(TgEfConstants.ColumnUid, TypeName = "CHAR(36)")]
	[SQLite.Collation("NOCASE")]
	public Guid Uid { get; set; }

	[DefaultValue("00000000-0000-0000-0000-000000000000")]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnLicenseKey, TypeName = "CHAR(36)")]
    [SQLite.Collation("NOCASE")]
    public Guid LicenseKey { get; set; }

	[DefaultValue(0)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnUserId, TypeName = "LONG(20)")]
	public long UserId { get; set; }

	[DefaultValue(0)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnLicenseType, TypeName = "INT(1)")]
	public TgEnumLicenseType LicenseType { get; set; }

	[DefaultValue("0001-01-01 00:00:00")]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnValidTo, TypeName = "DATETIME")]
	public DateTime ValidTo { get; set; }

	[DefaultValue(false)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnIsConfirmed, TypeName = "BIT")]
	public bool IsConfirmed { get; set; }

	public TgEfLicenseEntity()
    {
	    Default();
    }

	#endregion

	#region Public and private methods

	public string ToDebugString() => TgObjectUtils.ToDebugString(this);

	public void Default()
    {
		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
		LicenseKey = this.GetDefaultPropertyGuid(nameof(LicenseKey));
		UserId = this.GetDefaultPropertyLong(nameof(UserId));
		LicenseType = TgEnumLicenseType.Free;
		ValidTo = this.GetDefaultPropertyDateTime(nameof(ValidTo));
		IsConfirmed = this.GetDefaultPropertyBool(nameof(IsConfirmed));
    }

	public TgEfLicenseEntity Copy(TgEfLicenseEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		LicenseKey = item.LicenseKey;
		UserId = item.UserId;
		LicenseType = item.LicenseType;
		ValidTo = item.ValidTo;
		IsConfirmed = item.IsConfirmed;
		return this;
    }

	#endregion
}