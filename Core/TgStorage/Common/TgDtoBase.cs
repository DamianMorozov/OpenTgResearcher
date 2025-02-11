﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Common;

/// <summary> Base class for TgMvvmModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public partial class TgDtoBase : ObservableRecipient
{
	#region Public and private fields, properties, constructor

	[ObservableProperty]
	public partial bool IsLoad { get; set; }
	[ObservableProperty]
	public partial Guid Uid { get; set; }
	[ObservableProperty]
	public partial bool IsExistsAtStorage { get; set; }

	#endregion

	#region Public and private methods

	public override string ToString() => ToDebugString();

	public string ToDebugString() => TgObjectUtils.ToDebugString(this);

	public TgDtoBase Fill(TgDtoBase dto, bool isUidCopy)
	{
		IsLoad = dto.IsLoad;
		if (isUidCopy)
			Uid = dto.Uid;
		IsExistsAtStorage = dto.IsExistsAtStorage;
		return this;
	}

	#endregion
}