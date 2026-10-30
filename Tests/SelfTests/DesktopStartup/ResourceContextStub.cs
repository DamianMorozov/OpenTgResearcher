namespace Windows.ApplicationModel.Resources.Core;

public sealed class ResourceContext
{
    public static int AccessCalls { get; set; }
    public string[] Languages { get; set; } = [];
    /// <summary> Обнаруживает обращение к UWP API, аварийно завершающему установленное приложение без MSIX </summary>
    public static ResourceContext GetForViewIndependentUse()
    {
        AccessCalls++;
        throw new InvalidOperationException("UWP ResourceContext недоступен приложению без MSIX");
    }
}
