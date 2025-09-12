namespace TgInfrastructure.Helpers;

/// <summary> Debug utilities for logging exceptions and debug information </summary>
public static class TgDebugUtils
{
    public static void WriteExceptionToDebug(Exception ex, string message, string filePath, int lineNumber, string memberName)
    {
#if DEBUG
        // Debug output
        if (!string.IsNullOrEmpty(message))
            Debug.WriteLine(message);
        Debug.WriteLine($"Exception: {ex.Message}");
        if (ex.InnerException is not null)
            Debug.WriteLine($"InnerException: {ex.InnerException.Message}");

        // File path and line number
        Debug.WriteLine($"{Environment.NewLine}");
        var fileName = TgFileUtils.GetShortFilePath(filePath);
        Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Location: {fileName} file, {memberName} method, {lineNumber} line{Environment.NewLine}");
        Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Exception: {Environment.NewLine}");

        // Stack trace
        Debug.WriteLine($"{Environment.NewLine}");
        Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] StackTrace: {Environment.NewLine}");
        Debug.WriteLine($"{ex.StackTrace}{Environment.NewLine}");
#endif
    }

    public static void WriteExceptionToDebug(Exception ex, 
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
#if DEBUG
        // Debug output
        Debug.WriteLine($"Exception: {ex.Message}");
        if (ex.InnerException is not null)
            Debug.WriteLine($"InnerException: {ex.InnerException.Message}");

        // File path and line number
        Debug.WriteLine($"{Environment.NewLine}");
        var fileName = TgFileUtils.GetShortFilePath(filePath);
        Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Location: {fileName} file, {memberName} method, {lineNumber} line{Environment.NewLine}");
        Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Exception: {Environment.NewLine}");

        // Stack trace
        Debug.WriteLine($"{Environment.NewLine}");
        Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] StackTrace: {Environment.NewLine}");
        Debug.WriteLine($"{ex.StackTrace}{Environment.NewLine}");
#endif
    }
}
