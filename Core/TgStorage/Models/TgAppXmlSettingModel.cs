namespace TgStorage.Models;

/// <summary> App xml setting </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgAppXmlSettingModel(string name, string value) : ITgDebug
{
	#region Fields, properties, constructor

	public string Name { get; set; } = name;
	public string Value { get; set; } = value;

	public TgAppXmlSettingModel() : this(string.Empty, string.Empty) { }

	#endregion

	#region Methods

	public override string ToString() => $"{Name} | {Value}";

	public string ToDebugString() => ToString();

	#endregion
}
