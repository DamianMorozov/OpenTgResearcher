// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Contracts;

/// <summary> DTO </summary>
public interface ITgDto<TEfEntity, TDto> : ITgDebug
	where TEfEntity : class, ITgEfEntity<TEfEntity>, new()
    where TDto : class, ITgDto<TEfEntity, TDto>, new()
{
	#region Public and private fields, properties, constructor

	public bool IsLoad { get; set; }
	public Guid Uid { get; set; }
	public bool IsExistsAtStorage { get; set; }

	#endregion

	#region Public and private methods

	public string ToString();
	public TDto Copy(TDto dto, bool isUidCopy);
	public TDto Copy(TEfEntity item, bool isUidCopy);
	public TDto GetNewDto(TEfEntity item);
    public TEfEntity GetNewEntity(TDto dto);
	public TEfEntity GetNewEntity();

    #endregion
}