namespace TgInfrastructure.Contracts;

/// <summary> SQL entity </summary>
public interface ITgEfEntity<TEfEntity> : ITgDebug 
	where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
{
	#region Fields, properties, constructor

	public Guid Uid { get; set; }

	#endregion

	#region Methods

	public TEfEntity Copy(TEfEntity item, bool isUidCopy);

	#endregion
}
