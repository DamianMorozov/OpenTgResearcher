namespace OpenTgResearcherDesktop.Styles;

/// <summary> Минутный график нагрузки с пропусками недоступных измерений и временной шкалой </summary>
public sealed partial class TgHardwareHistoryChart : UserControl
{
    public static readonly DependencyProperty SamplesProperty = DependencyProperty.Register(nameof(Samples), typeof(IReadOnlyList<TgHardwareSample>),
        typeof(TgHardwareHistoryChart), new PropertyMetadata(null, OnChartChanged));
    public static readonly DependencyProperty IsMemoryProperty = DependencyProperty.Register(nameof(IsMemory), typeof(bool),
        typeof(TgHardwareHistoryChart), new PropertyMetadata(false, OnChartChanged));

    public IReadOnlyList<TgHardwareSample>? Samples
    {
        get => (IReadOnlyList<TgHardwareSample>?)GetValue(SamplesProperty);
        set => SetValue(SamplesProperty, value);
    }
    public bool IsMemory { get => (bool)GetValue(IsMemoryProperty); set => SetValue(IsMemoryProperty, value); }

    /// <summary> Создаёт график, который перерисовывается при измерении окна и смене темы </summary>
    public TgHardwareHistoryChart()
    {
        InitializeComponent();
        IsTabStop = false;
        SizeChanged += OnChartSizeChanged;
        ActualThemeChanged += OnChartThemeChanged;
        Loaded += OnChartLoaded;
        Unloaded += OnChartUnloaded;
    }

    // Предоставляем графику отдельный узел доступности вместо невидимого контейнера
    protected override Microsoft.UI.Xaml.Automation.Peers.AutomationPeer OnCreateAutomationPeer() =>
        new Microsoft.UI.Xaml.Automation.Peers.FrameworkElementAutomationPeer(this);

    // Перестраиваем график после обновления источника
    private static void OnChartChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) => ((TgHardwareHistoryChart)sender).Redraw();

    // Координаты всегда рассчитываем для фактической ширины
    private void OnChartSizeChanged(object sender, SizeChangedEventArgs e) => Redraw();

    // Используем кисти действующей темы приложения
    private void OnChartThemeChanged(FrameworkElement sender, object args) => DispatcherQueue.TryEnqueue(Redraw);

    // Подписи осей следуют языку приложения
    private void OnChartLoaded(object sender, RoutedEventArgs e)
    {
        Localizer.Get().LanguageChanged += OnLanguageChanged;
        Redraw();
    }

    // Убираем подписку невидимого графика
    private void OnChartUnloaded(object sender, RoutedEventArgs e) => Localizer.Get().LanguageChanged -= OnLanguageChanged;

    // Меняем подписи даже при остановленных показаниях
    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e) => DispatcherQueue.TryEnqueue(Redraw);

    // Отрисовываем шкалу и доступные отрезки без изменения исходных данных
    private void Redraw()
    {
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(this, "HardwareHistory.Text".GetLocalizedResource());
        ChartCanvas.Children.Clear();
        if (ActualWidth < 100 || ActualHeight < 60) return;
        var foreground = ForegroundProbe.Background;
        var grid = GridProbe.Background;
        var accent = AccentProbe.Background;
        var width = ActualWidth - 52;
        var height = ActualHeight - 38;
        foreach (var value in new[] { 100, 50, 0 })
        {
            var y = 8 + (100 - value) * height / 100;
            ChartCanvas.Children.Add(new Microsoft.UI.Xaml.Shapes.Line { X1 = 40, X2 = 40 + width, Y1 = y, Y2 = y, Stroke = grid, StrokeThickness = 1 });
            AddLabel(value == 100 ? "100 %" : $"{value}", 0, y - 8, foreground);
        }
        AddLabel("HardwareMinuteAgo".GetLocalizedResource(), 40, ActualHeight - 20, foreground);
        var now = AddLabel("HardwareNow".GetLocalizedResource(), 0, ActualHeight - 20, foreground);
        now.Measure(new Windows.Foundation.Size(double.PositiveInfinity, 24));
        Canvas.SetLeft(now, ActualWidth - now.DesiredSize.Width);
        if (Samples is not { Count: > 0 })
        {
            AddLabel("HardwareWaiting".GetLocalizedResource(), 60, height / 2, foreground);
            return;
        }
        DrawSeries(Samples, isApplication: false, foreground, width, height);
        DrawSeries(Samples, isApplication: true, accent, width, height);
        var hasAny = Samples.Any(sample => IsMemory ? sample.MemoryApp is not null || sample.MemoryTotal is not null : sample.CpuApp is not null || sample.CpuTotal is not null);
        if (!hasAny) AddLabel("HardwareNoSensors".GetLocalizedResource(), 60, height / 2, foreground);
    }

    // Разрываем линию при отсутствующем показании или перерыве получения данных
    private void DrawSeries(IReadOnlyList<TgHardwareSample> samples, bool isApplication, Brush brush, double width, double height)
    {
        var end = samples[^1].TimestampUtc;
        var points = new PointCollection();
        DateTime? previous = null;
        foreach (var sample in samples)
        {
            var value = IsMemory ? isApplication ? sample.MemoryApp : sample.MemoryTotal : isApplication ? sample.CpuApp : sample.CpuTotal;
            if (value is null || previous is not null && (sample.TimestampUtc - previous.Value).TotalSeconds > 3)
            {
                AddSegment(points, brush, isApplication);
                points = new();
            }
            if (value is not null)
                points.Add(new Windows.Foundation.Point(40 + Math.Clamp(60 - (end - sample.TimestampUtc).TotalSeconds, 0, 60) * width / 60,
                    8 + (100 - value.Value) * height / 100));
            previous = sample.TimestampUtc;
        }
        AddSegment(points, brush, isApplication);
    }

    // Одиночное измерение показываем точкой, системную серию отличаем пунктиром
    private void AddSegment(PointCollection points, Brush brush, bool isApplication)
    {
        if (points.Count == 0) return;
        if (points.Count == 1)
        {
            var dot = new Microsoft.UI.Xaml.Shapes.Ellipse { Width = 4, Height = 4, Fill = brush };
            Canvas.SetLeft(dot, points[0].X - 2);
            Canvas.SetTop(dot, points[0].Y - 2);
            ChartCanvas.Children.Add(dot);
            return;
        }
        var line = new Microsoft.UI.Xaml.Shapes.Polyline { Points = points, Stroke = brush, StrokeThickness = 2 };
        if (!isApplication) line.StrokeDashArray = new DoubleCollection { 4, 3 };
        ChartCanvas.Children.Add(line);
    }

    // Добавляем локализованную подпись, не уменьшая её при сжатии графика
    private TextBlock AddLabel(string text, double x, double y, Brush brush)
    {
        var label = new TextBlock { Text = text, FontSize = 12, Foreground = brush };
        Canvas.SetLeft(label, x);
        Canvas.SetTop(label, y);
        ChartCanvas.Children.Add(label);
        return label;
    }
}
