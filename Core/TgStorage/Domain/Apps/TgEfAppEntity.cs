﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Apps;

/// <summary> App entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(ApiHash), IsUnique = true)]
[Index(nameof(ApiId))]
[Index(nameof(PhoneNumber))]
[Index(nameof(ProxyUid))]
public sealed class TgEfAppEntity : ITgDbEntity, ITgDbFillEntity<TgEfAppEntity>
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

	[DefaultValue("00000000-0000-0000-0000-000000000000")]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnApiHash, TypeName = "CHAR(36)")]
    [SQLite.Collation("NOCASE")]
    public Guid ApiHash { get; set; }
    
	[DefaultValue(0)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnApiId, TypeName = "INT")]
    public int ApiId { get; set; }

	[DefaultValue("+00000000000")]
	[ConcurrencyCheck]
	[MaxLength(20)]
	[Column(TgEfConstants.ColumnPhoneNumber, TypeName = "NVARCHAR(20)")]
	public string PhoneNumber { get; set; } = default!;

    [DefaultValue("00000000-0000-0000-0000-000000000000")]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnProxyUid, TypeName = "CHAR(36)")]
    [SQLite.Collation("NOCASE")]
	public Guid? ProxyUid { get; set; }

	public TgEfProxyEntity? Proxy { get; set; }

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(64)]
	[Column(TgEfConstants.ColumnFirstName, TypeName = "NVARCHAR(64)")]
	public string FirstName { get; set; } = default!;

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(64)]
	[Column(TgEfConstants.ColumnLastName, TypeName = "NVARCHAR(64)")]
	public string LastName { get; set; } = default!;

	public TgEfAppEntity() : base()
    {
	    Default();
    }

	#endregion

	#region Public and private methods

	public string ToDebugString() => TgObjectUtils.ToDebugString(this);

	public void Default()
    {
		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
		ApiHash = this.GetDefaultPropertyGuid(nameof(ApiHash));
        ApiId = this.GetDefaultPropertyInt(nameof(ApiId));
        PhoneNumber = this.GetDefaultPropertyString(nameof(PhoneNumber));
		//ProxyUid = this.GetDefaultPropertyGuid(nameof(ProxyUid));
		ProxyUid = null;
		FirstName = this.GetDefaultPropertyString(nameof(FirstName));
		LastName = this.GetDefaultPropertyString(nameof(LastName));
    }

	public TgEfAppEntity Fill(TgEfAppEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		if (ApiHash == this.GetDefaultPropertyGuid(nameof(ApiHash)))
			ApiHash = item.ApiHash;
	    ApiId = item.ApiId;
		FirstName = item.FirstName;
		LastName = item.LastName;
		PhoneNumber = item.PhoneNumber;
	    ProxyUid = item.ProxyUid;
		return this;
    }

	public TgEfAppEntity Backup(object item)
	{
		return this;
	}

	#endregion
}