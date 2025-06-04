// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
// ReSharper disable InconsistentNaming

namespace OpenTgResearcherConsole.Helpers;

internal sealed partial class TgMenuHelper
{
    public void CatchException(Exception ex, string message = "",
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        if (!string.IsNullOrEmpty(message))
            TgLog.MarkupLine($"  {TgLog.GetMarkupString(message)}");
        TgLog.MarkupLine($"  {TgLocale.StatusException}: " + TgLog.GetMarkupString(ex.Message));
        if (ex.InnerException is not null)
            TgLog.MarkupLine($"  {TgLocale.StatusInnerException}: " + TgLog.GetMarkupString(ex.InnerException.Message));

        TgDebugUtils.WriteExceptionToDebug(ex, message, filePath, lineNumber, memberName);
        TgLog.TypeAnyKeyForReturn();
    }
}
