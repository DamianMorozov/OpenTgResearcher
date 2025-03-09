// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Versions;

/// <summary> Proxy DTO </summary>
public sealed partial class TgEfVersionDto : TgDtoBase, ITgDto<TgEfVersionEntity, TgEfVersionDto>
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial short Version { get; set; }
	[ObservableProperty]
	public partial string Description { get; set; }

	public TgEfVersionDto() : base()
	{
		Version = 0;
		Description = string.Empty;
	}

	#endregion
	
	#region Public and private methods

	public override string ToString() => $"{Version} | {Description}";

	public TgEfVersionDto Copy(TgEfVersionDto dto, bool isUidCopy)
	{
		base.Copy(dto, isUidCopy);
		Version = dto.Version;
		Description = dto.Description;
		return this;
	}

	public TgEfVersionDto Copy(TgEfVersionEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		Version = item.Version;
		Description = item.Description;
		return this;
	}

	public TgEfVersionDto GetNewDto(TgEfVersionEntity item) => new TgEfVersionDto().Copy(item, isUidCopy: true);

	public TgEfVersionEntity GetNewEntity(TgEfVersionDto dto) => new()
	{
		Uid = dto.Uid,
		Version = dto.Version,
		Description = dto.Description,
	};

	public TgEfVersionEntity GetNewEntity() => new()
	{
		Uid = Uid,
		Version = Version,
		Description = Description,
	};

	#endregion
}
