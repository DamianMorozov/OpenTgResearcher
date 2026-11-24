namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Показывает поверхность медиа поверх клиентской области приложения </summary>
internal sealed class TgWindowMediaPopup
{
    private readonly Border _surface;
    private readonly Microsoft.UI.Xaml.Controls.Primitives.Popup _popup;

    public TgWindowMediaPopup(XamlRoot root, FrameworkElement media)
    {
        _surface = new() { Background = new SolidColorBrush(Microsoft.UI.Colors.Black), Child = media };
        _popup = new() { XamlRoot = root, Child = _surface };
        Resize(root, null);
        root.Changed += Resize;
        _popup.IsOpen = true;
    }

    /// <summary> Закрывает просмотр и освобождает поверхность для возврата в карточку </summary>
    public void Close()
    {
        if (_popup.XamlRoot is { } root) root.Changed -= Resize;
        _popup.IsOpen = false;
        _surface.Child = null;
        _popup.Child = null;
    }

    /// <summary> Проверяет, что нажатие относится к кнопке или ползунку проигрывателя </summary>
    public static bool IsPlayerControl(TappedRoutedEventArgs e, FrameworkElement media)
    {
        for (var element = e.OriginalSource as DependencyObject; element is not null && element != media; element = VisualTreeHelper.GetParent(element))
            if (element is Microsoft.UI.Xaml.Controls.Primitives.ButtonBase or Slider) return true;
        return false;
    }

    // Подгоняем подложку под клиентскую область при изменении размеров окна
    private void Resize(XamlRoot root, XamlRootChangedEventArgs? args)
    {
        _surface.Width = root.Size.Width;
        _surface.Height = root.Size.Height;
    }
}
