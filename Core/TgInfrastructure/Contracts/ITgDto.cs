namespace TgInfrastructure.Contracts;

/// <summary> DTO </summary>
public interface ITgDto<TEfEntity, TDto> : ITgDebug
	where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
    where TDto : class, ITgDto<TEfEntity, TDto>, new()
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
    public TDto Copy(TDto dto, bool isUidCopy);
	public TDto Copy(TEfEntity item, bool isUidCopy);
	public TDto GetNewDto(TEfEntity item);
    public TEfEntity GetEntity();

    #endregion
}
