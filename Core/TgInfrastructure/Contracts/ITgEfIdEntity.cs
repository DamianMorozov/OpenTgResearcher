namespace TgInfrastructure.Contracts;

/// <summary> SQL entity </summary>
public interface ITgEfIdEntity<TEfEntity> : ITgEfEntity<TEfEntity>
	where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
{
    #region Fields, properties, constructor

    public long Id { get; set; }

    #endregion
}
