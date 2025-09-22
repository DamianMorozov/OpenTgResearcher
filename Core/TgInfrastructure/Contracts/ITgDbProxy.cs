namespace TgInfrastructure.Contracts;

/// <summary> Proxy entity </summary>
public interface ITgDbProxy : ITgEfEntity
{
	public TgEnumProxyType Type { get; set; }
	public string HostName { get; set; }
	public ushort Port { get; set; }
	public string UserName { get; set; }
	public string Password { get; set; }
	public string Secret { get; set; }
}
