namespace TgStorage.Validators;

/// <summary> Filter validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfFilterValidator : TgEfValidatorBase<TgEfFilterEntity>
{
	#region Fields, properties, constructor

	public TgEfFilterValidator()
	{
		RuleFor(item => item.FilterType)
			.NotEmpty()
			.NotNull();
		RuleFor(item => item.Name)
			.NotEmpty()
			.NotNull();
		RuleFor(item => item.Mask)
			.NotNull();
		RuleFor(item => item.Size)
			.NotNull();
		RuleFor(item => item.SizeType)
			.NotNull();
	}

	#endregion
}
