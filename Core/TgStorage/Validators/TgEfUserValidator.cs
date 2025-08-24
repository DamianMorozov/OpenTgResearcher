// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Validators;

/// <summary> User validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfUserValidator : TgEfValidatorBase<TgEfUserEntity>
{
	#region Fields, properties, constructor

	public TgEfUserValidator()
	{
		//
	}

	#endregion
}