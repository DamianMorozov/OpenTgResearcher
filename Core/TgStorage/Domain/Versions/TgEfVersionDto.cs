namespace TgStorage.Domain.Versions;

/// <summary> EF proxy DTO </summary>
public sealed partial class TgEfVersionDto : TgDtoBase, ITgEfVersionDto
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial short Version { get; set; }
	[ObservableProperty]
	public partial string Description { get; set; }

	public TgEfVersionDto() : base()
	{
		Version = 0;
		Description = string.Empty;
	}

    #endregion

    #region Methods

    /// <inheritdoc />
    public override string ToConsoleString() => 
        $"{Version,3} | " +
        $"{TgDataFormatUtils.GetFormatString(Description, 50).TrimEnd(),-50}";

    /// <inheritdoc />
    public override string ToConsoleHeaderString() =>
        $"{TgDataFormatUtils.GetFormatString(nameof(Version), 3).TrimEnd(),-3} | " +
        $"Description";

	#endregion
}
