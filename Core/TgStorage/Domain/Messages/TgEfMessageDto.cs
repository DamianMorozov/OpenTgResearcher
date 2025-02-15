// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Messages;

/// <summary> Message DTO </summary>
public sealed partial class TgEfMessageDto : TgDtoBase, ITgDto<TgEfMessageDto, TgEfMessageEntity>
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial DateTime DtCreated { get; set; }
	[ObservableProperty]
	public partial long SourceId { get; set; }
	[ObservableProperty]
	public partial long Id { get; set; }
	[ObservableProperty]
	public partial TgEnumMessageType Type { get; set; }
	[ObservableProperty]
	public partial long Size { get; set; }
	[ObservableProperty]
	public partial string Message { get; set; } = string.Empty;
	[ObservableProperty]
	public partial TgEnumDirection Direction { get; set; } = TgEnumDirection.Default;

	#endregion

	#region Public and private methods

	public string DtChangedString => $"{DtCreated:yyyy-MM-dd HH:mm:ss}";

	public override string ToString() => $"{DtCreated} | {SourceId} | {Id} | {Type} | {Size} | {Message}";

	public TgEfMessageDto Fill(TgEfMessageDto dto, bool isUidCopy)
	{
		base.Fill(dto, isUidCopy);
		DtCreated = dto.DtCreated;
		SourceId = dto.SourceId;
		Id = dto.Id;
		Type = dto.Type;
		Size = dto.Size;
		Message = dto.Message;
		return this;
	}

	public TgEfMessageDto Fill(TgEfMessageEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		DtCreated = item.DtCreated;
		SourceId = item.SourceId;
		Id = item.Id;
		Type = item.Type;
		Size = item.Size;
		Message = item.Message;
		Direction = TgEnumDirection.Default;
		return this;
	}

	public TgEfMessageDto GetDto(TgEfMessageEntity item)
	{
		var dto = new TgEfMessageDto();
		dto.Fill(item, isUidCopy: true);
		return dto;
	}

	public TgEfMessageEntity GetEntity() => new()
	{
		Uid = Uid,
		DtCreated = DtCreated,
		SourceId = SourceId,
		Id = Id,
		Type = Type,
		Size = Size,
		Message = Message,
	};

	#endregion
}
