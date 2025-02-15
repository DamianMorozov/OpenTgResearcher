// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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
public sealed class TgEfFilterEntity : ITgDbEntity, ITgDbFillEntity<TgEfFilterEntity>
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

	public TgEfFilterEntity() : base()
	{
		Default();
	}

	#endregion

	#region Public and private methods

	public string ToDebugString() => FilterType switch
	{
		TgEnumFilterType.MinSize or TgEnumFilterType.MaxSize =>
			$"{TgEfConstants.TableFilters} | {Uid} | {TgCommonUtils.GetIsEnabled(IsEnabled)} | {this.GetStringForFilterType()} | {Name} | {Size} | {SizeType}",
		_ => $"{TgEfConstants.TableFilters} | {Uid} | {TgCommonUtils.GetIsEnabled(IsEnabled)} | {this.GetStringForFilterType()} | {Name} | {(string.IsNullOrEmpty(Mask) ? $"<{nameof(string.Empty)}>" : Mask)}",
	};

	public string ToConsoleString() =>
		$"{TgDataFormatUtils.GetFormatString(Name, 20).TrimEnd(),-20} | " +
		$"{TgDataFormatUtils.GetFormatString(Mask, 20).TrimEnd(),-20} | " +
		$"{(IsEnabled ? "enabled" : ""),-7} | " +
		$"{this.GetStringForFilterType(),-20} | " +
		$"{Size,12} | " +
		$"{SizeType} ";

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

	public TgEfFilterEntity Fill(TgEfFilterEntity item, bool isUidCopy)
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