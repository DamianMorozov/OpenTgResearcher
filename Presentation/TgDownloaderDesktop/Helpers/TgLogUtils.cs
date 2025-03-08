// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TgFileUtils = TgInfrastructure.Utils.TgFileUtils;

namespace TgDownloaderDesktop.Helpers;

/// <summary> Log utils </summary>
public static class TgLogUtils
{
	#region Public and private methods

	public static void LogFatal(Exception ex, string message = "", 
		[CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
	{
		if (!string.IsNullOrEmpty(message))
			Log.Fatal(ex, $"{message} exception!");
		Log.Fatal(ex, $"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
#if DEBUG
		if (!string.IsNullOrEmpty(message))
			Debug.WriteLine(ex, $"{message} exception!");
		Debug.WriteLine($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
		Debug.WriteLine(ex);
		Debug.WriteLine(ex.StackTrace);
#endif
	}

	public static void LogFatalProxy(Exception ex, string message, string filePath, int lineNumber, string memberName)
	{
		if (!string.IsNullOrEmpty(message))
			Log.Fatal(ex, $"{message} exception!");
		Log.Fatal(ex, $"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
#if DEBUG
		if (!string.IsNullOrEmpty(message))
			Debug.WriteLine(ex, $"{message} exception!");
		Debug.WriteLine($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
		Debug.WriteLine(ex);
		Debug.WriteLine(ex.StackTrace);
#endif
	}

	public static void LogFatal(string message = "", 
		[CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
	{
		if (!string.IsNullOrEmpty(message))
			Log.Fatal($"{message} exception!");
		Log.Fatal($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
#if DEBUG
		if (!string.IsNullOrEmpty(message))
			Debug.WriteLine($"{message} exception!");
		Debug.WriteLine($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
#endif
	}

	public static void LogFatalProxy(string message, string filePath, int lineNumber, string memberName)
	{
		if (!string.IsNullOrEmpty(message))
			Log.Fatal($"{message} exception!");
		Log.Fatal($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
#if DEBUG
		if (!string.IsNullOrEmpty(message))
			Debug.WriteLine($"{message} exception!");
		Debug.WriteLine($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
#endif
	}

	public static void LogInformation(string message,
		[CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
	{
		if (!string.IsNullOrEmpty(message))
			Log.Information($"{message}");
		Log.Information($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
#if DEBUG
		if (!string.IsNullOrEmpty(message))
			Debug.WriteLine(message);
		Debug.WriteLine($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
#endif
	}

	public static void LogInformationProxy(string message, string filePath, int lineNumber, string memberName)
	{
		if (!string.IsNullOrEmpty(message))
			Log.Information($"{message}");
		Log.Information($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
#if DEBUG
		if (!string.IsNullOrEmpty(message))
			Debug.WriteLine(message);
		Debug.WriteLine($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
#endif
	}

	#endregion
}
