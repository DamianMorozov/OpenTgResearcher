namespace TgStorage.Validators;

/// <summary> Proxy validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfProxyValidator : TgEfValidatorBase<TgEfProxyEntity>
{
	#region Fields, properties, constructor

	public TgEfProxyValidator()
	{
		RuleFor(item => item.Type)
			.NotNull();
		RuleFor(item => item.HostName)
			.NotEmpty()
			.NotNull();
		RuleFor(item => item.Port)
			.NotNull()
			.GreaterThan(ushort.MinValue)
			.LessThan(ushort.MaxValue);
	}

	#endregion
}
