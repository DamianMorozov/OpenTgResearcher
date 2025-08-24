// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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