namespace TgStorage.Validators;

/// <summary> Document validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfDocumentValidator : TgEfValidatorBase<TgEfDocumentEntity>
{
	#region Fields, properties, constructor

	public TgEfDocumentValidator()
	{
		RuleFor(item => item.Id)
			.NotNull()
			.GreaterThanOrEqualTo(0);
		RuleFor(item => item.MessageId)
			.NotNull()
			.GreaterThanOrEqualTo(0);
		RuleFor(item => item.FileName)
			.NotNull();
		RuleFor(item => item.FileSize)
			.NotNull()
			.GreaterThanOrEqualTo(0);
		RuleFor(item => item.AccessHash)
			.NotNull();
	}

	#endregion
}
