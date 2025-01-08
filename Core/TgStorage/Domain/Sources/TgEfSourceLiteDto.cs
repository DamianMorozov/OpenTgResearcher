﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Sources;

/// <summary> Source DTO </summary>
public sealed partial class TgEfSourceLiteDto : TgDtoBase, ITgDto<TgEfSourceLiteDto, TgEfSourceEntity>
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial long Id { get; set; }
	[ObservableProperty]
	public partial string DtChangedString { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string UserName { get; set; } = string.Empty;
	[ObservableProperty]
	public partial bool IsSourceActive { get; set; }
	[ObservableProperty]
	public partial string Title { get; set; } = string.Empty;
	[ObservableProperty]
	public partial int FirstId { get; set; }
	[ObservableProperty]
	public partial int Count { get; set; }
	[ObservableProperty]
	public partial bool IsAutoUpdate { get; set; }
	[ObservableProperty]
	public partial bool IsDownload { get; set; }
	[ObservableProperty]
	public partial string ProgressPercentString { get; set; }

	#endregion

	#region Public and private methods

	public override string ToString() => ProgressPercentString;

	public TgEfSourceLiteDto Fill(TgEfSourceLiteDto dto, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = dto.Uid;
		DtChangedString = dto.DtChangedString;
		Id = dto.Id;
		UserName = dto.UserName;
		IsSourceActive = dto.IsActive;
		Title = dto.Title;
		FirstId = dto.FirstId;
		Count = dto.Count;
		IsAutoUpdate = dto.IsAutoUpdate;
		IsDownload = dto.IsDownload;
		ProgressPercentString = dto.ProgressPercentString;
		return this;
	}

	public TgEfSourceLiteDto Fill(TgEfSourceEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		DtChangedString = $"{item.DtChanged:yyyy-MM-dd HH:mm:ss}";
		Id = item.Id;
		UserName = item.UserName ?? string.Empty;
		IsSourceActive = item.IsActive;
		Title = item.Title ?? string.Empty;
		FirstId = item.FirstId;
		Count = item.Count;
		IsAutoUpdate = item.IsAutoUpdate;
		var progress = (float)item.FirstId * 100 / item.Count;
		ProgressPercentString = progress == 0 ? $"{0:00.00} %" : $"{progress:#00.00} %";
		return this;
	}

	public TgEfSourceLiteDto GetDto(TgEfSourceEntity item)
	{
		var dto = new TgEfSourceLiteDto();
		dto.Fill(item, isUidCopy: true);
		return dto;
	}

	public TgEfSourceEntity GetEntity() => throw new NotImplementedException(TgLocaleHelper.Instance.UseOverrideMethod);

	public void SetIsDownload(bool isDownload) => IsDownload = isDownload;

	#endregion
}