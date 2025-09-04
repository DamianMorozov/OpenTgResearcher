// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Filters;

/// <summary> Contact ViewModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfFilterViewModel : TgEntityViewModelBase<TgEfFilterEntity, TgEfFilterDto>, ITgDtoViewModel
{
	#region Fields, properties, constructor

	public override ITgEfFilterRepository Repository { get; }
	[ObservableProperty]
	public partial TgEfFilterDto Dto { get; set; } = null!;


	public TgEfFilterViewModel(Autofac.IContainer container, TgEfFilterEntity item) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfFilterRepository>();
        Fill(item);
	}

	public TgEfFilterViewModel(Autofac.IContainer container) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfFilterRepository>();
        TgEfFilterEntity item = new();
		Fill(item);
	}

	#endregion

	#region Methods

	public override string ToString() => Dto.ToString() ?? string.Empty;

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfFilterEntity item)
	{
		Dto ??= new();
		Dto.Copy(item, isUidCopy: true);
	}

	public async Task<TgEfStorageResult<TgEfFilterEntity>> SaveAsync() => await Repository.SaveAsync(Dto.GetEntity());

	#endregion
}