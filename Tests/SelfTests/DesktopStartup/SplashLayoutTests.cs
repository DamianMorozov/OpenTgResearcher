namespace OpenTgResearcherDesktop.StartupSelfTests;

internal static partial class Program
{
    // Декоративный шлейф состоит из отдельных звёздочек, уменьшающихся к концу
    private static Task SplashStarTrailLayoutAsync()
    {
        var page = ReadSplashLayout();
        var style = page.Descendants().Single(x => x.Attribute(XName.Get("Key", "http://schemas.microsoft.com/winfx/2006/xaml"))?.Value == "SplashTrailStarStyle");
        Assert(style.Elements().Any(x => x.Attribute("Property")?.Value == "Data" && !string.IsNullOrWhiteSpace(x.Attribute("Value")?.Value)),
            "Общий рисунок звёздочки отсутствует");
        var previousSize = 14f;
        for (var index = 1; index <= 5; index++)
        {
            var star = NamedSplashElement(page, $"ContinueOrbitStar{index}");
            var size = float.Parse(star.Attribute("Width")!.Value, CultureInfo.InvariantCulture);
            Assert(size > 0 && size < previousSize && star.Name.LocalName == "Path", "Шлейф не уменьшается к концу или не использует векторные звёзды");
            Assert(star.Ancestors().Any(x => x.Attribute("IsHitTestVisible")?.Value == "False"), "Звёздочки перехватывают ввод");
            Assert(star.Attribute("Style")?.Value == "{StaticResource SplashTrailStarStyle}", "Звёздочка не использует общий рисунок");
            previousSize = size;
        }
        return Task.CompletedTask;
    }

    // Появление кнопки не должно менять измеряемую высоту её строки и сдвигать заставку
    private static Task SplashContinueReservesSpaceAsync()
    {
        var page = ReadSplashLayout();
        var host = NamedSplashElement(page, "SplashContinueHost");
        Assert(host.Attribute("Visibility") is null && float.Parse(host.Attribute("Height")?.Value ?? "0", CultureInfo.InvariantCulture) >= 100,
            "Область кнопки не резервирует высоту до завершения загрузки");
        Assert(!host.Ancestors().Any(x => x.Attribute("Visibility")?.Value.Contains("IsLoadComplete") == true),
            "Родитель кнопки схлопывается до завершения загрузки");
        var button = NamedSplashElement(page, "SplashContinueButton");
        Assert(button.Attribute("Visibility")?.Value.Contains("IsLoadComplete") == true, "Кнопка доступна до готовности приложения");
        return Task.CompletedTask;
    }

    // Карточки этапов сохраняют колонки значка, текста и состояния независимо от готовности этапа
    private static Task SplashModernRowsAsync()
    {
        var rows = NamedSplashElement(ReadSplashLayout(), "SplashLoadingRows");
        var cards = rows.Elements().Where(x => x.Name.LocalName == "Border").ToArray();
        Assert(cards.Length == 7, "Этапы загрузки не оформлены семью отдельными карточками");
        foreach (var card in cards)
        {
            var grid = card.Elements().Single(x => x.Name.LocalName == "Grid");
            Assert(grid.Descendants().Count(x => x.Name.LocalName == "ColumnDefinition") == 3, "В карточке нет фиксированных колонок состояния и значка");
            Assert(grid.Elements().Any(x => x.Attribute("Grid.Column")?.Value == "2" && x.Attribute("Width")?.Value == "24"),
                "Статус этапа меняет ширину карточки при появлении галочки");
            Assert(grid.Elements().Where(x => x.Attribute("Grid.Column") is null).All(x => x.Attribute("Visibility") is null),
                "Значок этапа скрыт до завершения мониторинга оборудования");
        }
        return Task.CompletedTask;
    }

    // По орбите движется небольшая векторная стрелка, декоративный слой пропускает ввод
    private static Task SplashArrowOrbitLayoutAsync()
    {
        var page = ReadSplashLayout();
        var arrow = NamedSplashElement(page, "ContinueOrbitArrow");
        Assert(arrow.Name.LocalName == "Viewbox" && arrow.Descendants().Any(x => x.Name.LocalName == "Path" && x.Attribute("Data") is not null),
            "На орбите отсутствует векторная стрелка");
        Assert(float.Parse(arrow.Attribute("Width")!.Value, CultureInfo.InvariantCulture) <= 14, "Стрелка слишком крупная");
        Assert(arrow.Descendants().Any(x => x.Name.LocalName == "Path" && x.Attribute("Stroke")?.Value.Contains("SystemControlForegroundAccentBrush") == true),
            "Стрелка не использует контрастный акцент приложения");
        Assert(arrow.Ancestors().Any(x => x.Attribute("IsHitTestVisible")?.Value == "False"), "Декоративный значок перехватывает нажатия");
        Assert(!page.Descendants().Attributes().Any(x => x.Value.StartsWith("ContinueOrbitTail", StringComparison.Ordinal)),
            "На заставке остался прежний огненный шлейф");
        return Task.CompletedTask;
    }

    // Габариты значка и отступ кнопки из настоящей разметки обеспечивают зазор на всей траектории
    private static Task ArrowOrbitClearsButtonAsync()
    {
        var page = ReadSplashLayout();
        var host = NamedSplashElement(page, "SplashContinueHost");
        var button = NamedSplashElement(page, "SplashContinueButton");
        var margin = float.Parse(button.Attribute("Margin")!.Value, CultureInfo.InvariantCulture);
        var height = float.Parse(host.Attribute("Height")!.Value, CultureInfo.InvariantCulture);
        var iconLayout = NamedSplashElement(page, "ContinueOrbitArrow");
        var iconWidth = float.Parse(iconLayout.Attribute("Width")!.Value, CultureInfo.InvariantCulture);
        var iconHeight = float.Parse(iconLayout.Attribute("Height")!.Value, CultureInfo.InvariantCulture);
        var maximumWidth = float.Parse(host.Attribute("MaxWidth")!.Value, CultureInfo.InvariantCulture);
        foreach (var width in new[] { maximumWidth, 300f })
        {
            var icon = new FrameworkElement { ActualWidth = iconWidth, ActualHeight = iconHeight };
            new TgButtonOrbitAnimationHelper(new() { ActualWidth = width, ActualHeight = height }, icon).Start(animationsEnabled: true);
            var movement = (Vector3KeyFrameAnimation)icon.Visual.Animations["Offset"];
            var rotation = (ScalarKeyFrameAnimation)icon.Visual.Animations["RotationAngleInDegrees"];
            Assert(movement.Frames.Count > 4, "Для значка не создана замкнутая траектория");
            for (var segment = 1; segment < movement.Frames.Count; segment++)
            {
                for (var sample = 0; sample <= 10; sample++)
                {
                    var point = System.Numerics.Vector3.Lerp(movement.Frames[segment - 1].Value, movement.Frames[segment].Value, sample / 10f);
                    var angle = (rotation.Frames[segment - 1].Value + (rotation.Frames[segment].Value - rotation.Frames[segment - 1].Value) * sample / 10f)
                        * MathF.PI / 180;
                    var halfWidth = (MathF.Abs(MathF.Cos(angle)) * iconWidth + MathF.Abs(MathF.Sin(angle)) * iconHeight) / 2;
                    var halfHeight = (MathF.Abs(MathF.Sin(angle)) * iconWidth + MathF.Abs(MathF.Cos(angle)) * iconHeight) / 2;
                    var centerX = point.X + iconWidth / 2;
                    var centerY = point.Y + iconHeight / 2;
                    Assert(centerX - halfWidth >= 0 && centerY - halfHeight >= 0 && centerX + halfWidth <= width && centerY + halfHeight <= height,
                        "Значок выходит за зарезервированную область");
                    var gap = Math.Max(Math.Max(margin - centerX - halfWidth, centerX - halfWidth - width + margin),
                        Math.Max(margin - centerY - halfHeight, centerY - halfHeight - height + margin));
                    Assert(gap >= 5.9f, "Между стрелкой и кнопкой отсутствует зазор в шесть пикселей");
                }
            }
        }
        return Task.CompletedTask;
    }

    // Читаем настоящую разметку страницы вместо её копии в тесте
    private static XDocument ReadSplashLayout()
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("TgSplashScreenPage.xaml")!;
        return XDocument.Load(stream);
    }

    // Находим элемент заставки по имени из пространства имён XAML
    private static XElement NamedSplashElement(XDocument page, string name) => page.Descendants()
        .SingleOrDefault(x => x.Attribute(XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml"))?.Value == name)
        ?? throw new InvalidOperationException($"На заставке отсутствует {name}");
}
