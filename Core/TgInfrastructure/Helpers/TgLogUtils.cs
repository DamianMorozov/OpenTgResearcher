// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgInfrastructure.Helpers;

/// <summary> Log utils </summary>
public static class TgLogUtils
{
    #region Public and private fields, properties, constructor

    private static string _startupLog = string.Empty;

    #endregion

    #region Public and private methods

    public static string GetAppDirectory(TgEnumAppType appType) => appType switch
    {
        TgEnumAppType.Console => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), TgConstants.OpenTgResearcherConsole)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
        TgEnumAppType.Desktop => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), TgConstants.OpenTgResearcherDesktop)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
        TgEnumAppType.Blazor => AppDomain.CurrentDomain.BaseDirectory
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
        _ => string.Empty,
    };

    public static string GetLogsDirectory(TgEnumAppType appType) => appType switch
    {
        TgEnumAppType.Console => Path.Combine(GetAppDirectory(appType), "current")
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
        TgEnumAppType.Desktop => Path.Combine(GetAppDirectory(appType), "current")
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
        TgEnumAppType.Blazor => Path.Combine(GetAppDirectory(appType), "Logs")
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
        _ => string.Empty,
    };

    /// <summary> Create log </summary>
    public static void Create(TgEnumAppType appType, bool isAppStart = false)
	{
		try
		{
            switch (appType)
            {
                case TgEnumAppType.Console:
                case TgEnumAppType.Desktop:
                case TgEnumAppType.Blazor:
                    _startupLog = Path.Combine(GetLogsDirectory(appType), $"Log-{DateTime.Now:yyyy-MM-dd}.txt");
                    break;
            }
            if (string.IsNullOrEmpty(_startupLog)) return;

            // Check directory
            if (!Directory.Exists(Path.GetDirectoryName(_startupLog)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(_startupLog)!);
            }

            // Check rewrite flag
            if (isAppStart)
            {
                if (!File.Exists(_startupLog))
                    File.CreateText(_startupLog).Close();
                WriteLog($"App started");
            }
        }
        catch (Exception ex)
		{
            WriteExceptionWithMessage(ex, "Failed to start app log!");
		}
    }

    public static async Task CloseAndFlushAsync()
    {
        if (string.IsNullOrEmpty(_startupLog) || !File.Exists(_startupLog)) return;
        try
        {
            // Flush the log file
            using var stream = new FileStream(_startupLog, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            await writer.WriteLineAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] App closed");
        }
        catch (Exception ex)
        {
            WriteExceptionWithMessage(ex, "Failed to close and flush app log!");
        }
    }

    public static void CloseAndFlush()
    {
        if (string.IsNullOrEmpty(_startupLog) || !File.Exists(_startupLog)) return;
        try
        {
            // Flush the log file
            using var stream = new FileStream(_startupLog, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] App closed");
        }
        catch (Exception ex)
        {
            WriteExceptionWithMessage(ex, "Failed to close and flush app log!");
        }
    }

    private static void WriteCallerCore(string filePath, int lineNumber, string memberName)
    {
        var fileName = TgFileUtils.GetShortFilePath(filePath);
        File.AppendAllText(_startupLog, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Location: {fileName} file, {memberName} method, {lineNumber} line{Environment.NewLine}");
    }

    private static void WriteCallerExceptionCore(string filePath, int lineNumber, string memberName)
    {
        WriteCallerCore(filePath, lineNumber, memberName);
        File.AppendAllText(_startupLog, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Exception: {Environment.NewLine}");
        File.AppendAllText(_startupLog, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] StackTrace: {Environment.NewLine}");
    }

    public static void WriteLog(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        File.AppendAllText(_startupLog, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
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
        WriteCallerExceptionCore(filePath, lineNumber, memberName);
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
