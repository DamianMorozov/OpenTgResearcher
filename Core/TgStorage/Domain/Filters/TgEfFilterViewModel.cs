// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Filters;

/// <summary> Contact view-model </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfFilterViewModel : TgEntityViewModelBase<TgEfFilterEntity, TgEfFilterDto>, ITgDtoViewModel
{
	#region Public and private fields, properties, constructor

	public override TgEfFilterRepository Repository { get; } = new();
	[ObservableProperty]
	public partial TgEfFilterDto Dto { get; set; } = null!;


	public TgEfFilterViewModel(TgEfFilterEntity item) : base()
	{
		Fill(item);
	}

	public TgEfFilterViewModel() : base()
	{
		TgEfFilterEntity item = new();
		Fill(item);
	}

	#endregion

	#region Public and private methods

	public override string ToString() => Dto.ToString() ?? string.Empty;

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfFilterEntity item)
	{
		Dto ??= new();
		Dto.Copy(item, isUidCopy: true);
	}

	public async Task<TgEfStorageResult<TgEfFilterEntity>> SaveAsync() =>
		await Repository.SaveAsync(Dto.GetNewEntity());

	#endregion
}