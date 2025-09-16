namespace TgStorage.Validators;

/// <summary> Source validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfSourceValidator : TgEfValidatorBase<TgEfSourceEntity>
{
	#region Fields, properties, constructor

	public TgEfSourceValidator()
	{
		RuleFor(item => item.Title)
			.NotNull();
		RuleFor(item => item.About)
			.NotNull();
		//RuleFor(item => item.Count)
		//	.GreaterThan(0);
		RuleFor(item => item.FirstId)
			.GreaterThan(0);
	}

	#endregion
}
