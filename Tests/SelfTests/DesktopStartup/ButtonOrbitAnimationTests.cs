namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static partial class Program
{
    // Звёзды отстают от стрелки по тому же контуру, уменьшают яркость к концу шлейфа и полностью гаснут
    private static Task ButtonStarTrailFadesAsync()
    {
        var arrow = new FrameworkElement { ActualWidth = 14, ActualHeight = 14 };
        var stars = new[] { 10, 8, 6, 5, 4 }.Select(size => new FrameworkElement { ActualWidth = size, ActualHeight = size, Opacity = 0 }).ToArray();
        var helper = new TgButtonOrbitAnimationHelper(new() { ActualWidth = 500, ActualHeight = 112 }, arrow, stars);
        helper.Start(animationsEnabled: true);
        var movement = (Vector3KeyFrameAnimation)arrow.Visual.Animations["Offset"];
        var previousPeak = 1f;
        for (var index = 0; index < stars.Length; index++)
        {
            var star = stars[index];
            var trail = (Vector3KeyFrameAnimation)star.Visual.Animations["Offset"];
            var fade = (ScalarKeyFrameAnimation)star.Visual.Animations["Opacity"];
            var peak = fade.Frames.Max(frame => frame.Value);
            Assert(star.Opacity == 1 && peak > 0 && peak < previousPeak, "Звёзды скрыты разметкой или яркость не уменьшается к концу шлейфа");
            Assert(trail.DelayTime.TotalMilliseconds == (index + 1) * 150 && fade.DelayTime == trail.DelayTime && trail.Duration == movement.Duration,
                "Шлейф не следует за стрелкой с нарастающей задержкой");
            Assert(fade.IterationBehavior == AnimationIterationBehavior.Forever && fade.Duration.TotalMilliseconds == 1200
                && fade.Frames.First().Value == 0 && fade.Frames.Last().Value == 0, "Звёздочка не проявляется и не исчезает полностью");
            var halfSize = new System.Numerics.Vector3((float)star.ActualWidth / 2, (float)star.ActualHeight / 2, 0);
            Assert(trail.Frames.Count == movement.Frames.Count, "Шлейф движется по другому контуру");
            for (var frame = 0; frame < movement.Frames.Count; frame++)
            {
                var head = movement.Frames[frame].Value + new System.Numerics.Vector3(7, 7, 0);
                Assert(trail.Frames[frame].Progress == movement.Frames[frame].Progress && trail.Frames[frame].Value + halfSize == head,
                    "Звёзды срезают углы или теряют равномерное движение за стрелкой");
            }
            previousPeak = peak;
        }
        Assert(previousPeak >= 0.3f, "Конец удлинённого шлейфа слишком тусклый");
        helper.Stop();
        Assert(stars.Append(arrow).All(element => element.Opacity == 0 && element.Visual.Opacity == 0 && element.Visual.Animations.Count == 0),
            "После остановки сохранились звёзды или анимации");
        helper.Start(animationsEnabled: false);
        Assert(stars.Append(arrow).All(element => element.Visual.Animations.Count == 0), "Шлейф игнорирует отключение анимаций Windows");
        return Task.CompletedTask;
    }

    // Стрелка поворачивается по касательной к контуру, а в конце обхода не вращается обратно
    private static Task ButtonOrbitTurnsAlongPathAsync()
    {
        var icon = new FrameworkElement { ActualWidth = 14, ActualHeight = 14 };
        new TgButtonOrbitAnimationHelper(new() { ActualWidth = 500, ActualHeight = 112 }, icon).Start(animationsEnabled: true);
        Assert(icon.Visual.Animations.TryGetValue("RotationAngleInDegrees", out var turn), "Стрелка не меняет направление во время движения");
        var rotation = (ScalarKeyFrameAnimation)turn!;
        var movement = (Vector3KeyFrameAnimation)icon.Visual.Animations["Offset"];
        Assert(rotation.Duration == movement.Duration && rotation.IterationBehavior == AnimationIterationBehavior.Forever,
            "Повороты не синхронизированы с обходом кнопки");
        Assert(icon.Visual.CenterPoint == new System.Numerics.Vector3(7, 7, 0), "Стрелка вращается не вокруг собственного центра");
        Assert(rotation.Frames.Count == movement.Frames.Count && rotation.Frames.First().Value == 0 && rotation.Frames.Last().Value == 360,
            "Поворот стрелки не описывает полный обход");
        for (var index = 1; index < movement.Frames.Count; index++)
        {
            var direction = movement.Frames[index].Value - movement.Frames[index - 1].Value;
            var expected = MathF.Atan2(direction.Y, direction.X) * 180 / MathF.PI;
            if (expected < 0) expected += 360;
            var actual = (rotation.Frames[index - 1].Value + rotation.Frames[index].Value) / 2;
            Assert(MathF.Abs(MathF.IEEERemainder(expected - actual, 360)) < 0.1f,
                $"Направление стрелки расходится с движением на участке {index}: {actual}° вместо {expected}°");
            Assert(rotation.Frames[index].Progress == movement.Frames[index].Progress && rotation.Frames[index].Value >= rotation.Frames[index - 1].Value,
                "Стрелка поворачивается обратно или раньше соответствующего участка траектории");
        }
        return Task.CompletedTask;
    }

    // Замкнутый контур значка идёт по часовой стрелке с равномерной скоростью
    private static Task ButtonOrbitClockwiseAsync()
    {
        var icon = new FrameworkElement { ActualWidth = 20, ActualHeight = 20 };
        var host = new FrameworkElement { ActualWidth = 500, ActualHeight = 112 };
        new TgButtonOrbitAnimationHelper(host, icon).Start(animationsEnabled: true);
        var movement = (Vector3KeyFrameAnimation)icon.Visual.Animations["Offset"];
        var points = movement.Frames.Select(frame => frame.Value + new System.Numerics.Vector3(10, 10, 0)).ToArray();
        Assert(movement.IterationBehavior == AnimationIterationBehavior.Forever && movement.Duration.TotalSeconds == 6,
            "Движение значка не зациклено с шестисекундным обходом");
        Assert(points.First() == points.Last() && points[1].X > points[0].X, "Контур не замкнут или начинается против часовой стрелки");
        var signedArea = 0f;
        var speed = System.Numerics.Vector3.Distance(points[0], points[1]) / (movement.Frames[1].Progress - movement.Frames[0].Progress);
        for (var index = 1; index < points.Length; index++)
        {
            signedArea += points[index - 1].X * points[index].Y - points[index].X * points[index - 1].Y;
            var progress = movement.Frames[index].Progress - movement.Frames[index - 1].Progress;
            Assert(progress > 0 && MathF.Abs(System.Numerics.Vector3.Distance(points[index - 1], points[index]) / progress - speed) < 0.1f,
                "Значок меняет скорость при переходе между сторонами кнопки");
        }
        Assert(signedArea > 0, "Контур обходит кнопку против часовой стрелки в экранных координатах");
        return Task.CompletedTask;
    }

    // Отключённые анимации и уход со страницы скрывают значок и освобождают движение
    private static Task ButtonOrbitStopAndReducedMotionAsync()
    {
        var icon = new FrameworkElement { ActualWidth = 20, ActualHeight = 20 };
        var helper = new TgButtonOrbitAnimationHelper(new() { ActualWidth = 500, ActualHeight = 112 }, icon);
        helper.Start(animationsEnabled: false);
        Assert(icon.Visual.Animations.Count == 0 && icon.Visual.Opacity == 0, "Запущены отключённые анимации Windows");
        helper.Start(animationsEnabled: true);
        Assert(icon.Visual.Animations.Count == 2 && icon.Visual.Opacity == 1, "Не запущены движение и повороты стрелки");
        helper.Stop();
        Assert(icon.Visual.Animations.Count == 0 && icon.Visual.Opacity == 0, "Значок продолжает движение после остановки");
        return Task.CompletedTask;
    }

    // Нулевая прозрачность XAML не должна оставлять значок невидимым при работающем композиторе
    private static Task ButtonOrbitRevealsXamlIconAsync()
    {
        var icon = new FrameworkElement { ActualWidth = 20, ActualHeight = 20, Opacity = 0 };
        var helper = new TgButtonOrbitAnimationHelper(new() { ActualWidth = 500, ActualHeight = 112 }, icon);
        helper.Start(animationsEnabled: true);
        Assert(icon.Opacity == 1, "Прозрачность XAML скрывает движущийся значок");
        helper.Stop();
        Assert(icon.Opacity == 0, "После ухода со страницы XAML продолжает показывать значок");
        return Task.CompletedTask;
    }
}
