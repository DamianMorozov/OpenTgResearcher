// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Helpers;

/// <summary> Log helper </summary>
[DebuggerDisplay("{ToDebugString()}")]
public sealed class TgLogHelper : ITgHelper
{
	#region Design pattern "Lazy Singleton"

	private static TgLogHelper _instance = null!;
	public static TgLogHelper Instance => LazyInitializer.EnsureInitialized(ref _instance);

	#endregion

	#region Public and private fields, properties, constructor

	private Action<string> _markupLine = _ => { };
	private Action<string> _markupLineStamp = _ => { };

    #endregion

	#region Public and private methods

    public string ToDebugString() => "";

	public void WriteLine(string message) => _markupLine(message);

	public void MarkupLine(string message) => _markupLineStamp(GetLineStamp(message));

	public void MarkupInfo(string message) => _markupLineStamp(GetLineStampInfo(message));

	public void MarkupWarning(string message) => _markupLineStamp(GetLineStampWarning(message));

	public void SetMarkupLine(Action<string> markupLine) => _markupLine = markupLine;

	public void SetMarkupLineStamp(Action<string> markupLineStamp) => _markupLineStamp = markupLineStamp;

	public string GetMarkupString(string message, bool isReplaceSpec = false) => 
		string.IsNullOrEmpty(message) ? string.Empty : isReplaceSpec
			? message
				.Replace("[", "[[").Replace("]", "]]")
				.Replace("'", "").Replace("/", "")
			: message
				.Replace("[", "[[").Replace("]", "]]")
				.Replace("'", "");

	public string GetDtStamp() => $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

	public string GetDtShortStamp() => $"{DateTime.Now:HH:mm:ss}";

	public string GetLineStamp(string message) => $" {GetDtStamp()} | {GetMarkupString(message)}";

	public string GetLineStampInfo(string message)
	{
		message = message.Replace("[", "[[").Replace("]", "]]");
		message = message.Replace("'", "");
		return $"[green] {GetDtStamp()} | i {message}[/]";
	}

	public string GetLineStampWarning(string message)
	{
		message = message.Replace("[", "[[").Replace("]", "]]");
		message = message.Replace("'", "");
		return $"[red] {GetDtStamp()} | x {message}[/]";
	}

    public void TypeAnyKeyForReturn()
    {
        WriteLine($" {TgLocaleHelper.Instance.TypeAnyKeyForReturn}");
        Console.ReadKey();
    }

    #endregion
}