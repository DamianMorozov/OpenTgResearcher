// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Contracts;

/// <summary> SQL entity </summary>
public interface ITgEfEntity<TEfEntity> : ITgCommon 
	where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
{
	#region Public and private fields, properties, constructor

	public Guid Uid { get; set; }
	public byte[]? RowVersion { get; set; }

	#endregion

	#region Public and private methods

	public TEfEntity Copy(TEfEntity item, bool isUidCopy);

	#endregion
}