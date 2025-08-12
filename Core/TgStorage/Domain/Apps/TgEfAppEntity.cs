// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Apps;

/// <summary> App entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(ApiHash), IsUnique = true)]
[Index(nameof(ApiId))]
[Index(nameof(PhoneNumber))]
[Index(nameof(ProxyUid))]
public sealed class TgEfAppEntity : ITgEfEntity<TgEfAppEntity>
{
	#region Public and private fields, properties, constructor

	[DefaultValue("00000000-0000-0000-0000-000000000000")]
	[Key]
	[Required]
	[Column(TgEfConstants.ColumnUid, TypeName = "CHAR(36)")]
	[SQLite.Collation("NOCASE")]
	public Guid Uid { get; set; }

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
	public string PhoneNumber { get; set; } = null!;

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
	public string FirstName { get; set; } = null!;

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(64)]
	[Column(TgEfConstants.ColumnLastName, TypeName = "NVARCHAR(64)")]
	public string LastName { get; set; } = null!;

	[DefaultValue(false)]
	[ConcurrencyCheck]
	[Column(TgEfConstants.ColumnUseBot, TypeName = "BIT")]
	public bool UseBot { get; set; }

	[DefaultValue("")]
	[ConcurrencyCheck]
	[MaxLength(50)]
	[Column(TgEfConstants.ColumnBotTokenKey, TypeName = "NVARCHAR(50)")]
	public string BotTokenKey { get; set; } = null!;


    [DefaultValue(false)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnUseClient, TypeName = "BIT")]
    public bool UseClient { get; set; }
    
    public TgEfAppEntity()
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
		ProxyUid = null;
		FirstName = this.GetDefaultPropertyString(nameof(FirstName));
		LastName = this.GetDefaultPropertyString(nameof(LastName));
		UseBot = this.GetDefaultPropertyBool(nameof(UseBot));
		BotTokenKey = this.GetDefaultPropertyString(nameof(BotTokenKey));
        UseClient = this.GetDefaultPropertyBool(nameof(UseClient));
    }

	public TgEfAppEntity Copy(TgEfAppEntity item, bool isUidCopy)
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
		UseBot = item.UseBot;
		BotTokenKey = item.BotTokenKey;
        UseClient = item.UseClient;
        return this;
    }

    public void SetUseBot(bool useBot)
    {
        UseBot = useBot;
        if (useBot)
            UseClient = false;
    }

    public void SetUseClient(bool useClient)
    {
        UseClient = useClient;
        if (useClient)
            UseBot = false;
    }

    #endregion
}