namespace TgInfrastructure.Contracts;

/// <summary> SQL entity </summary>
public interface ITgEfIdEntity : ITgEfEntity
{
    #region Fields, properties, constructor

    public long Id { get; set; }

    #endregion
}
