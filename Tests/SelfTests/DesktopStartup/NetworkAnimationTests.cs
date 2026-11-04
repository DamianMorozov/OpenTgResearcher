namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static partial class Program
{
    // Проверяем настоящую разметку и наличие отдельного значка OTR для исполняемого файла и окна
    private static Task ToolbarNetworkLayoutAndIconAsync()
    {
        using var shell = typeof(Program).Assembly.GetManifestResourceStream("ShellPage.xaml")!;
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var band = XDocument.Load(shell).Descendants().SingleOrDefault(element => element.Attribute(x + "Name")?.Value == "ToolbarScanBand");
        Assert(band is not null, "В верхнем логотипе отсутствует полоса сканирования");
        Assert(band!.Parent?.Attribute("Style")?.Value == "{StaticResource TgAppLogoStyle}", "Полоса находится вне общего логотипа");
        Assert(band.Attribute("IsHitTestVisible")?.Value == "False", "Полоса перехватывает ввод");
        var projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../../Presentation/OpenTgResearcherDesktop"));
        var project = XDocument.Load(Path.Combine(projectDirectory, "OpenTgResearcherDesktop.csproj"));
        Assert(project.Descendants("ApplicationIcon").Single().Value == @"Assets\Otr.ico", "EXE использует прежний значок");
        var content = project.Descendants("Content").SingleOrDefault(element => element.Attribute("Include")?.Value == @"Assets\Otr.ico");
        Assert(content?.Attribute("CopyToPublishDirectory")?.Value == "PreserveNewest", "Значок окна отсутствует в публикации");
        var icon = File.ReadAllBytes(Path.Combine(projectDirectory, "Assets/Otr.ico"));
        Assert(BitConverter.ToUInt16(icon, 2) == 1 && BitConverter.ToUInt16(icon, 4) >= 6, "ICO не содержит размеров для панели задач и масштабирования");
        Assert(File.ReadAllText(Path.Combine(projectDirectory, "MainWindow.xaml.cs")).Contains("Assets/Otr.ico"), "Окно использует другой значок");
        Assert(File.ReadAllText(Path.Combine(projectDirectory, "Package.appxmanifest")).Contains(@"Assets\Otr.png"), "Пакет использует прежний значок");
        return Task.CompletedTask;
    }

    // Реальная служба должна удерживать сканирование до окончания последней сетевой операции
    private static async Task NetworkScanTracksOverlappingLoadsAsync()
    {
        using var state = new LoadStateService();
        var scanner = new FrameworkElement { ActualWidth = 76, ActualHeight = 18, Opacity = 0 };
        var helper = new TgNetworkAnimationHelper(scanner, state, action => action(), () => true);
        helper.Start();
        Assert(scanner.Visual.Animations.Count == 0, "Простой запускает сканирование");
        var storage = Guid.NewGuid();
        await state.PrepareStorageTokenAsync(storage);
        Assert(scanner.Visual.Animations.Count == 0, "Чтение БД запускает сетевую анимацию");
        var first = Guid.NewGuid(); var second = Guid.NewGuid(); var download = Guid.NewGuid();
        await state.PrepareOnlineTokenAsync(first);
        Assert(scanner.Opacity == 1 && scanner.Visual.Animations.ContainsKey("Offset.Y"), "Начало подключения не проявило полосу");
        var scan = (ScalarKeyFrameAnimation)scanner.Visual.Animations["Offset.Y"];
        Assert(scan.IterationBehavior == AnimationIterationBehavior.Forever && scan.Duration.TotalSeconds >= 2, "Сканирование не повторяется плавно");
        Assert(scan.Frames.First().Value == -18 && scan.Frames.Last().Value >= 76, "Полоса не проходит через логотип");
        await state.PrepareOnlineTokenAsync(second);
        await state.PrepareDownloadTokenAsync(download);
        state.StopSoftOnlineProcessing(first);
        Assert(ReferenceEquals(scan, scanner.Visual.Animations["Offset.Y"]), "Параллельная загрузка перезапускает эффект");
        state.StopSoftOnlineProcessing(second);
        Assert(scanner.Visual.Animations.Count > 0, "Завершение парсинга остановило продолжающееся скачивание");
        state.StopHardDownloadProcessing();
        Assert(scanner.Visual.Animations.Count == 0 && scanner.Opacity == 0, "Отмена последней загрузки не убрала полосу");
        helper.Stop();
    }

    // Отписка и отключение анимаций Windows должны исключать повторный запуск эффекта
    private static async Task NetworkScanDetachAndReducedMotionAsync()
    {
        using var state = new LoadStateService();
        var scanner = new FrameworkElement { ActualWidth = 76, ActualHeight = 18 };
        var pending = new Queue<Action>();
        var enabled = false;
        var helper = new TgNetworkAnimationHelper(scanner, state, action => pending.Enqueue(action), () => enabled);
        await state.PrepareOnlineTokenAsync(Guid.NewGuid());
        helper.Start();
        while (pending.TryDequeue(out var action)) action();
        Assert(scanner.Visual.Animations.Count == 0 && scanner.Opacity == 0, "Отключение анимаций Windows проигнорировано");
        enabled = true;
        helper.Stop();
        helper.Start();
        while (pending.TryDequeue(out var action)) action();
        Assert(scanner.Visual.Animations.Count > 0, "Загрузка до появления оболочки не обнаружена");
        state.StopHardOnlineProcessing();
        await state.PrepareDownloadTokenAsync(Guid.NewGuid());
        helper.Stop();
        while (pending.TryDequeue(out var action)) action();
        Assert(scanner.Visual.Animations.Count == 0 && scanner.Opacity == 0, "Отложенное уведомление оживило выгруженную оболочку");
    }

}
