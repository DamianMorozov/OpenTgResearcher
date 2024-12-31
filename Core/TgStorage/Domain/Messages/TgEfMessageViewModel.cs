﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Messages;

/// <summary> Message view-model </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfMessageViewModel : TgEntityViewModelBase<TgEfMessageEntity>, ITgDtoViewModel
{
	#region Public and private fields, properties, constructor

	public override TgEfMessageRepository Repository { get; } = new(TgEfUtils.EfContext);
	[ObservableProperty]
	private TgEfMessageDto _dto = default!;
	public Action<TgEfMessageViewModel> UpdateAction { get; set; } = _ => { };


	public TgEfMessageViewModel(TgEfMessageEntity item) : base()
	{
		Fill(item);
	}

	public TgEfMessageViewModel() : base()
	{
		TgEfMessageEntity item = Repository.GetNewItem();
		Fill(item);
	}

	#endregion

	#region Public and private methods

	public override string ToString() => Dto.ToString() ?? string.Empty;

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfMessageEntity item)
	{
		Dto ??= new();
		Dto.Fill(item, isUidCopy: true);
	}

	public async Task<TgEfStorageResult<TgEfMessageEntity>> SaveAsync() =>
		await Repository.SaveAsync(Dto.GetEntity());

	#endregion
}