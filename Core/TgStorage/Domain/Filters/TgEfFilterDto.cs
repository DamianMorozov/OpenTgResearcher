// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Filters;

/// <summary> Filter DTO </summary>
public sealed partial class TgEfFilterDto : TgDtoBase, ITgDto<TgEfFilterEntity, TgEfFilterDto>
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial bool IsEnabled { get; set; }
	[ObservableProperty]
	public partial TgEnumFilterType FilterType { get; set; }
	[ObservableProperty]
	public partial string Name { get; set; }
	[ObservableProperty]
	public partial string Mask { get; set; }
	[ObservableProperty]
	public partial long Size { get; set; }
	[ObservableProperty]
	public partial TgEnumFileSizeType SizeType { get; set; }

	public long SizeAtBytes => SizeType switch
	{
		TgEnumFileSizeType.KBytes => Size * 1024,
		TgEnumFileSizeType.MBytes => Size * 1024 * 1024,
		TgEnumFileSizeType.GBytes => Size * 1024 * 1024 * 1024,
		TgEnumFileSizeType.TBytes => Size * 1024 * 1024 * 1024 * 1024,
		_ => Size,
	};

	public TgEfFilterDto() : base()
	{
		IsEnabled = false;
		FilterType = TgEnumFilterType.None;
		Name = string.Empty;
		Mask = string.Empty;
		Size = 0;
		SizeType = TgEnumFileSizeType.Bytes;
	}

    #endregion

    #region Public and private methods

    public string GetStringForFilterType() => FilterType switch
    {
        TgEnumFilterType.SingleName => TgLocaleHelper.Instance.MenuFiltersSetSingleName,
        TgEnumFilterType.SingleExtension => TgLocaleHelper.Instance.MenuFiltersSetSingleExtension,
        TgEnumFilterType.MultiName => TgLocaleHelper.Instance.MenuFiltersSetMultiName,
        TgEnumFilterType.MultiExtension => TgLocaleHelper.Instance.MenuFiltersSetMultiExtension,
        TgEnumFilterType.MinSize => TgLocaleHelper.Instance.MenuFiltersSetMinSize,
        TgEnumFilterType.MaxSize => TgLocaleHelper.Instance.MenuFiltersSetMaxSize,
        _ => $"<{TgLocaleHelper.Instance.MenuFiltersError}>",
    };

    public override string ToConsoleString() =>
        $"{TgDataFormatUtils.GetFormatString(Name, 20).TrimEnd(),-20} | " +
        $"{TgDataFormatUtils.GetFormatString(Mask, 20).TrimEnd(),-20} | " +
        $"{(IsEnabled ? "enabled" : "disabled"),-8} | " +
        $"{GetStringForFilterType(),-20} | " +
        $"{Size,12} | " +
        $"{SizeType} ";

    public override string ToConsoleHeaderString() =>
        $"{TgDataFormatUtils.GetFormatString(nameof(Name), 20).TrimEnd(),-20} | " +
        $"{TgDataFormatUtils.GetFormatString(nameof(Mask), 20).TrimEnd(),-20} | " +
        $"{TgDataFormatUtils.GetFormatString(nameof(IsEnabled), 8).TrimEnd(),-8} | " +
        $"{TgDataFormatUtils.GetFormatString(nameof(FilterType), 20).TrimEnd(),-20} | " +
        $"{TgDataFormatUtils.GetFormatString(nameof(Size), 12).TrimEnd(),-12} | " +
        $"SizeType";

    public TgEfFilterDto Copy(TgEfFilterDto dto, bool isUidCopy)
	{
		base.Copy(dto, isUidCopy);
		IsEnabled = dto.IsEnabled;
		FilterType = dto.FilterType;
		Name = dto.Name;
		Mask = dto.Mask;
		Size = dto.Size;
		SizeType = dto.SizeType;
		return this;
	}

	public TgEfFilterDto Copy(TgEfFilterEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		IsEnabled = item.IsEnabled;
		FilterType = item.FilterType;
		Name = item.Name;
		Mask = item.Mask;
		Size = item.Size;
		SizeType = item.SizeType;
		return this;
	}

	public TgEfFilterDto GetNewDto(TgEfFilterEntity item) => new TgEfFilterDto().Copy(item, isUidCopy: true);

	public TgEfFilterEntity GetNewEntity(TgEfFilterDto dto) => new()
	{
		Uid = dto.Uid,
		IsEnabled = dto.IsEnabled,
		FilterType = dto.FilterType,
		Name = dto.Name,
		Mask = dto.Mask,
		Size = dto.Size,
		SizeType = dto.SizeType,
	};

	public TgEfFilterEntity GetNewEntity() => new()
	{
		Uid = Uid,
		IsEnabled = IsEnabled,
		FilterType = FilterType,
		Name = Name,
		Mask = Mask,
		Size = Size,
		SizeType = SizeType,
	};

	#endregion
}
