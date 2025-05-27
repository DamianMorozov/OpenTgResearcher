// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Contracts;

/// <summary> Debug </summary>
public interface ITgDebug
{
	#region Public and private methods

	public string ToDebugString();

	#endregion
}