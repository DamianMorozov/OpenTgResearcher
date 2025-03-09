// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Messages;

/// <summary> Message DTO </summary>
public sealed partial class TgEfMessageDto : TgDtoBase, ITgDto<TgEfMessageEntity, TgEfMessageDto>
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

	public TgEfMessageDto() : base()
	{
		DtCreated = DateTime.MinValue;
		SourceId = 0;
		Id = 0;
		Type = TgEnumMessageType.Message;
		Size = 0;
		Message = string.Empty;
	}

	#endregion

	#region Public and private methods

	public string DtChangedString => $"{DtCreated:yyyy-MM-dd HH:mm:ss}";

	public override string ToString() => $"{DtCreated} | {SourceId} | {Id} | {Type} | {Size} | {Message}";

	public TgEfMessageDto Copy(TgEfMessageDto dto, bool isUidCopy)
	{
		base.Copy(dto, isUidCopy);
		DtCreated = dto.DtCreated;
		SourceId = dto.SourceId;
		Id = dto.Id;
		Type = dto.Type;
		Size = dto.Size;
		Message = dto.Message;
		return this;
	}

	public TgEfMessageDto Copy(TgEfMessageEntity item, bool isUidCopy)
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

	public TgEfMessageDto GetNewDto(TgEfMessageEntity item) => new TgEfMessageDto().Copy(item, isUidCopy: true);

	public TgEfMessageEntity GetNewEntity(TgEfMessageDto dto) => new()
	{
		Uid = dto.Uid,
		DtCreated = dto.DtCreated,
		SourceId = dto.SourceId,
		Id = dto.Id,
		Type = dto.Type,
		Size = dto.Size,
		Message = dto.Message,
	};

	public TgEfMessageEntity GetNewEntity() => new()
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
