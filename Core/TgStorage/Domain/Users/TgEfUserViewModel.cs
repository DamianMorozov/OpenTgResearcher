// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Users;

/// <summary> User ViewModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfUserViewModel : TgEntityViewModelBase<TgEfUserEntity, TgEfUserDto>, ITgDtoViewModel
{
	#region Fields, properties, constructor

	public override ITgEfUserRepository Repository { get; }
	[ObservableProperty]
	public partial TgEfUserDto Dto { get; set; } = null!;


	public TgEfUserViewModel(Autofac.IContainer container, TgEfUserEntity item) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfUserRepository>();
        Fill(item);
	}

	public TgEfUserViewModel(Autofac.IContainer container) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfUserRepository>();
        TgEfUserEntity item = new();
		Fill(item);
	}

	#endregion

	#region Methods

	public override string ToString() => Dto.ToString();

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfUserEntity item)
	{
		Dto ??= new();
		Dto.Copy(item, isUidCopy: true);
	}

	public async Task<TgEfStorageResult<TgEfUserEntity>> SaveAsync() => await Repository.SaveAsync(Dto.GetEntity());

	#endregion
}