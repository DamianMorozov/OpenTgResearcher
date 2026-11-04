namespace OpenTgResearcherDesktop.Helpers;

/// <summary> Мягкая точка со шлейфом, обходящая кнопку по часовой стрелке без перехвата ввода </summary>
internal sealed class TgButtonOrbitAnimationHelper
{
    private readonly FrameworkElement _host;
    private readonly FrameworkElement[] _particles;
    private readonly Visual[] _visuals;

    /// <summary> Получает область кнопки и частицы от головы к концу шлейфа </summary>
    internal TgButtonOrbitAnimationHelper(FrameworkElement host, params FrameworkElement[] particles)
    {
        _host = host;
        _particles = particles;
        _visuals = particles.Select(ElementCompositionPreview.GetElementVisual).ToArray();
    }

    /// <summary> Запускает бесконечное движение вокруг уже доступной кнопки с учётом настройки анимаций Windows </summary>
    internal void Start(bool animationsEnabled)
    {
        Stop();
        if (!animationsEnabled) return;
        var path = CreatePath((float)_host.ActualWidth, (float)_host.ActualHeight);
        var perimeter = 0f;
        for (var index = 1; index < path.Count; index++) perimeter += System.Numerics.Vector3.Distance(path[index - 1], path[index]);
        for (var index = 0; index < _visuals.Length; index++)
        {
            var visual = _visuals[index];
            var halfSize = new System.Numerics.Vector3((float)_particles[index].ActualWidth / 2, (float)_particles[index].ActualHeight / 2, 0);
            // Нулевая прозрачность XAML исключает отрисовку даже при активной анимации визуального слоя
            _particles[index].Opacity = 1f / (index + 1);
            visual.Opacity = 1f / (index + 1);
            visual.Offset = path[0] - halfSize;
            var movement = visual.Compositor.CreateVector3KeyFrameAnimation();
            movement.Duration = TimeSpan.FromSeconds(6);
            movement.DelayTime = TimeSpan.FromMilliseconds(index * 60);
            movement.IterationBehavior = AnimationIterationBehavior.Forever;
            var easing = visual.Compositor.CreateLinearEasingFunction();
            movement.InsertKeyFrame(0, visual.Offset, easing);
            var distance = 0f;
            for (var point = 1; point < path.Count; point++)
            {
                distance += System.Numerics.Vector3.Distance(path[point - 1], path[point]);
                movement.InsertKeyFrame(point == path.Count - 1 ? 1 : distance / perimeter, path[point] - halfSize, easing);
            }
            visual.StartAnimation("Offset", movement);
        }
    }

    /// <summary> Останавливает все частицы при уходе со заставки или закрытии приложения </summary>
    internal void Stop()
    {
        for (var index = 0; index < _visuals.Length; index++)
        {
            var visual = _visuals[index];
            visual.StopAnimation("Offset");
            _particles[index].Opacity = 0;
            visual.Opacity = 0;
            visual.Offset = System.Numerics.Vector3.Zero;
        }
    }

    // Формируем замкнутый контур со скруглениями и началом в середине верхней стороны кнопки
    private static List<System.Numerics.Vector3> CreatePath(float width, float height)
    {
        const float inset = 4;
        var radius = Math.Min(12, (height - inset * 2) / 2);
        var right = width - inset;
        var bottom = height - inset;
        var points = new List<System.Numerics.Vector3> { new(width / 2, inset, 0) };
        var centers = new System.Numerics.Vector2[]
        {
            new(right - radius, inset + radius), new(right - radius, bottom - radius),
            new(inset + radius, bottom - radius), new(inset + radius, inset + radius)
        };
        for (var corner = 0; corner < centers.Length; corner++)
        {
            for (var part = 0; part <= 4; part++)
            {
                var angle = (-90 + corner * 90 + part * 22.5f) * MathF.PI / 180;
                points.Add(new(centers[corner].X + radius * MathF.Cos(angle), centers[corner].Y + radius * MathF.Sin(angle), 0));
            }
        }
        points.Add(points[0]);
        return points;
    }
}
