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
[Index(nameof(IsFileNamingByMessage))]
[Index(nameof(IsUserAccess))]
public sealed class TgEfSourceEntity : ITgEfEntity<TgEfSourceEntity>
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

    public ICollection<TgEfDocumentEntity> Documents { get; set; } = null!;

	public ICollection<TgEfMessageEntity> Messages { get; set; } = null!;

    public TgEfSourceEntity() : base()
    {
        Default();
    }

    #endregion

    #region Public and private methods

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
		IsFileNamingByMessage = this.GetDefaultPropertyBool(nameof(IsFileNamingByMessage));
	    IsUserAccess = this.GetDefaultPropertyBool(nameof(IsUserAccess));
	    Documents = new List<TgEfDocumentEntity>();
        Messages = new List<TgEfMessageEntity>();
    }

    public TgEfSourceEntity Copy(TgEfSourceEntity item, bool isUidCopy)
    {
		if (isUidCopy)
			Uid = item.Uid;
		DtChanged = item.DtChanged > DateTime.MinValue ? item.DtChanged : DateTime.Now;
		if (Id == this.GetDefaultPropertyLong(nameof(Id)))
			Id = item.Id;
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
		IsFileNamingByMessage = item.IsFileNamingByMessage;
		IsUserAccess = item.IsUserAccess;
        return this;
	}

	public static string ToHeaderString() => 
		$"{nameof(Id),11} | " +
		$"{TgDataFormatUtils.GetFormatString(nameof(UserName), 25).TrimEnd(),-25} | " +
		$"Access | " +
		$"Active | " +
		$"Update | " +
		$"%        | " +
		$"{nameof(Title),-30} | " +
		$"Progress";

	public string ToConsoleString() => 
		$"{Id,11} | " +
		$"{TgDataFormatUtils.GetFormatString(UserName, 25).TrimEnd(),-25} | " +
		$"{(IsUserAccess ? "access" : ""),-6} | " +
		$"{(IsActive ? "active" : ""),-6} | " +
		$"{(IsAutoUpdate ? "auto" : ""),-6} | " +
		$"{GetPercentCountString()} | " +
		$"{TgDataFormatUtils.GetFormatString(Title, 30).TrimEnd(),-30} | " +
		$"{FirstId} {TgLocaleHelper.Instance.From} {Count} {TgLocaleHelper.Instance.Messages}";

	public string GetPercentCountString()
    {
	    var percent = Count <= FirstId ? 100 : FirstId > 1 ? (float)FirstId * 100 / Count : 0;
	    if (IsPercentCountAll())
		    return "100.00 %";
	    return percent > 9 ? $" {percent:00.00} %" : $"  {percent:0.00} %";
    }

    public bool IsPercentCountAll() => Count <= FirstId;

	#endregion
}