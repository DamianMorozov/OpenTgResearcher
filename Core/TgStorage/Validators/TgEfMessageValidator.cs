namespace TgStorage.Validators;

/// <summary> Message validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfMessageValidator : TgEfValidatorBase<TgEfMessageEntity>
{
	#region Fields, properties, constructor

	public TgEfMessageValidator()
	{
		RuleFor(item => item.Id)
			.NotNull()
			.GreaterThanOrEqualTo(0);
		RuleFor(item => item.DtCreated)
			.NotNull();
		RuleFor(item => item.Message)
			.NotNull();
	}

	#endregion
}
