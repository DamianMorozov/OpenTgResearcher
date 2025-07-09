// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Sources;

/// <summary> Source DTO </summary>
public sealed partial class TgEfSourceDto : TgDtoBase, ITgDto<TgEfSourceEntity, TgEfSourceDto>
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial long Id { get; set; }
	[ObservableProperty]
	public partial long AccessHash { get; set; } = -1;
	[ObservableProperty]
	public partial string UserName { get; set; } = string.Empty;
	[ObservableProperty]
	public partial DateTime DtChanged { get; set; } = DateTime.MinValue;
	[ObservableProperty]
	public partial bool IsSourceActive { get; set; }
	[ObservableProperty]
	public partial string Title { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string About { get; set; } = string.Empty;
	[ObservableProperty]
	public partial int FirstId { get; set; }
	[ObservableProperty]
	public partial int Count { get; set; }
	[ObservableProperty]
	public partial string Directory { get; set; } = string.Empty;
	[ObservableProperty]
	public partial bool IsAutoUpdate { get; set; }
	[ObservableProperty]
	public partial bool IsCreatingSubdirectories { get; set; }
	[ObservableProperty]
	public partial bool IsUserAccess { get; set; }
	[ObservableProperty]
	public partial string CurrentFileName { get; set; } = string.Empty;
	[ObservableProperty]
	public partial long CurrentFileSize { get; set; }
	[ObservableProperty]
	public partial long CurrentFileTransmitted { get; set; }
	[ObservableProperty]
	public partial long CurrentFileSpeed { get; set; }
	[ObservableProperty]
	public partial bool IsDownload { get; set; }
	[ObservableProperty]
	public partial bool IsFileNamingByMessage { get; set; }
	[ObservableProperty]
	public partial bool IsRestrictSavingContent { get; set; }

	public string DtChangedString => $"{DtChanged:yyyy-MM-dd HH:mm:ss}";

	public float Progress => Count == 0 ? 0 : (float)FirstId * 100 / Count;
	public string ProgressPercentString => Progress == 0 ? "{0:00.00} %" : $"{Progress:#00.00} %";
	public bool IsComplete => FirstId >= Count;
	public float CurrentFileProgress => CurrentFileSize > 0 ? (float)CurrentFileTransmitted * 100 / CurrentFileSize : 0;
	public string CurrentFileProgressPercentString =>
		(CurrentFileProgress == 0 ? "{0:00.00}" : $"{CurrentFileProgress:#00.00}") + " %";
	public string CurrentFileProgressMBString =>
		(CurrentFileTransmitted == 0 ? "{0:0.00}" : $"{(float)CurrentFileTransmitted / 1024 / 1024:### ##0.00}") + " from " +
		(CurrentFileSize == 0 ? "{0:0.00}" : $"{(float)CurrentFileSize / 1024 / 1024:### ##0.00}") + " MB";
	public string CurrentFileSpeedKBString =>
		(CurrentFileSpeed == 0 ? "{0:0.00}" : $"{(float)CurrentFileSpeed / 1024:### 000.00}") + " KB/sec";
	public string CurrentFileSpeedMBString =>
		(CurrentFileSpeed == 0 ? "{0:0.00}" : $"{(float)CurrentFileSpeed / 1024 / 1024:##0.00}") + " MB/sec";
	public bool IsReadySourceDirectory => !string.IsNullOrEmpty(Directory) && System.IO.Directory.Exists(Directory);
	public string IsReadySourceDirectoryDescription => IsReadySourceDirectory
		? $"{TgLocaleHelper.Instance.TgDirectoryIsExists}." : $"{TgLocaleHelper.Instance.TgDirectoryIsNotExists}!";
	public bool IsReady => Id > 0;
	public bool IsReadySourceFirstId => FirstId > 0;

	public TgEfSourceDto() : base()
	{
		DtChanged = DateTime.MinValue;
		Id = 0;
		AccessHash = 0;
		IsSourceActive = false;
		UserName = string.Empty;
		Title = string.Empty;
		About = string.Empty;
		FirstId = 0;
		Count = 0;
		Directory = string.Empty;
		IsAutoUpdate = false;
		IsCreatingSubdirectories = false;
		IsUserAccess = false;
		IsFileNamingByMessage = false;
        IsRestrictSavingContent = false;
		IsDownload = false;
		CurrentFileName = string.Empty;
	}

    #endregion

    #region Public and private methods

    public string GetPercentCountString()
    {
        var percent = Count <= FirstId ? 100 : FirstId > 1 ? (float)FirstId * 100 / Count : 0;
        if (Count <= FirstId)
            return "100.00 %";
        return percent > 9 ? $" {percent:00.00} %" : $"  {percent:0.00} %";
    }

    public override string ToString() => ProgressPercentString;

    public override string ToConsoleString() =>
        $"{Id,11} | " +
        $"{TgDataFormatUtils.GetFormatString(UserName, 25).TrimEnd(),-25} | " +
        $"{(IsUserAccess ? "access" : ""),-6} | " +
        $"{(IsActive ? "active" : ""),-6} | " +
        $"{(IsAutoUpdate ? "auto" : ""),-6} | " +
        $"{GetPercentCountString()} | " +
        $"{TgDataFormatUtils.GetFormatString(Title, 30).TrimEnd(),-30} | " +
        $"{FirstId} {TgLocaleHelper.Instance.From} {Count} {TgLocaleHelper.Instance.Messages}";

    public override string ToConsoleHeaderString() =>
        $"{nameof(Id),11} | " +
        $"{TgDataFormatUtils.GetFormatString(nameof(UserName), 25).TrimEnd(),-25} | " +
        $"Access | " +
        $"Active | " +
        $"Update | " +
        $"%        | " +
        $"{nameof(Title),-30} | " +
        $"Progress";

    public TgEfSourceDto Copy(TgEfSourceDto dto, bool isUidCopy)
	{
		base.Copy(dto, isUidCopy);
		DtChanged = dto.DtChanged;
		Id = dto.Id;
		AccessHash = dto.AccessHash;
		IsSourceActive = dto.IsActive;
		UserName = dto.UserName;
		Title = dto.Title;
		About = dto.About;
		FirstId = dto.FirstId;
		Count = dto.Count;
		Directory = dto.Directory;
		IsAutoUpdate = dto.IsAutoUpdate;
		IsCreatingSubdirectories = dto.IsCreatingSubdirectories;
		IsUserAccess = dto.IsUserAccess;
		IsDownload = dto.IsDownload;
		CurrentFileName = dto.CurrentFileName;
		IsFileNamingByMessage = dto.IsFileNamingByMessage;
        IsRestrictSavingContent = dto.IsRestrictSavingContent;
		return this;
	}

	public TgEfSourceDto Copy(TgEfSourceEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		DtChanged = item.DtChanged;
		Id = item.Id;
		AccessHash = item.AccessHash;
		IsSourceActive = item.IsActive;
		UserName = item.UserName ?? string.Empty;
		Title = item.Title ?? string.Empty;
		About = item.About ?? string.Empty;
		FirstId = item.FirstId;
		Count = item.Count;
		Directory = item.Directory ?? string.Empty;
		IsAutoUpdate = item.IsAutoUpdate;
		IsCreatingSubdirectories = item.IsCreatingSubdirectories;
		IsUserAccess = item.IsUserAccess;
		IsFileNamingByMessage = item.IsFileNamingByMessage;
        IsRestrictSavingContent = item.IsRestrictSavingContent;
		return this;
	}

	public TgEfSourceDto GetNewDto(TgEfSourceEntity item) => new TgEfSourceDto().Copy(item, isUidCopy: true);

	public TgEfSourceEntity GetNewEntity(TgEfSourceDto dto) => new()
	{
		Uid = dto.Uid,
		DtChanged = dto.DtChanged,
		Id = dto.Id,
		AccessHash = dto.AccessHash,
		IsActive = dto.IsSourceActive,
		UserName = dto.UserName,
		Title = dto.Title,
		About = dto.About,
		FirstId = dto.FirstId,
		Count = dto.Count,
		Directory = dto.Directory,
		IsAutoUpdate = dto.IsAutoUpdate,
		IsCreatingSubdirectories = dto.IsCreatingSubdirectories,
		IsUserAccess = dto.IsUserAccess,
		IsFileNamingByMessage = dto.IsFileNamingByMessage,
        IsRestrictSavingContent = dto.IsRestrictSavingContent,
	};

	public TgEfSourceEntity GetNewEntity() => new()
	{
		Uid = Uid,
		DtChanged = DtChanged,
		Id = Id,
		AccessHash = AccessHash,
		IsActive = IsSourceActive,
		UserName = UserName,
		Title = Title,
		About = About,
		FirstId = FirstId,
		Count = Count,
		Directory = Directory,
		IsAutoUpdate = IsAutoUpdate,
		IsCreatingSubdirectories = IsCreatingSubdirectories,
		IsUserAccess = IsUserAccess,
		IsFileNamingByMessage = IsFileNamingByMessage,
        IsRestrictSavingContent = IsRestrictSavingContent,
	};

	public void SetIsDownload(bool isDownload) => IsDownload = isDownload;

	#endregion
}