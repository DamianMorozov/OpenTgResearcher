namespace TgStorage.Domain.Sources;

/// <summary> EF source entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(DtChanged))]
[Index(nameof(Id), IsUnique = true)]
[Index(nameof(AccessHash))]
[Index(nameof(IsActive))]
[Index(nameof(UserName))]
[Index(nameof(Title))]
[Index(nameof(Count))]
[Index(nameof(CountThreads))]
[Index(nameof(Directory))]
[Index(nameof(FirstId))]
[Index(nameof(IsAutoUpdate))]
[Index(nameof(IsCreatingSubdirectories))]
[Index(nameof(IsUserAccess))]
[Index(nameof(IsFileNamingByMessage))]
[Index(nameof(IsRestrictSavingContent))]
[Index(nameof(IsSubscribe))]
[Index(nameof(IsDownloadThumbnail))]
[Index(nameof(IsJoinFileNameWithMessageId))]
[Index(nameof(IsRewriteFiles))]
[Index(nameof(IsRewriteMessages))]
[Index(nameof(IsSaveFiles))]
[Index(nameof(IsSaveMessages))]
[Index(nameof(IsParsingComments))]
public sealed class TgEfSourceEntity : ITgEfSourceEntity
{
	#region Fields, properties, constructor

	[DefaultValue("00000000-0000-0000-0000-000000000000")]
	[Key]
	[Required]
	[Column(TgEfConstants.ColumnUid, TypeName = "CHAR(36)")]
	[SQLite.Collation("NOCASE")]
	public Guid Uid { get; set; }

	[DefaultValue("0001-01-01 00:00:00")]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnDtChanged, TypeName = "DATETIME")]
	public DateTime DtChanged { get; set; }

	[DefaultValue(-1)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnId, TypeName = "LONG(20)")]
	public long Id { get; set; }

	[DefaultValue(-1)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnAccessHash, TypeName = "LONG(20)")]
	public long AccessHash { get; set; }

	[DefaultValue(false)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnIsActive, TypeName = "BIT")]
	public bool IsActive { get; set; }

	[DefaultValue("")]
    [ConcurrencyCheck]
    [MaxLength(128)]
    [Column(TgEfConstants.ColumnUserName, TypeName = "NVARCHAR(128)")]
    public string? UserName { get; set; }

    [DefaultValue("")]
    [ConcurrencyCheck]
    [MaxLength(256)]
    [Column(TgEfConstants.ColumnTitle, TypeName = "NVARCHAR(256)")]
    public string? Title { get; set; }

    [DefaultValue("")]
    [ConcurrencyCheck]
	[MaxLength(1024)]
	[Column(TgEfConstants.ColumnAbout, TypeName = "NVARCHAR(1024)")]
    public string? About { get; set; }

	[DefaultValue(1)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnFirstId, TypeName = "INT")]
	public int FirstId { get; set; }

	[DefaultValue(1)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnCount, TypeName = "INT")]
    public int Count { get; set; }

    [DefaultValue(10)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnCountThreads, TypeName = "INT")]
    public int CountThreads { get; set; }

    [DefaultValue("")]
    [ConcurrencyCheck]
    [MaxLength(256)]
    [Column(TgEfConstants.ColumnDirectory, TypeName = "NVARCHAR(256)")]
    public string? Directory { get; set; }

    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsAutoUpdate, TypeName = "BIT")]
    public bool IsAutoUpdate { get; set; }

    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsUserAccess, TypeName = "BIT")]
    public bool IsUserAccess { get; set; }

    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsCreatingSubdirectories, TypeName = "BIT")]
    public bool IsCreatingSubdirectories { get; set; }

    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsFileNamingByMessage, TypeName = "BIT")]
    public bool IsFileNamingByMessage { get; set; }

    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsRestrictSavingContent, TypeName = "BIT")]
    public bool IsRestrictSavingContent { get; set; }

    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsSubscribe, TypeName = "BIT")]
    public bool IsSubscribe { get; set; }

    [DefaultValue(true)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsDownloadThumbnail, TypeName = "BIT")]
    public bool IsDownloadThumbnail { get; set; }

    [DefaultValue(true)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsJoinFileNameWithMessageId, TypeName = "BIT")]
    public bool IsJoinFileNameWithMessageId { get; set; }

    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsRewriteFiles, TypeName = "BIT")]
    public bool IsRewriteFiles { get; set; }

    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsRewriteMessages, TypeName = "BIT")]
    public bool IsRewriteMessages { get; set; }

    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsSaveMessages, TypeName = "BIT")]
    public bool IsSaveMessages { get; set; }

    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsSaveFiles, TypeName = "BIT")]
    public bool IsSaveFiles { get; set; }

    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnIsParsingComments, TypeName = "BIT")]
    public bool IsParsingComments { get; set; }

    public ICollection<TgEfChatUserEntity> ChatUsers { get; set; } = null!;
    public ICollection<TgEfDocumentEntity> Documents { get; set; } = null!;
	public ICollection<TgEfMessageEntity> Messages { get; set; } = null!;

    public TgEfSourceEntity() => Default();

    #endregion

    #region Methods

    /// <inheritdoc />
    public string ToDebugString() =>
        $"{TgEfConstants.TableSources} | {Uid} | {Id} | {(IsAutoUpdate ? "a" : " ")} | {(FirstId == Count ? "v" : "x")} | {UserName} | " +
        $"{(string.IsNullOrEmpty(Title) 
	        ? string.Empty 
	        : TgDataFormatUtils.TrimStringEnd(Title))} | {FirstId} {TgLocaleHelper.Instance.From} {Count} {TgLocaleHelper.Instance.Messages}";

    /// <inheritdoc />
    public void Default()
    {
		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
		DtChanged = this.GetDefaultPropertyDateTime(nameof(DtChanged));
		Id = this.GetDefaultPropertyLong(nameof(Id));
		AccessHash = this.GetDefaultPropertyLong(nameof(AccessHash));
		IsActive = this.GetDefaultPropertyBool(nameof(IsActive));
		UserName = this.GetDefaultPropertyString(nameof(UserName));
	    Title = this.GetDefaultPropertyString(nameof(Title));
	    About = this.GetDefaultPropertyString(nameof(About));
	    Count = this.GetDefaultPropertyInt(nameof(Count));
        CountThreads = this.GetDefaultPropertyInt(nameof(CountThreads));
	    Directory = this.GetDefaultPropertyString(nameof(Directory));
	    FirstId = this.GetDefaultPropertyInt(nameof(FirstId));
	    IsAutoUpdate = this.GetDefaultPropertyBool(nameof(IsAutoUpdate));
	    IsCreatingSubdirectories = this.GetDefaultPropertyBool(nameof(IsCreatingSubdirectories));
	    IsUserAccess = this.GetDefaultPropertyBool(nameof(IsUserAccess));
		IsFileNamingByMessage = this.GetDefaultPropertyBool(nameof(IsFileNamingByMessage));
        IsRestrictSavingContent = this.GetDefaultPropertyBool(nameof(IsRestrictSavingContent));
        IsSubscribe = this.GetDefaultPropertyBool(nameof(IsSubscribe));
        IsDownloadThumbnail = this.GetDefaultPropertyBool(nameof(IsDownloadThumbnail));
        IsJoinFileNameWithMessageId = this.GetDefaultPropertyBool(nameof(IsJoinFileNameWithMessageId));
        IsRewriteFiles = this.GetDefaultPropertyBool(nameof(IsRewriteFiles));
        IsRewriteMessages = this.GetDefaultPropertyBool(nameof(IsRewriteMessages));
        IsSaveFiles = this.GetDefaultPropertyBool(nameof(IsSaveFiles));
        IsSaveMessages = this.GetDefaultPropertyBool(nameof(IsSaveMessages));
        IsParsingComments = this.GetDefaultPropertyBool(nameof(IsParsingComments));

        ChatUsers = [];
        Documents = [];
        Messages = [];
    }

	public string GetPercentCountString()
    {
	    var percent = Count <= FirstId ? 100 : FirstId > 1 ? (float)FirstId * 100 / Count : 0;
	    if (Count <= FirstId)
		    return "100.00 %";
	    return percent > 9 ? $" {percent:00.00} %" : $"  {percent:0.00} %";
    }

	#endregion
}
