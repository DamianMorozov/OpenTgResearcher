namespace OpenTgResearcherDesktop.Views;

// Вместо автоматически сгенерированного XAML подключаем только необходимые странице элементы
public sealed partial class TgSplashScreenPage : SplashPageBase
{
    public FrameworkElement MainContent { get; } = new();
    public FrameworkElement SplashOrbit { get; } = new();
    public FrameworkElement SplashScanBand { get; } = new();
    public FrameworkElement SplashContinueButton { get; } = new();
    public FrameworkElement SplashLoadingRows { get; } = new();
    public FrameworkElement SplashContinueHost { get; } = new() { ActualWidth = 500, ActualHeight = 112 };
    public FrameworkElement ContinueOrbitArrow { get; } = new() { ActualWidth = 14, ActualHeight = 14 };
    public FrameworkElement ContinueOrbitStar1 { get; } = new() { ActualWidth = 10, ActualHeight = 10 };
    public FrameworkElement ContinueOrbitStar2 { get; } = new() { ActualWidth = 8, ActualHeight = 8 };
    public FrameworkElement ContinueOrbitStar3 { get; } = new() { ActualWidth = 6, ActualHeight = 6 };
    public FrameworkElement ContinueOrbitStar4 { get; } = new() { ActualWidth = 5, ActualHeight = 5 };
    public FrameworkElement ContinueOrbitStar5 { get; } = new() { ActualWidth = 4, ActualHeight = 4 };
    public SplashBindings Bindings { get; } = new();
    // Создание элементов выполнено в тестовой части страницы
    private void InitializeComponent() { }
}
