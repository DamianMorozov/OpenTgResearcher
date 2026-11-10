namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Анимация заставки без ожидания инициализации приложения на потоке интерфейса </summary>
internal sealed class TgSplashAnimationHelper
{
    private const int _introDurationMilliseconds = 2000;
    private const int _scanDurationMilliseconds = 840;
    private readonly Visual _content;
    private readonly Visual _orbit;
    private readonly Visual _scanner;
    private readonly Visual _continueButton;
    private readonly Visual _rows;
    private readonly FrameworkElement _contentElement;
    private readonly FrameworkElement _orbitElement;
    private readonly FrameworkElement _scannerElement;
    private CompositionScopedBatch? _appearanceBatch;
    private CompositionScopedBatch? _scanBatch;
    private TaskCompletionSource _appearanceCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary> Получает визуальные слои заставки, не изменяя разметку остальных страниц </summary>
    internal TgSplashAnimationHelper(FrameworkElement content, FrameworkElement orbit, FrameworkElement scanner, FrameworkElement continueButton,
        FrameworkElement rows)
    {
        _contentElement = content;
        _orbitElement = orbit;
        _scannerElement = scanner;
        _content = ElementCompositionPreview.GetElementVisual(content);
        _orbit = ElementCompositionPreview.GetElementVisual(orbit);
        _scanner = ElementCompositionPreview.GetElementVisual(scanner);
        _continueButton = ElementCompositionPreview.GetElementVisual(continueButton);
        _rows = ElementCompositionPreview.GetElementVisual(rows);
    }

    /// <summary> Выполняет появление и орбиту параллельно за две секунды, затем запускает сканирование </summary>
    internal void Start(bool isLoading, bool animationsEnabled)
    {
        StopAnimations();
        if (_appearanceCompleted.Task.IsCompleted) _appearanceCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!animationsEnabled || !isLoading) { _appearanceCompleted.TrySetResult(); return; }

        var compositor = _content.Compositor;
        _appearanceBatch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
        _appearanceBatch.Completed += OnAppearanceCompleted;
        // До первого кадра размер Composition может быть нулевым, поэтому используем уже измеренную разметку
        _content.CenterPoint = new((float)_contentElement.ActualWidth / 2, (float)_contentElement.ActualHeight / 2, 0);
        StartFade(_content, _introDurationMilliseconds);
        var scale = compositor.CreateVector3KeyFrameAnimation();
        scale.Duration = TimeSpan.FromMilliseconds(_introDurationMilliseconds);
        scale.InsertKeyFrame(0, new(0.97f, 0.97f, 1));
        scale.InsertKeyFrame(1, System.Numerics.Vector3.One, CreateEaseOut(compositor));
        _content.StartAnimation("Scale", scale);

        // Ожидаем фактического окончания появления строк, не задерживая параллельную инициализацию приложения
        var rowsFade = compositor.CreateScalarKeyFrameAnimation();
        rowsFade.Duration = TimeSpan.FromMilliseconds(_introDurationMilliseconds);
        rowsFade.InsertKeyFrame(0, 0);
        rowsFade.InsertKeyFrame(1, 1, compositor.CreateLinearEasingFunction());
        _rows.StartAnimation("Opacity", rowsFade);

        _orbit.Opacity = 1;
        _orbit.CenterPoint = new((float)_orbitElement.ActualWidth / 2, (float)_orbitElement.ActualHeight / 2, 0);
        var rotation = compositor.CreateScalarKeyFrameAnimation();
        rotation.Duration = TimeSpan.FromMilliseconds(_introDurationMilliseconds);
        rotation.InsertKeyFrame(0, 0);
        rotation.InsertKeyFrame(1, 360, compositor.CreateLinearEasingFunction());
        _orbit.StartAnimation("RotationAngleInDegrees", rotation);
        _appearanceBatch.End();
    }

    // Выполняем один проход сканирования после завершения обоих вводных эффектов
    private void StartScan()
    {
        var compositor = _scanner.Compositor;
        _scanBatch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
        _scanBatch.Completed += OnScanCompleted;
        // Полоса проходит квадратную область логотипа и возвращается только за её границами
        var scan = compositor.CreateScalarKeyFrameAnimation();
        scan.Duration = TimeSpan.FromMilliseconds(_scanDurationMilliseconds);
        scan.InsertKeyFrame(0, -(float)_scannerElement.ActualHeight);
        scan.InsertKeyFrame(0.75f, (float)_scannerElement.ActualWidth);
        scan.InsertKeyFrame(1, (float)_scannerElement.ActualWidth);
        _scanner.StartAnimation("Offset.Y", scan);
        var glow = compositor.CreateScalarKeyFrameAnimation();
        glow.Duration = scan.Duration;
        glow.InsertKeyFrame(0, 0);
        glow.InsertKeyFrame(0.08f, 0.65f);
        glow.InsertKeyFrame(0.65f, 0.65f);
        glow.InsertKeyFrame(0.8f, 0);
        glow.InsertKeyFrame(1, 0);
        _scanner.StartAnimation("Opacity", glow);
        _scanBatch.End();
    }

    /// <summary> Ожидает полного появления строк или прекращения показа заставки </summary>
    internal Task WaitForAppearanceAsync(CancellationToken ct = default) => _appearanceCompleted.Task.WaitAsync(ct);

    /// <summary> Освобождает анимации при уходе со страницы и восстанавливает видимость элементов </summary>
    internal void Stop()
    {
        StopAnimations();
        _appearanceCompleted.TrySetResult();
    }

    // Запускаем сканирование только после окончания и появления, и вращения
    private void OnAppearanceCompleted(object sender, CompositionBatchCompletedEventArgs args)
    {
        StopIntroAnimations();
        StartScan();
        _appearanceCompleted.TrySetResult();
        TgLogUtils.WriteLog("Заставка: мягкое появление и орбита завершены, сканирование запущено");
    }

    // Оставляем знак OTR неподвижным после единственного прохода полосы
    private void OnScanCompleted(object sender, CompositionBatchCompletedEventArgs args)
    {
        StopScan();
        TgLogUtils.WriteLog("Заставка: сканирование завершено");
    }

    // Освобождаем пакет появления и восстанавливаем видимость без оставшихся движущихся слоёв
    private void StopAnimations()
    {
        StopIntroAnimations();
        StopScan();
    }

    // Убираем вводные эффекты, сохраняя полную видимость текста и логотипа
    private void StopIntroAnimations()
    {
        if (_appearanceBatch is not null)
        {
            _appearanceBatch.Completed -= OnAppearanceCompleted;
            _appearanceBatch.Dispose();
            _appearanceBatch = null;
        }
        _orbit.StopAnimation("RotationAngleInDegrees");
        _orbit.RotationAngleInDegrees = 0;
        _orbit.Opacity = 0;
        _content.StopAnimation("Opacity");
        _content.StopAnimation("Scale");
        _continueButton.StopAnimation("Opacity");
        _rows.StopAnimation("Opacity");
        _content.Opacity = 1;
        _content.Scale = System.Numerics.Vector3.One;
        _continueButton.Opacity = 1;
        _rows.Opacity = 1;
    }

    // Освобождаем пакет сканирования и убираем полосу при завершении или уходе со страницы
    private void StopScan()
    {
        if (_scanBatch is not null)
        {
            _scanBatch.Completed -= OnScanCompleted;
            _scanBatch.Dispose();
            _scanBatch = null;
        }
        _scanner.StopAnimation("Offset.Y");
        _scanner.StopAnimation("Opacity");
        _scanner.Opacity = 0;
        _scanner.Offset = System.Numerics.Vector3.Zero;
    }

    // Проявляем элемент без задержки команд и продолжения загрузки
    private static void StartFade(Visual visual, int milliseconds)
    {
        var animation = visual.Compositor.CreateScalarKeyFrameAnimation();
        animation.Duration = TimeSpan.FromMilliseconds(milliseconds);
        animation.InsertKeyFrame(0, 0);
        animation.InsertKeyFrame(1, 1, CreateEaseOut(visual.Compositor));
        visual.StartAnimation("Opacity", animation);
    }

    // Завершаем появление плавным замедлением
    private static CubicBezierEasingFunction CreateEaseOut(Compositor compositor) => compositor.CreateCubicBezierEasingFunction(new(0.16f, 1), new(0.3f, 1));
}
