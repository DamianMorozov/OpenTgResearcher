// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Contacts;

/// <summary> Contact view-model </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfContactViewModel : TgEntityViewModelBase<TgEfContactEntity>, ITgDtoViewModel
{
	#region Public and private fields, properties, constructor

	public override TgEfContactRepository Repository { get; } = new();
	[ObservableProperty]
	public partial TgEfContactDto Dto { get; set; } = null!;


	public TgEfContactViewModel(TgEfContactEntity item) : base()
	{
		Fill(item);
	}

	public TgEfContactViewModel() : base()
	{
		TgEfContactEntity item = new();
		Fill(item);
	}

	#endregion

	#region Public and private methods

	public override string ToString() => Dto.ToString() ?? string.Empty;

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfContactEntity item)
	{
		Dto ??= new();
		Dto.Fill(item, isUidCopy: true);
	}

	public async Task<TgEfStorageResult<TgEfContactEntity>> SaveAsync() =>
		await Repository.SaveAsync(Dto.GetEntity());

	#endregion
}