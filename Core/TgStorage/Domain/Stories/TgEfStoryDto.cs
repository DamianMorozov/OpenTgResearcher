// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Contacts;

/// <summary> Proxy DTO </summary>
public sealed partial class TgEfStoryDto : TgDtoBase, ITgDto<TgEfStoryDto, TgEfStoryEntity>
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial DateTime DtChanged { get; set; }
	[ObservableProperty]
	public partial long Id { get; set; }
	[ObservableProperty]
	public partial long FromId { get; set; }
	[ObservableProperty]
	public partial string FromName { get; set; } = string.Empty;
	[ObservableProperty]
	public partial DateTime Date { get; set; }
	[ObservableProperty]
	public partial DateTime ExpireDate { get; set; }
	[ObservableProperty]
	public partial string Caption { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string Type { get; set; } = string.Empty;
	[ObservableProperty]
	public partial int Offset { get; set; }
	[ObservableProperty]
	public partial int Length { get; set; }
	[ObservableProperty]
	public partial string Message { get; set; } = string.Empty;
	[ObservableProperty]
	public partial bool IsDownload { get; set; }
	public bool IsReady => Id > 0;

	#endregion

	#region Public and private methods

	public string DtChangedString => $"{DtChanged:yyyy-MM-dd HH:mm:ss}";

	public override string ToString() => $"{DtChanged} | {Id} | {FromId} | {FromName} | {Date} | {Caption}";

	public TgEfStoryDto Fill(TgEfStoryDto dto, bool isUidCopy)
	{
		base.Fill(dto, isUidCopy);
		DtChanged = dto.DtChanged;
		Id = dto.Id;
		FromId = dto.FromId;
		FromName = dto.FromName;
		Date = dto.Date;
		ExpireDate = dto.ExpireDate;
		Caption = dto.Caption;
		Type = dto.Type;
		Offset = dto.Offset;
		Length = dto.Length;
		Message = dto.Message;

		IsDownload = dto.IsDownload;

		return this;
	}

	public TgEfStoryDto Fill(TgEfStoryEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		DtChanged = item.DtChanged;
		Id = item.Id;
		FromId = item.FromId ?? 0;
		FromName = item.FromName ?? string.Empty;
		Date = item.Date ?? DateTime.MinValue;
		ExpireDate = item.ExpireDate ?? DateTime.MinValue;
		Caption = item.Caption ?? string.Empty;
		Type = item.Type ?? string.Empty;
		Offset = item.Offset;
		Length = item.Length;
		Message = item.Message ?? string.Empty;

		IsDownload = false;

		return this;
	}

	public TgEfStoryDto GetDto(TgEfStoryEntity item)
	{
		var dto = new TgEfStoryDto();
		dto.Fill(item, isUidCopy: true);
		return dto;
	}

	public TgEfStoryEntity GetEntity() => new()
	{
		Uid = Uid,
		DtChanged = DtChanged,
		Id = Id,
		FromId = FromId,
		FromName = FromName,
		Date = Date,
		ExpireDate = ExpireDate,
		Caption = Caption,
		Type = Type,
		Offset = Offset,
		Message = Message,
	};

	#endregion
}
