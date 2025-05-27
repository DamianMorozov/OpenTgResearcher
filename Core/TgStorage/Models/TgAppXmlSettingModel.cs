// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgStorage.Models;

/// <summary> App xml setting </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgAppXmlSettingModel(string name, string value) : ITgDebug
{
	#region Public and private fields, properties, constructor

	public string Name { get; set; } = name;
	public string Value { get; set; } = value;

	public TgAppXmlSettingModel() : this(string.Empty, string.Empty) { }

	#endregion

	#region Public and private methods

	public override string ToString() => $"{Name} | {Value}";

	public string ToDebugString() => ToString();

	#endregion
}