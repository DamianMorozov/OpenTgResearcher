namespace TgStorage.Validators;

/// <summary> Version validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfVersionValidator : TgEfValidatorBase<TgEfVersionEntity>
{
	#region Fields, properties, constructor

	public TgEfVersionValidator()
	{
		RuleFor(item => item.Version)
			.NotNull()
			.GreaterThanOrEqualTo((short)0);
		RuleFor(item => item.Description)
			.NotNull()
			.NotEmpty();
	}

	#endregion
}
