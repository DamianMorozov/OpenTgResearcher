﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Contacts;

/// <summary> Filter DTO </summary>
public sealed partial class TgEfFilterDto : TgDtoBase, ITgDto<TgEfFilterDto, TgEfFilterEntity>
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial bool IsEnabled { get; set; }
	[ObservableProperty]
	public partial TgEnumFilterType FilterType { get; set; }
	[ObservableProperty]
	public partial string Name { get; set; } = string.Empty;
	[ObservableProperty]
	public partial string Mask { get; set; } = string.Empty;
	[ObservableProperty]
	public partial long Size { get; set; }
	[ObservableProperty]
	public partial TgEnumFileSizeType SizeType { get; set; }

	public long SizeAtBytes => SizeType switch
	{
		TgEnumFileSizeType.KBytes => Size * 1024,
		TgEnumFileSizeType.MBytes => Size * 1024 * 1024,
		TgEnumFileSizeType.GBytes => Size * 1024 * 1024 * 1024,
		TgEnumFileSizeType.TBytes => Size * 1024 * 1024 * 1024 * 1024,
		_ => Size,
	};

	#endregion

	#region Public and private methods

	public override string ToString() => $"{IsEnabled} | {FilterType} | {Name} | {Mask} | {Size} | {SizeType}";

	public TgEfFilterDto Fill(TgEfFilterDto dto, bool isUidCopy)
	{
		base.Fill(dto, isUidCopy);
		IsEnabled = dto.IsEnabled;
		FilterType = dto.FilterType;
		Name = dto.Name;
		Mask = dto.Mask;
		Size = dto.Size;
		SizeType = dto.SizeType;
		return this;
	}

	public TgEfFilterDto Fill(TgEfFilterEntity item, bool isUidCopy)
	{
		if (isUidCopy)
			Uid = item.Uid;
		IsEnabled = item.IsEnabled;
		FilterType = item.FilterType;
		Name = item.Name;
		Mask = item.Mask;
		Size = item.Size;
		SizeType = item.SizeType;
		return this;
	}

	public TgEfFilterDto GetDto(TgEfFilterEntity item)
	{
		var dto = new TgEfFilterDto();
		dto.Fill(item, isUidCopy: true);
		return dto;
	}

	public TgEfFilterEntity GetEntity() => new()
	{
		Uid = Uid,
		IsEnabled = IsEnabled,
		FilterType = FilterType,
		Name = Name,
		Mask = Mask,
		Size = Size,
		SizeType = SizeType,
	};

	#endregion
}
