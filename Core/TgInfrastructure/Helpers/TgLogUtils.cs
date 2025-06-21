// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Helpers;

/// <summary> Log utils </summary>
public static class TgLogUtils
{
    #region Public and private fields, properties, constructor

    private static string _startupLog = default!;

	#endregion

	#region Public and private methods

	/// <summary> Initialize startup log </summary>
	public static void InitStartupLog(string appName, bool isWebApp, bool isRewrite)
	{
		try
		{
            _startupLog = !isWebApp
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    appName, "current", $"{appName}-StartupLog.txt")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"{appName}-StartupLog.txt");

            if (!string.IsNullOrEmpty(_startupLog) && !Directory.Exists(Path.GetDirectoryName(_startupLog)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(_startupLog)!);
            }

            // Check rewrite flag
            if (isRewrite && File.Exists(_startupLog))
                File.Delete(_startupLog);
            
            WriteLog($"App started");
        }
        catch (Exception ex)
		{
            WriteExceptionWithMessage(ex, "App startup log failed!");
		}
    }

    private static void WriteCallerCore(string filePath, int lineNumber, string memberName)
    {
        var fileName = TgFileUtils.GetShortFilePath(filePath);
        File.AppendAllText(_startupLog, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Location: {fileName} file, {memberName} method, {lineNumber} line{Environment.NewLine}");
        File.AppendAllText(_startupLog, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Exception: {Environment.NewLine}");
        File.AppendAllText(_startupLog, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] StackTrace: {Environment.NewLine}");
    }

    public static void WriteLog(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        File.AppendAllText(_startupLog, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
    }

    public static void WriteLog(string message,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        WriteLog(message);
        WriteCallerCore(filePath, lineNumber, memberName);
    }

    public static void WriteLogWithCaller(string message,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        WriteLog(message);
        WriteCallerCore(filePath, lineNumber, memberName);
    }

    private static void WriteExceptionCore(Exception ex, string filePath, int lineNumber, string memberName)
    {
        var message = ex.Message;
        if (ex.InnerException is not null)
            message += Environment.NewLine + ex.InnerException.Message;
        WriteLog(message);
        WriteCallerCore(filePath, lineNumber, memberName);
        WriteLog(ex.StackTrace?.ToString() ?? string.Empty);

        TgDebugUtils.WriteExceptionToDebug(ex, message, filePath, lineNumber, memberName);
    }

    public static void WriteException(Exception ex,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") => 
        WriteExceptionCore(ex, filePath, lineNumber, memberName);

    public static void WriteExceptionWithMessage(Exception ex, string message = "",
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        WriteLog(message);
        WriteExceptionCore(ex, filePath, lineNumber, memberName);
    }

    #endregion
}
