// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Sources;

/// <summary> Source view-model </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfSourceViewModel : TgEntityViewModelBase<TgEfSourceEntity, TgEfSourceDto>, ITgEfSourceViewModel
{
	#region Public and private fields, properties, constructor

	public override TgEfSourceRepository Repository { get; } = new();
    [ObservableProperty]
	public partial TgEfSourceDto Dto { get; set; } = null!;

    public TgEfSourceViewModel(TgEfSourceEntity item) : base()
    {
		Fill(item);
    }

    public TgEfSourceViewModel() : base()
    {
		TgEfSourceEntity item = new();
		Fill(item);
	}

	#endregion

	#region Public and private methods

	public override string ToString() => Dto.ToString();

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfSourceEntity item)
    {
		Dto ??= new();
		Dto.Copy(item, isUidCopy: true);
	}

	public async Task<TgEfStorageResult<TgEfSourceEntity>> SaveAsync() =>
		await Repository.SaveAsync(Dto.GetNewEntity());

	#endregion
}