// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Versions;

/// <summary> Version ViewModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfVersionViewModel : TgEntityViewModelBase<TgEfVersionEntity, TgEfVersionDto>, ITgDtoViewModel
{
	#region Fields, properties, constructor

	public override ITgEfVersionRepository Repository { get; }
	[ObservableProperty]
	public partial TgEfVersionDto Dto { get; set; } = null!;

	public TgEfVersionViewModel(Autofac.IContainer container, TgEfVersionEntity item) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfVersionRepository>();
        Fill(item);
	}

	public TgEfVersionViewModel(Autofac.IContainer container) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfVersionRepository>();
        TgEfVersionEntity item = new();
		Fill(item);
	}

	#endregion

	#region Methods

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