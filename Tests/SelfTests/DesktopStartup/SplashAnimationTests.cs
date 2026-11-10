namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static partial class Program
{
    // Настоящая последовательность этапов оставляет короткую паузу между обновлениями строк
    private static async Task SplashRowsHaveBriefPauseAsync()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763)) return;
        SplashPageState.ViewModel = new();
        var page = new TgSplashScreenPage();
        page.RaiseLoaded();
        page.MainContent.Visual.Compositor.Batches.Single().Complete();
        page.ViewModel.StorageGate.TrySetResult();
        await page.LoadSplashScreenAsync(() => { });
        Assert(page.Bindings.Updates.Count == 8 && page.ViewModel.IsLoadComplete, "Не все семь этапов и завершение загрузки обновили строки");
        for (var index = 1; index < 7; index++)
        {
            var elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(page.Bindings.Updates[index - 1], page.Bindings.Updates[index]);
            Assert(elapsed.TotalMilliseconds >= 140, "Последовательные строки загрузки отображаются без короткой паузы");
        }
        page.RaiseUnloaded();
    }

    // Орбита и сканирующая полоса должны существовать в настоящей разметке и не перехватывать ввод
    private static Task SplashAnimationLayoutAsync()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgSplashScreenPage.xaml")!;
        var document = XDocument.Load(stream);
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var controls = document.Descendants().Where(element => element.Attribute(x + "Name") is not null)
            .ToDictionary(element => (string)element.Attribute(x + "Name")!, element => element);
        foreach (var name in new[] { "SplashOrbit", "SplashScanBand" })
        {
            Assert(controls.TryGetValue(name, out var control), $"На заставке отсутствует {name}");
            Assert(control!.Attribute("IsHitTestVisible")?.Value == "False", $"Декоративный элемент {name} перехватывает ввод");
        }
        Assert(controls["SplashScanBand"].Parent?.Attribute("Style")?.Value == "{StaticResource TgAppLogoStyle}",
            "Сканирующая полоса не находится внутри общего рисунка логотипа");
        Assert(controls.ContainsKey("SplashContinueButton"), "Нет кнопки продолжения для завершения заставки");
        Assert(controls.ContainsKey("SplashLoadingRows"), "Строки загрузки не выделены для медленного появления");
        Assert(controls.TryGetValue("ContinueOrbitArrow", out var icon), "У кнопки продолжения отсутствует стрелка");
        Assert(icon!.Ancestors().Any(element => element.Attribute("IsHitTestVisible")?.Value == "False"), "Значок перехватывает нажатия кнопки");
        return Task.CompletedTask;
    }

    // Знак OTR рисуется разметкой внутри круга и не зависит от внешнего изображения
    private static Task SplashVectorLogoAsync()
    {
        using var layout = typeof(Program).Assembly.GetManifestResourceStream("TgSplashScreenPage.xaml")!;
        var document = XDocument.Load(layout);
        using var shell = typeof(Program).Assembly.GetManifestResourceStream("ShellPage.xaml")!;
        using var styles = typeof(Program).Assembly.GetManifestResourceStream("TgUiControls.xaml")!;
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var style = XDocument.Load(styles).Descendants().SingleOrDefault(element => element.Attribute(x + "Key")?.Value == "TgAppLogoStyle");
        Assert(style is not null, "У заставки и заголовка нет общего рисунка OTR");
        Assert(new[] { document, XDocument.Load(shell) }.All(page => page.Descendants()
            .Any(element => element.Attribute("Style")?.Value == "{StaticResource TgAppLogoStyle}")), "Страницы используют разный рисунок логотипа");
        var text = style!.Descendants().Single(element => element.Name.LocalName == "TextBlock" && element.Attribute("Text")?.Value == "OTR");
        var circle = text.Ancestors().FirstOrDefault(element => element.Name.LocalName == "Border" && element.Attribute("CornerRadius")?.Value == "40");
        Assert(circle?.Attribute("Width")?.Value == "80" && circle.Attribute("Height")?.Value == "80", "Буквы OTR не вписаны в круг");
        return Task.CompletedTask;
    }

    // Мягкое появление всей заставки занимает две секунды и сохраняет центр страницы
    private static Task SplashSoftAppearanceAsync()
    {
        var content = new FrameworkElement { ActualWidth = 800, ActualHeight = 600 };
        content.Visual.Size = new(800, 600);
        var animation = new TgSplashAnimationHelper(content, new(), new(), new(), new());
        animation.Start(isLoading: true, animationsEnabled: true);
        var fade = (ScalarKeyFrameAnimation)content.Visual.Animations["Opacity"];
        var scale = (Vector3KeyFrameAnimation)content.Visual.Animations["Scale"];
        Assert(fade.Frames.First().Value == 0 && fade.Frames.Last().Value == 1, "Заставка не проявляется полностью");
        Assert(scale.Frames.First().Value.X < 1 && scale.Frames.Last().Value == System.Numerics.Vector3.One, "Нет мягкого увеличения логотипа");
        Assert(content.Visual.CenterPoint == new System.Numerics.Vector3(400, 300, 0), "Появление смещает центр страницы");
        Assert(fade.Duration.TotalSeconds == 2 && scale.Duration == fade.Duration, "Мягкое появление не сокращено до двух секунд");
        return Task.CompletedTask;
    }

    // Орбита завершается за две секунды, после чего начинается единственный проход сканирования
    private static Task SplashOrbitAndScanAsync()
    {
        var orbit = new FrameworkElement { ActualWidth = 112, ActualHeight = 112 };
        var scan = new FrameworkElement { ActualWidth = 76, ActualHeight = 18 };
        orbit.Visual.Size = new(112, 112);
        var content = new FrameworkElement();
        new TgSplashAnimationHelper(content, orbit, scan, new(), new()).Start(isLoading: true, animationsEnabled: true);
        var rotation = (ScalarKeyFrameAnimation)orbit.Visual.Animations["RotationAngleInDegrees"];
        Assert(rotation.Duration.TotalSeconds == 2 && rotation.IterationBehavior != AnimationIterationBehavior.Forever,
            "Орбита не завершается одновременно с двухсекундным появлением");
        Assert(scan.Visual.Animations.Count == 0, "Сканирование включилось до завершения появления и орбиты");
        Assert(rotation.Frames.First().Value == 0 && rotation.Frames.Last().Value == 360 && orbit.Visual.Opacity == 1, "Орбита не вращается или скрыта");
        Assert(orbit.Visual.CenterPoint == new System.Numerics.Vector3(56, 56, 0), "Орбита вращается не вокруг логотипа");
        content.Visual.Compositor.Batches.Single().Complete();
        Assert(orbit.Visual.Animations.Count == 0 && orbit.Visual.Opacity == 0, "Орбита не остановлена перед сканированием");
        var movement = (ScalarKeyFrameAnimation)scan.Visual.Animations["Offset.Y"];
        var glow = (ScalarKeyFrameAnimation)scan.Visual.Animations["Opacity"];
        Assert(movement.Duration.TotalMilliseconds == 840 && glow.Duration == movement.Duration, "Сканирование не ускорено в 2,5 раза");
        Assert(movement.Frames.First().Value <= -scan.ActualHeight && movement.Frames.Last().Value >= scan.ActualWidth,
            "Сканирование не проходит изображение целиком");
        Assert(glow.Frames.First().Value == 0 && glow.Frames.Last().Value == 0 && glow.Frames.Any(frame => frame.Value > 0), "Полоса не появляется или не гаснет");
        Assert(new[] { rotation, movement, glow }.All(value => value.IterationBehavior != AnimationIterationBehavior.Forever), "Вводные эффекты повторяются бесконечно");
        return Task.CompletedTask;
    }

    // Нулевой размер визуального слоя до первого кадра не смещает центр орбиты и не останавливает сканирование
    private static Task SplashMeasuredGeometryAsync()
    {
        var content = new FrameworkElement { ActualWidth = 800, ActualHeight = 600 };
        var orbit = new FrameworkElement { ActualWidth = 112, ActualHeight = 112 };
        var scan = new FrameworkElement { ActualWidth = 76, ActualHeight = 18 };
        foreach (var element in new[] { content, orbit, scan }) element.Visual.Size = System.Numerics.Vector2.Zero;
        new TgSplashAnimationHelper(content, orbit, scan, new(), new()).Start(isLoading: true, animationsEnabled: true);
        Assert(content.Visual.CenterPoint == new System.Numerics.Vector3(400, 300, 0), "Появление не учитывает измеренный размер XAML");
        Assert(orbit.Visual.CenterPoint == new System.Numerics.Vector3(56, 56, 0), "Орбита вращается вокруг угла вместо центра логотипа");
        content.Visual.Compositor.Batches.Single().Complete();
        var movement = (ScalarKeyFrameAnimation)scan.Visual.Animations["Offset.Y"];
        Assert(movement.Frames.First().Value == -18 && movement.Frames.Last().Value == 76,
            "Сканирование осталось на месте из-за нулевого размера ещё не отрисованного визуального слоя");
        return Task.CompletedTask;
    }

    // Появление и орбита останавливаются перед сканированием, которое также завершается по своему событию
    private static Task SplashIntroAndScanStopInOrderAsync()
    {
        var controls = Enumerable.Range(0, 5).Select(_ => new FrameworkElement()).ToArray();
        var animation = new TgSplashAnimationHelper(controls[0], controls[1], controls[2], controls[3], controls[4]);
        animation.Start(isLoading: true, animationsEnabled: true);
        controls[0].Visual.Compositor.Batches.Single().Complete();
        Assert(controls[0].Visual.Animations.Count == 0 && controls[1].Visual.Animations.Count == 0 && controls[2].Visual.Animations.Count > 0,
            "Этапы появления и сканирования пересекаются или не запускаются");
        controls[2].Visual.Compositor.Batches.Single().Complete();
        Assert(controls.All(element => element.Visual.Animations.Count == 0), "После полной загрузки приложения продолжаются анимации");
        Assert(controls[0].Visual.Opacity == 1 && controls[3].Visual.Opacity == 1, "После загрузки не видны страница или кнопка продолжения");
        return Task.CompletedTask;
    }

    // Отключение анимаций Windows оставляет доступные статичные элементы без зацикленного движения
    private static Task SplashReducedMotionAsync()
    {
        var controls = Enumerable.Range(0, 5).Select(_ => new FrameworkElement()).ToArray();
        var animation = new TgSplashAnimationHelper(controls[0], controls[1], controls[2], controls[3], controls[4]);
        animation.Start(isLoading: true, animationsEnabled: false);
        animation.Stop();
        Assert(controls.All(element => element.Visual.Animations.Count == 0), "Запущены эффекты при отключённых анимациях Windows");
        Assert(controls[0].Visual.Opacity == 1 && controls[3].Visual.Opacity == 1, "Страница или кнопка скрыты без анимации");
        Assert(animation.WaitForAppearanceAsync().IsCompletedSuccessfully, "Отключённая анимация задерживает готовность приложения");
        return Task.CompletedTask;
    }

    // Повторный показ не накапливает анимации, а уход со страницы освобождает все слои
    private static Task SplashStopAndRestartAsync()
    {
        var controls = Enumerable.Range(0, 5).Select(_ => new FrameworkElement()).ToArray();
        var animation = new TgSplashAnimationHelper(controls[0], controls[1], controls[2], controls[3], controls[4]);
        animation.Start(isLoading: true, animationsEnabled: true);
        animation.Stop();
        Assert(controls.All(element => element.Visual.Animations.Count == 0), "После ухода со страницы остались анимации");
        animation.Start(isLoading: false, animationsEnabled: true);
        Assert(controls[1].Visual.Animations.Count == 0 && controls[2].Visual.Animations.Count == 0, "Повторный показ готовой страницы запускает загрузку");
        animation.Stop();
        Assert(controls[0].Visual.Scale == System.Numerics.Vector3.One && controls[0].Visual.Opacity == 1, "После остановки нарушена видимость страницы");
        return Task.CompletedTask;
    }

    // Настоящая последовательность страницы не завершает эффекты после первого этапа журналирования
    private static async Task SplashLoopsUntilActualCompletionAsync()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763)) return;
        SplashPageState.ViewModel = new();
        var page = new TgSplashScreenPage();
        page.RaiseLoaded();
        var loading = page.LoadSplashScreenAsync(() => { });
        try
        {
            Assert(!loading.IsCompleted && !page.ViewModel.IsLoadComplete, "Проверка не удержала загрузку хранилища");
            page.MainContent.Visual.Compositor.Batches.Single().Complete();
            Assert(!loading.IsCompleted && !page.ViewModel.IsLoadComplete, "Окончание анимации подменило незавершённую загрузку БД");
            Assert(page.SplashOrbit.Visual.Animations.Count == 0 && page.SplashScanBand.Visual.Animations.Count > 0,
                "Сканирование не сменило двухсекундное вращение орбиты");
        }
        finally { page.ViewModel.StorageGate.TrySetResult(); await loading; }
        Assert(page.ViewModel.IsLoadComplete && page.ContinueOrbitArrow.Visual.Animations.Count > 0, "Готовая кнопка не получила стрелку");
        var stars = new[] { page.ContinueOrbitStar1, page.ContinueOrbitStar2, page.ContinueOrbitStar3, page.ContinueOrbitStar4, page.ContinueOrbitStar5 };
        Assert(stars.All(star => star.Visual.Animations.ContainsKey("Offset") && star.Visual.Animations.ContainsKey("Opacity")),
            "Настоящая страница не подключила движение и затухание каждой звёздочки");
        page.SplashScanBand.Visual.Compositor.Batches.Single().Complete();
        Assert(page.SplashScanBand.Visual.Animations.Count == 0, "Проход сканирования не остановился");
        page.RaiseUnloaded();
        Assert(page.ContinueOrbitArrow.Visual.Animations.Count == 0 && stars.All(star => star.Visual.Animations.Count == 0),
            "Стрелка или звёзды не остановились при переходе");
    }

    // Закрытие во время ожидания не оставляет зацикленных эффектов и не проявляет кнопку после закрытия
    private static async Task SplashCloseWhileLoadingAsync()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763)) return;
        SplashPageState.ViewModel = new();
        var page = new TgSplashScreenPage();
        page.RaiseLoaded();
        var loading = page.LoadSplashScreenAsync(() => { });
        App.IsClosing = true;
        page.RaiseUnloaded();
        page.ViewModel.StorageGate.TrySetResult();
        await loading;
        Assert(!page.ViewModel.IsLoadComplete && new[] { page.MainContent, page.SplashOrbit, page.SplashScanBand, page.SplashContinueButton }
            .All(element => element.Visual.Animations.Count == 0), "Закрытая заставка сохранила или повторно запустила анимации");
        Assert(page.MainContent.Visual.Compositor.Batches.Single().IsDisposed, "Закрытая заставка сохранила пакет анимации строк");
    }

    // Две секунды относятся к настоящей анимации строк, а окончание кадров освобождает ожидающую загрузку
    private static async Task SplashRowsAppearForTwoSecondsAsync()
    {
        var content = new FrameworkElement();
        var rows = new FrameworkElement();
        var animation = new TgSplashAnimationHelper(content, new(), new(), new(), rows);
        animation.Start(isLoading: true, animationsEnabled: true);
        var fade = (ScalarKeyFrameAnimation)rows.Visual.Animations["Opacity"];
        Assert(fade.Duration.TotalSeconds == 2 && fade.Frames.First().Value == 0 && fade.Frames.Last().Value == 1,
            "Строки загрузки не появляются в течение двух секунд");
        var completion = animation.WaitForAppearanceAsync();
        var batch = content.Visual.Compositor.Batches.Single();
        Assert(batch.IsEnded && !completion.IsCompleted, "Подготовка заставки завершилась до появления строк");
        batch.Complete();
        await completion;
        animation.Stop();
        Assert(batch.IsDisposed && rows.Visual.Animations.Count == 0 && rows.Visual.Opacity == 1, "После готовности строки продолжают анимироваться");
    }

    // Быстрая инициализация ожидает появления строк, а закрытие освобождает это ожидание без таймера
    private static async Task SplashWaitsForAppearanceAsync()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763)) return;
        SplashPageState.ViewModel = new();
        var page = new TgSplashScreenPage();
        page.RaiseLoaded();
        var loading = page.LoadSplashScreenAsync(() => { });
        page.ViewModel.StorageGate.TrySetResult();
        await page.ViewModel.HardwareStarted.Task;
        Assert(!loading.IsCompleted && !page.ViewModel.IsLoadComplete, "Кнопка продолжения появилась до окончания анимации строк");
        page.MainContent.Visual.Compositor.Batches.Single().Complete();
        await loading;
        Assert(page.ViewModel.IsLoadComplete && page.SplashLoadingRows.Visual.Animations.Count == 0, "Заставка не стала неподвижной после готовности");
        page.RaiseUnloaded();
    }

    // Закрытие во время появления строк освобождает именно ожидание анимации, а не только ожидание БД
    private static async Task SplashCloseDuringAppearanceAsync()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763)) return;
        SplashPageState.ViewModel = new();
        var page = new TgSplashScreenPage();
        page.RaiseLoaded();
        var loading = page.LoadSplashScreenAsync(() => { });
        page.ViewModel.StorageGate.TrySetResult();
        await page.ViewModel.HardwareStarted.Task;
        Assert(!loading.IsCompleted, "Загрузка не ожидает появления строк");
        App.IsClosing = true;
        page.RaiseUnloaded();
        await loading.WaitAsync(TimeSpan.FromSeconds(2));
        Assert(!page.ViewModel.IsLoadComplete && page.MainContent.Visual.Compositor.Batches.Single().IsDisposed,
            "Закрытие во время появления строк не освободило ожидание или проявило кнопку продолжения");
    }
}
