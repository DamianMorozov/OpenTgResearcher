// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Versions;

/// <summary> Version view-model </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfVersionViewModel : TgEntityViewModelBase<TgEfVersionEntity, TgEfVersionDto>, ITgDtoViewModel
{
	#region Public and private fields, properties, constructor

	public override TgEfVersionRepository Repository { get; } = new();
	[ObservableProperty]
	public partial TgEfVersionDto Dto { get; set; } = null!;

	public TgEfVersionViewModel(TgEfVersionEntity item) : base()
	{
		Fill(item);
	}

	public TgEfVersionViewModel() : base()
	{
		TgEfVersionEntity item = new();
		Fill(item);
	}

	#endregion

	#region Public and private methods

	public override string ToString() => Dto.ToString() ?? string.Empty;

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfVersionEntity item)
	{
		Dto ??= new();
		Dto.Copy(item, isUidCopy: true);
	}

	public async Task<TgEfStorageResult<TgEfVersionEntity>> SaveAsync() =>
		await Repository.SaveAsync(Dto.GetNewEntity());

	#endregion
}