// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Domain.Messages;

/// <summary> Message ViewModel </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed partial class TgEfMessageViewModel : TgEntityViewModelBase<TgEfMessageEntity, TgEfMessageDto>, ITgDtoViewModel
{
	#region Fields, properties, constructor

    public override ITgEfMessageRepository Repository { get; }

    [ObservableProperty]
	public partial TgEfMessageDto Dto { get; set; } = null!;


	public TgEfMessageViewModel(Autofac.IContainer container, TgEfMessageEntity item) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfMessageRepository>();
        Fill(item);
	}

	public TgEfMessageViewModel(Autofac.IContainer container) : base()
	{
        var scope = container.BeginLifetimeScope();
        Repository = scope.Resolve<ITgEfMessageRepository>();
        TgEfMessageEntity item = new();
		Fill(item);
	}

	#endregion

	#region Methods

	public override string ToString() => Dto.ToString() ?? string.Empty;

	public override string ToDebugString() => Dto.ToDebugString();

	public void Fill(TgEfMessageEntity item)
	{
		Dto ??= new();
		Dto.Copy(item, isUidCopy: true);
	}

	#endregion
}
