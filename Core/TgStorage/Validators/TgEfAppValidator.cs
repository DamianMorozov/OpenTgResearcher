// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Validators;

/// <summary> App validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfAppValidator : TgEfValidatorBase<TgEfAppEntity>
{
	#region Fields, properties, constructor

	public TgEfAppValidator()
	{
		RuleFor(item => item.ApiHash)
			.NotNull();
		RuleFor(item => item.ApiId)
			.NotNull();
		RuleFor(item => item.PhoneNumber)
			.NotEmpty()
			.NotNull();
	}

	#endregion
}