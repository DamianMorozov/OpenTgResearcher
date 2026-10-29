namespace Windows.ApplicationModel.Resources.Core;

public sealed class ResourceContext
{
    public string[] Languages { get; set; } = [];
    /// <summary> Возвращает контекст ресурсов без обращения к платформе Windows </summary>
    public static ResourceContext GetForViewIndependentUse() => new();
}
