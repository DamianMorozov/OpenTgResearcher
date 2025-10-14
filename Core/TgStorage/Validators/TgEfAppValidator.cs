namespace TgStorage.Validators;

/// <summary> App validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfAppValidator : TgEfValidatorBase<TgEfAppEntity>
{
	#region Fields, properties, constructor

	public TgEfAppValidator()
	{
		RuleFor(item => item.ApiHash)
            .NotEmpty()
			.NotNull();
		RuleFor(item => item.ApiId)
            .NotEmpty()
            .NotNull();
		RuleFor(item => item.PhoneNumber)
			.NotEmpty()
			.NotNull();
	}

	#endregion
}
