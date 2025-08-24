// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Validators;

/// <summary> Message validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfMessageRelationValidator : TgEfValidatorBase<TgEfMessageRelationEntity>
{
	#region Fields, properties, constructor

	public TgEfMessageRelationValidator()
	{
		RuleFor(item => item.ChildMessageId)
			.NotNull()
			.GreaterThanOrEqualTo(0);
		RuleFor(item => item.ChildSourceId)
            .NotNull()
            .GreaterThanOrEqualTo(0);
		RuleFor(item => item.ParentMessageId)
			.NotNull()
			.GreaterThanOrEqualTo(0);
		RuleFor(item => item.ParentSourceId)
            .NotNull()
            .GreaterThanOrEqualTo(0);
	}

	#endregion
}