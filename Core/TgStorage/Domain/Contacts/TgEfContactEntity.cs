// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Contacts;

/// <summary> Contact entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(DtChanged))]
[Index(nameof(Id), IsUnique = true)]
[Index(nameof(AccessHash))]
[Index(nameof(IsActive))]
[Index(nameof(IsBot))]
[Index(nameof(FirstName))]
[Index(nameof(LastName))]
[Index(nameof(UserName))]
[Index(nameof(PhoneNumber))]
[Index(nameof(Status))]
[Index(nameof(LangCode))]
public sealed class TgEfContactEntity : ITgDbEntity, ITgDbFillEntity<TgEfContactEntity>
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

	[DefaultValue(false)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnIsBot, TypeName = "BIT")]
	public bool IsBot { get; set; }

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(128)]
	[Column(TgEfConstants.ColumnFirstName, TypeName = "NVARCHAR(128)")]
	public string? FirstName { get; set; }

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(128)]
	[Column(TgEfConstants.ColumnLastName, TypeName = "NVARCHAR(128)")]
	public string? LastName { get; set; }

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(128)]
	[Column(TgEfConstants.ColumnUserName, TypeName = "NVARCHAR(128)")]
	public string? UserName { get; set; }

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(128)]
	[Column(TgEfConstants.ColumnUserNames, TypeName = "NVARCHAR(128)")]
	public string? UserNames { get; set; }

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(20)]
	[Column(TgEfConstants.ColumnPhoneNumber, TypeName = "NVARCHAR(20)")]
	public string? PhoneNumber { get; set; } = null!;

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(20)]
	[Column(TgEfConstants.ColumnStatus, TypeName = "NVARCHAR(20)")]
	public string? Status { get; set; } = null!;

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(128)]
	[Column(TgEfConstants.ColumnRestrictionReason, TypeName = "NVARCHAR(128)")]
	public string? RestrictionReason { get; set; } = null!;

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(16)]
	[Column(TgEfConstants.ColumnLangCode, TypeName = "NVARCHAR(16)")]
	public string? LangCode { get; set; } = null!;

	[DefaultValue(-1)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnStoriesMaxId, TypeName = "INT(20)")]
	public int StoriesMaxId { get; set; }

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(20)]
	[Column(TgEfConstants.ColumnBotInfoVersion, TypeName = "NVARCHAR(20)")]
	public string? BotInfoVersion { get; set; } = null!;

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(128)]
	[Column(TgEfConstants.ColumnBotInlinePlaceholder, TypeName = "NVARCHAR(128)")]
	public string BotInlinePlaceholder { get; set; } = null!;

	[DefaultValue(-1)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnBotActiveUsers, TypeName = "INT(20)")]
	public int BotActiveUsers { get; set; }

	public TgEfContactEntity() : base()
	{
		Default();
	}

	#endregion

	#region Public and private methods

	public string ToDebugString() => TgObjectUtils.ToDebugString(this);

	public void Default()
	{
		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
		DtChanged = this.GetDefaultPropertyDateTime(nameof(DtChanged));
		Id = this.GetDefaultPropertyLong(nameof(Id));
		AccessHash = this.GetDefaultPropertyLong(nameof(AccessHash));
		IsActive = this.GetDefaultPropertyBool(nameof(IsActive));
		IsBot = this.GetDefaultPropertyBool(nameof(IsBot));
		FirstName = this.GetDefaultPropertyString(nameof(FirstName));
		LastName = this.GetDefaultPropertyString(nameof(LastName));
		UserName = this.GetDefaultPropertyString(nameof(UserName));
		UserNames = this.GetDefaultPropertyString(nameof(UserNames));
		PhoneNumber = this.GetDefaultPropertyString(nameof(PhoneNumber));
		Status = this.GetDefaultPropertyString(nameof(Status));
		RestrictionReason = this.GetDefaultPropertyString(nameof(RestrictionReason));
		LangCode = this.GetDefaultPropertyString(nameof(LangCode));
		StoriesMaxId = this.GetDefaultPropertyInt(nameof(StoriesMaxId));
		BotInfoVersion = this.GetDefaultPropertyString(nameof(BotInfoVersion));
		BotInlinePlaceholder = this.GetDefaultPropertyString(nameof(BotInlinePlaceholder));
		BotActiveUsers = this.GetDefaultPropertyInt(nameof(BotActiveUsers));
	}

	public TgEfContactEntity Fill(TgEfContactEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		DtChanged = item.DtChanged > DateTime.MinValue ? item.DtChanged : DateTime.Now;
		if (Id == this.GetDefaultPropertyLong(nameof(Id)))
			Id = item.Id;
		AccessHash = item.AccessHash;
		IsActive = item.IsActive;
		IsBot = item.IsBot;
		FirstName = item.FirstName;
		LastName = item.LastName;
		UserName = item.UserName;
		UserNames = item.UserNames;
		PhoneNumber = item.PhoneNumber;
		Status = item.Status;
		RestrictionReason = item.RestrictionReason;
		LangCode = item.LangCode;
		StoriesMaxId = item.StoriesMaxId;
		BotInfoVersion = item.BotInfoVersion;
		BotInlinePlaceholder = item.BotInlinePlaceholder;
		BotActiveUsers = item.BotActiveUsers;
		return this;
	}

	public string ToConsoleString()
	{
		var name = TgDataFormatUtils.GetFormatString(FirstName, 30).TrimEnd();
		name += TgDataFormatUtils.GetFormatString(LastName, 30).TrimEnd();
		return $"{Id,11} | " +
			$"{TgDataFormatUtils.GetFormatString(UserName, 25).TrimEnd(),-25} | " +
			$"{(IsActive ? "active" : ""),-6} | " +
			$"{TgDataFormatUtils.GetFormatString(PhoneNumber, 11).TrimEnd(),-11} | " +
			$"{name,-40}";
	}

	#endregion
}