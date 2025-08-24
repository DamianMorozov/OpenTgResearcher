// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Documents;

/// <summary> Contact DTO </summary>
public sealed partial class TgEfDocumentDto : TgDtoBase, ITgDto<TgEfDocumentEntity, TgEfDocumentDto>
{
	#region Fields, properties, constructor

	[ObservableProperty]
	public partial long SourceId { get; set; }
	[ObservableProperty]
	public partial long Id { get; set; }
	[ObservableProperty]
	public partial int MessageId { get; set; }
	[ObservableProperty]
	public partial string FileName { get; set; } = string.Empty;
	[ObservableProperty]
	public partial long FileSize { get; set; }
	[ObservableProperty]
	public partial long AccessHash { get; set; }

	public TgEfDocumentDto() : base()
	{
		SourceId = 0;
		Id = 0;
		MessageId = 0;
		FileName = string.Empty;
		FileSize = 0;
		AccessHash = 0;
	}

	#endregion

	#region Private methods

	public override string ToString() => $"{SourceId} | {Id} | {AccessHash}";

	public TgEfDocumentDto Copy(TgEfDocumentDto dto, bool isUidCopy)
	{
		base.Copy(dto, isUidCopy);
		SourceId = dto.SourceId;
		Id = dto.Id;
		MessageId = dto.MessageId;
		FileName = dto.FileName;
		FileSize = dto.FileSize;
		AccessHash = dto.AccessHash;
		return this;
	}

	public TgEfDocumentDto Copy(TgEfDocumentEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		SourceId = item.SourceId;
		Id = item.Id;
		MessageId = item.MessageId;
		FileName = item.FileName;
		FileSize = item.FileSize;
		AccessHash = item.AccessHash;
		return this;
	}

	public TgEfDocumentDto GetNewDto(TgEfDocumentEntity item) => new TgEfDocumentDto().Copy(item, isUidCopy: true);

	public TgEfDocumentEntity GetNewEntity(TgEfDocumentDto dto) => new()
	{
		Uid = dto.Uid,
		SourceId = dto.SourceId,
		Id = dto.Id,
		MessageId = dto.MessageId,
		FileName = dto.FileName,
		FileSize = dto.FileSize,
		AccessHash = dto.AccessHash,
	};

	public TgEfDocumentEntity GetNewEntity() => new()
	{
		Uid = Uid,
		SourceId = SourceId,
		Id = Id,
		MessageId = MessageId,
		FileName = FileName,
		FileSize = FileSize,
		AccessHash = AccessHash,
	};

	#endregion
}
