// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Proxies;

/// <summary> Proxy entity </summary>
[DebuggerDisplay("{ToDebugString()}")]
[Index(nameof(Uid), IsUnique = true)]
[Index(nameof(Type))]
[Index(nameof(HostName))]
[Index(nameof(Port))]
[Index(nameof(UserName))]
[Index(nameof(Password))]
[Index(nameof(Secret))]
public sealed class TgEfProxyEntity : ITgDbProxy<TgEfProxyEntity>, ITgEfEntity<TgEfProxyEntity>
{
	#region Public and private fields, properties, constructor

	[DefaultValue("00000000-0000-0000-0000-000000000000")]
	[Key]
	[Required]
	[Column(TgEfConstants.ColumnUid, TypeName = "CHAR(36)")]
	[SQLite.Collation("NOCASE")]
	public Guid Uid { get; set; }

	[DefaultValue(TgEnumProxyType.None)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnType, TypeName = "INT")]
    public TgEnumProxyType Type { get; set; }

    [DefaultValue("No proxy")]
    [ConcurrencyCheck]
    [MaxLength(128)]
    [Column(TgEfConstants.ColumnHostName, TypeName = "NVARCHAR(128)")]
    public string HostName { get; set; } = null!;

    [DefaultValue(404)]
    [ConcurrencyCheck]
    [Column(TgEfConstants.ColumnPort, TypeName = "INT(5)")]
    public ushort Port { get; set; }

    [DefaultValue("No user")]
    [ConcurrencyCheck]
    [MaxLength(128)]
    [Column(TgEfConstants.ColumnUserName, TypeName = "NVARCHAR(128)")]
    public string UserName { get; set; } = null!;

    [DefaultValue("No password")]
    [ConcurrencyCheck]
    [MaxLength(128)]
    [Column(TgEfConstants.ColumnPassword, TypeName = "NVARCHAR(128)")]
    public string Password { get; set; } = null!;

    [DefaultValue("")]
    [ConcurrencyCheck]
    [MaxLength(128)]
    [Column(TgEfConstants.ColumnSecret, TypeName = "NVARCHAR(128)")]
    public string Secret { get; set; } = null!;

    public ICollection<TgEfAppEntity> Apps { get; set; } = null!;

	public TgEfProxyEntity()
    {
        Default();
    }

    #endregion

    #region Public and private methods

    public string ToDebugString() =>
        $"{TgEfConstants.TableProxies} | {Uid} | {Type} | {HostName} | {Port} | {UserName} | {Password} | " +
        $"{TgDataUtils.GetIsFlag(!string.IsNullOrEmpty(Secret), Secret, "<No secret>")}";

    public void Default()
    {
		Uid = this.GetDefaultPropertyGuid(nameof(Uid));
		Type = this.GetDefaultPropertyGeneric<TgEnumProxyType>(nameof(Type));
	    HostName = this.GetDefaultPropertyString(nameof(HostName));
	    Port = this.GetDefaultPropertyUshort(nameof(Port));
	    UserName = this.GetDefaultPropertyString(nameof(UserName));
	    Password = this.GetDefaultPropertyString(nameof(Password));
	    Secret = this.GetDefaultPropertyString(nameof(Secret));
	    Apps = [];
    }

    public TgEfProxyEntity Copy(TgEfProxyEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		Type = item.Type;
	    HostName = item.HostName;
	    Port = item.Port;
	    UserName = item.UserName;
	    Password = item.Password;
	    Secret = item.Secret;
        return this;
    }

	#endregion
}