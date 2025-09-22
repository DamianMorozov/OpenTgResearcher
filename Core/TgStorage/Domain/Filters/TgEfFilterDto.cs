namespace TgStorage.Domain.Filters;

/// <summary> EF filter DTO </summary>
public sealed partial class TgEfFilterDto : TgDtoBase, ITgEfFilterDto
{
	#region Fields, properties, constructor

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

    #region Methods

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

	#endregion
}
