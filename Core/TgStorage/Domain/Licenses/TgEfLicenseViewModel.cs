namespace TgStorage.Domain.Licenses;

public sealed partial class TgEfLicenseViewModel : TgEntityViewModelBase<TgEfLicenseEntity, TgEfLicenseDto>, ITgDtoViewModel
{
	#region Fields, properties, constructor

	public override ITgEfLicenseRepository Repository { get; }

	[ObservableProperty]
	public partial TgEfLicenseDto Dto { get; set; } = null!;

	public TgEfLicenseViewModel(Autofac.IContainer container, TgEfLicenseEntity item) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfLicenseRepository>();
        Fill(item);
	}

	public TgEfLicenseViewModel(Autofac.IContainer container) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfLicenseRepository>();
        TgEfLicenseEntity item = new();
		Fill(item);
	}

	#endregion

	#region Methods

	public override string ToString() => Dto.ToString();

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfLicenseEntity item)
	{
		Dto ??= new();
		Dto.Copy(item, isUidCopy: true);
	}

	public async Task<TgEfStorageResult<TgEfLicenseEntity>> SaveAsync() => await Repository.SaveAsync(Dto.GetEntity());

	#endregion
}
