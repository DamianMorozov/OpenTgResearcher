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

    /// <inheritdoc />
    public override string ToString() => Dto.ToString();

    /// <inheritdoc />
	public override string ToDebugString() => Dto.ToDebugString();

    /// <inheritdoc />
	public void Fill(TgEfUserEntity item) => Dto = TgEfDomainUtils.CreateNewDto(item, isUidCopy: true);

    /// <inheritdoc />
	public async Task<TgEfStorageResult<TgEfUserEntity>> SaveAsync() => await Repository.SaveAsync(TgEfDomainUtils.CreateNewEntity(Dto, isUidCopy: true));

    #endregion
}
