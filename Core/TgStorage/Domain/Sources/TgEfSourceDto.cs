﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Sources;

public sealed partial class TgEfSourceDto : TgViewModelBase, ITgDbEntity, ITgDbFillEntity<TgEfSourceDto>
{
	[ObservableProperty]
	private Guid _uid;
	[ObservableProperty]
	private long _id;
	[ObservableProperty]
	private string _userName = string.Empty;
	[ObservableProperty]
	private string _dtChanged = string.Empty;
	[ObservableProperty]
	private bool _isSourceActive;
	[ObservableProperty]
	private bool _isAutoUpdate;
	[ObservableProperty]
	private string _title = string.Empty;
	[ObservableProperty]
	private int _firstId;
	[ObservableProperty]
	private int _count;

	public void Default()
	{
		throw new NotImplementedException();
	}

	public TgEfSourceDto Fill(TgEfSourceDto item, bool isUidCopy)
	{
		throw new NotImplementedException();
	}
}
