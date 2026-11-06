namespace OpenTgResearcherDesktop.Models;

/// <summary> Состояние обзора и минутной истории оборудования, независимое от элементов окна </summary>
public sealed class TgHardwareDashboardModel : ObservableObject
{
    private TgHardwareMetrics _latest;
    private TgHardwareSample? _current;
    private bool _isPaused;
    private bool _isHistory;
    private TgHardwareSample[] _history = [];

    public bool IsPaused => _isPaused;
    public bool IsHistory => _isHistory;
    public bool HasData => _current is not null;
    public bool HasCpuApp => _current?.CpuApp is not null;
    public bool HasCpuTotal => _current?.CpuTotal is not null;
    public bool HasMemoryApp => _current?.MemoryApp is not null;
    public bool HasMemoryTotal => _current?.MemoryTotal is not null;
    public double CpuAppValue => _current?.CpuApp ?? 0;
    public double CpuTotalValue => _current?.CpuTotal ?? 0;
    public double MemoryAppValue => _current?.MemoryApp ?? 0;
    public double MemoryTotalValue => _current?.MemoryTotal ?? 0;
    public string CpuAppText => FormatPercent(_current?.CpuApp);
    public string CpuTotalText => FormatPercent(_current?.CpuTotal);
    public string MemoryAppPercentText => FormatPercent(_current?.MemoryApp);
    public string MemoryTotalPercentText => FormatPercent(_current?.MemoryTotal);
    public string MemoryAppText => HasData ? FormatSize(_latest.MemoryAppGb) : "—";
    public string MemoryTotalText => HasMemoryTotal ? FormatSize(_latest.MemoryUsedGb) : "—";
    public IReadOnlyList<TgHardwareSample> History => _history;
    public string LastReadingText => _current?.TimestampUtc.ToLocalTime().ToString("HH:mm:ss") ?? "—";

    /// <summary> Принимает новое измерение и удаляет точки за пределами последней минуты </summary>
    public void Apply(TgHardwareMetrics metrics)
    {
        if (IsPaused || metrics.TimestampUtc == default || (_current is not null && metrics.TimestampUtc <= _current.TimestampUtc)) return;
        _latest = metrics;
        var hasMemory = double.IsFinite(metrics.MemoryTotalGb) && metrics.MemoryTotalGb > 0;
        _current = new(metrics.TimestampUtc, ValidPercent(metrics.CpuAppPercent), ValidPercent(metrics.CpuTotalPercent),
            hasMemory ? ValidPercent(metrics.MemoryAppPercent) : null, hasMemory ? ValidPercent(metrics.MemoryTotalPercent) : null);
        _history = [.. _history.Where(sample => sample.TimestampUtc >= metrics.TimestampUtc.AddSeconds(-60)), _current];
        foreach (var property in new[] { nameof(HasData), nameof(HasCpuApp), nameof(HasCpuTotal), nameof(HasMemoryApp), nameof(HasMemoryTotal),
            nameof(CpuAppValue), nameof(CpuTotalValue), nameof(MemoryAppValue), nameof(MemoryTotalValue), nameof(CpuAppText), nameof(CpuTotalText),
            nameof(MemoryAppText), nameof(MemoryTotalText), nameof(MemoryAppPercentText), nameof(MemoryTotalPercentText), nameof(LastReadingText), nameof(History) })
            OnPropertyChanged(property);
    }

    /// <summary> Приостанавливает обновление страницы, сохраняя последние значения и историю </summary>
    public void TogglePause() => SetProperty(ref _isPaused, !_isPaused, nameof(IsPaused));

    /// <summary> Меняет представление без сброса измерений </summary>
    public void SelectView(bool isHistory) => SetProperty(ref _isHistory, isHistory, nameof(IsHistory));

    // Отсутствующие и недопустимые показания сохраняем пропусками, настоящий ноль остаётся нулём
    private static double? ValidPercent(double value) => double.IsFinite(value) && value >= 0 && value <= 100 ? value : null;

    // Округляем только текст, сохраняя точность исходных точек графика
    private static string FormatPercent(double? value) => value is null ? "—" : $"{value.Value:0.#} %";

    // Используем общие единицы размеров приложения
    private static string FormatSize(double gigabytes) => double.IsFinite(gigabytes) && gigabytes >= 0
        ? TgFileUtils.GetFileSizeAsString(gigabytes * 1024 * 1024 * 1024) : "—";
}
