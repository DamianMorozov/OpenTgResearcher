namespace TgStorage.Domain.Sources;

/// <summary> EF source DTO </summary>
public sealed partial class TgEfSourceDto : TgDtoBase, ITgEfSourceDto
{
    #region Fields, properties, constructor

    [ObservableProperty]
    public partial long Id { get; set; }
    [ObservableProperty]
    public partial long AccessHash { get; set; } = -1;
    [ObservableProperty]
    public partial string UserName { get; set; } = string.Empty;
    public string Link => TgStringUtils.FormatChatLink(UserName, Id).Item2;
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
    public partial int CountThreads { get; set; }
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
    [ObservableProperty]
    public partial bool IsSubscribe { get; set; }
    [ObservableProperty]
    public partial bool IsDownloadThumbnail { get; set; }
    [ObservableProperty]
    public partial bool IsJoinFileNameWithMessageId { get; set; }
    [ObservableProperty]
    public partial bool IsRewriteFiles { get; set; }
    [ObservableProperty]
    public partial bool IsRewriteMessages { get; set; }
    [ObservableProperty]
    public partial bool IsSaveMessages { get; set; }
    [ObservableProperty]
    public partial bool IsSaveFiles { get; set; }
    [ObservableProperty]
    public partial bool IsParsingComments { get; set; }

    public string DtChangedString => $"{DtChanged:yyyy-MM-dd HH:mm:ss}";

    public float Progress => Count == 0 ? 0 : (float)FirstId * 100 / Count;
    public string ProgressPercentString => Progress == 0 ? "{0:00.00} %" : $"{Progress:#00.00} %";
    public bool IsComplete => FirstId >= Count;
    public float CurrentFileProgress => CurrentFileSize > 0 ? (float)CurrentFileTransmitted * 100 / CurrentFileSize : 0;
    public string CurrentFileProgressPercentString =>
        (CurrentFileProgress == 0 ? "{0:00.00}" : $"{CurrentFileProgress:#00.00}") + " %";
    public string CurrentFileProgressMBString =>
        (CurrentFileTransmitted == 0 ? "{0:0}" : $"{(float)CurrentFileTransmitted / 1024 / 1024:### ##0}") + " from " +
        (CurrentFileSize == 0 ? "{0:0}" : $"{(float)CurrentFileSize / 1024 / 1024:### ##0}") + " MB";
    public string CurrentFileSpeedKBString =>
        (CurrentFileSpeed == 0 ? "{0:0}" : $"{(float)CurrentFileSpeed / 1024:### 000}") + " KB/sec";
    public string CurrentFileSpeedMBString =>
        (CurrentFileSpeed == 0 ? "{0:0}" : $"{(float)CurrentFileSpeed / 1024 / 1024:##0}") + " MB/sec";
    public bool IsReadySourceDirectory => !string.IsNullOrEmpty(Directory) && System.IO.Directory.Exists(Directory);
    public string IsReadySourceDirectoryDescription => IsReadySourceDirectory
        ? $"{TgLocaleHelper.Instance.TgDirectoryIsExists}." : $"{TgLocaleHelper.Instance.TgDirectoryIsNotExists}!";
    public bool IsReady => Id > 0;
    public bool IsReadySourceFirstId => FirstId > 0;

    public string DisplayName => $"{(string.IsNullOrWhiteSpace(UserName) ? "-" : UserName)} | {(string.IsNullOrWhiteSpace(Title) ? "-" : Title)}".Trim();

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
        CountThreads = 10;
        Directory = string.Empty;
        IsAutoUpdate = false;
        IsCreatingSubdirectories = false;
        IsUserAccess = false;
        IsFileNamingByMessage = false;
        IsParsingComments = false;
        IsRestrictSavingContent = false;
        IsSubscribe = false;
        IsDownloadThumbnail = true;
        IsJoinFileNameWithMessageId = true;
        IsRewriteFiles = false;
        IsRewriteMessages = false;
        IsSaveFiles = false;
        IsSaveMessages = false;
        IsDownload = false;
        CurrentFileName = string.Empty;
    }

    #endregion

    #region Methods

    public string GetPercentCountString()
    {
        var percent = Count <= FirstId ? 100 : FirstId > 1 ? (float)FirstId * 100 / Count : 0;
        if (Count <= FirstId)
            return "100.00 %";
        return percent > 9 ? $" {percent:00.00} %" : $"  {percent:0.00} %";
    }

    /// <inheritdoc />
    public override string ToString() => ProgressPercentString;

    /// <inheritdoc />
    public override string ToConsoleString() =>
        $"{Id,11} | " +
        $"{(IsSubscribe ? "subscribe" : ""),-9} | " +
        $"{TgDataFormatUtils.GetFormatString(UserName, 25).TrimEnd(),-25} | " +
        $"{(IsUserAccess ? "access" : ""),-6} | " +
        $"{(IsSourceActive ? "active" : ""),-6} | " +
        $"{(IsAutoUpdate ? "auto" : ""),-6} | " +
        $"{GetPercentCountString()} | " +
        $"{TgDataFormatUtils.GetFormatString(Title, 30).TrimEnd(),-30} | " +
        $"{FirstId} {TgLocaleHelper.Instance.From} {Count} {TgLocaleHelper.Instance.Messages}";

    /// <inheritdoc />
    public override string ToConsoleHeaderString() =>
        $"{nameof(Id),11} | " +
        $"Subscribe | " +
        $"{TgDataFormatUtils.GetFormatString(nameof(UserName), 25).TrimEnd(),-25} | " +
        $"Access | " +
        $"Active | " +
        $"Update | " +
        $"%        | " +
        $"{nameof(Title),-30} | " +
        $"Progress";

    #endregion
}
