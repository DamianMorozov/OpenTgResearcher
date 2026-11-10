namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Стрелка с затухающим шлейфом звёздочек, обходящая кнопку по часовой стрелке </summary>
internal sealed class TgButtonOrbitAnimationHelper
{
    private const int _orbitDurationMilliseconds = 6000;
    private readonly FrameworkElement _host;
    private readonly FrameworkElement _icon;
    private readonly FrameworkElement[] _stars;

    /// <summary> Получает область кнопки, стрелку и звёздочки от начала к концу шлейфа </summary>
    internal TgButtonOrbitAnimationHelper(FrameworkElement host, FrameworkElement icon, params FrameworkElement[] stars)
    {
        _host = host;
        _icon = icon;
        _stars = stars;
    }

    /// <summary> Запускает движение с поворотами и затуханием шлейфа с учётом настройки анимаций Windows </summary>
    internal void Start(bool animationsEnabled)
    {
        Stop();
        if (!animationsEnabled) return;
        var path = CreatePath((float)_host.ActualWidth, (float)_host.ActualHeight);
        var perimeter = 0f;
        for (var index = 1; index < path.Count; index++) perimeter += System.Numerics.Vector3.Distance(path[index - 1].Position, path[index].Position);
        _icon.Opacity = 1;
        ElementCompositionPreview.GetElementVisual(_icon).Opacity = 1;
        StartOrbit(_icon, path, perimeter, delayMilliseconds: 0, rotate: true);
        for (var index = 0; index < _stars.Length; index++)
        {
            var delayMilliseconds = (index + 1) * 150;
            // Звёзды следуют по тому же контуру с задержкой, постепенно теряя размер и яркость
            StartOrbit(_stars[index], path, perimeter, delayMilliseconds, rotate: false);
            StartStarFade(_stars[index], 0.85f - index * 0.13f, delayMilliseconds);
        }
    }

    // Синхронизируем повороты стрелки с расстоянием, пройденным по скруглённому контуру
    private static void StartOrbit(FrameworkElement element, List<(System.Numerics.Vector3 Position, float Angle)> path, float perimeter,
        int delayMilliseconds, bool rotate)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        var halfSize = new System.Numerics.Vector3((float)element.ActualWidth / 2, (float)element.ActualHeight / 2, 0);
        visual.CenterPoint = halfSize;
        visual.Offset = path[0].Position - halfSize;
        var movement = visual.Compositor.CreateVector3KeyFrameAnimation();
        movement.Duration = TimeSpan.FromMilliseconds(_orbitDurationMilliseconds);
        movement.DelayTime = TimeSpan.FromMilliseconds(delayMilliseconds);
        movement.IterationBehavior = AnimationIterationBehavior.Forever;
        var rotation = rotate ? visual.Compositor.CreateScalarKeyFrameAnimation() : null;
        if (rotation is not null)
        {
            rotation.Duration = movement.Duration;
            rotation.IterationBehavior = AnimationIterationBehavior.Forever;
        }
        var easing = visual.Compositor.CreateLinearEasingFunction();
        movement.InsertKeyFrame(0, visual.Offset, easing);
        rotation?.InsertKeyFrame(0, path[0].Angle, easing);
        var distance = 0f;
        for (var point = 1; point < path.Count; point++)
        {
            distance += System.Numerics.Vector3.Distance(path[point - 1].Position, path[point].Position);
            var progress = point == path.Count - 1 ? 1 : distance / perimeter;
            movement.InsertKeyFrame(progress, path[point].Position - halfSize, easing);
            rotation?.InsertKeyFrame(progress, path[point].Angle, easing);
        }
        visual.StartAnimation("Offset", movement);
        if (rotation is not null) visual.StartAnimation("RotationAngleInDegrees", rotation);
    }

    // Проявляем и полностью гасим каждую звёздочку, оставляя конец шлейфа едва заметным
    private static void StartStarFade(FrameworkElement star, float peakOpacity, int delayMilliseconds)
    {
        var visual = ElementCompositionPreview.GetElementVisual(star);
        // Нулевая прозрачность XAML скрывает элемент даже при работающей анимации композитора
        star.Opacity = 1;
        visual.Opacity = 0;
        var fade = visual.Compositor.CreateScalarKeyFrameAnimation();
        fade.Duration = TimeSpan.FromMilliseconds(1200);
        fade.DelayTime = TimeSpan.FromMilliseconds(delayMilliseconds);
        fade.IterationBehavior = AnimationIterationBehavior.Forever;
        var easing = visual.Compositor.CreateLinearEasingFunction();
        fade.InsertKeyFrame(0, 0, easing);
        fade.InsertKeyFrame(0.15f, peakOpacity, easing);
        fade.InsertKeyFrame(1, 0, easing);
        visual.StartAnimation("Opacity", fade);
    }

    /// <summary> Останавливает движение, повороты и затухание при уходе со заставки или закрытии приложения </summary>
    internal void Stop()
    {
        StopElement(_icon);
        foreach (var star in _stars) StopElement(star);
    }

    // Возвращаем декоративный элемент в исходное скрытое состояние
    private static void StopElement(FrameworkElement element)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        visual.StopAnimation("Offset");
        visual.StopAnimation("RotationAngleInDegrees");
        visual.StopAnimation("Opacity");
        element.Opacity = 0;
        visual.Opacity = 0;
        visual.Offset = System.Numerics.Vector3.Zero;
        visual.RotationAngleInDegrees = 0;
    }

    // Формируем замкнутый контур со скруглениями и непрерывным поворотом от нуля до полного оборота
    private static List<(System.Numerics.Vector3 Position, float Angle)> CreatePath(float width, float height)
    {
        // Отступ сохраняет зазор между декоративными элементами и кнопкой
        const float inset = 10;
        var radius = Math.Min(12, (height - inset * 2) / 2);
        var right = width - inset;
        var bottom = height - inset;
        var points = new List<(System.Numerics.Vector3 Position, float Angle)> { (new(width / 2, inset, 0), 0) };
        var centers = new System.Numerics.Vector2[]
        {
            new(right - radius, inset + radius), new(right - radius, bottom - radius),
            new(inset + radius, bottom - radius), new(inset + radius, inset + radius)
        };
        for (var corner = 0; corner < centers.Length; corner++)
        {
            for (var part = 0; part <= 4; part++)
            {
                var heading = corner * 90 + part * 22.5f;
                var angle = (heading - 90) * MathF.PI / 180;
                points.Add((new(centers[corner].X + radius * MathF.Cos(angle), centers[corner].Y + radius * MathF.Sin(angle), 0), heading));
            }
        }
        points.Add((points[0].Position, 360));
        return points;
    }
}
