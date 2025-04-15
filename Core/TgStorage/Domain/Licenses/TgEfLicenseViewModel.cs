// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Licenses;

public sealed partial class TgEfLicenseViewModel : TgEntityViewModelBase<TgEfLicenseEntity, TgEfLicenseDto>, ITgDtoViewModel
{
	#region Public and private fields, properties, constructor

	public override TgEfLicenseRepository Repository { get; } = new();

	[ObservableProperty]
	public partial TgEfLicenseDto Dto { get; set; } = null!;

	public TgEfLicenseViewModel(TgEfLicenseEntity item) : base()
	{
		Fill(item);
	}

	public TgEfLicenseViewModel() : base()
	{
		TgEfLicenseEntity item = new();
		Fill(item);
	}

	#endregion

	#region Public and private methods

	public override string ToString() => Dto.ToString() ?? string.Empty;

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfLicenseEntity item)
	{
		Dto ??= new();
		Dto.Copy(item, isUidCopy: true);
	}

	public async Task<TgEfStorageResult<TgEfLicenseEntity>> SaveAsync() =>
		await Repository.SaveAsync(Dto.GetNewEntity());

	#endregion
}