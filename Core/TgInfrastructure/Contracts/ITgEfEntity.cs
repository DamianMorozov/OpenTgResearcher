namespace TgInfrastructure.Contracts;

/// <summary> EF entity </summary>
public interface ITgEfEntity : ITgDebug
{
	#region Fields, properties, constructor

    /// <summary> UID field </summary>
	public Guid Uid { get; set; }

    #endregion

    #region Methods

    /// <summary> Set default values </summary>
    public void Default();

    #endregion
}
