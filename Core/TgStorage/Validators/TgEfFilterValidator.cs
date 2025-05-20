// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Validators;

/// <summary> Filter validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfFilterValidator : TgEfValidatorBase<TgEfFilterEntity>
{
	#region Public and private fields, properties, constructor

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