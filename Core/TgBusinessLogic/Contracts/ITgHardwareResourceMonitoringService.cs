namespace TgBusinessLogic.Contracts;

/// <summary> Hardware resource monitoring service </summary>
public interface ITgHardwareResourceMonitoringService : IDisposable
{
    /// <summary> Event for subscribers (UI/logger/telemetry) </summary>
    public event EventHandler<TgHardwareMetrics>? MetricsUpdated;
    /// <summary> Start monitoring hardware metrics </summary>
    public void StartMonitoring(TimeSpan? interval = null, CancellationToken ct = default);
    /// <summary> Stop monitoring hardware metrics </summary>
    public Task StopMonitoringAsync(bool isClose);
}
