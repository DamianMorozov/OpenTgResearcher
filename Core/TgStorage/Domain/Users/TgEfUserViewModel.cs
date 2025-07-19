// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Users;

/// <summary> User view-model </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfUserViewModel : TgEntityViewModelBase<TgEfUserEntity, TgEfUserDto>, ITgDtoViewModel
{
	#region Public and private fields, properties, constructor

	public override TgEfUserRepository Repository { get; } = new();
	[ObservableProperty]
	public partial TgEfUserDto Dto { get; set; } = null!;


	public TgEfUserViewModel(TgEfUserEntity item) : base()
	{
		Fill(item);
	}

	public TgEfUserViewModel() : base()
	{
		TgEfUserEntity item = new();
		Fill(item);
	}

	#endregion

	#region Public and private methods

	public override string ToString() => Dto.ToString() ?? string.Empty;

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfUserEntity item)
	{
		Dto ??= new();
		Dto.Copy(item, isUidCopy: true);
	}

	public async Task<TgEfStorageResult<TgEfUserEntity>> SaveAsync() =>
		await Repository.SaveAsync(Dto.GetNewEntity());

	#endregion
}