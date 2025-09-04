// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Contracts;

/// <summary> Hardware resource monitoring service </summary>
public interface ITgHardwareResourceMonitoringService : IDisposable
{
    /// <summary> Event for subscribers (UI/logger/telemetry) </summary>
    public event EventHandler<TgHardwareMetrics>? MetricsUpdated;
    /// <summary> Start monitoring hardware metrics </summary>
    public Task StartMonitoringAsync(TimeSpan? interval = null, CancellationToken ct = default);
    /// <summary> Stop monitoring hardware metrics </summary>
    public Task StopMonitoringAsync(bool isClose);
}
