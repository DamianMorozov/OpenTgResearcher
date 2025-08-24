// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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