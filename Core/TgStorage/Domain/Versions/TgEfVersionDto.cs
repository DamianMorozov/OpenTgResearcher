// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Versions;

/// <summary> Proxy DTO </summary>
public sealed partial class TgEfVersionDto : TgDtoBase, ITgDto<TgEfVersionEntity, TgEfVersionDto>
{
	#region Fields, properties, constructor

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

    #region Methods

    public override string ToConsoleString() => 
        $"{Version,3} | " +
        $"{TgDataFormatUtils.GetFormatString(Description, 50).TrimEnd(),-50}";

    public override string ToConsoleHeaderString() =>
        $"{TgDataFormatUtils.GetFormatString(nameof(Version), 3).TrimEnd(),-3} | " +
        $"Description";

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

	public TgEfVersionEntity GetEntity() => new()
	{
		Uid = Uid,
		Version = Version,
		Description = Description,
	};

	#endregion
}
