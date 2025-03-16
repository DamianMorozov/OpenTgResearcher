// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Sources;

/// <summary> Source DTO </summary>
public sealed partial class TgEfSourceLiteDto : TgDtoBase, ITgDto<TgEfSourceEntity, TgEfSourceLiteDto>
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial long Id { get; set; }
	[ObservableProperty]
	public partial string DtChangedString { get; set; }
	[ObservableProperty]
	public partial string UserName { get; set; }
	[ObservableProperty]
	public partial bool IsSourceActive { get; set; }
	[ObservableProperty]
	public partial string Title { get; set; }
	[ObservableProperty]
	public partial int FirstId { get; set; }
	[ObservableProperty]
	public partial int Count { get; set; }
	[ObservableProperty]
	public partial bool IsAutoUpdate { get; set; }
	[ObservableProperty]
	public partial bool IsCreatingSubdirectories { get; set; }
	[ObservableProperty]
	public partial bool IsUserAccess { get; set; }
	[ObservableProperty]
	public partial bool IsDownload { get; set; }
	[ObservableProperty]
	public partial int ProgressPercent { get; set; }
	[ObservableProperty]
	public partial string ProgressPercentString { get; set; }

	public TgEfSourceLiteDto() : base()
	{
		Id = 0;
		DtChangedString = string.Empty;
		UserName = string.Empty;
		IsSourceActive = false;
		Title = string.Empty;
		FirstId = 0;
		Count = 0;
		IsAutoUpdate = false;
		IsCreatingSubdirectories = false;
		IsUserAccess = false;
		IsDownload = false;
		ProgressPercent = 0;
		ProgressPercentString = string.Empty;
	}

	#endregion

	#region Public and private methods

	public override string ToString() => ProgressPercentString;

	public TgEfSourceLiteDto Copy(TgEfSourceLiteDto dto, bool isUidCopy)
	{
		base.Copy(dto, isUidCopy);
		Id = dto.Id;
		DtChangedString = dto.DtChangedString;
		UserName = dto.UserName;
		IsSourceActive = dto.IsActive;
		Title = dto.Title;
		FirstId = dto.FirstId;
		Count = dto.Count;
		IsAutoUpdate = dto.IsAutoUpdate;
		IsCreatingSubdirectories = dto.IsCreatingSubdirectories;
		IsUserAccess = dto.IsUserAccess;
		IsDownload = dto.IsDownload;
		ProgressPercent = dto.ProgressPercent;
		ProgressPercentString = dto.ProgressPercentString;
		return this;
	}

	public TgEfSourceLiteDto Copy(TgEfSourceEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		Id = item.Id;
		DtChangedString = $"{item.DtChanged:yyyy-MM-dd HH:mm:ss}";
		UserName = item.UserName ?? string.Empty;
		IsSourceActive = item.IsActive;
		Title = item.Title ?? string.Empty;
		FirstId = item.FirstId;
		Count = item.Count;
		IsAutoUpdate = item.IsAutoUpdate;
		IsCreatingSubdirectories = item.IsCreatingSubdirectories;
		IsUserAccess = item.IsUserAccess;
		ProgressPercent = item.Count == 0 ? 0 : item.FirstId * 100 / item.Count;
		ProgressPercentString = ProgressPercent == 0 ? "{0:00.00} %" : $"{ProgressPercent:#00.00} %";
		return this;
	}

	public TgEfSourceLiteDto GetNewDto(TgEfSourceEntity item) => new TgEfSourceLiteDto().Copy(item, isUidCopy: true);

	public TgEfSourceEntity GetNewEntity(TgEfSourceLiteDto dto) => new()
	{
		Uid = dto.Uid,
		Id = dto.Id,
		IsActive = dto.IsSourceActive,
		UserName = dto.UserName,
		Title = dto.Title,
		FirstId = dto.FirstId,
		Count = dto.Count,
		IsAutoUpdate = dto.IsAutoUpdate,
		IsCreatingSubdirectories = dto.IsCreatingSubdirectories,
		IsUserAccess = dto.IsUserAccess,
	};

	public TgEfSourceEntity GetNewEntity() => new()
	{
		Uid = Uid,
		Id = Id,
		IsActive = IsSourceActive,
		UserName = UserName,
		Title = Title,
		FirstId = FirstId,
		Count = Count,
		IsAutoUpdate = IsAutoUpdate,
		IsCreatingSubdirectories = IsCreatingSubdirectories,
		IsUserAccess = IsUserAccess,
	};

	public void SetIsDownload(bool isDownload) => IsDownload = isDownload;

	#endregion
}