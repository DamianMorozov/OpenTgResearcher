namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Сканирование верхнего логотипа во время общих сетевых операций </summary>
internal sealed class TgNetworkAnimationHelper
{
    private readonly FrameworkElement _scannerElement;
    private readonly Visual _scanner;
    private readonly ILoadStateService _state;
    private readonly Action<Action> _dispatch;
    private readonly Func<bool> _animationsEnabled;
    private bool _isAttached;
    private bool _isAnimating;

    /// <summary> Использует общее состояние подключения, парсинга и скачивания </summary>
    internal TgNetworkAnimationHelper(FrameworkElement scanner, ILoadStateService state, Action<Action> dispatch, Func<bool> animationsEnabled)
    {
        _scannerElement = scanner;
        _scanner = ElementCompositionPreview.GetElementVisual(scanner);
        _state = state;
        _dispatch = dispatch;
        _animationsEnabled = animationsEnabled;
    }

    /// <summary> Подписывается при появлении оболочки и учитывает уже выполняющуюся загрузку </summary>
    internal void Start()
    {
        if (_isAttached) return;
        _isAttached = true;
        _state.PropertyChanged += OnStateChanged;
        Refresh();
    }

    /// <summary> Останавливает эффект и освобождает подписку при уходе или закрытии </summary>
    internal void Stop()
    {
        _isAttached = false;
        _state.PropertyChanged -= OnStateChanged;
        StopScan();
    }

    // События фоновых загрузчиков переводим в поток интерфейса
    private void OnStateChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName is nameof(ILoadStateService.IsOnlineProcessing) or nameof(ILoadStateService.IsDownloadProcessing))
            _dispatch(Refresh);
    }

    // Параллельные операции не перезапускают полосу и не гасят продолжающееся скачивание
    private void Refresh()
    {
        if (!_isAttached) return;
        if ((!_state.IsOnlineProcessing && !_state.IsDownloadProcessing) || !_animationsEnabled()) { StopScan(); return; }
        if (_isAnimating) return;
        _isAnimating = true;
        _scannerElement.Opacity = 1;
        var scan = _scanner.Compositor.CreateScalarKeyFrameAnimation();
        scan.Duration = TimeSpan.FromSeconds(2.4);
        scan.IterationBehavior = AnimationIterationBehavior.Forever;
        scan.InsertKeyFrame(0, -(float)_scannerElement.ActualHeight);
        scan.InsertKeyFrame(0.8f, (float)_scannerElement.ActualWidth, _scanner.Compositor.CreateLinearEasingFunction());
        scan.InsertKeyFrame(1, (float)_scannerElement.ActualWidth);
        _scanner.StartAnimation("Offset.Y", scan);
        var glow = _scanner.Compositor.CreateScalarKeyFrameAnimation();
        glow.Duration = scan.Duration;
        glow.IterationBehavior = AnimationIterationBehavior.Forever;
        glow.InsertKeyFrame(0, 0);
        glow.InsertKeyFrame(0.1f, 0.7f);
        glow.InsertKeyFrame(0.7f, 0.7f);
        glow.InsertKeyFrame(0.85f, 0);
        glow.InsertKeyFrame(1, 0);
        _scanner.StartAnimation("Opacity", glow);
    }

    // Убираем и Composition, и XAML слой, сохраняя неподвижный знак OTR
    private void StopScan()
    {
        _isAnimating = false;
        _scanner.StopAnimation("Offset.Y");
        _scanner.StopAnimation("Opacity");
        _scanner.Opacity = 0;
        _scanner.Offset = System.Numerics.Vector3.Zero;
        _scannerElement.Opacity = 0;
    }
}
