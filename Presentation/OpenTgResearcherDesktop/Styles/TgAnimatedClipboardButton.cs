namespace OpenTgResearcherDesktop.Styles;

public sealed partial class TgAnimatedClipboardButton : Button
{
    public Symbol NormalSymbol
    {
        get => (Symbol)GetValue(NormalSymbolProperty);
        set => SetValue(NormalSymbolProperty, value);
    }
    public static readonly DependencyProperty NormalSymbolProperty =
        DependencyProperty.Register(nameof(NormalSymbol), typeof(Symbol), typeof(TgAnimatedClipboardButton),
            new PropertyMetadata(Symbol.Copy));

    public Symbol CheckedSymbol
    {
        get => (Symbol)GetValue(CheckedSymbolProperty);
        set => SetValue(CheckedSymbolProperty, value);
    }
    public static readonly DependencyProperty CheckedSymbolProperty =
        DependencyProperty.Register(nameof(CheckedSymbol), typeof(Symbol), typeof(TgAnimatedClipboardButton),
            new PropertyMetadata(Symbol.Accept));

    public double ResetDelay
    {
        get => (double)GetValue(ResetDelayProperty);
        set => SetValue(ResetDelayProperty, value);
    }
    public static readonly DependencyProperty ResetDelayProperty =
        DependencyProperty.Register(nameof(ResetDelay), typeof(double), typeof(TgAnimatedClipboardButton),
            new PropertyMetadata(2.0));

    public TgAnimatedClipboardButton()
    {
        DefaultStyleKey = typeof(TgAnimatedClipboardButton);
    }
}
