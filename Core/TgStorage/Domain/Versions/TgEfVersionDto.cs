// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Contacts;

/// <summary> Proxy DTO </summary>
public sealed partial class TgEfVersionDto : TgDtoBase, ITgDto<TgEfVersionDto, TgEfVersionEntity>
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial short Version { get; set; }
	[ObservableProperty]
	public partial string Description { get; set; } = string.Empty;

	#endregion
	
	#region Public and private methods

	public override string ToString() => $"{Version} | {Description}";

	public TgEfVersionDto Fill(TgEfVersionDto dto, bool isUidCopy)
	{
		base.Fill(dto, isUidCopy);
		Version = dto.Version;
		Description = dto.Description;
		return this;
	}

	public TgEfVersionDto Fill(TgEfVersionEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		Version = item.Version;
		Description = item.Description;
		return this;
	}

	public TgEfVersionDto GetDto(TgEfVersionEntity item)
	{
		var dto = new TgEfVersionDto();
		dto.Fill(item, isUidCopy: true);
		return dto;
	}

	public TgEfVersionEntity GetEntity() => new()
	{
		Uid = Uid,
		Version = Version,
		Description = Description,
	};

	#endregion
}
