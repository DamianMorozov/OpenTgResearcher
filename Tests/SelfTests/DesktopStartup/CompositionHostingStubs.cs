namespace Microsoft.UI.Xaml.Hosting;

public static class ElementCompositionPreview
{
    /// <summary> Возвращает единственный визуальный слой соответствующего элемента </summary>
    public static Visual GetElementVisual(FrameworkElement element) => element.Visual;
}
