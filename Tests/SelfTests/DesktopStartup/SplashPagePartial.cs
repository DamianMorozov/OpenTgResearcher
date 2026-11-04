namespace OpenTgResearcherDesktop.Views;

// Вместо автоматически сгенерированного XAML подключаем только необходимые странице элементы
public sealed partial class TgSplashScreenPage : SplashPageBase
{
    public FrameworkElement MainContent { get; } = new();
    public FrameworkElement SplashOrbit { get; } = new();
    public FrameworkElement SplashScanBand { get; } = new();
    public FrameworkElement SplashContinueButton { get; } = new();
    public FrameworkElement SplashLoadingRows { get; } = new();
    public FrameworkElement SplashContinueHost { get; } = new() { ActualWidth = 500, ActualHeight = 66 };
    public FrameworkElement ContinueOrbitHead { get; } = new() { ActualWidth = 14, ActualHeight = 14 };
    public FrameworkElement ContinueOrbitTailNear { get; } = new() { ActualWidth = 10, ActualHeight = 10 };
    public FrameworkElement ContinueOrbitTailMiddle { get; } = new() { ActualWidth = 8, ActualHeight = 8 };
    public FrameworkElement ContinueOrbitTailFar { get; } = new() { ActualWidth = 6, ActualHeight = 6 };
    public SplashBindings Bindings { get; } = new();
    // Создание элементов выполнено в тестовой части страницы
    private void InitializeComponent() { }
}
