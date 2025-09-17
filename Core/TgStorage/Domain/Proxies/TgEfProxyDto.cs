namespace TgStorage.Domain.Proxies;

/// <summary> Proxy DTO </summary>
public sealed partial class TgEfProxyDto : TgDtoBase, ITgDto<TgEfProxyEntity, TgEfProxyDto>
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial TgEnumProxyType Type { get; set; }
	[ObservableProperty]
	public partial string HostName { get; set; } = string.Empty;
	[ObservableProperty]
	public partial ushort Port { get; set; }
	[ObservableProperty]
	public partial string UserName { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string Password { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string Secret { get; set; } = string.Empty;
	public string PrettyName => $"{Type} | {TgDataFormatUtils.GetFormatString(HostName, 30)} | {Port} | {UserName}";

	public TgEfProxyDto() : base()
	{
		Type = TgEnumProxyType.None;
		HostName = string.Empty;
		Port = 0;
		UserName = string.Empty;
		Password = string.Empty;
		Secret = string.Empty;
	}

	#endregion

	#region Methods

	public override string ToString() => $"{Type} | {HostName} | {Port} | {UserName} | {Password} | {Secret}";

	public TgEfProxyDto Copy(TgEfProxyDto dto, bool isUidCopy)
	{
		base.Copy(dto, isUidCopy);
		Type = dto.Type;
		HostName = dto.HostName;
		Port = dto.Port;
		UserName = dto.UserName;
		Password = dto.Password;
		Secret = dto.Secret;
		return this;
	}

	public TgEfProxyDto Copy(TgEfProxyEntity item, bool isUidCopy)
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

	public TgEfProxyEntity GetEntity() => new()
	{
		Uid = Uid,
		Type = Type,
		HostName = HostName,
		Port = Port,
		UserName = UserName,
		Password = Password,
		Secret = Secret,
	};

	#endregion
}
