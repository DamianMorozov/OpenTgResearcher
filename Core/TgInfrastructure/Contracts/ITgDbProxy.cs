// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Contracts;

/// <summary> Proxy entity </summary>
public interface ITgDbProxy<TEfEntity> : ITgEfEntity<TEfEntity>
	where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
{
	public TgEnumProxyType Type { get; set; }
	public string HostName { get; set; }
	public ushort Port { get; set; }
	public string UserName { get; set; }
	public string Password { get; set; }
	public string Secret { get; set; }
}