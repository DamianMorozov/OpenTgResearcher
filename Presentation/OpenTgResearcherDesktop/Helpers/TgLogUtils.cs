// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using System.Runtime.CompilerServices;
using TgFileUtils = TgInfrastructure.Utils.TgFileUtils;

namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Log utils </summary>
public static class TgLogUtils
{
    #region Public and private fields, properties, constructor

    private static readonly string _startupLog = 
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), TgConstants.OpenTgResearcherDesktop, "current", 
			$"{TgConstants.OpenTgResearcherDesktop}-StartupLog.txt");

	#endregion

	#region Public and private methods

	public static void LogFatal(Exception ex, string message = "", 
		[CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
	{
#if DEBUG
		if (!string.IsNullOrEmpty(message))
			Debug.WriteLine(ex, $"{message} exception!");
		Debug.WriteLine($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
		Debug.WriteLine(ex);
		Debug.WriteLine(ex.StackTrace);
#endif
		try
		{
			if (!string.IsNullOrEmpty(message))
				Log.Fatal(ex, $"{message} exception!");
			Log.Fatal(ex, $"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
		}
		catch (Exception)
		{
			//
		}
	}

	public static void LogFatalProxy(Exception ex, string message, string filePath, int lineNumber, string memberName)
	{
#if DEBUG
		if (!string.IsNullOrEmpty(message))
			Debug.WriteLine(ex, $"{message} exception!");
		Debug.WriteLine($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
		Debug.WriteLine(ex);
		Debug.WriteLine(ex.StackTrace);
#endif
		try
		{
			if (!string.IsNullOrEmpty(message))
				Log.Fatal(ex, $"{message} exception!");
			Log.Fatal(ex, $"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
		}
		catch (Exception)
		{
			//
		}
	}

	public static void LogFatal(string message = "", 
		[CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
	{
#if DEBUG
		if (!string.IsNullOrEmpty(message))
			Debug.WriteLine($"{message} exception!");
		Debug.WriteLine($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
#endif
		try
		{
			if (!string.IsNullOrEmpty(message))
				Log.Fatal($"{message} exception!");
			Log.Fatal($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
		}
		catch (Exception)
		{
			//
		}
	}

	public static void LogFatalProxy(string message, string filePath, int lineNumber, string memberName)
	{
#if DEBUG
		if (!string.IsNullOrEmpty(message))
			Debug.WriteLine($"{message} exception!");
		Debug.WriteLine($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
#endif
		try
		{
			if (!string.IsNullOrEmpty(message))
				Log.Fatal($"{message} exception!");
			Log.Fatal($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
		}
		catch (Exception)
		{
			//
		}
	}

	public static void LogInformation(string message,
		[CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
	{
#if DEBUG
		if (!string.IsNullOrEmpty(message))
			Debug.WriteLine(message);
		Debug.WriteLine($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
#endif
		try
		{
			if (!string.IsNullOrEmpty(message))
				Log.Information($"{message}");
			Log.Information($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
		}
		catch (Exception)
		{
			//
		}
	}

	public static void LogInformationProxy(string message, string filePath, int lineNumber, string memberName)
	{
#if DEBUG
		if (!string.IsNullOrEmpty(message))
			Debug.WriteLine(message);
		Debug.WriteLine($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
#endif
		try
		{
			if (!string.IsNullOrEmpty(message))
				Log.Information($"{message}");
			Log.Information($"{TgFileUtils.GetShortFilePath(filePath)} | {lineNumber} | {memberName}");
		}
		catch (Exception)
		{
			//
		}
	}

	/// <summary> Initialize startup log </summary>
	public static void InitStartupLog()
	{
		try
		{
			if (!string.IsNullOrEmpty(_startupLog) && !Directory.Exists(Path.GetDirectoryName(_startupLog)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(_startupLog)!);
            }
            if (File.Exists(_startupLog))
                File.Delete(_startupLog);
        }
        catch (Exception ex)
		{
			LogFatal(ex, "Initialize startup log failed!");
		}
    }

	public static void WriteToLog(string message)
	{
		File.AppendAllText(_startupLog, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
    }

	public static void WriteException(Exception ex,
		[CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
	{
        var message = ex.Message;
        if (ex.InnerException is not null)
            message += Environment.NewLine + ex.InnerException.Message;
		var fileName = TgFileUtils.GetShortFilePath(filePath);
        File.AppendAllText(_startupLog, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Location: {fileName} file, {memberName} method, {lineNumber} line{Environment.NewLine}");
        File.AppendAllText(_startupLog, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Exception: {Environment.NewLine}");
        File.AppendAllText(_startupLog, $"{message}{Environment.NewLine}");
        File.AppendAllText(_startupLog, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] StackTrace: {Environment.NewLine}");
        File.AppendAllText(_startupLog, $"{ex.StackTrace}{Environment.NewLine}");
    }

    #endregion
}
