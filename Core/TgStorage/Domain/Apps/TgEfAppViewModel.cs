﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Apps;

/// <summary> App view-model </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfAppViewModel : TgEntityViewModelBase<TgEfAppEntity>, ITgDtoViewModel
{
	#region Public and private fields, properties, constructor

	public override TgEfAppRepository Repository { get; } = new(TgEfUtils.EfContext);
	[ObservableProperty]
	public partial TgEfAppDto Dto { get; set; } = default!;

	public TgEfAppViewModel(TgEfAppEntity item) : base()
	{
		Fill(item);
	}

	public TgEfAppViewModel() : base()
	{
		TgEfAppEntity item = new();
		Fill(item);
	}

	#endregion

	#region Public and private methods

	public override string ToString() => Dto.ToString() ?? string.Empty;

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfAppEntity item)
	{
		Dto ??= new();
		Dto.Fill(item, isUidCopy: true);
	}

	public async Task<TgEfStorageResult<TgEfAppEntity>> SaveAsync() =>
		await Repository.SaveAsync(Dto.GetEntity());

	#endregion
}