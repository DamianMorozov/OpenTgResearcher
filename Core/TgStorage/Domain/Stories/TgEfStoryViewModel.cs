// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Stories;

/// <summary> Story view-model </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfStoryViewModel : TgEntityViewModelBase<TgEfStoryEntity, TgEfStoryDto>, ITgDtoViewModel
{
	#region Public and private fields, properties, constructor

	public override TgEfStoryRepository Repository { get; } = new();
	[ObservableProperty]
	public partial TgEfStoryDto Dto { get; set; } = null!;

	public TgEfStoryViewModel(TgEfStoryEntity item) : base()
	{
		Fill(item);
	}

	public TgEfStoryViewModel() : base()
	{
		TgEfStoryEntity item = new();
		Fill(item);
	}

	#endregion

	#region Public and private methods

	public override string ToString() => Dto.ToString() ?? string.Empty;

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfStoryEntity item)
	{
		Dto ??= new();
		Dto.Copy(item, isUidCopy: true);
	}

	public async Task<TgEfStorageResult<TgEfStoryEntity>> SaveAsync() =>
		await Repository.SaveAsync(Dto.GetNewEntity());

	#endregion
}