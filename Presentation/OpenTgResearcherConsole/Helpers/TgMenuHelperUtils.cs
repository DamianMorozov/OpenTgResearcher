namespace OpenTgResearcherConsole.Helpers;

internal sealed partial class TgMenuHelper
{
    #region Methods

    /// <summary> Open web-site </summary>
    private static async Task WebSiteOpenAsync(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            TgDebugUtils.WriteExceptionToDebug(ex);
            AnsiConsole.WriteLine($"  Opening error URL: {ex.Message}");
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary> Catch exception, show message and write to debug </summary>
    public static void CatchException(Exception ex, string message = "",
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        if (!string.IsNullOrEmpty(message))
            TgLog.WriteLine($"  {TgLog.GetMarkupString(message)}");
        TgLog.WriteLine($"  {TgLocale.StatusException}: " + TgLog.GetMarkupString(ex.Message));
        if (ex.InnerException is not null)
            TgLog.WriteLine($"  {TgLocale.StatusInnerException}: " + TgLog.GetMarkupString(ex.InnerException.Message));

        TgDebugUtils.WriteExceptionToDebug(ex, message, filePath, lineNumber, memberName);
        TgLog.TypeAnyKeyForReturn();
    }

    #endregion
}
