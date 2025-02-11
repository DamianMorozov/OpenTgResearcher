// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Contracts;

public interface ITgDto<TDto, TEntity> : ITgCommon where TDto : new() where TEntity : new()
{
	#region Public and private fields, properties, constructor

	public bool IsLoad { get; set; }
	public Guid Uid { get; set; }
	public bool IsExistsAtStorage { get; set; }

	#endregion

	#region Public and private methods

	public string ToString();
	public TDto Fill(TDto dto, bool isUidCopy);
	public TDto Fill(TEntity item, bool isUidCopy);
	public TDto GetDto(TEntity item);
	public TEntity GetEntity();

	#endregion
}