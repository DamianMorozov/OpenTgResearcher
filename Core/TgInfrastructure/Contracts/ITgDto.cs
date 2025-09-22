namespace TgInfrastructure.Contracts;

/// <summary> DTO </summary>
public interface ITgDto : ITgDebug
{
	#region Fields, properties, constructor

	public bool IsLoad { get; set; }
	public Guid Uid { get; set; }
	public bool IsExistsAtStorage { get; set; }

	#endregion

	#region Methods

	public string ToString();
    public string ToConsoleString();
    public string ToConsoleHeaderString();

    #endregion
}
