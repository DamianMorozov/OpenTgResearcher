namespace TgStorage.Domain.Filters;

/// <summary> Filter entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(IsEnabled))]
[Index(nameof(FilterType))]
[Index(nameof(Name))]
[Index(nameof(Mask))]
[Index(nameof(Size))]
[Index(nameof(SizeType))]
public sealed class TgEfFilterEntity : ITgEfEntity<TgEfFilterEntity>
{
	#region Fields, properties, constructor

	[DefaultValue("00000000-0000-0000-0000-000000000000")]
	[Key]
	[Required]
	[Column(TgEfConstants.ColumnUid, TypeName = "CHAR(36)")]
	[SQLite.Collation("NOCASE")]
	public Guid Uid { get; set; }

	[DefaultValue(true)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnIsEnabled, TypeName = "BIT")]
	public bool IsEnabled { get; set; }

	[DefaultValue(TgEnumFilterType.SingleName)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnFilterType, TypeName = "INT")]
	public TgEnumFilterType FilterType { get; set; }

	[DefaultValue("Any")]
	[ConcurrencyCheck]
	[MaxLength(128)]
	[Column(TgEfConstants.ColumnName, TypeName = "NVARCHAR(128)")]
	public string Name { get; set; } = null!;

	[DefaultValue("*")]
	[ConcurrencyCheck]
	[MaxLength(128)]
	[Column(TgEfConstants.ColumnMask, TypeName = "NVARCHAR(128)")]
	public string Mask { get; set; } = null!;

	[DefaultValue(0)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnSize, TypeName = "LONG(20)")]
	public long Size { get; set; }

	[DefaultValue(TgEnumFileSizeType.Bytes)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnSizeType, TypeName = "INT")]
	public TgEnumFileSizeType SizeType { get; set; }

	public TgEfFilterEntity()
	{
		Default();
	}

    #endregion

    #region Methods

    public string ToDebugString() => TgObjectUtils.ToDebugString(this);

	public void Default()
	{
		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
		IsEnabled = this.GetDefaultPropertyBool(nameof(IsEnabled));
		FilterType = this.GetDefaultPropertyGeneric<TgEnumFilterType>(nameof(FilterType));
		Name = this.GetDefaultPropertyString(nameof(Name));
		Mask = this.GetDefaultPropertyString(nameof(Mask));
		Size = this.GetDefaultPropertyUint(nameof(Size));
		SizeType = this.GetDefaultPropertyGeneric<TgEnumFileSizeType>(nameof(SizeType));
	}

	public TgEfFilterEntity Copy(TgEfFilterEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		IsEnabled = item.IsEnabled;
		FilterType = item.FilterType;
		Name = item.Name;
		Mask = string.IsNullOrEmpty(item.Mask) &&
			(Equals(item.FilterType, TgEnumFilterType.MinSize) ||
			Equals(item.FilterType, TgEnumFilterType.MaxSize)) ? "*" : item.Mask;
		Size = item.Size;
		SizeType = item.SizeType;
		return this;
	}

	#endregion
}
