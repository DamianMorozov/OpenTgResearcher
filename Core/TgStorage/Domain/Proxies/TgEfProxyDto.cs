// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Contacts;

/// <summary> Proxy DTO </summary>
public sealed partial class TgEfProxyDto : TgDtoBase, ITgDto<TgEfProxyDto, TgEfProxyEntity>
{
	#region Public and private fields, properties, constructor

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

	#endregion

	#region Public and private methods

	public override string ToString() => $"{Type} | {HostName} | {Port} | {UserName} | {Password} | {Secret}";

	public TgEfProxyDto Fill(TgEfProxyDto dto, bool isUidCopy)
	{
		base.Fill(dto, isUidCopy);
		Type = dto.Type;
		HostName = dto.HostName;
		Port = dto.Port;
		UserName = dto.UserName;
		Password = dto.Password;
		Secret = dto.Secret;
		return this;
	}

	public TgEfProxyDto Fill(TgEfProxyEntity item, bool isUidCopy)
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

	public TgEfProxyDto GetDto(TgEfProxyEntity item)
	{
		var dto = new TgEfProxyDto();
		dto.Fill(item, isUidCopy: true);
		return dto;
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
