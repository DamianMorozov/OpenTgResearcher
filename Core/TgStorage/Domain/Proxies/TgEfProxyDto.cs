namespace TgStorage.Domain.Proxies;

/// <summary> EF proxy DTO </summary>
public sealed partial class TgEfProxyDto : TgDtoBase, ITgEfProxyDto
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

    /// <inheritdoc />
    public override string ToString() => $"{Type} | {HostName} | {Port} | {UserName} | {Password} | {Secret}";

	#endregion
}
