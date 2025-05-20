// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Validators;

/// <summary> Source validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfSourceValidator : TgEfValidatorBase<TgEfSourceEntity>
{
	#region Public and private fields, properties, constructor

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