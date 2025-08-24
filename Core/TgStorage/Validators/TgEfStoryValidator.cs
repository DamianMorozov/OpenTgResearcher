// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Validators;

/// <summary> Story validator </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgEfStoryValidator : TgEfValidatorBase<TgEfStoryEntity>
{
	#region Fields, properties, constructor

	public TgEfStoryValidator()
	{
		//
	}

	#endregion
}