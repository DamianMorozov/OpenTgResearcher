// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Sources;

/// <summary> Source entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(DtChanged))]
[Index(nameof(Id), IsUnique = true)]
[Index(nameof(AccessHash))]
[Index(nameof(IsActive))]
[Index(nameof(UserName))]
[Index(nameof(Title))]
[Index(nameof(Count))]
[Index(nameof(Directory))]
[Index(nameof(FirstId))]
[Index(nameof(IsAutoUpdate))]
[Index(nameof(IsCreatingSubdirectories))]
[Index(nameof(IsUserAccess))]
[Index(nameof(IsFileNamingByMessage))]
[Index(nameof(IsRestrictSavingContent))]
[Index(nameof(IsSubscribe))]
public sealed class TgEfSourceEntity : ITgEfIdEntity<TgEfSourceEntity>
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

    public ICollection<TgEfDocumentEntity> Documents { get; set; } = null!;

	public ICollection<TgEfMessageEntity> Messages { get; set; } = null!;

    public TgEfSourceEntity()
    {
        Default();
    }

    #endregion

    #region Methods

    public string ToDebugString() =>
        $"{TgEfConstants.TableSources} | {Uid} | {Id} | {(IsAutoUpdate ? "a" : " ")} | {(FirstId == Count ? "v" : "x")} | {UserName} | " +
        $"{(string.IsNullOrEmpty(Title) 
	        ? string.Empty 
	        : TgDataFormatUtils.TrimStringEnd(Title))} | {FirstId} {TgLocaleHelper.Instance.From} {Count} {TgLocaleHelper.Instance.Messages}";

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
	    Directory = this.GetDefaultPropertyString(nameof(Directory));
	    FirstId = this.GetDefaultPropertyInt(nameof(FirstId));
	    IsAutoUpdate = this.GetDefaultPropertyBool(nameof(IsAutoUpdate));
	    IsCreatingSubdirectories = this.GetDefaultPropertyBool(nameof(IsCreatingSubdirectories));
	    IsUserAccess = this.GetDefaultPropertyBool(nameof(IsUserAccess));
		IsFileNamingByMessage = this.GetDefaultPropertyBool(nameof(IsFileNamingByMessage));
        IsRestrictSavingContent = this.GetDefaultPropertyBool(nameof(IsRestrictSavingContent));
        IsSubscribe = this.GetDefaultPropertyBool(nameof(IsSubscribe));
	    Documents = [];
        Messages = [];
    }

    public TgEfSourceEntity Copy(TgEfSourceEntity item, bool isUidCopy)
    {
		if (isUidCopy)
        {
			Uid = item.Uid;
            // Unique key
            Id = item.Id;
        }
		DtChanged = item.DtChanged > DateTime.MinValue ? item.DtChanged : DateTime.Now;
		AccessHash = item.AccessHash;
		IsActive = item.IsActive;
		FirstId = item.FirstId;
		UserName = item.UserName;
		Title = item.Title;
		About = item.About;
		Count = item.Count;
		Directory = item.Directory;
		IsAutoUpdate = item.IsAutoUpdate;
		IsCreatingSubdirectories = item.IsCreatingSubdirectories;
		IsUserAccess = item.IsUserAccess;
		IsFileNamingByMessage = item.IsFileNamingByMessage;
        IsRestrictSavingContent = item.IsRestrictSavingContent;
        IsSubscribe = item.IsSubscribe;
        return this;
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