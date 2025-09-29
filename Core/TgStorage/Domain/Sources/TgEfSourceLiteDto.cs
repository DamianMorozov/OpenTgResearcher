namespace TgStorage.Domain.Sources;

/// <summary> EF source DTO </summary>
public sealed partial class TgEfSourceLiteDto : TgDtoBase, ITgEfSourceLiteDto
{
    #region Fields, properties, constructor

    [ObservableProperty]
	public partial long Id { get; set; }
	[ObservableProperty]
	public partial string DtChangedString { get; set; }
	[ObservableProperty]
	public partial string UserName { get; set; }
	[ObservableProperty]
	public partial bool IsSourceActive { get; set; }
	[ObservableProperty]
	public partial string Title { get; set; }
	[ObservableProperty]
	public partial int FirstId { get; set; }
	[ObservableProperty]
	public partial int Count { get; set; }
	[ObservableProperty]
	public partial bool IsAutoUpdate { get; set; }
	[ObservableProperty]
	public partial bool IsCreatingSubdirectories { get; set; }
	[ObservableProperty]
	public partial bool IsUserAccess { get; set; }
	[ObservableProperty]
	public partial bool IsFileNamingByMessage { get; set; }
	[ObservableProperty]
	public partial bool IsRestrictSavingContent { get; set; }
	[ObservableProperty]
	public partial bool IsSubscribe { get; set; }
	[ObservableProperty]
	public partial bool IsDownload { get; set; }
	[ObservableProperty]
	public partial int ProgressPercent { get; set; }
	[ObservableProperty]
	public partial string ProgressPercentString { get; set; }
	[ObservableProperty]
	public partial int ParticipantsCount { get; set; }

	public TgEfSourceLiteDto() : base()
	{
		Id = 0;
		DtChangedString = string.Empty;
		UserName = string.Empty;
		IsSourceActive = false;
		Title = string.Empty;
		FirstId = 0;
		Count = 0;
		IsAutoUpdate = false;
		IsCreatingSubdirectories = false;
		IsFileNamingByMessage = false;
		IsUserAccess = false;
		IsDownload = false;
		ProgressPercent = 0;
		ProgressPercentString = string.Empty;
	}

    #endregion

    #region Methods

    /// <inheritdoc />
    public override string ToString() => ProgressPercentString;

	public void SetIsDownload(bool isDownload) => IsDownload = isDownload;

	#endregion
}
