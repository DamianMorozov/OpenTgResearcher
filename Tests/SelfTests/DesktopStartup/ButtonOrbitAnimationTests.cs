namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static partial class Program
{
    // Замкнутый контур идёт по часовой стрелке вне кнопки, а частицы сохраняют скорость и задержку шлейфа
    private static Task ButtonOrbitClockwiseWithTrailAsync()
    {
        var host = new FrameworkElement { ActualWidth = 500, ActualHeight = 66 };
        var particles = Enumerable.Range(0, 4).Select(_ => new FrameworkElement { ActualWidth = 14, ActualHeight = 14 }).ToArray();
        new TgButtonOrbitAnimationHelper(host, particles).Start(animationsEnabled: true);
        var head = (Vector3KeyFrameAnimation)particles[0].Visual.Animations["Offset"];
        var points = head.Frames.Select(frame => frame.Value + new System.Numerics.Vector3(7, 7, 0)).ToArray();
        Assert(head.IterationBehavior == AnimationIterationBehavior.Forever && head.Duration.TotalSeconds > 0, "Движение у кнопки не зациклено");
        Assert(points.First() == points.Last() && points[1].X > points[0].X, "Контур не замкнут или начинается против часовой стрелки");
        Assert(points.All(point => point.X <= 8 || point.X >= 492 || point.Y <= 8 || point.Y >= 58), "Точка пересекает внутреннюю область кнопки");
        var signedArea = 0f;
        var speed = System.Numerics.Vector3.Distance(points[0], points[1]) / (head.Frames[1].Progress - head.Frames[0].Progress);
        for (var index = 1; index < points.Length; index++)
        {
            signedArea += points[index - 1].X * points[index].Y - points[index].X * points[index - 1].Y;
            var progress = head.Frames[index].Progress - head.Frames[index - 1].Progress;
            Assert(progress > 0 && MathF.Abs(System.Numerics.Vector3.Distance(points[index - 1], points[index]) / progress - speed) < 0.1f,
                "Частица меняет скорость при переходе между сторонами кнопки");
        }
        Assert(signedArea > 0, "Контур обходит кнопку против часовой стрелки в экранных координатах");
        for (var index = 1; index < particles.Length; index++)
        {
            var tail = (Vector3KeyFrameAnimation)particles[index].Visual.Animations["Offset"];
            Assert(tail.DelayTime.TotalMilliseconds == index * 60 && tail.Duration == head.Duration, "Шлейф не следует за головой");
            Assert(particles[index].Visual.Opacity < particles[index - 1].Visual.Opacity, "Шлейф не затухает к концу");
        }
        return Task.CompletedTask;
    }

    // Отключённые анимации и уход со страницы скрывают все частицы и освобождают каждую запущенную анимацию
    private static Task ButtonOrbitStopAndReducedMotionAsync()
    {
        var particles = Enumerable.Range(0, 4).Select(_ => new FrameworkElement { ActualWidth = 14, ActualHeight = 14 }).ToArray();
        var helper = new TgButtonOrbitAnimationHelper(new() { ActualWidth = 500, ActualHeight = 66 }, particles);
        helper.Start(animationsEnabled: false);
        Assert(particles.All(particle => particle.Visual.Animations.Count == 0 && particle.Visual.Opacity == 0), "Запущены отключённые анимации Windows");
        helper.Start(animationsEnabled: true);
        Assert(particles.All(particle => particle.Visual.Animations.Count == 1 && particle.Visual.Opacity > 0), "Не запущена голова или часть шлейфа");
        helper.Stop();
        Assert(particles.All(particle => particle.Visual.Animations.Count == 0 && particle.Visual.Opacity == 0), "Частицы продолжают движение после остановки");
        return Task.CompletedTask;
    }

    // Нулевая прозрачность XAML из стиля не должна оставлять частицы невидимыми при работающем композиторе
    private static Task ButtonOrbitRevealsXamlParticlesAsync()
    {
        var particles = Enumerable.Range(0, 4).Select(_ => new FrameworkElement { ActualWidth = 14, ActualHeight = 14, Opacity = 0 }).ToArray();
        var helper = new TgButtonOrbitAnimationHelper(new() { ActualWidth = 500, ActualHeight = 66 }, particles);
        helper.Start(animationsEnabled: true);
        Assert(particles.All(particle => particle.Opacity > 0), "Прозрачность XAML скрывает запущенные частицы");
        helper.Stop();
        Assert(particles.All(particle => particle.Opacity == 0), "После ухода со страницы XAML продолжает показывать частицы");
        return Task.CompletedTask;
    }
}
